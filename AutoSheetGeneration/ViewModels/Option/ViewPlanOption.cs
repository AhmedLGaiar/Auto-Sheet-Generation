using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSheetGeneration.ViewModels.Option
{
    public class ViewPlanOption : ObservableObject
    {
        public ViewPlan ViewPlan { get; }

        public string Name => ViewPlan.Name;

        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public ViewPlanOption(ViewPlan viewPlan)
        {
            ViewPlan = viewPlan;
        }
    }
}