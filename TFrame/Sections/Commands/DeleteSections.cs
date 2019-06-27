using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using TFrame.TTools;

namespace TFrame.Sections.Commands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class DeleteSections : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                Document doc = uidoc.Document;

                SelectionTools selTools = new SelectionTools(commandData);
                List<Element> beams = selTools.GetElemsOfCatFromSelection(BuiltInCategory.OST_StructuralFraming);
                List<Element> cols = selTools.GetElemsOfCatFromSelection(BuiltInCategory.OST_StructuralColumns);
                var elems = beams.Concat(cols);

                using (Transaction t = new Transaction(doc, "T Delete Sections"))
                {
                    t.Start();
                    foreach (Element elem in elems)
                    {
                        List<ViewSection> sections = SectionTools.CacheViewSections(elem, doc);
                        foreach (ViewSection vs in sections)
                        {
                            doc.Delete(vs.Id);
                        }
                    }
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
