using System;
using Rhino.Geometry;

namespace Helianthus
{
	public class BarGraphObject
	{
        private Mesh titleMesh;
		private Mesh barGraphMesh;

        public BarGraphObject()
		{
		}

        public void setBarGraphMesh(Mesh mesh)
        {
            barGraphMesh = mesh;
        }

        public void setTitleMesh(Mesh mesh)
        {
            titleMesh = mesh;
        }

        public Mesh getBarGraphMesh()
        {
            return barGraphMesh;
        }

        public Mesh getTitleMesh()
        {
            return titleMesh;
        }
    }
}

