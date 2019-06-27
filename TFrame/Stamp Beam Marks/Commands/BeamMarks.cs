using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using TFrame.TTools;
using TFrame.Stamp_Beam_Marks.Forms;

namespace TFrame.Stamp_Beam_Marks
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class BeamMarks : IExternalCommand
    {
        public List<Settings> settings;
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
                BeamMarksAction bmAction = new BeamMarksAction(commandData);

                selBeams = selTools.GetElemsOfCatFromSelection(BuiltInCategory.OST_StructuralFraming);
                while (selBeams.Count == 0) selBeams = selTools.UrgeSelection(BuiltInCategory.OST_StructuralFraming);

                BeamMarkFormAdvanced formAdvanced = new BeamMarkFormAdvanced(this);
                formAdvanced.ShowDialog();

                if (!GlobalParams.IsOK) return Result.Cancelled;
                using (Transaction t = new Transaction(doc, "Stamp Beam Marks"))
                {
                    t.Start();
                    bmAction.StampBeamMarksAdvanced(selBeams, settings);
                    t.Commit();
                }

                DataTools.WriteErrors(GlobalParams.ErrorPath, GlobalParams.Errors);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                GlobalParams.Errors.Add(ex.Message + ex.StackTrace);
                message = ex.Message;
                DataTools.WriteErrors(GlobalParams.ErrorPath, GlobalParams.Errors);
                return Result.Failed;
            }
        }
    }
}
