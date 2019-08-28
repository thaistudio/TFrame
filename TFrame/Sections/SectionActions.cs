using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TFrame
{
    public class SectionActions
    {
        ExternalCommandData _externalCommandData;
        Document _doc;
        UIDocument _uiDoc;

        public SectionActions(ExternalCommandData externalCommandData)
        {
            _externalCommandData = externalCommandData;
            _uiDoc = externalCommandData.Application.ActiveUIDocument;
            _doc = _uiDoc.Document;
        }

        BeamTools bTools = new BeamTools();
        GeometryTools gTools = new GeometryTools();
        

        /// <summary>
        /// This method get 2 ends of the line that origin runs 
        /// </summary>
        static public List<XYZ> GetOriginLine(Element e)
        {
            List<XYZ> originLine = new List<XYZ>();
            List<XYZ> boundingPoints = BeamTools.GetBeamBoundingPoints(e);
            XYZ top1 = boundingPoints[0];
            XYZ top2 = boundingPoints[1];
            XYZ top3 = boundingPoints[2];
            XYZ top4 = boundingPoints[3];
            XYZ p0 = GeometryTools.GetMidPoint(top1, top2);
            XYZ p1 = GeometryTools.GetMidPoint(top4, top3);
            originLine.Add(p0);
            originLine.Add(p1);
            return originLine;
        }

        static public XYZ GetSectionOriginByOffset(Element e, double offset)
        {
            XYZ p0 = GetOriginLine(e)[0];
            XYZ p1 = GetOriginLine(e)[1];
            XYZ originNoOffset = GeometryTools.GetMidPoint(p0, p1);
            XYZ paramVector = BeamTools.GetUnitNormalVector(p0, p1);
            XYZ originWithOffset = GeometryTools.GetPointAtDistNormalToAVector(paramVector, originNoOffset, -offset, p0.Z);

            return originWithOffset;
        }

        static public void BoundingBoxTransform(Element e, Section section)
        {
            XYZ p0 = GetOriginLine(e)[0];
            XYZ p1 = GetOriginLine(e)[1];

            section.Origin = GetSectionOriginByOffset(e, section.Offset);
            Transform trans = null;
            trans = Transform.Identity;
            trans.Origin = section.Origin;
            trans.BasisY = new XYZ(0, 0, 1);
            trans.BasisZ = SectionTools.GetBasisZ(p0, p1);
            trans.BasisX = SectionTools.GetBasisX(trans.BasisZ);
            section.Tran = trans;
        }

        static public void DefineSection(Element e, Section section)
        {
            BoundingBoxTransform(e, section);
            BoundingBoxXYZ bb = new BoundingBoxXYZ();
            bb.Enabled = true;
            SectionTools.GetBBCoordinates(e, section, bb);
            bb.Transform = section.Tran;
            section.BoundingBox = bb;
        }
    }
}
