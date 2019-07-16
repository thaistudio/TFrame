using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB.ExtensibleStorage;


namespace TFrame.TTools
{
    public class BeamTools 
    {

        /// <summary>
        /// Retrieve all rebars of host
        /// </summary>
        /// <param name="host"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public List<Rebar> CacheRebars(Element host, Document doc)
        {
            List<Rebar> cachedRebars = new List<Rebar>();
            IEnumerable<Element> rebars = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rebar).WhereElementIsNotElementType().Where(x => x.LookupParameter("Host Mark").AsString() == ElementTools.GetMark(host));
            foreach (Rebar rebar in rebars)
            {
                
                cachedRebars.Add(rebar);
            }
            return cachedRebars;
        }


        /// <summary>
        /// Return 2 midpoints at 2 ends at top plane of a beam
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        static public List<XYZ> GetBeamEnds(Element e)
        {
            List<XYZ> ends = new List<XYZ>();
            FamilyInstance fi = (FamilyInstance)e;
            if (fi.StructuralType == Autodesk.Revit.DB.Structure.StructuralType.Beam)
            {
                Location locLine = e.Location;
                LocationCurve locCurve = (LocationCurve)locLine;
                Curve curve = locCurve.Curve;
                XYZ p0 = curve.GetEndPoint(0);
                XYZ p1 = curve.GetEndPoint(1);
                ends.Add(p0);
                ends.Add(p1);
            }
            return ends;
        }

        /// <summary>
        /// 0 - b; 1 - h
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static List<double> GetBeamDims(Element e)
        {
            List<double> dims = new List<double>();
            double width;
            double height;
            if (e is FamilyInstance)
            {
                FamilySymbol sb = ((FamilyInstance)e).Symbol;
                width = sb.LookupParameter("b").AsDouble();
                height = sb.LookupParameter("h").AsDouble();
                dims.Add(width);
                dims.Add(height);
                return dims;
            }
            else return null;
        }

        public double GetRebarCover(Element e, Document doc, BuiltInParameter builtInParameter)
        {
            ElementId coverId = e.get_Parameter(builtInParameter).AsElementId();
            Element coverElem = doc.GetElement(coverId);
            RebarCoverType rebarCoverType = (RebarCoverType)coverElem;
            double cover = rebarCoverType.CoverDistance;
            return cover;
        }

        public static double GetBeamLength(Element e)
        {
            double length = e.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
            return length;
        }

        public List<XYZ> GetBeamMidLine(Element e)
        {
            double coeff = 1;
            List<XYZ> points = new List<XYZ>();

            LocationCurve locationCurve = (LocationCurve)e.Location;
            Curve eCurve = locationCurve.Curve;

            XYZ p0 = eCurve.GetEndPoint(0);
            double p0X = p0.X * coeff;
            double p0Y = p0.Y * coeff;
            double p0Z = p0.Z * coeff;
            points.Add(p0);

            XYZ p1 = eCurve.GetEndPoint(1);
            double p1X = p1.X * coeff;
            double p1Y = p1.Y * coeff;
            double p1Z = p1.Z * coeff;
            points.Add(p1);

            return points;
        }

        public XYZ GetUnitParallelVector(XYZ p0, XYZ p1)
        {
            double x = p1.X - p0.X;
            double y = p1.Y - p0.Y;
            double z = p1.Z - p0.Z;
            double distance = Math.Sqrt(x * x + y * y + z * z);
            XYZ paralVector = new XYZ(x / distance, y / distance, z / distance);
            return paralVector;
        }

        public XYZ GetUnitNormalVector(XYZ p0, XYZ p1)
        {
            double x = p0.Y - p1.Y;
            double y = p1.X - p0.X;
            double z = p1.Z - p0.Z;
            double distance = Math.Sqrt(x * x + y * y + z * z);
            XYZ normalVector = new XYZ( x / distance, y / distance, z / distance);
            return normalVector;
        }

