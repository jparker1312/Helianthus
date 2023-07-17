using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

using Grasshopper;
using Grasshopper.Documentation;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types.Transforms;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Rhino.Geometry.Collections;
using static Rhino.Render.TextureGraphInfo;
using Rhino.DocObjects;

namespace Helianthus
{
  public class MyComponent : GH_Component
  {
    int[] TREGENZA_PATCHES_PER_ROW = { 30, 30, 24, 24, 18, 12, 6, 1 };
    double[] TREGENZA_COEFFICIENTS = {
        0.0435449227, 0.0416418006, 0.0473984151, 0.0406730411,
        0.0428934136, 0.0445221864, 0.0455168385, 0.0344199465 };

    /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
    public MyComponent()
      : base("Site_Suitability",
             "Site Suitability",
             "Visualize photosynthetic sunlight levels on surfaces to " +
             "determine best locations for crop placement",
             "Helianthus",
             "02 | Analyze Data")
    {
    }

/// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
        pManager.AddGenericParameter("WEA_File", "WEA File",
        "File location for .wea file", GH_ParamAccess.item);
        pManager.AddGeometryParameter("SurfaceGeometry", "Surface Geometry",
            "Rhino Surfaces or Rhino Meshes", GH_ParamAccess.list);
        pManager.AddGeometryParameter("ContextGeometry", "Context Geometry",
            "Rhino Surfaces or Rhino Meshes", GH_ParamAccess.list);
        pManager.AddNumberParameter("GridSize", "Grid Size",
            "Grid Size for output geometry", GH_ParamAccess.item);
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Out", "Out", "Input Parameters",
            GH_ParamAccess.list);
        pManager.AddMeshParameter("Mesh", "Mesh", "Mesh viz",
            GH_ParamAccess.list);
        //pManager.AddLineParameter("Rays", "Rays", "Ray viz",
        //    GH_ParamAccess.list);
        //pManager.AddPointParameter("Points", "Points", "Points viz",
        //    GH_ParamAccess.list);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
    /// to store data in output parameters.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        string weaFileLocation = "";
        List<Brep> geometryInput = new List<Brep>();
        List<Brep> contextGeometryInput = new List<Brep>();
        double gridSize = 1.0;

        if (!DA.GetData(0, ref weaFileLocation)) { return; }
        //if (!DA.GetData(1, ref geometryInput)) { return; }
        if (!DA.GetDataList(1, geometryInput)) { return; }
        //optional???
        if (!DA.GetDataList(2, contextGeometryInput)) { }
        if (!DA.GetData(3, ref gridSize)) { return; }

        //TODO not going to currently filter on the hoys
        //TODO set WEA data info. maybe not needed
        //WeaDataObject weaDataObject = new WeaDataObject(locationDataObject, radiationDataList);
        //string weaFileName = weaDataObject.writeToWeaFile();

        string gendaymtx_arg_direct = "-m 1 -d -A -h " + weaFileLocation;
        string gendaymtx_arg_diffuse = "-m 1 -s -A -h " + weaFileLocation;
        string directRadiationRGB = callGenDayMtx(gendaymtx_arg_direct);
        string diffuseRadiationRGB = callGenDayMtx(gendaymtx_arg_diffuse);

        List<double> directRadiationList = new List<double>();
        List<double> diffuseRadiationList = new List<double>();

        directRadiationList = convertRgbRadiationList(directRadiationRGB);
        diffuseRadiationList = convertRgbRadiationList(diffuseRadiationRGB);

        //add radiation lists together
        List<double> totalRadiationList = new List<double>();
        for(int i = 0; i < directRadiationList.Count; i++)
        {
            totalRadiationList.Add(directRadiationList[i] +
                diffuseRadiationList[i]);
        }

        //caculate total radiation
        double sum_totalRadiation = 0.0; 
        totalRadiationList.ForEach(x => sum_totalRadiation += x);

        //.2 is the default value for ground radiaiton
        //get a ground radiation value that is constant and the same size list
        //of the total radiation list
        double groundRadiationConstant =
            (sum_totalRadiation / totalRadiationList.Count) * .2;

        //add ground radiation constant to total radiation list equal to the
        //total size of the list. Should have 290 values
        List<double> groundRadiationList = new List<double>();
        foreach(double value in totalRadiationList)
        {
            groundRadiationList.Add(groundRadiationConstant);
        }
        totalRadiationList.AddRange(groundRadiationList);

