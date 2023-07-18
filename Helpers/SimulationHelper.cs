using System;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry.Collections;
using Rhino.Geometry.Intersect;

namespace Helianthus
{
	public class SimulationHelper
	{
        public static int[] TREGENZA_PATCHES_PER_ROW = { 30, 30, 24, 24, 18, 12, 6, 1 };
        public static double[] TREGENZA_COEFFICIENTS = {
            0.0435449227, 0.0416418006, 0.0473984151, 0.0406730411,
            0.0428934136, 0.0445221864, 0.0455168385, 0.0344199465 };
		public SimulationHelper()
		{
		}

        public List<double> computeFinalRadiationList(
            IntersectionObject intersectionObject,
            List<double> totalRadiationList)
        {
            //compute the results
            //pt_rel = array of ival(0 or 1) * cos(ang)
            //int_matrix appends above value (0 or cos(ang)
            //rad_result = sum of (pt_rel[](cos of an angle) * all_rad[](dir and diff combined)) ...what is the size of this array? think just 1 number. same size as matrix
            //results appends above values
            List<List<double>> finalIntersectionMatrix = new List<List<double>>();
            List<double> finalRadiationList = new List<double>();

            for(int i = 0; i < intersectionObject.getIntersectionMatrix().Count; i++)
            {
                List<int> tempIntList = intersectionObject.getIntersectionMatrix()[i];
                List<double> tempAngleList = intersectionObject.getAngleMatrix()[i];
                List<double> intersectionCalculationList = new List<double>();

                for(int i2 = 0; i2 < tempAngleList.Count; i2++)
                {
                    intersectionCalculationList.Add(tempIntList[i2] * Math.Cos(tempAngleList[i2]));
                }
                finalIntersectionMatrix.Add(intersectionCalculationList);

                double radiationResult = 0;
                for(int i3 = 0; i3 < intersectionCalculationList.Count; i3++)
                {
                    radiationResult += intersectionCalculationList[i3] * totalRadiationList[i3];
                }
                finalRadiationList.Add(radiationResult);
            }

            return finalRadiationList;
        }

		public Mesh meshMainGeometryWithContext(Brep geometryInput, List<Brep> contextGeometryInput)
		{
            // mesh together the geometry and the context
            contextGeometryInput.Append(geometryInput);
            Brep mergedContextGeometry = new Brep();
            foreach(Brep b in contextGeometryInput)
            {
                mergedContextGeometry.Append(b);
            }
            Mesh[] contextMeshArray = Mesh.CreateFromBrep(
                mergedContextGeometry, new MeshingParameters());
            Mesh contextMesh = new Mesh();
            foreach(Mesh m in contextMeshArray){ contextMesh.Append(m); }

			return contextMesh;
		}

        public Mesh createJoinedMesh(Brep geometryInput)
        {
            //create gridded mesh from geometry
            //how: using geometry and grid size: use default size for now (just say 1 for now. trial with 2). then output study mesh
            //ladybug is creating Ladybug Mesh3D and we want to stay with rhino since we dont want our own geometry lib
            //right now we assume 1 geometry and not a list as input
            //not assuming any offset curretly
            MeshingParameters meshingParameters = new MeshingParameters();
            meshingParameters.MaximumEdgeLength = 1;
            meshingParameters.MinimumEdgeLength = 1;
            meshingParameters.GridAspectRatio = 1;

            Mesh[] griddedMeshArray = Mesh.CreateFromBrep(
                geometryInput, meshingParameters);
            Mesh meshJoined = new Mesh();
            foreach(Mesh m in griddedMeshArray){ meshJoined.Append(m); }
            meshJoined.FaceNormals.ComputeFaceNormals();

            return meshJoined;
        }

        public List<Point3d> getMeshJoinedPoints(Mesh meshJoined)
        {
            //add offset distance for all points representing the faces of the
            //gridded mesh
            List<Point3d> points = new List<Point3d>();
            for(int i = 0; i < meshJoined.Faces.Count; i++)
            {
                Point3d tempPoint = meshJoined.Faces.GetFaceCenter(i);
                Vector3d tempVector = meshJoined.FaceNormals[i];
                tempVector = tempVector * .1;  
                points.Add(new Point3d(tempPoint.X + tempVector.X,
                                       tempPoint.Y + tempVector.Y,
                                       tempPoint.Z + tempVector.Z));
            }

            return points;
        }

        public List<Vector3d> getAllVectors(Mesh tragenzaDomeMesh)
        {
            //not doing north calculation. relying on user to orient north correctly
            List<Vector3d> allVectors = new List<Vector3d>();
            foreach(Vector3d normal in tragenzaDomeMesh.FaceNormals)
            {
                allVectors.Add(normal);
            }
            allVectors.RemoveRange(allVectors.Count - TREGENZA_PATCHES_PER_ROW[6],
                TREGENZA_PATCHES_PER_ROW[6]);
            allVectors.Add(new Vector3d(0, 0, 1));
            //should have 145 after this

            //lb_grnd_vecs = lb_vecs in reverse (not sure what this is for...
            //doesn't make sense in my head right now)
            List<Vector3d> groundVectors = new List<Vector3d>();
            foreach (Vector3d vec in allVectors)
            {
                vec.Reverse();
                groundVectors.Add(vec);
            }
            allVectors.AddRange(groundVectors);

            return allVectors;
        }