        /// <summary>
        /// 0-Top1; 1-Top2; 2-Top3; 3-Top4; 4-Bot1; 5-Bot2; 6-Bot3; 7-Bot4
        /// </summary>
        /// <param name="e"></param>
        /// <param name="doc"></param>
        public static List<XYZ> GetBeamBoundingPoints(Element e)
        {
            Document doc = e.Document;

            XYZ p0 = GetBeamEnds(e)[0];
            XYZ p1 = GetBeamEnds(e)[1];

            List<XYZ> boundingPoints = new List<XYZ>();

            double length = GetBeamLength(e);
            double beamWidth = GetBeamDims(e)[0];
            double l;
            double rotation = GeometryTools.GetRotation(e, true);
            XYZ max = e.get_BoundingBox(doc.ActiveView).Max;
            XYZ min = e.get_BoundingBox(doc.ActiveView).Min;

            double sinRot = Math.Sin(rotation);
            double cosRot = Math.Cos(rotation);

            XYZ top1, top2, top3, top4;
            XYZ bot1, bot2, bot3, bot4;

            double xt1, yt1, zt;
            double xt2, yt2;
            double xt3, yt3;
            double xt4, yt4;
            // p0\p1
            if (p0.X < p1.X && p0.Y > p1.Y) // p0\p1
            {
                l = length;
                xt1 = max.X - l * cosRot;
                yt1 = max.Y;

                xt2 = max.X;
                yt2 = max.Y - l * sinRot;

                xt3 = min.X + l * cosRot;
                yt3 = min.Y;

                xt4 = min.X;
                yt4 = min.Y + l * sinRot;
            }
            else if (p0.X > p1.X && p0.Y < p1.Y) // p1\p0
            {
                l = length;
                xt3 = max.X - l * sinRot;
                yt3 = max.Y;

                xt4 = max.X;
                yt4 = max.Y - l * cosRot;

                xt1 = min.X + l * sinRot;
                yt1 = min.Y;

                xt2 = min.X;
                yt2 = min.Y + l * cosRot;
            }
            else if (p0.X < p1.X && p0.Y < p1.Y) // p0/p1
            {
                l = beamWidth;
                xt2 = max.X - l * sinRot;
                yt2 = max.Y;

                xt3 = max.X;
                yt3 = max.Y - l * cosRot;

                xt4 = min.X + l * sinRot;
                yt4 = min.Y;

                xt1 = min.X;
                yt1 = min.Y + l * cosRot;
            }
            else // p1/p0
            {
                l = beamWidth;
                xt4 = max.X - l * cosRot;
                yt4 = max.Y;

                xt1 = max.X;
                yt1 = max.Y - l * sinRot;

                xt2 = min.X + l * cosRot;
                yt2 = min.Y;

                xt3 = min.X;
                yt3 = min.Y + l * sinRot;
            }

            zt = max.Z;
            top1 = new XYZ(xt1, yt1, zt);
            top2 = new XYZ(xt2, yt2, zt);
            top3 = new XYZ(xt3, yt3, zt);
            top4 = new XYZ(xt4, yt4, zt);

            double zb = min.Z;
            bot1 = new XYZ(xt1, yt1, zb);
            bot2 = new XYZ(xt2, yt2, zb);
            bot3 = new XYZ(xt3, yt3, zb);
            bot4 = new XYZ(xt4, yt4, zb);

            boundingPoints.Add(top1);
            boundingPoints.Add(top2);
            boundingPoints.Add(top3);
            boundingPoints.Add(top4);
            boundingPoints.Add(bot1);
            boundingPoints.Add(bot2);
            boundingPoints.Add(bot3);
            boundingPoints.Add(bot4);

            return boundingPoints;
        }

        public List<List<double>> GetBeamPlane(Element e, Document doc)
        {
            List<List<double>> planes = new List<List<double>>();

            List<XYZ> points = GetBeamBoundingPoints(e);
            XYZ top1 = points[0];
            XYZ top2 = points[1];
            XYZ top3 = points[2];
            XYZ top4 = points[3];
            XYZ bot1 = points[4];
            XYZ bot2 = points[5];
            XYZ bot3 = points[6];
            XYZ bot4 = points[7];

            GeometryTools tGeo = new GeometryTools();
            planes.Add(tGeo.GetPlaneFrom3Points(top1, top2, bot1)); //left
            planes.Add(tGeo.GetPlaneFrom3Points(top3, top4, bot3)); //right
            planes.Add(tGeo.GetPlaneFrom3Points(top3, top2, top1)); //top
            planes.Add(tGeo.GetPlaneFrom3Points(bot3, bot2, bot1)); //bot
            planes.Add(tGeo.GetPlaneFrom3Points(top3, top2, bot2)); //front
            planes.Add(tGeo.GetPlaneFrom3Points(top1, top4, bot1)); //back

            return planes;
        }


