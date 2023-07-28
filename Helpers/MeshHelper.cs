using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Rhino.Geometry;
using Rhino.DocObjects;

namespace Helianthus
{
	public class MeshHelper
	{

        //TODO move to config
        public static Color Black_COLOR = Color.FromArgb(255, 0, 0, 0);

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

        public Mesh createGriddedMesh(List<Brep> geometryInput, double gridSize)
        {
            Mesh joinedMesh = new Mesh();
            MeshingParameters meshingParameters = new MeshingParameters();
            meshingParameters.MaximumEdgeLength = gridSize;
            meshingParameters.MinimumEdgeLength = gridSize;
            meshingParameters.GridAspectRatio = 1;

            List<Mesh[]> griddedMeshArrayList = new List<Mesh[]>();
            foreach (Brep b in geometryInput)
            {
                griddedMeshArrayList.Add(Mesh.CreateFromBrep(b, meshingParameters));
            }

            foreach (Mesh[] meshArray in griddedMeshArrayList)
            {
                foreach (Mesh m in meshArray) { joinedMesh.Append(m); }
            }

            joinedMesh.FaceNormals.ComputeFaceNormals();

            return joinedMesh;
        }

        public List<Point3d> getPointsOfMesh(Mesh joinedMesh)
        {
            List<Point3d> points = new List<Point3d>();
            for (int i = 0; i < joinedMesh.Faces.Count; i++)
            {
                Point3d tempPoint = joinedMesh.Faces.GetFaceCenter(i);
                Vector3d tempVector = joinedMesh.FaceNormals[i];
                tempVector = tempVector * .1;
                points.Add(new Point3d(tempPoint.X + tempVector.X,
                                       tempPoint.Y + tempVector.Y,
                                       tempPoint.Z + tempVector.Z));
            }

            return points;
        }

        public Mesh getContextMesh(List<Brep> geometryInput, List<Brep> contextGeometryInput)
        {
            foreach (Brep b in geometryInput) { contextGeometryInput.Append(b); }

            Brep mergedContextGeometry = new Brep();
            foreach (Brep b in contextGeometryInput)
            {
                mergedContextGeometry.Append(b);
            }

            Mesh[] contextMeshArray = Mesh.CreateFromBrep(
                mergedContextGeometry, new MeshingParameters());
            Mesh contextMesh = new Mesh();
            foreach (Mesh m in contextMeshArray) { contextMesh.Append(m); }

            return contextMesh;
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

        public Mesh create2dBaseMesh(Mesh inputMesh)
        {
            //Create a plane with a zaxis vector. The center point is set at 0.001
            //so that the graph information will sit in front of the graph background
            //TODO: center point should be impacted by Z height parameter
            Point3d center_point = new Point3d(0, 0, 0.001);
            Point3d height_point = new Point3d(0, 0, 10);
            //todo Constant? probably
            Vector3d zaxis = height_point - center_point;

            //Get the bounding box for the input geometry.
            //This will be used to offset and scale our graph
            Point3d minBoundingBoxGeometry = inputMesh.GetBoundingBox(true).Min;
            Point3d maxBoundingBoxGeometry = inputMesh.GetBoundingBox(true).Max;

            //scale the bounding box based on input
            //todo need to think about scale
            //maxBoundingBoxGeometry = Point3d.Multiply(maxBoundingBoxGeometry, 1);
            double boundingBoxWidth = maxBoundingBoxGeometry.X - minBoundingBoxGeometry.X;
            double boundingBoxHeight = maxBoundingBoxGeometry.Y - minBoundingBoxGeometry.Y;

            Interval xIntervalBaseMesh = new Interval(minBoundingBoxGeometry.X,
                minBoundingBoxGeometry.X + boundingBoxWidth);
            Interval yintervalBaseMesh = new Interval(minBoundingBoxGeometry.Y,
                minBoundingBoxGeometry.Y + boundingBoxHeight);

            //offset starter plane on z axis so that it does not interfer with
            //ground geometry. TODO: Take this as input 
            Plane basePlane = new Plane(new Point3d(0, 0, 0.0001), zaxis);
            Mesh baseMesh = Mesh.CreateFromPlane(basePlane,
                xIntervalBaseMesh, yintervalBaseMesh, 1, 1);

            //deaulting to a white color. Could allow for specification of base color...
            //todo make this editable
            baseMesh.VertexColors.CreateMonotoneMesh(
                Color.FromArgb(50, 250, 250, 250));

            return baseMesh;
        }

        public List<Mesh> createTextMesh(TextEntity textEntity, DimensionStyle dimensionStyle)
        {
            List<Mesh> listOfMeshes = new List<Mesh>();
            Brep[] breps = textEntity.CreateSurfaces(dimensionStyle, 1, 0);
            foreach (Brep b in breps)
            {
                Mesh[] meshArrayOfBreps = Mesh.CreateFromBrep(b, new MeshingParameters());
                foreach (Mesh m in meshArrayOfBreps)
                {
                    m.VertexColors.CreateMonotoneMesh(Black_COLOR);
                    listOfMeshes.Add(m);
                }
            }
            return listOfMeshes;
        }

        public Mesh getTitleTextMesh(string text, Mesh inputMesh,
            double textHeight)
        {
            //TODO: center point should be impacted by Z height parameter
            Point3d center_point = new Point3d(0, 0, 0.001);
            Point3d height_point = new Point3d(0, 0, 10);
            //Constant? probably
            Vector3d zaxis = height_point - center_point;

            //TODO make z depth adjustable
            DimensionStyle defaultDimensionStyle = new DimensionStyle();
            defaultDimensionStyle.TextHeight = textHeight;

            Point3d center_point_crop = new Point3d(
                inputMesh.GetBoundingBox(true).Min.X,
                inputMesh.GetBoundingBox(true).Max.Y + 5, 0.001);
            Plane plane_cropDli = new Plane(center_point_crop, zaxis);

            TextEntity textEntityDliCount = TextEntity.Create(text,
                plane_cropDli, defaultDimensionStyle, true,
                inputMesh.GetBoundingBox(true).Max.X -
                inputMesh.GetBoundingBox(true).Min.X, 0);

            List<Mesh> listMesh = createTextMesh(textEntityDliCount,
                defaultDimensionStyle);

            Mesh finalMesh = new Mesh();
            foreach(Mesh m in listMesh) { finalMesh.Append(m); }
            return finalMesh;
        }

        public Mesh getTitleTextMeshByPosition(string text, Point3d startTextPoint,
            double textHeight, double rectWidth)
        {
            //TODO: center point should be impacted by Z height parameter
            Point3d center_point = new Point3d(0, 0, 0.001);
            Point3d height_point = new Point3d(0, 0, 10);
            //Constant? probably
            Vector3d zaxis = height_point - center_point;

            //TODO make z depth adjustable
            DimensionStyle defaultDimensionStyle = new DimensionStyle();
            defaultDimensionStyle.TextHeight = textHeight;

            Point3d center_point_crop = new Point3d(startTextPoint);
            Plane plane_cropDli = new Plane(center_point_crop, zaxis);

            TextEntity textEntityDliCount = TextEntity.Create(text,
                plane_cropDli, defaultDimensionStyle, true, rectWidth, 0);

            List<Mesh> listMesh = createTextMesh(textEntityDliCount,
                defaultDimensionStyle);

            Mesh finalMesh = new Mesh();
            foreach (Mesh m in listMesh) { finalMesh.Append(m); }
            return finalMesh;
        }
    }
}

