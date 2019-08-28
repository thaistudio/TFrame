using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI.Selection;

namespace TFrame
{
    public class TagActions
    {
        public double stirrupTagLength = 250;
        public double l = 250;
        public double firstDimensionZ = 100;
        public double zIncreament = 100;

        public bool IsOK;

        public ElementId desiredFamily;
        public ElementId stirrupDesiredFamily;

        public ExternalCommandData externalCommandData;
        public Document _doc;
        public UIDocument _uiDoc;

        public DimensionType dimType; // Set dimension style for the Multi Refenrence Annotation tag
        public ElementId multiTagTypeId; // Set the type of the multi tag
        public ElementId tagTypeId; // Set tag type of the multi tag

        public double topCoeff = 1;
        public double stirrupCoeff = 1;
        public double botCoeff = 1;

        FamilyTools famTools;
        SectionTools secTools;

        public TagActions(ExternalCommandData commandData)
        {
            externalCommandData = commandData;
            _doc = commandData.Application.ActiveUIDocument.Document;
            _uiDoc = commandData.Application.ActiveUIDocument;
            secTools = new SectionTools(commandData);
        }

        public void TagMultiRebars(Element selectedBeam, Dictionary<Section, Dictionary<List<List<Rebar>>, List<Rebar>>> big)
        {
                foreach (KeyValuePair<Section, Dictionary<List<List<Rebar>>, List<Rebar>>> pair in big)
                {
                    Dictionary<List<List<Rebar>>, List<Rebar>> standardStirrupDic = pair.Value;
                    XYZ paraVector = BeamTools.GetUnitParallelVector(BeamTools.GetBeamEnds(selectedBeam)[0], BeamTools.GetBeamEnds(selectedBeam)[1]);
                    XYZ normalVector = BeamTools.GetUnitNormalVector(BeamTools.GetBeamEnds(selectedBeam)[0], BeamTools.GetBeamEnds(selectedBeam)[1]);

                    Section section = pair.Key;
                    double h = BeamTools.GetBeamDims(selectedBeam)[0];
                    ElementId ownerViewId = section.ViewSection.Id;
                    XYZ origin = section.Origin; // Origin of the section, a.k.a the center of the beam

                    foreach (KeyValuePair<List<List<Rebar>>, List<Rebar>> standardStirrupPair in standardStirrupDic)
                    {
                        List<List<Rebar>> sortedRebars = standardStirrupPair.Key;
                        List<Rebar> stirrups = standardStirrupPair.Value;

                        for (int j = 0; j < sortedRebars.Count; j++) // Row level
                        {
                            var v1 = sortedRebars[j];
                            double a = 0; // Add a space between 2 tags in z direction
                            for (int i = 0; i < sortedRebars[j].Count; i++) // Rebar level
                            {
                                Rebar rebar = sortedRebars[j][i];
                                ElementId shapeId = rebar.GetShapeId();
                                RebarShape shape = (RebarShape)_doc.GetElement(shapeId);

                                if (rebar.Quantity == 1)
                                {
                                    XYZ rebarCoordinate = RebarTools.GetRebarCoorAtSection(rebar, section, _doc);
                                    double beamMidElev = (selectedBeam.get_BoundingBox(section.ViewSection).Max.Z + selectedBeam.get_BoundingBox(section.ViewSection).Min.Z) / 2;
                                    double beamHeightNoRebarCover = 
                                    BeamTools.GetBeamDims(selectedBeam)[1] - BeamTools.GetRebarCover(selectedBeam, _doc, BuiltInParameter.CLEAR_COVER_BOTTOM) - BeamTools.GetRebarCover(selectedBeam, _doc, BuiltInParameter.CLEAR_COVER_TOP);
                                    double coeff = 1;
                                    double sideCoeff;

                                    XYZ leaderEnd, leaderElbow, tagHeadPosition;

                                    if (sortedRebars[j].Count == 1) a = 0;

                                    if (j == sortedRebars.Count - 1)
                                    {
                                        coeff = -1;
                                        sideCoeff = botCoeff;
                                    }
                                    else sideCoeff = topCoeff;

                                    leaderEnd = rebarCoordinate;
                                    leaderElbow = new XYZ(leaderEnd.X, leaderEnd.Y, leaderEnd.Z + (a + firstDimensionZ) * coeff);
                                    tagHeadPosition = GeometryTools.GetPointAtDistNormalToAVector(paraVector, leaderElbow, l * sideCoeff, leaderElbow.Z);

                                Autodesk.Revit.DB.Reference reference = new Autodesk.Revit.DB.Reference(rebar);

                                    IndependentTag tagRebars = IndependentTag.Create(_doc, desiredFamily, section.ViewSection.Id, reference, true, TagOrientation.Horizontal, leaderEnd);
                                    tagRebars.LeaderEndCondition = LeaderEndCondition.Free;
                                    tagRebars.LeaderElbow = leaderElbow;
                                    tagRebars.LeaderEnd = leaderEnd;
                                    tagRebars.TagHeadPosition = tagHeadPosition;

                                    a += zIncreament;
                                }

                                else if (rebar.Quantity > 1)
                                {
                                    // Set tag options
                                    double rebarElev = RebarTools.GetRebarElevation(rebar, section, _doc); //tStore.GetInfoFromElement<double>(rebar, "Elevation", DisplayUnitType.DUT_MILLIMETERS);
                                    double distToTopFace = RebarTools.GetDistanceFromRebarToEdges(rebar, section, _doc, 2);
                                    double distToBotFace = RebarTools.GetDistanceFromRebarToEdges(rebar, section, _doc, 3);
                                    double desiredZ;  // Height of tag comparing to top of the beam
                                    double sideCoeff;

                                    //if (sortedRebarsInRows[j].Count > 1) a = 0.4;
                                    //else a = 0;
                                    if (j == 0) // Top row
                                    {
                                        desiredZ = rebarElev + firstDimensionZ + a;
                                        sideCoeff = topCoeff;
                                    }

                                    else if (j == sortedRebars.Count - 1) // Bottom row
                                    {
                                        desiredZ = rebarElev - a - firstDimensionZ;
                                        sideCoeff = botCoeff;
                                    }
                                    else // All other rows
                                    {
                                        if (distToBotFace > distToTopFace) // Top half
                                        {
                                            desiredZ = rebarElev - a - firstDimensionZ;
                                            sideCoeff = topCoeff;
                                        }
                                        else if (distToBotFace < distToTopFace) // Bottom half
                                        {
                                            desiredZ = rebarElev + a + firstDimensionZ;
                                            sideCoeff = botCoeff;
                                        }
                                        else
                                        {
                                            desiredZ = a;
                                            sideCoeff = 1;
                                        }
                                    }

                                    List<ElementId> rebarIds = new List<ElementId>();
                                    rebarIds.Add(rebar.Id);
                                    ICollection<ElementId> ICRebarIds = rebarIds;

                                    MultiReferenceAnnotationType multiReference = GetMultiRefAnnotationType()[0];
                                    MultiReferenceAnnotationOptions options = new MultiReferenceAnnotationOptions(multiReference);
                                    options.DimensionLineOrigin = new XYZ(origin.X, origin.Y, desiredZ);
                                    options.TagHeadPosition = GetTagHeadPosition(paraVector, options.DimensionLineOrigin, l * sideCoeff, desiredZ);
                                    options.DimensionLineDirection = normalVector;
                                    options.DimensionPlaneNormal = section.ViewSection.ViewDirection;
                                    options.DimensionStyleType = DimensionStyleType.Linear;
                                    options.SetElementsToDimension(ICRebarIds);
                                    MultiReferenceAnnotation multiTag = MultiReferenceAnnotation.Create(_doc, ownerViewId, options);

                                    // Set dimension style
                                    ElementId dimId = multiTag.DimensionId;
                                    Dimension dim = (Dimension)_doc.GetElement(dimId);
                                    dim.DimensionType = dimType;

                                    // Set tag style
                                    ElementId tagId = multiTag.TagId;
                                    IndependentTag tag = (IndependentTag)_doc.GetElement(tagId);
                                    ElementId id = tag.GetTypeId();
                                    tag.ChangeTypeId(tagTypeId);
                                    ElementId id1 = tag.GetTypeId();

                                    //Set multi tag type
                                    multiTag.ChangeTypeId(multiTagTypeId);

                                    a += zIncreament;
                                }
                            }

                        }

                        foreach (Rebar stirrup in stirrups)
                        {
                            double beamWidthNoRebarCover = BeamTools.GetBeamDims(selectedBeam)[0] - 2 * BeamTools.GetRebarCover(selectedBeam, _doc, BuiltInParameter.CLEAR_COVER_OTHER);

                            double stirrupD = stirrup.get_Parameter(BuiltInParameter.REBAR_INSTANCE_BAR_DIAMETER).AsDouble(); // Can write this as a method
                            double d = ((beamWidthNoRebarCover - stirrupD) / 2 + stirrupTagLength) * stirrupCoeff;

                            XYZ tagHeadPosition = GeometryTools.GetPointAtDistNormalToAVector(paraVector, origin, d, origin.Z);
                            
                            XYZ leaderEnd = GeometryTools.GetPointAtDistNormalToAVector(paraVector, origin, d - stirrupTagLength * stirrupCoeff, origin.Z);
                            XYZ leaderElbow = leaderEnd; // geoTools.GetPointAtDistNormalToAVector(paraVector, origin, d, origin.Z);

                        Autodesk.Revit.DB.Reference reference = new Autodesk.Revit.DB.Reference(stirrup);

                            IndependentTag tagRebars = IndependentTag.Create(_doc, stirrupDesiredFamily, section.ViewSection.Id, reference, true, TagOrientation.Horizontal, leaderEnd);
                            tagRebars.LeaderEndCondition = LeaderEndCondition.Free;
                            tagRebars.LeaderElbow = leaderElbow;
                            tagRebars.LeaderEnd = leaderEnd;
                            tagRebars.TagHeadPosition = tagHeadPosition;
                        }
                    }
                }
        }