        /// <summary>
        /// Return points of any face of a beam
        /// </summary>
        /// <param name="e">Beam to find points</param>
        /// <param name="face">Face to find points</param>
        /// <returns></returns>
        public static List<XYZ> GetPointsOfFace(Element e, BeamFace face)
        {
            List<XYZ> points = new List<XYZ>(); // List of corner points of a face

            Face selFace = GetBeamFace(e, face); 
            EdgeArrayArray selArrayArray = selFace.EdgeLoops; // EdgeLoops property returns the exact points, in terms of coordinate

            foreach (EdgeArray edgeArray in selArrayArray) // Use loop to add 4 corner points to points
            {
                foreach (Edge edge in edgeArray)
                {
                    // Get 2 end points of each edge
                    Curve curve = edge.AsCurve();
                    XYZ p0 = curve.GetEndPoint(0);
                    XYZ p1 = curve.GetEndPoint(1);

                    if (points.Count == 0) points.Add(p0); // 1st iteration, just add c0

                    // Only add points that did not exist in points
                    if (!points.Contains(p1)) points.Add(p1); 
                    if (!points.Contains(p0)) points.Add(p0);
                }
            }

            return points;
        }

        /// <summary>
        /// Return any corner point of a beam. To get points, need to use GetPointsOfFace()
        /// The logic is in any face, define 2 lines that are normal to p0p1 (beam line) at 2 ends of the beam. Angle between those lines and points will be calculated. Corner points will create min or max angles. 
        /// </summary>
        /// <param name="e">The beam to get points</param>
        /// <param name="points">Use GetPointsOfFace. Use top/bot face as BeamFace(Face2/Face4)</param>
        /// <param name="point">The point to find</param>
        /// <returns></returns>
        public static XYZ GetBeamPoint(Element e, List<XYZ> points, BeamPoint point)
        {
            XYZ desiredPoint = new XYZ();

            // Oringinal end points of the beam
            XYZ p0o = GetBeamEnds(e)[0];
            XYZ p1o = GetBeamEnds(e)[1];

            // In some cases, i.e. beam connects to another beam at angle, min or max angles won't be the desired point. End points will be moved 1ft further to gurantee the max/min angle logic.
            XYZ p0 = GeometryTools.GetPointAlignToLineAtDistance(p1o, p0o, 1);
            XYZ p1 = GeometryTools.GetPointAlignToLineAtDistance(p0o, p1o, 1);

            XYZ p2 = GeometryTools.GetPointAtDistNormalToAVector(p1.Subtract(p0), p0, 1, p0.Z); // p0p1 _|_ p0p2
            XYZ p3 = GeometryTools.GetPointAtDistNormalToAVector(p0.Subtract(p1), p1, 1, p0.Z); // p0p1 _|_ p0p3

            if (point == BeamPoint.P0 || point == BeamPoint.P3) // 2 bottom points
            {
                List<XYZ> p03 = points.OrderBy(x => GeometryTools.AngleBetween2Vector(p0, p2, p0, x)).ToList();
                if (point == BeamPoint.P3) desiredPoint = p03.FirstOrDefault();
                if (point == BeamPoint.P0) desiredPoint = p03.LastOrDefault();
            }

            if (point == BeamPoint.P4 || point == BeamPoint.P7) // The other bottom points
            {
                List<XYZ> p47 = points.OrderBy(x => GeometryTools.AngleBetween2Vector(p1, p3, p1, x)).ToList(); // The angle is computed with p1p3 to return first and last members
                if (point == BeamPoint.P4) desiredPoint = p47.FirstOrDefault();
                if (point == BeamPoint.P7) desiredPoint = p47.LastOrDefault();
            }

            if (point == BeamPoint.P1 || point == BeamPoint.P2) // 2 top points
            {
                List<XYZ> p12 = points.OrderBy(x => GeometryTools.AngleBetween2Vector(p0, p2, p0, x)).ToList();
                if (point == BeamPoint.P2) desiredPoint = p12.FirstOrDefault();
                if (point == BeamPoint.P1) desiredPoint = p12.LastOrDefault();
            }

            if (point == BeamPoint.P5 || point == BeamPoint.P6) // The other bottom points
            {
                List<XYZ> p56 = points.OrderBy(x => GeometryTools.AngleBetween2Vector(p1, p3, p1, x)).ToList(); // The angle is computed with p1p3 to return first and last members
                if (point == BeamPoint.P5) desiredPoint = p56.FirstOrDefault();
                if (point == BeamPoint.P6) desiredPoint = p56.LastOrDefault();
            }
            return desiredPoint;
        }

