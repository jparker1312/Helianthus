using System.Collections.Generic;
using System.Drawing;

namespace Helianthus
{
	public class Config
	{
        public static Color BLACK_COLOR = Color.FromArgb(255, 0, 0, 0);
        public static Color WHITE_COLOR = Color.FromArgb(255, 255, 255, 255);
        public static Color GRAY_GREEN_COLOR = Color.FromArgb(100, 94, 113, 106);

        public static List<Color> DLI_COLOR_RANGE = new List<Color>
        {
            Color.FromArgb(5, 7, 0),
            Color.FromArgb(41, 66, 0),
            Color.FromArgb(78, 125, 0),
            Color.FromArgb(114, 184, 0),
            Color.FromArgb(150, 243, 0),
            Color.FromArgb(176, 255, 47),
            Color.FromArgb(198, 255, 106)
        };

        public static List<Color> YIELD_COLOR_RANGE = new List<Color>
        {
            Color.FromArgb(7, 0, 1),
            Color.FromArgb(66, 0, 8),
            Color.FromArgb(125, 0, 15),
            Color.FromArgb(184, 0, 22),
            Color.FromArgb(243, 0,  29),
            Color.FromArgb(255, 47, 72),
            Color.FromArgb(255, 106, 123)
        };

        public static List<Color> LIGHT_COLOR_RANGE = new List<Color>
        {
            Color.FromArgb(0, 7, 7),
            Color.FromArgb(0, 66, 58),
            Color.FromArgb(0, 125, 110),
            Color.FromArgb(0, 184, 162),
            Color.FromArgb(0, 243, 214),
            Color.FromArgb(47, 255, 230),
            Color.FromArgb(106, 255, 237)
        };

        public static List<Color> ENERGY_COLOR_RANGE = new List<Color>
        {
            Color.FromArgb(3, 0, 7),
            Color.FromArgb(25, 0, 66),
            Color.FromArgb(48, 0, 125),
            Color.FromArgb(70, 0, 184),
            Color.FromArgb(92, 0, 243),
            Color.FromArgb(126, 47, 255),
            Color.FromArgb(162, 106, 255)
        };
    }
}