using System.Collections.Generic;
using System.Linq;
using System;

using Autodesk.Revit.DB;

namespace TFrame
{
    public class DimensionActions
    {
        public DimensionActions()
        {

        }

        /// <summary>
        /// Entry command from cmdDimensions
        /// </summary>
        /// <param name="section"></param>
        public static void DetailSection(Section section)
        {
            Transaction t = new Transaction(section.ViewSection.Document);
            t.Start("Create Dimension");

            // Find references
            FindReferenceInView(section, UVPosition.Left);
            FindReferenceInView(section, UVPosition.Right);
            FindReferenceInView(section, UVPosition.Down);

            // Create dimension and place break line
            CreateDimension(section);
            section.ViewSection.CropBoxVisible = false;

            t.Commit();
        }

        /// <summary>
        /// Find dimensionable references of all beams in a view section. This method is repeated for left, right and down position.
        /// One DimensionSet contains all dimensionable references of all beams in a view section
        /// </summary>
        /// <param name="section"></param>
        /// <param name="uvPos"></param>
        public static void FindReferenceInView(Section section, UVPosition uvPos)
        {
            View view = section.ViewSection;

            // Find all beams in view
            // Add host beam to excluding list
            ICollection<ElementId> viewHostElementId = new List<ElementId>();
            viewHostElementId.Add(section.HostElement.Id);

            // Get all beams in this section view rather than host elem in view
            FilteredElementCollector otherBeamsInView = 
                new FilteredElementCollector(GlobalParams.Doccument, section.ViewSection.Id).Excluding(viewHostElementId).OfCategory(BuiltInCategory.OST_StructuralFraming); 
            
            // Create a new instance of DimensionSet
            DimensionSet dimensionSet = new DimensionSet(section, uvPos);

            // The host beam will belong to both position left and right
            Element hostBeam = section.HostElement;
            dimensionSet.CollectReferencesOfBeam(hostBeam);

            // If there are other beams rather than just the host beam, sort them by position: left and right
            if (otherBeamsInView.GetElementCount() > 0 && uvPos != UVPosition.Down) 
            {
                foreach (Element beam in otherBeamsInView)
                {
                    if (BeamIntersectsCropBox(section, beam, uvPos))
                    {
                        try
                        {
                            dimensionSet.CollectReferencesOfBeam(beam);
                            PlaceBreakLine(dimensionSet, beam);
                        }
                        catch (Exception ex)
                        {
                            GlobalParams.AddException(ex);
                        }
                        
                    }
                }
            }
            section.AddDimensionSet(dimensionSet);
        }

        /// <summary>
        /// Check if a beam intersects with the view section's crop box position (left/right/bottom face of the view section)
        /// Points on view section boundary will be projected to the beam in question's bottom face. 
        /// Starting point (viewer bb's max/min) will be checked first. If its projection is not on beam's face. Move to the next point 0.2 ft away in beam end0->end1 direction.
        /// Repeat the moving process until a point is on the face or the moving distance is greater than view section's limit (ZMaxExtra).
        /// </summary>
        /// <param name="beam">The beam that will be checked</param>
        /// <param name="section">The section that will be checked</param>
        /// <param name="uvPosition">Position of the view section that will be checked</param>
        /// <returns></returns>
        static bool BeamIntersectsCropBox(Section section, Element beam, UVPosition uvPosition)
        {
            // Get bottom face of the beam in question
            Face beamBotFace = BeamTools.GetBeamFace(beam, BeamFace.Face4);

            // Find the first point on view section's boundary
            XYZ point = null;
            if (uvPosition == UVPosition.Left) point = section.ViewerBoundingBox.Max;
            if (uvPosition == UVPosition.Right) point = section.ViewerBoundingBox.Min;
            
            // Check if the beam intersects with the view section's boundary
            return IsProjectedPointOnFace(point, beamBotFace, section, uvPosition);
        }

