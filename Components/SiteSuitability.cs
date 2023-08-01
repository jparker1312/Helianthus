using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Helianthus
{
  public class SiteSuitability : GH_Component
  {
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
        pManager.AddNumberParameter("Radiation_List", "Radiation List",
            "Radiation List", GH_ParamAccess.list);
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

        List<double> directRadiationList;
        List<double> diffuseRadiationList;
        directRadiationList = genDayMtxHelper.convertRgbRadiationList(
            directRadiationRGB);
        diffuseRadiationList = genDayMtxHelper.convertRgbRadiationList(
            diffuseRadiationRGB);

        SimulationHelper simulationHelper = new SimulationHelper();
        List<double> totalRadiationList = simulationHelper.getTotalRadiationList(
            directRadiationList, diffuseRadiationList);

        //create gridded mesh from geometry
        MeshHelper meshHelper = new MeshHelper();
        Mesh meshJoined = meshHelper.createGriddedMesh(geometryInput, gridSize);

        //add offset distance for all points representing the faces of the
        //gridded mesh
        List<Point3d> points = meshHelper.getPointsOfMesh(meshJoined);
        
        // mesh together the geometry and the context
        Mesh contextMesh = meshHelper.getContextMesh(
            geometryInput, contextGeometryInput);

        //get tragenza dome vectors. to use for intersection later
        Mesh tragenzaDomeMesh = simulationHelper.getTragenzaDome();
        List<Vector3d> allVectors = simulationHelper.getAllVectors(
            tragenzaDomeMesh);

        //intersect mesh rays
        IntersectionObject intersectionObject = simulationHelper.
            intersectMeshRays(contextMesh, points, allVectors,
                meshJoined.FaceNormals);
        
        //compute the results
        List<double> finalRadiationList =
            simulationHelper.computeFinalRadiationList(intersectionObject,
                totalRadiationList);

        //create the mesh and color
        double maxRadiation = finalRadiationList.Max();
        double minRadiation = finalRadiationList.Min();

        List<Color> faceColors = meshHelper.getFaceColors(finalRadiationList,
            maxRadiation);
        Mesh finalMesh = meshHelper.createFinalMesh(meshJoined);
        meshHelper.colorFinalMesh(finalMesh, faceColors);

        //Create Legend
        LegendHelper legendHelper = new LegendHelper();
        Mesh baseBarGraphMesh = legendHelper.createLegend(meshJoined, true);

        //Add legend descriptors
        Mesh legendDescriptorMin = legendHelper.addLegendDescriptor(
            Convert.ToString(Convert.ToInt32(minRadiation)) + " DLI",
            baseBarGraphMesh.GetBoundingBox(true).Max.X + 1,
            baseBarGraphMesh.GetBoundingBox(true).Min.Y, 1);
        Mesh legendDescriptorMax = legendHelper.addLegendDescriptor(
            Convert.ToString(Convert.ToInt32(maxRadiation)) + " DLI",
            baseBarGraphMesh.GetBoundingBox(true).Max.X + 1,
            baseBarGraphMesh.GetBoundingBox(true).Max.Y, 1);

        List<Mesh> finalMeshList = new List<Mesh>
        {
            finalMesh,
            baseBarGraphMesh,
            legendDescriptorMin,
            legendDescriptorMax
        };

        DA.SetDataList(0, finalRadiationList);
        DA.SetDataList(1, finalRadiationList);
        DA.SetDataList(2, finalMeshList);
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
