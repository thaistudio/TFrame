using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using ComboBox = System.Windows.Forms.ComboBox;

namespace TFrame.Stamp_Beam_Marks.Forms
{
    public partial class BeamMarkFormAdvanced : System.Windows.Forms.Form
    {
        private string savePath = GlobalParams.PathSettingBeamMark;

        BeamMarks _bm;
        Settings horSetting;
        Settings verSetting;
        Settings horSampleSetting;
        Settings verSampleSetting;

        // 5 PartSetting corresponds to 5 name parts of horizontal/vertical beams. Each PartSetting corresponds to 1 comboBox and 1 textBox
        PartSetting hP1, hP2, hP3, hP4, hP5; 
        PartSetting vP1, vP2, vP3, vP4, vP5;

        // PartSetting, comboBox and textBox will be added to lists, so that I can repeat the setting selection using SelectPartSetting method.
        // If I don't use this method, I have to repeat the code at least 10 times. Plus, any future addition part will need a lot of code modifications.
        List<PartSetting> hPS = new List<PartSetting>();
        List<PartSetting> vPS = new List<PartSetting>();
        List<PartSetting> allPS; // Concatenate hPS and vPS for looping sake
        List<System.Windows.Forms.ComboBox> comboBoxes = new List<System.Windows.Forms.ComboBox>();
        List<System.Windows.Forms.TextBox> textBoxes = new List<System.Windows.Forms.TextBox>();

        public BeamMarkFormAdvanced(BeamMarks bm)
        {
            _bm = bm;
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            CreateNamingList();
            InitiateLists();
            CreateRuleList();
            SetDefaultValues();
            LoadSetting();
        }

        private void InitiateLists()
        {
            horSetting = new Settings(_bm); horSetting.SettingName = "horSetting";
            verSetting = new Settings(_bm); verSetting.SettingName = "verSetting";
            horSetting.BeamOrientation = BeamOrientation.Horizontal;
            verSetting.BeamOrientation = BeamOrientation.Vertical;
            horSampleSetting = new Settings(_bm);
            verSampleSetting = new Settings(_bm);

            _bm.settings = new List<Settings>();
            _bm.settings.Add(horSetting);
            _bm.settings.Add(verSetting);
        }

        private void SetDefaultValues()
        {
            foreach (ComboBox cb in comboBoxes)
            {
                cb.SelectedIndex = 2;
            }
            foreach (System.Windows.Forms.TextBox tb in textBoxes)
            {
                tb.Text = "B";
            }
        }

        private void CreateNamingList()
        {
            hP1 = new PartSetting(); hP2 = new PartSetting(); hP3 = new PartSetting(); hP4 = new PartSetting(); hP5 = new PartSetting();
            vP1 = new PartSetting(); vP2 = new PartSetting(); vP3 = new PartSetting(); vP4 = new PartSetting(); vP5 = new PartSetting();
            comboBoxes.Add(comboBox1); comboBoxes.Add(comboBox2); comboBoxes.Add(comboBox3); comboBoxes.Add(comboBox4); comboBoxes.Add(comboBox5);
            comboBoxes.Add(comboBox12); comboBoxes.Add(comboBox11); comboBoxes.Add(comboBox10); comboBoxes.Add(comboBox9); comboBoxes.Add(comboBox8);
            textBoxes.Add(textBox1); textBoxes.Add(textBox2); textBoxes.Add(textBox3); textBoxes.Add(textBox4); textBoxes.Add(textBox5);
            textBoxes.Add(textBox10); textBoxes.Add(textBox9); textBoxes.Add(textBox8); textBoxes.Add(textBox7); textBoxes.Add(textBox6);

            List<NamingParts> parts = new List<NamingParts>();
            parts.Add(NamingParts.Text);
            parts.Add(NamingParts.Floor);
            parts.Add(NamingParts.GridName);
            parts.Add(NamingParts.Number);

            foreach (System.Windows.Forms.ComboBox cb in comboBoxes)
            {
                foreach (NamingParts np in parts) cb.Items.Add(np);
            }

            hPS.Add(hP1); hPS.Add(hP2); hPS.Add(hP3); hPS.Add(hP4); hPS.Add(hP5);
            vPS.Add(vP1); vPS.Add(vP2); vPS.Add(vP3); vPS.Add(vP4); vPS.Add(vP5);

            foreach (PartSetting ps in hPS) ps.IsHorizontal = true;
            foreach (PartSetting ps in vPS) ps.IsHorizontal = false;

            allPS = hPS.Concat(vPS).ToList();
        }

