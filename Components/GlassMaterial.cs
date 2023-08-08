﻿using System;
using System.Collections.Generic;
using System.Drawing;

using Grasshopper.Kernel;

namespace Helianthus.Components
{
  public class GlassMaterial : GH_Component
  {
    /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
    public GlassMaterial()
      : base("Glass_Material",
             "Glass Material",
             "Using default glass material transparency",
             "Helianthus",
             "01 | Import")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
        pManager.AddIntegerParameter("Glass_Type", "Glass Type",
            "Glass Type", GH_ParamAccess.item, 1);
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
        pManager.AddTextParameter("Available_Glass_Types",
            "Available Glass Types", "Available Glass Types",
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

        foreach(MaterialDataObject mdo in
            MaterialConfig.glassMaterialDataObjects)
        {
            if(inputOption == mdo.getMaterialId())
            {
                materialDataObject = mdo;
                break;
            }
        }

        if(materialDataObject == null)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                "could not find material from input: " + inputOption);
        }

        List<string> glassMaterials = new List<string>
        {
            MaterialConfig.lowIronGlass.printMaterial(),
            MaterialConfig.floatGlass.printMaterial(),
            MaterialConfig.dguGlass.printMaterial()
        };

            List<string> inputParams = new List<string>
        {
            inputOption.ToString()
        };

        DA.SetDataList(0, inputParams);
        DA.SetData(1, materialDataObject);
        DA.SetData(2, materialDataObject.getMaterialTransparency());
        DA.SetDataList(3, glassMaterials);
    }

    /// <summary>
    /// Provides an Icon for every component that will be visible in the User Interface.
    /// Icons need to be 24x24 pixels.
    /// </summary>
    protected override Bitmap Icon => Properties.Resources.glassMaterial_icon;

    public override GH_Exposure Exposure => GH_Exposure.tertiary;

    /// <summary>
    /// Each component must have a unique Guid to identify it. 
    /// It is vital this Guid doesn't change otherwise old ghx files 
    /// that use the old ID will partially fail during loading.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("bdebf069-45b2-40f9-880c-1f9e1edba393"); }
    }
  }
}
