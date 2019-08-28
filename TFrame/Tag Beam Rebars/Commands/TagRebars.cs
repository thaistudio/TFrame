using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;

namespace TFrame
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class TagRebars : IExternalCommand
    {
        List<Rebar> Rebars = new List<Rebar>();
        List<Autodesk.Revit.DB.Reference> References = new List<Autodesk.Revit.DB.Reference>();
        XYZ LeaderElbow;
        XYZ LeaderEnd;
        List<ElementId> ViewIds = new List<ElementId>();

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Document doc = commandData.Application.ActiveUIDocument.Document;
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;

                Initialize.InitializeDocument(doc);

                Selection sel = uiDoc.Selection;
                ICollection<ElementId> selIds = sel.GetElementIds();

                SectionTools secTool = new SectionTools(commandData);

                foreach (ElementId selId in selIds)
                {
                    Element selElem = doc.GetElement(selId);
                    Category cat = selElem.Category;
                    Category strFramingCat = Category.GetCategory(doc, BuiltInCategory.OST_StructuralFraming);

                    if (selElem is IndependentTag)
                    {
                        IndependentTag t = (IndependentTag)selElem;
                    }
                    if (selElem is Rebar)
                    {
                        Rebar rebar = (Rebar)selElem;
                        Rebars.Add(rebar);
                        Autodesk.Revit.DB.Reference reference = new Autodesk.Revit.DB.Reference(rebar);
                        References.Add(reference);
                    }
                    else if (cat.Name == strFramingCat.Name)
                    {
                        List<Rebar> rebars = BeamTools.CacheRebars(selElem, doc);
                        foreach (Rebar rebar in rebars)
                        {
                            Autodesk.Revit.DB.Reference reference = new Autodesk.Revit.DB.Reference(rebar);
                            References.Add(reference);
                        }

                        List<Section> sections = SectionTools.CacheSections(selElem, doc, SectionTools.SectionType.CrossSection);
                        foreach (Section section in sections)
                        {
                            View view = section.ViewSection;
                            ViewIds.Add(view.Id);
                        }
                    }
                }
                
                // Elbow
                LeaderElbow = new XYZ(2.47332910, -15.7435844688853, 0.7042322834);
                LeaderEnd = new XYZ(2.47332910, -15.7435844688853, -0.54576771653544);

                bool addLeader = true;

                XYZ pnt = new XYZ(2.98836717912826, -16.6007517695874, LeaderElbow.Z - 0.030891335);

                FamilyTools.SearchFamilySymbolByBuiltInCategory(BuiltInCategory.OST_RebarTags);
                ElementId desiredFamily = FamilyTools.SearchFamilySymbolByBuiltInCategory(BuiltInCategory.OST_RebarTags)[2].Id;

                using (Transaction t = new Transaction(doc, "Tag Rebars by Category"))
                {
                    t.Start();
                    foreach (Autodesk.Revit.DB.Reference referenceToTag in References)
                    {
                        foreach (ElementId ownerDBViewId in ViewIds)
                        {
                            IndependentTag tagRebars = IndependentTag.Create(doc, desiredFamily, ownerDBViewId, referenceToTag, addLeader, TagOrientation.Horizontal, pnt);
                            tagRebars.LeaderEndCondition = LeaderEndCondition.Free;
                            tagRebars.LeaderElbow = LeaderElbow;
                            tagRebars.LeaderEnd = LeaderEnd;
                            tagRebars.TagHeadPosition = pnt;
                        }
                    }
                    t.Commit();
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
