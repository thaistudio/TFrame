using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TFrame
{
    public class DimensionableReference
    {
        public DimensionableReference(Element beam, Face face, double faceElevation, DimensionSet dimensionSet)
        {
            RevitFace = face;
            FaceElevation = faceElevation;
            DimensionSet = dimensionSet;
            _beam = beam;
        }

        public DimensionSet DimensionSet {get; set;}
        public string Name { get; set; }
        public Reference RevitReference { get { return ElementTools.GetFaceReference(RevitFace, _beam); } }
        public Face RevitFace { get; set; }
        public double FaceElevation { get; set; }
        public Line DimensionLine { get; set; }
        private Element _beam;
    }
}
