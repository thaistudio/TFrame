using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace TFrame
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class Test : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Document doc = commandData.Application.ActiveUIDocument.Document;
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;
                List<Element> elems = new List<Element>();

                Selection sel = uiDoc.Selection;
                foreach (ElementId id in sel.GetElementIds())
                {
                    elems.Add(doc.GetElement(id));
                }
                double h = elems[0].LookupParameter("h").AsDouble();
                foreach (ElementId elementId in sel.GetElementIds())
                {
                    Element e = doc.GetElement(elementId);

                    using (Transaction t = new Transaction(doc, "Data"))
                    {
                        t.Start();
                        //TStoreData tStore = new TStoreData();
                        //tStore.AddInfoToElement(e, 10, "tField", UnitType.UT_Length, DisplayUnitType.DUT_MILLIMETERS);
                        //var v = tStore.GetInfoFromElement<int>(e, "tField", DisplayUnitType.DUT_MILLIMETERS);
                        
                        TaskDialog.Show("s", h.ToString());

                        t.Commit();
                    }
                }

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
            
        }
    }
}
