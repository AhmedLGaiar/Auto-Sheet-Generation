using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;

namespace AutoSheetGeneration.Commands
{
    public class ExternalApplication : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            // Create Tab
            string tabName = "SheetMaster";
            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch (Exception)
            {
                // Tab may already exist – safe to ignore
            }

            // Create Panel
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Create");

            // Path to the DLL
            string path = Assembly.GetExecutingAssembly().Location;

            // Tag Button
            PushButtonData buttonData = new PushButtonData(
                "Create",
                "Create Sheets",
                path,
                typeof(AutoGenerator).FullName
            );

            PushButton createButton = panel.AddItem(buttonData) as PushButton;
            createButton.ToolTip = "Automatically creates Sheets and ViewPorts For Structural Plans";

            // Load image from embedded resource
            var iconStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("AutoSheetGeneration.Resources.SheetIcon.png");

            if (iconStream != null)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = iconStream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                createButton.LargeImage = bitmap;
            }
            else
            {
                TaskDialog.Show("Error", "Icon resource not found!");
            }

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            // No cleanup needed
            return Result.Succeeded;
        }
    }
}