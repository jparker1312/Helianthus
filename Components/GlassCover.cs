using System;
using System.Collections.Generic;
using System.Drawing;

using Grasshopper.Kernel;

namespace Helianthus
{
  public class GlassCover : GH_Component
  {
    /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
    public GlassCover()
      : base("Glass_Cover",
             "Glass Cover",
             "Selection of glass types for inclusion of material " +
             "transmittance in simulations.",
             "Helianthus",
             "01 | Import Data")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddIntegerParameter(
            "Glass_Type",
            "Glass Type",
            "An integer representing the selected glass type." +
            $"{Environment.NewLine}{Environment.NewLine}" +
            "Glass Type 0 : Low-Iron Glass with 92% transmittance; " +
            $"{Environment.NewLine}" +
            "Glass Type 1 : Float Glass with 84% transmittance;" +
            $"{Environment.NewLine}" +
            "Glass Type 2 : Highly Selective Double-Glazed Unit (DGU) with " +
            "65% transmittance. " +
            $"{Environment.NewLine}{Environment.NewLine}" +
            "If unspecified, defaults to 0.",
            GH_ParamAccess.item,
            0);
            pManager[0].Optional = true;
        }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter(
            "Out",
            "Out",
            "Outputs the input parameters",
            GH_ParamAccess.list);
        pManager.AddGenericParameter(
            "Cover_Material",
            "Cover Material",
            "Selected Cover Material with its transmittance value",
            GH_ParamAccess.item);
        pManager.AddNumberParameter(
            "Transmittance Value",
            "Transmittance Value",
            "Transmittance Value of the selected cover matterial. " +
            "The transmittance is the fraction of incident light that passes " +
            "through a material.",
            GH_ParamAccess.item);
        pManager.AddTextParameter(
            "Available_Glass_Types",
            "Available Glass Types",
            "Available Glass Types with IDs, names, " +
            "and transmittance values",
            GH_ParamAccess.list);
    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object can be used to retrieve data from
    /// input parameters and 
    /// to store data in output parameters.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        int inputOption = 0;
        DA.GetData(0, ref inputOption);

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
    /// Provides an Icon for every component that will be visible in the User
    /// Interface.
    /// Icons need to be 24x24 pixels.
    /// </summary>
    protected override Bitmap Icon => Properties.Resources.glassCover_icon;

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
