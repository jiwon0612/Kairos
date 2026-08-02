using Kairos.App.Services;
using Kairos.Shared.DTOs;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Kairos.App.Views;

public partial class TodayPage : ContentPage
{
	private readonly ApiService _api = new();
	private readonly ObservableCollection<TodoResponse> _todos = new();

    public TodayPage()
	{
		InitializeComponent();
		TodoList.ItemsSource = _todos;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadTodayAsync();
    }

    private async Task LoadTodayAsync()
    {
		try
		{
            var todos = await _api.GetTodayTodosAsync();
            _todos.Clear();
            foreach (var todo in todos)
                _todos.Add(todo);
        }
		catch (Exception ex)
		{
            await DisplayAlert("오류", $"불러오기 실패: {ex.Message}", "확인");
        }
    }
}