using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Helianthus
{
	public class GenDayMtxHelper
	{
        //todo need to figure out how to make this editable. maybe just a parameter? how does ladybug do it?
        private string genDayMtxFilePath = "/Users/joel/Projects/Programming/AlbaThesis/GrasshopperTools/radiance/bin/gendaymtx";
        public static string gendaymtx_arg_direct = "-m 1 -d -A -h ";
        public static string gendaymtx_arg_diffuse = "-m 1 -s -A -h ";

		public GenDayMtxHelper()
		{
		}

		public string callGenDayMtx(string args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = false;
            startInfo.FileName = genDayMtxFilePath;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;       
            startInfo.RedirectStandardOutput = true;
            startInfo.Arguments = args;
            string stdOut = "";  
            try
            {
                using (Process exeProcess = Process.Start(startInfo))
                { 
                    exeProcess.WaitForExit();
                    stdOut = exeProcess.StandardOutput.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return stdOut;
        }

        public List<string> runMonthlyGenDayMtxSimulations(string pathname, string args, List<string> monthlyRange)
        {
            List<string> monthlySimulations = new List<string>();
            for (int monthCount = 1; monthCount <= 12; monthCount++)
            {
                if (monthlyRange.Contains(Convert.ToString(monthCount)))
                {
                    string gendaymtx_arg = args + pathname + "weaMonth-" + monthCount + ".wea";
                    string radiationRGB = callGenDayMtx(gendaymtx_arg);
                    monthlySimulations.Add(radiationRGB);
                }
            }
            return monthlySimulations;
        }

        public List<double> convertRgbRadiationList(string radiationString)
        {
            List<double> radiationList = new List<double>();
            string[] radiationRGB = radiationString.Split(
                new string[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );

            double wea_duration = 8760;
            int rowCounter = 1;
            for(int rowOfPatches_count =0; rowOfPatches_count < SimulationHelper.TREGENZA_PATCHES_PER_ROW.Length; rowOfPatches_count++)
            {
                var currentRowofPatches = new ArraySegment<string>(radiationRGB, rowCounter, SimulationHelper.TREGENZA_PATCHES_PER_ROW[rowOfPatches_count]);
                foreach(string dr in currentRowofPatches)
                {
                    string[] rgb = dr.Split(' ');
                    double rgbWeightedValue =
                        0.265074126 * Convert.ToDouble(rgb[0]) +
                        0.670114631 * Convert.ToDouble(rgb[1]) +
                        0.064811243 * Convert.ToDouble(rgb[2]);
                    rgbWeightedValue = rgbWeightedValue *
                        SimulationHelper.TREGENZA_COEFFICIENTS[rowOfPatches_count] *
                        wea_duration / 1000;

                    radiationList.Add(rgbWeightedValue);
                }
                rowCounter += SimulationHelper.TREGENZA_PATCHES_PER_ROW[rowOfPatches_count];
            }

            return radiationList;
        }
	}
}

