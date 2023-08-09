﻿using System;
using System.Collections.Generic;
using System.Drawing;

using Grasshopper.Kernel;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Helianthus
{
  public class DliVisualization : GH_Component
  {
    //TODO move to config
    public static Color Black_COLOR = Color.FromArgb(255, 0, 0, 0);

    public DliVisualization()
      : base("SiteSpecificCropVisualization",
             "Site-Specific Crop Visualization",
             "Site-specific crop visualization showing the crop-surface " +
             "suitability according to DLI values",
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
        pManager.AddNumberParameter("SurfaceDLI", "Surface DLI",
            "Surface Sunlight DLI", GH_ParamAccess.item);
        pManager.AddGeometryParameter("SurfaceGeometry", "Surface Geometry",
            "Rhino Surfaces or Rhino Meshes for which crop DLI analysis will " +
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
        pManager.AddLineParameter("SurfaceDLI_Constant", "Surface DLI Constant",
            "Surface DLI Constant Line", GH_ParamAccess.item);
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
        VisualizationDataObject legendData = new VisualizationDataObject();
        double avgSurfaceDLI = double.NaN;

        if (!DA.GetDataList(0, cropDataInput)) { return; }
        if (!DA.GetData(1, ref avgSurfaceDLI)) { return; }
        if (!DA.GetData(2, ref geometryInput)) { return; }
        if (!DA.GetData(3, ref legendData)) { return; }

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
        //calculate tile size. 32 is the current max DLI. 1/32 == .03125
        //todo this may be determined by an input
        double barGraphTileHeight = barGraphPanelHeight * .03125;
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

        //loop through crop data to get Max DLI value
        int cropMaxDli = 0;
        foreach (CropDataObject crop in cropDataInput)
        {
            if (crop.getDli() > cropMaxDli) { cropMaxDli = crop.getDli(); }
        }

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
            //add new tile for each dli value.
            for (int dliCount = 1; dliCount <= crop.getDli(); dliCount++)
            {
                //calculate Y Interval positions for this tile
                double tilePos_y_start = barGraphYStartPoint + xAxisPanelHeight +
                        ((dliCount - 1) * (barGraphTileHeight + tileSpacerSize));
                double tilePos_y_end = tilePos_y_start + barGraphTileHeight;

                Interval xTileInterval = new Interval(tilePos_x_start, tilePos_x_end);
                Interval yTileInterval = new Interval(tilePos_y_start, tilePos_y_end);
                Mesh mesh = Mesh.CreateFromPlane(defaultPlane, xTileInterval, yTileInterval, 1, 1);

                //maxColor will be max dli
                //determine the color value based on relation to max dli
                double colorValueMultiplier = Convert.ToDouble(crop.getDli())/
                        Convert.ToDouble(cropMaxDli);

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

        //YAxis DLI Panel
        for(int dliCount = 0; dliCount <= cropMaxDli; dliCount++)
        {
            //Calculate the YAxis panel Y position using the current DLI count
            // and taking consideration of the yOffset input parameter
            //TODO make z depth adjustable
            Point3d center_point_crop = new Point3d(yPanelStartPos, xAxisPanelHeight +
                (dliCount * (barGraphTileHeight + tileSpacerSize)) + legendData.getGraphOffset().Y, 0.001);
            Plane plane_cropDli = new Plane(center_point_crop, zaxis);

            TextEntity textEntityDliCount = TextEntity.Create(dliCount.ToString(),
                plane_cropDli, defaultDimensionStyle, true, yAxisPanelWidth, 0);

            listOfMesh.AddRange(createTextMesh(textEntityDliCount, defaultDimensionStyle));

            //Create line and text indicating the Avg Surface DLI
            if(dliCount == avgSurfaceDLI)
            {
                //probably will eventually be a plane with transparency
                //could create dotted line using faces with different transparencies
                //or creating a line of meshes
                Line dliLine = new Line(new Point3d(barGraphXStartPoint,
                    center_point_crop.Y, 0), center_point_crop);
                DA.SetData(1, dliLine);

                Point3d center_point_crop_dli = new Point3d(barGraphXStartPoint,
                    center_point_crop.Y + .3, 0.001);
                Plane plane_crop_dli = new Plane(center_point_crop_dli, zaxis);

                TextEntity textEntity_yearlyDli = TextEntity.Create("Yearly Average Surface DLI",
                    plane_crop_dli, defaultDimensionStyle, true, xAxisBarGraphLength/2, 0);

                listOfMesh.AddRange(createTextMesh(textEntity_yearlyDli, defaultDimensionStyle));
            }
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
    protected override Bitmap Icon => Properties.Resources.siteSpecificCropVis_icon;

    public override GH_Exposure Exposure => GH_Exposure.secondary;

    /// <summary>
    /// Each component must have a unique Guid to identify it. 
    /// It is vital this Guid doesn't change otherwise old ghx files 
    /// that use the old ID will partially fail during loading.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("a198cc45-fb68-4a49-81eb-4fe516d7314e"); }
    }
  }
}