        /// <summary>
        /// This is a recursion, repeating until found a point on beam's bot face or the moving distance is greater than ZMaxExtra
        /// </summary>
        /// <param name="point">A point from the view section lest/right face</param>
        /// <param name="face">Bottom/top face of the beam in question</param>
        /// <param name="section">The section in question</param>
        /// <param name="uvPosition"></param>
        /// <returns></returns>
        static bool IsProjectedPointOnFace(XYZ point, Face face, Section section, UVPosition uvPosition)
        {
            // Local fields
            bool result;
            Element hostBeam = section.HostElement; // Host beam of the view section

            // Check if point's projection in beam (not host beam) is valid
            IntersectionResult intersection = face.Project(point);
            if (intersection != null) return true; // If not null, then the beam in question intersects with the view section 
            else // If null, move the point to a distance 0.2 ft in host beam direction
            {
                XYZ movedPoint = GeometryTools.GetPointParallelToLineAtDistance(BeamTools.GetBeamEnds(hostBeam)[0], BeamTools.GetBeamEnds(hostBeam)[1], point, 0.2);
                if (uvPosition == UVPosition.Left)
                {
                    // If the moving distance is greater than view section limit, and yet a point can be projected to face. Then the beam does not intersect with the view section
                    if (movedPoint.DistanceTo(section.ViewerBoundingBox.Max) > section.ZMaxExtra) return false; 
                }
                if (uvPosition == UVPosition.Right)
                {
                    if (movedPoint.DistanceTo(section.ViewerBoundingBox.Min) > section.ZMaxExtra) return false;
                }

                // The code reaches here means the moving distance is still within view section limit. Keep moving
                result = IsProjectedPointOnFace(movedPoint, face, section, uvPosition);
                return result;
            }
        }

        /// <summary>
        /// Create dimensions for section
        /// </summary>
        /// <param name="section"></param>
        public static void CreateDimension(Section section)
        {
            Document doc = section.HostElement.Document;

            // There are 4 dimension sets in a section: left, right, up and down
            foreach (DimensionSet dimensionSet in section.DimensionSets)
            {
                // Create regular dimensions for all references
                Line line = CreateDimensionLine(dimensionSet, false);
                ReferenceArray referenceArray = new ReferenceArray();
                foreach (DimensionableReference reference in dimensionSet.References)
                {
                    referenceArray.Append(reference.RevitReference);
                }
                Dimension dimension = doc.Create.NewDimension(section.ViewSection, line, referenceArray);
                dimension.DimensionType = BeamDimensionData.Singleton.DimensionType;

                // If there are overall dimensions, create them
                if (dimensionSet.HasOverallDimension)
                {
                    ReferenceArray overallReferences = new ReferenceArray();
                    List<DimensionableReference> overallDimensionableReferences = dimensionSet.References.OrderBy(x => x.FaceElevation).ToList();
                    for (int i = 0; i < overallDimensionableReferences.Count; i++)
                    {
                        if (i == 0 || i == overallDimensionableReferences.Count - 1) overallReferences.Append(overallDimensionableReferences[i].RevitReference);
                    }

                    line = CreateDimensionLine(dimensionSet, true);
                    Dimension overallDimension = doc.Create.NewDimension(section.ViewSection, line, overallReferences);
                    overallDimension.DimensionType = BeamDimensionData.Singleton.DimensionType;
                }
            }
        }

