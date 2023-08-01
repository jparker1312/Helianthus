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
}
}

