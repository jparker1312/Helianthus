using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace Helianthus
{
	public class CropSurfaceObject
	{
        private List<Mesh> monthlyMeshList;
        private List<CropDataObject> cropDataObjectList;

        public CropSurfaceObject()
        {
            cropDataObjectList = new List<CropDataObject>();
            monthlyMeshList = new List<Mesh>();
        }

        public CropSurfaceObject(List<Mesh> meshList, List<CropDataObject> cropDataObject)
		{
			cropDataObjectList = cropDataObject;
            monthlyMeshList = meshList;
        }

		public List<Mesh> getTiledMeshList()
		{
			return monthlyMeshList;
		}

        public List<CropDataObject> getCropDataObjectList()
        {
            return cropDataObjectList;
        }

    }
}

