using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.ExtensibleStorage;

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

            TextNote tn = (TextNote)e;
            string name = tn.Text.ToString();
            string n = name.Replace("\r", "");

            //List<Section> sections = SectionTools.CacheSections(e, doc, SectionTools.SectionType.CrossSection);
            //foreach (Section section in sections)
            //{
            //    IList<ElementId> tag = section.RebarTagIds;
            //    IList<ElementId> dims = section.DimIds;
            //}
            

            return Result.Succeeded;
        }
    }
}
