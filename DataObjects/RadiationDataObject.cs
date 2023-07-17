using System;
using System.Collections.Generic;

namespace Helianthus
{
	public class RadiationDataObject
	{
		private DateTime dateTime;
		private Int32 dirRadiation;
		private Int32 diffRadiation;
		
		public RadiationDataObject()
		{
		}

		public static RadiationDataObject fromCSV(string csvLine)
		{
            string[] values = csvLine.Split(',');
            RadiationDataObject radiationDataObject = new RadiationDataObject();

			try {
				radiationDataObject.dateTime = new DateTime(Convert.ToInt32(values[0]),
				Convert.ToInt32(values[1]), Convert.ToInt32(values[2]),
				Convert.ToInt32(values[3]), Convert.ToInt32(values[4]), 0);
			}
			catch (Exception e) {
				Console.WriteLine(e.ToString());
			}
			
			radiationDataObject.dirRadiation = Convert.ToInt32(values[14]);
            radiationDataObject.diffRadiation = Convert.ToInt32(values[15]);
            return radiationDataObject;
		}
	}
}

