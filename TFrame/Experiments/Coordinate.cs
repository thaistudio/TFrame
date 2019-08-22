using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace TFrame
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Coordinate : IExternalCommand
    {
        double coeff = 1; // 1 ft = 304.8 mm
        public bool IsColumns;
        public bool IsBeams;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Autodesk.Revit.DB.Document doc = commandData.Application.ActiveUIDocument.Document;
                UIDocument uIDoc = commandData.Application.ActiveUIDocument;
                Selection sel = uIDoc.Selection;

                View view = doc.ActiveView;
                FilteredElementCollector beams = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming).WhereElementIsNotElementType();
                ICollection<ElementId> eIds = sel.GetElementIds();

                foreach (ElementId eId in eIds)
                {
                    Element e = doc.GetElement(eId);
                    //AnalyticalModel analyticalModel = e.GetAnalyticalModel();
                    //Curve eCurve = analyticalModel.GetCurve();
                    Category cat = e.Category;
                    Category strFramingCat = Category.GetCategory(doc, BuiltInCategory.OST_StructuralFraming);
                    Category strColCat = Category.GetCategory(doc, BuiltInCategory.OST_StructuralColumns);
                    Category lineCat = Category.GetCategory(doc, BuiltInCategory.OST_Lines);
                    if (cat.Name == strFramingCat.Name) // Structural Framing
                    {
                        IsBeams = true;
                        IsColumns = false;

                        LocationCurve locationCurve = (LocationCurve)e.Location;
                        Curve eCurve = locationCurve.Curve;

                        XYZ p0 = eCurve.GetEndPoint(0);
                        SupportClassCor s = new SupportClassCor();
                        s.GetCoordinate(p0);
                        double p0X = p0.X * coeff;
                        double p0Y = p0.Y * coeff;
                        double p0Z = p0.Z * coeff;

                        XYZ p1 = eCurve.GetEndPoint(1);
                        double p1X = p1.X * coeff;
                        double p1Y = p1.Y * coeff;
                        double p1Z = p1.Z * coeff;

                        //GetMidLineFromBBox mc = new GetMidLineFromBBox();
                        //mc.GetMidCurve(e.get_BoundingBox(view), e, IsBeams);

                        //using (Transaction t = new Transaction(doc, "shit"))
                        //{
                        //    t.Start();
                        //    mc.CreateLine(mc.A, mc.B, doc);
                        //    mc.CreateLine(mc.B, mc.C, doc);
                        //    mc.CreateLine(mc.C, mc.D, doc);
                        //    mc.CreateLine(mc.D, mc.A, doc);

                        //    mc.CreateLine(mc.M, mc.N, doc);
                        //    mc.CreateLine(mc.N, mc.P, doc);
                        //    mc.CreateLine(mc.P, mc.Q, doc);
                        //    mc.CreateLine(mc.Q, mc.M, doc);

                        //    mc.CreateLine(mc.O1, mc.O2, doc);
                        //    t.Commit();
                        //}

                        TaskDialog.Show("Coordinate0", "Point 0: " + p0X + ", " + p0Y + ", " + p0Z + "\r\n" + "Point 1: " + p1X + ", " + p1Y + ", " + p1Z);
                        //TaskDialog.Show("Coordinate", "Point 0: " + mc.O1.X * coeff + ", " + mc.O1.Y * coeff + ", " + mc.O1.Z * coeff + "\r\n"
                            //+ "Point 1: " + mc.O2.X * coeff + ", " + mc.O2.Y * coeff + ", " + mc.O2.Z * coeff);
                    }
                    else if (cat.Name == strColCat.Name) // Structural Columns
                    {
                        IsColumns = true;
                        IsBeams = false;

                        double baseOffset = e.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM).AsDouble();
                        double topOffset = e.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).AsDouble();

                        var x = e.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM).Id;

                        CoordinateGeometry mc = new CoordinateGeometry();
                        mc.GetMidCurve(e.get_BoundingBox(view), e, IsBeams, doc);

                        using (Transaction t = new Transaction(doc, "shit"))
                        {
                            t.Start();
                            //mc.CreateLine(mc.A, mc.B, doc);
                            //mc.CreateLine(mc.B, mc.C, doc);
                            //mc.CreateLine(mc.C, mc.D, doc);
                            //mc.CreateLine(mc.D, mc.A, doc);

                            //mc.CreateLine(mc.M, mc.N, doc);
                            //mc.CreateLine(mc.N, mc.P, doc);
                            //mc.CreateLine(mc.P, mc.Q, doc);
                            //mc.CreateLine(mc.Q, mc.M, doc);

                            mc.CreateLine(mc.O1, mc.O2, doc);
                            t.Commit();
                        }

                        TaskDialog.Show("Coordinate1", "Point 0: " + mc.O1.X * coeff + ", " + mc.O1.Y * coeff + ", " + (mc.O1.Z + topOffset) * coeff + "\r\n" 
                            + "Point 1: " + mc.O2.X * coeff + ", " + mc.O2.Y * coeff + ", " + (mc.O2.Z + baseOffset) * coeff);
                    }
                    else if (cat.Name == lineCat.Name) // Lines
                    {
                        CurveElement curveElement = (CurveElement)e;
                        Curve curve = curveElement.GeometryCurve;

                        XYZ p0 = curve.GetEndPoint(0);
                        SupportClassCor s = new SupportClassCor();
                        s.GetCoordinate(p0);
                        double p0X = p0.X * coeff;
                        double p0Y = p0.Y * coeff;
                        double p0Z = p0.Z * coeff;

                        XYZ p1 = curve.GetEndPoint(1);
                        double p1X = p1.X * coeff;
                        double p1Y = p1.Y * coeff;
                        double p1Z = p1.Z * coeff;

                        TaskDialog.Show("Coordinate2", "Point 0: " + p0X + ", " + p0Y + ", " + p0Z + "\r\n" + "Point 1: " + p1X + ", " + p1Y + ", " + p1Z);
                    }
                    else
                        TaskDialog.Show("Selection", "Please select a beam");
                    
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Coordinate Error", ex.Message);
                return Result.Failed;
            }
            return Result.Succeeded;
        }

       
    }

    public class SupportClassCor
    {
        private double x;
        private double y;
        private double z;

        public double X { get => x; }
        public double Y { get => y; }
        public double Z { get => z; }

        public bool GetCoordinate(XYZ point)
        {
            x = point.X;
            y = point.Y;
            z = point.Z;
            return true;
        }
    }

    /// <summary>
    /// Use this class to calculate coordinate, mid-curve, ...
    /// </summary>
    public class CoordinateGeometry
    {
        double coeff = 1; // 1 ft = 304.8 mm
        double ColHeight;
        double xB, yB, zB, xC, yC, zC, xD, yD, zD, xN, yN, zN, xP, yP, zP, xQ, yQ, zQ;
        private XYZ cQ;

        public double Rotation { get; private set; }
        public XYZ A { get; private set; }
        public XYZ B { get; private set; }
        public XYZ C { get; private set; }
        public XYZ D { get; private set; }
        public XYZ M { get; private set; }
        public XYZ N { get; private set; }
        public XYZ P { get; private set; }
        public XYZ Q { get => cQ; }

        public XYZ O1 { get; private set; }
        public XYZ O2 { get; private set; }

        public double GetRotation(Element e, bool IsBeams)
        {
            if (IsBeams)
            {
                LocationCurve lCurve = (LocationCurve)e.Location;
                Curve curve = lCurve.Curve;

                XYZ p0 = curve.GetEndPoint(0);
                SupportClassCor s = new SupportClassCor();
                s.GetCoordinate(p0);
                double p0X = p0.X * coeff;
                double p0Y = p0.Y * coeff;
                double p0Z = p0.Z * coeff;

                XYZ p1 = curve.GetEndPoint(1);
                double p1X = p1.X * coeff;
                double p1Y = p1.Y * coeff;
                double p1Z = p1.Z * coeff;

                XYZ vector = new XYZ(Math.Abs(p0X - p1X), Math.Abs(p0Y - p1Y), Math.Abs(p0Z - p1Z));
                Rotation = vector.AngleTo(new XYZ(1, 0, 0));
            }
            else if (!IsBeams)
            {
                LocationPoint locPoint = (LocationPoint)e.Location;
                Rotation = locPoint.Rotation;
            }
            return Rotation;
        }

        public bool GetMidCurve(BoundingBoxXYZ bb, Element e, bool IsBeams, Autodesk.Revit.DB.Document doc)
        {
            A = bb.Max; // Max point
            M = bb.Min; // Min point

            // Get rotation
            GetRotation(e, IsBeams);
            double rotation = Rotation;

            // Get length (b), width (a) and height (h a.k.a column height) of the bounding box
            FamilyInstance famIns = (FamilyInstance)e;
            FamilySymbol famSym = famIns.Symbol;
            double a = famSym.LookupParameter("bf").AsDouble();
            double b = famSym.LookupParameter("d").AsDouble();
            double h = Math.Abs(A.Z - M.Z);
            ColHeight = h;

            // Get other points on top plane, i.e B, C, D and midpoint O1
            xB = A.X - a * Math.Cos(rotation);
            yB = A.Y - a * Math.Sin(rotation);
            zB = A.Z;
            B = new XYZ(xB, yB, zB);

            xC = M.X;
            yC = M.Y;
            zC = A.Z;
            C = new XYZ(xC, yC, zC);

            xD = A.X - b * Math.Sin(rotation);
            yD = A.Y - b * Math.Cos(rotation);
            zD = A.Z;
            D = new XYZ(xD, yD, zD);

            O1 = new XYZ((A.X + xC) / 2, (A.Y + yC) / 2, A.Z);

            // Get other points on bottom plane, i.e N, P, Q
            xN = M.X + a * Math.Cos(rotation);
            yN = M.Y - a * Math.Sin(rotation);
            zN = M.Z;
            N = new XYZ(xN, yN, zN);

            xP = A.X;
            yP = A.Y;
            zP = M.Z;
            P = new XYZ(xP, yP, zP);

            xQ = M.X + b * Math.Sin(rotation);
            yQ = M.Y + b * Math.Cos(rotation);
            zQ = M.Z;
            cQ = new XYZ(xQ, yQ, zQ);

            O2 = new XYZ((M.X + xP) / 2, (M.Y + yP) / 2, M.Z);

            return true;
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
    }
}
