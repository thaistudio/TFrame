using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TFrame
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class DeleteSections : TCommand
    {
        public DeleteSections() : base("Delete Sections", true) { }

        protected override Result MainMethod()
        {
            List<Element> beams = SelectionTools.GetElemsOfCatFromSelection(BuiltInCategory.OST_StructuralFraming);
            List<Element> cols = SelectionTools.GetElemsOfCatFromSelection(BuiltInCategory.OST_StructuralColumns);
            var elems = beams.Concat(cols);

            foreach (Element elem in elems)
            {
                List<ViewSection> sections = SectionTools.CacheViewSections(elem, doc);
                foreach (ViewSection vs in sections)
                {
                    doc.Delete(vs.Id);
                }
            }

            return Result.Succeeded;
        }
    }
}
