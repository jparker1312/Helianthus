using System;
using Rhino.DocObjects;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Drawing;

namespace Helianthus
{
	public class Helper
	{
        //TODO move to config
        public static Color Black_COLOR = Color.FromArgb(255, 0, 0, 0);

		public Helper()
		{
		}

		public static List<Mesh> createTextMesh(TextEntity textEntity, DimensionStyle dimensionStyle)
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

