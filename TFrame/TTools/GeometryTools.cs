using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TFrame
{
    class GeometryTools
    {
        /// <summary>
        /// [0, 1, 2, 3] = a, b, c, d
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static List<double> GetPlaneFrom3Points(XYZ p0, XYZ p1, XYZ p2)
        {
            List<double> plane = new List<double>();
            XYZ sub1 = Subtract2Points(p0, p1);
            XYZ sub2 = Subtract2Points(p0, p2);
            double a = Multiply2Points(sub1, sub2).X;
            double b = Multiply2Points(sub1, sub2).Y;
            double c = Multiply2Points(sub1, sub2).Z;
            double d = -(p0.X * a + p0.Y * b + p0.Z * c);
            plane.Add(a);
            plane.Add(b);
            plane.Add(c);
            plane.Add(d);
            return plane;
        }

        public static XYZ Subtract2Points(XYZ p0, XYZ p1)
        {
            double x = p1.X - p0.X;
            double y = p1.Y - p0.Y;
            double z = p1.Z - p0.Z;
            XYZ subtacted = new XYZ(x, y, z);
            return subtacted;
        }

        public static XYZ Multiply2Points(XYZ p0, XYZ p1)
        {
            double x = p0.Y * p1.Z - p0.Z * p1.Y;
            double y = p0.Z * p1.X - p0.X * p1.Z;
            double z = p0.X * p1.Y - p0.Y * p1.X;
            XYZ multiplied = new XYZ(x, y, z);
            return multiplied;
        }

        public static double GetDistancePointToPlane(XYZ point, List<double> plane)
        {
            double distance = Math.Abs(plane[0] * point.X + plane[1] * point.Y + plane[2] * point.Z + plane[3])
                / (Math.Sqrt(plane[0] * plane[0] + plane[1] * plane[1] + plane[2] * plane[2]));

            return distance;
        }

        /// <summary>
        /// Return the distance from a point to a line made up from p0 and p1
        /// </summary>
        /// <param name="p2">Point to get distance from</param>
        /// <param name="p0">1st end point of the line</param>
        /// <param name="p1">2nd end point of the line</param>
        /// <returns></returns>
        public static double GetDistancePointToLine(XYZ p2, XYZ p0, XYZ p1)
        {
            // Get vectors p01 and p02
            XYZ p01 = new XYZ(p0.X - p1.X, p0.Y - p1.Y, p0.Z - p1.Z);
            XYZ p02 = new XYZ(p0.X - p2.X, p0.Y - p2.Y, p0.Z - p2.Z);

            // Calculate t of point p3 from line formula p0 + tp1. p1p3 orthogonal with p0p1
            double t = - (p02.X * p01.X + p02.Y * p01.Y + p02.Z * p01.Z) / (p01.X * p01.X + p01.Y * p01.Y + p01.Z * p01.Z);

            // Get p3
            XYZ p3 = new XYZ(p0.X + t * p01.X, p0.Y + t * p01.Y, p0.Z + t * p01.Z);

            // The distance from p2 to p01 is |p23|
            double distance = p3.DistanceTo(p2);
            return distance;
        }

        /// <summary>
        /// Return the angle formed from p0, p1 and X direction. Always return the smallest value
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        public static double GetAngleWithX(XYZ p0, XYZ p1)
        {
            XYZ vector = new XYZ(Math.Abs(p0.X - p1.X), Math.Abs(p0.Y - p1.Y), Math.Abs(p0.Z - p1.Z));
            double angle = vector.AngleTo(new XYZ(1, 0, 0));
            return angle;
        }

        /// <summary>
        /// Get a point beween 2 known points, provided the distance from that point to the end points
        /// </summary>
        /// <param name="p0">The first end point</param>
        /// <param name="p1">The second end point</param>
        /// <param name="distance">Distance between p0 and the looking point</param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static XYZ GetRelativePointBetween2Points(XYZ p0, XYZ p1, double distance)
        {
            XYZ lineEquation = GetLineEquationFrom2Points(p0, p1);
            double D = Math.Sqrt((Math.Pow(p0.X - p1.X, 2)) + (Math.Pow(p0.Y - p1.Y, 2)));
            double x2 = p0.X + distance * (p1.X - p0.X) / D;
            double y2 = p0.Y + distance * (p1.Y - p0.Y) / D;
            double z2 = ((x2 - p0.X) / lineEquation.X) * lineEquation.Z + p0.Z;
            XYZ p2 = new XYZ(x2, y2, z2);
            return p2;
        }

        /// <summary>
        /// X-l, Y-m, Z-n
        /// Line equation: (x-p0.X)/ l = (y-p0.Y) / m = (z-p0.Z) / n. 
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        public static XYZ GetLineEquationFrom2Points(XYZ p0, XYZ p1)
        {
            double l = p1.X - p0.X;
            double m = p1.Y - p0.Y;
            double n = p1.Z - p0.Z;
            XYZ lmn = new XYZ(l, m, n);
            return lmn;
        }

        public bool CreateLine(XYZ p0, XYZ p1, Autodesk.Revit.DB.Document doc)
        {
            Line line = Line.CreateBound(p0, p1);
            Curve curve = (Curve)line;
            Plane plane = Plane.CreateByThreePoints(p0, p1, new XYZ());
            SketchPlane sketchPlane = SketchPlane.Create(doc, plane);
            ModelCurve mLine = doc.Create.NewModelCurve(line, sketchPlane);
            return true;
        }


        /// <summary>
        /// Get rotation of either a beam or a column. For beams, always get the smallest angle with X dir, no matter the direction of the beams.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="IsBeams"></param>
        /// <returns></returns>
        public static double GetRotation(Element e, bool IsBeams)
        {
            double rotation;
            if (IsBeams)
            {
                LocationCurve lCurve = (LocationCurve)e.Location;
                Curve curve = lCurve.Curve;

                XYZ p0 = curve.GetEndPoint(0);
                SupportClassCor s = new SupportClassCor();
                s.GetCoordinate(p0);
                double p0X = p0.X;
                double p0Y = p0.Y;
                double p0Z = p0.Z;

                XYZ p1 = curve.GetEndPoint(1);
                double p1X = p1.X;
                double p1Y = p1.Y;
                double p1Z = p1.Z;

                XYZ vector = new XYZ(Math.Abs(p0X - p1X), Math.Abs(p0Y - p1Y), Math.Abs(p0Z - p1Z));
                rotation = vector.AngleTo(new XYZ(1, 0, 0));
            }
            else
            {
                LocationPoint locPoint = (LocationPoint)e.Location;
                rotation = locPoint.Rotation;
            }
            return rotation;
        }

        /// <summary>
        /// Get point p at dist from p0p1. pp0 _|_ p0p1
        /// p
        /// |
        /// ^ <--dist 
        /// |
        /// p0_____>___p1 
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="dist"></param>
        /// <returns></returns>
        public static XYZ GetPointNormalToLineAtDistance(XYZ p0, XYZ p1, double dist)
        {
            double a = (p1 - p0).X;
            double b = (p1 - p0).Y;
            double t = dist / (Math.Sqrt(a * a + b * b));
            double x = p0.X - b * t;
            double y = p0.Y + a * t;
            double z = p0.Z;
            XYZ p = new XYZ(x, y, z);
            return p;
        }


        /// <summary>
        /// Retrieve a point at distance l from lineOrigin (a point), perpendicular to beamVector
        /// </summary>
        /// <param name="beamVector"></param>
        /// <param name="lineOrigin">original point</param>
        /// <param name="l"></param>
        /// <param name="desiredZ"></param>
        /// <returns></returns>
        public static XYZ GetPointAtDistNormalToAVector(XYZ beamVector, XYZ lineOrigin, double l, double desiredZ)
        {
            double x, y;
            double torlerence = 0.0000001;
            if (Math.Abs(beamVector.X) < torlerence) 
            {
                y = lineOrigin.Y;
                x = lineOrigin.X - l;
            }
            else if (Math.Abs(beamVector.Y) < torlerence)
            {
                x = lineOrigin.X;
                y = lineOrigin.Y - l;
            }
            else
            {
                y = -l * beamVector.X / (Math.Sqrt(Math.Pow(beamVector.Y, 2) + Math.Pow(beamVector.X, 2))) + lineOrigin.Y;
                x = (-y + lineOrigin.Y) * beamVector.Y / beamVector.X + lineOrigin.X;
            }

            XYZ position = new XYZ(x, y, desiredZ);
            return position;
        }

        /// <summary>
        /// Return a point p at distance dist from p1. p0, p1, p align
        /// 
        /// p0____>____p1_____>____p
        ///                 dist   
        /// </summary>
        /// <param name="p0">1st point of the line</param>
        /// <param name="p1">2nd point of the line</param>
        /// <param name="dist">distance from p1 to p</param>
        /// <returns></returns>
        public static XYZ GetPointAlignToLineAtDistance(XYZ p0, XYZ p1, double dist)
        {
            double lineLength = p0.DistanceTo(p1);

            // A point of line p0p1 has coordinate: x = x0 + at; y = y0 + bt; z = z0 + zt
            // a = x1 - x0; b = y1 - y0; c = z1 - z0
            double a = p1.X - p0.X;
            double b = p1.Y - p0.Y;
            double c = p1.Z - p0.Z;
            double t = lineLength * (lineLength + dist) / (a * a + b * b + c * c);
            XYZ p = new XYZ(p0.X + a * t, p0.Y + b * t, p0.Z + c * t);

            return p;
        }

        /// <summary>
        /// Return a point p at distance dist from p2, pp2 // p0p1
        ///  p            p1
        ///  |            |
        ///  ^dist        ^  
        ///  |            |
        ///  p2           |
        ///               p0
        /// </summary>
        /// <param name="p0">1st point of the parallel vector</param>
        /// <param name="p1">2nd point of the parallel vector</param>
        /// <param name="p2">Point where p is distanced from</param>
        /// <param name="dist">distance from p2 to p</param>
        /// <returns></returns>
        public static XYZ GetPointParallelToLineAtDistance(XYZ p0, XYZ p1, XYZ p2, double dist)
        {
            double lineLength = p0.DistanceTo(p1);

            // A point of line p0p1 has coordinate: x = x0 + at; y = y0 + bt; z = z0 + zt
            // a = x1 - x0; b = y1 - y0; c = z1 - z0
            double a = p1.X - p0.X;
            double b = p1.Y - p0.Y;
            double c = p1.Z - p0.Z;
            double t = lineLength * dist / (a * a + b * b + c * c);
            XYZ p = new XYZ(p2.X + a * t, p2.Y + b * t, p2.Z + c * t);

            return p;
        }

        public static XYZ GetMidPoint(XYZ p0, XYZ p1)
        {
            double x = (p0.X + p1.X) / 2;
            double y = (p0.Y + p1.Y) / 2;
            double z = (p0.Z + p1.Z) / 2;
            XYZ midPoint = new XYZ(x, y, z);
            return midPoint;
        }


        /// <summary>
        /// Get angle between 2 vectors ab and xy
        /// </summary>
        /// <param name="a">Start point of vector ab</param>
        /// <param name="b">End point of vector ab</param>
        /// <param name="x">Start point of vector xy</param>
        /// <param name="y">End point of vector xy</param>
        /// <returns></returns>
        public static double AngleBetween2Vector(XYZ a, XYZ b, XYZ x, XYZ y)
        {
            XYZ u = b.Subtract(a);
            XYZ v = y.Subtract(x);
            double angle = u.AngleTo(v);
            return angle;
        }

        public static void Learn(List<Element> selBeams) 
        {
            #region use the following code to learn about API Geometry
            foreach (Element e in selBeams)
            {
                Document doc = e.Document;
                Options options = new Options();
                options.ComputeReferences = true;

                options.IncludeNonVisibleObjects = true;
                options.View = doc.ActiveView;
                GeometryElement v = e.get_Geometry(options);

                foreach (GeometryObject obj in v)
                {
                    if (obj is GeometryInstance)
                    {
                        GeometryInstance geometryInstance = (GeometryInstance)obj;
                        GeometryElement v1 = geometryInstance.GetInstanceGeometry();
                        GeometryElement v2 = geometryInstance.GetSymbolGeometry();

                        foreach (GeometryObject obj1 in v1)
                        {
                            Investigate(obj1);
                        }

                        foreach (GeometryObject obj2 in v2)
                        {
                            Investigate(obj2);
                        }
                    }

                    if (obj is Line)
                    {
                        int id = obj.GetHashCode();
                    }

                    Investigate(obj);

                }
            }

            void Investigate(GeometryObject obj)
            {
                if (obj is Solid)
                {
                    Solid solid = (Solid)obj;
                    foreach (Face face in solid.Faces)
                    {
                        BoundingBoxUV boundingBoxUV = face.GetBoundingBox();
                        Autodesk.Revit.DB.Reference r = face.Reference;
                        UV max = boundingBoxUV.Max;
                        UV min = boundingBoxUV.Min;
                        Mesh mesh = face.Triangulate();
                        IList<XYZ> vertices = mesh.Vertices;
                        int i = mesh.NumTriangles;
                    }
                    foreach (Edge edge in solid.Edges)
                    {
                        Curve curve = edge.AsCurve();
                        Autodesk.Revit.DB.Reference r = curve.Reference;
                        Face face0 = edge.GetFace(0);
                        Face face1 = edge.GetFace(1);
                        IList<XYZ> tes = edge.Tessellate();

                    }
                }
            }
            #endregion
        }

    }
}
