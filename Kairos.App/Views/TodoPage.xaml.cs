using Kairos.App.Services;
using Kairos.Shared.DTOs;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Kairos.App.Views
{
    [QueryProperty(nameof(ProjectId), "projectId")]
    public partial class TodoPage : ContentPage
    {
        private readonly ApiService _api = new();
        private readonly ObservableCollection<TodoResponse> _todos = new();

        public int ProjectId { get; set; }

        public TodoPage()
        {
            InitializeComponent();
            TodoList.ItemsSource = _todos;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadTodoAsync();
        }

        private async Task LoadTodoAsync()
        {
            try
            {
                var all = await _api.GetTodosAsync();
                _todos.Clear();
                foreach (var t in all.Where(t => t.ProjectID == ProjectId))
                    _todos.Add(t);

            }
            catch (Exception ex)
            {
                await DisplayAlert("오류", $"불러오기 실패: {ex.Message}", "확인");
            }
        }

        private async void OnAddClicked(object sender, EventArgs e)
        {
            var title = TitleEntry.Text;
            if (string.IsNullOrWhiteSpace(title))
                return;
            try
            {
                await _api.CreateTodoAsync(ProjectId, title);
                TitleEntry.Text = string.Empty;
                await LoadTodoAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("오류", $"추가 실패: {ex.Message}", "확인");
            }
        }

        private async void OnCheckChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox checkbox && checkbox.BindingContext is TodoResponse todo)
            {
                if (todo.IsCompleted == e.Value)
                    return;

                try
                {
                    await _api.SetCompletedAsync(todo.ID, e.Value);
                    todo.IsCompleted = e.Value;
                }
                catch (Exception ex)
                {
                    await DisplayAlert("오류", $"변경 실패: {ex.Message}", "확인");
                }
            }
        }
    }
}