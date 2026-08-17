using Kairos.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kairos.App.ViewModels
{
    public class ProjectItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly ProjectResponse _project;

        public ProjectItemViewModel(ProjectResponse projectResponse)
        {
            _project = projectResponse;
        }

        public int ID => _project.ID;
        public string Name => _project.Name;
        public bool IsToday => _project.IsToday;
        public int CompletedCount => _project.CompletedCount;
        public int TotalCount => _project.TotalCount;

        public double Progress =>
            _project.TotalCount > 0
            ? (double)_project.CompletedCount / _project.TotalCount : 0.0;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IconBackground));
                OnPropertyChanged(nameof(IconTextColor));
            }
        }

        public Color IconBackground 
            => _isSelected ? Color.FromArgb("#5EEAD4") : Color.FromArgb("#1E2127");
        public Color IconTextColor
            => _isSelected ? Color.FromArgb("#16181D") : Color.FromArgb("#8894A5");

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