        /// <summary>
        /// Create line for a dimension
        /// </summary>
        /// <param name="dimensionSet"></param>
        /// <param name="isOverallDimension"></param>
        /// <returns></returns>
        static Line CreateDimensionLine(DimensionSet dimensionSet, bool isOverallDimension)
        {
            Line line = null;

            // Get needed properties
            Section section = dimensionSet.Section;
            UVPosition uvPos = dimensionSet.Position;

            XYZ secondPoint = null;
            XYZ firstPoint = CalculateDimensionOrigin(dimensionSet, isOverallDimension);
            if (uvPos == UVPosition.Left)
            {
                secondPoint = GeometryTools.GetPointParallelToLineAtDistance
                    (XYZ.Zero, XYZ.BasisZ.Negate(), firstPoint, 1);
            }
            else if (uvPos == UVPosition.Right)
            {
                secondPoint = GeometryTools.GetPointParallelToLineAtDistance
                     (XYZ.Zero, XYZ.BasisZ, firstPoint, 1);
            }
            else if (uvPos == UVPosition.Down)
            {
                firstPoint = GeometryTools.GetPointParallelToLineAtDistance
                    (XYZ.Zero, XYZ.BasisZ.Negate(), section.ViewerBoundingBox.Max, section.YMaxExtra + BeamTools.GetBeamDims(section.HostElement)[1] + 0.5);
                secondPoint = GeometryTools.GetPointParallelToLineAtDistance
                    (XYZ.Zero, section.ViewSection.RightDirection, firstPoint, 1);
            }
            line = Line.CreateBound(firstPoint, secondPoint);
            return line;
        }

        /// <summary>
        /// Get the first point of the dimension line
        /// </summary>
        /// <param name="dimensionSet"></param>
        /// <param name="isOverallDimension"></param>
        /// <returns></returns>
        static XYZ CalculateDimensionOrigin(DimensionSet dimensionSet, bool isOverallDimension)
        {
            XYZ origin = null;

            // Spacing between dimensions in a dimension set
            double dimensionSpacing = 0;
            if (isOverallDimension) dimensionSpacing = dimensionSet.DimensionsSpacing;

            // Get needed properties
            Section section = dimensionSet.Section;
            UVPosition uvPos = dimensionSet.Position;

            // Calculate the distance from the edge of the host beam to the bounding box boudary
            if (uvPos == UVPosition.Left)
            {
                if (dimensionSet.HasOverallDimension) // When there are more than 1 dimension, the first point is 0.5ft + dimensionSpacing from vierwer bb's max/min points
                {
                    origin = GeometryTools.GetPointParallelToLineAtDistance
                    (section.ViewSection.RightDirection, XYZ.Zero, section.ViewerBoundingBox.Max, 0.5 + dimensionSpacing);
                }
                else // When there is only 1 dim a.k.a. no side beam, dimension is 0.5ft away from host beam's side face
                {
                    origin = GeometryTools.GetPointParallelToLineAtDistance
                    (section.ViewSection.RightDirection, XYZ.Zero, section.ViewerBoundingBox.Max, 0.5 - section.XMaxExtra);
                }
            }
            else if (uvPos == UVPosition.Right)
            {
                if (dimensionSet.HasOverallDimension) // When there are more than 1 dimension, the first point is 0.5ft + dimensionSpacing from vierwer bb's max/min points
                {
                    origin = GeometryTools.GetPointParallelToLineAtDistance
                      (XYZ.Zero, section.ViewSection.RightDirection, section.ViewerBoundingBox.Min, 0.5 + dimensionSpacing);
                }
                else // When there is only 1 dim a.k.a. no side beam, dimension is 0.5ft away from host beam's side face
                {
                    origin = GeometryTools.GetPointParallelToLineAtDistance
                      (XYZ.Zero, section.ViewSection.RightDirection, section.ViewerBoundingBox.Min, 0.5 - section.XMinExtra);
                }
            }

            return origin;
        }

