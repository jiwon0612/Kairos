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

        private async void OnEditProject(object sender, EventArgs e)
        {
            if (sender is Button item && item.BindingContext is ProjectResponse project)
            {
                var newName = await DisplayPromptAsync(
                    "프로젝트 수정",
                    "새로운 프로젝트 이름을 입력하세요:",
                    initialValue: project.Name);

                if (string.IsNullOrWhiteSpace(newName))
                    return;

                try
                {
                    await _api.UpdateProjectAsync(project.ID, newName);
                    await LoadProjectsAsync();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("오류", $"수정 실패: {ex.Message}", "확인");
                }
            }
        }

        private async void OnDeleteProject(object sender, EventArgs e)
        {
            if (sender is Button item && item.BindingContext is ProjectResponse project)
            {
                bool ok = await DisplayAlert(
                    "프로젝트 삭제",
                    $"'{project.Name}'을(를) 삭제할까요?\n포함된 할 일도 모두 삭제됩니다.",
                    "삭제",
                    "취소");
                if (!ok) return;

                try
                {
                    await _api.DeleteProjectAsync(project.ID);
                    await LoadProjectsAsync();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("오류", $"삭제 실패: {ex.Message}", "확인");
                }
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            _api.Logout();
            await Shell.Current.GoToAsync("//LoginPage");
        }

        private async void OnToggleToday(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is ProjectResponse project)
            {
                try
                {
                    bool newValue = !project.IsToday;
                    await _api.SetProjectTodayAsync(project.ID, newValue);
                    await LoadProjectsAsync();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("오류", $"변경 실패: {ex.Message}", "확인");
                }
            }
        }
    }
}
