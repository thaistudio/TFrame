using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;

namespace TFrame.TTools
{
    class Initialize
    {
        Document _doc;
        UIDocument _uidoc;

        public Initialize(ExternalCommandData commandData)
        {
            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = _uidoc.Document;
        }

        /// <summary>
        /// Use this method to add shared parameter from path to elements
        /// </summary>
        /// <param name="doc"></param>
        public static void InitializeDocument(Document doc)
        {
            string path = @"D:\Thai\Code\Revit\TFrame\Shared Parameters\TSharedParams.txt";
            
            // Open shared parameters file from path and access its data
            Application app = doc.Application;
            var v = app.SharedParametersFilename;
            app.SharedParametersFilename = path;
            DefinitionFile definitionFile = app.OpenSharedParameterFile();
            DefinitionGroups definitionGroups = definitionFile.Groups;

            // Get individual param group
            DefinitionGroup beamGroup = definitionGroups.get_Item("BeamParams");
            DefinitionGroup sectionGroup = definitionGroups.get_Item("SectionParams");

            // Get Definitions of each DefinitionGroup
            Definitions beamDefs = beamGroup.Definitions;
            Definitions secDefs = sectionGroup.Definitions;

            // Get category and insert to a category set
            CategorySet beamSet = app.Create.NewCategorySet();
            Category beams = Category.GetCategory(doc, BuiltInCategory.OST_StructuralFraming);
            beamSet.Insert(beams);

            CategorySet sectionSet = app.Create.NewCategorySet();
            Category viewSections = Category.GetCategory(doc, BuiltInCategory.OST_Views);
            sectionSet.Insert(viewSections);

            // Create an object of TypeBinding according to Categories
            TypeBinding typeBinding = app.Create.NewTypeBinding(beamSet);
            InstanceBinding beamInstanceBinding = app.Create.NewInstanceBinding(beamSet);
            InstanceBinding secInstanceBinding = app.Create.NewInstanceBinding(sectionSet);

            // Get the BindingMap of current document
            BindingMap bindingMap = doc.ParameterBindings;


            //Bind the definitions to the document

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Initialize Params");

                foreach (Definition def in beamDefs)
                {
                    bindingMap.Insert(def, beamInstanceBinding, BuiltInParameterGroup.INVALID);
                }

                foreach (Definition def in secDefs)
                {
                    
                    bindingMap.Insert(def, secInstanceBinding, BuiltInParameterGroup.INVALID);
                }

                t.Commit();
            }
                
        }
    }
}
