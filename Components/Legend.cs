using System;
using System.Collections.Generic;
using System.Drawing;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Helianthus
{
  public class Legend : GH_Component
  {
    /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
    public Legend()
      : base("LegendParameters",
             "Legend Parameters",
             "Legend Parameters for visualizations",
             "Helianthus",
             "03 | Visualize Data")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
        pManager.AddPointParameter("GraphOffset", "Graph Offset",
            "Set the offset position for the visualization", GH_ParamAccess.item,
            new Point3d(1,0,0));
        pManager.AddNumberParameter("GraphScale", "Graph Scale",
            "Scale the graph proportionally to input geometry",
            GH_ParamAccess.item, 1);
        pManager.AddNumberParameter("GraphBackgroundTransparency",
            "Graph Background Transparency", "The transparency of the " +
            "background for the visualization",GH_ParamAccess.item, 50);
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Out", "Out", "Input Parameters",
            GH_ParamAccess.item);
        pManager.AddGenericParameter("LegendParameters", "Legend Parameters",
            "Legend Parameters", GH_ParamAccess.item);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
    /// to store data in output parameters.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        Point3d graphOffset = Point3d.Unset;
        double graphScale = double.NaN;
        double graphBackgroundTransparency = double.NaN;

        if (!DA.GetData(0, ref graphOffset)) { return; }
        if (!DA.GetData(1, ref graphScale)) { return; }
        if (!DA.GetData(2, ref graphBackgroundTransparency)) { return; }

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

        List<String> listOfParameters = new List<string>();
        String paramData = "Graph Offset: " + graphOffset.ToString();
        listOfParameters.Add(paramData);
        paramData = "Graph Scale: " + graphScale.ToString();
        listOfParameters.Add(paramData);
        paramData = "Graph Background Transparency: " + graphBackgroundTransparency.ToString();
        listOfParameters.Add(paramData);

        LegendDataObject legendData = new LegendDataObject();
        legendData.setGraphOffset(graphOffset);
        legendData.setGraphScale(graphScale);
        legendData.setGraphBackgroundTransparency(graphBackgroundTransparency);

        DA.SetData(0, String.Join(", ", listOfParameters));
        DA.SetData(1, legendData);
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
