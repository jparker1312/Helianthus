using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Helianthus.Components
{
  public class PlasticMaterial : GH_Component
  {
    /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
    public PlasticMaterial()
      : base("Plastic_Material",
             "Plastic Material",
             "Using default plastic material transparency",
             "Helianthus",
             "01 | Import")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
        pManager.AddIntegerParameter("Plastic_Type", "Plastic Type",
            "Plastic Type", GH_ParamAccess.item, 1);
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Out", "Out", "Input Parameters",
            GH_ParamAccess.list);
        pManager.AddGenericParameter("Selected_Material", "Selected Material",
            "Selected Material", GH_ParamAccess.item);
        pManager.AddNumberParameter("Transparency Value", "Transparency Value",
            "Transparency Value", GH_ParamAccess.item);
        pManager.AddTextParameter("Available_Plastic_Types",
            "Available Plastic Types", "Available Plastic Types",
            GH_ParamAccess.list);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
    /// to store data in output parameters.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        int inputOption = 0;

        if (!DA.GetData(0, ref inputOption)) { return; }

        MaterialDataObject materialDataObject = null;

        foreach (MaterialDataObject mdo in
            MaterialConfig.plasticMaterialDataObjects)
        {
            if (inputOption == mdo.getMaterialId())
            {
                materialDataObject = mdo;
                break;
            }
        }

        if (materialDataObject == null)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                "could not find material from input: " + inputOption);
        }

        List<string> plasticMaterials = new List<string>
        {
            MaterialConfig.plaPlastic.printMaterial(),
            MaterialConfig.pvPlastic.printMaterial(),
            MaterialConfig.petPlastic.printMaterial()
        };

        List<string> inputParams = new List<string>
        {
            inputOption.ToString()
        };

        DA.SetDataList(0, inputParams);
        DA.SetData(1, materialDataObject);
        DA.SetData(2, materialDataObject.getMaterialTransparency());
        DA.SetDataList(3, plasticMaterials);
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
      get { return new Guid("ea8cc5f4-dace-4e52-b8e4-2355eaab55cd"); }
    }
  }
}
