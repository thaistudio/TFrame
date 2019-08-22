using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Windows;
using System.Windows.Media;

using Control = System.Windows.Controls.Control;
using System.Windows.Controls;

namespace TFrame
{
    public class WPFDataTools
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
                XmlNode checkboxNode = AddNode(xmlDoc, cb.Name, cb.IsChecked.ToString(), "Text", cb.Content.ToString(), checkboxesNode);
            }

            // Save labels
            if (control is Label)
            {
                if (NodeExists(xmlDoc, groupboxNodeName, labelsNodeName, HasGroupBox)) labelsNode = AddNode(xmlDoc, labelsNodeName, null, null, null, groupboxNode);
                Label lb = (Label)control;
                XmlNode labelNode = AddNode(xmlDoc, lb.Name, lb.Content.ToString(), null, null, labelsNode);
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
                XmlNode buttonNode = AddNode(xmlDoc, bt.Name, bt.Content.ToString(), null, null, buttonsNode);
            }

            // Save radio buttons
            if (control is RadioButton)
            {
                if (NodeExists(xmlDoc, groupboxNodeName, radiobuttonsNodeName, HasGroupBox)) radiobuttonsNode = AddNode(xmlDoc, radiobuttonsNodeName, null, null, null, groupboxNode);

                RadioButton rbt = (RadioButton)control;
                XmlNode radiobuttonNode = AddNode(xmlDoc, rbt.Name, rbt.IsChecked.ToString(), null, null, radiobuttonsNode);
            }
        }


        

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

                if (checkBoxNode.InnerText == "True") cb.IsChecked = true;
                else cb.IsChecked = false;
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

                lb.Content = labelNode.InnerText;
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

                cbb.SelectedItem = (ComboBoxItem)cbb.FindName(comboBoxNode.InnerText);
            }

            // Save textboxes
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

            // Save buttons
            if (control is Button)
            {
                Button bt = (Button)control;
                string btName = bt.Name;

                string btPath;
                if (groupBoxNodeName == null) btPath = xmlPath + buttonsNodeName + "/" + btName;
                else btPath = xmlPath + groupBoxNodeName + "/" + buttonsNodeName + "/" + btName;
                XmlNode buttonNode = xmlDoc.SelectSingleNode(btPath);

                bt.Content = buttonNode.InnerText;
            }

            // Save radio buttons
            if (control is RadioButton)
            {
                RadioButton rb = (RadioButton)control;
                string rbName = rb.Name;

                string rbPath;
                if (groupBoxNodeName == null) rbPath = xmlPath + buttonsNodeName + "/" + rbName;
                else rbPath = xmlPath + groupBoxNodeName + "/" + buttonsNodeName + "/" + rbName;
                XmlNode radioButtonNode = xmlDoc.SelectSingleNode(rbPath);

                if (radioButtonNode.InnerText == "True") rb.IsChecked = true;
                else rb.IsChecked = false;
            }
        }

        public static void LoadWinFormUI(Window winForm, string path)
        {
            XmlDocument xmlDoc = new XmlDocument();
            if (!File.Exists(path)) return; // Make sure there is something to load

            xmlDoc.Load(path);

            foreach (Control control in GetControls(winForm))
            {
                // Actions if there are groupboxes
                if (control is GroupBox)
                {
                    GroupBox groupBox = (GroupBox)control;
                    foreach (Control gbControl in GetControlsInGroupBox(groupBox)) LoadAction(xmlDoc, gbControl, groupBox.Name);
                }

                // Non-groupboxes
                LoadAction(xmlDoc, control);
            }

        }

        /// <summary>
        /// Save Windows Form UI to a Xml File
        /// </summary>
        /// <param name="winForm">the form to save</param>
        /// <param name="path">saving path</param>
        public static void SaveWinFormUI(Window winForm, string path)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode winFormNode = AddNode(xmlDoc, "WindowsForm", null, "Name", winForm.Name);
            XmlAttribute winFormAttribute = xmlDoc.CreateAttribute("HostMemberId");
            winFormNode.Attributes.Append(winFormAttribute);

            foreach (Control control in GetControls(winForm))
            {
                // If there are groupboxes
                if (control is GroupBox)
                {
                    GroupBox gb = (GroupBox)control;
                    XmlNode groupboxNode = AddNode(xmlDoc, gb.Name, null, "Name", gb.Content.ToString(), winFormNode);

                    foreach (Control gbControl in GetControlsInGroupBox(gb))
                    {
                        int i = GetControlsInGroupBox(gb).Count();
                        SaveAction(xmlDoc, gbControl, true, groupboxNode);
                    }
                }

                // Non-groupboxes
                SaveAction(xmlDoc, control, false, winFormNode);
            }

            xmlDoc.Save(path);
        }

        static IEnumerable<Control> GetControls(Window window)
        {
            System.Windows.Controls.Panel panel = (System.Windows.Controls.Panel)window.Content;
            UIElementCollection elements = panel.Children;
            List<FrameworkElement> listElem = elements.Cast<FrameworkElement>().ToList();
            return listElem.OfType<Control>();
        }

        static IEnumerable<Control> GetControlsInGroupBox(GroupBox groupBox)
        {
            System.Windows.Controls.Panel panel = (System.Windows.Controls.Panel)groupBox.Content;
            UIElementCollection elements = panel.Children;
            List<FrameworkElement> listElem = elements.Cast<FrameworkElement>().ToList();
            return listElem.OfType<Control>();
        }


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
    }
}
