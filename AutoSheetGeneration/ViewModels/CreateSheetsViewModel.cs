using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Autodesk.Revit.DB;
using AutoSheetGeneration.ViewModels.Option;
using AutoSheetGeneration.Commands;
using System.Collections.ObjectModel;
using Autodesk.Revit.UI;
using Autodesk.Revit.Creation;
using System.Xml.Linq;

namespace AutoSheetGeneration.ViewModels
{
    public class CreateSheetsViewModel : ObservableObject
    {
        public ObservableCollection<ViewPlanOption> ViewPlans { get; } = new ObservableCollection<ViewPlanOption>();

        public IRelayCommand CreateSheetCommand { get; }
        public IRelayCommand SelectAllCommand { get; }

        public CreateSheetsViewModel()
        {
            CreateSheetCommand = new RelayCommand(CreateSheet);
            SelectAllCommand = new RelayCommand(() => SelectAll(true));
            // Get structural ViewPlans
            List<ViewPlan> structuralPlans = new FilteredElementCollector(AutoGenerator.document)
                .OfClass(typeof(ViewPlan))
                .Cast<ViewPlan>()
                .Where(v => !v.IsTemplate).OrderBy(v=>v.Name).ToList();
            LoadViewPlans(structuralPlans);
        }

        public void LoadViewPlans(IEnumerable<ViewPlan> plans)
        {
            ViewPlans.Clear(); // optional: reset if reloading
            foreach (var plan in plans)
            {
                ViewPlans.Add(new ViewPlanOption(plan));
            }
        }

        public void SelectAll(bool select)
        {
            foreach (var vp in ViewPlans)
                vp.IsSelected = select;
        }

        private void CreateSheet()
        {
            using (Transaction transaction = new Transaction(AutoGenerator.document, "Sheet Generated"))
            {
                transaction.Start();

                // Get title block
                FamilySymbol fs = new FilteredElementCollector(AutoGenerator.document)
                    .OfClass(typeof(FamilySymbol))
                    .OfCategory(BuiltInCategory.OST_TitleBlocks)
                    .Cast<FamilySymbol>()
                    .FirstOrDefault();

                if (fs == null)
                {
                    TaskDialog.Show("Error", "No title block found.");
                    return;
                }

                // Only create sheets for selected ViewPlanOptions
                var selectedViews = ViewPlans
                    .Where(v => v.IsSelected)
                    .Select(v => v.ViewPlan)
                    .ToList();

                int sheetCounter = 1;
                foreach (var structuralPlan in selectedViews)
                {
                    ViewSheet newSheet = ViewSheet.Create(AutoGenerator.document, fs.Id);
                    newSheet.Name = $"Sheet for {structuralPlan.Name}";
                    newSheet.SheetNumber = $"S-{sheetCounter:D2}";

                    BoundingBoxUV outline = newSheet.Outline;
                    UV location = new UV(
                        (outline.Max.U + outline.Min.U) / 2,
                        (outline.Max.V + outline.Min.V) / 2
                    );

                    Viewport.Create(AutoGenerator.document, newSheet.Id, structuralPlan.Id,
                        new XYZ(location.U, location.V, 0));

                    sheetCounter++;
                }

                transaction.Commit();
            }
        }
    }
}
