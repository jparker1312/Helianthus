using System;
using GH_IO.Serialization;
using System.Xml.Linq;

namespace Helianthus
{
	public class MaterialDataObject
	{
		private int material_id;
		private string material_name;
		private double material_transparency;

		public MaterialDataObject()
		{
            material_id = 99;
            material_name = "No Material Selected";
            material_transparency = 1.0;
        }

        public MaterialDataObject(int id, string name,
			double transparency)
        {
			material_id = id;
			material_name = name;
			material_transparency = transparency;
        }

        public int getMaterialId()
        {
            return material_id;
        }

        public string getMaterialName()
        {
            return material_name;
        }

		public double getMaterialTransparency()
		{
			return material_transparency;
		}

		public string printMaterial()
		{
			return "Material ID: " + Convert.ToString(this.material_id) +
				", Material Name: " + material_name +
				", Material Transparency: " +
				Convert.ToString(material_transparency);
		}
	}
}

