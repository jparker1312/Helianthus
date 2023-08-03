using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Helianthus
{
	public class GenDayMtxHelper
	{
        //todo need to figure out how to make this editable. maybe just a parameter? how does ladybug do it?
        private string genDayMtxFilePath = "/Users/joel/Projects/Programming/AlbaThesis/GrasshopperTools/radiance/bin/gendaymtx";
        private static string gendaymtx_arg_direct = "-m 1 -d -A -h ";
        private static string gendaymtx_arg_diffuse = "-m 1 -s -A -h ";

		public GenDayMtxHelper()
		{
		}

		public string callGenDayMtx(string weaFileLocation, bool getDirectRadiation)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = false;
            startInfo.FileName = genDayMtxFilePath;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;       
            startInfo.RedirectStandardOutput = true;
            if (getDirectRadiation)
            {
                startInfo.Arguments = gendaymtx_arg_direct + weaFileLocation;
            }
            else
            {
                startInfo.Arguments = gendaymtx_arg_diffuse + weaFileLocation;
            }

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

        public List<string> runMonthlyGenDayMtxSimulations(string pathname,
            List<string> monthlyRange, bool getDirectRadiation)
        {
            List<string> monthlySimulations = new List<string>();
            for (int monthCount = 1; monthCount <= 12; monthCount++)
            {
                if (monthlyRange.Contains(Convert.ToString(monthCount)))
                {
                    string weaFileLocation = pathname + "weaMonth-" +
                        monthCount + ".wea";
                    string radiationRGB = callGenDayMtx(weaFileLocation, getDirectRadiation);
                    monthlySimulations.Add(radiationRGB);
                }
            }
            return monthlySimulations;
        }
	}
}