        public void LoadTagFamily()
        {
            string famName = "TDimTag";
            string tagPath = @"D:\Thai\Code\Revit\TFrame\TFamily\Single Rebar Tag\TDimTag.rfa";
            if (!FamilyTools.FamilyExists(famName)) _doc.LoadFamily(tagPath);
        }

        public XYZ GetTagHeadPosition(XYZ beamVector, XYZ lineOrigin, double l, double desiredZ)
        {
            XYZ position = GeometryTools.GetPointAtDistNormalToAVector(beamVector, lineOrigin, l, desiredZ);
            return position;
        }


        public List<Element> GetSelectedBeams()
        {
            List<Element> selectedBeams = new List<Element>();
            Selection sel = _uiDoc.Selection;
            ICollection<ElementId> selIds = sel.GetElementIds();

            foreach (ElementId selId in selIds)
            {
                Element selElem = _doc.GetElement(selId);
                selectedBeams.Add(selElem);
            }
            return selectedBeams;
        }

        public List<MultiReferenceAnnotationType> GetMultiRefAnnotationType()
        {
            List<MultiReferenceAnnotationType> annoTypes = new List<MultiReferenceAnnotationType>();
            MultiReferenceAnnotationType annoType;
            FilteredElementCollector annotations = new FilteredElementCollector(_doc).OfClass(typeof(MultiReferenceAnnotationType)).WhereElementIsElementType();
            foreach (Element e in annotations)
            {
                annoType = (MultiReferenceAnnotationType)e;
                annoTypes.Add(annoType);
            }
            return annoTypes;
        }

