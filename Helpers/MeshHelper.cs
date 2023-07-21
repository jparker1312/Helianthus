using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;

using Rhino.Geometry;

namespace Helianthus
{
	public class MeshHelper
	{

        private List<Color> colorRange;
        private double step;

		public MeshHelper()
		{
            //todo change to rgb
            //todo create steps of logical green
            colorRange = new List<Color>();
            //colorRange.Add(Color.Black);
            //colorRange.Add(Color.Gray);
            //colorRange.Add(Color.Gold);
            //colorRange.Add(Color.Yellow);
            colorRange.Add(Color.FromArgb(5, 7, 0));
            colorRange.Add(Color.FromArgb(41, 66, 0));
            colorRange.Add(Color.FromArgb(78, 125, 0));
            colorRange.Add(Color.FromArgb(114, 184, 0));
            colorRange.Add(Color.FromArgb(150, 243, 0));
            colorRange.Add(Color.FromArgb(176, 255, 47));
            colorRange.Add(Color.FromArgb(198, 255, 106));
            step = 1.0 / colorRange.Count;
		}

		public List<Color> getFaceColors(List<double> finalRadiationList, double maxRadiation)
		{
			List<Color> faceColors = new List<Color>();
            double colorIndTemp;
            foreach(double rad in finalRadiationList)
            {
                //get percentage of difference
                double tempRadPercentage = rad / maxRadiation;
                colorIndTemp = step;
                for(int colorIndCount = 0; colorIndCount < colorRange.Count; colorIndCount++)
                {
                    if( tempRadPercentage <= colorIndTemp ||
                        (tempRadPercentage == 1 && colorIndCount == (colorRange.Count - 1)))
                    {
                        Color minColor;
                        if(colorIndCount > 0)
                        {
                            minColor = colorRange[colorIndCount - 1];
                        }
                        else
                        {
                            minColor = colorRange[colorIndCount];
                        }

                        if(tempRadPercentage == 1)
                        {

                        }

                        Color maxColor = colorRange[colorIndCount];
                        double p = (tempRadPercentage - (colorIndTemp - step)) / (colorIndTemp - (colorIndTemp - step));
                        double red = minColor.R * (1 - p) + maxColor.R * p;
                        double green = minColor.G * (1 - p) + maxColor.G * p;
                        double blue = minColor.B * (1 - p) + maxColor.B * p;

                        faceColors.Add(Color.FromArgb(Convert.ToInt32(red),
                            Convert.ToInt32(green), Convert.ToInt32(blue)));
                        break;
                    }
                    colorIndTemp += step;
                }
            }

            return faceColors;
		}

        public Mesh createFinalMesh(Mesh meshJoined)
        {
            Mesh finalMesh = new Mesh();
            int faceIndexNumber = 0;
            int fInd = 0;
            foreach (MeshFace f in meshJoined.Faces)
            {
                Point3f a;
                Point3f b;
                Point3f c;
                Point3f d;
                meshJoined.Faces.GetFaceVertices(fInd, out a, out b, out c, out d);
                finalMesh.Vertices.Add(a);
                finalMesh.Vertices.Add(b);
                finalMesh.Vertices.Add(c);
            
                if (f.IsQuad)
                {
                    finalMesh.Vertices.Add(d);
                    finalMesh.Faces.AddFace(faceIndexNumber, faceIndexNumber + 1, faceIndexNumber + 2, faceIndexNumber + 3);
                    faceIndexNumber += 4;
                }
                else
                { 
                    finalMesh.Faces.AddFace(faceIndexNumber, faceIndexNumber + 1, faceIndexNumber + 2);
                    faceIndexNumber += 3;
                }
                fInd++;
            }

            return finalMesh;
        }

        //todo don't think this needs to return the mesh since it is returning parameter
        public Mesh colorFinalMesh(Mesh finalMesh, List<Color> faceColors)
        {
            finalMesh.VertexColors.CreateMonotoneMesh(Color.Gray);
            int faceIndexNumber = 0;
            int colorIndex = 0;
            foreach (MeshFace f in finalMesh.Faces)
            {
                finalMesh.VertexColors[faceIndexNumber] = faceColors[colorIndex];
                finalMesh.VertexColors[faceIndexNumber + 1] = faceColors[colorIndex];
                finalMesh.VertexColors[faceIndexNumber + 2] = faceColors[colorIndex];

                if (f.IsQuad)
                {
                    finalMesh.VertexColors[faceIndexNumber + 3] = faceColors[colorIndex];
                    faceIndexNumber += 4;
                }
                else
                {
                    faceIndexNumber += 3;
                }
                colorIndex++;
            }

            return finalMesh;
        }
    }
}

