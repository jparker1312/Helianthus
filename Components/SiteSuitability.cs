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
             "Visualize Daily Light Integral (DLI) of photosynthetic " +
             "sunlight levels on surfaces to determine best locations for " +
             "crop placement. DLI is defined as the total amount of " +
             "photosynthetically active radiation that impacts a square " +
             "meter over 24 hrs",
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
        pManager.AddGenericParameter(
            "WEA_File",
            "WEA File",
            "File path for .wea file. WEA files contain Daysim weather" +
            " format with the sunlight climate data specifically to support " +
            "building simulations. As such, the files are Typical " +
            "Meteorological Years (TMY) published by a variety of " +
            "organizations. The repository of climate data files can be " +
            "found on climate.onebuilding.org",
            GH_ParamAccess.item);
        pManager.AddGeometryParameter(
            "Analysis_Geometry",
            "Analysis Geometry",
            "Rhino Surfaces or Rhino Meshes to analyze",
            GH_ParamAccess.list);
        pManager.AddGeometryParameter(
            "Context_Geometry",
            "Context Geometry",
            "Optional. Rhino Surfaces or Rhino Meshes that can block " +
            "sunlight from reaching the analysis geometry",
            GH_ParamAccess.list);
        pManager[2].Optional = true;
        pManager.AddNumberParameter(
            "Grid_Size",
            "Grid Size",
            "Optional. Grid Size for output geometry. Default grid size " +
            "corresponds to 1 square meter which is the unit used for " +
            "monthly calculations.",
            GH_ParamAccess.item,
            1.0);
        pManager[3].Optional = true;
        pManager.AddBooleanParameter(
            "Run_Simulation",
            "Run Simulation",
            "Set to 'true' to run the simulation",
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
            "DLI_List",
            "DLI List",
            "Daily Light Integral (DLI) List containing values for each tile"  +
            "on the list of gridded meshes",
            GH_ParamAccess.list);
        pManager.AddMeshParameter(
            "Mesh",
            "Mesh",
            "A colored mesh of the analysis geometry showing gridded DLI " +
            "values",
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
        List<Brep> analysisGeometryInput = new List<Brep>();
        List<Brep> contextGeometryInput = new List<Brep>();
        double gridSize = 1.0;
        bool run_Simulation = false;

        if (!DA.GetData(0, ref weaFileLocation)) { return; }
        if (!DA.GetDataList(1, analysisGeometryInput)) { return; }
        DA.GetDataList(2, contextGeometryInput);
        DA.GetData(3, ref gridSize);
        if (!DA.GetData(4, ref run_Simulation)) { return; }
        if (!run_Simulation){ return; }

        List<double> genDayMtxTotalRadiationList = genDayMtxHelper.
            getGenDayMtxTotalRadiation(weaFileLocation);

        //create gridded mesh from geometry
        Mesh joinedMesh = meshHelper.createGriddedMesh(analysisGeometryInput, gridSize);

        List<double> finalRadiationList = simulationHelper.
            getSimulationRadiationList(joinedMesh, analysisGeometryInput,
            contextGeometryInput, genDayMtxTotalRadiationList, 1);
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
            analysisGeometryInput.ToString(),
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
        legendMesh.Translate(new Vector3d(2, 0, 0));

        double area = legendMesh.GetBoundingBox(true).Area;
        double textSize = 1;
        for(int count = 1000; count < area; count += 1000){ textSize++; }

        double rectWidth = legendMesh.GetBoundingBox(true).Max.X -
                legendMesh.GetBoundingBox(true).Min.X + 3;
        
        //Add legend descriptors
        Mesh legendDescriptorMin = legendHelper.addLegendDescriptor(
            Convert.ToString(Convert.ToInt32(minRadiation)) + " DLI",
            legendMesh.GetBoundingBox(true).Max.X + 1,
            legendMesh.GetBoundingBox(true).Min.Y, textSize, rectWidth);
        Mesh legendDescriptorMax = legendHelper.addLegendDescriptor(
            Convert.ToString(Convert.ToInt32(maxRadiation)) + " DLI",
            legendMesh.GetBoundingBox(true).Max.X + 1,
            legendMesh.GetBoundingBox(true).Max.Y, textSize, rectWidth);
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
