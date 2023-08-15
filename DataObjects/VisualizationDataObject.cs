using Rhino.Geometry;

namespace Helianthus
{
	public class VisualizationDataObject
	{
        Point3d diagramCenterpoint;
        Vector3d diagramRotationAxis;
        double diagramRotation = 0;
        private Point3d graphOffset;
		private double graphScale;
		private int graphBackgroundTransparency;

		public VisualizationDataObject()
		{
            diagramCenterpoint = new Point3d(0, 0, 0);
            diagramRotationAxis = new Vector3d(0, 0, 1);
            diagramRotation = 0;
            graphOffset = new Point3d(0, 0, .0001);
			graphScale = 1;
			graphBackgroundTransparency = 50;
		}

        public void setDiagramCenterpoint(Point3d centerpoint)
        {
            diagramCenterpoint = centerpoint;
        }

        public void setDiagramRotationAxis(Vector3d axis)
        {
            diagramRotationAxis = axis;
        }

        public void setDiagramRotation(double degrees)
        {
            diagramRotation = degrees;
        }

        public void setGraphOffset(Point3d offset)
		{
			graphOffset = offset;
		}

		public void setGraphScale(double scale)
		{
			graphScale = scale;
		}
		
		public void setGraphBackgroundTransparency(int transparency)
		{
			graphBackgroundTransparency = transparency;
		}

        public Point3d getDiagramCenterpoint()
        {
            return diagramCenterpoint;
        }

        public Vector3d getDiagramRotationAxis()
        {
            return diagramRotationAxis;
        }

        public double getDiagramRotation()
        {
            return diagramRotation;
        }

        public Point3d getGraphOffset()
		{
			return graphOffset;
		}

		public double getGraphScale()
		{
			return graphScale;
		}

		public int getGraphBackgroundTransparency()
		{
			return graphBackgroundTransparency;
		}
	}
}

