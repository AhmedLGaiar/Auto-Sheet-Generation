using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Auto_Sheet_Generation
{
    [Transaction(TransactionMode.Manual)]
    public class AutoGenerator : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIDocument uIDocument = commandData.Application.ActiveUIDocument;

            Document document = uIDocument.Document;
            try
            {
                using (Transaction transaction = new Transaction(document, "Sheet Generated"))
                {
                    transaction.Start();
                    // Create Empty Sheets
                    FilteredElementCollector element = new FilteredElementCollector(document)
                            .OfClass(typeof(FamilySymbol))
                            .OfCategory(BuiltInCategory.OST_TitleBlocks);

                    FamilySymbol fs = element.FirstOrDefault() as FamilySymbol;

                    // add ViewPort in Sheet
                    List<ViewPlan> viewCollector = new FilteredElementCollector(document)
                          .OfClass(typeof(ViewPlan)) // Filter specifically for ViewPlan
                          .WhereElementIsNotElementType() // Exclude element types
                          .Cast<ViewPlan>() // Cast to ViewPlan objects
                          .Where(v => !v.IsTemplate ) // Exclude templates 
                          .ToList(); // Convert to a list


                    int sheetCounter = 1;
                    foreach (ViewPlan structuralPlan in viewCollector)
                    {
                        ViewSheet newSheet = ViewSheet.Create(document, fs.Id);

                        // Set sheet name and number
                        newSheet.Name = $"Sheet for {structuralPlan.Name}";
                        newSheet.SheetNumber = $"S-{sheetCounter:D2}";

                        // Place the view on the sheet
                        BoundingBoxUV outline = newSheet.Outline;
                        UV location = new UV(
                            (outline.Max.U + outline.Min.U) / 2,
                            (outline.Max.V + outline.Min.V) / 2
                        ); 
                        Viewport.Create(document, newSheet.Id, structuralPlan.Id, new XYZ(location.U, location.V, 0));

                        sheetCounter++;
                    }
                    transaction.Commit();
                }
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
