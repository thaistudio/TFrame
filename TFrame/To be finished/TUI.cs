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
using Form = System.Windows.Forms.Form;

namespace TFrame
{
    

    public partial class TUI : Form
    {
        CreateRebar createRebar = new CreateRebar();
        SupportClass _sup;
        public TUI()
        {
        }

        public TUI(SupportClass sup)
        {
            _sup = sup;
            InitializeComponent();
            ListData(comboBox1, sup);
            ListData(comboBox2, sup);
        }

        private void ListData(ListControl list, SupportClass sup)
        {
            list.DataSource = null;
            list.DataSource = sup.Symbols;
            list.DisplayMember = "Name";
            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            _sup.SetFamilySymbols(comboBox1.SelectedItem);
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
    }
}