        /// <summary>
        /// Place break line for side beams
        /// </summary>
        /// <param name="dimensionSet"></param>
        /// <param name="beam"></param>
        public static void PlaceBreakLine(DimensionSet dimensionSet, Element beam)
        {
            XYZ coorOrigin = new XYZ();
            
            // Load break line family
            FamilyTools.LoadFamily(BeamDimensionData.Singleton.BreakLineFamilyDirectory);

            Section section = dimensionSet.Section;
            int positionCoefficient = 1; // 1 when on right, -1 on left

            // Place Family Instance
            // Find break line origin a.k.a. mid point between top and bot faces of the side beams
            // Calculate mid-elevation of side beams' top and bot faces
            double topElevation = BeamTools.GetBeamElevation(beam, BeamFace.Face2);
            double botElevation = BeamTools.GetBeamElevation(beam, BeamFace.Face4);
            double midElevation = (topElevation + botElevation) / 2; 
            double midElevToBbBoundary = 0; // Distance from the mid point to bounding box max (left beams), or min (right beams) points
            XYZ movedPoint = null; // Move max/min point of bounding box horizontally 0.1 to show avoid break line from being cut by the bounding box
            XYZ origin = null; // Origin point of break line
            if (dimensionSet.Position == UVPosition.Left) // Left beam
            {
                // Place break line 0.1 ft closer to the view center
                movedPoint = GeometryTools.GetPointParallelToLineAtDistance(XYZ.Zero, section.ViewSection.RightDirection, section.ViewerBoundingBox.Max, 0.1);
                midElevToBbBoundary = section.ViewerBoundingBox.Max.Z - midElevation;
                // Origin is movedPoint moved down a distance of midElevToBbBoundary
                origin = GeometryTools.GetPointParallelToLineAtDistance(XYZ.Zero, XYZ.BasisZ.Negate(), movedPoint, midElevToBbBoundary);
                positionCoefficient = -1;
            }
            else if (dimensionSet.Position == UVPosition.Right) // Right beam
            {
                midElevToBbBoundary = midElevation - section.ViewerBoundingBox.Min.Z;
                movedPoint = GeometryTools.GetPointParallelToLineAtDistance(XYZ.Zero, section.ViewSection.RightDirection.Negate(), section.ViewerBoundingBox.Min, 0.1);
                origin = GeometryTools.GetPointParallelToLineAtDistance(XYZ.Zero, XYZ.BasisZ, movedPoint, midElevToBbBoundary);
            }

            // Find the break line symbol and activate it
            IEnumerable<Element> symbolFilter = new FilteredElementCollector(GlobalParams.Doccument)
                .OfClass(typeof(FamilySymbol)).Where(x => x.Name == "M_Break Line");
            FamilySymbol symbol = (FamilySymbol)(symbolFilter.FirstOrDefault());
            symbol.Activate();

            // Place instance
            FamilyInstance breakLine = GlobalParams.Doccument.Create.NewFamilyInstance(origin, symbol, section.ViewSection);

            // Rotate break line
            // Center line to rotate
            XYZ beamDirection = ((Line)(((LocationCurve)(section.HostElement.Location)).Curve)).Direction;
            XYZ point = GeometryTools.GetPointParallelToLineAtDistance(XYZ.Zero, beamDirection, origin, 1);
            Line rotateLine = Line.CreateBound(origin, point);
            breakLine.Location.Rotate(rotateLine, positionCoefficient * Math.PI / 2);

            // Adjust break line size based on beam height
            double beamHeight = BeamTools.GetBeamDims(beam)[1];
            breakLine.LookupParameter("right")?.Set((beamHeight / 2) + 0.1);
            breakLine.LookupParameter("left")?.Set((beamHeight / 2) + 0.1);
        }

        public static void DetailLevelsInCrossSection(Section section)
        {
            ViewSection view = section.ViewSection;
            XYZ rightDirection = view.RightDirection;

            // Search levels in view
            List<Level> levels = FamilyTools.SearchElementsByType<Level>(view.Document, false, view);
            foreach (Level level in levels)
            {
                XYZ maxPoint = level.get_BoundingBox(view).Max;
                XYZ minPoint = level.get_BoundingBox(view).Min;

                // Move max point
                XYZ movedMaxPoint = GeometryTools.GetPointParallelToLineAtDistance(XYZ.Zero, -rightDirection, maxPoint, 0.5);
                XYZ movedMinPoint = GeometryTools.GetPointParallelToLineAtDistance(XYZ.Zero, rightDirection, minPoint, 0.5);
                level.get_BoundingBox(view).Max = movedMaxPoint;
                level.get_BoundingBox(view).Min = movedMinPoint;

            }
        }
    }
}
