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
using System.ComponentModel;
using System.Numerics;

namespace AutoSheetGeneration.ViewModels
{
    public class CreateSheetsViewModel : ObservableObject
    {

        #region Lists
        public ObservableCollection<ViewPlanOption> ViewPlans { get; } = new ObservableCollection<ViewPlanOption>();
        public ObservableCollection<TitleBlockOption> TitleBlock { get; } = new ObservableCollection<TitleBlockOption>();

        #endregion

        #region Commands

        private RelayCommand _CreateSheetCommand;
        public ICommand CreateSheetCommand => _CreateSheetCommand;


        private RelayCommand _SelectAllCommand;
        public ICommand SelectAllCommand => _SelectAllCommand;


        private RelayCommand _CreateColumnsandaxisplan;
        public ICommand CreateColumnsandaxisplan => _CreateColumnsandaxisplan;

        #endregion

        #region Property

        private TitleBlockOption _titleBlockOption;
        public TitleBlockOption TitleBlockOption
        {
            get => _titleBlockOption;
            set
            {
                if (SetProperty(ref _titleBlockOption, value))
                {
                    UpdateCommandStates();
                }
            }
        }

        #endregion

        #region Ctor
        public CreateSheetsViewModel()
        {
            _CreateSheetCommand = new RelayCommand(CreateSheet, CanExecuteSelectedPlans);
            _SelectAllCommand = new RelayCommand(() => SelectAll(true));
            _CreateColumnsandaxisplan = new RelayCommand(CreateColnaxisPlan, CanExecuteSelectedPlans);
            // Get structural ViewPlans
            List<ViewPlan> structuralPlans = new FilteredElementCollector(AutoGenerator.document)
                .OfClass(typeof(ViewPlan))
                .Cast<ViewPlan>()
                .Where(v => !v.IsTemplate).OrderBy(v => v.Name).ToList();
            LoadViewPlans(structuralPlans);

            // Get Title Blocks
            List<FamilySymbol> fs = new FilteredElementCollector(AutoGenerator.document)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .Cast<FamilySymbol>().ToList();

            LoadTitleBlocks(fs);

        }

        #endregion

        private bool CanExecuteSelectedPlans()
        {
            return ViewPlans.Any(v => v.IsSelected) && !(_titleBlockOption==null);
        }

        public void LoadViewPlans(IEnumerable<ViewPlan> plans)
        {
            ViewPlans.Clear(); // optional: reset if reloading
            foreach (var plan in plans)
            {
                var option = new ViewPlanOption(plan);
                option.SelectionChanged = UpdateCommandStates; // ← callback
                ViewPlans.Add(option);
            }
        }

        public void LoadTitleBlocks(IEnumerable<FamilySymbol> blocks)
        {
            TitleBlock.Clear(); // optional: reset if reloading
            foreach (var block in blocks)
            {
                var option = new TitleBlockOption(block);
                TitleBlock.Add(option);
            }

            if (TitleBlock.Count > 0)
                TitleBlockOption = TitleBlock[0];
        }

        private void UpdateCommandStates()
        {
            _CreateSheetCommand.NotifyCanExecuteChanged();
            _CreateColumnsandaxisplan.NotifyCanExecuteChanged();
        }
        public void SelectAll(bool select)
        {
            foreach (var vp in ViewPlans)
                vp.IsSelected = select;
        }

        private void CreateSheet()
        {
            // Get title block
            FamilySymbol fs = new FilteredElementCollector(AutoGenerator.document)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .Cast<FamilySymbol>()
                .FirstOrDefault(e=>e.Name== _titleBlockOption.Name);

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

            using (Transaction transaction = new Transaction(AutoGenerator.document, "Sheet Generated"))
            {
                transaction.Start();

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

        private void CreateColnaxisPlan()
        {

            var selectedViews = ViewPlans
                .Where(v => v.IsSelected)
                .Select(v => v.ViewPlan)
                .ToList();

            if (!selectedViews.Any())
            {
                TaskDialog.Show("Info", "No plans were selected.");
                return;
            }

            ViewFamilyType viewFamilyType = new FilteredElementCollector(AutoGenerator.document)
                .OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>()
                .FirstOrDefault(x => ViewFamily.StructuralPlan == x.ViewFamily);

            if (viewFamilyType == null)
            {
                TaskDialog.Show("Error", "Structural ViewFamilyType not found.");
                return;
            }

            var allLevels = new FilteredElementCollector(AutoGenerator.document)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .ToDictionary(l => l.Name, l => l);

            using (Transaction transaction = new Transaction(AutoGenerator.document, "Plan Generated"))
            {
                transaction.Start(); 

                foreach (var planOption in selectedViews)
                {
                    if (!allLevels.TryGetValue(planOption.Name, out var level))
                    {
                        TaskDialog.Show("Warning", $"Level not found for plan name: {planOption.Name}");
                        continue;
                    }

                    ViewPlan viewPlan = ViewPlan.Create(AutoGenerator.document, viewFamilyType.Id, level.Id);

                    viewPlan.Name = $"Colnaxis - {level.Name}";

                    var option = new ViewPlanOption(viewPlan);
                    option.SelectionChanged = UpdateCommandStates; // ← callback
                    ViewPlans.Add(option);

                    HideElements(viewPlan);

                }

                transaction.Commit();
            }
        }

        private void HideElements(ViewPlan viewPlan)
        {

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
        }
    }
}
