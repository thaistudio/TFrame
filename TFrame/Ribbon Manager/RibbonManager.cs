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
    public class RibbonManager : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            // Create a new tab
            string tabName = "ThaiStudio";
            string panelName = "Thai's rebar";
            application.CreateRibbonTab(tabName);

            // Create a new panel in tab
            RibbonPanel ribbonPanel = application.CreateRibbonPanel(tabName, panelName);

            
            // Create a push button to trigger a command and add it to the ribbon panel.
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData buttonData = new PushButtonData("TFrame", "Create Rebar", thisAssemblyPath, "TFrame.CreateRebar");
            PushButtonData buttonData2 = new PushButtonData("TCoordinate", "Coordinate", thisAssemblyPath, "TFrame.Coordinate");
            PushButtonData buttonData3 = new PushButtonData("TSections", "Create Sections", thisAssemblyPath, "TFrame.CreateViewSections");
            PushButtonData buttonData4 = new PushButtonData("TRebarsTag", "Tag Rebars", thisAssemblyPath, "TFrame.Tag.TagRebars");
            PushButtonData buttonData5 = new PushButtonData("MultiTag", "Tag Multi Rebars", thisAssemblyPath, "TFrame.Tag.TagMultiRefAnnotation");

            PushButton pushButton = (PushButton)ribbonPanel.AddItem(buttonData);
            PushButton pushButton2 = (PushButton)ribbonPanel.AddItem(buttonData2);
            PushButton pushButton3 = (PushButton)ribbonPanel.AddItem(buttonData3);
            PushButton pushButton4 = (PushButton)ribbonPanel.AddItem(buttonData4);
            PushButton pushButton5 = (PushButton)ribbonPanel.AddItem(buttonData5);

            // Add tooltip
            pushButton.ToolTip = "Use this command to create a frame.";

            // Add image
            Uri uriImage = new Uri(@"D:\Thai\Code\Revit\Thai addin\Ribbon\Image.png");
            BitmapImage largeImage = new BitmapImage(uriImage);
            pushButton.LargeImage = largeImage;

            //ElementClassFilter elements = new ElementClassFilter(typeof(ViewSection));

            //TUpdate tUp = new TUpdate(new Guid("edf41d36-061d-44fd-b2e9-46d176b8665f"));
            //UpdaterRegistry.RegisterUpdater(tUp, true);
            //UpdaterRegistry.AddTrigger(tUp.GetUpdaterId(), elements, Element.GetChangeTypeAny());
            //UpdaterRegistry.UnregisterUpdater(tUp.GetUpdaterId());
            return Result.Succeeded;
        }
    }

}
