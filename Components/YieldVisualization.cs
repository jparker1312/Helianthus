using System;
using System.Collections.Generic;
using System.Drawing;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input.Custom;
using Rhino.Render;
using Rhino.UI;
using static Rhino.Render.TextureGraphInfo;

namespace Helianthus
{
  public class YieldVisualization : GH_Component
  {
    //TODO move to config
    public static Color Black_COLOR = Color.FromArgb(255, 0, 0, 0);

    public YieldVisualization()
      : base("YieldVisualization",
             "Yield Visualization",
             "Yield visualization showing the yearly crop yields.",
             "Helianthus",
             "03 | Visualize Data")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
        pManager.AddGenericParameter("CropsToVisualize", "Crops To Visualize",
            "List of Crops that you want to visualize", GH_ParamAccess.list);
        pManager.AddGeometryParameter("SurfaceGeometry", "Surface Geometry",
            "Rhino Surfaces or Rhino Meshes for which crop yield analysis will " +
            "be conducted", GH_ParamAccess.item);
        pManager.AddGenericParameter("LegendParameters", "Legend Parameters",
            "Legend Parameters for the visualization", GH_ParamAccess.item);
    }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
        pManager.AddMeshParameter("Graph", "Graph", "Graph made up of a list " +
            "of meshes visualizing suitability of crop-surface DLIs",
            GH_ParamAccess.list);
    }

    /// <summary>
    /// ???? Create Bar Graph
    /// </summary>
    /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
    /// to store data in output parameters.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        List<CropDataObject> cropDataInput = new List<CropDataObject>();
        Brep geometryInput = new Brep();
        LegendDataObject legendData = new LegendDataObject();

        if (!DA.GetDataList(0, cropDataInput)) { return; }
        if (!DA.GetData(1, ref geometryInput)) { return; }
        if (!DA.GetData(2, ref legendData)) { return; }

        //Create empty output object mesh list
        List<Mesh> listOfMesh = new List<Mesh>();

        //Create a plane with a zaxis vector. The center point is set at 0.001
        //so that the graph information will sit in front of the graph background
        //TODO: center point should be impacted by Z height parameter
        Point3d center_point = new Point3d(0, 0, 0.001);
        Point3d height_point = new Point3d(0, 0, 10);
        //Constant? probably
        Vector3d zaxis = height_point - center_point;
        Plane defaultPlane = new Plane(center_point, zaxis);

        //Get the bounding box for the input geometry.
        //This will be used to offset and scale our graph
        Point3d minBoundingBoxGeometry = geometryInput.GetBoundingBox(true).Min;
        Point3d maxBoundingBoxGeometry = geometryInput.GetBoundingBox(true).Max;

        //Get the bar graph visualization start points (use point data holder
        //instead???). Do this before we scale the bounding box
        double barGraphXStartPoint = maxBoundingBoxGeometry.X + legendData.getGraphOffset().X;
        double barGraphYStartPoint = minBoundingBoxGeometry.Y + legendData.getGraphOffset().Y;

        //scale the bounding box based on input
        maxBoundingBoxGeometry = Point3d.Multiply(maxBoundingBoxGeometry, legendData.getGraphScale());

        //Take 10% of the bounding box. Reserve for the X axis key information
        double xAxisPanelHeight = maxBoundingBoxGeometry.Y * .1;
        //Get the remaining Y axis length
        double barGraphPanelHeight = maxBoundingBoxGeometry.Y - xAxisPanelHeight;

        //loop through crop data to get Max DLI value
        int cropMaxYield = 0;
        foreach (CropDataObject crop in cropDataInput)
        {
            if (crop.getCropYield() > cropMaxYield) { cropMaxYield = crop.getCropYield(); }
        }
        // TODO: calculate tile size. 32 is the current max DLI. 1/32 == .03125 / this needs to change to something less static
        //use the max yeild or dli?
        double barGraphTileHeight = barGraphPanelHeight * (1/Convert.ToDouble(cropMaxYield));
        //spacer using 10% of tile size currently
        double tileSpacerSize = barGraphTileHeight * .1;
        barGraphTileHeight -= tileSpacerSize;
        //x axis tile size is double to y axis (this is specific to our bar graph)
        double barGraphTileWidth = barGraphTileHeight * 2;
        //y key table area will be 1/3 the size of a tile in x length
        double yAxisPanelWidth = barGraphTileWidth * .33;
        double xAxisBarGraphLength = cropDataInput.Count *
                (barGraphTileWidth + tileSpacerSize) + yAxisPanelWidth;

        Interval xIntervalBaseMesh = new Interval(barGraphXStartPoint,
            barGraphXStartPoint + xAxisBarGraphLength);
        Interval yintervalBaseMesh = new Interval(barGraphYStartPoint,
            barGraphYStartPoint + maxBoundingBoxGeometry.Y);

        //offset starter plane on z axis so that it does not interfer with
        //ground geometry. TODO: Take this as input 
        Plane baseBarGraphPlane = new Plane(new Point3d(0, 0, 0.0001), zaxis);
        Mesh baseBarGraphMesh = Mesh.CreateFromPlane(baseBarGraphPlane,
            xIntervalBaseMesh, yintervalBaseMesh, 1, 1);

        //deaulting to a white color. Could allow for specification of base color...
        baseBarGraphMesh.VertexColors.CreateMonotoneMesh(
            Color.FromArgb(Convert.ToInt32(
                legendData.getGraphBackgroundTransparency()),250, 250, 250));
        listOfMesh.Add(baseBarGraphMesh);

        //detault to text height = .1 and then scale??. Add as a constant??
        DimensionStyle defaultDimensionStyle = new DimensionStyle();
        defaultDimensionStyle.TextHeight = .1 * legendData.getGraphScale();

        double yPanelStartPos = 0;
        int cropCount = 0;
        //loop through each crop, list name, create bar graph tiles
        foreach (CropDataObject crop in cropDataInput)
        {
            //list the crop names along the X Axis
            //Divide tile length by 2 in order to place text with middle indentation
            Point3d center_point_crops = new Point3d(((cropCount) *
                (barGraphTileWidth + tileSpacerSize)) + barGraphXStartPoint +
                (barGraphTileWidth/2), legendData.getGraphOffset().Y, 0.001);
            Plane plane_crop = new Plane(center_point_crops, zaxis);
            plane_crop.Rotate(1.5708, zaxis);

            TextEntity textEntityCropName = TextEntity.Create(crop.getSpecie(),
                plane_crop, defaultDimensionStyle, true, xAxisPanelHeight, 0);
            listOfMesh.AddRange(createTextMesh(textEntityCropName, defaultDimensionStyle));

            //calculate new X Interval positions for set of tiles
            double tilePos_x_start = barGraphXStartPoint + ((cropCount) *
                    (barGraphTileWidth + tileSpacerSize));
            double tilePos_x_end = tilePos_x_start + barGraphTileWidth;
            //add new tile for each yield value.
            for (int yieldCount = 1; yieldCount <= crop.getCropYield(); yieldCount++)
            {
                //calculate Y Interval positions for this tile
                double tilePos_y_start = barGraphYStartPoint + xAxisPanelHeight +
                        ((yieldCount - 1) * (barGraphTileHeight + tileSpacerSize));
                double tilePos_y_end = tilePos_y_start + barGraphTileHeight;

                Interval xTileInterval = new Interval(tilePos_x_start, tilePos_x_end);
                Interval yTileInterval = new Interval(tilePos_y_start, tilePos_y_end);
                Mesh mesh = Mesh.CreateFromPlane(defaultPlane, xTileInterval, yTileInterval, 1, 1);

                //maxColor will be max yield
                //determine the color value based on relation to max yield
                double colorValueMultiplier = Convert.ToDouble(crop.getCropYield())/
                        Convert.ToDouble(cropMaxYield);

                //based off of colors from graph but could be any values.
                //Constant Graph Value for bar chart. TODO Can make the editable
                //TODO remove from for loop
                double startRed = 206;
                double startGreen = 31;
                double startBlue = 83;

                int red = Convert.ToInt32(255 - (colorValueMultiplier *
                    (255-startRed)));
                int green = Convert.ToInt32(255 - (colorValueMultiplier *
                    (255-startGreen)));
                int blue = Convert.ToInt32(255 - (colorValueMultiplier *
                    (255-startBlue)));   

                mesh.VertexColors.CreateMonotoneMesh(Color.FromArgb(
                    255, red, green, blue));

                listOfMesh.Add(mesh);
            }

            yPanelStartPos = tilePos_x_end + tileSpacerSize;
            cropCount++;
        }

        //YAxis Yield Panel
        for(int yieldCount = 0; yieldCount <= cropMaxYield; yieldCount++)
        {
            //Calculate the YAxis panel Y position using the current yield count
            // and taking consideration of the yOffset input parameter
            //TODO make z depth adjustable
            Point3d center_point_crop = new Point3d(yPanelStartPos, xAxisPanelHeight +
                (yieldCount * (barGraphTileHeight + tileSpacerSize)) + legendData.getGraphOffset().Y, 0.001);
            Plane plane_cropDli = new Plane(center_point_crop, zaxis);

            TextEntity textEntityDliCount = TextEntity.Create(yieldCount.ToString(),
                plane_cropDli, defaultDimensionStyle, true, yAxisPanelWidth, 0);

            listOfMesh.AddRange(createTextMesh(textEntityDliCount, defaultDimensionStyle));
        }
        //Output the list of meshes
        DA.SetDataList(0, listOfMesh);
    }

    private List<Mesh> createTextMesh(TextEntity textEntity, DimensionStyle dimensionStyle)
    {
        List<Mesh> listOfMeshes = new List<Mesh>();
        Brep[] breps = textEntity.CreateSurfaces(dimensionStyle, 1, 0);
        foreach(Brep b in breps)
        {
            Mesh[] meshArrayOfBreps = Mesh.CreateFromBrep(b, new MeshingParameters());
            foreach(Mesh m in meshArrayOfBreps)
            {
                m.VertexColors.CreateMonotoneMesh(Black_COLOR);
                listOfMeshes.Add(m);
            }
        }
        return listOfMeshes;
    }

    /// <summary>
    /// Provides an Icon for every component that will be visible in the User Interface.
    /// Icons need to be 24x24 pixels.
    /// </summary>
    protected override Bitmap Icon => Properties.Resources.yieldVis_icon;

    public override GH_Exposure Exposure => GH_Exposure.tertiary;

    /// <summary>
    /// Each component must have a unique Guid to identify it. 
    /// It is vital this Guid doesn't change otherwise old ghx files 
    /// that use the old ID will partially fail during loading.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("d5dd2914-1088-4249-b446-eb6fc5470103"); }
    }
  }
}
