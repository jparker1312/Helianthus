using System;
using Rhino.DocObjects;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Drawing;

namespace Helianthus
{
	public class BarGraphHelper
	{
        private Point3d center_point;
        private Point3d height_point;
        private Vector3d zaxis;
        private Plane defaultPlane;
        private DimensionStyle defaultDimensionStyle;
        private MeshHelper meshHelper;

        public BarGraphHelper()
		{
            center_point = new Point3d(0, 0, 0.001);
            height_point = new Point3d(0, 0, 10);
            zaxis = height_point - center_point;
            defaultPlane = new Plane(center_point, zaxis);
            defaultDimensionStyle = new DimensionStyle
            {
                TextHeight = .3
            };
            meshHelper = new MeshHelper();
        }

		//public Mesh createBarGraph(Mesh meshInput,
  //          List<CropDataObject> cropDataInput, double maxDli,
  //          double overallMaxDli)
		//{
  //          //round DLI numbers
  //          maxDli = Math.Round(maxDli, 0, MidpointRounding.AwayFromZero);
  //          overallMaxDli = Math.Round(overallMaxDli, 0,
  //              MidpointRounding.AwayFromZero);

  //          Mesh finalMesh = new Mesh();
  //          //Get the bar graph visualization start points,
  //          //Do this before we scale the bounding box
  //          Point3d minBoundingBoxGeometry = meshInput.GetBoundingBox(true).Min;
  //          Point3d maxBoundingBoxGeometry = meshInput.GetBoundingBox(true).Max;
  //          double barGraphXStartPoint = minBoundingBoxGeometry.X;
  //          double barGraphYStartPoint = minBoundingBoxGeometry.Y;

  //          //10% of the bounding box. Reserve for the X axis key information
  //          double boundingBoxHeight = maxBoundingBoxGeometry.Y -
  //              minBoundingBoxGeometry.Y;
  //          double xAxisPanelHeight = boundingBoxHeight * .1;
  //          //Get the remaining Y axis length
  //          double barGraphPanelHeight = boundingBoxHeight - xAxisPanelHeight;

  //          double boundingBoxWidth = (maxBoundingBoxGeometry.X -
  //              minBoundingBoxGeometry.X);

  //          //can double as yAxisPanelWidth
  //          double columnWidth = boundingBoxWidth * (1 / Convert.ToDouble(
  //              cropDataInput.Count + 1));
  //          //calculate tile size. 32 is the current max DLI. 1/32 == .03125
  //          double barGraphTileHeight = barGraphPanelHeight * .03125;
  //          //spacer using 10% of tile size currently
  //          double tileSpacerSize = barGraphTileHeight * .1;
  //          barGraphTileHeight -= tileSpacerSize;
  //          //x axis tile size is double to y axis (specific to our graph)
  //          //double barGraphTileWidth = barGraphTileHeight * 2;
  //          double barGraphTileWidth = columnWidth - tileSpacerSize;
  //          //y key table area will be 1/3 the size of a tile in x length
  //          //double yAxisPanelWidth = barGraphTileWidth * .33;
  //          double finalYStartPoint = barGraphYStartPoint -
  //              boundingBoxHeight - 1;

  //          double yPanelStartPos = 0;
  //          int cropCount = 0;
  //          //loop through each crop, list name, create bar graph tiles
  //          foreach (CropDataObject crop in cropDataInput)
  //          {
  //              //list the crop names along the X Axis
  //              //Divide tile length by 2 in order to place text with middle
  //              //indentation
  //              Point3d center_point_crops = new Point3d((cropCount *
  //                  (barGraphTileWidth + tileSpacerSize)) +
  //                  barGraphXStartPoint + (barGraphTileWidth/2),
  //                  finalYStartPoint, 0.001);
  //              Plane plane_crop = new Plane(center_point_crops, zaxis);
  //              plane_crop.Rotate(1.5708, zaxis);

  //              TextEntity textEntityCropName = TextEntity.Create(
  //                  crop.getSpecie(), plane_crop, defaultDimensionStyle, true,
  //                  xAxisPanelHeight, 0);
  //              finalMesh.Append(meshHelper.createTextMesh(textEntityCropName,
  //                  defaultDimensionStyle));

  //              //create bar graph tiles
  //              //calculate new X Interval positions for set of tiles
  //              double tilePos_x_start = barGraphXStartPoint + ((cropCount) *
  //                      (barGraphTileWidth + tileSpacerSize));
  //              double tilePos_x_end = tilePos_x_start + barGraphTileWidth;

