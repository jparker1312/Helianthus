using System;
using System.IO;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
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
      : base("Crops List",
             "Crops",
             "Create List of Crops from CSV",
             "Helianthus",
             "Analyze")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
        pManager.AddTextParameter("Crop Info CSV", "CI", "CSV of Crop Data",
            GH_ParamAccess.item);
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
        pManager.AddGenericParameter("Crops List", "Crops",
            "List of crops from the Helianthus Database", GH_ParamAccess.list);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
    /// to store data in output parameters.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        String cropDataFile = null;

        if (!DA.GetData(0, ref cropDataFile)) { return; }
        if (cropDataFile == null) { return; }
        if (cropDataFile == "") { return; }

        List<CropDataObject> cropList = File.ReadAllLines(cropDataFile)
            .Skip(1)
            .Select(v => CropDataObject.fromCSV(v))
            .ToList();

        DA.SetDataList(0, cropList);
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
      get { return new Guid("e55845e1-52a7-4eec-b373-1c2fcfece9f7"); }
    }
  }
}
