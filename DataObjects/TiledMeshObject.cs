using System.Collections.Generic;
using Rhino.Geometry;

namespace Helianthus
{
	public class TiledMeshObject
	{
        private Mesh monthTitleMesh;
		private Mesh tiledMesh;
		private Mesh titleMesh;
        private Mesh legendMesh;
		private Mesh backgroundMesh;

		private List<double> dataTreeRadiation;
		private List<string> monthlyCropsThatFitRangeString;

        private BarGraphObject barGraphObject;

		public TiledMeshObject()
		{
			//tiledMesh = new Mesh();
			//title = new Mesh();
			//backgroundMesh = new Mesh();

			//dataTreeRadiation = new DataTree<double>();
			//monthlyCropsThatFitRangeString = new DataTree<string>();
		}

        public TiledMeshObject(Mesh mesh)
        {
            tiledMesh = mesh;
        }

        public void setMonthTitleMesh(Mesh mesh)
        {
            monthTitleMesh = mesh;
        }

        public void setTiledMesh(Mesh mesh)
		{
			tiledMesh = mesh;
		}

        public void setTitleMesh(Mesh mesh)
        {
            tiledMesh = mesh;
        }

        public void setLegendMesh(Mesh mesh)
        {
            legendMesh = mesh;
        }

        public void setBackgroundMesh(Mesh mesh)
        {
            tiledMesh = mesh;
        }

        public void setRadiationList(List<double> radiationValues)
        {
            dataTreeRadiation = radiationValues;
        }

        public void setMonthlyCropsList(List<string> cropValues)
        {
            monthlyCropsThatFitRangeString = cropValues;
        }

        public void setBarGraphObject(BarGraphObject barGraph)
        {
            barGraphObject = barGraph;
        }

        public List<double> getRadiationList()
        {
            return dataTreeRadiation;
        }

        public BarGraphObject getBarGraphMesh()
        {
            return barGraphObject;
        }

        public Mesh appendAllMeshes()
        {
            Mesh appendedMesh = new Mesh();
            appendedMesh.Append(monthTitleMesh);
            appendedMesh.Append(tiledMesh);
            appendedMesh.Append(titleMesh);
            appendedMesh.Append(legendMesh);
            appendedMesh.Append(barGraphObject.getBarGraphMesh());
            appendedMesh.Append(barGraphObject.getTitleMesh());
            return appendedMesh;
        }
    }
}

