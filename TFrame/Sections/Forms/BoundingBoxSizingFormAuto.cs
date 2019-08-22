using System;
using System.Windows.Forms;

namespace TFrame
{
    public partial class BoundingBoxSizingFormAuto : Form
    {
        SectionForm _sf;
        public Section Section { get; private set; }
        public CrossViewSections _cvs;
        public LongViewSections _lvs;
        bool IsLongSections;

        double FtToMm;

        public BoundingBoxSizingFormAuto(Section section, CrossViewSections cvs)
        {
            _cvs = cvs;
            Section = section;
            InitializeComponent();
            Initialize();
        }

        public BoundingBoxSizingFormAuto(Section section, LongViewSections lvs)
        {
            IsLongSections = true;
            _lvs = lvs;
            Section = section;
            InitializeComponent();
            Initialize();
        }

        void Initialize()
        {
            FtToMm = GlobalParams.FtToMm;
            label6.Text = (Section.ViewSectionName != null) ? Section.ViewSectionName : "";
            if (!IsLongSections)
            {
                for (int i = 0; i < _cvs.hosts.Count; i++)
                {
                    dataGridView1.Rows.Add();
                    if (Section.Host != null)
                    {
                        dataGridView1.Rows[i].Cells[0].Value = Section.Host;
                        break;
                    }
                    else dataGridView1.Rows[i].Cells[0].Value = _cvs.hosts[i];
                }
            }
            else
            {
                for (int i = 0; i < _lvs.hosts.Count; i++)
                {
                    dataGridView1.Rows.Add();
                    if (Section.Host != null)
                    {
                        dataGridView1.Rows[i].Cells[0].Value = Section.Host;
                        break;
                    }
                    else dataGridView1.Rows[i].Cells[0].Value = _lvs.hosts[i];
                }
            }

            _sf = (SectionForm)Application.OpenForms["SectionForm"];
            numericUpDown1.Value = (decimal)Section.XMaxExtra * (decimal)FtToMm;
            numericUpDown2.Value = (decimal)Section.YMaxExtra * (decimal)FtToMm;
            numericUpDown3.Value = (decimal)Section.ZMaxExtra * (decimal)FtToMm;
            numericUpDown4.Value = (decimal)Section.XMinExtra * (decimal)FtToMm;
            numericUpDown5.Value = (decimal)Section.YMinExtra * (decimal)FtToMm;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Section.XMaxExtra = (double)numericUpDown1.Value / FtToMm;
            Section.YMaxExtra = (double)numericUpDown2.Value / FtToMm;
            Section.ZMaxExtra = (double)numericUpDown3.Value / FtToMm;
            Section.XMinExtra = (double)numericUpDown4.Value / FtToMm;
            Section.YMinExtra = (double)numericUpDown5.Value / FtToMm;
            Close();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
        }

       
        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }
    }
}
