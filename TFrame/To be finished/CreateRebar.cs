using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;
using Autodesk.Revit.Creation;
using Document = Autodesk.Revit.DB.Document;
using Autodesk.Revit.DB.Structure;

namespace TFrame
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class CreateRebar : IExternalCommand
    {
        
        Element e;
        public List<Level> listLevels;
        public List<FamilySymbol> listSymbols;
        Level lvl;
        public CreateRebar()
        {
        }

        public List<FamilySymbol> Symbols
        {
            get => listSymbols;
        }
        public ReadOnlyCollection<Level> Levels
        {
            get => new ReadOnlyCollection<Level>(listLevels);
        }

        public bool AddToList(ListControl list, List<Level> levels)
        {
            list.DataSource = levels;
            return true;
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Document doc = commandData.Application.ActiveUIDocument.Document;
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;
                Selection sel = uiDoc.Selection;

                Autodesk.Revit.Creation.Document docCreation = doc.Create;

                

                Autodesk.Revit.DB.View view = doc.ActiveView;

                XYZ p1 = new XYZ();
                XYZ p2 = new XYZ(10, 0, 0);
                Curve curve = Line.CreateBound(p1, p2);
                

                ICollection < ElementId > listId = sel.GetElementIds();

                foreach (ElementId eId in listId)
                {
                    e = doc.GetElement(eId);
                }
                string symbolName = null;
                FilteredElementCollector elems = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming);
                int count = elems.Count();
                FilteredElementCollector symbols = elems.OfClass(typeof(FamilySymbol));
                int count2 = elems.Count();

                FilteredElementCollector levels = new FilteredElementCollector(doc).OfClass(typeof(Level));
                int levelscount = levels.Count();
                IList<Element> lvls = levels.ToElements();
                int lvlscount = lvls.Count;
                listLevels = new List<Level>();
                foreach (Level level in lvls)
                {
                    listLevels.Add(level);
                    lvl = level;
                    break;
                }
                SupportClass sup = new SupportClass(commandData);
                sup.GetFamilySymbols();
                TUI UI = new TUI(sup);
                UI.ShowDialog();

                listSymbols = new List<FamilySymbol>();
                //foreach (FamilySymbol symbol in symbols)
                //{

                //    listSymbols.Add(symbol);
                //    using (Transaction trans = new Transaction(doc, "Add beam"))
                //    {
                //        trans.Start();
                //        symbol.Activate();
                //        symbolName = symbol.Name;
                //        FamilyInstance newBeam = docCreation.NewFamilyInstance(curve, symbol, lvl, StructuralType.Beam);
                //        trans.Commit();
                //    }
                //    break;
                //}

                using (Transaction trans = new Transaction(doc, "Add beam"))
                {
                    trans.Start();
                    sup.SelSymbol.Activate();
                    symbolName = sup.SelSymbol.Name;
                    FamilyInstance newBeam = docCreation.NewFamilyInstance(curve, sup.SelSymbol, lvl, StructuralType.Beam);
                    trans.Commit();
                }


                TaskDialog.Show(symbolName, "Total structural framing elements: " + count.ToString() + "/n Number of Family Symbol is: " + count2.ToString());
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
            }
           
            return Result.Succeeded;
        }
    }
}
