using Kairos.App.Services;
using Kairos.App.ViewModels;
using Kairos.Shared.DTOs;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Kairos.App.Views;

public partial class TodayPage : ContentPage
{
	private readonly ApiService _api = new();
	private readonly ObservableCollection<TodoViewModel> _todos = new();

    public TodayPage()
	{
		InitializeComponent();
		TodoList.ItemsSource = _todos;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        StarredCheck.IsChecked = TodayFilterSettings.UseStarred;
        DueDateCheck.IsChecked = TodayFilterSettings.UseDueDate;
        DueBeforePicker.Date = TodayFilterSettings.DueBefore;

        await LoadTodayAsync();
    }

    private async Task LoadTodayAsync()
    {
		try
		{
            _todos.Clear();
            if (!TodayFilterSettings.UseStarred && !TodayFilterSettings.UseDueDate)
            {
                return;
            }

            var all = await _api.GetTodosAsync();
            IEnumerable<TodoResponse> filtered = all;

            if (TodayFilterSettings.UseStarred)
            {
                var starred = await _api.GetTodayTodosAsync();
                var starredIds = starred.Select(t => t.ID).ToHashSet();
                filtered = filtered.Where(t => starredIds.Contains(t.ID));
            }

            if (TodayFilterSettings.UseDueDate)
            {
                var limit = TodayFilterSettings.DueBefore.Date;
                filtered = filtered.Where(t => t.DueDate != null && t.DueDate.Value.ToLocalTime().Date <= limit);
            }

            var todos = filtered.OrderByDescending(t => t.Priority).ToList();

            foreach (var todo in todos)
                _todos.Add(new TodoViewModel(todo));
        }
		catch (Exception ex)
		{
            await DisplayAlert("오류", $"불러오기 실패: {ex.Message}", "확인");
        }
    }

    private async void OnFilterChanged(object sender, EventArgs e)
    {
        TodayFilterSettings.UseStarred = StarredCheck.IsChecked;
        TodayFilterSettings.UseDueDate = DueDateCheck.IsChecked;

        await LoadTodayAsync();
    }

    private async void OnDueBeforeChanged(object sender, EventArgs e)
    {
        TodayFilterSettings.DueBefore = DueBeforePicker.Date;
        await LoadTodayAsync();
    }
}