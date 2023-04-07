using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;


namespace Helianthus
{
  public class DliSuitability : GH_Component
  {
    /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
    public DliSuitability()
      : base("DLI_Suitability",
             "DLI Suitability",
             "Filters crops based on DLI thresholds",
             "Helianthus",
             "02 | Analyze Data")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
        pManager.AddNumberParameter("SurfaceDLI", "Surface DLI",
            "Surface Sunlight DLI", GH_ParamAccess.item);
        pManager.AddGenericParameter("CropDataList", "Crop Data List",
            "List of crops from the Helianthus Database", GH_ParamAccess.list);
        pManager.AddNumberParameter("MaterialTransmittance",
            "Material Light Transmittance",
            "Effectiveness of a material in transimitting sunlight",
            GH_ParamAccess.item, .65);
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Out", "Out", "Input Parameters",
            GH_ParamAccess.item);
        pManager.AddGenericParameter("FilteredCropList", "Filtered Crop List",
            "Crops Meeting Sunlight Threshold", GH_ParamAccess.list);
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
        // Declare a variable for the input String
        double surfaceDli = double.NaN;
        double dliTransmittance = double.NaN;
        List<CropDataObject> cropDataInput = new List<CropDataObject>();

        // Use the DA object to retrieve the data inside the first input parameter.
        // If the retieval fails (for example if there is no data) we need to abort.
        if (!DA.GetData(0, ref surfaceDli)) { return; }
        if (!DA.GetDataList(1, cropDataInput)) { return; }
        if (!DA.GetData(2, ref dliTransmittance)) { return; }

        List<String> listOfParameters = new List<string>();
        String paramData = "Surface Sunlight DLI: " + surfaceDli.ToString();
        listOfParameters.Add(paramData);
        //paramData = "Crop List: " + cropDataInput.;
        listOfParameters.Add(paramData);
        paramData = "DLI Transmittance: " + dliTransmittance.ToString();
        listOfParameters.Add(paramData);

        List<CropDataObject> cropListResult = new List<CropDataObject>();
        foreach (CropDataObject crop in cropDataInput)
        {
            if (crop.getDli() < surfaceDli){ cropListResult.Add(crop); }
        }

        // Use the DA object to assign a new String to the first output parameter.
        DA.SetData(0, String.Join(", ", listOfParameters));
        DA.SetDataList(1, cropListResult);
        DA.SetData(2, surfaceDli);
    }

    /// <summary>
    /// Provides an Icon for every component that will be visible in the User Interface.
    /// Icons need to be 24x24 pixels.
    /// You can add image files to your project resources and access them like this:
    /// return Resources.IconForThisComponent;
    /// </summary>
    protected override Bitmap Icon => Properties.Resources.dliSuitability_icon;


    public override GH_Exposure Exposure => GH_Exposure.secondary;

    /// <summary>
    /// Each component must have a unique Guid to identify it. 
    /// It is vital this Guid doesn't change otherwise old ghx files 
    /// that use the old ID will partially fail during loading.
    /// </summary>
    public override Guid ComponentGuid => new Guid("12a2d7b7-dfc0-4f87-b4ab-5c853012d758");
  }
}
