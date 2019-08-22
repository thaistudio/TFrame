using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TFrame
{
    public partial class BeamMarkForm : Form
    {
        Settings horSetting;
        Settings verSetting;
        Settings horSampleSetting;
        Settings verSampleSetting;

        List<Settings> settings = new List<Settings>();
        BeamMarks _bmark;
        
        public BeamMarkForm()
        {
            InitializeComponent();
        }

        public BeamMarkForm(BeamMarks beamMarks)
        {
            _bmark = beamMarks;
            InitializeComponent();

            horSetting = new Settings(beamMarks);
            verSetting = new Settings(beamMarks);
            horSampleSetting = new Settings(beamMarks);
            verSampleSetting = new Settings(beamMarks);

            List<SortingRule> upDown = new List<SortingRule>();
            List<SortingRule> upDown1 = new List<SortingRule>();
            upDown.Add(SortingRule.DownUp);
            upDown.Add(SortingRule.UpDown);
            upDown1.Add(SortingRule.DownUp);
            upDown1.Add(SortingRule.UpDown);
            List<SortingRule> leftRight = new List<SortingRule>();
            List<SortingRule> leftRight1 = new List<SortingRule>();
            leftRight.Add(SortingRule.LeftToRight);
            leftRight.Add(SortingRule.RightToLeft);
            leftRight1.Add(SortingRule.LeftToRight);
            leftRight1.Add(SortingRule.RightToLeft);
            comboBox2.DataSource = upDown;
            comboBox4.DataSource = upDown1;
            comboBox3.DataSource = leftRight;
            comboBox5.DataSource = leftRight1;

            textBox1.Text = "HB";
            textBox2.Text = "1";
            textBox3.Text = "c";
            comboBox1.SelectedIndex = 0;
            textBox4.Text = horSetting.HorGridName;

            textBox5.Text = "VB";
            textBox6.Text = "1";
            textBox8.Text = "c";
            comboBox6.SelectedIndex = 0;
            textBox7.Text = verSetting.VerGridName;

            radioButton2.Checked = true;
            radioButton3.Checked = true;

            textBox4.Enabled = false;
            textBox7.Enabled = false;
            textBox4.BackColor = System.Drawing.Color.White;
            textBox7.BackColor = System.Drawing.Color.White;
        }

        private void button1_Click(object sender, EventArgs e) // OK button
        {
            GlobalParams.IsOK = true;

            horSetting.BeamOrientation = BeamOrientation.Horizontal;
            horSetting.Prefix = textBox1.Text;
            horSetting.Suffix = Convert.ToInt32(textBox2.Text);
            horSetting.Circumfix = textBox3.Text;
            horSetting.Interfix = comboBox1.SelectedItem.ToString();
            horSetting.Rule1 = (SortingRule)comboBox2.SelectedItem;
            horSetting.Rule2 = (SortingRule)comboBox3.SelectedItem;
            horSetting.IsHorizontalBeams = true;

            verSetting.BeamOrientation = BeamOrientation.Vertical;
            verSetting.Prefix = textBox5.Text;
            verSetting.Suffix = Convert.ToInt32(textBox6.Text);
            verSetting.Circumfix = textBox8.Text;
            verSetting.Interfix = comboBox1.SelectedItem.ToString();
            verSetting.Rule1 = (SortingRule)comboBox5.SelectedItem;
            verSetting.Rule2 = (SortingRule)comboBox4.SelectedItem;
            verSetting.IsHorizontalBeams = false;

            settings.Add(horSetting);
            settings.Add(verSetting);
            _bmark.settings = settings;

            this.Close();
        }
        private void button2_Click(object sender, EventArgs e) // Cancel button
        {
            GlobalParams.IsOK = false;
            this.Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            horSampleSetting.Prefix = textBox1.Text;
            label11.Text = HorNameSample();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            horSampleSetting.Circumfix = textBox3.Text;
            label11.Text = HorNameSample();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                horSampleSetting.Suffix = Convert.ToInt32(textBox2.Text);
                label11.Text = HorNameSample();
            }
            catch (Exception)
            {
                MessageBox.Show("Please enter a number", "Wrong format!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox2.Text = "1";
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            horSampleSetting.Interfix = comboBox1.SelectedItem.ToString();
            label11.Text = HorNameSample();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            horSetting.UseHorGridName = true;
            horSampleSetting.Circumfix = horSampleSetting.HorGridName;
            label11.Text = HorNameSample();

            textBox3.Enabled = false;
            textBox3.BackColor = System.Drawing.SystemColors.Menu;
            textBox4.BackColor = System.Drawing.Color.White;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            verSetting.UseVerGridName = true;
            verSampleSetting.Circumfix = verSampleSetting.VerGridName;
            label14.Text = VerNameSample();

            textBox8.Enabled = false;
            textBox8.BackColor = System.Drawing.SystemColors.Menu;
            textBox7.BackColor = System.Drawing.Color.White;
        }

        private string HorNameSample()
        {
            string nameSample = horSampleSetting.Prefix + horSampleSetting.Interfix + horSampleSetting.Circumfix + horSampleSetting.Interfix + horSampleSetting.Suffix.ToString();
            return nameSample;
        }

        private string VerNameSample()
        {
            string nameSample = verSampleSetting.Prefix + verSampleSetting.Interfix + verSampleSetting.Circumfix + verSampleSetting.Interfix + verSampleSetting.Suffix.ToString();
            return nameSample;
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            verSampleSetting.Prefix = textBox5.Text;
            label14.Text = VerNameSample();
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            try
            {
                verSampleSetting.Suffix = Convert.ToInt32(textBox6.Text);
                label14.Text = VerNameSample();
            }
            catch (Exception)
            {
                MessageBox.Show("Please enter a number", "Wrong format!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox6.Text = "1";
            }
        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            verSampleSetting.Interfix = comboBox6.SelectedItem.ToString();
            label14.Text = VerNameSample();
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            verSampleSetting.Circumfix = textBox8.Text;
            label14.Text = VerNameSample();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            horSetting.UseHorGridName = false;
            horSampleSetting.Circumfix = textBox3.Text;
            label11.Text = HorNameSample();

            textBox4.BackColor = System.Drawing.SystemColors.Menu;
            textBox3.BackColor = System.Drawing.Color.Empty;
            textBox3.Enabled = true;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            verSetting.UseVerGridName = false;
            verSampleSetting.Circumfix = textBox8.Text;
            label14.Text = VerNameSample();

            textBox7.BackColor = System.Drawing.SystemColors.Menu;
            textBox8.BackColor = System.Drawing.Color.Empty;
            textBox8.Enabled = true;
        }
    }
}
