using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;

using TFrame.TTools;

namespace TFrame.CreateDimensions.Commands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class cmdDimension : IExternalCommand
    {
        public Document doc;
        public ExternalCommandData _commandData;
        public List<Element> selBeams = new List<Element>();

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                _commandData = commandData;
                doc = commandData.Application.ActiveUIDocument.Document;

                SelectionTools selTools = new SelectionTools(commandData);

                selBeams = selTools.GetElemsOfCatFromSelection(BuiltInCategory.OST_StructuralFraming);
                while (selBeams.Count == 0) selBeams = selTools.UrgeSelection(BuiltInCategory.OST_StructuralFraming);

                UIDocument uiddoc = commandData.Application.ActiveUIDocument;
                Selection sel = uiddoc.Selection;

                //foreach (ElementId id in sel.GetElementIds())
                //{
                //    Element e = doc.GetElement(id);
                //    Wall wall = (Wall)e;
                //}
                //GeometryTools.Learn(selBeams);


                //XYZ a = new XYZ(31.079,-7.201,0);
                //XYZ b = new XYZ(30.375, -5.691,0);
                //XYZ c = new XYZ(31.288, -8.686,0);
                //XYZ d = new XYZ();

                //GeometryTools gTools = new GeometryTools();
                //XYZ beamVector = b.Subtract(a);
                //XYZ normal = gTools.GetPointAtDistNormalToAVector(beamVector, a, 1, 0);

                //double ang = UnitUtils.Convert(GeometryTools.AngleBetween2Vector(a, normal, a, c), DisplayUnitType.DUT_RADIANS, DisplayUnitType.DUT_DECIMAL_DEGREES);

                

                using (Transaction t = new Transaction(doc, "Create Dimensions"))
                {
                    t.Start();
                    foreach (Element beam in selBeams)
                    {
                        XYZ p00 = BeamTools.GetBeamPoint(beam, BeamPoint.P0);
                        XYZ p10 = BeamTools.GetBeamPoint(beam, BeamPoint.P1);
                        XYZ p2 = BeamTools.GetBeamPoint(beam, BeamPoint.P2);
                        XYZ p3 = BeamTools.GetBeamPoint(beam, BeamPoint.P3);
                        XYZ p4 = BeamTools.GetBeamPoint(beam, BeamPoint.P4);
                        XYZ p5 = BeamTools.GetBeamPoint(beam, BeamPoint.P5);
                        XYZ p6 = BeamTools.GetBeamPoint(beam, BeamPoint.P6);
                        XYZ p7 = BeamTools.GetBeamPoint(beam, BeamPoint.P7);


                        Face f3 = BeamTools.GetBeamFace(beam, BeamFace.Face3);

                        Mesh m = f3.Triangulate();
                        var l  = m.Vertices;
                        string s0 = f3.Reference.ConvertToStableRepresentation(doc);
                        string beamUniqueId0 = beam.UniqueId;
                        string rep0 = beamUniqueId0 + ":0:INSTANCE:" + s0;
                        Reference instanceRef0 = Reference.ParseFromStableRepresentation(doc, rep0);

                        Face f5 = BeamTools.GetBeamFace(beam, BeamFace.Face5);

                        Mesh m1 = f5.Triangulate();
                        var l1 = m1.Vertices;
                        string s1 = f5.Reference.ConvertToStableRepresentation(doc);
                        string beamUniqueId1 = beam.UniqueId;
                        string rep1 = beamUniqueId1 + ":0:INSTANCE:" + s1;
                        Reference instanceRef1 = Reference.ParseFromStableRepresentation(doc, rep1);

                        XYZ p0 = new XYZ(42.9662924, -8.992850299, 0);
                        XYZ p1 = new XYZ(42.9662924, -9.484976283, 0);
                        Line line = Line.CreateBound(p1, p0);
                        ReferenceArray referenceArray = new ReferenceArray();
                        referenceArray.Append(instanceRef0);
                        referenceArray.Append(instanceRef1);

                        Dimension dimension = doc.Create.NewDimension(doc.ActiveView, line, referenceArray);
                    }
                    t.Commit();
                }
                    


                DataTools.WriteErrors(GlobalParams.ErrorPath, GlobalParams.Errors);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                GlobalParams.Errors.Add(ex.Message + ex.StackTrace);
                DataTools.WriteErrors(GlobalParams.ErrorPath, GlobalParams.Errors);
                message = ex.Message + ex.StackTrace;
                return Result.Failed;
            }
        }
    }
}
