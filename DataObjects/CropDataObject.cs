using System;

namespace Helianthus
{
	public class CropDataObject
	{
		private int crop_id;
		private string crop_type;
		private string specie;
		private string scientific_name;
		private int dli;
        private int dli_group_classification;
        private int yearly_crop_yield;
        private double monthly_crop_yield;
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
			cropDataObject.specie = values[2].Trim();
            cropDataObject.scientific_name = values[3];
            cropDataObject.dli = Convert.ToInt32(values[4]);
            cropDataObject.dli_group_classification = Convert.ToInt32(values[5]);
            cropDataObject.yearly_crop_yield = Convert.ToInt32(values[6]);
            cropDataObject.monthly_crop_yield = Convert.ToDouble(values[7]);

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

		public int getYearlyCropYield()
		{
			return this.yearly_crop_yield;
		}

        public double getMonthlyCropYield()
        {
            return this.monthly_crop_yield;
        }
    }
}