        /// <summary>
        /// This method can get any face of an element. Faces are indexed in TNote
        /// </summary>
        /// <param name="e"></param>
        /// <param name="beamFace"></param>
        /// <returns></returns>
        public static Face GetBeamFace(Element e, BeamFace beamFace)
        {
            Face desiredFace = null; // Return this

            bool good = true; // Use for most conditions. Good means that the next if can be accessed
            bool good1 = true; // Need an extra bool for getting front and back faces. To get them, 2 bools are required

            XYZ p0 = GetBeamEnds(e)[0];
            XYZ p1 = GetBeamEnds(e)[1];
            XYZ p2 = GeometryTools.GetPointAtDistNormalToAVector(p1.Subtract(p0), p0, 1, p0.Z);
            double topElev = p0.Z;
            double botElev = topElev - GetBeamDims(e)[1];
            double angle = 0;

            Solid solid = GetSolidFromElement(e); // Get the solid that represents e

            foreach (Face face in solid.Faces) // Loop through all faces of solid
            {
                if (desiredFace != null) break; // Found the desired face. Let's escape
                Mesh mesh = face.Triangulate(1); // Get the mesh (triangles on each face)
                IList<XYZ> vertices = mesh.Vertices; // Get all points on each face

                // Get top face - Face 2. Definition: all Z == topElev
                if (beamFace == BeamFace.Face2)
                {
                    foreach (XYZ vertex in vertices)
                    {
                        if (Math.Abs(vertex.Z - topElev) > GlobalParams.Tolerence) // Move on if point does not belong to top face
                        {
                            good = false;
                            break;
                        }
                        good = true;
                    }
                    if (good) // If good is never set to false, they are the one
                    {
                        desiredFace = face;
                        break; // Found the face, let's get out of the faces loop
                    }
                }

                // Get bot face - Face 4. Definition: all Z == botElev
                if (beamFace == BeamFace.Face4)
                {
                    foreach (XYZ vertex in vertices)
                    {
                        if (Math.Abs(vertex.Z - botElev) > GlobalParams.Tolerence) // Move on if point does not belong to top face
                        {
                            good = false;
                            break;
                        }
                        good = true;
                    }
                    if (good) // If good is never set to false, they are the one
                    {
                        desiredFace = face;
                        break; // Found the face, let's get out of the faces loop
                    }
                }

                // Determine if this face is a side face a.k.a. not Face2 and Face4
                good = IsSideFace(vertices);
                
                // Get Face 3 or Face5 - Definition: all points make an angle of less than 90 degrees with vector v1v2
                if ((beamFace == BeamFace.Face3 || beamFace == BeamFace.Face5) && good)
                {
                    foreach (XYZ vertex in vertices) // Use this loop to determine which side is the side face
                    {
                        angle = UnitUtils.Convert(GeometryTools.AngleBetween2Vector(p0, p2, p0, vertex), DisplayUnitType.DUT_RADIANS, DisplayUnitType.DUT_DECIMAL_DEGREES);
                        // This is when we need to calculate angle to determine which side is this face
                        if (beamFace == BeamFace.Face3)
                        {
                            if (angle > 90) // This means not Face 3
                            {
                                good = false;
                                break; // Move on if any angle > 90, which means not Face 3
                            }
                            good = true;
                        }
                        if (beamFace == BeamFace.Face5)
                        {
                            if (angle < 90) // This means not Face 5
                            {
                                good = false;
                                break;// Move on if any angle > 90, which means not Face 3
                            }
                            good = true;
                        }
                    }
                    if (good) // To reach here, a face need to be: 1. Be a side face, 2. All points make an angle of less than 90 with p0p2 vector
                    {
                        desiredFace = face;
                        break;
                    }
                }

                // Get Face 1 or Face 2 - Definition: there are at least 2 points making an angle of greater and less than 90 degrees with vector v1v2. Project p0 to Face 1 and Face 2, smaller distance is Face 1, the other is Face 2
                if ((beamFace == BeamFace.Face0 || beamFace == BeamFace.Face1) && good)
                {
                    good = false;
                    good1 = false;
                    for (int i = 0; i < vertices.Count; i++)
                    {
                        if (good && good1) // When 2 points found, which means this face is either front or back
                        {
                            IntersectionResult intersection = face.Project(p0);
                            if (beamFace == BeamFace.Face0)
                            {
                                if (intersection == null) break; // Intersection only null if p0 can't be projected to the face, which means the face is too far and is Face1
                                else
                                {
                                    double dist = intersection.Distance; // Distance from p0 to the face
                                    if (dist < p0.DistanceTo(p1) / 2) // dist to Face0 is always found, limit |p0p1|/2 is just too safe
                                    {
                                        desiredFace = face;
                                        break;
                                    }
                                    else break; // If dist greater than half beam length, this face must be face 1. Next!
                                }
                            }
                            if (beamFace == BeamFace.Face1)
                            {
                                if (intersection == null) // Because p0 can always be projected to Face0, null means this face is Face1
                                {
                                    desiredFace = face;
                                    break;
                                }
                                else // If intersection is not null, we check the distance
                                {
                                    double dist = intersection.Distance;
                                    if (dist > p0.DistanceTo(p1) / 2) // If dist > |p0p1|/2, this face is definitely Face1
                                    {
                                        desiredFace = face;
                                        break;
                                    }
                                    else break; // Not Face1, next!
                                }
                            }
                        }

                        angle = UnitUtils.Convert(GeometryTools.AngleBetween2Vector(p0, p2, p0, vertices[i]), DisplayUnitType.DUT_RADIANS, DisplayUnitType.DUT_DECIMAL_DEGREES);
                        // The next 2 if find 2 points that make angles greater and smaller than 90 degree to p0p2
                        if (angle < 90) // First point found
                        {
                            desiredFace = null;
                            good = true;
                            continue;
                        }
                        if (angle > 90) // Second point found
                        {
                            good1 = true;
                            continue;
                        }
                    }
                }
            }

            return desiredFace;
        }


