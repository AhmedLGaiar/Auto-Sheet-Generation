using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoSheetGeneration.Views;
using AutoSheetGeneration.ViewModels;

namespace AutoSheetGeneration.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class AutoGenerator : IExternalCommand
    {
        public static Document document;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uIDocument = commandData.Application.ActiveUIDocument;

            document = uIDocument.Document;
            try
            {
                var sheetView = new CreateSheetsView();
                sheetView.ShowDialog();
                return Result.Succeeded;
            }

            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
