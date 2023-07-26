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
using Rhino.DocObjects;

namespace Helianthus
{
  public class SiteSuitability : GH_Component
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
    public SiteSuitability()
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
        //todo decide if we want to give this option
        pManager.AddNumberParameter("GridSize", "Grid Size",
            "Grid Size for output geometry", GH_ParamAccess.item);
        //todo add boolean run parameter
        pManager.AddBooleanParameter("Run_Simulation", "Run Simulation",
            "Run Simulation", GH_ParamAccess.item);
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
        bool run_Simulation = true;

        if (!DA.GetData(0, ref weaFileLocation)) { return; }
        if (!DA.GetDataList(1, geometryInput)) { return; }
        //todo optional???
        if (!DA.GetDataList(2, contextGeometryInput)) { }
        if (!DA.GetData(3, ref gridSize)) { return; }
        if (!DA.GetData(4, ref run_Simulation)) { return; }
        if (!run_Simulation){ return; }

        string gendaymtx_arg_direct = GenDayMtxHelper.gendaymtx_arg_direct +
                weaFileLocation;
        string gendaymtx_arg_diffuse = GenDayMtxHelper.gendaymtx_arg_diffuse +
                weaFileLocation;
        GenDayMtxHelper genDayMtxHelper = new GenDayMtxHelper();
        string directRadiationRGB = genDayMtxHelper.callGenDayMtx(
            gendaymtx_arg_direct);
        string diffuseRadiationRGB = genDayMtxHelper.callGenDayMtx(
            gendaymtx_arg_diffuse);

        List<double> directRadiationList = new List<double>();
        List<double> diffuseRadiationList = new List<double>();
        directRadiationList = genDayMtxHelper.convertRgbRadiationList(
            directRadiationRGB);
        diffuseRadiationList = genDayMtxHelper.convertRgbRadiationList(
            diffuseRadiationRGB);

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

        Mesh meshJoined = new Mesh();
        foreach (Mesh[] meshArray in griddedMeshArrayList)
        {
            foreach(Mesh m in meshArray){ meshJoined.Append(m); }
        }

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
        foreach(Brep b in geometryInput)
        {
            contextGeometryInput.Append(b);
        }
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
        SimulationHelper simulationHelper = new SimulationHelper();
        Mesh tragenzaDomeMesh = simulationHelper.getTragenzaDome();

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
        IntersectionObject intersectionObject = simulationHelper.
                intersectMeshRays(contextMesh, points, allVectors,
                    meshJoined.FaceNormals);
        
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

        //todo check this
        MeshHelper meshHelper = new MeshHelper();
        List<Color> faceColors = meshHelper.getFaceColors(finalRadiationList, maxRadiation);

        Mesh finalMesh = meshHelper.createFinalMesh(meshJoined);

        //todo see if this works without assigning back to finalmesh
        meshHelper.colorFinalMesh(finalMesh, faceColors);

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
        double barGraphXEndPoint = barGraphXStartPoint +
                ((maxBoundingBoxPoint.X - minBoundingBoxPoint.X) * .05);

        //todo change this quick fix
        if(barGraphXEndPoint - barGraphXStartPoint < 1)
        {
            barGraphXEndPoint = barGraphXStartPoint + 1;
        }

        Interval xIntervalBaseMesh = new Interval(barGraphXStartPoint,
            barGraphXEndPoint);
        Interval yintervalBaseMesh = new Interval(barGraphYStartPoint,
            maxBoundingBoxPoint.Y);

        //offset starter plane on z axis so that it does not interfer with
        //ground geometry. TODO: Take this as input 
        Plane baseBarGraphPlane = new Plane(new Point3d(0, 0, 0.0001), zaxis);
        int xCount = Convert.ToInt32(xIntervalBaseMesh.Max-xIntervalBaseMesh.Min);
        int yCount = Convert.ToInt32(yintervalBaseMesh.Max-yintervalBaseMesh.Min);
        if(xCount < 2){ xCount = 2; }
        if(yCount < 10){ yCount = 10; }
        Mesh baseBarGraphMesh = Mesh.CreateFromPlane(baseBarGraphPlane,
            xIntervalBaseMesh, yintervalBaseMesh,xCount, yCount);

        //deaulting to a white color. Could allow for specification of base color...
        baseBarGraphMesh.VertexColors.CreateMonotoneMesh(
            Color.FromArgb(250, 250, 250, 250));


        //todo remove temp
        List<Color> colorRange = new List<Color>();
        colorRange.Add(Color.FromArgb(5, 7, 0));
        colorRange.Add(Color.FromArgb(41, 66, 0));
        colorRange.Add(Color.FromArgb(78, 125, 0));
        colorRange.Add(Color.FromArgb(114, 184, 0));
        colorRange.Add(Color.FromArgb(150, 243, 0));
        colorRange.Add(Color.FromArgb(176, 255, 47));
        colorRange.Add(Color.FromArgb(198, 255, 106));
        double step = 1.0 / colorRange.Count;
        double colorIndTemp;

        List<Color> barGraphVertexColors = new List<Color>();
        int maxXVertices = xCount + 1;
        int maxYVertices = yCount + 1;
        double faceRowCount = 1;
        int faceXCount = 0;
        foreach(Color c in baseBarGraphMesh.VertexColors)
        {
            //get percentage of difference
            double tempPercentage = faceRowCount / maxYVertices;
            colorIndTemp = step;
            for(int colorIndCount = 0; colorIndCount < colorRange.Count; colorIndCount++)
            {
                if( tempPercentage <= colorIndTemp ||
                        (tempPercentage == 1 && colorIndCount == (colorRange.Count - 1)))
                {
                    Color minColor;
                    if(colorIndCount > 0)
                    {
                        minColor = colorRange[colorIndCount - 1];
                    }
                    else
                    {
                        minColor = colorRange[colorIndCount];
                    }

                    //if(tempPercentage == 1)
                    //{

                    //}

                    Color maxColor = colorRange[colorIndCount];
                    double p = (tempPercentage - (colorIndTemp - step)) / (colorIndTemp - (colorIndTemp - step));
                    double red = minColor.R * (1 - p) + maxColor.R * p;
                    double green = minColor.G * (1 - p) + maxColor.G * p;
                    double blue = minColor.B * (1 - p) + maxColor.B * p; 
                    barGraphVertexColors.Add(Color.FromArgb(255, Convert.ToInt32(red),
                        Convert.ToInt32(green), Convert.ToInt32(blue)));

                    faceXCount++;
                    if(faceXCount == maxXVertices)
                    {
                        faceXCount = 0;
                        faceRowCount += 1;
                    }
                    break;
                }
                colorIndTemp += step;
            }
        }

        int faceIndexNumber = 0;
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
