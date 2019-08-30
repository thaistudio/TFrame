using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace TFrame
{
    public class DimensionSet : DataBinding
    {
        public DimensionSet(Section section, UVPosition uvPos)
        {
            References = new List<DimensionableReference>();
            Dimensions = new List<Dimension>();
            Position = uvPos;
            Name = uvPos.ToString();
            Section = section;
        }

        public TView TVIew { get; set; }
        public double DimensionsSpacing { get; set; } = 0.5;
        public List<DimensionableReference> References { get; set; }
        public List<Dimension> Dimensions { get; set; }
        public UVPosition Position { get; set; }
        public SectionType SectionType { get; set; }
        public double DimensionToBeamEdge { get; set; }
        public bool HasOverallDimension { get { return _hasOverallDimension(); } }
        public DimensionType DimensionType
        {
            get { return BeamDimensionData.Singleton.DimensionType; }
        }
        public string Name { get; set; }
        
        public Section Section { get; set; }

        public void AddReference(Element beam, Face face, double faceElevation, DimensionSet dimensionSet)
        {
            if (ContainsReference(faceElevation) && dimensionSet.Position != UVPosition.Down) return;
            DimensionableReference tRef = new DimensionableReference(beam, face, faceElevation, dimensionSet);
            References.Add(tRef);
        }

        public void CollectReferencesOfBeam(Element beam)
        {
            if (Position == UVPosition.Down) // Horizontal dimensions
            {
                Face leftFace = BeamTools.GetBeamFace(beam, BeamFace.Face5);
                Face rightFace = BeamTools.GetBeamFace(beam, BeamFace.Face3);

                AddReference(beam, leftFace, 0, this);
                AddReference(beam, rightFace, 0, this);
            }
            else // Side dimensions
            {
                // Get beam's top and bottom faces
                Face botFace = BeamTools.GetBeamFace(beam, BeamFace.Face4);
                Face topFace = BeamTools.GetBeamFace(beam, BeamFace.Face2);

                // Get beam faces' elevations
                double botElevation = BeamTools.GetBeamElevation(beam, BeamFace.Face4);
                double topElevation = BeamTools.GetBeamElevation(beam, BeamFace.Face2);

                AddReference(beam, botFace, botElevation, this);
                AddReference(beam, topFace, topElevation, this);
            }
        }

        bool ContainsReference(double faceElevation)
        {
            if (References.Any(x => x.FaceElevation == faceElevation)) return true;
            else return false;
        }

        private bool _hasOverallDimension()
        {
            if (References.Count > 2) return true;
            else return false;
        }
    }

    public enum UVPosition
    {
        Left,
        Right,
        Up,
        Down
    }
}
