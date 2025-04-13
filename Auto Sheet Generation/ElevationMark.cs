using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auto_Sheet_Generation
{
    [Transaction(TransactionMode.Manual)]
    public class ElevationMark : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uIDocument = commandData.Application.ActiveUIDocument;
            Document document = uIDocument.Document;

            List<ElevationMarker> elevationMarkers = new FilteredElementCollector(document)
                .OfClass(typeof(ElevationMarker))
                .Cast<ElevationMarker>()
                .ToList();

            List<ViewPlan> viewCollector = new FilteredElementCollector(document)
                       .OfClass(typeof(ViewPlan)) // Filter specifically for ViewPlan
                       .WhereElementIsNotElementType() // Exclude element types
                       .Cast<ViewPlan>() // Cast to ViewPlan objects
                       .Where(v => !v.IsTemplate) // Exclude templates 
                       .ToList(); // Convert to a list
            if (elevationMarkers == null)
            {
                message = "No elevation marker found!";
                return Result.Failed;
            }

            BoundingBoxXYZ boundingBoxXYZ = viewCollector[0].get_BoundingBox(null);
            try
            {
                using (Transaction transaction = new Transaction(document, "Sheet Generated"))
                {

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



