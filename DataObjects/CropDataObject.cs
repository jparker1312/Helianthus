using System;
using System.IO;

namespace Helianthus
{
	public class CropDataObject
	{
		private int crop_id;
		private string crop_type;
		private string specie;
		private string scientific_name;
		private int dli;
		//private double crop_yield;
		//private string crop_dimensions;
		//private string crop_visualization;

		public CropDataObject()
		{
		}

		public static CropDataObject fromCSV(string csvLine)
		{
            string[] values = csvLine.Split(',');
            CropDataObject cropDataObject = new CropDataObject();
			cropDataObject.crop_id = Convert.ToInt32(values[0]);
            cropDataObject.crop_type = values[1];
			cropDataObject.specie = values[2];
            cropDataObject.scientific_name = values[3];
            cropDataObject.dli = Convert.ToInt32(values[4]);
            //cropDataObject.crop_yield = Convert.ToDouble(values[4]);
            //cropDataObject.crop_dimensions = values[7];
            //cropDataObject.crop_visualization = values[8];
            return cropDataObject;
		}

		public int getDli()
		{
			return this.dli;
		}

        public string getSpecie()
        {
            return this.specie;
        }
    }
}

