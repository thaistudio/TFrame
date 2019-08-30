using System.Windows;
using System.Windows.Forms;
using SaveLoadUI;
using Autodesk.Revit.DB;

namespace TFrame
{
    /// <summary>
    /// Interaction logic for BeamDimUC.xaml
    /// </summary>
    public partial class BeamDimUC : BaseWindowWPF
    {
        string path = @"D:\Thai\Code\Revit\TFrame\Data\WPF.xml";
        BeamDimensionData formData;
        public string mess { get; set; }
        public BeamDimUC(Document doc) : base()
        {
            Initialize();
            InitializeComponent();
            RevitData rd = new RevitData(doc);
            rd.GenList<DimensionType>();
            dimStyleComboBox.ItemsSource = rd.LinearDimensioTypes;

            WPF.LoadWinFormUI(this, path);
            WPF.LoadComboBoxItemsSource(this, path);
            if (breakLineFamDirectory.Items.IsEmpty) breakLineFamDirectory.Items.Add(GlobalParams.BreakLineDirectory);
        }

        void Initialize()
        {
            formData = BeamDimensionData.Singleton;
            DataContext = formData;
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            WPF.SaveWinFormUI(this, path);
            BeamDimensionData.Singleton.OK = true;
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            BeamDimensionData.Singleton.OK = false;
            Close();
        }

        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog folderBrowserDialog = new OpenFileDialog();
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                breakLineFamDirectory.Items.Add(folderBrowserDialog.FileName);
                breakLineFamDirectory.Text = folderBrowserDialog.FileName;
            }
        }
    }
}
