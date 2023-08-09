using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;

using Grasshopper.Kernel;
using System.Linq;

namespace Helianthus
{
  public class CropsList : GH_Component
  {
    /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
    public CropsList()
      : base("Import_Crop_Data",
             "Import Crop Data",
             "Creates a list of crop data from a CSV file saved in your local " +
             "file system.",
             "Helianthus",
             "01 | Import Data")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddTextParameter(
            "Crop_Data_CSV",
            "Crop Data CSV",
            "The crop data csv can be downloaded from _____. " +
            "The .csv contains each crop id, type, specie, scientific name, " +
            "dli, dli classification, yearly yields, and monthly yields.",
            GH_ParamAccess.item);
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddGenericParameter(
            "Crop_Data",
            "Crop Data",
            "Contains a list of crop data to analyze and support simulations.",
            GH_ParamAccess.list);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
    /// to store data in output parameters.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        string cropDataFile = null;
        if (!DA.GetData(0, ref cropDataFile)) { return; }
        if (cropDataFile == null) { return; }
        if (cropDataFile == "") { return; }

        List<CropDataObject> cropList = File.ReadAllLines(cropDataFile)
            .Skip(1)
            .Select(v => CropDataObject.fromCSV(v))
            .ToList();

        DA.SetDataList(0, cropList);
    }

    ///// <summary>
    ///// Provides an Icon for every component that will be visible in the User Interface.
    ///// Icons need to be 24x24 pixels.
    ///// </summary>
    protected override Bitmap Icon => Properties.Resources.importCropData_icon;

    /// <summary>
    /// Each component must have a unique Guid to identify it. 
    /// It is vital this Guid doesn't change otherwise old ghx files 
    /// that use the old ID will partially fail during loading.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("e55845e1-52a7-4eec-b373-1c2fcfece9f7"); }
    }
  }
}