        //create gridded mesh from geometry
        //how: using geometry and grid size: use default size for now (just say 1 for now. trial with 2). then output study mesh
        //ladybug is creating Ladybug Mesh3D and we want to stay with rhino since we dont want our own geometry lib
        //right now we assume 1 geometry and not a list as input
        //not assuming any offset curretly
        MeshingParameters meshingParameters = new MeshingParameters();
        meshingParameters.MaximumEdgeLength = gridSize;
        meshingParameters.MinimumEdgeLength = gridSize;
        meshingParameters.GridAspectRatio = 1;

        List<Mesh[]> griddedMeshArrayList = new List<Mesh[]>();
        foreach (Brep b in geometryInput)
        {
            griddedMeshArrayList.Add(Mesh.CreateFromBrep(b, meshingParameters));
        }

        //Mesh[] griddedMeshArray = Mesh.CreateFromBrep(
        //    geometryInput, meshingParameters);
        Mesh meshJoined = new Mesh();
        foreach (Mesh[] meshArray in griddedMeshArrayList)
        {
            foreach(Mesh m in meshArray){ meshJoined.Append(m); }
        }
        //foreach(Mesh m in griddedMeshArray){ meshJoined.Append(m); }
        meshJoined.FaceNormals.ComputeFaceNormals();

        //add offset distance for all points representing the faces of the
        //gridded mesh
        List<Point3d> points = new List<Point3d>();
        for(int i = 0; i < meshJoined.Faces.Count; i++)
        {
            Point3d tempPoint = meshJoined.Faces.GetFaceCenter(i);
            Vector3d tempVector = meshJoined.FaceNormals[i];
            tempVector = tempVector * .1;  
            points.Add(new Point3d(tempPoint.X + tempVector.X,
                                   tempPoint.Y + tempVector.Y,
                                   tempPoint.Z + tempVector.Z));
        }
        
        // mesh together the geometry and the context
        //todo think this works...check that it doesn't update the other
        foreach(Brep b in geometryInput)
        {
            contextGeometryInput.Append(b);
        }
        //contextGeometryInput.Append(geometryInput);
        Brep mergedContextGeometry = new Brep();
        foreach(Brep b in contextGeometryInput)
        {
            mergedContextGeometry.Append(b);
        }
        Mesh[] contextMeshArray = Mesh.CreateFromBrep(
            mergedContextGeometry, new MeshingParameters());
        Mesh contextMesh = new Mesh();
        foreach(Mesh m in contextMeshArray){ contextMesh.Append(m); }

        //get tragenza dome vectors. to use for intersection later
        Mesh tragenzaDomeMesh = getTragenzaDome();
        //tragenzaDomeMesh.FaceNormals.ComputeFaceNormals();

        //not doing north calculation. relying on user to orient north correctly
        List<Vector3d> allVectors = new List<Vector3d>();
        foreach(Vector3d normal in tragenzaDomeMesh.FaceNormals)
        {
            allVectors.Add(normal);
        }
        allVectors.RemoveRange(allVectors.Count - TREGENZA_PATCHES_PER_ROW[6],
            TREGENZA_PATCHES_PER_ROW[6]);
        allVectors.Add(new Vector3d(0, 0, 1));
        //should have 145 after this

        //lb_grnd_vecs = lb_vecs in reverse (not sure what this is for...
        //doesn't make sense in my head right now)
        List<Vector3d> groundVectors = new List<Vector3d>();
        foreach (Vector3d vec in allVectors)
        {
            vec.Reverse();
            groundVectors.Add(vec);
        }
        allVectors.AddRange(groundVectors);

        //intersect mesh rays. //1 context mesh (currently joined_mesh)
        //normals: array of vec face normals of focus mesh
        //points(all points on selected geomtry),
        //all_vectors(dome vectors and ground vectors)
        //intersect_mesh_rays( what does this do) : Intersect a group of rays (represented by points and vectors) with a mesh. returns
        //intersection_matrix -- A 2D matrix of 0's and 1's indicating the results of the intersection. Each sub-list of the matrix represents one of the
        //points and has a length equal to the vectors. 0 indicates a blocked ray and 1 indicates a ray that was not blocked.
        //angle_matrix -- A 2D matrix of angles in radians. Each sub-list of the matrix represents one of the normals and has a length equal to the
        //supplied vectors. Will be None if no normals are provided.
        //todo add context mesh
        IntersectionObject intersectionObject = intersectMeshRays(contextMesh, points, allVectors, meshJoined.FaceNormals);
        
