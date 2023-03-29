using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Helianthus
{
  public class SunlightDli : GH_Component
  {
    //probably make this constant

    /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
    public SunlightDli()
      : base("SunlightDLI",
             "Sunlight DLI",
             "Converts solar radiation data into a Daily Light Integral (DLI) constant",
             "Helianthus",
             "02 | Analyze Data")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
        pManager.AddNumberParameter("SurfaceSolarRadiation", "Surface Solar Radiation",
            "Surface Solar Radiation as an arithmetic mean, unit is kWh/m2",
            GH_ParamAccess.item);
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Out", "Out", "Input Parameters",
            GH_ParamAccess.item);
        pManager.AddNumberParameter("SurfaceDLI", "Surface DLI",
            "Surface Sunlight DLI", GH_ParamAccess.item);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
    /// to store data in output parameters.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        double surfaceSunlightConstant = double.NaN;

        if (!DA.GetData(0, ref surfaceSunlightConstant)) { return; }

        // If the retrieved data is Nothing, we need to abort.
        if (surfaceSunlightConstant.Equals(null)) { return; }

        List<String> listOfParameters = new List<string>();

        String paramData = "Surface Sunlight Constant: " + surfaceSunlightConstant.ToString();
        listOfParameters.Add(paramData);

        //Convert Surface Sunlight constant units to DLI
        //Divide by days in a year
        double surfaceSunlightDLI = surfaceSunlightConstant / 365;
        //divide by the determined hours of sunlight. Should be the same as the input for the EPW duration
        surfaceSunlightDLI = surfaceSunlightDLI / 12;
        //multiply by 1000 to get the W/m2
        surfaceSunlightDLI = surfaceSunlightDLI * 1000;
        //divide by 2.02 to get the par
        surfaceSunlightDLI = surfaceSunlightDLI / 2.02;
        //multiply by .0864 to get the DLI
        surfaceSunlightDLI = surfaceSunlightDLI * 0.0864;
        surfaceSunlightDLI = Math.Round(surfaceSunlightDLI, 0);

        DA.SetData(0, String.Join(", ", listOfParameters));
        DA.SetData(1, surfaceSunlightDLI);
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

    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <summary>
    /// Each component must have a unique Guid to identify it. 
    /// It is vital this Guid doesn't change otherwise old ghx files 
    /// that use the old ID will partially fail during loading.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("0e76ae59-23d5-4fbc-b2f3-ab6c70dd7445"); }
    }
  }
}
