using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace TFrame
{ 
    public class SelectionTools
    {
        static ExternalCommandData _commandData;
        static UIDocument _uiDoc;
        static Document _doc;
        FamilyTools fTools;

        public SelectionTools(ExternalCommandData commandData)
        {
            _commandData = GlobalParams.ExternalCommandData;
            _uiDoc = _commandData.Application.ActiveUIDocument;
            _doc = _uiDoc.Document;
        }

        public static List<Element> GetSelectedMembers(ExternalCommandData commandData)
        {
            List<Element> elems = new List<Element>();
            foreach (ElementId id in _uiDoc.Selection.GetElementIds())
            {
                Element elem = _doc.GetElement(id);
                elems.Add(elem);
            }
            return elems;
        }

        /// <summary>
        /// Use this method to filter elements of a category from selections (i.e. select beams only)
        /// </summary>
        /// <param name="commandData"></param>
        /// <returns></returns>
        public static List<Element> GetElemsOfCatFromSelection(BuiltInCategory category)
        {
            List<Element> elems = new List<Element>();
            foreach (ElementId id in _uiDoc.Selection.GetElementIds())
            {
                Element elem = _doc.GetElement(id);
                FamilySymbol symbol = FamilyTools.GetFamilySymbol(elem);
                if (symbol != null)
                {
                    string name1 = symbol.Family.FamilyCategory.Name;
                    string name2 = Category.GetCategory(_doc, category).Name;
                    if (name1 == name2) elems.Add(elem);
                }
            }
            return elems;
        }

        public static List<Element> GetElemsOfCatFromList(List<Element> elems, BuiltInCategory category)
        {
            List<Element> list = new List<Element>();
            foreach (Element elem in elems)
            {
                FamilySymbol symbol = FamilyTools.GetFamilySymbol(elem);
                if (symbol != null)
                {
                    string name1 = symbol.Family.FamilyCategory.Name;
                    string name2 = Category.GetCategory(_doc, category).Name;
                    if (name1 == name2) list.Add(elem);
                }
            }
            return list;
        }

        /// <summary>
        /// Urge the users to select 
        /// </summary>
        public static List<Element> UrgeSelection(BuiltInCategory category)
        {
            List<Element> sel = _uiDoc.Selection.PickElementsByRectangle("Please select beams! You can select other elements too, the command will filter beams out.").ToList();
            List<Element> elems = GetElemsOfCatFromList(sel, category);
            return elems;
        }

        public static List<Element> UrgePickSelection(UIDocument uiDoc)
        {
            Document doc = uiDoc.Document;
            List<Element> elems = new List<Element>();
            IList<Autodesk.Revit.DB.Reference> references = _uiDoc.Selection.PickObjects(ObjectType.Element, "Please select beams!");
            foreach (Autodesk.Revit.DB.Reference reference in references)
            {
                Element elem = doc.GetElement(reference);
                elems.Add(elem);
            }
            return elems;
        }
    }
}
