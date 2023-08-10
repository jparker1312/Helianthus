using System;
using System.Collections.Generic;
using System.Drawing;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Helianthus.Components
{
  public class MonthlyYields : GH_Component
  {
    private BarGraphHelper barGraphHelper;
    private MeshHelper meshHelper;

    /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
    public MonthlyYields()
      : base("Monthly_Yields",
             "Monthly Yields",
             "Prospective yield values. These projections take in " +
             "consideration a production system in a Controlled Environment " +
             "with optimized vertical agriculture system that enhances the " +
             "maximum yield.",
             "Helianthus",
             "03 | Visualize Data")
    {
        barGraphHelper = new BarGraphHelper();
        meshHelper = new MeshHelper();
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddGenericParameter(
            "Monthly_Analysis",
            "Monthly Analysis",
            "List of monthly data containing the displayed meshes and " +
            "simulated data.",
            GH_ParamAccess.list);
        pManager.AddGenericParameter(
            "Crop_Data",
            "Crop Data",
            "Imported crop data to contrast with surface specifications, " +
            "obtained from the Import_Crop_Data component. ",
            GH_ParamAccess.list);
        pManager.AddTextParameter(
            "Crop_Selection",
            "Crop Selection",
            "Contains monthly list of selected crops.",
            GH_ParamAccess.tree);
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
        pManager.AddMeshParameter(
            "Mesh",
            "Mesh",
            "A monthly list of a bar graph displaying yield performance of " +
            "selected crops.",
            GH_ParamAccess.list);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
    /// to store data in output parameters.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        List<TiledMeshObject> tiledMeshObjects = new List<TiledMeshObject>();
        List<CropDataObject> cropData = new List<CropDataObject>();
        Grasshopper.Kernel.Data.GH_Structure<GH_String> cropDataInput =
            new Grasshopper.Kernel.Data.GH_Structure<GH_String>();
        bool run_Simulation = false;

        if (!DA.GetDataList(0, tiledMeshObjects)) { return; }
        if (!DA.GetDataList(1, cropData)) { return; }
        if (!DA.GetDataTree(2, out cropDataInput)) { return; }
        if (!DA.GetData(3, ref run_Simulation)) { return; }

        if (!run_Simulation){ return; }

        //create bar graph of yields highlighting the selected crops for each
        //month
        List<List<string>> selectedCropsByMonth = new List<List<string>>();
        for(int monthCount =0; monthCount < cropDataInput.Branches.Count;
                monthCount++)
        {
            List<string> cropNames = new List<string>();
            foreach (GH_String cropName in cropDataInput.Branches[monthCount])
            {
                cropNames.Add(cropName.ToString());
            }
            selectedCropsByMonth.Add(cropNames);
        }

        int maxOverallYield = 0;
        foreach (CropDataObject cropDataObject in cropData)
        {
            if (cropDataObject.getMonthlyCropYield() > maxOverallYield)
            {
                maxOverallYield = Convert.ToInt32(cropDataObject.
                    getMonthlyCropYield());
            }
        }

        List<Mesh> finalMeshList = new List<Mesh>();
        int monthlyCount = 0;
        foreach(TiledMeshObject tiledMeshObject in tiledMeshObjects)
        {
            Mesh barGraphMesh = barGraphHelper.createBarGraph2(
                tiledMeshObject.getBarGraphMesh().getBarGraphMesh(),
                cropData, 0, maxOverallYield,
                selectedCropsByMonth[monthlyCount],
                Convert.ToString(Config.BarGraphType.YIELD));

            Mesh yieldTitleText = meshHelper.getTitleTextMesh(
                "Yield Projections (kg?)", barGraphMesh, 1, 2, true);
                barGraphMesh.Append(yieldTitleText);

            Mesh appendedMesh = tiledMeshObject.appendAllMeshes();
            appendedMesh.Append(barGraphMesh);

            //add bg plane
            Mesh meshBase2dPlane = meshHelper.create2dBaseMesh(appendedMesh);
            appendedMesh.Append(meshBase2dPlane);

            finalMeshList.Add(appendedMesh);
            monthlyCount++;
        }

        DA.SetDataList(0, cropDataInput);
        DA.SetDataList(1, finalMeshList);
    }

    /// <summary>
    /// Provides an Icon for every component that will be visible in the User Interface.
    /// Icons need to be 24x24 pixels.
    /// </summary>
    protected override Bitmap Icon => Properties.Resources.monthlyYields_icon;

    public override GH_Exposure Exposure => GH_Exposure.secondary;

    /// <summary>
    /// Each component must have a unique Guid to identify it. 
    /// It is vital this Guid doesn't change otherwise old ghx files 
    /// that use the old ID will partially fail during loading.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("8a71e9f4-68e3-48b8-abee-36bff7840411"); }
    }
  }
}
