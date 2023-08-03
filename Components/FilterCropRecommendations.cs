using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Helianthus.Components
{
  public class FilterCropRecommendations : GH_Component
  {
    /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
    public FilterCropRecommendations()
      : base("Filter_Crop_Recommendations",
             "Filter Crop Recommendations",
             "Filter Crop Recommendations",
             "Helianthus",
             "02 | Analyze Data")
        {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
        pManager.AddTextParameter("Crop_Recommendations",
            "Crop_Recommendations",
            "Crop_Recommendations", GH_ParamAccess.tree);
        pManager.AddTextParameter("January_Crops",
            "January Crops",
            "January Crops", GH_ParamAccess.list);
        pManager.AddTextParameter("February_Crops",
            "February Crops",
            "February Crops", GH_ParamAccess.list);
        pManager.AddTextParameter("March_Crops",
            "March Crops",
            "March Crops", GH_ParamAccess.list);
        pManager.AddTextParameter("April_Crops",
            "April Crops",
            "April Crops", GH_ParamAccess.list);
        pManager.AddTextParameter("May_Crops",
            "May Crops",
            "May Crops", GH_ParamAccess.list);
        pManager.AddTextParameter("June_Crops",
            "June Crops",
            "June Crops", GH_ParamAccess.list);
        pManager.AddTextParameter("July_Crops",
            "July Crops",
            "July Crops", GH_ParamAccess.list);
        pManager.AddTextParameter("August_Crops",
            "August Crops",
            "August Crops", GH_ParamAccess.list);
        pManager.AddTextParameter("September_Crops",
            "September Crops",
            "September Crops", GH_ParamAccess.list);
        pManager.AddTextParameter("October_Crops",
            "October Crops",
            "October Crops", GH_ParamAccess.list);
        pManager.AddTextParameter("November_Crops",
            "November Crops",
            "November Crops", GH_ParamAccess.list);
        pManager.AddTextParameter("December_Crops",
            "December Crops",
            "December Crops", GH_ParamAccess.list);
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Out", "Out", "Input Parameters",
            GH_ParamAccess.list);
        pManager.AddTextParameter("Crop_Selection",
            "Crop Selection", "Crop Selection", GH_ParamAccess.list);
        pManager.AddTextParameter("Warnings", "Warnings", "Warnings",
            GH_ParamAccess.list);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
    /// to store data in output parameters.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {

        Grasshopper.Kernel.Data.GH_Structure<GH_String> cropDataInput = new Grasshopper.Kernel.Data.GH_Structure<GH_String>();
        //DataTree<StringTest> cropDataInput = new DataTree<StringTest>();
        //GH_StructurePath<StringTest> cropDataInput = new DataTree<StringTest>();
        List<string> janCrops = new List<string>();
        List<string> febCrops = new List<string>();
        List<string> marCrops = new List<string>();
        List<string> aprCrops = new List<string>();
        List<string> mayCrops = new List<string>();
        List<string> junCrops = new List<string>();
        List<string> julCrops = new List<string>();
        List<string> augCrops = new List<string>();
        List<string> sepCrops = new List<string>();
        List<string> octCrops = new List<string>();
        List<string> novCrops = new List<string>();
        List<string> decCrops = new List<string>();

        if (!DA.GetDataTree (0, out cropDataInput)) { return; }
        if (!DA.GetDataList(1, janCrops)) { return; }
        if (!DA.GetDataList(2, febCrops)) { return; }
        if (!DA.GetDataList(3, marCrops)) { return; }
        if (!DA.GetDataList(4, aprCrops)) { return; }
        if (!DA.GetDataList(5, mayCrops)) { return; }
        if (!DA.GetDataList(6, junCrops)) { return; }
        if (!DA.GetDataList(7, julCrops)) { return; }
        if (!DA.GetDataList(8, augCrops)) { return; }
        if (!DA.GetDataList(9, sepCrops)) { return; }
        if (!DA.GetDataList(10, octCrops)) { return; }
        if (!DA.GetDataList(11, novCrops)) { return; }
        if (!DA.GetDataList(12, decCrops)) { return; }


        List<List<string>> userInputMonthlyCrops = new List<List<string>>
        {
            janCrops,
            febCrops,
            marCrops,
            aprCrops,
            marCrops,
            junCrops,
            julCrops,
            augCrops,
            sepCrops,
            octCrops,
            novCrops,
            decCrops
        };

        DataTree<string> finalCropList = new DataTree<string>();
        List<string> warnings = new List<string>();
        int monthCount = 0;
        foreach(List<string> monthlyUserListCrops in userInputMonthlyCrops)
        {
            foreach (string cropName in monthlyUserListCrops)
            {
                Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(monthCount + 1);
                List<GH_String> recMonthCropList = cropDataInput.Branches[monthCount];
                bool found = false;
                foreach (GH_String cropRecName in recMonthCropList)
                {
                    if (cropRecName.ToString().Equals(cropName))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    warnings.Add("month: " + (monthCount + 1) + " unrecognized or unrecommended name: " + cropName);
                }
                finalCropList.Add(cropName, path);
            }
            monthCount++;
        }

        DA.SetDataList(0, new List<string>());
        DA.SetDataTree(1, finalCropList);
        DA.SetDataList(2, warnings);

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
      get { return new Guid("cb040424-06da-4530-afc6-80a600cf8c4c"); }
    }
  }
}
