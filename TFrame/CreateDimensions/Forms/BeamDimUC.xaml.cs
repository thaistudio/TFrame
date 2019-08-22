using System.Windows;

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

            WPFDataTools.LoadWinFormUI(this, path);
        }

        void Initialize()
        {
            formData = BeamDimensionData.Singleton;
            DataContext = formData;
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            WPFDataTools.SaveWinFormUI(this, path);
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
