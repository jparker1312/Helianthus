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
    private MeshHelper meshHelper;
    private SimulationHelper simulationHelper;
    private GenDayMtxHelper genDayMtxHelper;

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
        meshHelper = new MeshHelper();
        simulationHelper = new SimulationHelper();
        genDayMtxHelper = new GenDayMtxHelper();
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddGenericParameter("WEA_File", "WEA File",
        "File location for .wea file", GH_ParamAccess.item);
        pManager.AddGeometryParameter("SurfaceGeometry", "Surface Geometry",
            "Rhino Surfaces or Rhino Meshes", GH_ParamAccess.list);
        pManager.AddGeometryParameter("ContextGeometry", "Context Geometry",
            "Rhino Surfaces or Rhino Meshes", GH_ParamAccess.list);
        pManager[2].Optional = true;
        pManager.AddNumberParameter("GridSize", "Grid Size",
            "Grid Size for output geometry", GH_ParamAccess.item, 1.0);
        pManager[3].Optional = true;
        pManager.AddBooleanParameter("Run_Simulation", "Run Simulation",
            "Run Simulation", GH_ParamAccess.item);
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
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
        DA.GetDataList(2, contextGeometryInput);
        DA.GetData(3, ref gridSize);
        if (!DA.GetData(4, ref run_Simulation)) { return; }
        if (!run_Simulation){ return; }

        List<double> genDayMtxTotalRadiationList = genDayMtxHelper.
            getGenDayMtxTotalRadiation(weaFileLocation);

        //create gridded mesh from geometry
        Mesh joinedMesh = meshHelper.createGriddedMesh(geometryInput, gridSize);

        List<double> finalRadiationList = simulationHelper.getSimulationRadiationList(joinedMesh,
            geometryInput, contextGeometryInput, genDayMtxTotalRadiationList);
        double maxRadiation = finalRadiationList.Max();
        double minRadiation = finalRadiationList.Min();

        //create the mesh and color
        Mesh finalMesh = meshHelper.createFinalMesh(joinedMesh);
        List<Color> faceColors = meshHelper.getFaceColors(finalRadiationList,
            maxRadiation);
        meshHelper.colorFinalMesh(finalMesh, faceColors);

        //Create Legend
        Mesh legendMesh = createSiteSuitabilityLegend(finalMesh, minRadiation,
            maxRadiation);

        List<Mesh> finalMeshList = new List<Mesh>
        {
            finalMesh,
            legendMesh
        };

        List<string> inputParams = new List<string>
        {
            weaFileLocation,
            geometryInput.ToString(),
            contextGeometryInput.ToString(),
            gridSize.ToString()
        };
        
        DA.SetDataList(0, inputParams);
        DA.SetDataList(1, finalRadiationList);
        DA.SetDataList(2, finalMeshList);
    }

    private Mesh createSiteSuitabilityLegend(Mesh mesh, double minRadiation,
        double maxRadiation)
    {
        //Create Legend
        LegendHelper legendHelper = new LegendHelper();
        Mesh legendMesh = legendHelper.createLegend(mesh, true);

        //Add legend descriptors
        Mesh legendDescriptorMin = legendHelper.addLegendDescriptor(
            Convert.ToString(Convert.ToInt32(minRadiation)) + " DLI",
            legendMesh.GetBoundingBox(true).Max.X + 1,
            legendMesh.GetBoundingBox(true).Min.Y, 1);
        Mesh legendDescriptorMax = legendHelper.addLegendDescriptor(
            Convert.ToString(Convert.ToInt32(maxRadiation)) + " DLI",
            legendMesh.GetBoundingBox(true).Max.X + 1,
            legendMesh.GetBoundingBox(true).Max.Y, 1);
        legendMesh.Append(legendDescriptorMin);
        legendMesh.Append(legendDescriptorMax);

        return legendMesh;  
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