        //compute the results
        //pt_rel = array of ival(0 or 1) * cos(ang)
        //int_matrix appends above value (0 or cos(ang)
        //rad_result = sum of (pt_rel[](cos of an angle) * all_rad[](dir and diff combined)) ...what is the size of this array? think just 1 number. same size as matrix
        //results appends above values
        List<List<double>> finalIntersectionMatrix = new List<List<double>>();
        List<double> finalRadiationList = new List<double>();

        for(int i = 0; i < intersectionObject.getIntersectionMatrix().Count; i++)
        {
            List<int> tempIntList = intersectionObject.getIntersectionMatrix()[i];
            List<double> tempAngleList = intersectionObject.getAngleMatrix()[i];
            List<double> intersectionCalculationList = new List<double>();

            for(int i2 = 0; i2 < tempAngleList.Count; i2++)
            {
                intersectionCalculationList.Add(tempIntList[i2] * Math.Cos(tempAngleList[i2]));
            }
            finalIntersectionMatrix.Add(intersectionCalculationList);

            double radiationResult = 0;
            for(int i3 = 0; i3 < intersectionCalculationList.Count; i3++)
            {
                radiationResult += intersectionCalculationList[i3] * totalRadiationList[i3];
            }
            finalRadiationList.Add(radiationResult);
        }

        //create the mesh and color
        double maxRadiation = finalRadiationList.Max();
        double minRadiation = finalRadiationList.Min();
        double diffRadiation = maxRadiation - minRadiation;

        //todo change to rgb
        //todo create steps of logical yellow
        List<Color> colorRange = new List<Color>();
        colorRange.Add(Color.Black);
        colorRange.Add(Color.Gray);
        colorRange.Add(Color.Gold);
        colorRange.Add(Color.Yellow);

        double step = 1.0 / colorRange.Count;

        List<Color> faceColors = new List<Color>();

        foreach(double rad in finalRadiationList)
        {
            //if (rad == minRadiation)
            //{
            //    faceColors.Add(colorRange.First());
            //}
            //else if (rad == maxRadiation)
            //{
            //    faceColors.Add(colorRange.Last());
            //}
            //else
            //{
                //get percentage of difference
                double tempRadPercentage = rad / maxRadiation;
                double colorIndTemp = step;
                for(int colorIndCount = 0; colorIndCount < colorRange.Count; colorIndCount++)
                {
                    if( tempRadPercentage <= colorIndTemp)
                    {
                        //need to change how i work with the min and max. this still needs to be altered
                        Color minColor;
                        if(colorIndCount > 0)
                        {
                            minColor = colorRange[colorIndCount - 1];
                        }
                        else
                        {
                            minColor = colorRange[colorIndCount];
                        }

                        Color maxColor = colorRange[colorIndCount];
                        double p = (tempRadPercentage - (colorIndTemp - step)) / (colorIndTemp - (colorIndTemp - step));

                        double red = minColor.R * (1 - p) + maxColor.R * p;
                        double green = minColor.G * (1 - p) + maxColor.G * p;
                        double blue = minColor.B * (1 - p) + maxColor.B * p; 

                        faceColors.Add(Color.FromArgb(255, Convert.ToInt32(red),
                            Convert.ToInt32(green), Convert.ToInt32(blue)));

                        break;
                    }

                    colorIndTemp += step;
                }
            //}
        }

        Mesh finalMesh = new Mesh();
        int faceIndexNumber = 0;
        int fInd = 0;
        foreach (MeshFace f in meshJoined.Faces)
        {
            Point3f a;
            Point3f b;
            Point3f c;
            Point3f d;
            meshJoined.Faces.GetFaceVertices(fInd, out a, out b, out c, out d);

            finalMesh.Vertices.Add(a);
            finalMesh.Vertices.Add(b);
            finalMesh.Vertices.Add(c);
            
            if (f.IsQuad)
            {
                finalMesh.Vertices.Add(d);
                finalMesh.Faces.AddFace(faceIndexNumber, faceIndexNumber + 1, faceIndexNumber + 2, faceIndexNumber + 3);
                faceIndexNumber += 4;
            }
            else
            {
                
                finalMesh.Faces.AddFace(faceIndexNumber, faceIndexNumber + 1, faceIndexNumber + 2);
                faceIndexNumber += 3;
            }
            fInd++;
        }