  //              Mesh barGraphTiles = createBarGraphTiles(crop,
  //                  overallMaxDli, maxDli, xAxisPanelHeight, finalYStartPoint,
  //                  barGraphTileHeight, tileSpacerSize, tilePos_x_start,
  //                  tilePos_x_end);
  //              finalMesh.Append(barGraphTiles);

  //              yPanelStartPos = tilePos_x_end + tileSpacerSize;
  //              cropCount++;
  //          }

  //          //create legend for recommended crops
  //          Mesh legendMeshes = createLegendForRecommendedCrops(
  //              barGraphXStartPoint, finalYStartPoint, barGraphPanelHeight,
  //              barGraphTileWidth, barGraphTileHeight, tileSpacerSize);
  //          finalMesh.Append(legendMeshes);

  //          int cropMaxDli = 0;
  //          foreach (CropDataObject crop in cropDataInput)
  //          {
  //              if (crop.getDli() > cropMaxDli) { cropMaxDli = crop.getDli(); }
  //          }

  //          //YAxis DLI Panel
  //          Mesh yPanelList = createBarGraphYAxisPanel(cropMaxDli,
  //              yPanelStartPos, xAxisPanelHeight, barGraphTileHeight,
  //              tileSpacerSize, finalYStartPoint, columnWidth);//todo check this
  //          finalMesh.Append(yPanelList);

  //          return finalMesh;
		//}





        public Mesh createBarGraph2(Mesh meshInput,
            List<CropDataObject> cropDataInput, double maxDli,
            double overallDenomenator, List<string> selectedCrops, string type)
        {
            //round DLI numbers
            if(type.Equals(Convert.ToString(Config.BarGraphType.DLI)))
            {
                maxDli = Math.Round(maxDli, 0, MidpointRounding.AwayFromZero);
            } 
            overallDenomenator = Math.Round(overallDenomenator, 0,
                MidpointRounding.AwayFromZero);

            Mesh finalMesh = new Mesh();
            //Get the bar graph visualization start points,
            //Do this before we scale the bounding box
            Point3d minBoundingBoxGeometry = meshInput.GetBoundingBox(true).Min;
            Point3d maxBoundingBoxGeometry = meshInput.GetBoundingBox(true).Max;
            double barGraphXStartPoint = minBoundingBoxGeometry.X;
            double barGraphYStartPoint = minBoundingBoxGeometry.Y;

            //10% of the bounding box. Reserve for the X axis key information
            double boundingBoxHeight = maxBoundingBoxGeometry.Y -
                minBoundingBoxGeometry.Y;
            double xAxisPanelHeight = boundingBoxHeight * .1;
            //Get the remaining Y axis length
            double barGraphPanelHeight = boundingBoxHeight - xAxisPanelHeight;

            double boundingBoxWidth = (maxBoundingBoxGeometry.X -
                minBoundingBoxGeometry.X);

            //can double as yAxisPanelWidth
            double columnWidth = boundingBoxWidth * (1 / Convert.ToDouble(
                cropDataInput.Count + 1));
            //calculate tile size. 32 is the current max DLI. 1/32 == .03125
            double barGraphTileHeight = barGraphPanelHeight * .03125;
            //spacer using 10% of tile size currently
            double tileSpacerSize = barGraphTileHeight * .1;
            barGraphTileHeight -= tileSpacerSize;
            //x axis tile size is double to y axis (specific to our graph)
            //double barGraphTileWidth = barGraphTileHeight * 2;
            double barGraphTileWidth = columnWidth - tileSpacerSize;
            //y key table area will be 1/3 the size of a tile in x length
            //double yAxisPanelWidth = barGraphTileWidth * .33;
            double finalYStartPoint = barGraphYStartPoint -
                boundingBoxHeight - 1;

            double yPanelStartPos = 0;
            int cropCount = 0;
            //loop through each crop, list name, create bar graph tiles
            foreach (CropDataObject crop in cropDataInput)
            {
                //list the crop names along the X Axis
                //Divide tile length by 2 in order to place text with middle
                //indentation
                Point3d center_point_crops = new Point3d((cropCount *
                    (barGraphTileWidth + tileSpacerSize)) +
                    barGraphXStartPoint + (barGraphTileWidth / 2),
                    finalYStartPoint, 0.001);
                Plane plane_crop = new Plane(center_point_crops, zaxis);
                plane_crop.Rotate(1.5708, zaxis);

                TextEntity textEntityCropName = TextEntity.Create(
                    crop.getSpecie(), plane_crop, defaultDimensionStyle, true,
                    xAxisPanelHeight, 0);
                finalMesh.Append(meshHelper.createTextMesh(textEntityCropName,
                    defaultDimensionStyle));

                //create bar graph tiles
                //calculate new X Interval positions for set of tiles
                double tilePos_x_start = barGraphXStartPoint + ((cropCount) *
                        (barGraphTileWidth + tileSpacerSize));
                double tilePos_x_end = tilePos_x_start + barGraphTileWidth;

                if (type.Equals(Convert.ToString(Config.BarGraphType.DLI)))
                {
                    Mesh barGraphTiles = createBarGraphTiles(crop,
                        overallDenomenator, maxDli, xAxisPanelHeight, finalYStartPoint,
                        barGraphTileHeight, tileSpacerSize, tilePos_x_start,
                        tilePos_x_end);
                    finalMesh.Append(barGraphTiles);
                }
                else if (type.Equals(Convert.ToString(Config.BarGraphType.YIELD)))
                {
                    Mesh barGraphTiles = createBarGraphTilesYield(crop,
                        overallDenomenator, selectedCrops, xAxisPanelHeight,
                        finalYStartPoint, barGraphTileHeight, tileSpacerSize,
                        tilePos_x_start, tilePos_x_end);
                    finalMesh.Append(barGraphTiles);
                }

                yPanelStartPos = tilePos_x_end + tileSpacerSize;
                cropCount++;
            }

            double maxY = 0;
            if (type.Equals(Convert.ToString(Config.BarGraphType.DLI)))
            {
                //create legend for recommended crops
                Mesh legendMeshes = createLegendForRecommendedCrops(
                    barGraphXStartPoint, finalYStartPoint, barGraphPanelHeight,
                    barGraphTileWidth, barGraphTileHeight, tileSpacerSize);
                finalMesh.Append(legendMeshes);

                foreach (CropDataObject crop in cropDataInput)
                {
                    if (crop.getDli() > maxY) { maxY = crop.getDli(); }
                }
            }
            else if (type.Equals(Convert.ToString(Config.BarGraphType.YIELD)))
            {
                maxY = overallDenomenator;
            }

            //YAxis DLI Panel
            Mesh yPanelList = createBarGraphYAxisPanel(maxY,
                yPanelStartPos, xAxisPanelHeight, barGraphTileHeight,
                tileSpacerSize, finalYStartPoint, columnWidth);
            finalMesh.Append(yPanelList);

            return finalMesh;
        }













