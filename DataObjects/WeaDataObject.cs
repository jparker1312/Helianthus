using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Helianthus
{
	public class WeaDataObject
	{
		private string location;
		private string direct_normal_irradiance;
		private string diffuse_horizontal_irradiance;
		private string direct_horizontal_irradiance;
		private string global_horizontal_irradiance;
		private string enforce_on_hour;
		private string datetimes;
		private string hoys;
		private string analysis_period;
		private string timestep;
		private string is_leap_year;
		private string is_continuous;
		private string is_annual;
		private string header;
		private List<string> lines;

		public WeaDataObject()
		{
		}

		public WeaDataObject(string pathname)
		{
			lines = File.ReadAllLines(pathname).ToList();
		}

		public WeaDataObject(LocationDataObject locationDataObject, List<RadiationDataObject> listRadiationDataObjects)
		{

		}

		public string writeToWeaFile() {
			return "test.wea";
		}

		//public static WeaDataObject readWeaFile(string pathname)
		//{
		//	WeaDataObject weaDataObject = new WeaDataObject();

		//	//READ THE LOCATION DATA
		//	//1 LINE - RECORD DATA
		//	//LocationDataObject locationDataObject = new LocationDataObject();
		//	//string locationString = File.ReadAllLines(pathname).First();
		//	//locationDataObject = LocationDataObject.fromCSV(locationString);

		//	//SKIP THE NEXT 7 LINES OF THE HEADER
		//	//READ THE DATA
		//	List<string> radiationDataObjectList = File.ReadAllLines(pathname)
		//		.ToList();

		//	return new WeaDataObject();
		//}

		public bool writeWeaDataToMonthlyFiles(string path)
		{
			List<List<string>> monthlyList = new List<List<string>>();

			for (int monthCount = 1; monthCount <= 12; monthCount++)
			{
				List<string> month = lines.GetRange(0, 6);
				foreach(string line in lines)
				{
					string[] tempLines = line.Split(' ');
					if (tempLines[0] == Convert.ToString(monthCount)){
						month.Add(line);
					}
				}

				monthlyList.Add(month);
			}

			int monthlyCount = 1;
			foreach(List<string> monthLines in monthlyList)
			{
				File.WriteAllLines(path + "weaMonth-" + monthlyCount + ".wea", monthLines);
				monthlyCount++;
			}
			
			return true;
		}
	}
}

