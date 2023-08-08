using System.Collections.Generic;

namespace Helianthus
{
	public class MaterialConfig
	{
		public MaterialConfig()
		{
		}

        //GLASS MATERIALS
		public static MaterialDataObject lowIronGlass =
			new MaterialDataObject(0, "Low-Iron Glass", .92);

        public static MaterialDataObject floatGlass =
            new MaterialDataObject(1, "Float Glass", .84);

        public static MaterialDataObject dguGlass =
            new MaterialDataObject(2,
				"Highly Selective Double-Glazed Unit (DGU)", .65);

        public static List<MaterialDataObject> glassMaterialDataObjects =
            new List<MaterialDataObject>
        {
            lowIronGlass,
            floatGlass,
            dguGlass
        };

        // PLASTIC MATERIALS
        public static MaterialDataObject plaPlastic =
            new MaterialDataObject(0, "3D-Printed Polylactic Acid (PLA)", .70);

        public static MaterialDataObject pvPlastic =
            new MaterialDataObject(1, "Polymer Photovoltaics (PV)", .70);

        public static MaterialDataObject petPlastic =
            new MaterialDataObject(2, "3D-Printed Upcycled PET Bottles", .65);

        public static List<MaterialDataObject> plasticMaterialDataObjects =
            new List<MaterialDataObject>
        {
            plaPlastic,
            pvPlastic,
            petPlastic
        };
    }
}