        public Dictionary<Section, Dictionary<List<List<Rebar>>, List<Rebar>>> GroupSectionWithRebars(Element selectedBeam)
        {
            Dictionary<Section, Dictionary<List<List<Rebar>>, List<Rebar>>> big = new Dictionary<Section, Dictionary<List<List<Rebar>>, List<Rebar>>>();
            Dictionary<List<List<Rebar>>, List<Rebar>> standardStirrup = new Dictionary<List<List<Rebar>>, List<Rebar>>();

            List<Section> sections = SectionTools.CacheSections(selectedBeam, _doc, SectionTools.SectionType.CrossSection);
            List<Rebar> rebars = BeamTools.CacheRebars(selectedBeam, _doc);

            foreach (Section section in sections)
            {
                List<Rebar> standardRebars = SortRebarAndStirrups(rebars, RebarStyle.Standard);
                List<Rebar> stirrups = SortRebarAndStirrups(rebars, RebarStyle.StirrupTie);
                List<List<Rebar>> sortedRebarsInRows = SortRebars(standardRebars, section);
                standardStirrup = new Dictionary<List<List<Rebar>>, List<Rebar>>();
                standardStirrup.Add(sortedRebarsInRows, stirrups);
                big.Add(section, standardStirrup);
            }
           
            return big;
        }

        public List<List<Rebar>> SortRebars(List<Rebar> rebars, Section section)
        {
            List<double> dists = new List<double>();
            Dictionary<Rebar, double> pairs = new Dictionary<Rebar, double>();

            List<double> listOfUniqueElevations = RebarTools.GetRebarUniqueElevations(rebars, section, _doc).OrderByDescending(x => x).ToList();
            int numberOfUniqueElevs = listOfUniqueElevations.Count;

            Dictionary<Rebar, double> rebarAndElev = new Dictionary<Rebar, double>();

            // Sort rebars by rows. Result: orderByDescendingElevRebars
            List<List<Rebar>> rebarsInRows = SortRebarToRows(rebars, section);

            return rebarsInRows;
        }

