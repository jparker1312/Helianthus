using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Helianthus
{

  public class CropSuitability : GH_Component
  {
    private MeshHelper meshHelper;
    private SimulationHelper simulationHelper;

    /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
    public CropSuitability()
      : base("Crop_Suitability",
             "Crop Suitability",
             "Calculates the suitable crops for the selected surface on a " +
             "monthly basis according to the crop DLI requirements and " +
             "the simulated sunlight DLI reaching the surface.",
             "Helianthus",
             "03 | Visualize Data")
    {
        meshHelper = new MeshHelper();
        simulationHelper = new SimulationHelper();
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddGenericParameter(
            "Crop_Data",
            "Crop Data",
            "Imported crop data to contrast with surface specifications, " +
            "obtained from the Import_Crop_Data component. ",
            GH_ParamAccess.list);
        pManager.AddGenericParameter(
            "WEA_File",
            "WEA File",
            "File path for .wea file." +
            $"{Environment.NewLine}{Environment.NewLine}" +
            "WEA files contain Daysim weather format with the sunlight " +
            "climate data specifically to support building simulations. As " +
            "such, the files are Typical Meteorological Years (TMY) " +
            "published by a variety of organizations." +
            $"{Environment.NewLine}{Environment.NewLine}" +
            "The repository of climate data files can be found on " +
            "climate.onebuilding.org",
            GH_ParamAccess.item);
        pManager.AddTextParameter(
            "Monthly_WEA_Files_Folder",
            "Monthly WEA Files Folder",
            "Folder path for saving generated monthly WEA files.",
            GH_ParamAccess.item);
        pManager.AddGeometryParameter(
            "Analysis_Geometry",
            "Analysis Geometry",
            "Takes one Rhino Surface or Rhino Mesh to analyze",
            GH_ParamAccess.item);
        pManager.AddGeometryParameter(
            "Context_Geometry",
            "Context Geometry",
            "Optional. Rhino Surfaces or Rhino Meshes that can block " +
            "sunlight from reaching the analysis geometry",
            GH_ParamAccess.list);
        pManager[4].Optional = true;
        pManager.AddTextParameter(
            "Simulation_Period",
            "Simulation Period",
            "Optional. A simulation period containing a list of months for " +
            "computation to run. Note that the simulation works on a monhtly " +
            "basis. To add the months, it is required input a panel with the " +
            "'multiline data' option unchecked. " +
            $"{Environment.NewLine}{Environment.NewLine}" +
            "If unspecified, the simulation will run for all 12 months.",
            GH_ParamAccess.list);
        pManager[5].Optional = true;
        pManager.AddGenericParameter(
            "Cover_Material",
            "Cover_Material",
            "Optional. The selected cover material with its light " +
            "transmittance value, Various material types can be found under " +
            "the Glass_Cover and Plastic_Cover components. If unspecified, " +
            "defaults to an outdoor growth environment.",
            GH_ParamAccess.item);
        pManager[6].Optional = true;
        pManager.AddGenericParameter(
            "Visualization_Parameters",
            "Visualization Parameters",
            "Visualization parameters for legends and diagrams.",
            GH_ParamAccess.item);
        pManager[7].Optional = true;
        pManager.AddBooleanParameter(
            "Run_Simulation",
            "Run Simulation",
            "Run Simulation",
            GH_ParamAccess.item);
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter(
            "Out",
            "Out",
            "Outputs the input parameters",
            GH_ParamAccess.list);
        pManager.AddNumberParameter(
            "Monthly_Surface_DLIs",
            "Monthly Surface DLIs",
            "Outputs the total list of surface DLIs for the selected " +
            "surface and simulation period.",
            GH_ParamAccess.tree);
        pManager.AddTextParameter(
            "Monthly_Suitable_Crops",
            "Monthly Suitable Crops",
            "Outputs the total list of suitable crops for the selected " +
            "surface and simulation period.",
            GH_ParamAccess.tree);
        pManager.AddGenericParameter(
            "Monthly_Analysis",
            "Monthly Analysis",
            "List of monthly data containing the displayed meshes and " +
            "simulated data.",
            GH_ParamAccess.list);
        pManager.AddMeshParameter(
            "Mesh",
            "Mesh",
            "A monthly list of a colored mesh of the analysis geometry " +
            "showing gridded DLIs and a suitable crops bar graph.",
            GH_ParamAccess.list);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
    /// to store data in output parameters.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        //REQ
        List<CropDataObject> cropDataInput = new List<CropDataObject>();
        string weaFileLocation = "";
        string pathname_MonthlyWeaFiles = "";
        Brep geometryInput = new Brep();
        bool run_Simulation = false;
        //OPT
        List<Brep> contextGeometryInput = new List<Brep>();
        List<string> simulation_monthRange = new List<string>();
        MaterialDataObject materialDataObject = new MaterialDataObject();
        VisualizationDataObject visualizationParams = new VisualizationDataObject();

        if (!DA.GetDataList(0, cropDataInput)) { return; }
        if (!DA.GetData(1, ref weaFileLocation)) { return; }
        if (!DA.GetData(2, ref pathname_MonthlyWeaFiles)) { return; }
        if (!DA.GetData(3, ref geometryInput)) { return; }
        DA.GetDataList(4, contextGeometryInput);
        DA.GetDataList(5, simulation_monthRange);
        DA.GetData(6, ref materialDataObject);
        DA.GetData(7, ref visualizationParams);
        if (!DA.GetData(8, ref run_Simulation)) { return; }

        if (!run_Simulation){ return; }

        //create monthly wea files
        WeaDataObject weaDataObject = new WeaDataObject(weaFileLocation);
        if (!weaDataObject.writeWeaDataToMonthlyFiles(
            pathname_MonthlyWeaFiles))
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                "Could not write WEA monthly files: please check path");
            return;
        }

        //run monthly simulations
        GenDayMtxHelper genDayMtxHelper = new GenDayMtxHelper();
        List<List<double>> totalRadiationListByMonth = genDayMtxHelper.
            runMonthlyGenDayMtxSimulations2(
                pathname_MonthlyWeaFiles, simulation_monthRange);

        //new section for joinedmesh
        //create gridded mesh from geometry
        List<Brep> geometryInputList = new List<Brep>{ geometryInput };
        Mesh joinedMesh = meshHelper.createGriddedMesh(geometryInputList, 1);
        Mesh finalMesh = meshHelper.createFinalMesh(joinedMesh);

        double overallMaxRadiation = 0;
        double overallMinRadiation = 0;
        DataTree<double> dataTreeRadiation = new DataTree<double>();
        DataTree<string> monthlyCropsThatFitRangeString = new DataTree<string>();
        int monthCount = 1;
        List<TiledMeshObject> tiledMeshObjects = new List<TiledMeshObject>();
        foreach(List<double> radiationList in totalRadiationListByMonth)
        {
            List<double> finalRadiationList = simulationHelper.
                getSimulationRadiationList(joinedMesh, geometryInputList,
                    contextGeometryInput, radiationList,
                    materialDataObject.getMaterialTransparency());

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

            //todo need to think more about how im assigning the
            //data trees to the indiv tiled objects
            //todo can i simplify this and put this in the tiled object?
            Grasshopper.Kernel.Data.GH_Path path = new
                    Grasshopper.Kernel.Data.GH_Path(monthCount);
            dataTreeRadiation.AddRange(radiationList, path);

            List<string> recCropList = populateDataTreeWithRecommendedCrops2(
                cropDataInput, maxRadiation);
            monthlyCropsThatFitRangeString.AddRange(recCropList, path);

            TiledMeshObject tiledMeshObject = new TiledMeshObject();
            //todo maybe these should just be lists until the end and then
            //convert to datatree
            tiledMeshObject.setRadiationList(finalRadiationList);
            tiledMeshObject.setMonthlyCropsList(recCropList);
            tiledMeshObjects.Add(tiledMeshObject);
            monthCount++;
        }

        //start of specific monthly calls
        //variable for previous bounding box if there is one
        Point3d previousBoundingBoxX = new Point3d();  
        List<Mesh> finalMeshList = new List<Mesh>();
        monthCount = 0;
        foreach(TiledMeshObject tiledMeshObject in tiledMeshObjects)
        {
            Mesh monthlyFinalMesh = finalMesh.DuplicateMesh();
            Mesh tempAppendedMeshTiled = new Mesh();

            //color mesh tiles
            createTiledMeshSection(tiledMeshObject, monthlyFinalMesh,
                overallMaxRadiation, overallMinRadiation, previousBoundingBoxX,
                monthCount, tempAppendedMeshTiled, visualizationParams);

            //create bar graph
            double maxRadiation = tiledMeshObject.getRadiationList().Max();
            BarGraphObject barGraphObject = createBarGraphSection(
                tempAppendedMeshTiled, cropDataInput, visualizationParams, maxRadiation,
                overallMaxRadiation);
            tiledMeshObject.setBarGraphObject(barGraphObject);

            //add month name
            Mesh monthTitleMesh = meshHelper.getTitleTextMesh(
                Convert.ToString((Config.Months)monthCount),
                tempAppendedMeshTiled, 2, 4, true);
            tiledMeshObject.setMonthTitleMesh(monthTitleMesh);
            tempAppendedMeshTiled.Append(monthTitleMesh);

            //add bg plane
            Mesh meshBase2dPlane = meshHelper.create2dBaseMesh(
                tempAppendedMeshTiled);
            tiledMeshObject.setBackgroundMesh(meshBase2dPlane);
            tempAppendedMeshTiled.Append(meshBase2dPlane);

            //incrementing next section position
            previousBoundingBoxX = meshBase2dPlane.GetBoundingBox(true).Max;

            finalMeshList.Add(tempAppendedMeshTiled);
            monthCount++;
        }

        List<string> inputParams = new List<string>
        {
            weaFileLocation,
            geometryInput.ToString(),
            contextGeometryInput.ToString(),
            pathname_MonthlyWeaFiles,
            simulation_monthRange.ToString(),
            cropDataInput.ToString(),
            visualizationParams.ToString(),
        };

        DA.SetDataList(0, inputParams);
        DA.SetDataTree(1, dataTreeRadiation);
        DA.SetDataTree(2, monthlyCropsThatFitRangeString);
        DA.SetDataList(3, tiledMeshObjects);
        DA.SetDataList(4, finalMeshList);
    }

    private void createTiledMeshSection(TiledMeshObject tiledMeshObject,
        Mesh monthlyFinalMesh, double overallMaxRadiation,
        double overallMinRadiation, Point3d previousBoundingBox, int monthCount,
        Mesh tempAppendedMeshTiled, VisualizationDataObject visualizationParams)
    {
        //color the mesh
        List<Color> faceColors = meshHelper.getFaceColors(
            tiledMeshObject.getRadiationList(), overallMaxRadiation);
        monthlyFinalMesh = meshHelper.colorFinalMesh(monthlyFinalMesh,
            faceColors);
        monthlyFinalMesh.FaceNormals.ComputeFaceNormals();

        meshHelper.rotateSurfaceToTopView(monthlyFinalMesh,
            visualizationParams.getDiagramRotation(),
            visualizationParams.getDiagramRotationAxis());

        Point3d cenPtMesh = monthlyFinalMesh.GetBoundingBox(true).Center;
        Vector3d translateVector = Point3d.Subtract(
            visualizationParams.getDiagramCenterpoint(), cenPtMesh);
        monthlyFinalMesh.Translate(translateVector);

        incrementMeshPosition(monthlyFinalMesh, previousBoundingBox,
            monthCount);

        tiledMeshObject.setTiledMesh(monthlyFinalMesh);
        tempAppendedMeshTiled.Append(monthlyFinalMesh);

        //add horizontal legend
        double maxRadiation = tiledMeshObject.getRadiationList().Max();
        double minRadiation = tiledMeshObject.getRadiationList().Min();
        Mesh legendMesh = createLegendWithDescriptors(monthlyFinalMesh,
            minRadiation, maxRadiation, overallMinRadiation,
            overallMaxRadiation);
        tiledMeshObject.setLegendMesh(legendMesh);
        tempAppendedMeshTiled.Append(legendMesh);

        //add section header for tiled surface
        Mesh surfaceDliTitleMesh = meshHelper.getTitleTextMesh(
            "Surface DLI", monthlyFinalMesh, 1, 4, true);
        tiledMeshObject.setTitleMesh(surfaceDliTitleMesh);
        tempAppendedMeshTiled.Append(surfaceDliTitleMesh);
    }

    private BarGraphObject createBarGraphSection(
        Mesh boundingMesh, List<CropDataObject> cropDataInput,
        VisualizationDataObject visualizationParams, double maxRadiation,
        double overallMaxRadiation)
    {
        //create bar graph section
        BarGraphObject barGraphObject = new BarGraphObject();
        //add section header for Bar
        Mesh cropRecommendationsTitleMesh = meshHelper.
            getTitleTextMeshByPosition("Crop Recommendations",
                new Point3d(boundingMesh.GetBoundingBox(true).Min.X,
                boundingMesh.GetBoundingBox(true).Min.Y - 2, 0.001), 1,
                boundingMesh.GetBoundingBox(true).Max.X -
                boundingMesh.GetBoundingBox(true).Min.X);

        barGraphObject.setTitleMesh(cropRecommendationsTitleMesh);
        boundingMesh.Append(cropRecommendationsTitleMesh);

        //create bar graph mesh
        BarGraphHelper barGraphHelper = new BarGraphHelper();
        Mesh barGraphMesh = barGraphHelper.createBarGraph2(
            boundingMesh, cropDataInput, maxRadiation, overallMaxRadiation,
            null, Config.BarGraphType.DLI.ToString());
        barGraphObject.setBarGraphMesh(barGraphMesh);
        boundingMesh.Append(barGraphMesh);

        return barGraphObject;
    }

    private Mesh createLegendWithDescriptors(Mesh mesh, double minRadiation,
        double maxRadiation, double overallMinRadiation,
        double overallMaxRadiation)
    {
        //add horizontal legend
        LegendHelper legendHelper = new LegendHelper();
        Mesh legendMesh = legendHelper.createLegend(mesh, false);

        double legendWidth = legendMesh.GetBoundingBox(true).Max.X -
            legendMesh.GetBoundingBox(true).Min.X;
        double legendHeight = legendMesh.GetBoundingBox(true).Max.Y -
            legendMesh.GetBoundingBox(true).Min.Y;
        double percentageRadMax = maxRadiation / overallMaxRadiation;
        double maxXPos = percentageRadMax * legendWidth;
        double percentageRadMin = minRadiation / overallMaxRadiation;
        double minXPos = percentageRadMin * legendWidth;

        Mesh minMonthMarker = meshHelper.create2dMesh(legendHeight,
            (overallMaxRadiation / legendWidth) * .1);
        minMonthMarker.Translate(new Vector3d(
            legendMesh.GetBoundingBox(true).Min.X,
            legendMesh.GetBoundingBox(true).Min.Y, 0));
        minMonthMarker.Translate(new Vector3d(minXPos, 0, 0));

        Mesh maxMonthMarker = meshHelper.create2dMesh(legendHeight,
            (overallMaxRadiation / legendWidth) * .1);
        maxMonthMarker.Translate(new Vector3d(
            legendMesh.GetBoundingBox(true).Min.X,
            legendMesh.GetBoundingBox(true).Min.Y, 0));
        maxMonthMarker.Translate(new Vector3d(maxXPos, 0, 0));

        //Add legend descriptors
        Mesh legendDescriptorMin = legendHelper.addLegendDescriptor(
            Convert.ToString(Convert.ToInt32(overallMinRadiation)) + " Min/yr",
                legendMesh.GetBoundingBox(true).Min.X,
                legendMesh.GetBoundingBox(true).Min.Y - .5, .5, 3);
        Mesh legendDescriptorMax = legendHelper.addLegendDescriptor(
            Convert.ToString(Convert.ToInt32(overallMaxRadiation)) + " Max/yr",
                legendMesh.GetBoundingBox(true).Max.X,
                legendMesh.GetBoundingBox(true).Min.Y - .5, .5, 3);

        if (minRadiation == maxRadiation)
        {
            Mesh legendDescriptorMonthlyMin = legendHelper.addLegendDescriptor(
            Convert.ToString(Convert.ToInt32(minRadiation)) + " Min/Max/mth",
            legendMesh.GetBoundingBox(true).Min.X,
            legendMesh.GetBoundingBox(true).Max.Y + 1, .5, 3);
            legendDescriptorMonthlyMin.Translate(new Vector3d(
                minXPos, 0, 0));
            legendMesh.Append(legendDescriptorMonthlyMin);
        }
        else
        {
            Mesh legendDescriptorMonthlyMin = legendHelper.addLegendDescriptor(
                Convert.ToString(Convert.ToInt32(minRadiation)) + " Min/mth",
                legendMesh.GetBoundingBox(true).Min.X,
                legendMesh.GetBoundingBox(true).Max.Y + 1.5, .5, 3);
            Mesh legendDescriptorMonthlyMax = legendHelper.addLegendDescriptor(
                Convert.ToString(Convert.ToInt32(maxRadiation)) + " Max/mth",
                legendMesh.GetBoundingBox(true).Max.X,
                legendMesh.GetBoundingBox(true).Max.Y + 1.5, .5, 3);

            legendDescriptorMonthlyMin.Translate(new Vector3d(
                minXPos, 0, 0));
            legendDescriptorMonthlyMax.Translate(new Vector3d(
                -(legendWidth - maxXPos), 0, 0));
            legendMesh.Append(legendDescriptorMonthlyMin);
            legendMesh.Append(legendDescriptorMonthlyMax);
        }

        legendMesh.Append(minMonthMarker);
        legendMesh.Append(maxMonthMarker);
        legendMesh.Append(legendDescriptorMin);
        legendMesh.Append(legendDescriptorMax);
        return legendMesh;
    }

    private List<string> populateDataTreeWithRecommendedCrops2(
        List<CropDataObject> cropDataList, double maxRadiation)
    {
        List<string> cropList = new List<string>();
        foreach (CropDataObject crop in cropDataList)
        {
            if (crop.getDli() < maxRadiation)
            {
                cropList.Add(crop.getSpecie());
            }
        }
        return cropList;
    }

    private void incrementMeshPosition(Mesh mesh,
        Point3d previousBoundingBox, int monthCount)
    {
        int diagramSpacer = 2;
        double diagramXIncrement = previousBoundingBox.X + diagramSpacer;
        Point3d minPtMesh = mesh.GetBoundingBox(true).Min;    
        Vector3d temp = new Vector3d(diagramXIncrement - minPtMesh.X, 0, 0);
        if(monthCount != 0) { mesh.Translate(temp); }
    }

    /// <summary>
    /// Provides an Icon for every component that will be visible in the User Interface.
    /// Icons need to be 24x24 pixels.
    /// </summary>
    protected override Bitmap Icon => Properties.Resources.cropSuitability_icon;

    public override GH_Exposure Exposure => GH_Exposure.secondary;

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
