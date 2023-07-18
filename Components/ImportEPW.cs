using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Helianthus
{
  public class ImportEPW : GH_Component
  {
    /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
    public ImportEPW()
      : base("ImportEPW",
            "Import EPW Data",
             "Import Radiation Data from EPW file",
             "Helianthus",
             "01 | Import")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
        pManager.AddTextParameter("EPW_File", "EPW File",
        "EPW File", GH_ParamAccess.item);
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
        pManager.AddGenericParameter("LocationData", "Location Data",
            "Location Data from EPW file", GH_ParamAccess.item);
        pManager.AddGenericParameter("RadiationDataList", "Radiation Data List",
            "List of RadiationObjects containing Radiation Values for direct and diffuse", GH_ParamAccess.list);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
    /// to store data in output parameters.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        String epwDataFile = null;

        if (!DA.GetData(0, ref epwDataFile)) { return; }
        if (epwDataFile == null) { return; }
        if (epwDataFile == "") { return; }

        //READ THE LOCATION DATA
        //1 LINE - RECORD DATA
        LocationDataObject locationDataObject = new LocationDataObject();
        string locationString = File.ReadAllLines(epwDataFile).First();
        locationDataObject = LocationDataObject.fromCSV(locationString);

        //SKIP THE NEXT 7 LINES OF THE HEADER
        //READ THE DATA - HAS 35 FIELDS. WE JUST NEED 2. DIR AND DIFFUSE
        List<RadiationDataObject> radiationDataObjectList = File.ReadAllLines(epwDataFile)
            .Skip(8)
            .Select(v => RadiationDataObject.fromCSV(v))
            .ToList();

        DA.SetData(0, locationDataObject);
        DA.SetDataList(1, radiationDataObjectList);
    }

    /// <summary>
    /// Provides an Icon for every component that will be visible in the User Interface.
    /// Icons need to be 24x24 pixels.
    /// </summary>
    protected override Bitmap Icon => Properties.Resources.importCropData_icon;

    public override GH_Exposure Exposure => GH_Exposure.quarternary;

    /// <summary>
    /// Each component must have a unique Guid to identify it. 
    /// It is vital this Guid doesn't change otherwise old ghx files 
    /// that use the old ID will partially fail during loading.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("7373f7c2-61bd-4d74-95b7-b24276291be6"); }
    }
  }
}