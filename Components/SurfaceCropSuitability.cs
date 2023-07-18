using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Helianthus.Components
{
  public class SurfaceCropSuitability : GH_Component
  {
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
        //todo add monthly value to determine the months to run
        //todo add a path to place monthly wea files
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
        Brep geometryInput = new Brep();
        List<Brep> contextGeometryInput = new List<Brep>();

        if (!DA.GetData(0, ref weaFileLocation)) { return; }
        if (!DA.GetData(1, ref geometryInput)) { return; }
        //todo optional???
        if (!DA.GetDataList(2, contextGeometryInput)) { return; }

        WeaDataObject weaDataObject = new WeaDataObject(weaFileLocation);

        //create monthly wea files
        //todo check if successful
        string pathname = "/Users/joel/Projects/Programming/AlbaThesis/Helianthus/" +
            "HelianthusData/monthlyFiles/";
        weaDataObject.writeWeaDataToMonthlyFiles(pathname);

        //run monthly simulations
        GenDayMtxHelper genDayMtxHelper = new GenDayMtxHelper();
        List<string> directRadiationRGBList =
                genDayMtxHelper.runMonthlyGenDayMtxSimulations(
                    pathname, GenDayMtxHelper.gendaymtx_arg_direct);
        List<string> diffuseRadiationRGBList =
                genDayMtxHelper.runMonthlyGenDayMtxSimulations(
                    pathname, GenDayMtxHelper.gendaymtx_arg_diffuse);

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
            //List<double> monthDirectValues = monthlyDirectRadiationList[i];
            //List<double> monthDiffuseValues = monthlyDiffuseRadiationList[i];
            List<Double> totalRadiationValues = new List<double>();
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
        foreach(List<Double> totalRadiationList in totalRadiationListByMonth)
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
        MeshHelper meshHelper = new MeshHelper();
        Mesh finalMesh = meshHelper.createFinalMesh(joinedMesh);
        //start of specific monthly calls
        int monthCount = 0;
        foreach(List<double> radiationList in totalRadiationListByMonth)
        {
            Mesh monthlyFinalMesh = finalMesh.DuplicateMesh();
            List<double> finalRadiationList =
                    simulationHelper.computeFinalRadiationList(
                        intersectionObject, radiationList);

            //create the mesh and color
            double maxRadiation = finalRadiationList.Max();
            //double minRadiation = finalRadiationList.Min();
            //double diffRadiation = maxRadiation - minRadiation;

            List<Color> faceColors = meshHelper.getFaceColors(
                finalRadiationList, maxRadiation);

            monthlyFinalMesh = meshHelper.colorFinalMesh(monthlyFinalMesh, faceColors);

            //todo add xy movement for monthly chart
            //monthlyFinalMesh.Translate(
            //    (monthlyFinalMesh.GetBoundingBox(true).Max.X -
            //    monthlyFinalMesh.GetBoundingBox(true).Min.X) * monthCount,
            //    0, 0);

            double xIncrement = (monthlyFinalMesh.GetBoundingBox(true).Max.X -
                monthlyFinalMesh.GetBoundingBox(true).Min.X) * monthCount;
            Point3d tempPt = new Point3d(500, 100, 1);
            Point3d centerPtMesh = monthlyFinalMesh.GetBoundingBox(true).Center;
            Vector3d translateVector = Point3d.Subtract(tempPt, centerPtMesh);
            translateVector.X = translateVector.X + xIncrement;
            monthlyFinalMesh.Translate(translateVector);

            finalMeshList.Add(monthlyFinalMesh);
            monthCount++;
        }

        DA.SetDataList(0, totalRadiationListByMonth);
        DA.SetDataList(1, finalMeshList);
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
