using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TFrame.Experiments
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class StampBeamsID : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                Document doc = uidoc.Document;

                FilteredElementCollector filter = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming).WhereElementIsNotElementType();


                using (Transaction t = new Transaction(doc, "Stamp beam ID"))
                {
                    t.Start();
                    foreach (Element beam in filter)
                    {
                        beam.LookupParameter("ID").Set(beam.Id.ToString());
                    }
                    t.Commit();
                }
                  
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
