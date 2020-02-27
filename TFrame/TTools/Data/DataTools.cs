using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Forms;
using System.Windows;

using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.DB;
using Control = System.Windows.Forms.Control;
using Form = System.Windows.Forms.Form;
using WPFControl = System.Windows.Controls.Control;

namespace TFrame
{
    public class DataTools
    {
        static XmlNode checkboxesNode = null;
        static XmlNode labelsNode = null;
        static XmlNode comboboxesNode = null;
        static XmlNode textboxesNode = null;
        static XmlNode buttonsNode = null;
        static XmlNode radiobuttonsNode = null;
        static XmlNode numericsNode = null;

        static string checkboxesNodeName = "Checkboxes";
        static string labelsNodeName = "Labels";
        static string comboboxesNodeName = "Comboboxes";
        static string textboxesNodeName = "Textboxes";
        static string buttonsNodeName = "Buttons";
        static string numericssNodeName = "Numerics";
        static string radiobuttonsNodeName = "Radiobuttons";

        static string xmlPath = @"//WindowsForm/";

        public static void WriteErrors(string path, List<string> errors)
        {
            File.WriteAllLines(path, errors);
        }

        /// <summary>
        /// Check if a node path exists
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="groupboxNodeName"></param>
        /// <param name="nodeName"></param>
        /// <param name="HasGroupBox"></param>
        /// <returns></returns>
        static bool NodeExists(XmlDocument xmlDoc, string groupboxNodeName, string nodeName, bool HasGroupBox)
        {
            string firstChildName = ((XmlNode)xmlDoc).FirstChild.Name;
            if (HasGroupBox)
            {
                if (xmlDoc.SelectSingleNode("/" + firstChildName + "/" + groupboxNodeName + "/" + nodeName) == null) return true;
                else return false;
            }
            else
            {
                if (xmlDoc.SelectSingleNode("/" + firstChildName + "/" + nodeName) == null) return true;
                else return false;
            }   
        }

        #region Save Action
        /// <summary>
        /// This is where the real saving action happens. Enclose the code into this method to avoid repitition in SaveWinFormUI()
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="control"></param>
        /// <param name="HasGroupBox"></param>
        /// <param name="groupboxNode"></param>
        static void SaveAction(XmlDocument xmlDoc, Control control, bool HasGroupBox, XmlNode groupboxNode)
        {
            string groupboxNodeName = null;
            if (groupboxNode != null) groupboxNodeName = groupboxNode.Name;

            if (control is CheckBox)
            {
                if (NodeExists(xmlDoc, groupboxNodeName, checkboxesNodeName, HasGroupBox)) checkboxesNode = AddNode(xmlDoc, checkboxesNodeName, null, null, null, groupboxNode); // Create this node only once
                CheckBox cb = (CheckBox)control;
                XmlNode checkboxNode = AddNode(xmlDoc, cb.Name, cb.Checked.ToString(), "Text", cb.Text, checkboxesNode);
            }

            // Save labels
            if (control is Label)
            {
                if (NodeExists(xmlDoc, groupboxNodeName, labelsNodeName, HasGroupBox)) labelsNode = AddNode(xmlDoc, labelsNodeName, null, null, null, groupboxNode);
                Label lb = (Label)control;
                XmlNode labelNode = AddNode(xmlDoc, lb.Name, lb.Text, null, null, labelsNode);
            }

            // Save comboboxes
            if (control is ComboBox)
            {
                if (NodeExists(xmlDoc, groupboxNodeName, comboboxesNodeName, HasGroupBox)) comboboxesNode = AddNode(xmlDoc, comboboxesNodeName, null, null, null, groupboxNode);

                ComboBox cbb = (ComboBox)control;
                XmlNode comboboxNode = AddNode(xmlDoc, cbb.Name, cbb.Text, null, null, comboboxesNode);
            }

            // Save textboxes
            if (control is TextBox)
            {
                if (NodeExists(xmlDoc, groupboxNodeName, textboxesNodeName, HasGroupBox)) textboxesNode = AddNode(xmlDoc, textboxesNodeName, null, null, null, groupboxNode);

                TextBox tb = (TextBox)control;
                XmlNode textboxNode = AddNode(xmlDoc, tb.Name, tb.Text, null, null, textboxesNode);
            }

            // Save buttons
            if (control is Button)
            {
                if (NodeExists(xmlDoc, groupboxNodeName, buttonsNodeName, HasGroupBox)) buttonsNode = AddNode(xmlDoc, buttonsNodeName, null, null, null, groupboxNode);

                Button bt = (Button)control;
                XmlNode buttonNode = AddNode(xmlDoc, bt.Name, bt.Text, null, null, buttonsNode);
            }

            // Save radio buttons
            if (control is RadioButton)
            {
                if (NodeExists(xmlDoc, groupboxNodeName, radiobuttonsNodeName, HasGroupBox)) radiobuttonsNode = AddNode(xmlDoc, radiobuttonsNodeName, null, null, null, groupboxNode);

                RadioButton rbt = (RadioButton)control;
                XmlNode radiobuttonNode = AddNode(xmlDoc, rbt.Name, rbt.Checked.ToString(), null, null, radiobuttonsNode);
            }

            // Save numerics
            if (control is NumericUpDown)
            {
                if (NodeExists(xmlDoc, groupboxNodeName, numericssNodeName, HasGroupBox)) numericsNode = AddNode(xmlDoc, numericssNodeName, null, null, null, groupboxNode);

                NumericUpDown nm = (NumericUpDown)control;
                XmlNode numericNode = AddNode(xmlDoc, nm.Name, nm.Value.ToString(), null, null, numericsNode);
            }
        }
        #endregion

