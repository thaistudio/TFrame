using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
namespace TFrame
{
    public partial class TagRebarsForm : System.Windows.Forms.Form
    {
        ExternalCommandData _externalCommandData;
        TagActions _tagAction;
        double ftToMm;

        public TagRebarsForm(TagActions tagAction)
        {
            _tagAction = tagAction;
            _externalCommandData = _tagAction.externalCommandData;
            InitializeComponent();
            Initialize(_externalCommandData);
        }

        public void Initialize(ExternalCommandData commandData)
        {
            ftToMm = GlobalParams.FtToMm;

            Document doc = commandData.Application.ActiveUIDocument.Document;
            
            comboBox5.DataSource = FamilyTools.SearchFamilyByBuiltInCategory(BuiltInCategory.OST_RebarTags);
            comboBox5.DisplayMember = "Name";

            comboBox6.DataSource = FamilyTools.SearchFamilyByBuiltInCategory(BuiltInCategory.OST_RebarTags);
            comboBox6.DisplayMember = "Name";

            comboBox8.DataSource = FamilyTools.SearchFamilyByBuiltInCategory(BuiltInCategory.OST_RebarTags);
            comboBox8.DisplayMember = "Name";

            List<Element> l = FamilyTools.SearchFamilySymbolAsElements<MultiReferenceAnnotationType>(true);
            comboBox2.DataSource = l;
            comboBox2.DisplayMember = "Name";

            List<Element> dimensionStyle = FamilyTools.SearchFamilySymbolOfFamilyName<DimensionType>("Linear Dimension Style");
            comboBox3.DataSource = dimensionStyle;
            comboBox3.DisplayMember = "Name";

            numericUpDown1.Value = (decimal)_tagAction.stirrupTagLength;
            numericUpDown2.Value = (decimal)_tagAction.l;
            numericUpDown3.Value = (decimal)_tagAction.firstDimensionZ;
            numericUpDown4.Value = (decimal)_tagAction.zIncreament;

            checkBox2.Checked = true;
            checkBox4.Checked = true;
            checkBox5.Checked = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _tagAction.l = Convert.ToDouble(numericUpDown2.Value) / ftToMm;
            _tagAction.stirrupTagLength = Convert.ToDouble(numericUpDown1.Value) / ftToMm;
            _tagAction.firstDimensionZ = Convert.ToDouble(numericUpDown3.Value) / ftToMm;
            _tagAction.zIncreament = Convert.ToDouble(numericUpDown4.Value) / ftToMm;

            _tagAction.IsOK = true;
            _tagAction.multiTagTypeId = ((MultiReferenceAnnotationType)(comboBox2.SelectedItem)).Id;
            _tagAction.dimType = (DimensionType)comboBox3.SelectedItem;
            _tagAction.desiredFamily = ((FamilySymbol)(comboBox1.SelectedItem)).Id;
            _tagAction.stirrupDesiredFamily = ((FamilySymbol)(comboBox4.SelectedItem)).Id;
            _tagAction.tagTypeId = ((FamilySymbol)(comboBox7.SelectedItem)).Id;
            
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _tagAction.IsOK = false;
            Close();
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox1.DataSource = FamilyTools.SearchFamilySymbolByFamily((Family)comboBox5.SelectedItem);
            comboBox1.DisplayMember = "Name";
        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox4.DataSource = FamilyTools.SearchFamilySymbolByFamily((Family)comboBox6.SelectedItem);
            comboBox4.DisplayMember = "Name";
        }

        private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox7.DataSource = FamilyTools.SearchFamilySymbolByFamily((Family)comboBox8.SelectedItem);
            comboBox7.DisplayMember = "Name";
        }


        private void checkBox1_Click(object sender, EventArgs e)
        {
            _tagAction.topCoeff = -1;
            if (checkBox2.Checked) checkBox2.Checked = false;
        }

        private void checkBox2_Click(object sender, EventArgs e)
        {
            _tagAction.topCoeff = 1;
            if (checkBox1.Checked) checkBox1.Checked = false;
        }

        private void checkBox3_Click(object sender, EventArgs e)
        {
            _tagAction.stirrupCoeff = -1;
            checkBox4.Checked = false;
        }

        private void checkBox4_Click(object sender, EventArgs e)
        {
            _tagAction.stirrupCoeff = 1;
            checkBox3.Checked = false;
        }

        private void checkBox6_Click(object sender, EventArgs e)
        {
            _tagAction.botCoeff = -1;
            checkBox5.Checked = false;
        }

        private void checkBox5_Click(object sender, EventArgs e)
        {
            _tagAction.botCoeff = 1;
            checkBox6.Checked = false;
        }
    }
}