        private void CreateRuleList()
        {
            comboBox6.Items.Add(SortingRule.LeftToRight);
            comboBox6.Items.Add(SortingRule.RightToLeft);
            comboBox7.Items.Add(SortingRule.UpDown);
            comboBox7.Items.Add(SortingRule.DownUp);
            comboBox6.SelectedIndex = 1;
            comboBox7.SelectedIndex = 1;
        }
       
        private void button1_Click(object sender, EventArgs e) // OK button
        {
            horSetting.PartSettings = hPS;
            verSetting.PartSettings = vPS;

            GlobalParams.IsOK = true;
            OKAction();
            Close();
        }

        private void button2_Click(object sender, EventArgs e) // Cancel button
        {
            GlobalParams.IsOK = false;
            Close();
        }

        private void SelectPartSetting(System.Windows.Forms.ComboBox cb, System.Windows.Forms.TextBox tb, PartSetting ps)
        {
            if (cb.SelectedIndex == 0) // Text
            {
                ps.Part = tb.Text;
                ps.IsText = true;
            }
            if (cb.SelectedIndex == 1) // Floor
            {
                ps.Part = tb.Text;
                ps.IsFloor = true;
            }
            if (cb.SelectedIndex == 2) // Grid Name
            {
                ps.Part = tb.Text;
                ps.IsGridName = true;
            }
            if (cb.SelectedIndex == 3) // Number
            {
                ps.Part = tb.Text;
                ps.IsNumber = true;
            }
            ps.Index = cb.SelectedIndex;
        }

        private void OKAction()
        {
            // In stead of repeating the code inside SelectPartSetting 10 times, I have 1 line of code here.  
            // Any modification to the selecting, inthe future, just needs to take place in SelectPartSetting()
            for (int i = 0; i < comboBoxes.Count; i++) SelectPartSetting(comboBoxes[i], textBoxes[i], allPS[i]);

            horSetting.Rule1 = SortingRule.DownUp;
            horSetting.Rule2 = (SortingRule)comboBox6.SelectedItem;
            verSetting.Rule1 = SortingRule.LeftToRight;
            verSetting.Rule2 = (SortingRule)comboBox7.SelectedItem;

            SaveSettings();
        }

        /// <summary>
        /// Save settings to an xml file stored in savePath
        /// </summary>
        private void SaveSettings()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode beamMark = xmlDoc.CreateElement("beamMark");
            xmlDoc.AppendChild(beamMark);
            XmlNode verSettingNode = xmlDoc.CreateElement("setting");
            foreach (Settings setting in _bm.settings)
            {
                XmlNode settingNode = xmlDoc.CreateElement("setting");
                XmlAttribute settingAtt = xmlDoc.CreateAttribute("name");
                settingAtt.Value = setting.SettingName;
                settingNode.Attributes.Append(settingAtt);

                XmlNode directionNode = xmlDoc.CreateElement("order");
                directionNode.InnerText = setting.GetType().GetProperty("Rule2").GetValue(setting).ToString();
                settingNode.AppendChild(directionNode);

                int i = 0;
                foreach (PartSetting ps in setting.PartSettings)
                {
                    XmlNode partNode = xmlDoc.CreateElement("part");
                    XmlAttribute partAtt = xmlDoc.CreateAttribute("no");
                    partAtt.Value = i.ToString();
                    partNode.Attributes.Append(partAtt);
                    i++;

                    foreach (PropertyInfo prop in ps.GetType().GetProperties())
                    {
                        XmlNode propNode = xmlDoc.CreateElement(prop.Name);
                        propNode.InnerText = prop.GetValue(ps).ToString();
                        partNode.AppendChild(propNode);
                    }
                    settingNode.AppendChild(partNode);
                }
                beamMark.AppendChild(settingNode);
            }
            xmlDoc.Save(savePath);
        }

