using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Helianthus.Components
{

  public class SurfaceCropSuitability : GH_Component
  {
    enum Months  
    {  
        January, February, March, April, May, June, July, August, September, October, November, December  
    };  

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
             "Returns a monthly analysis of a surface and recommends suitable " +
             "crops to grow",
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
        pManager.AddTextParameter("Radiation_List_By_Month",
            "Radiation List By Month", "Radiation List By Month",
            GH_ParamAccess.list);
        pManager.AddGenericParameter("Crop_Recommendations_By_Month",
            "Crop Recommendations By Month", "Crop Recommendations By Month",
            GH_ParamAccess.list);
        pManager.AddMeshParameter("Mesh", "Mesh", "Mesh viz",
            GH_ParamAccess.list);
        pManager.AddMeshParameter("Other_Mesh", "Other Mesh", "Mesh viz",
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
                    GenDayMtxHelper.gendaymtx_arg_direct,
                    simulation_monthRange);
        List<string> diffuseRadiationRGBList =
                genDayMtxHelper.runMonthlyGenDayMtxSimulations(
                    pathname_MonthlyWeaFiles,
                    GenDayMtxHelper.gendaymtx_arg_diffuse,
                    simulation_monthRange);

        List<List<double>> monthlyDirectRadiationList = new List<List<double>>();
        List<List<double>> monthlyDiffuseRadiationList = new List<List<double>>();
        List<double> directRadiationList = new List<double>();
        List<double> diffuseRadiationList = new List<double>();
        foreach(string monthRGB in directRadiationRGBList)
        {
            directRadiationList = genDayMtxHelper.convertRgbRadiationList(
                monthRGB);
            monthlyDirectRadiationList.Add(directRadiationList);
        }
        foreach(string monthRGB in diffuseRadiationRGBList)
        {
            diffuseRadiationList = genDayMtxHelper.convertRgbRadiationList(
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
        SimulationHelper simulationHelper = new SimulationHelper();
        Mesh joinedMesh = simulationHelper.createJoinedMesh(geometryInput);
        List<Point3d> points = simulationHelper.getMeshJoinedPoints(joinedMesh);
        Mesh contextMesh = simulationHelper.meshMainGeometryWithContext(
            geometryInput, contextGeometryInput);

        //get tragenza dome vectors. to use for intersection later
        Mesh tragenzaDomeMesh = simulationHelper.getTragenzaDome();
        List<Vector3d> allVectors = simulationHelper.getAllVectors(
            tragenzaDomeMesh);

        IntersectionObject intersectionObject = simulationHelper.intersectMeshRays(
            contextMesh, points, allVectors, joinedMesh.FaceNormals);

        List<Mesh> finalMeshList = new List<Mesh>();
        List<Mesh> otherMeshList = new List<Mesh>();
        MeshHelper meshHelper = new MeshHelper();
        Mesh finalMesh = meshHelper.createFinalMesh(joinedMesh);

        //todo variable for previous bounding box if there is one
        double previousX = 0;

        //start of specific monthly calls
        int monthCount = 0;
        foreach(List<double> radiationList in totalRadiationListByMonth)
        {
            Mesh monthlyFinalMesh = finalMesh.DuplicateMesh();
            List<double> finalRadiationList =
                    simulationHelper.computeFinalRadiationList(
                        intersectionObject, radiationList);

            //create the mesh and color
            //todo put this inside getFaceColors
            double maxRadiation = finalRadiationList.Max();
            //double minRadiation = finalRadiationList.Min();
            //double diffRadiation = maxRadiation - minRadiation;

            List<Color> faceColors = meshHelper.getFaceColors(
                finalRadiationList, maxRadiation);
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

                //double xIncrement = (monthlyFinalMesh.GetBoundingBox(true).Max.X -
                //    monthlyFinalMesh.GetBoundingBox(true).Min.X + diagramSpacer) * monthCount;
            Point3d centerPtMesh = monthlyFinalMesh.GetBoundingBox(true).Center;
            Vector3d translateVector = Point3d.Subtract(
                diagramCenterpoint, centerPtMesh);
            translateVector.X = translateVector.X + xIncrement;
            monthlyFinalMesh.Translate(translateVector);

            //todo optimize
            List<CropDataObject> cropListResult = new List<CropDataObject>();
            DliHelper dliHelper = new DliHelper();
            double maxDli = dliHelper.getDliFromX(maxRadiation);
            foreach (CropDataObject crop in cropDataInput)
            {
                if (crop.getDli() < maxDli){ cropListResult.Add(crop); }
            }

            //todo add crops bar graph
            BarGraphHelper barGraphHelper = new BarGraphHelper();
            Mesh barGraphMesh = barGraphHelper.createBarGraph(
                monthlyFinalMesh, cropDataInput, legendData);

            //add background plane
            monthlyFinalMesh.Append(barGraphMesh);

            //add month name
            //todo change width calculation
            Mesh monthTitleMesh = meshHelper.getTitleTextMesh(Convert.ToString((Months)monthCount),
                monthlyFinalMesh);

            monthlyFinalMesh.Append(monthTitleMesh);
            finalMeshList.Add(monthlyFinalMesh);

            //add bg plane
            Mesh meshBase2dPlane = meshHelper.create2dBaseMesh(monthlyFinalMesh);
            otherMeshList.Add(meshBase2dPlane);

            previousX = meshBase2dPlane.GetBoundingBox(true).Max.X -
                    meshBase2dPlane.GetBoundingBox(true).Min.X;
            //finalMeshList.Add(meshBase2dPlane);

            monthCount++;
        }

        //todo output input parameters
        DA.SetDataList(0, totalRadiationListByMonth);

        DA.SetDataList(1, totalRadiationListByMonth);
        //todo fix out of crop recommended list
        DA.SetDataList(2, cropDataInput);
        DA.SetDataList(3, finalMeshList);
        DA.SetDataList(4, otherMeshList);
    }

    /// <summary>
    /// Provides an Icon for every component that will be visible in the User Interface.
    /// Icons need to be 24x24 pixels.
    /// </summary>
    protected override System.Drawing.Bitmap Icon
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
