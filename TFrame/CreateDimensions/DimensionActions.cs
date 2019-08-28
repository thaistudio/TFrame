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

        public static void DetailSection(Section section)
        {
            Transaction t = new Transaction(section.ViewSection.Document);
            t.Start("Create Dimension");

            FindReferenceInView(section, UVPosition.Left);
            FindReferenceInView(section, UVPosition.Right);
            FindReferenceInView(section, UVPosition.Down);
            CreateDimension(section);

            section.ViewSection.CropBoxVisible = false;
            t.Commit();
        }

        public static void FindReferenceInView(Section section, UVPosition uvPos)
        {
            View view = section.ViewSection;

            // Find all beams in view
            ICollection<ElementId> viewHostElementId = new FilteredElementCollector(view.Document).OfCategory(BuiltInCategory.OST_Views).ToElementIds();
            viewHostElementId.Clear();
            viewHostElementId.Add(section.HostElement.Id);
            FilteredElementCollector otherBeamsInView = 
                new FilteredElementCollector(GlobalParams.Doccument, view.Id).Excluding(viewHostElementId).OfCategory(BuiltInCategory.OST_StructuralFraming); // Get other beams rather than host elem in view

            DimensionSet dimensionSet = new DimensionSet(section, uvPos);

            // The host beam will belong to both direction
            Element hostBeam = section.HostElement;
            dimensionSet.GetReferencesOfBeam(hostBeam);

            // If there are other beams rather than just the host beam, sort them by position: left and right
            if (otherBeamsInView.GetElementCount() > 0 && uvPos != UVPosition.Down) 
            {
                foreach (Element beam in otherBeamsInView)
                {
                    if (BeamIntersectsCropBox(beam, section.ViewerBoundingBox, uvPos))
                    {
                        dimensionSet.GetReferencesOfBeam(beam);
                        PlaceBreakLine(dimensionSet, beam);
                    }
                    else 
                    {
                        if (BeamIsOnPosition(beam, uvPos, section))
                        {
                            dimensionSet.GetReferencesOfBeam(beam);
                            PlaceBreakLine(dimensionSet, beam);
                        }
                    }
                }
            }
            section.AddDimensionSet(dimensionSet);
        }

        /// <summary>
        /// Check if a beam intersects with the view's crop box
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="boundingBox"></param>
        /// <param name="uvPosition"></param>
        /// <returns></returns>
        static bool BeamIntersectsCropBox(Element beam, BoundingBoxXYZ boundingBox, UVPosition uvPosition)
        {
            // Need to get the bottom face of the beam to determine its location, based on the projection of the viewer's bounding box's max point
            // Project max point to botFace to determine beam position
            Face botFace = BeamTools.GetBeamFace(beam, BeamFace.Face4);
            IntersectionResult projectedPointOnBotFace;
            if (uvPosition == UVPosition.Left) projectedPointOnBotFace = botFace.Project(boundingBox.Max);
            else projectedPointOnBotFace = botFace.Project(boundingBox.Min);

            if (projectedPointOnBotFace != null) return true;
            else return false;
        }

        /// <summary>
        /// Check if a beam is on uvPosition of the view
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="uvPosition"></param>
        /// <param name="section"></param>
        /// <param name="tView"></param>
        /// <returns></returns>
        static bool BeamIsOnPosition(Element beam, UVPosition uvPosition, Section section)
        {
            View view = section.ViewSection;
            // Need to get the bottom face of the beam to determine its location, based on the projection of the viewer's bounding box's max point
            // Project max point to botFace to determine beam position
            Face botFace = BeamTools.GetBeamFace(beam, BeamFace.Face4);

            XYZ horizontalVector = view.RightDirection;
            XYZ dummyPoint = null;

            if (uvPosition == UVPosition.Left)
            { 
                // Create a dummy point 100mm from host beam's left edge to check if this beam is left or right
                dummyPoint = GeometryTools.GetPointParallelToLineAtDistance
                    (new XYZ(), horizontalVector, section.ViewerBoundingBox.Max, section.XMaxExtra - UnitUtils.ConvertToInternalUnits(0.05, DisplayUnitType.DUT_METERS));
            }
            else
            {
                // Create a dummy point 100mm from host beam's left edge to check if this beam is left or right
                dummyPoint = GeometryTools.GetPointParallelToLineAtDistance
                    (new XYZ(), horizontalVector, section.ViewerBoundingBox.Max, section.XMaxExtra + BeamTools.GetBeamDims(section.HostElement)[0] + UnitUtils.ConvertToInternalUnits(0.05, DisplayUnitType.DUT_METERS));
            }

            IntersectionResult projectedDummyPointOnBotFace = botFace.Project(dummyPoint);
            if (projectedDummyPointOnBotFace == null) return false;
            else return true;
        }

        public static void CreateDimension(Section section)
        {
            Document doc = section.HostElement.Document;

            foreach (DimensionSet dimensionSet in section.DimensionSets)
            {
                Line line = CreateDimensionLine(dimensionSet, false);
                ReferenceArray referenceArray = new ReferenceArray();
                foreach (DimensionableReference reference in dimensionSet.References)
                {
                    referenceArray.Append(reference.RevitReference);
                }

                Dimension dimension = doc.Create.NewDimension(section.ViewSection, line, referenceArray);

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
                dimension.DimensionType = BeamDimensionData.Singleton.DimensionType;
                var l = dimension.DimensionType.Name;
            }
        }

        /// <summary>
        /// Create line for dimensions
        /// </summary>
        /// <param name="dimensionSet"></param>
        /// <param name="dimensionsSpacing">Spacing betwwen 2 dimensions in case there are more than 1 dim</param>
        /// <returns></returns>
        static Line CreateDimensionLine(DimensionSet dimensionSet, bool isOverallDimension)
        {
            Line line = null;

            // Get needed properties
            Section section = dimensionSet.Section;
            UVPosition uvPos = dimensionSet.Position;

            XYZ downDirection = new XYZ(0, 0, -1);
            XYZ upDirection = new XYZ(0, 0, 1);
            XYZ secondPoint = null;
            XYZ firstPoint = CalculateDimensionOrigin(dimensionSet, isOverallDimension);
            if (uvPos == UVPosition.Left)
            {
                secondPoint = GeometryTools.GetPointParallelToLineAtDistance
                    (new XYZ(), downDirection, firstPoint, 1);
            }
            else if (uvPos == UVPosition.Right)
            {
                secondPoint = GeometryTools.GetPointParallelToLineAtDistance
                     (new XYZ(), upDirection, firstPoint, 1);
            }
            else if (uvPos == UVPosition.Down)
            {
                firstPoint = GeometryTools.GetPointParallelToLineAtDistance
                    (new XYZ(), downDirection, section.ViewerBoundingBox.Max, section.YMaxExtra + BeamTools.GetBeamDims(section.HostElement)[1] + 0.5);
                secondPoint = GeometryTools.GetPointParallelToLineAtDistance
                    (new XYZ(), section.ViewSection.RightDirection, firstPoint, 1);
            }
            line = Line.CreateBound(firstPoint, secondPoint);
            return line;
        }

        static XYZ CalculateDimensionOrigin(DimensionSet dimensionSet, bool isOverallDimension)
        {
            XYZ origin = null;

            double dimensionSpacing = 0;
            if (isOverallDimension) dimensionSpacing = dimensionSet.DimensionsSpacing;

            // Get needed properties
            Section section = dimensionSet.Section;
            UVPosition uvPos = dimensionSet.Position;

            // Get horizontal vector and coordinate origin
            XYZ horzVector = section.ViewSection.RightDirection;
            XYZ coorOrigin = new XYZ();

            // Calculate beam dimensions
            Element hostBeam = section.HostElement;
            
            // Calculate the distance from the edge of the host beam to the bounding box boudary
            //double edgeToBoundingBox;
            //double dimToBeamEdge = .5; // dimensionSet.DimensionToBeamEdge;
            if (uvPos == UVPosition.Left)
            {
                //edgeToBoundingBox = section.XMaxExtra - hostWidth / 2;
                origin = GeometryTools.GetPointParallelToLineAtDistance(horzVector, coorOrigin, section.ViewerBoundingBox.Max, .5 + dimensionSpacing); // dimToBeamEdge - edgeToBoundingBox);
            }
            else if (uvPos == UVPosition.Right)
            {
                //edgeToBoundingBox = section.XMinExtra - hostWidth / 2;
                origin = GeometryTools.GetPointParallelToLineAtDistance(coorOrigin, horzVector, section.ViewerBoundingBox.Min, .5 + dimensionSpacing); // dimToBeamEdge - edgeToBoundingBox);
            }

            return origin;
        }

        // Place break line for side beams
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
                midElevToBbBoundary = section.ViewerBoundingBox.Max.Z - midElevation;
                movedPoint = GeometryTools.GetPointParallelToLineAtDistance(XYZ.Zero, section.ViewSection.RightDirection, section.ViewerBoundingBox.Max, 0.1);
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
    }
}
