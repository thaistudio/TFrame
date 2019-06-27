using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace TFrame.TTools
{
    public class SelectionTools
    {
        ExternalCommandData _commandData;
        UIDocument _uiDoc;
        Document _doc;
        FamilyTools fTools;

        public SelectionTools(ExternalCommandData commandData)
        {
            _commandData = commandData;
            _uiDoc = _commandData.Application.ActiveUIDocument;
            _doc = _uiDoc.Document;
        }

        public List<Element> GetSelectedMembers(ExternalCommandData commandData)
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
        /// Use this method to filter elements of a category from selections (i.e. filter beams)
        /// </summary>
        /// <param name="commandData"></param>
        /// <returns></returns>
        public List<Element> GetElemsOfCatFromSelection(BuiltInCategory category)
        {
            fTools = new FamilyTools(_commandData);
            List<Element> elems = new List<Element>();
            foreach (ElementId id in _uiDoc.Selection.GetElementIds())
            {
                Element elem = _doc.GetElement(id);
                FamilySymbol symbol = fTools.GetFamilySymbol(elem);
                if (symbol != null)
                {
                    string name1 = symbol.Family.FamilyCategory.Name;
                    string name2 = Category.GetCategory(_doc, category).Name;
                    if (name1 == name2) elems.Add(elem);
                }
            }
            return elems;
        }

        public List<Element> GetElemsOfCatFromList(List<Element> elems, BuiltInCategory category)
        {
            fTools = new FamilyTools(_commandData);
            List<Element> list = new List<Element>();
            foreach (Element elem in elems)
            {
                FamilySymbol symbol = fTools.GetFamilySymbol(elem);
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
        public List<Element> UrgeSelection(BuiltInCategory category)
        {
            List<Element> sel = _uiDoc.Selection.PickElementsByRectangle("Please select beams! You can select other elements too, the command will filter beams out.").ToList();
            List<Element> elems = GetElemsOfCatFromList(sel, category);
            return elems;
        }

        public List<Element> UrgePickSelection(UIDocument uiDoc)
        {
            Document doc = uiDoc.Document;
            List<Element> elems = new List<Element>();
            IList<Reference> references = _uiDoc.Selection.PickObjects(ObjectType.Element, "Please select beams!");
            foreach (Reference reference in references)
            {
                Element elem = doc.GetElement(reference);
                elems.Add(elem);
            }
            return elems;
        }
    }
}