        //public Mesh createBarGraphYield(Mesh meshInput,
        //    List<CropDataObject> cropDataInput, List<string> selectedCrops,
        //    double maxOverallYield)
        //{
        //    Mesh finalMesh = new Mesh();
        //    //Get the bar graph visualization start points,
        //    Point3d minBoundingBoxGeometry = meshInput.GetBoundingBox(true).Min;
        //    Point3d maxBoundingBoxGeometry = meshInput.GetBoundingBox(true).Max;
        //    double barGraphXStartPoint = minBoundingBoxGeometry.X;
        //    double barGraphYStartPoint = minBoundingBoxGeometry.Y;

        //    //10% of the bounding box. Reserve for the X axis key information
        //    double boundingBoxHeight = (maxBoundingBoxGeometry.Y -
        //        minBoundingBoxGeometry.Y);
        //    double xAxisPanelHeight = boundingBoxHeight * .1;
        //    //Get the remaining Y axis length
        //    double barGraphPanelHeight = boundingBoxHeight - xAxisPanelHeight;
        //    //calculate yAxisPanel
        //    double boundingBoxWidth = (maxBoundingBoxGeometry.X -
        //        minBoundingBoxGeometry.X);
        //    //can double as yAxisPanelWidth
        //    double columnWidth = boundingBoxWidth * (1 / Convert.ToDouble(cropDataInput.Count + 1));
        //    //double yAxisPanelWidth = boundingBoxWidth * (1/cropDataInput.Count + 7);
        //    //calculate tile size. 32 is the current max DLI. 1/32 == .03125
        //    double barGraphTileHeight = barGraphPanelHeight * .03125;
        //    //spacer using 10% of tile size currently
        //    double tileSpacerSize = barGraphTileHeight * .1;
        //    barGraphTileHeight -= tileSpacerSize;
        //    //x axis tile size is double to y axis (specific to our graph)
        //    double barGraphTileWidth = columnWidth - tileSpacerSize;

