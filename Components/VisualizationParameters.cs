using System;
using System.Collections.Generic;
using System.Drawing;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Helianthus
{
  public class VisualizationParameters : GH_Component
  {
    /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
    public VisualizationParameters()
      : base("Visualization_Paramters",
             "Visualization Paramters",
             "Visualization parameters for legends and diagrams.",
             "Helianthus",
             "03 | Visualize Data")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddPointParameter(
            "Visualization_Centerpoint",
            "Visualization Centerpoint",
            "Point3d to be used as the starting point to generate the " +
            "simulation visualization. If unspecified, defaults to " +
            "Point3d(0,0,0).",
            GH_ParamAccess.item,
            new Point3d(0,0,0));
        pManager[0].Optional = true;
        pManager.AddNumberParameter(
            "Visualization_Scale",
            "Visualization Scale",
            "Scales the entire visualization to a factor. If unspecified, " +
            "defaults to 1.",
            GH_ParamAccess.item,
            1.0);
        pManager[1].Optional = true;
        pManager.AddIntegerParameter(
            "Visualization_Background_Transparency",
            "Visualization Background Transparency",
            "Determines transparency of the background for the " +
            "visualization. Specifying 0 will give a completely transparent " +
            "background while 255 will be completely white. If unspecified, " +
            "defaults to 50.",
            GH_ParamAccess.item,
            50);
        pManager[2].Optional = true;
        pManager.AddVectorParameter(
            "Analysis_Geometry_Rotation_Axis",
            "Analysis Geometry Rotation Axis",
            "Vector3d given as an axis to set the orientation of analysis " +
            "geometry to top-view. This parameter works in conjunction with " +
            "the Analysis_Geometry_Rotation input. If unspecified, defaults " +
            "to Vector3d(0,0,1).",
            GH_ParamAccess.item,
            new Vector3d(0,0,1));
        pManager[3].Optional = true;
        pManager.AddNumberParameter(
            "Analysis_Geometry_Rotation",
            "Analysis Geometry Rotation",
            "A number given in degrees that determines the rotation of the " +
            "analysis geopmtery on the established axis. If unspecified, " +
            "defaults to 0.",
            GH_ParamAccess.item,
            0);
        pManager[4].Optional = true;
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
            GH_ParamAccess.item);
        pManager.AddGenericParameter(
            "Visualizaion_Parameters",
            "Visualizaion Parameters",
            "Outputs visualization parameters for legends and diagrams.",
            GH_ParamAccess.item);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object can be used to retrieve data from input
    /// parameters and 
    /// to store data in output parameters.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        Point3d visualizationCenterpoint = new Point3d(0,0,0);
        double visualizationScale = 1;
        int visualizationBgTransparency = 50;
        Vector3d aGeometryRotationAxis = new Vector3d(0,0,1);
        double aGeometryRotation = 0;

        DA.GetData(0, ref visualizationCenterpoint);
        DA.GetData(1, ref visualizationScale);
        DA.GetData(2, ref visualizationBgTransparency);
        DA.GetData(3, ref aGeometryRotationAxis);
        DA.GetData(4, ref aGeometryRotation);

        //check if input parameter scale is <= 0
        if (visualizationScale <= 0)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                "Scale needs to be greater than 0");
            return;
        }
        //check input parameter transparency
        if (visualizationBgTransparency < 0 ||
                visualizationBgTransparency > 255) {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                "Transparency should be between 0 and 255");
            return;
        }

        List<string> listOfParameters = new List<string>();
        string paramData = "Visualization Scale: " +
                visualizationScale.ToString();
        listOfParameters.Add(paramData);
        paramData = "Visualization Background Transparency: " +
                visualizationBgTransparency.ToString();
        listOfParameters.Add(paramData);

        VisualizationDataObject visualizationParams =
                new VisualizationDataObject();
        visualizationParams.setDiagramCenterpoint(visualizationCenterpoint);
        visualizationParams.setDiagramRotationAxis(aGeometryRotationAxis);
        visualizationParams.setDiagramRotation(aGeometryRotation);
        visualizationParams.setGraphScale(visualizationScale);
        visualizationParams.setGraphBackgroundTransparency(
            visualizationBgTransparency);

        DA.SetData(0, string.Join(", ", listOfParameters));
        DA.SetData(1, visualizationParams);
    }

    /// <summary>
    /// Provides an Icon for every component that will be visible in the User
    /// Interface.
    /// Icons need to be 24x24 pixels.
    /// </summary>
    protected override Bitmap Icon =>
            Properties.Resources.visualizationParameters_icon;

    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <summary>
    /// Each component must have a unique Guid to identify it. 
    /// It is vital this Guid doesn't change otherwise old ghx files 
    /// that use the old ID will partially fail during loading.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("32968359-25ce-4aee-9540-3b1b81e370df"); }
    }
  }
}
