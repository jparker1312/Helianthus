using System;
using Rhino.DocObjects;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Drawing;

namespace Helianthus
{
	public class BarGraphHelper
	{
        //TODO move to config
        public static Color Black_COLOR = Color.FromArgb(255, 0, 0, 0);

		public BarGraphHelper()
		{
		}

		public List<Mesh> createBarGraph(Mesh meshInput, List<CropDataObject> cropDataInput, LegendDataObject legendData)
		{
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
            Point3d minBoundingBoxGeometry = meshInput.GetBoundingBox(true).Min;
            Point3d maxBoundingBoxGeometry = meshInput.GetBoundingBox(true).Max;

            //Get the bar graph visualization start points (use point data holder
            //instead???). Do this before we scale the bounding box
            double barGraphXStartPoint = minBoundingBoxGeometry.X + legendData.getGraphOffset().X;
            double barGraphYStartPoint = minBoundingBoxGeometry.Y + legendData.getGraphOffset().Y;

            //scale the bounding box based on input
            //todo need to think about scale
            maxBoundingBoxGeometry = Point3d.Multiply(maxBoundingBoxGeometry, legendData.getGraphScale());

            //Take 10% of the bounding box. Reserve for the X axis key information
            double boundingBoxHeight = maxBoundingBoxGeometry.Y - minBoundingBoxGeometry.Y;

            double xAxisPanelHeight = boundingBoxHeight * .1;
            //Get the remaining Y axis length
            double barGraphPanelHeight = boundingBoxHeight - xAxisPanelHeight;
            //calculate tile size. 32 is the current max DLI. 1/32 == .03125
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
            double finalYStartPoint = barGraphYStartPoint - boundingBoxHeight - 1;
            Interval yintervalBaseMesh = new Interval(finalYStartPoint,
                finalYStartPoint + boundingBoxHeight);

            //offset starter plane on z axis so that it does not interfer with
            //ground geometry. TODO: Take this as input 
            Plane baseBarGraphPlane = new Plane(new Point3d(0, 0, 0.0001), zaxis);
            Mesh baseBarGraphMesh = Mesh.CreateFromPlane(baseBarGraphPlane,
                xIntervalBaseMesh, yintervalBaseMesh, 1, 1);

            //baseBarGraphMesh.Translate(new Vector3d(0, -boundingBoxHeight - 1, 0));

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
                //todo might need legendData.getGraphOffset().Y
                Point3d center_point_crops = new Point3d(((cropCount) *
                    (barGraphTileWidth + tileSpacerSize)) + barGraphXStartPoint +
                    (barGraphTileWidth/2), finalYStartPoint, 0.001);
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
                    double tilePos_y_start = finalYStartPoint + xAxisPanelHeight +
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
                    double startRed = 114;
                    double startGreen = 184;
                    double startBlue = 0;

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
                    (dliCount * (barGraphTileHeight + tileSpacerSize)) + finalYStartPoint, 0.001);
                Plane plane_cropDli = new Plane(center_point_crop, zaxis);

                TextEntity textEntityDliCount = TextEntity.Create(dliCount.ToString(),
                    plane_cropDli, defaultDimensionStyle, true, yAxisPanelWidth, 0);

                listOfMesh.AddRange(createTextMesh(textEntityDliCount, defaultDimensionStyle));

                //Create line and text indicating the Avg Surface DLI
                ////todo maybe dpnt need this???
                //if(dliCount == avgSurfaceDLI)
                //{
                //    //probably will eventually be a plane with transparency
                //    //could create dotted line using faces with different transparencies
                //    //or creating a line of meshes
                //    Line dliLine = new Line(new Point3d(barGraphXStartPoint,
                //        center_point_crop.Y, 0), center_point_crop);
                //    //todo need to turn this into mesh?
                //    //DA.SetData(1, dliLine);

                //    Point3d center_point_crop_dli = new Point3d(barGraphXStartPoint,
                //        center_point_crop.Y + .3, 0.001);
                //    Plane plane_crop_dli = new Plane(center_point_crop_dli, zaxis);

                //    TextEntity textEntity_yearlyDli = TextEntity.Create("Yearly Average Surface DLI",
                //        plane_crop_dli, defaultDimensionStyle, true, xAxisBarGraphLength/2, 0);

                //    listOfMesh.AddRange(createTextMesh(textEntity_yearlyDli, defaultDimensionStyle));
                //}
            }
            //Output the list of meshes

            return listOfMesh;
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
	}
}

