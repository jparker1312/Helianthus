using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Helianthus
{
	public class GenDayMtxHelper
	{
        //todo need to figure out how to make this editable. maybe just a parameter? how does ladybug do it?
        //private string genDayMtxFilePath = "/Users/joel/Projects/Programming/AlbaThesis/GrasshopperTools/radiance/bin/gendaymtx";
        private static string gendaymtx_arg_direct = "-m 1 -d -A -h ";
        private static string gendaymtx_arg_diffuse = "-m 1 -s -A -h ";

		public GenDayMtxHelper()
		{
		}

        public List<double> getGenDayMtxTotalRadiation(string weaFileLocation,
            string radianceFolder)
        {
            string genDayMtxFilePath = radianceFolder + "/bin/gendaymtx";
            GenDayMtxHelper genDayMtxHelper = new GenDayMtxHelper();
            string directRadiationRGB = genDayMtxHelper.callGenDayMtx(
                genDayMtxFilePath, weaFileLocation, true);
            string diffuseRadiationRGB = genDayMtxHelper.callGenDayMtx(
                genDayMtxFilePath, weaFileLocation, false);

            SimulationHelper simulationHelper = new SimulationHelper();
            List<double> directRadiationList;
            List<double> diffuseRadiationList;
            directRadiationList = simulationHelper.convertRgbRadiationList(
                directRadiationRGB);
            diffuseRadiationList = simulationHelper.convertRgbRadiationList(
                diffuseRadiationRGB);

            List<double> totalRadiationList = simulationHelper.
                getTotalRadiationList(directRadiationList, diffuseRadiationList);

            return totalRadiationList;
        }

        public string callGenDayMtx(string genDayMtxFilePath, string weaFileLocation, bool getDirectRadiation)
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

        public List<List<double>> runMonthlyGenDayMtxSimulations2(string pathname,
            List<string> monthlyRange, string radianceFolder)
        {
            List<List<double>> totalRadiationListByMonth = new List<List<double>>();
            for (int monthCount10 = 1; monthCount10 <= 12; monthCount10++)
            {
                if (monthlyRange.Contains(Convert.ToString(monthCount10)))
                {
                    string weaFileLocation = pathname + "weaMonth-" +
                        monthCount10 + ".wea";

                    List<double> genDayMtxTotalRadiationList = 
                        getGenDayMtxTotalRadiation(weaFileLocation,
                        radianceFolder);

                    totalRadiationListByMonth.Add(genDayMtxTotalRadiationList);
                }
            }

            return totalRadiationListByMonth;
        }
	}
}

