using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSheetGeneration.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class ColumnAndAxis : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uIDocument = commandData.Application.ActiveUIDocument;

            Document document = uIDocument.Document;
            ViewFamilyType viewFamilyType = new FilteredElementCollector(document)
                .OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>()
                .FirstOrDefault(x => ViewFamily.StructuralPlan == x.ViewFamily);
            Level level = new FilteredElementCollector(document)
                .OfClass(typeof(Level)).Cast<Level>()
                .FirstOrDefault(x => x.Name == "02-_STR_FC");
            try
            {
                using (Transaction transaction = new Transaction(document, "Sheet Generated"))
                {
                    transaction.Start();
                    ViewPlan viewPlan = ViewPlan.Create(document, viewFamilyType.Id, level.Id);
                    viewPlan.Name = "Columns and Axis Plan";

                    List<BuiltInCategory> structuralCategories = new List<BuiltInCategory>
                    {
                        BuiltInCategory.OST_StructuralFraming,
                        BuiltInCategory.OST_Floors,
                        BuiltInCategory.OST_StructuralFoundation,
                        BuiltInCategory.OST_FloorsStructure,
                        BuiltInCategory.OST_Walls,
                        BuiltInCategory.OST_StructuralFramingSystem,
                        BuiltInCategory.OST_Rebar,
                        BuiltInCategory.OST_HiddenStructuralFoundationLines,
                        BuiltInCategory.OST_AreaRein,
                        BuiltInCategory.OST_Elev,
                        BuiltInCategory.OST_Lines,
                        BuiltInCategory.OST_CLines,
                    };

                    foreach (BuiltInCategory category in structuralCategories)
                    {
                        ElementId categoryId = new ElementId((int)category);

                        // Check if the category can be hidden in the view
                        if (viewPlan.CanCategoryBeHidden(categoryId))
                        {
                            // Set the visibility of the category
                            viewPlan.SetCategoryHidden(categoryId, true); // true = hide, false = unhide
                        }
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