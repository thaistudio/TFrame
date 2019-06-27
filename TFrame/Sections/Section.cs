using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace TFrame.Sections
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
