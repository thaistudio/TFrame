using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace TFrame
{
    public class Section
    {
        private double _L;
        private string _ViewSectionName;

        public double L
        {
            get
            {
                return _L;
            }
            set
            {
                _L = value;
                Location = "L/" + Convert.ToString(_L);
            }
        }
        public string ViewSectionName  // This is the Name property of ViewSection, but it can be set. ViewSection.Name can only get
        {
            get
            {
                return _ViewSectionName;
            }
            set
            {
                _ViewSectionName = value;
                Suffix = Convert.ToInt32(_ViewSectionName.Split(' ')[1]);
                Prefix = _ViewSectionName.Split(' ')[0];
            }
        }
        public int Suffix { get; set; }
        public string Prefix { get; set; }

        public bool IsRelative { get; set; }

        public bool IsManualNamed { get; set; }

        public string Location { get; set; }
        public double d { get; set; }
        public double Offset { get; set; }
        public BoundingBoxXYZ BoundingBox { get; set; }
        public XYZ Origin { get; set; }
        public Transform Tran { get; set; }
        public Autodesk.Revit.DB.View Template { get; set; }
        public ViewSection ViewSection { get; set; } // This property is used to get the ViewSection for create and delete purpose
        public ViewFamilyType ViewFamilyType { get; set; }
        public string Host { get; set; }

        // Coordinates of the bounding box
        public double XMax { get; set; }
        public double YMax { get; set; }
        public double ZMax { get; set; }
        public double XMin { get; set; }
        public double YMin { get; set; }
        public double ZMin { get; set; }
        public double XMaxExtra { get; set; } = 1;
        public double YMaxExtra { get; set; } = 1;
        public double ZMaxExtra { get; set; } = 1;
        public double XMinExtra { get; set; } = 1;
        public double YMinExtra { get; set; } = 1;
        public double ZMinExtra { get; set; } = 0;
        public double XMaxManual { get; set; } = 2;
        public double YMaxManual { get; set; } = 2;
        public double ZMaxManual { get; set; } = 2;
        public double XMinManual { get; set; } = 2;
        public double YMinManual { get; set; } = 2;
        public double ZMinManual { get; set; } = 2;

        // These are the properties needed for dimensioning the section
        public List<DimensionSet> DimensionSets { get; set; }
        public Element HostElement { get; set; }
        public View RevitView { get; set; }
        public string Name { get; set; }
        public BoundingBoxXYZ ViewerBoundingBox { get; set; }
        public Element Viewer { get; set; }

        public void SetDimensionProperties()
        {
            DimensionSets = new List<DimensionSet>();
            Name = ViewSection.Name;
            RevitView = ViewSection;
            HostElement = ElementTools.GetElementByMarkInView(ViewSection.LookupParameter("T_Mark").AsString(), ViewSection);
            Viewer = new FilteredElementCollector(ViewSection.Document).OfCategory(BuiltInCategory.OST_Viewers).Where(x => x.Name == Name).FirstOrDefault();

            Transaction t = new Transaction(ViewSection.Document);
            t.Start("CropBox");
            ViewSection.CropBoxVisible = true; // Crop box has to be visible to get viewer's bounding box
            t.Commit();

            ViewerBoundingBox = Viewer.get_BoundingBox(ViewSection);
        }

        public void AddDimensionSet(DimensionSet dimensionSet)
        {
            DimensionSets.Add(dimensionSet);
        }

        public XYZ GetSectionPoint(Point point)
        {
            XYZ p0 = GeometryTools.GetPointParallelToLineAtDistance(XYZ.Zero, XYZ.BasisZ.Negate(), ViewerBoundingBox.Max, ViewerBoundingBox.Max.Z - ViewerBoundingBox.Min.Z);
            XYZ p1 = ViewerBoundingBox.Max;
            XYZ p2 = GeometryTools.GetPointParallelToLineAtDistance(XYZ.Zero, XYZ.BasisZ, ViewerBoundingBox.Min, ViewerBoundingBox.Max.Z - ViewerBoundingBox.Min.Z);
            XYZ p3 = ViewerBoundingBox.Min;
            XYZ p4 = GeometryTools.GetPointNormalToLineAtDistance(p0, p3, ZMaxExtra);
            XYZ p5 = GeometryTools.GetPointNormalToLineAtDistance(p1, p2, ZMaxExtra);
            XYZ p6 = GeometryTools.GetPointNormalToLineAtDistance(p2, p1, -ZMaxExtra);
            XYZ p7 = GeometryTools.GetPointNormalToLineAtDistance(p3, p0, -ZMaxExtra);
            if (point == Point.P0) return p0;
            if (point == Point.P1) return p1;
            if (point == Point.P2) return p2;
            if (point == Point.P3) return p3;
            if (point == Point.P4) return p4;
            if (point == Point.P5) return p5;
            if (point == Point.P6) return p6;
            if (point == Point.P7) return p7;
            return null;
        }
    }

    public class FieldClass // Everything shown in the form is from this class
    {
        // Fields to be saved
        public static int ViewSectionIndex { get; set; } = 0; // Index of view section last selection
        public static int ViewTempIndex { get; set; } = 4; // Index of view template last selection
        public static List<Section> Sections { get; set; } // Setions
        public static int FieldLocation { get; set; } // Location

        // bool fields to determined which button is hit
        public static bool IsLoaded { get; set; }
        public static bool IsDeleted { get; set; }
        public static bool IsCleared { get; set; }
        public static bool IsRadioClicked { get; set; } = false;
        public static bool IsRelativeDistanceClicked { get; set; }
        public static bool IsAdded { get; set; } = false;
        public static bool IsOK { get; set; }
        public static bool IsBBManual { get; set; }
        public static bool IsBBAuto { get; set; }
        public static bool IsSuffEdited { get; set; }
        public static bool IsUpdated { get; set; }
    }
}
