using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Media.Imaging;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace TFrame
{
    public class NewRibbon : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            // Add a new ribbon panel 
            RibbonPanel ribbonPanel = application.CreateRibbonPanel("TFrame Ribbon Panel");

            // Create a push button to trigger a command and add it to the ribbon panel.
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData buttonData = new PushButtonData("TFrame", "Thai Studio", thisAssemblyPath, "TFrame.TFrame");

            PushButton pushButton = (PushButton)ribbonPanel.AddItem(buttonData);

            // Add tooltip
            pushButton.ToolTip = "Use this command to create a frame.";

            // Add image
            Uri uriImage = new Uri(@"D:\Thai\Code\Revit\Thai addin\Ribbon\Image.png");
            BitmapImage largeImage = new BitmapImage(uriImage);
            pushButton.LargeImage = largeImage;

            return Result.Succeeded;
        }
    }

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class TFrame : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TaskDialog.Show("T", "H");
            return Result.Succeeded;
        }
    }
}
