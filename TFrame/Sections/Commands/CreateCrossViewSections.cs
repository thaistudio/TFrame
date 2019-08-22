using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


namespace TFrame
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    //[Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class CrossViewSections : IExternalCommand
    {
        public List<View> delViews { get; set; }

        public List<View> templates;
        public List<ViewFamilyType> Sections;
        public List<string> UniqueIds = new List<string>();

        bool IsBeam = true;

        public List<Element> selBeams = new List<Element>(); //Hold all selected beams
        public List<string> hosts = new List<string>(); // This list will be passed to BoundingBoxSizingFromAuto to show host name (beam's name)

        public List<Section> cachedSectionsAllBeams = new List<Section>(); // Existing cross sections from all beams. This list will be pass to CrossSectionForm
        public List<Section> passedFromFormsSections; // Sections received from form, will be compared with cachedSections to get tobeCreatedSections
        public List<Section> delSections; //Sections that will be deleted
        public List<Section> cachedSections1Beam; // Existing crosssections in each beam
        public List<Section> tobeCreatedSections; // The actual sections that will be created

        public bool IsExecuted; // Make the OK button the only button trigger transaction

        FieldClass field = new FieldClass();

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                BeamTools tAct = new BeamTools();
                SectionTools secTool = new SectionTools(commandData);
                TStoreData tData = new TStoreData();

                Document doc = commandData.Application.ActiveUIDocument.Document;
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;

                SelectionTools selTools = new SelectionTools(commandData);

                selBeams = selTools.GetElemsOfCatFromSelection(BuiltInCategory.OST_StructuralFraming);
                while (selBeams.Count == 0) selBeams = selTools.UrgeSelection(BuiltInCategory.OST_StructuralFraming);
                
                Initialize.InitializeDocument(doc);

                // Collect section view tpyes
                FilteredElementCollector sectionFilter = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType));

                Sections = new List<ViewFamilyType>();
                foreach (Element e in sectionFilter)
                {
                    ViewFamilyType v = (ViewFamilyType)e;
                    var viewFam = v.ViewFamily;
                    if (viewFam == ViewFamily.Section) Sections.Add(v);
                }

                //Collect templates
                FilteredElementCollector views = new FilteredElementCollector(doc).OfClass(typeof(View));
                templates = new List<View>();
                
                foreach (View view in views)
                {
                    if (view.IsTemplate)
                        templates.Add(view);
                }

                // Verify selections
                foreach (Element elem in selBeams)
                {
                    UniqueIds.Add(elem.UniqueId);
                    hosts.Add(ElementTools.GetMark(elem));
                    cachedSections1Beam = secTool.CacheSections(elem, doc, SectionTools.SectionType.CrossSection);
                    cachedSectionsAllBeams.AddRange(cachedSections1Beam);
                }

                SectionForm secForm = new SectionForm(this, uiDoc);
                secForm.ShowDialog();

                if (!FieldClass.IsOK)
                {
                    return Result.Cancelled;
                }

                // Create sections
                using (Transaction t = new Transaction(doc, "Create Cross View Sections"))
                {
                    t.Start();
                    foreach (Element elem in selBeams)
                    {
                        secTool.CreateBoundingBox(elem, doc, passedFromFormsSections);
                        if (FieldClass.IsCleared || FieldClass.IsDeleted || FieldClass.IsUpdated)
                        {
                            foreach (Section sec in delSections)
                            {
                                if (sec.ViewSection != null)
                                    doc.Delete(sec.ViewSection.Id);
                            }
                            delSections.Clear();
                        }

                        if (passedFromFormsSections.Count > 0)
                        {
                            ListTools.CompareList(passedFromFormsSections, cachedSectionsAllBeams, tobeCreatedSections, "L");

                            foreach (Section sec in tobeCreatedSections)
                            {
                                ViewSection viewSection = ViewSection.CreateSection(doc, sec.ViewFamilyType.Id, sec.BoundingBox);
                                viewSection.LookupParameter("T_Mark").Set(elem.LookupParameter("Mark").AsString());
                                tData.AddInfoToElement<string>(viewSection, "CrossSection", "Direction", UnitType.UT_Undefined, DisplayUnitType.DUT_UNDEFINED);
                                if (sec.ViewSectionName != null) viewSection.LookupParameter("T_SectionName").Set(sec.ViewSectionName);
                                viewSection.ViewTemplateId = sec.Template.Id;
                                viewSection.CropBoxVisible = false;
                                sec.ViewSection = viewSection;
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
                TaskDialog.Show("View Sections", ex.Message);
                DataTools.WriteErrors(GlobalParams.ErrorPath , GlobalParams.Errors);
                return Result.Failed;
            }
        }
    }
}
