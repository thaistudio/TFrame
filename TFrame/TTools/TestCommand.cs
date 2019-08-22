using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace TFrame
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class TestCommand : TCommand
    {
        public TestCommand() : base("Test Command", true)
        {

        }

        protected override Result MainMethod()
        {
            Selection sel = uiDoc.Selection;


            ElementId id = sel.GetElementIds().FirstOrDefault();
            Element e = doc.GetElement(id);
            var v = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views).Where(x => x.Name == "Section 50").FirstOrDefault();
            BoundingBoxXYZ b1 = e.get_BoundingBox(doc.ActiveView);
            BoundingBoxXYZ b2 = e.get_BoundingBox((View)v);
            return Result.Succeeded;
        }
    }
}
