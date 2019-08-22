using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace TFrame
{
    public class GridTools
    {
        /// <summary>
        /// Return horizontal or vertical grids from a list of grids
        /// </summary>
        /// <param name="grids"></param>
        /// <returns></returns>
        public static List<Grid> GetGridsByDirection(Document doc, GridOrientation gridLine)
        {
            FilteredElementCollector gridFilter = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Grids).WhereElementIsNotElementType();

            List<Grid> horGrids = new List<Grid>();
            List<Grid> verGrids = new List<Grid>();

            if (gridFilter.Count() == 0) return null;

            foreach (Grid grid in gridFilter)
            {
                // Get max and min points with correct Z
                XYZ maxPoint = GetCorrectMinMaxPoints(grid)[0];
                XYZ minPoint = GetCorrectMinMaxPoints(grid)[1];

                // Get angle of the grid
                double angle = GeometryTools.GetAngleWithX(maxPoint, minPoint);

                if (angle < Math.PI / 4) horGrids.Add(grid);
                else verGrids.Add(grid);
            }
            if (gridLine == GridOrientation.Horizontal) return horGrids;
            else return verGrids;
        }

        /// <summary>
        /// Return max and min point of a grid with correct Z. The points from Revit do not have correct value. 0-Max, 1-Min.
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        public static  List<XYZ> GetCorrectMinMaxPoints(Grid grid)
        {
            List<XYZ> points = new List<XYZ>();

            // Get 2 end points of the grid
            Outline outline = grid.GetExtents();
            XYZ maxPoint = outline.MaximumPoint;
            XYZ minPoint = outline.MinimumPoint;

            // Get max point with correct Z
            XYZ location = ((Line)grid.Curve).Origin;
            double correctZ = location.Z;
            XYZ maxPoint1 = new XYZ(maxPoint.X, maxPoint.Y, correctZ);
            XYZ minPoint1 = new XYZ(minPoint.X, minPoint.Y, correctZ);

            points.Add(maxPoint1);
            points.Add(minPoint1);

            return points;
        }

        /// <summary>
        /// Move all grid from List<Element> to List<Grid>
        /// </summary>
        /// <param name="elems"></param>
        /// <returns></returns>
        public static List<Grid> ListElemsToListGrids(List<Element> elems)
        {
            List<Grid> grids = new List<Grid>();
            foreach (Element elem in elems)
            {
                grids.Add((Grid)elem);
            }
            return grids;
        }
    }

    public enum GridOrientation
    {
        Horizontal,
        Vertical
    }
}