        #region Load Action
        /// <summary>
        /// Load data of a xml doc, with or without group boxes
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="control"></param>
        /// <param name="groupBoxNodeName"></param>
        static void LoadAction(XmlDocument xmlDoc, Control control, string groupBoxNodeName = null)
        {
            if (control is CheckBox)
            {
                CheckBox cb = (CheckBox)control;
                string cbName = cb.Name;

                string cbPath;
                if (groupBoxNodeName == null) cbPath = xmlPath + checkboxesNodeName + "/" + cbName;
                else cbPath = xmlPath + groupBoxNodeName + "/" + checkboxesNodeName + "/" + cbName;
                XmlNode checkBoxNode = xmlDoc.SelectSingleNode(cbPath);

                if (checkBoxNode.InnerText == "True") cb.Checked = true;
                else cb.Checked = false;
            }

            // Load labels
            if (control is Label)
            {
                Label lb = (Label)control;
                string lbName = lb.Name;

                string lbPath;
                if (groupBoxNodeName == null) lbPath = xmlPath + labelsNodeName + "/" + lbName;
                else lbPath = xmlPath + groupBoxNodeName + "/" + labelsNodeName + "/" + lbName;
                XmlNode labelNode = xmlDoc.SelectSingleNode(lbPath);

                lb.Text = labelNode.InnerText;
            }

            // Load comboboxes
            if (control is ComboBox)
            {
                ComboBox cbb = (ComboBox)control;
                string cbbName = cbb.Name;

                string cbbPath;
                if (groupBoxNodeName == null) cbbPath = xmlPath + comboboxesNodeName + "/" + cbbName;
                else cbbPath = xmlPath + groupBoxNodeName + "/" + comboboxesNodeName + "/" + cbbName;
                XmlNode comboBoxNode = xmlDoc.SelectSingleNode(cbbPath);

                int selIndex = cbb.FindStringExact(comboBoxNode.InnerText);
                cbb.SelectedIndex = selIndex;
            }

            // Load textboxes
            if (control is TextBox)
            {
                TextBox tb = (TextBox)control;
                string tbName = tb.Name;

                string tbPath;
                if (groupBoxNodeName == null) tbPath = xmlPath + textboxesNodeName + "/" + tbName;
                else tbPath = xmlPath + groupBoxNodeName + "/" + textboxesNodeName + "/" + tbName;
                XmlNode textBoxNode = xmlDoc.SelectSingleNode(tbPath);

                tb.Text = textBoxNode.InnerText;
            }

            // Load buttons
            if (control is Button)
            {
                Button bt = (Button)control;
                string btName = bt.Name;

                string btPath;
                if (groupBoxNodeName == null) btPath = xmlPath + buttonsNodeName + "/" + btName;
                else btPath = xmlPath + groupBoxNodeName + "/" + buttonsNodeName + "/" + btName;
                XmlNode buttonNode = xmlDoc.SelectSingleNode(btPath);

                bt.Text = buttonNode.InnerText;
            }

            // Load radio buttons
            if (control is RadioButton)
            {
                RadioButton rb = (RadioButton)control;
                string rbName = rb.Name;

                string rbPath;
                if (groupBoxNodeName == null) rbPath = xmlPath + buttonsNodeName + "/" + rbName;
                else rbPath = xmlPath + groupBoxNodeName + "/" + buttonsNodeName + "/" + rbName;
                XmlNode radioButtonNode = xmlDoc.SelectSingleNode(rbPath);

                if (radioButtonNode.InnerText == "True") rb.Checked = true;
                else rb.Checked = false;
            }

            // Load numerics
            if (control is NumericUpDown)
            {
                NumericUpDown nm = (NumericUpDown)control;
                string nmName = nm.Name;

                string nmPath;
                if (groupBoxNodeName == null) nmPath = xmlPath + buttonsNodeName + "/" + nmName;
                else nmPath = xmlPath + groupBoxNodeName + "/" + buttonsNodeName + "/" + nmName;
                XmlNode numericNode = xmlDoc.SelectSingleNode(nmPath);

                nm.Value = Convert.ToDecimal(numericNode.InnerText);
            }
        }
        #endregion