        public List<List<Rebar>> SortRebarToRows(List<Rebar> rebars, Section section)
        {
            Element beam = _doc.GetElement(rebars.First().GetHostId());

            List<double> listOfUniqueValues = RebarTools.GetRebarUniqueElevations(rebars, section, _doc).OrderByDescending(x => x).ToList();

            List<Rebar> rebars2 = rebars.OrderByDescending(x => RebarTools.GetRebarElevation(x, section, _doc)).ToList();

            Dictionary<Rebar, double> rebarsAndElevs = new Dictionary<Rebar, double>();

            List<Rebar> rebarsByDescendingElev = new List<Rebar>();
            List<List<Rebar>> rebarsByLevel = new List<List<Rebar>>();
            List<Rebar> row1 = new List<Rebar>();

            double tolerance = BeamTools.GetRebarCover(beam, _doc, BuiltInParameter.CLEAR_COVER_TOP);

            foreach (Rebar rebar in rebars2)
            {
                double rebarElev = RebarTools.GetRebarElevation(rebar, section, _doc);
                rebarsAndElevs.Add(rebar, rebarElev);
            }

            while (listOfUniqueValues.Count > 0)
            {
                foreach (KeyValuePair<Rebar, double> pair in rebarsAndElevs)
                {
                    Rebar rebar = pair.Key;
                    double elev = pair.Value;

                    if (elev == listOfUniqueValues.Max())
                    {
                        if (rebarsByDescendingElev.Count == 0)
                        {
                            rebarsByDescendingElev.Add(rebar);
                            row1.Add(rebar);
                            rebarsByLevel.Add(row1);
                        }
                        else if (Math.Abs(elev - RebarTools.GetRebarElevation(rebarsByDescendingElev.Last(), section, _doc)) > tolerance)
                        {
                            rebarsByDescendingElev.Add(rebar);
                            List<Rebar> row2 = new List<Rebar>();
                            row2.Add(rebar);
                            rebarsByLevel.Add(row2);
                        }
                        else if (Math.Abs(elev - RebarTools.GetRebarElevation(rebarsByDescendingElev.Last(), section, _doc)) < tolerance)
                        {
                            rebarsByDescendingElev.Add(rebar);
                            rebarsByLevel.Last().Add(rebar);
                        }
                        rebarsAndElevs.Remove(rebar);
                        listOfUniqueValues.Remove(elev);
                        break;
                    }
                }
            }

            #region
            //bool FoundMax = false;
            //while (listOfUniqueValues.Count > 0)
            //{
            //    foreach (Rebar rebar in rebars.OrderByDescending(x => rebarTools.GetRebarElevation(x, section, _doc)))
            //    {
            //        double rebarElev = rebarTools.GetRebarElevation(rebar, section, _doc);

            //        foreach (double uniqueElevation in listOfUniqueValues)
            //        {
            //            if (rebarElev == uniqueElevation && uniqueElevation == listOfUniqueValues.Max())
            //            {
            //                if (rebarsByDescendingElev.Count == 0)
            //                {
            //                    rebarsByDescendingElev.Add(rebar);
            //                    row1.Add(rebar);
            //                    rebarsByLevel.Add(row1);
            //                }
            //                else if (Math.Abs(rebarElev - rebarTools.GetRebarElevation(rebarsByDescendingElev.Last(), section, _doc)) > tolerance)
            //                {
            //                    rebarsByDescendingElev.Add(rebar);
            //                    List<Rebar> row2 = new List<Rebar>();
            //                    row2.Add(rebar);
            //                    rebarsByLevel.Add(row2);
            //                }
            //                else if (Math.Abs(rebarElev - rebarTools.GetRebarElevation(rebarsByDescendingElev.Last(), section, _doc)) < tolerance)
            //                {
            //                    rebarsByDescendingElev.Add(rebar);
            //                    rebarsByLevel.Last().Add(rebar);
            //                }

            //                FoundMax = true;
            //                break;
            //            }
            //            if (rebars.Count == rebarsByDescendingElev.Count)
            //            {
            //                listOfUniqueValues.Remove(listOfUniqueValues.Max());
            //                break;
            //            }
            //            else if (rebarElev == uniqueElevation && uniqueElevation != listOfUniqueValues.Max() && FoundMax)
            //            {
            //                listOfUniqueValues.Remove(listOfUniqueValues.Max());
            //                FoundMax = false;
            //                break;
            //            }
            //        }
            //    }
            //}

            #endregion

            return rebarsByLevel;
        }

        public List<Rebar> SortRebarAndStirrups(List<Rebar> rebars, RebarStyle rebarStyle)
        {
            List<Rebar> standardRebars = new List<Rebar>();
            List<Rebar> stirrupRebars = new List<Rebar>();
            foreach (Rebar rebar in rebars)
            {
                if (((RebarShape)(_doc.GetElement(rebar.GetShapeId()))).RebarStyle == RebarStyle.Standard)
                {
                    standardRebars.Add(rebar);
                }
                else stirrupRebars.Add(rebar);
            }
            if (rebarStyle == RebarStyle.Standard) return standardRebars;
            else return stirrupRebars;
        }
    }
}
