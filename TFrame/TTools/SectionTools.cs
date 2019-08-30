using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TFrame
{
    public class SectionTools 
    {
        static string TName = "T_SectionName";
        static string ViewSectionName = "View Name";

        ExternalCommandData _commandData;


        public SectionTools(ExternalCommandData commandData)
        {
            _commandData = commandData;
        }

        BeamTools tAct = new BeamTools();
        double FtToMm = GlobalParams.FtToMm;

        /// <summary>
        /// Use this class to cache all ViewSection belong to an element
        /// </summary>
        /// <param name="e"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static List<ViewSection> CacheViewSections(Element e, Document doc)
        {
            List<ViewSection> CachedSections = new List<ViewSection>();
            string mark = ElementTools.GetMark(e);
            var viewElements = new FilteredElementCollector(doc).OfClass(typeof(ViewSection)).Where(x => x.LookupParameter("T_Mark").AsString() == mark).Where(x => x.LookupParameter("T_Mark").AsString() != "");
            foreach (Element viewElement in viewElements)
            {
                if (viewElement is ViewSection)
                {
                    CachedSections.Add((ViewSection)viewElement);
                }
            }
            return CachedSections;
        }
        

        /// <summary>
        /// Cache all cross Section (not ViewSection) belong to a beam
        /// </summary>
        /// <param name="uidoc"></param>
        /// <param name="Mark"></param>
        public static List<Section> CacheSections(Element beam, Document doc, SectionType sectionType)
        {
            List<Section> CachedSections = new List<Section>();
            string mark = ElementTools.GetMark(beam);
            TStoreData tData = new TStoreData();
            var viewElements = new FilteredElementCollector(doc).OfClass(typeof(ViewSection)).Where(x => x.LookupParameter("T_Mark").AsString() == mark).Where(x => x.LookupParameter("T_Mark").AsString() != "");
            foreach (Element viewElement in viewElements)
            {
                if (viewElement is ViewSection)
                {
                    ViewSection vs = (ViewSection)viewElement;
                    Section CachedSection = new Section();

                    if (!vs.IsTemplate)
                    {
                        var v1 = DataTools.GetInfoFromElement<string>(vs, "Direction", DisplayUnitType.DUT_UNDEFINED);
                        if (v1 != null && v1 == "CrossSection" && sectionType == SectionType.CrossSection)
                        {
                            var b = vs.get_BoundingBox(doc.ActiveView);

                            CachedSection.ViewSection = vs; // Retrieve ViewSection

                            RetrieveSectionOriginLocation(beam, CachedSection); // Retrieve Origin, L, d
                            CachedSection.Template = (View)doc.GetElement(vs.ViewTemplateId); // Retrieve Template
                            GetBBCoordinates(beam, CachedSection, b); // Retrieve coordianates of the bounding box

                            var v = viewElement.LookupParameter(TName).AsString();

                            if (viewElement.LookupParameter(TName).AsString() != null)
                            {
                                CachedSection.ViewSectionName = viewElement.LookupParameter(TName).AsString(); // Retrieve View Section Name
                                CachedSection.IsManualNamed = true;
                            }
                            else CachedSection.ViewSectionName = viewElement.LookupParameter(ViewSectionName).AsString();
                            CachedSection.ViewFamilyType = (ViewFamilyType)doc.GetElement(vs.GetTypeId());
                            CachedSection.Host = mark;

                            CachedSections.Add(CachedSection);
                        }
                        else if (v1 != null && v1 == "LongSection" && sectionType == SectionType.LongSection)
                        {
                            int r = vs.GetEntitySchemaGuids().Count;
                            string offset = DataTools.GetInfoFromElement<string>(vs, "Offset", DisplayUnitType.DUT_UNDEFINED);
                            string viewFamType = DataTools.GetInfoFromElement<string>(vs, "ViewFamilyType", DisplayUnitType.DUT_UNDEFINED);
                            string template = DataTools.GetInfoFromElement<string>(vs, "Template", DisplayUnitType.DUT_UNDEFINED);
                            string viewSectionName = vs.Name;
                            string host = DataTools.GetInfoFromElement<string>(vs, "Host", DisplayUnitType.DUT_UNDEFINED);

                            CachedSection.Offset = Convert.ToDouble(offset);
                            CachedSection.ViewFamilyType = (ViewFamilyType)(FamilyTools.GetTypeInstanceByName<ViewFamilyType>(viewFamType, doc).FirstOrDefault());
                            CachedSection.Template = (View)(FamilyTools.GetTypeInstanceByName<View>(template, doc).FirstOrDefault());
                            CachedSection.ViewSectionName = viewSectionName;
                            CachedSection.Host = host;
                            CachedSection.ViewSection = vs;

                            CachedSections.Add(CachedSection);
                        }
                    }
                }
            }
            return CachedSections;
        }

        

        static void RetrieveSectionOriginLocation(Element e, Section section)
        {
            Section CachedSection = section;
            ViewSection vs = CachedSection.ViewSection;

            XYZ OMax = vs.CropBox.Max;
            XYZ OMin = vs.CropBox.Min;

            // Get midline of the selected beam(s)
            XYZ p0 = BeamTools.GetBeamMidLine(e)[0];
            XYZ p1 = BeamTools.GetBeamMidLine(e)[1];

            //// Get all corners of the crop section top and bot surfaces. There are 4 cases corresponding to 4 way a beam can be orientated
            XYZ Ovs = vs.Origin; // The origin from vs, this one is not the one I set in BoundingBoxTransform(). vs is the cached section

            // Retrieve dimensions of the crop box
            double rot = GeometryTools.GetRotation(e, true); // Get the rotation of the selected beam
            double b = Math.Abs(OMax.X);
            double l = Math.Abs(OMin.Z);

            // Declare all coordinates
            double xMaxTop, yMaxTop, zMaxTop;
            double xMinTop, yMinTop, zMinTop;
            double x4Top, y4Top, z4Top;
            double xO, yO, zO; // Origin retrieved by Thai

            XYZ MaxTop, MinTop, P4Top;

            // Case 1: p0/p1
            if (p0.X <= p1.X && p0.Y <= p1.Y)
            {
                // Top Max Point
                xMaxTop = Ovs.X + b * Math.Sin(rot);
                yMaxTop = Ovs.Y - b * Math.Cos(rot);
                zMaxTop = Ovs.Z + OMax.Y;
                MaxTop = new XYZ(xMaxTop, yMaxTop, zMaxTop);

                // Top Min Point
                xMinTop = Ovs.X + l * Math.Cos(rot);
                yMinTop = Ovs.Y + l * Math.Sin(rot);
                zMinTop = MaxTop.Z;
                MinTop = new XYZ(xMinTop, yMinTop, zMinTop);

                // Top 4th Point
                x4Top = MaxTop.X + l * Math.Cos(rot);
                y4Top = MaxTop.Y + l * Math.Sin(rot);
                z4Top = MaxTop.Z;
                P4Top = new XYZ(x4Top, y4Top, z4Top);

                // Top Origin
                XYZ OTop = new XYZ(Ovs.X, Ovs.Y, MaxTop.Z);

                // Bot Max Point
                XYZ MaxBot = new XYZ(MaxTop.X, MaxTop.Y, Ovs.Z);

                // Bot Min Point
                XYZ MinBot = new XYZ(MinTop.X, MinTop.Y, Ovs.Z);

                // Bot 4th Point
                XYZ P4Bot = new XYZ(P4Top.X, P4Top.Y, Ovs.Z);

                // Calculate distance from OTop and MaxTop to the beam centerline
                Line beamCenterLine = Line.CreateBound(p0, p1);
                double OTopToBeamCL = beamCenterLine.Distance(new XYZ(OTop.X, OTop.Y, p0.Z)); // Project Ovs to top beam plane to calculate distance
                double TopMaxToBeamCL = beamCenterLine.Distance(new XYZ(MaxTop.X, MaxTop.Y, p0.Z));

                // Implement TAction class to retrieve beam's b and h
                double beamB = BeamTools.GetBeamDims(e)[0];
                double beamH = BeamTools.GetBeamDims(e)[1];

                // Calculate distances from Ovs and Maxvs to beam bounding lines (a.k.a. XMaxExtra, XMinExtra, YMaxExtra, YMinExtra, ZMaxExtra)
                CachedSection.XMaxExtra = OTopToBeamCL - beamB / 2;
                CachedSection.XMinExtra = TopMaxToBeamCL - beamB / 2;
                CachedSection.YMaxExtra = Ovs.Z + OMax.Y - p0.Z;
                CachedSection.YMinExtra = Math.Abs(-Ovs.Z + p0.Z) - beamH;
                CachedSection.ZMaxExtra = Math.Abs(OMin.Z);

                // Retrieve Origin (defined by Thai not Revit API)
                xO = Ovs.X + (CachedSection.XMaxExtra + beamB / 2) * Math.Sin(rot);
                yO = Ovs.Y - (CachedSection.XMaxExtra + beamB / 2) * Math.Cos(rot);
                zO = Ovs.Z + OMax.Y / 2;
                XYZ CachedOrigin = new XYZ(xO, yO, zO);
                CachedSection.Origin = CachedOrigin;

                CachedSection.d = Math.Round((UnitUtils.ConvertFromInternalUnits((new XYZ(xO, yO, p1.Z)).DistanceTo(p0), DisplayUnitType.DUT_MILLIMETERS)), 2);
                CachedSection.L = Math.Abs(Math.Round(UnitUtils.ConvertFromInternalUnits(p0.DistanceTo(p1), DisplayUnitType.DUT_MILLIMETERS) / CachedSection.d, 2));
            }

            // Case 2: p0/p1
            else if (p0.X > p1.X && p0.Y > p1.Y)
            {
                // Top Max Point
                xMaxTop = Ovs.X - b * Math.Sin(rot);
                yMaxTop = Ovs.Y + b * Math.Cos(rot);
                zMaxTop = Ovs.Z + OMax.Y;
                MaxTop = new XYZ(xMaxTop, yMaxTop, zMaxTop);

                // Top Min Point
                xMinTop = Ovs.X - l * Math.Cos(rot);
                yMinTop = Ovs.Y - l * Math.Sin(rot);
                zMinTop = MaxTop.Z;
                MinTop = new XYZ(xMinTop, yMinTop, zMinTop);

                // Top 4th Point
                x4Top = MaxTop.X - l * Math.Cos(rot);
                y4Top = MaxTop.Y - l * Math.Sin(rot);
                z4Top = MaxTop.Z;
                P4Top = new XYZ(x4Top, y4Top, z4Top);

                // Top Origin
                XYZ OTop = new XYZ(Ovs.X, Ovs.Y, MaxTop.Z);

                // Bot Max Point
                XYZ MaxBot = new XYZ(MaxTop.X, MaxTop.Y, Ovs.Z);

                // Bot Min Point
                XYZ MinBot = new XYZ(MinTop.X, MinTop.Y, Ovs.Z);

                // Bot 4th Point
                XYZ P4Bot = new XYZ(P4Top.X, P4Top.Y, Ovs.Z);

                // Calculate distance from OTop and MaxTop to the beam centerline
                Line beamCenterLine = Line.CreateBound(p0, p1);
                double OTopToBeamCL = beamCenterLine.Distance(new XYZ(OTop.X, OTop.Y, p0.Z)); // Project Ovs to top beam plane to calculate distance
                double TopMaxToBeamCL = beamCenterLine.Distance(new XYZ(MaxTop.X, MaxTop.Y, p0.Z));

                // Implement TAction class to retrieve beam's b and h
                BeamTools beamTools = new BeamTools();
                double beamB = BeamTools.GetBeamDims(e)[0];
                double beamH = BeamTools.GetBeamDims(e)[1];

                // Calculate distances from Ovs and Maxvs to beam bounding lines (a.k.a. XMaxExtra, XMinExtra, YMaxExtra, YMinExtra, ZMaxExtra)
                CachedSection.XMaxExtra = OTopToBeamCL - beamB / 2;
                CachedSection.XMinExtra = TopMaxToBeamCL - beamB / 2;
                CachedSection.YMaxExtra = Ovs.Z + OMax.Y - p0.Z;
                CachedSection.YMinExtra = Math.Abs(-Ovs.Z + p0.Z) - beamH;
                CachedSection.ZMaxExtra = Math.Abs(OMin.Z);

                // Retrieve Origin (defined by Thai not Revit API)
                xO = Ovs.X - (CachedSection.XMaxExtra + beamB / 2) * Math.Sin(rot);
                yO = Ovs.Y + (CachedSection.XMaxExtra + beamB / 2) * Math.Cos(rot);
                zO = Ovs.Z + OMax.Y / 2;
                XYZ CachedOrigin = new XYZ(xO, yO, zO);
                CachedSection.Origin = CachedOrigin;

                CachedSection.d = Math.Round((UnitUtils.ConvertFromInternalUnits((new XYZ(xO, yO, p1.Z)).DistanceTo(p0), DisplayUnitType.DUT_MILLIMETERS)), 2);
                CachedSection.L = Math.Abs(Math.Round(UnitUtils.ConvertFromInternalUnits(p0.DistanceTo(p1), DisplayUnitType.DUT_MILLIMETERS) / CachedSection.d, 2));
            }

            // Case 3: p0\p1
            else if (p0.X < p1.X && p0.Y > p1.Y)
            {
                // Top Max Point
                xMaxTop = Ovs.X - b * Math.Sin(rot);
                yMaxTop = Ovs.Y - b * Math.Cos(rot);
                zMaxTop = Ovs.Z + OMax.Y;
                MaxTop = new XYZ(xMaxTop, yMaxTop, zMaxTop);

                // Top Min Point
                xMinTop = Ovs.X + l * Math.Cos(rot);
                yMinTop = Ovs.Y - l * Math.Sin(rot);
                zMinTop = MaxTop.Z;
                MinTop = new XYZ(xMinTop, yMinTop, zMinTop);

                // Top 4th Point
                x4Top = MaxTop.X + l * Math.Cos(rot);
                y4Top = MaxTop.Y - l * Math.Sin(rot);
                z4Top = MaxTop.Z;
                P4Top = new XYZ(x4Top, y4Top, z4Top);

                // Top Origin
                XYZ OTop = new XYZ(Ovs.X, Ovs.Y, MaxTop.Z);

                // Bot Max Point
                XYZ MaxBot = new XYZ(MaxTop.X, MaxTop.Y, Ovs.Z);

                // Bot Min Point
                XYZ MinBot = new XYZ(MinTop.X, MinTop.Y, Ovs.Z);

                // Bot 4th Point
                XYZ P4Bot = new XYZ(P4Top.X, P4Top.Y, Ovs.Z);

                // Calculate distance from OTop and MaxTop to the beam centerline
                Line beamCenterLine = Line.CreateBound(p0, p1);
                double OTopToBeamCL = beamCenterLine.Distance(new XYZ(OTop.X, OTop.Y, p0.Z)); // Project Ovs to top beam plane to calculate distance
                double TopMaxToBeamCL = beamCenterLine.Distance(new XYZ(MaxTop.X, MaxTop.Y, p0.Z));

                // Implement TAction class to retrieve beam's b and h
                double beamB = BeamTools.GetBeamDims(e)[0];
                double beamH = BeamTools.GetBeamDims(e)[1];

                // Calculate distances from Ovs and Maxvs to beam bounding lines (a.k.a. XMaxExtra, XMinExtra, YMaxExtra, YMinExtra, ZMaxExtra)
                CachedSection.XMaxExtra = OTopToBeamCL - beamB / 2;
                CachedSection.XMinExtra = TopMaxToBeamCL - beamB / 2;
                CachedSection.YMaxExtra = Ovs.Z + OMax.Y - p0.Z;
                CachedSection.YMinExtra = Math.Abs(-Ovs.Z + p0.Z) - beamH;
                CachedSection.ZMaxExtra = Math.Abs(OMin.Z);

                // Retrieve Origin (defined by Thai not Revit API)
                xO = Ovs.X - (CachedSection.XMaxExtra + beamB / 2) * Math.Sin(rot);
                yO = Ovs.Y - (CachedSection.XMaxExtra + beamB / 2) * Math.Cos(rot);
                zO = Ovs.Z + OMax.Y / 2;
                XYZ CachedOrigin = new XYZ(xO, yO, zO);
                CachedSection.Origin = CachedOrigin;

                CachedSection.d = Math.Round((UnitUtils.ConvertFromInternalUnits((new XYZ(xO, yO, p1.Z)).DistanceTo(p0), DisplayUnitType.DUT_MILLIMETERS)), 2);
                CachedSection.L = Math.Abs(Math.Round(UnitUtils.ConvertFromInternalUnits(p0.DistanceTo(p1), DisplayUnitType.DUT_MILLIMETERS) / CachedSection.d, 2));
            }

            // Case 4: p1\p0
            else
            {
                // Top Max Point
                xMaxTop = Ovs.X + b * Math.Sin(rot);
                yMaxTop = Ovs.Y + b * Math.Cos(rot);
                zMaxTop = Ovs.Z + OMax.Y;
                MaxTop = new XYZ(xMaxTop, yMaxTop, zMaxTop);

                // Top Min Point
                xMinTop = Ovs.X - l * Math.Cos(rot);
                yMinTop = Ovs.Y + l * Math.Sin(rot);
                zMinTop = MaxTop.Z;
                MinTop = new XYZ(xMinTop, yMinTop, zMinTop);

                // Top 4th Point
                x4Top = MaxTop.X - l * Math.Cos(rot);
                y4Top = MaxTop.Y + l * Math.Sin(rot);
                z4Top = MaxTop.Z;
                P4Top = new XYZ(x4Top, y4Top, z4Top);

                // Top Origin
                XYZ OTop = new XYZ(Ovs.X, Ovs.Y, MaxTop.Z);

                // Bot Max Point
                XYZ MaxBot = new XYZ(MaxTop.X, MaxTop.Y, Ovs.Z);

                // Bot Min Point
                XYZ MinBot = new XYZ(MinTop.X, MinTop.Y, Ovs.Z);

                // Bot 4th Point
                XYZ P4Bot = new XYZ(P4Top.X, P4Top.Y, Ovs.Z);

                // Calculate distance from OTop and MaxTop to the beam centerline
                Line beamCenterLine = Line.CreateBound(p0, p1);
                double OTopToBeamCL = beamCenterLine.Distance(new XYZ(OTop.X, OTop.Y, p0.Z)); // Project Ovs to top beam plane to calculate distance
                double TopMaxToBeamCL = beamCenterLine.Distance(new XYZ(MaxTop.X, MaxTop.Y, p0.Z));

                // Implement TAction class to retrieve beam's b and h
                BeamTools beamTools = new BeamTools();
                double beamB = BeamTools.GetBeamDims(e)[0];
                double beamH = BeamTools.GetBeamDims(e)[1];

                // Calculate distances from Ovs and Maxvs to beam bounding lines (a.k.a. XMaxExtra, XMinExtra, YMaxExtra, YMinExtra, ZMaxExtra)
                CachedSection.XMaxExtra = OTopToBeamCL - beamB / 2;
                CachedSection.XMinExtra = TopMaxToBeamCL - beamB / 2;
                CachedSection.YMaxExtra = Ovs.Z + OMax.Y - p0.Z;
                CachedSection.YMinExtra = Math.Abs(-Ovs.Z + p0.Z) - beamH;
                CachedSection.ZMaxExtra = Math.Abs(OMin.Z);

                // Retrieve Origin (defined by Thai not Revit API)
                xO = Ovs.X + (CachedSection.XMaxExtra + beamB / 2) * Math.Sin(rot);
                yO = Ovs.Y + (CachedSection.XMaxExtra + beamB / 2) * Math.Cos(rot);
                zO = Ovs.Z + OMax.Y / 2;
                XYZ CachedOrigin = new XYZ(xO, yO, zO);
                CachedSection.Origin = CachedOrigin;

                CachedSection.d = Math.Round((UnitUtils.ConvertFromInternalUnits((new XYZ(xO, yO, p1.Z)).DistanceTo(p0), DisplayUnitType.DUT_MILLIMETERS)), 2);
                CachedSection.L = Math.Abs(Math.Round(UnitUtils.ConvertFromInternalUnits(p0.DistanceTo(p1), DisplayUnitType.DUT_MILLIMETERS) / CachedSection.d, 2));
            }
        }


        static BoundingBoxXYZ bb;
        /// <summary>
        /// Use this method to retrieve the bounding box of the selected element
        /// </summary>
        /// <param name="e">The selected element</param>
        /// <param name="doc">The opened doc</param>
        /// <param name="pSections">This method retrieves bb for each Section of this list</param>
        /// <returns></returns>
        public static void CreateBoundingBox(Element e, Document doc, List<Section> pSections)
        {
            BoundingBoxTransform(e, pSections);

            foreach (Section sec in pSections)
            {
                bb = new BoundingBoxXYZ();
                bb.Enabled = true;
                GetBBCoordinates(e, sec, bb);
                bb.Transform = sec.Tran;
                sec.BoundingBox = bb;
            }
        }

       

        static void BoundingBoxTransform(Element e, List<Section> pSections)
        {
            XYZ p0 = BeamTools.GetBeamEnds(e)[0];
            XYZ p1 = BeamTools.GetBeamEnds(e)[1];
            GetOrigin(p0, p1, pSections);

            foreach (Section sec in pSections)
            {
                Transform tran = null;
                tran = Transform.Identity;
                tran.Origin = sec.Origin;

                // Both ways work. I save both here for later references
                tran.BasisY = new XYZ(0, 0, 1);
                tran.BasisZ = GetBasisZ(p0, p1);
                tran.BasisX = GetBasisX(GetBasisZ(p0, p1));
                //tran.set_Basis(1, new XYZ(0, 0, 1));
                //tran.set_Basis(2, GetBasisZ(p0, p1));
                //tran.set_Basis(0, GetBasisX(GetBasisZ(p0, p1)));
                sec.Tran = tran;
            }
        }

       
        public static XYZ GetBasisZ(XYZ p0, XYZ p1)
        {
            double x = p1.X - p0.X;
            double y = p1.Y - p0.Y;
            double z = p1.Z - p0.Z;
            double distance = Math.Sqrt(x * x + y * y + z * z);
            XYZ Z = new XYZ(x / distance, y / distance, z / distance);

            return Z;
        }

        public static XYZ GetBasisX(XYZ BasisZ)
        {
            double x = -BasisZ.Y;
            double y = BasisZ.X;
            double z = BasisZ.Z;
            XYZ Z = new XYZ(x, y, z);

            return Z;
        }


        public static void GetOrigin(XYZ p0, XYZ p1, List<Section> pSections)
        {
            List<XYZ> origins = new List<XYZ>();

            double x, y, z;

            if (pSections.Count > 0)
            {
                foreach (Section sec in pSections)
                {
                    if (sec.IsRelative)
                    {
                        x = p0.X + (p1.X - p0.X) / sec.L;
                        y = p0.Y + (p1.Y - p0.Y) / sec.L;
                        z = (p0.Z + p1.Z) / 2;
                    }
                    else
                    {
                        x = GeometryTools.GetRelativePointBetween2Points(p0, p1, sec.d).X;
                        y = GeometryTools.GetRelativePointBetween2Points(p0, p1, sec.d).Y;
                        z = GeometryTools.GetRelativePointBetween2Points(p0, p1, sec.d).Z;
                    }
                    sec.Origin = new XYZ(x, y, z);
                }
            }
        }

        public static void GetBBCoordinates(Element e, Section sec, BoundingBoxXYZ bb)
        {
            double b = BeamTools.GetBeamDims(e)[0];
            double h = BeamTools.GetBeamDims(e)[1];

            if (!FieldClass.IsBBManual)
            {
                sec.XMax = b / 2 + sec.XMaxExtra;
                sec.YMax = sec.YMaxExtra;
                sec.ZMax = sec.ZMaxExtra;

                sec.XMin = -(b / 2 + sec.XMinExtra);
                sec.YMin = -(h + sec.YMinExtra);
                sec.ZMin = -(sec.ZMinExtra);
            }
            else
            {
                sec.XMax = sec.XMaxManual + sec.XMaxExtra;
                sec.YMax = sec.YMaxManual + sec.YMaxExtra;
                sec.ZMax = sec.ZMaxManual + sec.ZMaxExtra;

                sec.XMin = -(sec.XMinManual + sec.XMinExtra);
                sec.YMin = -(sec.YMinManual + sec.YMinExtra);
                sec.ZMin = -(sec.ZMinManual + sec.ZMinExtra);
            }

            //BoundingBoxXYZ bb = sec.BoundingBox;
            if (bb != null)
            {
                bb.Max = new XYZ(sec.XMax, sec.YMax, sec.ZMax);
                bb.Min = new XYZ(sec.XMin, sec.YMin, sec.ZMin);
            }
        }

        public enum SectionType
        {
            CrossSection,
            LongSection
        }
    }
}
