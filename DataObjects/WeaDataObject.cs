using System;
using System.Collections.Generic;

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

		public WeaDataObject(LocationDataObject locationDataObject, List<RadiationDataObject> listRadiationDataObjects)
		{

		}

		public string writeToWeaFile() {
			return "test.wea";
		}
	}
}

