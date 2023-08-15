using System;
using System.Drawing;

using Grasshopper.Kernel;

namespace Helianthus
{
  public class SG_CorrectionFactor : GH_Component
  {
    /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
    public SG_CorrectionFactor()
      : base("SG_CorrectionFactor",
             "SG Correction Factor",
             "Factor as an input for the correction of a known systemic " +
             "inaccuracy on the simulated DLI calculations. The factor is " +
             "obtained from an empirical validation using environmental " +
             "sunlight sensing.",
             "Helianthus",
             "01 | Import Data")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddNumberParameter(
            "SG_Correction_Factor",
            "SG Correction Factor",
            "" +
            "study.",
            GH_ParamAccess.item);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
    /// to store data in output parameters.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        DA.SetData(0, 1.5);
    }

    /// <summary>
    /// Provides an Icon for every component that will be visible in the User Interface.
    /// Icons need to be 24x24 pixels.
    /// </summary>
    protected override Bitmap Icon => Properties.Resources.sgIndex_icon;

    public override GH_Exposure Exposure => GH_Exposure.secondary;

    /// <summary>
    /// Each component must have a unique Guid to identify it. 
    /// It is vital this Guid doesn't change otherwise old ghx files 
    /// that use the old ID will partially fail during loading.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("dfe72bff-c050-4612-a79e-ee65f35236a2"); }
    }
  }
}