        /// <summary>
        /// Load settings from the saved xml file a.k.a. savedPath
        /// </summary>
        private void LoadSetting()
        {
            XmlDocument xmlDoc = new XmlDocument();
            if (!System.IO.Directory.Exists(savePath)) return;

            xmlDoc.Load(savePath);
            XmlNodeList indexList = xmlDoc.SelectNodes("//beamMark/setting/part/Index");
            XmlNodeList textList = xmlDoc.SelectNodes("//beamMark/setting/part/Part");
            XmlNodeList orderList = xmlDoc.SelectNodes("//beamMark/setting/order");

            for (int i = 0; i < comboBoxes.Count; i++)
            {
                comboBoxes[i].SelectedIndex = Convert.ToInt32(indexList.Item(i).InnerText);
                textBoxes[i].Text = textList.Item(i).InnerText; 
            }

            comboBox6.SelectedIndex = comboBox6.FindStringExact(orderList.Item(0).InnerText);
            comboBox7.SelectedIndex = comboBox7.FindStringExact(orderList.Item(1).InnerText);
        }

        // Event zone
        /// <summary>
        /// Set values to part setting
        /// </summary>
        /// <param name="i"></param>
        private void ComboBoxEvent(int i)
        {
            if (comboBoxes[i].SelectedIndex == 0) // Text
            {
                textBoxes[i].Enabled = true;
                textBoxes[i].Text = "B";
            }
            if (comboBoxes[i].SelectedIndex == 1) // Floor
            {
                textBoxes[i].Enabled = false;
                textBoxes[i].Text = verSampleSetting.LevelName;
            }
            if (comboBoxes[i].SelectedIndex == 2) // GridName
            {
                textBoxes[i].Enabled = false;
                if (allPS[i].IsHorizontal) textBoxes[i].Text = horSampleSetting.HorGridName;
                else textBoxes[i].Text = verSampleSetting.VerGridName;
            }
            if (comboBoxes[i].SelectedIndex == 3) // Number
            {
                textBoxes[i].Enabled = true;
                textBoxes[i].Text = "1";
            }
        }

        private void TextBoxEvent() 
        {
            OnlyNumber();
            SampleName();
        }

        /// <summary>
        /// Only allow number input when Numer is selected in comboBox
        /// </summary>
        private void OnlyNumber()
        {
            for (int i = 0; i < textBoxes.Count; i++)
            {
                if (comboBoxes[i].SelectedIndex == 3)
                {
                    try
                    {
                        Convert.ToInt32(textBoxes[i].Text);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Please enter a number", "Wrong format!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        textBoxes[i].Text = "1";
                    }
                }
            }
        }
       
        /// <summary>
        /// Create a live beam mark sample
        /// </summary>
        private void SampleName()
        {
            label7.Text = string.Empty;
            label10.Text = string.Empty;
            for (int i = 0; i < hPS.Count; i++)
            {
                label7.Text += textBoxes[i].Text;
            }
            for (int i = hPS.Count; i < textBoxes.Count; i++)
            {
                label10.Text += textBoxes[i].Text;
            }
        }

        #region Combobox events
        // Combobox events
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxEvent(0);
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxEvent(1);
        }


        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxEvent(2);
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxEvent(3);
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxEvent(4);
        }
        private void comboBox12_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxEvent(5);
        }

        private void comboBox11_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxEvent(6);
        }

        private void comboBox10_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxEvent(7);
        }

        private void comboBox9_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxEvent(8);
        }

        private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxEvent(9);
        }
        #endregion

        #region Textbox events
        // Textbox events
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            TextBoxEvent();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            TextBoxEvent();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            TextBoxEvent();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            TextBoxEvent();
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            TextBoxEvent();
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            TextBoxEvent();
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            TextBoxEvent();
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            TextBoxEvent();
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            TextBoxEvent();
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            TextBoxEvent();
        }
        #endregion
    }
}