        //    double finalYStartPoint = barGraphYStartPoint -
        //        boundingBoxHeight - 1;

        //    double yPanelStartPos = 0;
        //    int cropCount = 0;
        //    //loop through each crop, list name, create bar graph tiles
        //    foreach (CropDataObject crop in cropDataInput)
        //    {
        //        Point3d center_point_crops = new Point3d((cropCount *
        //            (barGraphTileWidth + tileSpacerSize)) +
        //            barGraphXStartPoint + (barGraphTileWidth / 2),
        //            finalYStartPoint, 0.001);
        //        Plane plane_crop = new Plane(center_point_crops, zaxis);
        //        plane_crop.Rotate(1.5708, zaxis);

        //        TextEntity textEntityCropName = TextEntity.Create(
        //            crop.getSpecie(), plane_crop, defaultDimensionStyle, true,
        //            xAxisPanelHeight, 0);
        //        finalMesh.Append(meshHelper.createTextMesh(textEntityCropName,
        //            defaultDimensionStyle));

        //        //create bar graph tiles
        //        //calculate new X Interval positions for set of tiles
        //        double tilePos_x_start = barGraphXStartPoint + ((cropCount) *
        //                (barGraphTileWidth + tileSpacerSize));
        //        double tilePos_x_end = tilePos_x_start + barGraphTileWidth;

        //        //todo only diff
        //        Mesh barGraphTiles = createBarGraphTilesYield(crop,
        //            maxOverallYield, selectedCrops, xAxisPanelHeight,
        //            finalYStartPoint, barGraphTileHeight, tileSpacerSize,
        //            tilePos_x_start, tilePos_x_end);

        //        finalMesh.Append(barGraphTiles);

        //        yPanelStartPos = tilePos_x_end + tileSpacerSize;
        //        cropCount++;
        //    }

        //    //YAxis DLI Panel
        //    Mesh yPanelList = createBarGraphYAxisPanel(maxOverallYield,
        //        yPanelStartPos, xAxisPanelHeight, barGraphTileHeight,
        //        tileSpacerSize, finalYStartPoint, columnWidth);
        //    finalMesh.Append(yPanelList);

        //    return finalMesh;
        //}

        private Mesh createBarGraphTiles(CropDataObject crop,
            double overallMaxDli, double maxDli,
            double xAxisPanelHeight, double finalYStartPoint,
            double barGraphTileHeight, double tileSpacerSize,
            double tilePos_x_start, double tilePos_x_end)
        {
            bool lessThanMax = false;
            //check if crop dli fits recommended crops
            if (crop.getDli() <= maxDli){ lessThanMax = true; }

            Mesh finalMesh = colorTiles(crop.getDli(), finalYStartPoint,
                xAxisPanelHeight, barGraphTileHeight, tileSpacerSize,
                tilePos_x_start, tilePos_x_end, lessThanMax,
                Convert.ToDouble(crop.getDli()), overallMaxDli,
                Config.DLI_COLOR_RANGE);

            return finalMesh;
        }

        private Mesh createLegendForRecommendedCrops(
            double barGraphXStartPoint, double finalYStartPoint,
            double barGraphPanelHeight, double barGraphTileWidth,
            double barGraphTileHeight, double tileSpacerSize)
        {
            Mesh finalMesh = new Mesh();
            Point3d center_point_recs = new Point3d(barGraphXStartPoint,
                finalYStartPoint + barGraphPanelHeight, 0.001);

            Point3d rccenter_point = new Point3d(0,
                0, 0.001);
            Plane recPlane = new Plane(rccenter_point, zaxis);
            Plane plane_recs = new Plane(center_point_recs, zaxis);

            Interval xTileIntervalrecs = new Interval(barGraphXStartPoint, barGraphXStartPoint + barGraphTileWidth);
            Interval yTileIntervalrecs = new Interval(finalYStartPoint + barGraphPanelHeight, finalYStartPoint + barGraphPanelHeight + barGraphTileHeight);
            Mesh rec_mesh = Mesh.CreateFromPlane(recPlane, xTileIntervalrecs, yTileIntervalrecs, 1, 1);
            rec_mesh.VertexColors.CreateMonotoneMesh(Color.FromArgb(100, 94, 113, 106));
            finalMesh.Append(rec_mesh);

            plane_recs.Translate(new Vector3d(barGraphTileWidth, 0.5, 0));
            TextEntity textEntityRecName = TextEntity.Create("Not Recommended",
                plane_recs, defaultDimensionStyle, true, 20, 0);
            finalMesh.Append(meshHelper.createTextMesh(textEntityRecName, defaultDimensionStyle));

            recPlane.Translate(new Vector3d(0, -barGraphTileHeight - tileSpacerSize, 0));
            Mesh rec_mesh2 = Mesh.CreateFromPlane(recPlane, xTileIntervalrecs, yTileIntervalrecs, 1, 1);
            rec_mesh2.VertexColors.CreateMonotoneMesh(Color.FromArgb(255, 255, 255, 255));
            finalMesh.Append(rec_mesh2);

            plane_recs.Translate(new Vector3d(0, -1, 0));
            TextEntity textEntityRec2Name = TextEntity.Create("Recommended Under Supplemental Lighting",
                plane_recs, defaultDimensionStyle, true, 20, 0);
            finalMesh.Append(meshHelper.createTextMesh(textEntityRec2Name, defaultDimensionStyle));

            return finalMesh;
        }

