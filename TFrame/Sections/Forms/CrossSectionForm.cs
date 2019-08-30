using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TFrame
{
    
    public partial class SectionForm : System.Windows.Forms.Form
    {
        CrossViewSections _cvs;
        Document _doc;
        UIDocument _uidoc;
        public Autodesk.Revit.DB.View selView;

        public int suff;

        List<string> ViewSectionNames;

        public Section _section { get; set; }

        List<Section> tobePassedSections; // this list will be passed to CreateCrossViewSections
        List<Section> CachedSections2;

        double ftToMm;

        public SectionForm(CrossViewSections cvs, UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = uidoc.Document;
            InitializeComponent();
            Initialize(cvs);
        }

        private void Initialize(CrossViewSections cvs)
        {
            
            ftToMm = GlobalParams.FtToMm;

            _cvs = cvs;
            ViewSectionNames = new List<string>();
            CachedSections2 = _cvs.cachedSectionsAllBeams;


            FieldClass.IsCleared = false;
            FieldClass.IsDeleted = false;

            textBoxRelativeDistance.BackColor = System.Drawing.SystemColors.Menu;
            textBoxAbsDistance.BackColor = System.Drawing.SystemColors.Menu;

            // Make textBox1 and textBox2 unable when "Manual Naming" is not checked
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            textBox1.Text = "S";
            textBox2.Text = "1";

            suff = textBox2.Text == null ? _section.Suffix : Convert.ToInt32(textBox2.Text);
            
            _cvs.delSections = new List<Section>();
            _cvs.delViews = new List<Autodesk.Revit.DB.View>();
            _cvs.passedFromFormsSections = new List<Section>();
            _cvs.tobeCreatedSections = new List<Section>();
            int ViewTempIndex1 = FieldClass.ViewTempIndex;
            int ViewSectionIndex2 = FieldClass.ViewSectionIndex;
            int Location2 = FieldClass.FieldLocation;
            bool Loaded = FieldClass.IsLoaded;

            if (FieldClass.Sections == null) tobePassedSections = new List<Section>();
            else tobePassedSections = FieldClass.Sections;

            foreach (Section s in CachedSections2)
            {
                tobePassedSections.Add(s);
            }

            // Transfer data from Section2 to _cvs.sections
            _cvs.passedFromFormsSections = tobePassedSections;

            // Load data for Sections gridDataView
            for (int i = 0; i < CachedSections2.Count; i++)
            {
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Cells[0].Value = Math.Round(CachedSections2[i].L, 3);
                dataGridView1.Rows[i].Cells[1].Value = Math.Round(CachedSections2[i].d, 3);
                dataGridView1.Rows[i].Cells[2].Value = CachedSections2[i].Template != null ? CachedSections2[i].Template.Name : "";
                dataGridView1.Rows[i].Cells[3].Value = CachedSections2[i].ViewFamilyType.Name;
                dataGridView1.Rows[i].Cells[4].Value = CachedSections2[i].ViewSectionName;
                dataGridView1.Rows[i].Cells[5].Value = CachedSections2[i].Host;
                ViewSectionNames.Add(CachedSections2[i].ViewSectionName);
            }
            dataGridView1.Rows[0].Selected = true;

            // Load data for Relative Distance, Abs Distance, Prefix, Suffix
            if (tobePassedSections.Count > 0)
            {
                comboBox2.SelectedIndex = comboBox2.FindStringExact(dataGridView1.Rows[0].Cells[3].Value.ToString());
                comboBox1.SelectedIndex = comboBox1.FindStringExact(dataGridView1.Rows[0].Cells[2].Value.ToString());

                if (tobePassedSections[0].IsManualNamed == true)
                {
                    textBox2.Text = tobePassedSections[0].Suffix.ToString();
                    textBox1.Text = tobePassedSections[0].Prefix;
                    checkBox1.Checked = true;
                }

                textBoxRelativeDistance.Text = Math.Round(tobePassedSections[0].L, 3).ToString();
                textBoxAbsDistance.Text = Math.Round(tobePassedSections[0].d, 3).ToString();
            }

            
            // Load view section
            comboBox2.DataSource = _cvs.Sections;
            comboBox2.DisplayMember = "Name";
            comboBox2.SelectedIndex = ViewSectionIndex2;
            
            // Load data for View Templates
            comboBox1.DataSource = _cvs.templates;
            comboBox1.DisplayMember = "Name";
            comboBox1.SelectedIndex = ViewTempIndex1;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            FieldClass.ViewTempIndex = comboBox1.SelectedIndex;
        }


        private void button2_Click(object sender, EventArgs e) // Add button, click to add section location
        {
            FieldClass.IsAdded = true;
            int i = tobePassedSections.Count; // Section number, start with 1

            _section = new Section();

            if (radioButton3.Checked)
            {
                _section.L = Convert.ToDouble(textBoxRelativeDistance.Text);
                _section.IsRelative = true;
            }

            if (radioButton4.Checked)
            {
                _section.d = Convert.ToDouble(textBoxAbsDistance.Text) / ftToMm;
            }

            string vsn = "";
            if (FieldClass.IsSuffEdited && !FieldClass.IsUpdated) suff = Convert.ToInt32(textBox2.Text);
            if (checkBox1.Checked)
            {
                vsn = textBox1.Text + " " + suff;
                _section.ViewSectionName = textBox1.Text + " " + suff;
                suff++;
                if (ViewSectionNames.Where(x => x == vsn).ToList().Count > 0)
                {
                    TaskDialog.Show("Warning", "Chosen name has existed");
                    return;
                }
            }

            ViewSectionNames.Add(_section.ViewSectionName);

            _section.Template = (Autodesk.Revit.DB.View)comboBox1.SelectedItem;
            _section.ViewFamilyType = (ViewFamilyType)comboBox2.SelectedItem;

            dataGridView1.Rows.Add();
            dataGridView1.Rows[i].Cells[0].Value = radioButton3.Checked ? _section.Location : "";

            if (radioButton4.Checked) dataGridView1.Rows[i].Cells[1].Value = _section.d * ftToMm;
            else dataGridView1.Rows[i].Cells[1].Value = null;

            dataGridView1.Rows[i].Cells[2].Value = _section.Template.Name;
            dataGridView1.Rows[i].Cells[3].Value = _section.ViewFamilyType.Name;
            dataGridView1.Rows[i].Cells[4].Value = _section.ViewSectionName;

            textBox2.Text = suff.ToString();
            i++;

            tobePassedSections.Add(_section);
            FieldClass.IsSuffEdited = false;
            FieldClass.IsUpdated = false;
        }


        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            button7.Enabled = true;
            FieldClass.IsRadioClicked = true;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            FieldClass.IsRadioClicked = true;
            button7.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e) // Update button
        {
            FieldClass.IsUpdated = true;
            int row = dataGridView1.CurrentCell.RowIndex;
           
            _section = new Section();

            if (radioButton3.Checked)
            {
                _section.L = Convert.ToDouble(textBoxRelativeDistance.Text); 
                _section.IsRelative = true;
            }
            else if (radioButton4.Checked)
            {
                _section.d = Convert.ToDouble(textBoxAbsDistance.Text) / ftToMm;
            }
            else
            {
                _section.L = Convert.ToDouble(textBoxRelativeDistance.Text);
                _section.d = Convert.ToDouble(textBoxAbsDistance.Text) / ftToMm;
                _section.IsRelative = true;
            }

            _section.Template = (Autodesk.Revit.DB.View)comboBox1.SelectedItem;
            _section.ViewFamilyType = (ViewFamilyType)comboBox2.SelectedItem;

            string vsn;
            if (FieldClass.IsSuffEdited) suff = Convert.ToInt32(textBox2.Text);
            if (checkBox1.Checked)
            {
                vsn = textBox1.Text + " " + suff;
                if (ViewSectionNames.Where(x => x == vsn).ToList().Count > 0)
                {
                    TaskDialog.Show("Warning", "Chosen name has existed");
                    return;
                }
                ViewSectionNames.RemoveAt(row);
                _section.ViewSectionName = textBox1.Text + " " + textBox2.Text;
                ViewSectionNames.Insert(row, _section.ViewSectionName);
            }

            _cvs.delSections.Add(tobePassedSections[row]);
            tobePassedSections.Insert(row, _section);

            if (radioButton3.Checked) dataGridView1.Rows[row].Cells[0].Value = _section.Location;
            else if (!radioButton3.Checked && radioButton4.Checked) dataGridView1.Rows[row].Cells[0].Value = null;
            else if (radioButton4.Checked) dataGridView1.Rows[row].Cells[1].Value = _section.d * ftToMm;
            else if (!radioButton4.Checked && radioButton3.Checked) dataGridView1.Rows[row].Cells[1].Value = null;
            else
            {
                dataGridView1.Rows[row].Cells[0].Value = _section.Location;
                dataGridView1.Rows[row].Cells[1].Value = _section.d;
            }

            dataGridView1.Rows[row].Cells[2].Value = _section.Template.Name;
            dataGridView1.Rows[row].Cells[3].Value = _section.ViewFamilyType.Name;
            dataGridView1.Rows[row].Cells[4].Value = _section.ViewSectionName;
        }

        private void button4_Click(object sender, EventArgs e) // Delete button
        {
            FieldClass.IsDeleted = true;

            if (dataGridView1.CurrentCell != null)
            {
                int row = dataGridView1.CurrentCell.RowIndex;
                _cvs.delSections.Add(tobePassedSections[row]);

                tobePassedSections.RemoveAt(row);
                ViewSectionNames.RemoveAt(row);

                dataGridView1.Rows.RemoveAt(row);
            }
            else return;
        }

        private void button1_Click(object sender, EventArgs e) //OK button
        {
            if (!FieldClass.IsAdded && !FieldClass.IsDeleted && !FieldClass.IsCleared && !FieldClass.IsBBAuto && !FieldClass.IsBBManual && !FieldClass.IsUpdated)
            {
                TaskDialog.Show("Warning", "Please Add, Delete or Clear sections. Or hit Cancel to exit!");
            }
            else 
            {
                FieldClass.IsOK = true;
                FieldClass.IsLoaded = true;
                TSave();
                Close();
            }
        }

        private void button5_Click(object sender, EventArgs e) // Cancel button
        {
            //this.DialogResult = DialogResult.Cancel;
            FieldClass.IsOK = false;
            Close();
        }

        private void SectionForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = false;
        }

        private void TSave()
        {
            //foreach (string uniqueId in _cvs.UniqueIds)
            //DataTools.SaveWinFormUI(this, @"D:\Thai\Code\Revit\TFrame\Data\UI.xml", uniqueId);
        }

        private void SaveOptionsToFile()
        {

        }

        private void button6_Click(object sender, EventArgs e) // Clear button
        {
            FieldClass.IsCleared = true;

            for (int i = tobePassedSections.Count - 1; i > -1; i--)
            {
                dataGridView1.Rows.RemoveAt(i);
            }
            
            foreach (Section sec in _cvs.passedFromFormsSections)
            {
                _cvs.delSections.Add(sec);
            }
            tobePassedSections.Clear();
        }

        private void button7_Click(object sender, EventArgs e) // BBAuto button
        {
            try
            {
                FieldClass.IsBBAuto = true;
                if (tobePassedSections.Count == 0) TaskDialog.Show("Warning", "Please add a section first");
                else if (dataGridView1.SelectedRows[0] == null) TaskDialog.Show("Warning", "Please select a section first");
                else
                {
                    int row = dataGridView1.CurrentCell.RowIndex;
                    BoundingBoxSizingFormAuto bbForm = new BoundingBoxSizingFormAuto(tobePassedSections[row], _cvs);
                    bbForm.ShowDialog();
                }
            }
            catch (Exception)
            {
                TaskDialog.Show("Warning", "Please select a whole row");
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            FieldClass.IsRelativeDistanceClicked = true;

            textBoxAbsDistance.ReadOnly = true;
            textBoxAbsDistance.BackColor = System.Drawing.Color.Empty;

            textBoxRelativeDistance.ReadOnly = false;
            textBoxRelativeDistance.BackColor = System.Drawing.Color.Empty;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            FieldClass.IsRelativeDistanceClicked = false;

            textBoxRelativeDistance.ReadOnly = true;
            textBoxRelativeDistance.BackColor = System.Drawing.SystemColors.Menu;

            textBoxAbsDistance.ReadOnly = false;
            textBoxAbsDistance.BackColor = System.Drawing.Color.Empty;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            FieldClass.IsSuffEdited = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox1.Enabled = true;
                textBox2.Enabled = true;
            }
            else 
            {
                textBox1.Enabled = false;
                textBox2.Enabled = false;
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (!FieldClass.IsRadioClicked && FieldClass.IsAdded)
            {
                TaskDialog.Show("Warning", "Please select bounding box size!");
            }
            else if (!FieldClass.IsAdded && !FieldClass.IsDeleted && !FieldClass.IsCleared && !FieldClass.IsBBAuto && !FieldClass.IsBBManual)
            {
                TaskDialog.Show("Warning", "Please Add, Delete or Clear sections. Or hit Cancel to exit!");
            }
            else
            {
                FieldClass.IsOK = true;
                FieldClass.IsLoaded = true;
                TSave();
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                int row = dataGridView1.CurrentCell.RowIndex;

                comboBox2.SelectedIndex = comboBox2.FindStringExact(dataGridView1.Rows[row].Cells[3].Value.ToString());
                comboBox1.SelectedIndex = comboBox1.FindStringExact(dataGridView1.Rows[row].Cells[2].Value.ToString());

                if (tobePassedSections[row].IsManualNamed == true)
                {
                    textBox2.Text = tobePassedSections[row].Suffix.ToString();
                    textBox1.Text = tobePassedSections[row].Prefix;
                    checkBox1.Checked = true;
                }

                textBoxRelativeDistance.Text = tobePassedSections[row].L.ToString();
                textBoxAbsDistance.Text = tobePassedSections[row].d.ToString();
            }
            catch 
            {
                return;
            }
        }

        private void NumberTextBox(object sender)
        {
            System.Windows.Forms.TextBox tb = (System.Windows.Forms.TextBox)sender;
            try
            {
                double d = Convert.ToDouble(tb.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("Please enter a valid number!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tb.Text = "1";
            }
        }

        private void TextBoxRelativeDistance_Leave(object sender, EventArgs e)
        {
            NumberTextBox(sender);
        }

        private void TextBoxAbsDistance_Leave(object sender, EventArgs e)
        {
            NumberTextBox(sender);
        }
    }
}
