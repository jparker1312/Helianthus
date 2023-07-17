using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace Helianthus
{
	public class IntersectionObject
	{
		private List<List<int>> intersectionMatrix;
        private List<List<double>> angleMatrix;
		private List<Ray3d> rayList;

		public IntersectionObject()
		{
			intersectionMatrix = new List<List<int>>();
			angleMatrix = new List<List<double>>();
			rayList = new List<Ray3d>();
		}

		public void setIntersectionMatrix(List<List<int>> matrix)
		{
			intersectionMatrix = matrix;
		}

		public void setAngleMatrix(List<List<double>> matrix)
		{
			angleMatrix = matrix;
		}

		public void setRayList(List<Ray3d> list)
		{
			rayList = list;
		}

		public List<List<int>> getIntersectionMatrix()
		{
			return intersectionMatrix;
		}

		public List<List<double>> getAngleMatrix()
		{
			return angleMatrix;
		}

		public List<Ray3d> getRayList()
		{
			return rayList;
		}
	}
}

