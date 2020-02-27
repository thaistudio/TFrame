using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;

namespace TFrame
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class TagMultiRefAnnotation : TCommand
    {
        public TagMultiRefAnnotation() : base("Tag Multi Rebars", false) { }

        protected override Result MainMethod()
        {
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

            return Result.Succeeded;
        }
    }
}
