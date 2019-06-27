using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TFrame
{
    
    /// <summary>
    /// Use this class to calculate/get any desired info, like member height, coordinate, ...
    /// </summary>
    public class SupportClass
    {
        private ExternalCommandData m_commandData;

        public SupportClass(ExternalCommandData commandData)
        {
            m_commandData = commandData;
        }
        private List<FamilySymbol> listSymbols;

        public ReadOnlyCollection<FamilySymbol> Symbols
        {
            get => new ReadOnlyCollection<FamilySymbol>(listSymbols);
        }

        public FamilySymbol SelSymbol
        {
            get;
            set;
        }
        /// <summary>
        /// This method return the list of all loaded structural framing family symbols in the document
        /// </summary>
        /// <param name="commandData"></param>
        /// <returns></returns>
        public List<FamilySymbol> GetFamilySymbols()
        {
            listSymbols = new List<FamilySymbol>();
            Document doc = m_commandData.Application.ActiveUIDocument.Document;

            // Get all loaded structural framing family symbols in the document
            FilteredElementCollector symbols = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof(FamilySymbol));
            foreach (FamilySymbol symbol in symbols)
            {
                listSymbols.Add(symbol);
            }

            return listSymbols;
        }

        public bool SetFamilySymbols(object symbol)
        {
            Document doc = m_commandData.Application.ActiveUIDocument.Document;
            SelSymbol = (FamilySymbol)symbol;
            return true;
        }

    }
}
