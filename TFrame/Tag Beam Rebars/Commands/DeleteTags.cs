using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;

using TFrame.TTools;

namespace TFrame.Tag
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class DeleteTags : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;
                Document doc = uiDoc.Document;
                Selection sel = uiDoc.Selection;

                BeamTools btools = new BeamTools();
                SelectionTools selTools = new SelectionTools(commandData);
                List<Element> selBeams = selTools.GetElemsOfCatFromSelection(BuiltInCategory.OST_StructuralFraming);
                while (selBeams.Count == 0) selBeams = selTools.UrgePickSelection(uiDoc);

                using (Transaction t = new Transaction(doc, "Delete Rebars"))
                {
                    t.Start();

                    foreach (Element e in selBeams)
                    {
                        List<Rebar> rebars = btools.CacheRebars(e, doc);
                        foreach (Rebar rebar in rebars)
                        {
                            List<IndependentTag> indTags = CacheIndepentTags(doc, rebar);
                            List<MultiReferenceAnnotation> multiTags = CacheMultiTags(doc, rebar);

                            foreach (IndependentTag indTag in indTags)
                            {
                                doc.Delete(indTag.Id);
                            }

                            foreach (MultiReferenceAnnotation multiTag in multiTags)
                            {
                                doc.Delete(multiTag.Id);
                            }
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


        public List<IndependentTag> CacheIndepentTags(Document doc, Rebar rebar)
        {
            List<IndependentTag> indTagList = new List<IndependentTag>();
            FilteredElementCollector indTags = new FilteredElementCollector(doc).OfClass(typeof(IndependentTag)).WhereElementIsNotElementType();
            foreach (IndependentTag indTag in indTags)
            {
                ElementId tagId = indTag.TaggedLocalElementId;
                if (tagId == rebar.Id) indTagList.Add(indTag);
            }
            return indTagList;
        }

        public List<MultiReferenceAnnotation> CacheMultiTags(Document doc, Rebar rebar)
        {
            List<MultiReferenceAnnotation> multiTagList = new List<MultiReferenceAnnotation>();
            FilteredElementCollector multiTags = new FilteredElementCollector(doc).OfClass(typeof(MultiReferenceAnnotation)).WhereElementIsNotElementType();
            foreach (MultiReferenceAnnotation multiTag in multiTags)
            {
                ElementId dimId = multiTag.DimensionId;
                Dimension dim = (Dimension)doc.GetElement(dimId);
                ReferenceArray array = dim.References;
                foreach (Reference reff in array)
                {
                    ElementId rebarId = reff.ElementId;
                    if (rebarId == rebar.Id)
                    {
                        multiTagList.Add(multiTag);
                        break;
                    }
                }
            }
            return multiTagList;
        }
    }
}
