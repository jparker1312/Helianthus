using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Helianthus.Components
{
  public class YieldProjections : GH_Component
  {
    /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
    public YieldProjections()
      : base("Yield_Projections",
             "YieldProjections",
             "Yield Projections",
             "Helianthus",
             "03 | Visualize Data")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
        pManager.AddMeshParameter(
            "Surface_Mesh_List",
            "Surface Mesh List",
            "Surface Mesh List",
            GH_ParamAccess.list);
        pManager.AddGenericParameter(
            "Crops_For_Yield_Projections",
            "Crops For Yield Projections",
            "List of Crops that you want to visualize",
            GH_ParamAccess.list);
        pManager.AddNumberParameter(
            "Radiation_List_By_Month",
            "Radiation List By Month",
            "Radiation List By Month",
            GH_ParamAccess.list);
    }


    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
    /// to store data in output parameters.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        //todo get params

        //todo cycle through tiled meshes and assign a crop to each face according to DLI

        //todo cycle through the new value list to calculate the total yield per month for each crop

        //todo some output tbd
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
      get { return new Guid("8a71e9f4-68e3-48b8-abee-36bff7840411"); }
    }
  }
}
