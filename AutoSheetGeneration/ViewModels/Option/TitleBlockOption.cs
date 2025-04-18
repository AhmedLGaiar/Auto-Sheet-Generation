using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSheetGeneration.ViewModels.Option
{
    public class TitleBlockOption : ObservableObject
    {
        public FamilySymbol titleBlock { get; }

        public string Name => titleBlock.Name;

        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                SetProperty(ref _isSelected, value);
            }
        }

        public TitleBlockOption(FamilySymbol titleBlock)
        {
            this.titleBlock = titleBlock;
        }
    }
}
