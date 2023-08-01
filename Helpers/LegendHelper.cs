using System;
using System.Collections.Generic;
using System.Drawing;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Helianthus
{
	public class LegendHelper
	{
        private Point3d center_point;
        private Point3d height_point;
        private Vector3d zaxis;
        private Plane baseBarGraphPlane;
        private double step;

        public LegendHelper()
		{
            center_point = new Point3d(0, 0, 0.001);
            height_point = new Point3d(0, 0, 10);
            zaxis = height_point - center_point;
            baseBarGraphPlane = new Plane(new Point3d(0, 0, 0.0001), zaxis);
            step = 1.0 / Config.DLI_COLOR_RANGE.Count;
        }

		public Mesh createLegend(Mesh joinedMesh, bool isVerticalLegend)
		{
            //used to offset and scale our graph
            Point3d minBoundingBoxPoint = joinedMesh.GetBoundingBox(true).Min;
            Point3d maxBoundingBoxPoint = joinedMesh.GetBoundingBox(true).Max;

            double barGraphXStartPoint;
            double barGraphYStartPoint;
            double barGraphXEndPoint;
            double barGraphYEndPoint;

            if (isVerticalLegend)
            {
                barGraphXStartPoint = maxBoundingBoxPoint.X + 1;
                barGraphYStartPoint = minBoundingBoxPoint.Y;
                barGraphXEndPoint = barGraphXStartPoint +
                        ((maxBoundingBoxPoint.X - minBoundingBoxPoint.X) * .05);
                barGraphYEndPoint = maxBoundingBoxPoint.Y;

                //todo change this quick fix
                if (barGraphXEndPoint - barGraphXStartPoint < 1)
                {
                    barGraphXEndPoint = barGraphXStartPoint + 1;
                }

                //todo why dont i just check orientation and then set y to max Z
                if ((barGraphYEndPoint - barGraphYStartPoint) < (barGraphXEndPoint - barGraphXStartPoint))
                {
                    barGraphYEndPoint = (barGraphXEndPoint - barGraphXStartPoint) * 2;
                }
            }
            else
            {
                //horizontal legend
                barGraphXStartPoint = minBoundingBoxPoint.X;
                barGraphYStartPoint = minBoundingBoxPoint.Y - 2;
                barGraphXEndPoint = barGraphXStartPoint +
                        ((maxBoundingBoxPoint.X - minBoundingBoxPoint.X) * .05);
                barGraphYEndPoint = barGraphYStartPoint +
                    (maxBoundingBoxPoint.X - minBoundingBoxPoint.X);
            }

            Interval xIntervalBaseMesh = new Interval(barGraphXStartPoint,
                barGraphXEndPoint);
            Interval yintervalBaseMesh = new Interval(barGraphYStartPoint,
                barGraphYEndPoint);

            //offset starter plane on z axis so that it does not interfer with
            //ground geometry. TODO: Take this as input 
            int xCount = Convert.ToInt32(xIntervalBaseMesh.Max - xIntervalBaseMesh.Min);
            int yCount = Convert.ToInt32(yintervalBaseMesh.Max - yintervalBaseMesh.Min);

            if (xCount < 2) { xCount = 2; }
            if (yCount < 10) { yCount = 10; }

            Mesh baseBarGraphMesh = Mesh.CreateFromPlane(baseBarGraphPlane,
                xIntervalBaseMesh, yintervalBaseMesh, xCount, yCount);
            baseBarGraphMesh.VertexColors.CreateMonotoneMesh(Color.Gray);

            if (!isVerticalLegend)
            {
                double radians = (Math.PI / 180) * -90;
                baseBarGraphMesh.Rotate(radians, new Vector3d(0, 0, 1),
                    baseBarGraphMesh.GetBoundingBox(true).Min);
            }

            double colorIndTemp;
            List<Color> barGraphVertexColors = new List<Color>();
            int maxXVertices = xCount + 1;
            int maxYVertices = yCount + 1;
            double faceRowCount = 1;
            int faceXCount = 0;
            foreach (Color c in baseBarGraphMesh.VertexColors)
            {
                //get percentage of difference
                double tempPercentage = faceRowCount / maxYVertices;
                colorIndTemp = step;
                for (int colorIndCount = 0; colorIndCount <
                    Config.DLI_COLOR_RANGE.Count; colorIndCount++)
                {
                    if (tempPercentage <= colorIndTemp ||
                            (tempPercentage == 1 && colorIndCount ==
                            (Config.DLI_COLOR_RANGE.Count - 1)))
                    {
                        Color minColor;
                        if (colorIndCount > 0)
                        {
                            minColor = Config.DLI_COLOR_RANGE[colorIndCount - 1];
                        }
                        else
                        {
                            minColor = Config.DLI_COLOR_RANGE[colorIndCount];
                        }

                        Color maxColor = Config.DLI_COLOR_RANGE[colorIndCount];
                        double p = (tempPercentage - (colorIndTemp - step)) / (colorIndTemp - (colorIndTemp - step));
                        double red = minColor.R * (1 - p) + maxColor.R * p;
                        double green = minColor.G * (1 - p) + maxColor.G * p;
                        double blue = minColor.B * (1 - p) + maxColor.B * p;
                        barGraphVertexColors.Add(Color.FromArgb(255, Convert.ToInt32(red),
                            Convert.ToInt32(green), Convert.ToInt32(blue)));

                        faceXCount++;
                        if (faceXCount == maxXVertices)
                        {
                            faceXCount = 0;
                            faceRowCount += 1;
                        }
                        break;
                    }
                    colorIndTemp += step;
                }
            }

            int faceIndexNumber = 0;
            foreach (Color c in barGraphVertexColors)
            {
                baseBarGraphMesh.VertexColors[faceIndexNumber] = c;
                faceIndexNumber++;
            }

            return baseBarGraphMesh;
        }

        public Mesh addLegendDescriptor(string text, double centerPointX,
            double centerPointY, double textHeight)
        {
            Point3d center_point = new Point3d(0, 0, 0.001);
            Point3d height_point = new Point3d(0, 0, 10);
            //Constant? probably
            Vector3d zaxis = height_point - center_point;

            //detault to text height = .1 and then scale??. Add as a constant??
            DimensionStyle defaultDimensionStyle = new DimensionStyle();
            defaultDimensionStyle.TextHeight = textHeight;

            Point3d center_point_crops = new Point3d(centerPointX,
                centerPointY, 0.001);
            Plane plane_crop = new Plane(center_point_crops, zaxis);

            TextEntity textEntityCropName = TextEntity.Create(text, plane_crop,
                defaultDimensionStyle, true, 10, 0);

            MeshHelper meshHelper = new MeshHelper();
            Mesh finalTextMesh = meshHelper.createTextMesh(textEntityCropName,
                defaultDimensionStyle);
            return finalTextMesh;
        }
	}
}

