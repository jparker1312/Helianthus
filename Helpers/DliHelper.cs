using System;
namespace Helianthus
{
	public class DliHelper
	{
		public DliHelper()
		{
		}

		public double getDliFromX(double surfaceSunlight)
		{
            //todo need to check this calculation for monthly...
            //Convert Surface Sunlight constant units to DLI
            //Divide by days in a year
            double surfaceSunlightDLI = surfaceSunlight / 365;
            //divide by the determined hours of sunlight. Should be the same as the input for the EPW duration
            surfaceSunlightDLI = surfaceSunlightDLI / 12;
            //multiply by 1000 to get the W/m2
            surfaceSunlightDLI = surfaceSunlightDLI * 1000;
            //divide by 2.02 to get the par
            surfaceSunlightDLI = surfaceSunlightDLI / 2.02;
            //multiply by .0864 to get the DLI
            surfaceSunlightDLI = surfaceSunlightDLI * 0.0864;
            surfaceSunlightDLI = Math.Round(surfaceSunlightDLI, 0);

            ////Convert Surface Sunlight constant units to DLI
            ////Divide by days in a year
            //double surfaceSunlightDLI = surfaceSunlight / 365;
            ////divide by the determined hours of sunlight. Should be the same as the input for the EPW duration
            //surfaceSunlightDLI = surfaceSunlightDLI / 12;
            //multiply by 1000 to get the W/m2
            //double surfaceSunlightDLI = surfaceSunlight * 1000;
            ////divide by 2.02 to get the par
            //surfaceSunlightDLI = surfaceSunlightDLI / 2.02;
            ////multiply by .0864 to get the DLI
            //surfaceSunlightDLI = surfaceSunlightDLI * 0.0864;
            //surfaceSunlightDLI = Math.Round(surfaceSunlightDLI, 0);

            return surfaceSunlightDLI;
		}
	}
}