        finalMesh.VertexColors.CreateMonotoneMesh(Color.Gray);
        faceIndexNumber = 0;
        int colorIndex = 0;
        foreach (MeshFace f in meshJoined.Faces)
        {
            finalMesh.VertexColors[faceIndexNumber] = faceColors[colorIndex];
            finalMesh.VertexColors[faceIndexNumber + 1] = faceColors[colorIndex];
            finalMesh.VertexColors[faceIndexNumber + 2] = faceColors[colorIndex];

            if (f.IsQuad)
            {
                finalMesh.VertexColors[faceIndexNumber + 3] = faceColors[colorIndex];
                faceIndexNumber += 4;
            }
            else
            {
                faceIndexNumber += 3;
            }

            colorIndex++; 
        }

         //Create a plane with a zaxis vector. The center point is set at 0.001
        //so that the graph information will sit in front of the graph background
        //TODO: center point should be impacted by Z height parameter
        Point3d center_point = new Point3d(0, 0, 0.001);
        Point3d height_point = new Point3d(0, 0, 10);
        //Constant? probably
        Vector3d zaxis = height_point - center_point;
        Plane defaultPlane = new Plane(center_point, zaxis);

        //Get the bounding box for the input geometry.
        //This will be used to offset and scale our graph
        Point3d minBoundingBoxPoint = meshJoined.GetBoundingBox(true).Min;
        Point3d maxBoundingBoxPoint = meshJoined.GetBoundingBox(true).Max;

        //Get the bar graph visualization start points (use point data holder
        //instead???). Do this before we scale the bounding box
        double barGraphXStartPoint = maxBoundingBoxPoint.X + 1;
        double barGraphYStartPoint = minBoundingBoxPoint.Y;

        Interval xIntervalBaseMesh = new Interval(barGraphXStartPoint,
            barGraphXStartPoint + ((maxBoundingBoxPoint.X - minBoundingBoxPoint.X) * .05));
        Interval yintervalBaseMesh = new Interval(barGraphYStartPoint, maxBoundingBoxPoint.Y);

        //offset starter plane on z axis so that it does not interfer with
        //ground geometry. TODO: Take this as input 
        Plane baseBarGraphPlane = new Plane(new Point3d(0, 0, 0.0001), zaxis);
        Mesh baseBarGraphMesh = Mesh.CreateFromPlane(baseBarGraphPlane,
            xIntervalBaseMesh, yintervalBaseMesh,
            Convert.ToInt32(xIntervalBaseMesh.Max-xIntervalBaseMesh.Min),
            Convert.ToInt32(yintervalBaseMesh.Max-yintervalBaseMesh.Min));

        //deaulting to a white color. Could allow for specification of base color...
        baseBarGraphMesh.VertexColors.CreateMonotoneMesh(
            Color.FromArgb(250,250, 250, 250));

        List<Color> barGraphVertexColors = new List<Color>();
        int barGraphXLength = Convert.ToInt32(xIntervalBaseMesh.Max - xIntervalBaseMesh.Min) + 1;
        int barGraphYLength = Convert.ToInt32(yintervalBaseMesh.Max - yintervalBaseMesh.Min) + 1;
        double faceRowCount = 1;
        int faceXCount = 0;
        foreach(Color c in baseBarGraphMesh.VertexColors)
        {
            //get percentage of difference
            double tempPercentage = faceRowCount / barGraphYLength;
            double colorIndTemp = step;
            for(int colorIndCount = 0; colorIndCount < colorRange.Count; colorIndCount++)
            {
                if( tempPercentage <= colorIndTemp)
                {
                    //need to change how i work with the min and max. this still needs to be altered
                    Color minColor;
                    if(colorIndCount > 0)
                    {
                        minColor = colorRange[colorIndCount - 1];
                    }
                    else
                    {
                        minColor = colorRange[colorIndCount];
                    }

                    Color maxColor = colorRange[colorIndCount];
                    double p = (tempPercentage - (colorIndTemp - step)) / (colorIndTemp - (colorIndTemp - step));

                    double red = minColor.R * (1 - p) + maxColor.R * p;
                    double green = minColor.G * (1 - p) + maxColor.G * p;
                    double blue = minColor.B * (1 - p) + maxColor.B * p; 

                    barGraphVertexColors.Add(Color.FromArgb(255, Convert.ToInt32(red),
                        Convert.ToInt32(green), Convert.ToInt32(blue)));

                    faceXCount++;
                    if(faceXCount == barGraphXLength)
                    {
                        faceXCount = 0;
                        faceRowCount += 1;
                    }
                    break;
                }
                colorIndTemp += step;
            }
        }

