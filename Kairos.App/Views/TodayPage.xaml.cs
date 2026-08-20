using Kairos.App.Services;
using Kairos.App.ViewModels;
using Kairos.Shared.DTOs;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Kairos.App.Views;

public partial class TodayPage : ContentPage
{
	private readonly ApiService _api = new();
	private readonly ObservableCollection<TodoGroup> _groups = new();

    public ObservableCollection<TodoGroup> Groups => _groups;

    public TodayPage()
	{
		InitializeComponent();
        //TodoList.ItemsSource = _groups;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        StarredCheck.IsToggled = TodayFilterSettings.UseStarred;
        DueDateCheck.IsToggled = TodayFilterSettings.UseDueDate;
        DueBeforePicker.Date = TodayFilterSettings.DueBefore;

        await LoadTodayAsync();
    }

    private async Task LoadTodayAsync()
    {
		try
		{
            Loading.IsVisible = true;
            Loading.IsRunning = true;
            EmptyView.IsVisible = false;

            if (!TodayFilterSettings.UseStarred && !TodayFilterSettings.UseDueDate)
            {
                _groups.Clear();
                EmptyView.IsVisible = true;
                return;
            }

            var projects = await _api.GetProjectsAsync();
            var projectNames = projects.ToDictionary(p => p.ID, p => p.Name);

            var all = await _api.GetTodosAsync();
            IEnumerable<TodoResponse> filtered = all;

            if (TodayFilterSettings.UseDueDate)
            {
                var limit = TodayFilterSettings.DueBefore.Date;
                filtered = filtered.Where(t => 
                t.DueDate != null && 
                t.DueDate.Value.ToLocalTime().Date <= limit);
            }

            if (TodayFilterSettings.UseStarred)
            {
                var starred = await _api.GetTodayTodosAsync();
                var starredIds = starred.Select(t => t.ID).ToHashSet();
                filtered = filtered.Where(t => starredIds.Contains(t.ID));
            }

            var grouped = filtered
                .GroupBy(t => t.ProjectID)
                .Select(g => new TodoGroup(projectNames.TryGetValue(g.Key, out var name) ? name : "기타", g.OrderByDescending(t => t.Priority)
                .Select(t => new TodoViewModel(t))
                ));

            _groups.Clear();
            foreach (var group in grouped)
                _groups.Add(group);

            EmptyView.IsVisible = _groups.Count == 0;
        }
		catch (Exception ex)
		{
            await DisplayAlert("오류", $"불러오기 실패: {ex.Message}", "확인");
        }
        finally
        {
            Loading.IsVisible = false;
            Loading.IsRunning = false;
        }
    }

    private async void OnFilterChanged(object sender, EventArgs e)
    {
        TodayFilterSettings.UseStarred = StarredCheck.IsToggled;
        TodayFilterSettings.UseDueDate = DueDateCheck.IsToggled;

        await LoadTodayAsync();
    }

    private async void OnDueBeforeChanged(object sender, EventArgs e)
    {
        TodayFilterSettings.DueBefore = DueBeforePicker.Date;
        await LoadTodayAsync();
    }

    private async void OnCheckChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox cb && cb.BindingContext is TodoViewModel todo)
        {
            if (todo.IsCompleted == e.Value)
                return;

            try
            {
                await _api.SetCompletedAsync(todo.ID, e.Value);
                todo.IsCompleted = e.Value;
                await LoadTodayAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("오류", $"변경 실패: {ex.Message}", "확인");
            }
        }
    }

    private async void OnAccountClicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet(
            "계정", "취소", null, "로그아웃", "회원 탈퇴");

        if (action == "로그아웃")
        {
            await Logout();
        }
        else if (action == "회원 탈퇴")
        {
            await DeleteAccount();
        }
    }

    private async Task Logout()
    {
        bool ok = await DisplayAlert("로그아웃", "로그아웃할까요?", "로그아웃", "취소");
        if (!ok) return;

        _api.Logout();
        await Shell.Current.GoToAsync("//LoginPage");
    }

    private async Task DeleteAccount()
    {
        bool ok = await DisplayAlert(
       "회원 탈퇴",
       "정말 탈퇴할까요?\n모든 프로젝트와 할 일이 삭제되며 되돌릴 수 없습니다.",
       "탈퇴", "취소");
        if (!ok) return;

        bool confirm = await DisplayAlert(
        "최종 확인",
        "이 작업은 되돌릴 수 없습니다. 계속할까요?",
        "네, 탈퇴합니다", "취소");
        if (!confirm) return;

        try
        {
            bool success = await _api.DeleteAccountAsync();
            if (success)
            {
                await Shell.Current.GoToAsync("//LoginPage");
            }
            else
            {
                await DisplayAlert("오류", "탈퇴에 실패했어요. 다시 시도해주세요.", "확인");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("오류", $"탈퇴 실패: {ex.Message}", "확인");
        }
    }
}