        #region Load data from windows form
        /// <summary>
        /// Load data from any winForm
        /// </summary>
        /// <param name="winForm"></param>
        /// <param name="path"></param>
        public static void LoadWinFormUI(Form winForm, string path)
        {
            XmlDocument xmlDoc = new XmlDocument();
            if (!File.Exists(path)) return; // Make sure there is something to load

            xmlDoc.Load(path);

            foreach (Control control in winForm.Controls)
            {
                // Actions if there are groupboxes
                if (control is GroupBox)
                {
                    GroupBox groupBox = (GroupBox)control;
                    foreach (Control gbControl in groupBox.Controls) LoadAction(xmlDoc, gbControl, groupBox.Name);
                }

                // Non-groupboxes
                LoadAction(xmlDoc, control);
            }
        }
        #endregion

        #region Save data from any windows form
        /// <summary>
        /// Save any Windows Form UI to a Xml File
        /// </summary>
        /// <param name="winForm">the form to save</param>
        /// <param name="path">saving path</param>
        public static void SaveWinFormUI(Form winForm, string path)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode winFormNode = AddNode(xmlDoc, "WindowsForm", null, "Name", winForm.Name);
            XmlAttribute winFormAttribute = xmlDoc.CreateAttribute("HostMemberId");
            winFormNode.Attributes.Append(winFormAttribute);

            foreach (Control control in winForm.Controls)
            {
                // If there are groupboxes
                if (control is GroupBox)
                {
                    GroupBox gb = (GroupBox)control;
                    Control.ControlCollection gbControls = gb.Controls;
                    XmlNode groupboxNode = AddNode(xmlDoc, gb.Name, null, "Name", gb.Text, winFormNode);

                    foreach (Control gbControl in gbControls)
                    {
                        SaveAction(xmlDoc, gbControl, true, groupboxNode);
                    }
                }

                // Non-groupboxes
                SaveAction(xmlDoc, control, false, winFormNode);
            }

            xmlDoc.Save(path);
        }
        #endregion

        #region XMLHelper

        static XmlNode AddNode(XmlDocument xmlDoc, string newNodeName, 
            string innerText = null,
            string attributeName = null, string attributeValue = null,
            XmlNode parentNode = null)
        {
            XmlNode newNode = xmlDoc.CreateElement(newNodeName);

            // Add innertext
            if (innerText != null)
            {
                newNode.InnerText = innerText;
            }

            // Add if attribute if there is any
            if (attributeName != null)
            {
                XmlAttribute newNodeAttribute = xmlDoc.CreateAttribute(attributeName);
                if (attributeValue != null) newNodeAttribute.Value = attributeValue;
                newNode.Attributes.Append(newNodeAttribute);
            }

            
            if (parentNode != null) // Append newNode to parentNode
            {
                parentNode.AppendChild(newNode);
            }
            else // Append newNode to xmlDoc
            {
                xmlDoc.AppendChild(newNode);
            }

            return newNode;
        }

