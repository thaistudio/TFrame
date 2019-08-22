using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;

namespace TFrame
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class cmdDimension : TCommand
    {
        public List<Element> selBeams = new List<Element>();

        public cmdDimension() : base("Create Beam Dimensions", false) { }
           
        protected override Result MainMethod()
        {
            SelectionTools selTools = new SelectionTools(commandData);
            var v = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Viewers).Where(x => x.Name == "Section 50");
            // Urge users to select beams
            selBeams = selTools.GetElemsOfCatFromSelection(BuiltInCategory.OST_StructuralFraming);
            while (selBeams.Count == 0) selBeams = selTools.UrgeSelection(BuiltInCategory.OST_StructuralFraming);

            // Main actions
            foreach (Element beam in selBeams)
            {
                BeamDimUC form = new BeamDimUC(doc);
                form.ShowDialog();
                SectionTools sTools = new SectionTools(commandData);
                List<Section> sections = sTools.CacheSections(beam, doc, SectionTools.SectionType.CrossSection);
                DimensionActions dimActions = new DimensionActions();

                foreach (Section section in sections)
                {
                    section.SetDimensionProperties();

                    Transaction t = new Transaction(doc);
                    t.Start("Create Dimension");
                    DimensionActions.FindReferenceInView(section, UVPosition.Left);
                    DimensionActions.FindReferenceInView(section, UVPosition.Right);
                    DimensionActions.FindReferenceInView(section, UVPosition.Down);

                   
                    DimensionActions.CreateDimension(section);

                    section.ViewSection.CropBoxVisible = false;
                    t.Commit();
                }
            }
            return Result.Succeeded;
        }
    }
}