        /// <summary>
        /// Determine if this face is a side face a.k.a. not Face3 and Face4
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        static bool IsSideFace(IList<XYZ> vertices)
        {
            bool good = false;
            double z = 0;
            for (int i = 0; i < vertices.Count; i++) // Use this loop to determine if this is a side face
            {
                if (i > 0) // Do nothing in the first iteration
                {
                    if (Math.Abs(vertices[i].Z - z) < GlobalParams.Tolerence) continue; // This means 2 points are on the same plane. Thank you, next!
                    if (Math.Abs(vertices[i].Z - z) > GlobalParams.Tolerence) // This means there are different elevations and this face is a side face
                    {
                        good = true; // Only need 1 true to process to next condition
                        break;
                    }
                }
                z = vertices[i].Z; // Add Z value of the current point to compare to next point
            }
            return good;
        }

        /// <summary>
        /// Return a solid that represents element e
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        static Solid GetSolidFromElement(Element e)
        {
            Document doc = e.Document;
            Options options = new Options();
            options.ComputeReferences = true;
            options.IncludeNonVisibleObjects = true;
            options.DetailLevel = ViewDetailLevel.Fine;
            //options.View = doc.ActiveView;
            GeometryElement geoElem = e.get_Geometry(options);
            
            Solid solid = null;
            foreach (GeometryObject obj in geoElem)
            {
                // If a beam is not joined to any others, geoElem will return 2 lines and 1 GeometryInstance, which holds 2 solid and 1 line. 1 of the solid is the one describes our family instance.
                if (obj is GeometryInstance)
                {
                    GeometryInstance geometryInstance = (GeometryInstance)obj; // return obj to GeometryInstance
                    GeometryElement geoElem1 = geometryInstance.GetInstanceGeometry(); // geoElem1 holds 2 solids and 1 line as mention above.

                    foreach (GeometryObject obj1 in geoElem1)
                    {
                        solid = GetValidSolidFromGeoObj(obj1); // Get the valid solid with volume != 0
                        if (solid != null) break; // Found solid, let's run
                    }
                }
                else if (obj is Line) continue; // Next iteration if obj is a Line
                // Otherwise, if a beam is connected, geoElem will return 2 solids, 4 lines and 1 GeometryInstance. In this case, we will get the solid right here
                else
                {
                    solid = GetValidSolidFromGeoObj(obj);
                    if (solid != null) break; // Found solid, let's run
                }
                    
            }
            return solid;
        }