        public static void AddAttribute(XmlDocument xmlDoc, string attributeName, string attributeVal, XmlNode node)
        {
            XmlAttribute xmlAttribute = xmlDoc.CreateAttribute(attributeName);
            xmlAttribute.Value = attributeVal;
            node.Attributes.Append(xmlAttribute);
        }
        #endregion

        #region Save/Load data to Revit element
        public static void SaveSimpleDataToElement<T>(Element element, T data, string fieldName, DisplayUnitType displayUnitType, 
            Schema schema = null, bool needUnit = false, UnitType unitType = UnitType.UT_Undefined, 
            string documentation = null)
        {

            // Delete existing schemas
            DeleteExsistingEntity(element, fieldName);

            if (schema == null) schema = CreateASchema<T>(fieldName, SchemaType.SimpleField, needUnit, unitType, documentation);

            Entity entity = new Entity(schema);
            entity.Set<T>(fieldName, data, displayUnitType);

            element.SetEntity(entity);
        }

        public static void SaveArrayDataToElement<T>(Element element, IList<T> array, string fieldName, DisplayUnitType displayUnitType,
            Schema schema = null, bool needUnit = false, UnitType unitType = UnitType.UT_Undefined,
            string documentation = null)
        {
            // Delete existing schemas
            DeleteExsistingEntity(element, fieldName);

            if (schema == null) schema = CreateASchema<T>(fieldName, SchemaType.ArrayField, needUnit, unitType, documentation, array);

            Entity entity = new Entity(schema);
            entity.Set<IList<T>>(fieldName, array, displayUnitType);

            element.SetEntity(entity);
        }

        static void DeleteExsistingEntity(Element element, string fieldName)
        {
            // Delete existing schemas
            IList<Guid> existingGuids = element.GetEntitySchemaGuids();
            if (existingGuids.Count > 0)
            {
                foreach (Guid existingGuid in existingGuids)
                {
                    Schema existingSchema = Schema.Lookup(existingGuid);
                    Entity existingEntity = element.GetEntity(existingSchema);
                    IList<Field> existingFields = existingSchema.ListFields();
                    if (existingSchema.SchemaName == fieldName) element.DeleteEntity(existingSchema);
                }
            }
        }

        public static Schema CreateASchema<T>(string fieldName, SchemaType schemaType,
            bool needUnit = false, UnitType unitType = UnitType.UT_Undefined, string documentation = null, IList<T> array = null, Type keyType = null)
        {
            // Verify if schema has already existed
            IList<Schema> schemas = Schema.ListSchemas();
            foreach (Schema schema in schemas)
            {
                try
                {
                    if (schema.GetField(fieldName) != null) return schema;
                }
                catch (Exception)
                {
                }
            }

            //
            Guid g = Guid.NewGuid();
            SchemaBuilder schemaBuilder = new SchemaBuilder(g);
            schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
            schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
            schemaBuilder.SetVendorId("THAISTUDIO");

            //Create a field
            FieldBuilder field = null;
            if (schemaType == SchemaType.SimpleField) field = schemaBuilder.AddSimpleField(fieldName, typeof(T));
            else if (schemaType == SchemaType.ArrayField) field = schemaBuilder.AddArrayField(fieldName, typeof(T));
            else if (schemaType == SchemaType.MapField) field = schemaBuilder.AddMapField(fieldName, keyType, typeof(T));

            if (documentation != null) field.SetDocumentation(documentation);

            schemaBuilder.SetSchemaName(fieldName);

            if (needUnit) field.SetUnitType(unitType);

            return schemaBuilder.Finish();
        }

        public static T LoadDataFromElement<T>(Element e, string fieldName, DisplayUnitType displayUnitType)
        {
            T v = default(T);
            var schemaGuids = e.GetEntitySchemaGuids();
            foreach (Guid schemaGuid in schemaGuids)
            {
                Schema schema = Schema.Lookup(schemaGuid);
                if (schema.SchemaName == fieldName)
                {
                    v = e.GetEntity(schema).Get<T>(fieldName, displayUnitType);
                    break;
                }
                else v = default(T);
            }
            return v;
        }
        #endregion
    }

    public enum SchemaType
    {
        SimpleField,
        ArrayField,
        MapField,
        None
    }
}
