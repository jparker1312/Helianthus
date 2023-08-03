using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Helianthus
{

  public class SurfaceCropSuitability : GH_Component
  {
    enum Months  
    {  
        January, February, March, April, May, June, July, August, September,
        October, November, December  
    };

    private MeshHelper meshHelper;

    /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
    public SurfaceCropSuitability()
      : base("Surface_Crop_Suitability",
             "Surface Crop Suitability",
             "Returns a monthly analysis of a surface and recommends " +
             "suitable crops to grow",
             "Helianthus",
             "02 | Analyze Data")
    {
        meshHelper = new MeshHelper();
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
        pManager.AddGenericParameter("WEA_File", "WEA File",
        "File location for .wea file", GH_ParamAccess.item);
        pManager.AddGeometryParameter("SurfaceGeometry", "Surface Geometry",
            "Rhino Surface", GH_ParamAccess.item);
        pManager.AddGeometryParameter("ContextGeometry", "Context Geometry",
            "Rhino Surfaces or Rhino Meshes", GH_ParamAccess.list);
        pManager.AddPointParameter("Diagram_Centerpoint", "Diagram_Centerpoint",
            "Centerpoint for diagran", GH_ParamAccess.item);
        pManager.AddVectorParameter("Diagram_Rotation_Axis", "Diagram Rotation Axis",
            "Diagram Rotation Axis - to help orient planes to top-view",
            GH_ParamAccess.item);
        pManager.AddNumberParameter("Diagram_Rotation", "Diagram Rotation",
            "Diagram Rotation - to help orient planes to top-view",
            GH_ParamAccess.item);
        pManager.AddTextParameter("Location_For_Monthly_WEA_Files",
        "Location For Monthly WEA Files",
        "Location For Monthly WEA Files", GH_ParamAccess.item);
        pManager.AddTextParameter("Month_Range","Month Range",
            "List of Months for computation to run", GH_ParamAccess.list);
        pManager.AddGenericParameter("CropsToVisualize", "Crops To Visualize",
            "List of Crops that you want to visualize", GH_ParamAccess.list);
        pManager.AddGenericParameter("LegendParameters", "Legend Parameters",
            "Legend Parameters for the visualization", GH_ParamAccess.item);
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
        pManager.AddNumberParameter("Radiation_List_By_Month",
            "Radiation List By Month", "Radiation List By Month",
            GH_ParamAccess.tree);
        pManager.AddTextParameter("Crop_Recommendations_By_Month",
            "Crop Recommendations By Month", "Crop Recommendations By Month",
            GH_ParamAccess.tree);
        pManager.AddGenericParameter("Crop_Surface_Object",
            "Crop Surface Object", "Crop Surface Object", GH_ParamAccess.item);
        pManager.AddMeshParameter("Mesh",
            "Mesh", "Mesh", GH_ParamAccess.list);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
    /// to store data in output parameters.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        string weaFileLocation = "";
        Brep geometryInput = new Brep();
        List<Brep> contextGeometryInput = new List<Brep>();
        Point3d diagramCenterpoint = new Point3d();
        Vector3d diagramRotationAxis = new Vector3d();
        double diagramRotation = 0;
        string pathname_MonthlyWeaFiles = "";
        List<string> simulation_monthRange = new List<string>();
        List<CropDataObject> cropDataInput = new List<CropDataObject>();
        LegendDataObject legendData = new LegendDataObject();
        bool run_Simulation = true;

        if (!DA.GetData(0, ref weaFileLocation)) { return; }
        if (!DA.GetData(1, ref geometryInput)) { return; }
        //todo optional???
        if (!DA.GetDataList(2, contextGeometryInput)) { return; }
        if (!DA.GetData(3, ref diagramCenterpoint)) { return; }
        if (!DA.GetData(4, ref diagramRotationAxis)) { return; }
        if (!DA.GetData(5, ref diagramRotation)) { return; }
        if (!DA.GetData(6, ref pathname_MonthlyWeaFiles)) { return; }
        if (!DA.GetDataList(7, simulation_monthRange)) { return; }
        if (!DA.GetDataList(8, cropDataInput)) { return; }
        if (!DA.GetData(9, ref legendData)) { return; }
        if (!DA.GetData(10, ref run_Simulation)) { return; }

        if (!run_Simulation){ return; }

        WeaDataObject weaDataObject = new WeaDataObject(weaFileLocation);

        //create monthly wea files
        if (!weaDataObject.writeWeaDataToMonthlyFiles(pathname_MonthlyWeaFiles))
        {
            //todo throw error
            return;
        }

        //run monthly simulations
        GenDayMtxHelper genDayMtxHelper = new GenDayMtxHelper();
        List<string> directRadiationRGBList =
                genDayMtxHelper.runMonthlyGenDayMtxSimulations(
                    pathname_MonthlyWeaFiles,
                    simulation_monthRange, true);
        List<string> diffuseRadiationRGBList =
                genDayMtxHelper.runMonthlyGenDayMtxSimulations(
                    pathname_MonthlyWeaFiles,
                    simulation_monthRange, false);

        SimulationHelper simulationHelper = new SimulationHelper();
        List<List<double>> monthlyDirectRadiationList = new List<List<double>>();
        List<List<double>> monthlyDiffuseRadiationList = new List<List<double>>();
        List<double> directRadiationList = new List<double>();
        List<double> diffuseRadiationList = new List<double>();
        foreach(string monthRGB in directRadiationRGBList)
        {
            directRadiationList = simulationHelper.convertRgbRadiationList(
                monthRGB);
            monthlyDirectRadiationList.Add(directRadiationList);
        }
        foreach(string monthRGB in diffuseRadiationRGBList)
        {
            diffuseRadiationList = simulationHelper.convertRgbRadiationList(
                monthRGB);
            monthlyDiffuseRadiationList.Add(diffuseRadiationList);
        }

        //add radiation lists together
        List<List<double>> totalRadiationListByMonth = new List<List<double>>();
        for(int i = 0; i < monthlyDirectRadiationList.Count; i++)
        {
            List<double> totalRadiationValues = new List<double>();
            for(int lineCount = 0; lineCount < monthlyDirectRadiationList[i].Count; lineCount++)
            {
                totalRadiationValues.Add(
                    monthlyDirectRadiationList[i][lineCount] +
                    monthlyDiffuseRadiationList[i][lineCount]);
            }
            totalRadiationListByMonth.Add(totalRadiationValues);
        }

        //caculate total radiation
        List<double> sum_totalRadiationList = new List<double>();
        double sum_totalRadiation;
        foreach(List<double> totalRadiationList in totalRadiationListByMonth)
        {
            sum_totalRadiation = 0.0; 
            totalRadiationList.ForEach(x => sum_totalRadiation += x);
            sum_totalRadiationList.Add(sum_totalRadiation);
        }
        
        //.2 is the default value for ground radiaiton
        //get a ground radiation value that is constant and the same size list
        //of the total radiation list
        List<double> groundRadiationConstants = new List<double>();
        foreach(double sum_rad in sum_totalRadiationList)
        {
            double groundRadiationConstant =
                (sum_rad / totalRadiationListByMonth[0].Count) * .2;
            groundRadiationConstants.Add(groundRadiationConstant);
        }

        //add ground radiation constant to total radiation list equal to the
        //total size of the list. Should have 290 values
        for(int count = 0; count < totalRadiationListByMonth.Count; count++)
        {
            List<double> groundRadiationList = new List<double>();
            foreach(double value in totalRadiationListByMonth[count])
            {
                groundRadiationList.Add(groundRadiationConstants[count]);
            }
            totalRadiationListByMonth[count].AddRange(groundRadiationList);
        }

        //Note: these calls should be the same for all monthly meshes
        Mesh joinedMesh = simulationHelper.createJoinedMesh(geometryInput);
        List<Point3d> points = simulationHelper.getMeshJoinedPoints(joinedMesh);
        //todo duplicate in Mesh helper
        Mesh contextMesh = simulationHelper.meshMainGeometryWithContext(
            geometryInput, contextGeometryInput);

        //get tragenza dome vectors. to use for intersection later
        Mesh tragenzaDomeMesh = simulationHelper.getTragenzaDome();
        List<Vector3d> allVectors = simulationHelper.getAllVectors(
            tragenzaDomeMesh);

        IntersectionObject intersectionObject = simulationHelper.intersectMeshRays(
            contextMesh, points, allVectors, joinedMesh.FaceNormals);

        List<Mesh> finalMeshList = new List<Mesh>();
        List<Mesh> tiledMeshList = new List<Mesh>();
        Mesh finalMesh = meshHelper.createFinalMesh(joinedMesh);

        //todo maybe remove this
        double overallMaxRadiation = 0;
        double overallMinRadiation = 0;
        foreach(List<double> radiationList in totalRadiationListByMonth)
        {
            List<double> finalRadiationList =
            simulationHelper.computeFinalRadiationList(
                intersectionObject, radiationList);
            double maxRadiation = finalRadiationList.Max();
            double minRadiation = finalRadiationList.Min();

            if (maxRadiation > overallMaxRadiation)
            {
                overallMaxRadiation = maxRadiation;
            }
            if(minRadiation < overallMinRadiation || overallMinRadiation == 0)
            {
                overallMinRadiation = minRadiation;
            }
        }

        //todo variable for previous bounding box if there is one
        double previousX = 0;

            //start of specific monthly calls


        DataTree<string> monthlyCropsThatFitRangeString = new DataTree<string>();
        DataTree<double> dataTreeRadiation = new DataTree<double>();

        List<double[]> totalRadiationListByMonthArray = new List<double[]>();
        List<List<CropDataObject>> finalMonthlyCropList = new List<List<CropDataObject>>();
        int monthCount = 0;
        foreach(List<double> radiationList in totalRadiationListByMonth)
        {
            Mesh monthlyFinalMesh = finalMesh.DuplicateMesh();
            List<double> finalRadiationList =
                simulationHelper.computeFinalRadiationList(
                    intersectionObject, radiationList);

            Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(monthCount + 1);
            //path.AppendElement(monthCount);

            dataTreeRadiation.AddRange(finalRadiationList, path);
            //path.Increment(0);

            //create the mesh and color
            //todo put this inside getFaceColors
            double maxRadiation = finalRadiationList.Max();
            double minRadiation = finalRadiationList.Min();

            List<Color> faceColors = meshHelper.getFaceColors(
                finalRadiationList, overallMaxRadiation);
            monthlyFinalMesh = meshHelper.colorFinalMesh(monthlyFinalMesh, faceColors);
            monthlyFinalMesh.FaceNormals.ComputeFaceNormals();

            Vector3d faceNrm = monthlyFinalMesh.FaceNormals.First();
            if(faceNrm.IsPerpendicularTo(new Vector3d(0, 0, 1)))
            {
                Point3d cen = monthlyFinalMesh.GetBoundingBox(true).Center;
                //todo or set perpendicular to itself?
                faceNrm.Rotate(1.5708, new Vector3d(0, 0, 1));
                //todo need to change this rotation for planes that are not completely vertical
                monthlyFinalMesh.Rotate(-1.5708, faceNrm, cen);

                if(diagramRotation != 0)
                {
                    double radians = (Math.PI / 180) * diagramRotation;
                    monthlyFinalMesh.Rotate(radians, diagramRotationAxis, cen);
                }
            }

            //todo should i allow input here?
            int diagramSpacer = 2;
            double xIncrement = (previousX + diagramSpacer) * monthCount;
            Point3d centerPtMesh = monthlyFinalMesh.GetBoundingBox(true).Center;
            Vector3d translateVector = Point3d.Subtract(
                diagramCenterpoint, centerPtMesh);
            translateVector.X = translateVector.X + xIncrement;
            monthlyFinalMesh.Translate(translateVector);

            //todo should i do this here?
            tiledMeshList.Add(monthlyFinalMesh.DuplicateMesh());

            //add section header for tiled surface
            Mesh surfaceDliTitleMesh = meshHelper.getTitleTextMesh(
                "Surface DLI", monthlyFinalMesh, 1, 4);

            //add horizontal legend
            LegendHelper legendHelper = new LegendHelper();
            Mesh legendMesh = legendHelper.createLegend(monthlyFinalMesh, false);
            monthlyFinalMesh.Append(legendMesh);
            monthlyFinalMesh.Append(surfaceDliTitleMesh);

            //Add legend descriptors
            Mesh legendDescriptorMin = legendHelper.addLegendDescriptor(
                Convert.ToString(Convert.ToInt32(overallMinRadiation)) + " Min/yr",
                    legendMesh.GetBoundingBox(true).Min.X,
                    legendMesh.GetBoundingBox(true).Min.Y - 1, .5);
            Mesh legendDescriptorMax = legendHelper.addLegendDescriptor(
                Convert.ToString(Convert.ToInt32(overallMaxRadiation)) + " Max/yr",
                    legendMesh.GetBoundingBox(true).Max.X,
                    legendMesh.GetBoundingBox(true).Min.Y - 1, .5);
            monthlyFinalMesh.Append(legendDescriptorMin);
            monthlyFinalMesh.Append(legendDescriptorMax);

            double legendWidth = legendMesh.GetBoundingBox(true).Max.X -
                legendMesh.GetBoundingBox(true).Min.X;
            double legendHeight = legendMesh.GetBoundingBox(true).Max.Y -
                legendMesh.GetBoundingBox(true).Min.Y;
            double percentageRadMax = maxRadiation / overallMaxRadiation;
            double maxXPos = percentageRadMax * legendWidth;
            double percentageRadMin = minRadiation / overallMaxRadiation;
            double minXPos = percentageRadMin * legendWidth;
            if(minRadiation == maxRadiation)
            {
                Mesh legendDescriptorMonthlyMin = legendHelper.addLegendDescriptor(
                Convert.ToString(Convert.ToInt32(minRadiation)) + " Min/Max/mth",
                legendMesh.GetBoundingBox(true).Min.X,
                legendMesh.GetBoundingBox(true).Max.Y + 1, .5);

                legendDescriptorMonthlyMin.Translate(new Vector3d(
                    minXPos, 0, 0));

                monthlyFinalMesh.Append(legendDescriptorMonthlyMin);
            }
            else
            {
                Mesh legendDescriptorMonthlyMin = legendHelper.addLegendDescriptor(
                    Convert.ToString(Convert.ToInt32(minRadiation)) + " Min/mth",
                    legendMesh.GetBoundingBox(true).Min.X,
                    legendMesh.GetBoundingBox(true).Max.Y + 1, .5);
                Mesh legendDescriptorMonthlyMax = legendHelper.addLegendDescriptor(
                    Convert.ToString(Convert.ToInt32(maxRadiation)) + " Max/mth",
                    legendMesh.GetBoundingBox(true).Max.X,
                    legendMesh.GetBoundingBox(true).Max.Y + 1, .5);

                legendDescriptorMonthlyMin.Translate(new Vector3d(
                    minXPos, 0, 0));
                legendDescriptorMonthlyMax.Translate(new Vector3d(
                    -(legendWidth - maxXPos), 0, 0));

                monthlyFinalMesh.Append(legendDescriptorMonthlyMin);
                monthlyFinalMesh.Append(legendDescriptorMonthlyMax);
            }

            Mesh minMonthMarker = meshHelper.create2dMesh(legendHeight, (overallMaxRadiation / legendWidth) * .1);
            minMonthMarker.Translate(new Vector3d(legendMesh.GetBoundingBox(true).Min.X, legendMesh.GetBoundingBox(true).Min.Y, 0));
            minMonthMarker.Translate(new Vector3d(minXPos, 0, 0));
            monthlyFinalMesh.Append(minMonthMarker);

            Mesh maxMonthMarker = meshHelper.create2dMesh(legendHeight, (overallMaxRadiation / legendWidth) * .1);
            maxMonthMarker.Translate(new Vector3d(legendMesh.GetBoundingBox(true).Min.X, legendMesh.GetBoundingBox(true).Min.Y, 0));
            maxMonthMarker.Translate(new Vector3d(maxXPos, 0, 0));
            monthlyFinalMesh.Append(maxMonthMarker);


            //add section header for tiled surface
            Mesh cropRecommendationsTitleMesh = meshHelper.getTitleTextMeshByPosition(
                "Crop Recommendations",
                new Point3d(monthlyFinalMesh.GetBoundingBox(true).Min.X,
                monthlyFinalMesh.GetBoundingBox(true).Min.Y - 2, 0.001),
                1,
            monthlyFinalMesh.GetBoundingBox(true).Max.X -
            monthlyFinalMesh.GetBoundingBox(true).Min.X);
            monthlyFinalMesh.Append(cropRecommendationsTitleMesh);

            //add crops bar graph
            BarGraphHelper barGraphHelper = new BarGraphHelper();
            Mesh barGraphMesh = barGraphHelper.createBarGraph(monthlyFinalMesh,
                cropDataInput, legendData, maxRadiation, overallMaxRadiation);
            monthlyFinalMesh.Append(barGraphMesh);

            //add month name
            //todo change width calculation
            Mesh monthTitleMesh = meshHelper.getTitleTextMesh(
                Convert.ToString((Months)monthCount), monthlyFinalMesh, 2, 4);
            monthlyFinalMesh.Append(monthTitleMesh);

            //add bg plane
            Mesh meshBase2dPlane = meshHelper.create2dBaseMesh(monthlyFinalMesh);
            monthlyFinalMesh.Append(meshBase2dPlane);
            //otherMeshList.Add(meshBase2dPlane);
            previousX = meshBase2dPlane.GetBoundingBox(true).Max.X -
                    meshBase2dPlane.GetBoundingBox(true).Min.X;

            //todo can just return mesh instead of list?? not sure i want to
            finalMeshList.Add(monthlyFinalMesh);


            List<CropDataObject> monthlyCropsThatFitRange = new List<CropDataObject>();
            //todo change this. make this the string below. do this before bar graph call
            foreach (CropDataObject crop in cropDataInput)
            {
                //todo need to get passed the max radiation value converted to dli
                if (crop.getDli() < maxRadiation)
                {
                    monthlyCropsThatFitRange.Add(crop);
                    monthlyCropsThatFitRangeString.Add(crop.getSpecie(), path);
                }
            }
            finalMonthlyCropList.Add(monthlyCropsThatFitRange); 
            monthCount++;
        }

        CropSurfaceObject cropSurfaceObject = new CropSurfaceObject(
            finalMeshList, cropDataInput);

        //todo output input parameters
        DA.SetDataList(0, totalRadiationListByMonth);
        DA.SetDataTree(1, dataTreeRadiation);
        DA.SetDataTree(2, monthlyCropsThatFitRangeString);
        DA.SetData(3, cropSurfaceObject);

        //DataTree<CropDataObject> dt = new DataTree<CropDataObject>();
        //    dt.Branch().Add(cropDataInput[0]);

        //CropSurfaceObject cropSurfaceObject = new CropSurfaceObject(tiledMeshList, cropDataInput);
        DA.SetDataList(4, finalMeshList);
        }

    /// <summary>
    /// Provides an Icon for every component that will be visible in the User Interface.
    /// Icons need to be 24x24 pixels.
    /// </summary>
    protected override Bitmap Icon
    {
      get
      { 
        // You can add image files to your project resources and access them like this:
        //return Resources.IconForThisComponent;
        return null;
      }
    }

    /// <summary>
    /// Each component must have a unique Guid to identify it. 
    /// It is vital this Guid doesn't change otherwise old ghx files 
    /// that use the old ID will partially fail during loading.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("aed67efa-3fb3-4273-8f4b-5f4b391f6778"); }
    }
  }
}
