using System;
using Rhino.Geometry;

namespace Helianthus
{
	public class LegendDataObject
	{
		private Point3d graphOffset;
		private double graphScale;
		private double graphBackgroundTransparency;
		//private ConsoleColor graphBackgroundColor;
		//bar chart colors
		//text height
		//font style?

		public LegendDataObject()
		{
			graphOffset = new Point3d(1,0,.01);
			graphScale = 1;
			graphBackgroundTransparency = 50;
		}

		public void setGraphOffset(Point3d offset)
		{
			graphOffset = offset;
		}

		public void setGraphScale(double scale)
		{
			graphScale = scale;
		}
		
		public void setGraphBackgroundTransparency(double transparency)
		{
			graphBackgroundTransparency = transparency;
		}

		public Point3d getGraphOffset()
		{
			return this.graphOffset;
		}

		public double getGraphScale()
		{
			return this.graphScale;
		}

		public double getGraphBackgroundTransparency()
		{
			return this.graphBackgroundTransparency;
		}

	}
}

