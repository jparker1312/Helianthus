using System;
using System.Collections.Generic;
using System.Drawing;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Helianthus
{
  public class EnergySavings : GH_Component
  {
    /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
    public EnergySavings()
      : base("Energy_Savings",
             "Energy Savings",
             "Indicates the energy savings on a yearly basis achieved by " +
             "the implementation of a hybrid lighting system that " +
             "prioritizes sunlight over artificial light according to a " +
             "specified crop specie and sunlight analysis.",
             "Helianthus",
             "03 | Visualize Data")
    {
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
            "Selected_Crop",
            "Selected Crop",
            "Selected Crop to analyze.",
            GH_ParamAccess.item);
        pManager.AddPointParameter(
            "Visualization_Centerpoint",
            "Visualization Centerpoint",
            "Point3d to be used as the starting point to generate the " +
            "simulation visualization. If unspecified, defaults to " +
            "Point3d(0,0,0).",
            GH_ParamAccess.item,
            new Point3d(0, 0, 0));
            pManager[3].Optional = true;
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
            "A bar graph displaying the energy savings obtained through a " +
            "hybrid lighting system for a specified crop along a year.",
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
        string selectedCrop = "";
        Point3d diagramCenterpoint = new Point3d();
        bool run_Simulation = false;

        if (!DA.GetDataList(0, tiledMeshObjects)) { return; }
        if (!DA.GetDataList(1, cropData)) { return; }
        if (!DA.GetData(2, ref selectedCrop)) { return; }
        DA.GetData(3, ref diagramCenterpoint);
        if (!DA.GetData(4, ref run_Simulation)) { return; }

        if (!run_Simulation) { return; }
        if (selectedCrop == "") { return; }

        List<double> avgRadMonthList = new List<double>();
        foreach (TiledMeshObject tiledMeshObject in tiledMeshObjects)
        {
            int radCount = 0;
            double totalRadiation = 0;
            foreach (double rad in tiledMeshObject.getRadiationList())
            {
                totalRadiation += rad;
                radCount++;
            }
            avgRadMonthList.Add(totalRadiation / Convert.ToDouble(radCount));
        }

        CropDataObject selectedCropObject = null;
        foreach (CropDataObject cropDataObject in cropData)
        {
            if (cropDataObject.getSpecie().Equals(selectedCrop))
            {
                selectedCropObject = cropDataObject;
                break;
            }
        }

        if (selectedCropObject == null)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                "Input is not a valid crop: " + selectedCrop);
        }

        BarGraphHelper barGraphHelper = new BarGraphHelper();
        Mesh barGraphMesh = barGraphHelper.createBarGraphMonth(
                tiledMeshObjects[0].getBarGraphMesh().getBarGraphMesh(),
                diagramCenterpoint, cropData, avgRadMonthList,
                selectedCropObject.getDli(), selectedCrop,
                Convert.ToString(Config.BarGraphType.ENERGY));

        //add bg plane
        MeshHelper meshHelper = new MeshHelper();
        Mesh meshBase2dPlane = meshHelper.create2dBaseMesh(barGraphMesh);
        barGraphMesh.Append(meshBase2dPlane);

        List<Mesh> finalMeshList = new List<Mesh>
    {
        barGraphMesh
    };

        DA.SetDataList(0, new List<string>());
        DA.SetDataList(1, finalMeshList);
    }

    /// <summary>
    /// Provides an Icon for every component that will be visible in the User Interface.
    /// Icons need to be 24x24 pixels.
    /// </summary>
    protected override Bitmap Icon => Properties.Resources.energySavings_icon;

    public override GH_Exposure Exposure => GH_Exposure.secondary;

    /// <summary>
    /// Each component must have a unique Guid to identify it. 
    /// It is vital this Guid doesn't change otherwise old ghx files 
    /// that use the old ID will partially fail during loading.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("1a25779b-4fbd-44da-be63-c0a71588f093"); }
    }
  }
}
