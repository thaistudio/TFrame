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
    public class FamilyTools
    {
        Document _doc;

        public FamilyTools()
        {
            _doc = GlobalParams.Doc;
        }

        /// <summary>
        /// Return family symbols of a built-in category
        /// </summary>
        /// <param name="builtInCategory"></param>
        public List<FamilySymbol> SearchFamilySymbolByBuiltInCategory(BuiltInCategory builtInCategory)
        {
            List<FamilySymbol> familySymbols = new List<FamilySymbol>();
            FilteredElementCollector elements = new FilteredElementCollector(_doc).OfCategory(builtInCategory).WhereElementIsElementType();
            foreach (Element element in elements)
            {
                var v = element.Category;
                if (element is FamilySymbol)
                {
                    FamilySymbol familySymbol = (FamilySymbol)element;
                    familySymbols.Add(familySymbol);
                }
            }
            return familySymbols;
        }

        public List<FamilySymbol> SearchFamilySymbolByFamily(Family family)
        {
            List<FamilySymbol> familySymbols = new List<FamilySymbol>();

            ISet<ElementId> symbolIds = family.GetFamilySymbolIds();

            foreach (ElementId symbolId in symbolIds)
            {
                Element elemSymbol = _doc.GetElement(symbolId);
                FamilySymbol familySymbol = (FamilySymbol)elemSymbol;
                familySymbols.Add(familySymbol);
            }

            return familySymbols;
        }

        /// <summary>
        /// Return list of families based on type (a.k.a. class)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public object SearchFamilyByType<T> ()
        {
            List<Family> list = new List<Family>();
            List<ElementId> typeIds = new List<ElementId>(); 
            Element elemOfType = new FilteredElementCollector(_doc).OfClass(typeof(T)).FirstElement();
            
            ElementId typeId = elemOfType.GetTypeId();
            Element type = _doc.GetElement(typeId);
            FamilySymbol famSymbol = (FamilySymbol)type;
            Family fam = famSymbol.Family;
            ElementId catId = fam.FamilyCategoryId;

            FilteredElementCollector families = new FilteredElementCollector(_doc).OfClass(typeof(Family));

            foreach (Family family in families)
            {
                if (family.FamilyCategoryId == catId)
                {
                    list.Add(family);
                }
            }
            return list;
        }


        public List<Family> SearchFamilyByBuiltInCat (BuiltInCategory cat)
        {
            List<Family> list = new List<Family>();
            FamilySymbol symbol = SearchFamilySymbolByBuiltInCategory(cat).FirstOrDefault();
            Family fam = symbol.Family;
            ElementId catId = fam.FamilyCategoryId;

            FilteredElementCollector families = new FilteredElementCollector(_doc).OfClass(typeof(Family));

            foreach (Family family in families)
            {
                if (family.FamilyCategoryId == catId)
                {
                    list.Add(family);
                }
            }
            return list;
        }

        public List<Element> SearchFamilySymbolAsElements<T>(bool IsElementType)
        {
            List<Element> ts = new List<Element>();
            List<FamilySymbol> familySymbols = new List<FamilySymbol>();
            FilteredElementCollector filter;
            if (IsElementType)
            {
                filter = new FilteredElementCollector(_doc).OfClass(typeof(T)).WhereElementIsElementType();
            }
            else
            {
                filter = new FilteredElementCollector(_doc).OfClass(typeof(T)).WhereElementIsNotElementType();
            }

            foreach (Element element in filter)
            {
                ts.Add(element);
            }
            return ts;
        }

        public List<Element> SearchFamilySymbolOfFamilyName<T>(string familyName)
        {
            List<Element> ts = new List<Element>();
            List<FamilySymbol> familySymbols = new List<FamilySymbol>();
            FilteredElementCollector filter;
            filter = new FilteredElementCollector(_doc).OfClass(typeof(T)).WhereElementIsElementType();
            foreach (ElementType elementType in filter)
            {
                if (elementType.FamilyName == familyName)
                    ts.Add(elementType);
            }
            return ts;
        }

        public static bool FamilyExists(string familyName)
        {
            IEnumerable<Element> families = new FilteredElementCollector(GlobalParams.Doc).OfClass(typeof(Family)).Where(x => x.Name == familyName);
            if (families.Count() > 0) return true;
            return false;
        }

        public FamilySymbol GetFamilySymbol(Element e)
        {
            if (e is FamilyInstance)
            {
                FamilyInstance familyInstance = (FamilyInstance)e;
                FamilySymbol symbol = familyInstance.Symbol;
                return symbol;
            }
            else return null;
        }

        /// <summary>
        /// Use this method to find an instance of a type based on its name (string)
        /// </summary>
        /// <typeparam name="T">Type or Class</typeparam>
        /// <param name="name">Type's name</param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static List<Element> GetTypeInstanceByName<T>(string name, Document doc)
        {
            List<Element> elems = new List<Element>();
            FilteredElementCollector filter = new FilteredElementCollector(doc).OfClass(typeof(T));
            foreach (Element e in filter)
            {
                if (e.Name == name) elems.Add(e);
            }
            return elems;
        }

        /// <summary>
        /// Load a family
        /// </summary>
        /// <param name="path"></param>
        /// <param name="famName"></param>
        public static void LoadFamily(string path, string famName = null)
        {
            if (!FamilyExists(famName)) GlobalParams.Doc.LoadFamily(path);
        }
    }
}
