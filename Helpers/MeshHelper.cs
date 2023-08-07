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
        private Point3d center_point;
        private Point3d height_point;
        private Vector3d zaxis;
        private double step;

        public MeshHelper()
		{
            center_point = new Point3d(0, 0, 0.001);
            height_point = new Point3d(0, 0, 10);
            zaxis = height_point - center_point;
            step = 1.0 / Config.DLI_COLOR_RANGE.Count;
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
                griddedMeshArrayList.Add(Mesh.CreateFromBrep(b,
                    meshingParameters));
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

        public Mesh getContextMesh(List<Brep> geometryInput,
            List<Brep> contextGeometryInput)
        {
            Brep mergedContextGeometry = new Brep();
            foreach (Brep b in geometryInput)
            {
                mergedContextGeometry.Append(b);
            }
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
                bool colorSet = false;
                for(int colorIndCount = 0; colorIndCount < Config.DLI_COLOR_RANGE.Count; colorIndCount++)
                {
                    if( tempRadPercentage <= colorIndTemp ||
                        (tempRadPercentage == 1 && colorIndCount == (Config.DLI_COLOR_RANGE.Count - 1)))
                    {
                        Color minColor;
                        if(colorIndCount > 0)
                        {
                            minColor = Config.DLI_COLOR_RANGE[colorIndCount - 1];
                        }
                        else
                        {
                            minColor = Config.DLI_COLOR_RANGE[colorIndCount];
                        }

                        if(tempRadPercentage == 1)
                        {

                        }

                        Color maxColor = Config.DLI_COLOR_RANGE[colorIndCount];
                        double p = (tempRadPercentage - (colorIndTemp - step)) / (colorIndTemp - (colorIndTemp - step));
                        double red = minColor.R * (1 - p) + maxColor.R * p;
                        double green = minColor.G * (1 - p) + maxColor.G * p;
                        double blue = minColor.B * (1 - p) + maxColor.B * p;

                        faceColors.Add(Color.FromArgb(Convert.ToInt32(red),
                            Convert.ToInt32(green), Convert.ToInt32(blue)));
                        colorSet = true;
                        break;
                    }
                    colorIndTemp += step;
                }

                if (!colorSet)
                {

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
            //Get the bounding box for the input geometry.
            //This will be used to offset and scale our graph
            Point3d minBoundingBoxGeometry = inputMesh.GetBoundingBox(true).Min;
            Point3d maxBoundingBoxGeometry = inputMesh.GetBoundingBox(true).Max;

            //scale the bounding box based on input
            double boundingBoxWidth = maxBoundingBoxGeometry.X -
                minBoundingBoxGeometry.X;
            double boundingBoxHeight = maxBoundingBoxGeometry.Y -
                minBoundingBoxGeometry.Y;

            Interval xIntervalBaseMesh = new Interval(
                minBoundingBoxGeometry.X - 1,
                minBoundingBoxGeometry.X + boundingBoxWidth + 1);
            Interval yintervalBaseMesh = new Interval(
                minBoundingBoxGeometry.Y - 1,
                minBoundingBoxGeometry.Y + boundingBoxHeight + 1);

            //offset starter plane on z axis so that it does not interfer with
            Plane basePlane = new Plane(new Point3d(0, 0, 0.0001), zaxis);
            Mesh baseMesh = Mesh.CreateFromPlane(basePlane,
                xIntervalBaseMesh, yintervalBaseMesh, 1, 1);

            //deaulting to a white color. Could allow for specification of base color...
            //todo make this editable?
            baseMesh.VertexColors.CreateMonotoneMesh(
                Color.FromArgb(50, 250, 250, 250));

            return baseMesh;
        }


        public Mesh create2dMesh(double height, double width)
        {
            Interval xIntervalBaseMesh = new Interval(0, width);
            Interval yintervalBaseMesh = new Interval(0, height);

            //offset starter plane on z axis so that it does not interfer with
            //ground geometry. TODO: Take this as input 
            Plane basePlane = new Plane(new Point3d(0, 0, 0.01), zaxis);
            Mesh baseMesh = Mesh.CreateFromPlane(basePlane,
                xIntervalBaseMesh, yintervalBaseMesh, 1, 1);

            //deaulting to a white color. Could allow for specification of base color...
            //todo make this editable
            baseMesh.VertexColors.CreateMonotoneMesh(
                Color.FromArgb(255, 255, 255, 255));

            return baseMesh;
        }

        public Mesh createTextMesh(TextEntity textEntity,
            DimensionStyle dimensionStyle)
        {
            List<Mesh> listOfMeshes = new List<Mesh>();
            Brep[] breps = textEntity.CreateSurfaces(dimensionStyle, 1, 0);
            foreach (Brep b in breps)
            {
                Mesh[] meshArrayOfBreps = Mesh.CreateFromBrep(b, new MeshingParameters());
                foreach (Mesh m in meshArrayOfBreps)
                {
                    m.VertexColors.CreateMonotoneMesh(Config.BLACK_COLOR);
                    listOfMeshes.Add(m);
                }
            }
            Mesh finalMesh = new Mesh();
            foreach(Mesh m in listOfMeshes) { finalMesh.Append(m); }
            return finalMesh;
        }

        public Mesh getTitleTextMesh(string text, Mesh inputMesh,
            double textHeight, double offsetY)
        {
            //TODO make z depth adjustable
            DimensionStyle defaultDimensionStyle = new DimensionStyle();
            defaultDimensionStyle.TextHeight = textHeight;

            Point3d center_point_crop = new Point3d(
                inputMesh.GetBoundingBox(true).Min.X,
                inputMesh.GetBoundingBox(true).Max.Y + offsetY, 0.001);
            Plane plane_cropDli = new Plane(center_point_crop, zaxis);

            TextEntity textEntityDliCount = TextEntity.Create(text,
                plane_cropDli, defaultDimensionStyle, true,
                inputMesh.GetBoundingBox(true).Max.X -
                inputMesh.GetBoundingBox(true).Min.X, 0);

            Mesh finalMesh = createTextMesh(textEntityDliCount,
                defaultDimensionStyle);
            return finalMesh;
        }

        public Mesh getTitleTextMeshByPosition(string text,
            Point3d startTextPoint, double textHeight, double rectWidth)
        {
            //TODO make z depth adjustable
            DimensionStyle defaultDimensionStyle = new DimensionStyle();
            defaultDimensionStyle.TextHeight = textHeight;

            Point3d center_point_crop = new Point3d(startTextPoint);
            Plane plane_cropDli = new Plane(center_point_crop, zaxis);

            TextEntity textEntityDliCount = TextEntity.Create(text,
                plane_cropDli, defaultDimensionStyle, true, rectWidth, 0);

            Mesh finalMesh = createTextMesh(textEntityDliCount,
                defaultDimensionStyle);
            return finalMesh;
        }

        public Mesh createMeshFromPlane(Plane plane, double xStart, double xEnd,
            double yStart, double yend)
        {
            Interval xTileInterval = new Interval(xStart, xEnd);
            Interval yTileInterval = new Interval(yStart, yend);
            Mesh mesh = Mesh.CreateFromPlane(plane, xTileInterval,
                yTileInterval, 1, 1);
            return mesh;
        }

        private Plane createPlaneFromCenterpoint(Point3d centerpoint)
        {
            return new Plane(centerpoint, zaxis);
        }

        public Color colorMeshByColorStepping(double colorValueMultiplier, List<Color> colorRange)
        {
            double colorIndTemp = step;
            double p;
            double red;
            double green;
            double blue;
            Color minColor;
            Color maxColor;
            Color finalColor = new Color();
            for (int colorIndCount = 0; colorIndCount < colorRange.Count; colorIndCount++)
            {
                if (colorValueMultiplier <= colorIndTemp ||
                    (colorValueMultiplier == 1 &&
                    colorIndCount == (colorRange.Count - 1)))
                {
                    if (colorIndCount > 0)
                    {
                        minColor = colorRange[colorIndCount - 1];
                    }
                    else
                    {
                        minColor = colorRange[colorIndCount];
                    }

                    maxColor = colorRange[colorIndCount];
                    p = (colorValueMultiplier - (colorIndTemp - step)) /
                        (colorIndTemp - (colorIndTemp - step));
                    red = minColor.R * (1 - p) + maxColor.R * p;
                    green = minColor.G * (1 - p) + maxColor.G * p;
                    blue = minColor.B * (1 - p) + maxColor.B * p;
                    finalColor = Color.FromArgb(255, Convert.ToInt32(red),
                        Convert.ToInt32(green), Convert.ToInt32(blue));
                    break;
                }
                colorIndTemp += step;
            }

            return finalColor;
        }

        public void rotateSurfaceToTopView(Mesh mesh, double diagramRotation,
            Vector3d diagramRotationAxis)
        {
            Vector3d faceNrm = mesh.FaceNormals.First();
            if (faceNrm.IsPerpendicularTo(new Vector3d(0, 0, 1)))
            {
                Point3d cen = mesh.GetBoundingBox(true).Center;
                //todo or set perpendicular to itself?
                faceNrm.Rotate(1.5708, new Vector3d(0, 0, 1));
                //todo need to change this rotation for planes that are
                //not completely vertical
                mesh.Rotate(-1.5708, faceNrm, cen);

                //todo this needs to be seperated
                if (diagramRotation != 0)
                {
                    double radians = (Math.PI / 180) * diagramRotation;
                    mesh.Rotate(radians, diagramRotationAxis, cen);
                }
            }
        }
    }
}