        /// <summary>
        /// Return a valid solid (volume != 0) from a GeometryObject
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        static Solid GetValidSolidFromGeoObj(GeometryObject obj)
        {
            if (obj is Solid) // Only allow solid
            {
                Solid solid = (Solid)obj;
                if (solid.Volume != 0) return solid; // Only allow valid solid
                else return null;
            }
            else return null;
        }


        //static IList<XYZ> GetVertices(GeometryObject obj)
        //{
        //    IList<XYZ> vertices = null;
        //    if (obj is Solid)
        //    {
        //        Solid solid = (Solid)obj;
        //        foreach (Face face in solid.Faces)
        //        {
        //            Reference r = face.Reference;
        //            Mesh mesh = face.Triangulate();
        //            vertices = mesh.Vertices;
        //        }
        //        foreach (Edge edge in solid.Edges)
        //        {
        //            Curve curve = edge.AsCurve();
        //            Reference r = curve.Reference;
        //            Face face0 = edge.GetFace(0);
        //            Face face1 = edge.GetFace(1);
        //            IList<XYZ> tes = edge.Tessellate();

        //        }
        //    }
        //}

        static void Investigate(GeometryObject obj)
        {
            if (obj is Solid)
            {
                Solid solid = (Solid)obj;
                foreach (Face face in solid.Faces)
                {
                    Reference r = face.Reference;
                    Mesh mesh = face.Triangulate();
                    IList<XYZ> vertices = mesh.Vertices;
                }
                foreach (Edge edge in solid.Edges)
                {
                    Curve curve = edge.AsCurve();
                    Reference r = curve.Reference;
                    Face face0 = edge.GetFace(0);
                    Face face1 = edge.GetFace(1);
                    IList<XYZ> tes = edge.Tessellate();
                }
            }
        }
    }


    /// <summary>
    /// See TNote for details
    /// </summary>
    public enum BeamFace
    {
        Face0 = 0, Face1 = 1, Face2, Face3 = 3, Face4 = 4, Face5 = 5
    }

    public enum BeamPoint
    {
        P0, P1, P2, P3, P4, P5, P6, P7, P8
    }

    public class TStoreData
    {
        public void AddInfoToElement<T>(Element e, T info, string fieldName, UnitType unitType, DisplayUnitType displayUnitType)
        {

            // Delete existing schemas
            IList<Guid> existingGuids = e.GetEntitySchemaGuids();
            if (existingGuids.Count > 0)
            {
                foreach (Guid existingGuid in existingGuids)
                {
                    Schema existingSchema = Schema.Lookup(existingGuid);
                    Entity existingEntity = e.GetEntity(existingSchema);
                    IList<Field> existingFields = existingSchema.ListFields();
                    if (existingSchema.SchemaName == fieldName) e.DeleteEntity(existingSchema);
                }
            }
            
            // Add new schemas
            Guid g = Guid.NewGuid();
            SchemaBuilder schemaBuilder = new SchemaBuilder(g);
            schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
            schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
            schemaBuilder.SetVendorId("THAISTUDIO");

            //Create a field
            FieldBuilder field = schemaBuilder.AddSimpleField(fieldName, typeof(T));
            field.SetDocumentation(fieldName);
            
            schemaBuilder.SetSchemaName(fieldName);

            if (info is double) field.SetUnitType(unitType);

           
            Schema schema = schemaBuilder.Finish();

            Entity entity = new Entity(schema);

            //Get the filed from the schema
            Field getField = schema.GetField(fieldName);

            if (info is string) entity.Set(getField, info, DisplayUnitType.DUT_UNDEFINED);
            else if (!(info is string)) entity.Set(getField, info, displayUnitType);

            e.SetEntity(entity);
        }

        public T GetInfoFromElement<T>(Element e, string fieldName, DisplayUnitType displayUnitType)
        {
            T v = default(T);
            var schemaGuids = e.GetEntitySchemaGuids();
            foreach (Guid schemaGuid in schemaGuids)
            {
                Schema schema = Schema.Lookup(schemaGuid);
                if (schema.SchemaName == fieldName)
                {
                    v = e.GetEntity(schema).Get<T>(fieldName, displayUnitType);
                    break;
                }
                else v = default(T);
            }
            return v;
        }
    }
}

