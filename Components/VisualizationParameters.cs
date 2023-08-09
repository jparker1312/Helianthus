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
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
        pManager.AddPointParameter(
            "Diagram_Centerpoint",
            "Diagram_Centerpoint",
            "Centerpoint for diagran",
            GH_ParamAccess.item);
        pManager[0].Optional = true;
        pManager.AddVectorParameter(
            "Diagram_Rotation_Axis",
            "Diagram Rotation Axis",
            "Diagram Rotation Axis - to help orient planes to top-view",
            GH_ParamAccess.item);
        pManager[1].Optional = true;
        pManager.AddNumberParameter(
            "Diagram_Rotation",
            "Diagram Rotation",
            "Diagram Rotation - to help orient planes to top-view",
            GH_ParamAccess.item);
        pManager[2].Optional = true;
        pManager.AddPointParameter(
            "GraphOffset",
            "Graph Offset",
            "Set the offset position for the visualization",
            GH_ParamAccess.item,
            new Point3d(0,0,0));
        pManager[3].Optional = true;
        pManager.AddNumberParameter(
            "GraphScale",
            "Graph Scale",
            "Scale the graph proportionally to input geometry",
            GH_ParamAccess.item,
            1);
        pManager[4].Optional = true;
        pManager.AddNumberParameter(
            "GraphBackgroundTransparency",
            "Graph Background Transparency",
            "The transparency of the background for the visualization",
            GH_ParamAccess.item,
            50);
        pManager[5].Optional = true;
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter(
            "Out",
            "Out",
            "Outputs the input parameters",
            GH_ParamAccess.item);
        pManager.AddGenericParameter(
            "LegendParameters",
            "Legend Parameters",
            "Legend Parameters",
            GH_ParamAccess.item);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
    /// to store data in output parameters.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        Point3d diagramCenterpoint = new Point3d();
        Vector3d diagramRotationAxis = new Vector3d();
        double diagramRotation = 0;
        Point3d graphOffset = Point3d.Unset;
        double graphScale = double.NaN;
        double graphBackgroundTransparency = double.NaN;

        DA.GetData(0, ref diagramCenterpoint);
        DA.GetData(1, ref diagramRotationAxis);
        DA.GetData(2, ref diagramRotation);
        DA.GetData(3, ref graphOffset);
        DA.GetData(4, ref graphScale);
        DA.GetData(5, ref graphBackgroundTransparency);

        //check if input parameter scale is <= 0
        if (graphScale <= 0)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                "Scale needs to be greater than 0");
            return;
        }
        //check input parameter transparency
        if (graphBackgroundTransparency < 0 || graphBackgroundTransparency > 255) {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                "Transparency should be between 0 and 255");
            return;
        }

        List<string> listOfParameters = new List<string>();
        string paramData = "Graph Offset: " + graphOffset.ToString();
        listOfParameters.Add(paramData);
        paramData = "Graph Scale: " + graphScale.ToString();
        listOfParameters.Add(paramData);
        paramData = "Graph Background Transparency: " + graphBackgroundTransparency.ToString();
        listOfParameters.Add(paramData);

        VisualizationDataObject visualizationParams = new VisualizationDataObject();
        visualizationParams.setDiagramCenterpoint(diagramCenterpoint);
        visualizationParams.setDiagramRotationAxis(diagramRotationAxis);
        visualizationParams.setDiagramRotation(diagramRotation);
        visualizationParams.setGraphOffset(graphOffset);
        visualizationParams.setGraphScale(graphScale);
        visualizationParams.setGraphBackgroundTransparency(graphBackgroundTransparency);

        DA.SetData(0, string.Join(", ", listOfParameters));
        DA.SetData(1, visualizationParams);
    }

    /// <summary>
    /// Provides an Icon for every component that will be visible in the User Interface.
    /// Icons need to be 24x24 pixels.
    /// </summary>
    protected override Bitmap Icon => Properties.Resources.legend_icon;

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
