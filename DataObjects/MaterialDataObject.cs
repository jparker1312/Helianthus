using System;
namespace Helianthus
{
	public class MaterialDataObject
	{
		private int material_id;
		private string material_name;
		private double material_transparency;

		public MaterialDataObject()
		{
		}

		public string getMaterialName()
        {
            return this.material_name;
        }

		public double getMaterialTransparency()
		{
			return this.material_transparency;
		}
	}
}

