using Kairos.App.Services;
using Kairos.Shared.DTOs;
using System.Collections.ObjectModel;

namespace Kairos.App
{
    public partial class MainPage : ContentPage
    {
        private readonly ApiService _api = new();
        private readonly ObservableCollection<ProjectResponse> _projects = new();

        public MainPage()
        {
            InitializeComponent();
            ProjectList.ItemsSource = _projects;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadProjectsAsync();
        }

        private async Task LoadProjectsAsync()
        {
            try
            {
                var projects = await _api.GetProjectsAsync();
                _projects.Clear();
                foreach (var project in projects)
                {
                    _projects.Add(project);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("오류", $"프로젝트 불러오기 실패: {ex.Message}", "확인");
            }
        }

        private async void OnAddClicked(object sender, EventArgs e)
        {
            var name = NameEntry.Text;
            if (string.IsNullOrWhiteSpace(name))
                return;

            try
            {
                await _api.CreateProjectAsync(name);
                NameEntry.Text = string.Empty;
                await LoadProjectsAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("오류", $"추가 실패: {ex.Message}", "확인");
            }
        }

        private async void OnProjectSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is not ProjectResponse selectedProject)
                return;

            ProjectList.SelectedItem = null;

            await Shell.Current.GoToAsync($"TodoPage?projectId={selectedProject.ID}");
        }
    }
}