        public Mesh getTragenzaDome()
        {
            // compute constants to be used in the generation of patch points
            // patch row count is just the tragenza patch row counts. not subdivided in our case

            //not sure about all of these
            Vector3d base_vec = new Vector3d(0, 1, 0);
            Vector3d rotateAxis = new Vector3d(1, 0, 0);
            //should be 15
            double vertical_angle = Math.PI / (2 * TREGENZA_PATCHES_PER_ROW.Length - 1);

            // loop through the patch values and generate points for each vertex
            List<Point3d> vertices = new List<Point3d>();
            List<MeshFace> faces = new List<MeshFace>();

            int pt_i = -2;
            for(int row_count = 0; row_count < TREGENZA_PATCHES_PER_ROW.Length - 1; row_count++)
            {
                pt_i += 2;
                double horizontal_angle = -2 * Math.PI / TREGENZA_PATCHES_PER_ROW[row_count];
                Vector3d vec01 = new Vector3d(base_vec);
                vec01.Rotate(vertical_angle * row_count, rotateAxis);
                Vector3d vec02 = new Vector3d(vec01);
                vec02.Rotate(vertical_angle, rotateAxis);

                double correctionAngle = -horizontal_angle / 2;
                Vector3d vec1 = new Vector3d(vec01);
                vec1.Rotate(correctionAngle, new Vector3d(0, 0, 1));
                Vector3d vec2 = new Vector3d(vec02);
                vec2.Rotate(correctionAngle, new Vector3d(0, 0, 1));

                vertices.Add(new Point3d(vec1));
                vertices.Add(new Point3d(vec2));

                for(int patchCount = 0; patchCount < TREGENZA_PATCHES_PER_ROW[row_count]; patchCount++)
                {
                    Vector3d vec3 = new Vector3d(vec1);
                    vec3.Rotate(horizontal_angle, new Vector3d(0, 0, 1));
                    Vector3d vec4 = new Vector3d(vec2);
                    vec4.Rotate(horizontal_angle, new Vector3d(0, 0, 1));
                    vertices.Add(new Point3d(vec3));
                    vertices.Add(new Point3d(vec4));
                    faces.Add(new MeshFace(pt_i, pt_i + 1, pt_i + 3, pt_i + 2));
                    pt_i += 2;
                    vec1 = vec3;
                    vec2 = vec4;
                }
            }

            int endVertI = vertices.Count;
            int startVertI = vertices.Count -
                TREGENZA_PATCHES_PER_ROW[TREGENZA_PATCHES_PER_ROW.Length - 2] * 2 - 1;
            vertices.Add(new Point3d(0, 0, 1));

            for (int patchCount = 0; patchCount <
                TREGENZA_PATCHES_PER_ROW[TREGENZA_PATCHES_PER_ROW.Length - 2] * 2;
                patchCount += 2)
            {
                faces.Add(new MeshFace(startVertI + patchCount, endVertI,
                    startVertI + patchCount + 2));
            }

            Mesh patch_mesh = new Mesh();
            patch_mesh.Faces.AddFaces(faces);
            patch_mesh.Vertices.AddVertices(vertices);
            patch_mesh.FaceNormals.ComputeFaceNormals();

            return patch_mesh;
        }

        public IntersectionObject intersectMeshRays(
        Mesh contextMesh, List<Point3d> points, List<Vector3d> allVectors,
        MeshFaceNormalList joinedMeshNormals)
        {
            //both matrixes, so might need to change format slightly
            IntersectionObject intersectionObject = new IntersectionObject();
            List<List<int>> intersectionMatrix = new List<List<int>>();
            List<List<double>> angleMatrix = new List<List<double>>();
            double cutoffAngle = Math.PI / 2;

            //todo make this process run with parellel processing in the future??
            for(int i = 0; i < points.Count; i++)
            {
                List<int> intList = new List<int>();
                List<double> angleList = new List<double>();
                Point3d point = points[i];
                Vector3d normalVector = joinedMeshNormals[i];

                foreach(Vector3d vec in allVectors)
                {
                    double vectorAngle = Vector3d.VectorAngle(normalVector, vec);
                    angleList.Add(vectorAngle);

                    if(vectorAngle <= cutoffAngle)
                    {
                        if(Intersection.MeshRay(contextMesh, new Ray3d(point, vec)) >= 0)
                        {
                            intList.Add(0);
                        }
                        else { intList.Add(1); }
                    }
                    //the vector is pointing below the surface
                    else { intList.Add(0); }
                }
                intersectionMatrix.Add(intList);
                angleMatrix.Add(angleList);
            }

            intersectionObject.setIntersectionMatrix(intersectionMatrix);
            intersectionObject.setAngleMatrix(angleMatrix);

            return intersectionObject; 
        }
	}
}