        faceIndexNumber = 0;
        foreach (Color c in barGraphVertexColors)
        {
            baseBarGraphMesh.VertexColors[faceIndexNumber] = c;
            faceIndexNumber++;
        }

        List<Mesh> finalMeshList = new List<Mesh>();

        //detault to text height = .1 and then scale??. Add as a constant??
        DimensionStyle defaultDimensionStyle = new DimensionStyle();
        //defaultDimensionStyle.TextHeight = .1 * legendData.getGraphScale();
        defaultDimensionStyle.TextHeight = 1;

        Point3d center_point_crops = new Point3d(xIntervalBaseMesh.Max + 1, yintervalBaseMesh.Min, 0.001);
        Plane plane_crop = new Plane(center_point_crops, zaxis);

        TextEntity textEntityCropName = TextEntity.Create(
            Convert.ToString(Convert.ToInt32(minRadiation)) + " kW/m2",
            plane_crop, defaultDimensionStyle, true, 10, 0);
        finalMeshList.AddRange(Helper.createTextMesh(textEntityCropName, defaultDimensionStyle));

        center_point_crops = new Point3d(xIntervalBaseMesh.Max + 1, yintervalBaseMesh.Max, 0.001);
        plane_crop = new Plane(center_point_crops, zaxis);

        textEntityCropName = TextEntity.Create(
            Convert.ToString(Convert.ToInt32(maxRadiation)) + " kW/m2",
            plane_crop, defaultDimensionStyle, true, 10, 0);
        finalMeshList.AddRange(Helper.createTextMesh(textEntityCropName, defaultDimensionStyle));

        finalMeshList.Add(finalMesh);
        finalMeshList.Add(baseBarGraphMesh);

        DA.SetDataList(0, finalRadiationList);
        DA.SetDataList(1, finalMeshList);

        //List<Line> lines = new List<Line>();
        //foreach (Ray3d r in intersectionObject.getRayList())
        //{
        //    lines.Add(new Line(r.Position, r.Direction));
        //}

        //List<Point3d> pointsd = new List<Point3d>();
        //foreach(Ray3d r in intersectionObject.getRayList())
        //{
        //    pointsd.Add(r.Position);
        //}

