using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;

namespace TFrame
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class TagMultiRefAnnotation : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Document doc = commandData.Application.ActiveUIDocument.Document;

                Initialize.InitializeDocument(doc);

                TagActions tagAction = new TagActions(commandData);

                List<Element> selBeams = SelectionTools.GetElemsOfCatFromSelection(BuiltInCategory.OST_StructuralFraming);
                while (selBeams.Count == 0) selBeams = SelectionTools.UrgeSelection(BuiltInCategory.OST_StructuralFraming);

                Dictionary<Element, Dictionary<Section, Dictionary<List<List<Rebar>>, List<Rebar>>>> bigDic =
                    new Dictionary<Element, Dictionary<Section, Dictionary<List<List<Rebar>>, List<Rebar>>>>();

                foreach (Element selectedBeam in selBeams)
                {
                    Dictionary<Section, Dictionary<List<List<Rebar>>, List<Rebar>>> big = tagAction.GroupSectionWithRebars(selectedBeam);
                    bigDic.Add(selectedBeam, big);
                }
                
                using (Transaction t1 = new Transaction(doc, "Load Family"))
                {
                    t1.Start();
                    tagAction.LoadTagFamily();
                    t1.Commit();
                }

                using (TagRebarsForm form = new TagRebarsForm(tagAction))
                {
                    form.ShowDialog();
                }

                if (tagAction.IsOK)
                {
                    using (Transaction t = new Transaction(doc, "Tag Multi Rebars"))
                    {
                        t.Start();
                        foreach (KeyValuePair<Element, Dictionary<Section, Dictionary<List<List<Rebar>>, List<Rebar>>>> pair in bigDic)
                        {
                            Dictionary<Section, Dictionary<List<List<Rebar>>, List<Rebar>>> big = pair.Value;
                            Element selectedBeam = pair.Key;
                            tagAction.TagMultiRebars(selectedBeam, big);
                        }
                        t.Commit();
                    }
                }

                DataTools.WriteErrors(GlobalParams.ErrorPath, GlobalParams.Errors);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                GlobalParams.Errors.Add(ex.Message + ex.StackTrace);
                message = ex.Message;
                DataTools.WriteErrors(GlobalParams.ErrorPath, GlobalParams.Errors);
                return Result.Failed;
            }
        }
    }
}