        private Mesh createBarGraphTilesYield(CropDataObject crop,
            double overallMaxYield, List<string> cropNames, 
            double xAxisPanelHeight, double finalYStartPoint,
            double barGraphTileHeight, double tileSpacerSize,
            double tilePos_x_start, double tilePos_x_end)
        {
            bool selected = false;
            foreach(string cropName in cropNames)
            {
                if (crop.getSpecie().Equals(cropName))
                {
                    selected = true;
                    break;
                }
            }

            double maxCount = crop.getMonthlyCropYield();
            //add new tile for each yield value
            if(maxCount < 1)
            {
                maxCount = 1;
            }


            Mesh finalMesh = colorTiles(maxCount, finalYStartPoint,
                xAxisPanelHeight, barGraphTileHeight, tileSpacerSize,
                tilePos_x_start, tilePos_x_end, selected,
                crop.getMonthlyCropYield(), overallMaxYield,
                Config.YIELD_COLOR_RANGE);

            return finalMesh;
        }

        private Mesh colorTiles(double maxCount, double finalYStartPoint,
            double xAxisPanelHeight, double barGraphTileHeight,
            double tileSpacerSize, double tilePos_x_start,
            double tilePos_x_end, bool monoMesh, double numerator,
            double denomenator, List<Color> colorRange)
        {
            Mesh finalMesh = new Mesh();
            for (int count = 1; count <= maxCount; count++)
            {
                //calculate Y Interval positions for this tile
                double tilePos_y_start = finalYStartPoint +
                    xAxisPanelHeight + ((count - 1) *
                    (barGraphTileHeight + tileSpacerSize));
                double tilePos_y_end = tilePos_y_start + barGraphTileHeight;

                Mesh mesh = meshHelper.createMeshFromPlane(defaultPlane,
                    tilePos_x_start, tilePos_x_end,
                    tilePos_y_start, tilePos_y_end);

                if (!monoMesh)
                {
                    mesh.VertexColors.CreateMonotoneMesh(
                        Config.GRAY_GREEN_COLOR);
                }
                else
                {
                    //determine the color value based on relation to overall max dli
                    double colorValueMultiplier = numerator / denomenator;
                    Color meshColor = meshHelper.colorMeshByColorStepping(
                        colorValueMultiplier, colorRange);
                    mesh.VertexColors.CreateMonotoneMesh(meshColor);
                }
                finalMesh.Append(mesh);
            }

            return finalMesh;
        }

        private Mesh createBarGraphYAxisPanel(double maxCount,
            double yPanelStartPos, double xAxisPanelHeight,
            double barGraphTileHeight, double tileSpacerSize,
            double finalYStartPoint, double yAxisPanelWidth)
        {
            Mesh finalMesh = new Mesh();
            for (int count = 0; count <= maxCount; count++)
            {
                //Calculate the YAxis panel Y position using the current DLI count
                // and taking consideration of the yOffset input parameter
                Point3d center_point_crop = new Point3d(yPanelStartPos, xAxisPanelHeight +
                    (count * (barGraphTileHeight + tileSpacerSize)) + finalYStartPoint, 0.001);
                Plane plane_cropDli = new Plane(center_point_crop, zaxis);

                TextEntity textEntityDliCount = TextEntity.Create(count.ToString(),
                    plane_cropDli, defaultDimensionStyle, true, yAxisPanelWidth, 0);

                finalMesh.Append(meshHelper.createTextMesh(textEntityDliCount, defaultDimensionStyle));
            }

            return finalMesh;
        }
    }
}

