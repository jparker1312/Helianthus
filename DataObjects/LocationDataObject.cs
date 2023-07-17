using System;
namespace Helianthus
{
	public class LocationDataObject
	{
        string city;
        string state;
        string country;
        string source;
        string stationId;
        double latitude;
        double longitude;
        double timeZone;
        double elevation;

		public LocationDataObject()
		{
		}

        public static LocationDataObject fromCSV(string csvLine)
		{
            string[] values = csvLine.Split(',');
            LocationDataObject radiationDataObject = new LocationDataObject();
			radiationDataObject.city = values[1];
            radiationDataObject.state = values[2];
            radiationDataObject.country = values[3];
            radiationDataObject.source = values[4];
            radiationDataObject.stationId = values[5];
            radiationDataObject.latitude = Convert.ToDouble(values[6]);
            radiationDataObject.longitude = Convert.ToDouble(values[7]);
            radiationDataObject.timeZone = Convert.ToDouble(values[8]);
            radiationDataObject.elevation = Convert.ToDouble(values[9]);
            return radiationDataObject;
		}
	}
}