        //DA.SetDataList(2, lines);
        //DA.SetDataList(3, pointsd);
    }

    private Mesh getTragenzaDome()
    {
        // compute constants to be used in the generation of patch points
        // patch row count is just the tragenza patch row counts. not subdivided in our case

        //not sure about all of these
        Vector3d base_vec = new Vector3d(0, 1, 0);
        Vector3d rotateAxis = new Vector3d(1, 0, 0);
        //double vertical_angle = Math.PI / (2 * TREGENZA_PATCHES_PER_ROW_2.Length + 1);
        //should be 15
        double vertical_angle = Math.PI / (2 * TREGENZA_PATCHES_PER_ROW.Length - 1);

        // loop through the patch values and generate points for each vertex
        List<Point3d> vertices = new List<Point3d>();
        List<MeshFace> faces = new List<MeshFace>();

        //foreach(TREGENZA_PATCHES_PER_ROW[0])
        int pt_i = -2;
        //int row_i = 0;

        for(int row_count = 0; row_count < TREGENZA_PATCHES_PER_ROW.Length - 1; row_count++)
        //foreach(int row_count in TREGENZA_PATCHES_PER_ROW_2)
        {
            pt_i += 2;
            double horizontal_angle = -2 * Math.PI / TREGENZA_PATCHES_PER_ROW[row_count];
            //double horizontal_angle = -2 * Math.PI / row_count;
            Vector3d vec01 = new Vector3d(base_vec);
            vec01.Rotate(vertical_angle * row_count, rotateAxis);
            //vec01.Rotate(vertical_angle * row_i, rotateAxis);
            Vector3d vec02 = new Vector3d(vec01);
            vec02.Rotate(vertical_angle, rotateAxis);

            double correctionAngle = -horizontal_angle / 2;
            Vector3d vec1 = new Vector3d(vec01);
            vec1.Rotate(correctionAngle, new Vector3d(0, 0, 1));
            Vector3d vec2 = new Vector3d(vec02);
            vec2.Rotate(correctionAngle, new Vector3d(0, 0, 1));
            //vec1 = rotate_xy(vec1, correctionAngle);
            //vec2 = rotate_xy(vec2, correctionAngle);
            

            //am i adding these vectors correctly and in the right order??? think so
            vertices.Add(new Point3d(vec1));
            vertices.Add(new Point3d(vec2));

            //for(int patchCount = 0; patchCount < row_count; patchCount++)
            for(int patchCount = 0; patchCount < TREGENZA_PATCHES_PER_ROW[row_count]; patchCount++)
            {
                //check horizonatl rotation method...
                Vector3d vec3 = new Vector3d(vec1);
                vec3.Rotate(horizontal_angle, new Vector3d(0, 0, 1));
                Vector3d vec4 = new Vector3d(vec2);
                vec4.Rotate(horizontal_angle, new Vector3d(0, 0, 1));
                //vec3 = rotate_xy(vec3, horizontal_angle);
                //vec4 = rotate_xy(vec4, horizontal_angle);

                vertices.Add(new Point3d(vec3));
                vertices.Add(new Point3d(vec4));

                //not sure what this does....or how it works...
                faces.Add(new MeshFace(pt_i, pt_i + 1, pt_i + 3, pt_i + 2));
                pt_i += 2;
                vec1 = vec3;
                vec2 = vec4;
            }
            //row_i++;
        }

        //todo add this back after I figure out above
        //add triangular faces to represent the last circular patch.....might not need extra patches adding because already added to tregenza patches
        int endVertI = vertices.Count;
        //todo need to check this 
        int startVertI = vertices.Count - TREGENZA_PATCHES_PER_ROW[TREGENZA_PATCHES_PER_ROW.Length - 2] * 2 - 1;
        //int startVertI = vertices.Count - TREGENZA_PATCHES_PER_ROW_2[TREGENZA_PATCHES_PER_ROW_2.Length - 1] * 2 - 1;
        vertices.Add(new Point3d(0, 0, 1));
        //for (int patchCount = 0; patchCount < TREGENZA_PATCHES_PER_ROW_2[TREGENZA_PATCHES_PER_ROW_2.Length - 1] * 2; patchCount += 2)
        for (int patchCount = 0; patchCount < TREGENZA_PATCHES_PER_ROW[TREGENZA_PATCHES_PER_ROW.Length - 2] * 2; patchCount += 2)
        {
            faces.Add(new MeshFace(startVertI + patchCount, endVertI, startVertI + patchCount + 2));
        }


        //todo may need to look at the mesh constructor in ladybug. create the Mesh3D object and derive the patch vectors from the mesh
        Mesh patch_mesh = new Mesh();
        patch_mesh.Faces.AddFaces(faces);
        patch_mesh.Vertices.AddVertices(vertices);
        //think i should do this....
        patch_mesh.FaceNormals.ComputeFaceNormals();
        //think this last part is adding a patch to the end. need to check
        //patch_mesh.FaceNormals

        //convert face normals or return mesh??? think ill return mesh for now

        return patch_mesh;
    }

    private Vector3d rotate_xy(Vector3d vec, double angle)
    {
        double cos_a = Math.Cos(angle);
        double sin_a = Math.Sin(angle);
        double qx = cos_a * vec.X - sin_a * vec.Y;
        double qy = sin_a * vec.X + cos_a * vec.Y;
        return new Vector3d(qx, qy, vec.Z);
    }

    private IntersectionObject intersectMeshRays(
        Mesh contextMesh, List<Point3d> points, List<Vector3d> allVectors,
        MeshFaceNormalList joinedMeshNormals)
    {
        //both matrixes, so might need to change format slightly
        IntersectionObject intersectionObject = new IntersectionObject();
        List<List<int>> intersectionMatrix = new List<List<int>>();
        List<List<double>> angleMatrix = new List<List<double>>();
        double cutoffAngle = Math.PI / 2;

        //todo trials remove this later
        //List<Ray3d> raysList = new List<Ray3d>();

        //todo make this process run with parellel processing in the future??
        for(int i = 0; i < points.Count; i++)
        {
            List<int> intList = new List<int>();
            List<double> angleList = new List<double>();

            Point3d point = points[i];
            Vector3d normalVector = joinedMeshNormals[i];

            foreach(Vector3d vec in allVectors)
            {
                double vectorAngle = Vector3d.VectorAngle(normalVector, vec);
                angleList.Add(vectorAngle);

                if(vectorAngle <= cutoffAngle)
                {
                    Ray3d ray = new Ray3d(point, vec);
                    //todo remove this
                    //raysList.Add(ray);

                    if(Intersection.MeshRay(contextMesh, ray) >= 0)
                    {
                        intList.Add(0);
                    }
                    else
                    {
                        intList.Add(1);
                    }
                }
                else //the vector is pointing below the surface
                {
                    intList.Add(0);
                }
            }

            //not sure what the 'B' and 'd' things are in ladybug
            intersectionMatrix.Add(intList);
            angleMatrix.Add(angleList);
        }


        intersectionObject.setIntersectionMatrix(intersectionMatrix);
        intersectionObject.setAngleMatrix(angleMatrix);
        //todo remove
        //intersectionObject.setRayList(raysList);

        return intersectionObject; 
    }

    private List<double> convertRgbRadiationList(string radiationString)
    {
        List<double> radiationList = new List<double>();
        string[] radiationRGB = radiationString.Split(
            new string[] { "\r\n", "\r", "\n" },
            StringSplitOptions.None
        );

        //todo temp value if all times are used
        double wea_duration = 8760;
        int rowCounter = 1;
        for(int rowOfPatches_count =0; rowOfPatches_count < TREGENZA_PATCHES_PER_ROW.Length; rowOfPatches_count++)
        {
            var currentRowofPatches = new ArraySegment<string>(radiationRGB, rowCounter, TREGENZA_PATCHES_PER_ROW[rowOfPatches_count]);
            foreach(string dr in currentRowofPatches)
            {
                string[] rgb = dr.Split(' ');
                double rgbWeightedValue =
                    0.265074126 * Convert.ToDouble(rgb[0]) +
                    0.670114631 * Convert.ToDouble(rgb[1]) +
                    0.064811243 * Convert.ToDouble(rgb[2]);
                rgbWeightedValue = rgbWeightedValue *
                    TREGENZA_COEFFICIENTS[rowOfPatches_count] *
                    wea_duration / 1000;
                radiationList.Add(rgbWeightedValue);
            }
            rowCounter += TREGENZA_PATCHES_PER_ROW[rowOfPatches_count];
        }

        return radiationList;
    }

    private string callGenDayMtx(string args)
    {
        //todo change the way we're doing the visible sunlight by using. think i was doig that already
        // Use ProcessStartInfo class
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.UseShellExecute = false;
        startInfo.FileName = "/Users/joel/Projects/Programming/AlbaThesis/GrasshopperTools/radiance/bin/gendaymtx";
        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
        startInfo.Arguments = args;
        startInfo.RedirectStandardOutput = true;

        string stdOut = "";  
        try
        {
            using (Process exeProcess = Process.Start(startInfo))
            {
                    
                exeProcess.WaitForExit();
                stdOut = exeProcess.StandardOutput.ReadToEnd();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return stdOut;
    }

    /// <summary>
    /// Provides an Icon for every component that will be visible in the User Interface.
    /// Icons need to be 24x24 pixels.
    /// </summary>
    protected override Bitmap Icon => Properties.Resources.dliSuitability_icon;

    public override GH_Exposure Exposure => GH_Exposure.quarternary;

    /// <summary>
    /// Each component must have a unique Guid to identify it. 
    /// It is vital this Guid doesn't change otherwise old ghx files 
    /// that use the old ID will partially fail during loading.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("ce176854-ff36-4266-8939-9d4e13d4ca9b"); }
    }
  }
}
