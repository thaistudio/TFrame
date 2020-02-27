using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.ExtensibleStorage;

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


                beams = SelectionTools.GetElemsOfCatFromSelection(BuiltInCategory.OST_StructuralFraming);
                while (beams.Count == 0) beams = SelectionTools.UrgeSelection(BuiltInCategory.OST_StructuralFraming);

                foreach (Element beam in beams)
                {
                    cachedSections1Beam = SectionTools.CacheSections(beam, doc, SectionTools.SectionType.LongSection);
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
                                SectionActions.DefineSection(beam, sec);
                                ViewSection viewSection = ViewSection.CreateSection(doc, sec.ViewFamilyType.Id, sec.BoundingBox);
                                viewSection.LookupParameter("T_Mark").Set(beam.LookupParameter("Mark").AsString());

                                sec.Host = viewSection.LookupParameter("T_Mark").AsString();

                                Schema longSetionSchema = DataTools.CreateASchema<string>("LongSection", SchemaType.SimpleField);
                                Schema sectionOffsetSchema = DataTools.CreateASchema<string>("LongSection", SchemaType.SimpleField);
                                Schema sectionViewFamilyTypeSchema = DataTools.CreateASchema<string>("LongSection", SchemaType.SimpleField);
                                Schema sectionTemplateSchema = DataTools.CreateASchema<string>("LongSection", SchemaType.SimpleField);
                                Schema sectionHostSchema = DataTools.CreateASchema<string>("LongSection", SchemaType.SimpleField);

                                DataTools.SaveSimpleDataToElement<string>(viewSection, "LongSection", "Direction", DisplayUnitType.DUT_UNDEFINED, longSetionSchema);
                                DataTools.SaveSimpleDataToElement<string>(viewSection, sec.Offset.ToString(), "Offset", DisplayUnitType.DUT_UNDEFINED, sectionOffsetSchema);
                                DataTools.SaveSimpleDataToElement<string>(viewSection, sec.ViewFamilyType.Name, "ViewFamilyType", DisplayUnitType.DUT_UNDEFINED, sectionViewFamilyTypeSchema);
                                DataTools.SaveSimpleDataToElement<string>(viewSection, sec.Template.Name, "Template", DisplayUnitType.DUT_UNDEFINED, sectionTemplateSchema);
                                DataTools.SaveSimpleDataToElement<string>(viewSection, sec.Host, "Host", DisplayUnitType.DUT_UNDEFINED, sectionHostSchema);

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
