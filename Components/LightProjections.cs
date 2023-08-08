using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Helianthus.Components
{
  public class LightProjections : GH_Component
  {
    /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
    public LightProjections()
      : base("Light_Projections",
             "Light Projections",
             "Light Projections",
             "Helianthus",
             "03 | Visualize Data")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
        pManager.AddGenericParameter(
            "Tiled_Mesh_Obect",
            "Tiled Mesh Obect",
            "Tiled Mesh Obect",
            GH_ParamAccess.list);
        pManager.AddGenericParameter("CropsToVisualize", "Crops To Visualize",
            "List of Crops that you want to visualize", GH_ParamAccess.list);
        pManager.AddTextParameter(
            "Selected_Crop",
            "Selected Crop",
            "Selected Crop",
            GH_ParamAccess.item);
        pManager.AddPointParameter("Diagram_Centerpoint", "Diagram_Centerpoint",
            "Centerpoint for diagran", GH_ParamAccess.item);
        pManager[3].Optional = true;
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
        foreach(TiledMeshObject tiledMeshObject in tiledMeshObjects)
        {
            int radCount = 0;
            double totalRadiation = 0;
            foreach(double rad in tiledMeshObject.getRadiationList())
            {
                totalRadiation += rad;
                radCount++;
            }
            avgRadMonthList.Add(totalRadiation / Convert.ToDouble(radCount));
        }

            CropDataObject selectedCropObject = null;
        foreach(CropDataObject cropDataObject in cropData)
        {
            if (cropDataObject.getSpecie().Equals(selectedCrop))
            {
                selectedCropObject = cropDataObject;
                break;
            }
        }

        if(selectedCropObject == null)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                "Input is not a valid crop: " + selectedCrop);
        }

        BarGraphHelper barGraphHelper = new BarGraphHelper();
        Mesh barGraphMesh = barGraphHelper.createBarGraphMonth(
                tiledMeshObjects[0].getBarGraphMesh().getBarGraphMesh(),
                diagramCenterpoint, cropData, avgRadMonthList,
                selectedCropObject.getDli(), selectedCrop,
                Convert.ToString(Config.BarGraphType.A_LIGHT));

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
      get { return new Guid("c46085b3-876e-4541-b4f9-642918414207"); }
    }
  }
}
