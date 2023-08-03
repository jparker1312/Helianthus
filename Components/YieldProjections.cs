using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
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
        pManager.AddGenericParameter(
            "Crop_Surface_Object",
            "Crop Surface Object",
            "Crop Surface Object",
            GH_ParamAccess.item);
        pManager.AddTextParameter(
            "Crop_Projections",
            "Crop Projections",
            "Crop Projections",
            GH_ParamAccess.tree);
        pManager.AddBooleanParameter("Run_Simulation", "Run Simulation",
            "Run Simulation", GH_ParamAccess.item);
        }


    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Out", "Out", "Input Parameters",
            GH_ParamAccess.list);
        pManager.AddMeshParameter("Mesh", "Mesh", "Mesh viz",
            GH_ParamAccess.list);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
    /// to store data in output parameters.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        //todo get params
        //List<Mesh> meshInput = new List<Mesh>();
        //CropSurfaceObject cropSurfaceObject = new CropSurfaceObject();

        CropSurfaceObject cropSurfaceObject = new CropSurfaceObject();
        Grasshopper.Kernel.Data.GH_Structure<GH_String> cropDataInput = new Grasshopper.Kernel.Data.GH_Structure<GH_String>();
        bool run_Simulation = false;

        //if (!DA.GetData(0, ref cropSurfaceObject)) { return; }
        if (!DA.GetData(0, ref cropSurfaceObject)) { return; }
        if (!DA.GetDataTree(1, out cropDataInput)) { return; }
        if (!DA.GetData(2, ref run_Simulation)) { return; }

        if (!run_Simulation){ return; }

        //todo cycle through tiled meshes and assign a crop to each face according to DLI: not needed currently
        //todo cycle through the new value list to calculate the total yield per month for each crop: not needed currently

        //todo create bar graph of yields highlighting the selected crops for each month

        List<List<string>> selectedCropsByMonth = new List<List<string>>();
        for(int monthCount =0; monthCount < cropDataInput.Branches.Count; monthCount++)
        {
            List<string> cropNames = new List<string>();
            foreach (GH_String cropName in cropDataInput.Branches[monthCount])
            {
                cropNames.Add(cropName.ToString());
            }
            selectedCropsByMonth.Add(cropNames);
        }


        //todo need prior meshes for alignment
        //todo need full crop list. can get this from same object
        //todo overall max radiation will be result of full crop list

        int maxOverallYield = 0;
        foreach (CropDataObject cropDataObject in cropSurfaceObject.getCropDataObjectList())
        {
            if(cropDataObject.getMonthlyCropYield() > maxOverallYield)
            {
                maxOverallYield = Convert.ToInt32(cropDataObject.getMonthlyCropYield());
            }
        }

        List<Mesh> finalMeshList = new List<Mesh>();
        BarGraphHelper barGraphHelper = new BarGraphHelper();
        MeshHelper meshHelper = new MeshHelper();
        int monthlyCount = 0;
        foreach(Mesh monthlyMesh in cropSurfaceObject.getTiledMeshList())
        {
            Mesh barGraphMesh = barGraphHelper.createBarGraphYield(
                monthlyMesh, cropSurfaceObject.getCropDataObjectList(),
                selectedCropsByMonth[monthlyCount], maxOverallYield);

            Mesh yieldTitleText = meshHelper.getTitleTextMesh(
                "Yield Projections", barGraphMesh, 1, 2);
                barGraphMesh.Append(yieldTitleText);

            //add bg plane
            Mesh meshBase2dPlane = meshHelper.create2dBaseMesh(barGraphMesh);
            barGraphMesh.Append(meshBase2dPlane);

            finalMeshList.Add(barGraphMesh);
            monthlyCount++;
        }

        finalMeshList.AddRange(cropSurfaceObject.getTiledMeshList());


        //previousX = meshBase2dPlane.GetBoundingBox(true).Max.X -
        //        meshBase2dPlane.GetBoundingBox(true).Min.X;

        //todo some output tbd
        DA.SetDataList(0, cropDataInput);
        DA.SetDataList(1, finalMeshList);
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
