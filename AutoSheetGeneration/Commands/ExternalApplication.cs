using Autodesk.Revit.UI;
using System;
using System.Reflection;

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

            PushButton tagButton = panel.AddItem(buttonData) as PushButton;
            tagButton.ToolTip = "Automatically creates Sheets and ViewPorts For Structural Plans";

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            // No cleanup needed
            return Result.Succeeded;
        }
    }
}