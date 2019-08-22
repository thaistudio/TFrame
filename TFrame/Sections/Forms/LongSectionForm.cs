using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Form = System.Windows.Forms.Form;


namespace TFrame
{
    public partial class LongSectionForm : Form
    {
        UIDocument _uidoc;
        Document _doc;
        LongViewSections _lvs;

        int suff;
        List<string> viewSectionNames = new List<string>();
        List<Section> tobePassedSections = new List<Section>(); // This list will be passed to CreateLongVewSections
        List<Section> cachedSections = new List<Section>();

        double ftTomm;

        Section section;

        public LongSectionForm(LongViewSections lvs)
        {
            ftTomm = GlobalParams.FtToMm;
            _lvs = lvs;
            _uidoc = lvs.uiDoc;
            _doc = lvs.doc;

            InitializeComponent();
            Initialize();
        }

        void Initialize()
        {
            // Load view section
            comboBox1.DataSource = _lvs.sections;
            comboBox1.DisplayMember = "Name";
            comboBox1.SelectedIndex = 1;

            // Load data for View Templates
            comboBox2.DataSource = _lvs.templates;
            comboBox2.DisplayMember = "Name";
            comboBox2.SelectedIndex = 1;

            // Make textBox1 and textBox2 unable when "Manual Naming" is not checked
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            textBox1.Text = "S";
            textBox2.Text = "1";

            cachedSections = _lvs.cachedSectionsAllBeams;
            tobePassedSections.AddRange(cachedSections);

            int i = 0;
            foreach (Section cachedSection in tobePassedSections)
            {
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Cells[0].Value = cachedSection.Offset * ftTomm;
                dataGridView1.Rows[i].Cells[1].Value = cachedSection.Template.Name;
                dataGridView1.Rows[i].Cells[2].Value = cachedSection.ViewFamilyType.Name;
                dataGridView1.Rows[i].Cells[3].Value = cachedSection.ViewSectionName;
                dataGridView1.Rows[i].Cells[4].Value = cachedSection.Host;
                viewSectionNames.Add(cachedSection.ViewSectionName);
                i++;
            }
        }

        private void button2_Click(object sender, EventArgs e) // OK button
        {
            _lvs.passedFromFormsSections = tobePassedSections;
            _lvs.IsOK = true;
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _lvs.IsOK = false;
            Close();
        }

        private void button5_Click(object sender, EventArgs e) // Add button
        {
            FieldClass.IsAdded = true;
            section = new Section();

            int i = tobePassedSections.Count;

            string vsn = "";
            if (FieldClass.IsSuffEdited && !FieldClass.IsUpdated) suff = Convert.ToInt32(textBox2.Text);
            if (checkBox1.Checked)
            {
                vsn = textBox1.Text + " " + suff;
                section.ViewSectionName = textBox1.Text + " " + suff;
                suff++;
                if (viewSectionNames.Where(x => x == vsn).ToList().Count > 0)
                {
                    TaskDialog.Show("Warning", "Chosen name has existed");
                    return;
                }
            }

            viewSectionNames.Add(section.ViewSectionName);

            section.Template = (Autodesk.Revit.DB.View)comboBox2.SelectedItem;
            section.ViewFamilyType = (ViewFamilyType)comboBox1.SelectedItem;
            section.Offset = (double)numericUpDown1.Value / ftTomm;

            dataGridView1.Rows.Add();

            dataGridView1.Rows[i].Cells[0].Value = section.Offset * ftTomm;
            dataGridView1.Rows[i].Cells[1].Value = section.Template.Name;
            dataGridView1.Rows[i].Cells[2].Value = section.ViewFamilyType.Name;
            dataGridView1.Rows[i].Cells[3].Value = section.ViewSectionName;

            textBox2.Text = suff.ToString();
            i++;

            tobePassedSections.Add(section);
            _lvs.passedFromFormsSections.Add(section);
            FieldClass.IsSuffEdited = false;
            FieldClass.IsUpdated = false;
        }

        private void button4_Click(object sender, EventArgs e) //Delete button
        {
            FieldClass.IsDeleted = true;
            if(dataGridView1.CurrentCell != null)
            {
                int row = dataGridView1.CurrentCell.RowIndex;
                _lvs.delSections.Add(tobePassedSections[row]);

                tobePassedSections.RemoveAt(row);
                viewSectionNames.RemoveAt(row);

                dataGridView1.Rows.RemoveAt(row);
            }
            else return;
        }

        private void button6_Click(object sender, EventArgs e) //Clear button
        {
            FieldClass.IsCleared = true;

            for (int i = tobePassedSections.Count - 1; i > -1; i--)
            {
                dataGridView1.Rows.RemoveAt(i);
            }

            foreach (Section sec in tobePassedSections)
            {
                _lvs.delSections.Add(sec);
            }
            tobePassedSections.Clear();
        }

        private void button1_Click(object sender, EventArgs e) //Update button
        {
            FieldClass.IsUpdated = true;
            int row = dataGridView1.CurrentCell.RowIndex;

            section = new Section();

            section.Template = (Autodesk.Revit.DB.View)comboBox2.SelectedItem;
            section.ViewFamilyType = (ViewFamilyType)comboBox1.SelectedItem;
            section.Offset = (double)numericUpDown1.Value / ftTomm;

            string vsn;
            if (FieldClass.IsSuffEdited) suff = Convert.ToInt32(textBox2.Text);
            if (checkBox1.Checked)
            {
                vsn = textBox1.Text + " " + suff;
                if (viewSectionNames.Where(x => x == vsn).ToList().Count > 0)
                {
                    TaskDialog.Show("Warning", "Chosen name has existed");
                    return;
                }
                viewSectionNames.RemoveAt(row);
                section.ViewSectionName = textBox1.Text + " " + textBox2.Text;
                viewSectionNames.Insert(row, section.ViewSectionName);
            }

            _lvs.delSections.Add(tobePassedSections[row]);
            tobePassedSections.Insert(row, section);

            dataGridView1.Rows[row].Cells[0].Value = section.Offset * ftTomm;
            dataGridView1.Rows[row].Cells[1].Value = section.Template.Name;
            dataGridView1.Rows[row].Cells[2].Value = section.ViewFamilyType.Name;
            dataGridView1.Rows[row].Cells[3].Value = section.ViewSectionName;
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

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                FieldClass.IsBBAuto = true;
                if (tobePassedSections.Count == 0) TaskDialog.Show("Warning", "Please add a section first");
                else if (dataGridView1.SelectedRows[0] == null) TaskDialog.Show("Warning", "Please select a section first");
                else
                {
                    int row = dataGridView1.CurrentCell.RowIndex;
                    BoundingBoxSizingFormAuto bbForm = new BoundingBoxSizingFormAuto(tobePassedSections[row], _lvs);
                    bbForm.ShowDialog();
                }
            }
            catch (Exception)
            {
                TaskDialog.Show("Warning", "Please select a whole row");
            }
        }
    }
}
