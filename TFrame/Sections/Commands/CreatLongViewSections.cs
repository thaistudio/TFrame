using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace TFrame
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class LongViewSections : IExternalCommand
    {
        public List<Element> beams = new List<Element>();
        public List<ViewFamilyType> sections;
        public List<View> templates;

        public List<Section> cachedSectionsAllBeams = new List<Section>(); // Existing long sections from all beams. This list will be pass to CrossSectionForm
        public List<Section> passedFromFormsSections = new List<Section>(); // Sections received from form, will be compared with cachedSections to get tobeCreatedSections
        public List<Section> delSections = new List<Section>(); //Sections that will be deleted
        public List<Section> tobeCreatedSections = new List<Section>(); // The actual sections that will be created
        public List<Section> cachedSections1Beam; // Existing long sections in each beam

        public List<string> hosts = new List<string>(); // This list will be passed to BoundingBoxSizingFromAuto to show host name (beam's name)

        public Document doc;
        public UIDocument uiDoc;
        public bool IsOK;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                doc = commandData.Application.ActiveUIDocument.Document;
                uiDoc = commandData.Application.ActiveUIDocument;
                Selection sel = uiDoc.Selection;

                Initialize.InitializeDocument(doc);

                SectionActions sAct = new SectionActions(commandData);
                SelectionTools selTools = new SelectionTools(commandData);
                SectionTools secTools = new SectionTools(commandData);
                TStoreData tData = new TStoreData();

                beams = selTools.GetElemsOfCatFromSelection(BuiltInCategory.OST_StructuralFraming);
                while (beams.Count == 0) beams = selTools.UrgeSelection(BuiltInCategory.OST_StructuralFraming);

                foreach (Element beam in beams)
                {
                    cachedSections1Beam = secTools.CacheSections(beam, doc, SectionTools.SectionType.LongSection);
                    cachedSectionsAllBeams.AddRange(cachedSections1Beam);
                    hosts.Add(ElementTools.GetMark(beam));
                }

                // Collect section types
                FilteredElementCollector sectionFilter = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType));

                sections = new List<ViewFamilyType>();
                foreach (Element e in sectionFilter)
                {
                    ViewFamilyType v = (ViewFamilyType)e;
                    var viewFam = v.ViewFamily;
                    if (viewFam == ViewFamily.Section) sections.Add(v);
                }

                // Collect templates
                FilteredElementCollector views = new FilteredElementCollector(doc).OfClass(typeof(View));
                templates = new List<View>();

                foreach (View view in views)
                {
                    if (view.IsTemplate)
                        templates.Add(view);
                }

                using (LongSectionForm form = new LongSectionForm(this))
                {
                    form.ShowDialog();
                }

                if (IsOK) // When OK button hit
                {
                    using (Transaction t = new Transaction(doc, "Create Long View Sections"))
                    {
                        t.Start();
                        foreach (Element beam in beams)
                        {
                            if (FieldClass.IsCleared || FieldClass.IsDeleted || FieldClass.IsUpdated)
                            {
                                foreach (Section sec in delSections)
                                {
                                    if (sec.ViewSection != null)
                                        doc.Delete(sec.ViewSection.Id);
                                }
                                delSections.Clear();
                            }

                            ListTools.CompareList(passedFromFormsSections, cachedSectionsAllBeams, tobeCreatedSections, "Offset");

                            foreach (Section sec in tobeCreatedSections)
                            {
                                sAct.DefineSection(beam, sec);
                                ViewSection viewSection = ViewSection.CreateSection(doc, sec.ViewFamilyType.Id, sec.BoundingBox);
                                viewSection.LookupParameter("T_Mark").Set(beam.LookupParameter("Mark").AsString());

                                sec.Host = viewSection.LookupParameter("T_Mark").AsString();

                                tData.AddInfoToElement<string>(viewSection, "LongSection", "Direction", UnitType.UT_Undefined, DisplayUnitType.DUT_UNDEFINED);
                                tData.AddInfoToElement<string>(viewSection, sec.Offset.ToString(), "Offset", UnitType.UT_Undefined, DisplayUnitType.DUT_UNDEFINED);
                                tData.AddInfoToElement<string>(viewSection, sec.ViewFamilyType.Name, "ViewFamilyType", UnitType.UT_Undefined, DisplayUnitType.DUT_UNDEFINED);
                                tData.AddInfoToElement<string>(viewSection, sec.Template.Name, "Template", UnitType.UT_Undefined, DisplayUnitType.DUT_UNDEFINED);
                                tData.AddInfoToElement<string>(viewSection, sec.Host, "Host", UnitType.UT_Undefined, DisplayUnitType.DUT_UNDEFINED);

                                if (sec.ViewSectionName != null) viewSection.LookupParameter("T_SectionName").Set(sec.ViewSectionName);
                                viewSection.ViewTemplateId = sec.Template.Id;
                                viewSection.CropBoxVisible = false;
                                sec.ViewSection = viewSection;
                            }
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
                DataTools.WriteErrors(GlobalParams.ErrorPath , GlobalParams.Errors);
                return Result.Failed;
            }
        }
    }
}
