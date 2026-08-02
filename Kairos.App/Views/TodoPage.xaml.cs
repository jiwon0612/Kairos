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

            int priority = PriorityPicker.SelectedIndex switch
            {
                0 => 2, // 높음
                1 => 1, // 보통
                2 => 0, // 낮음
                _ => 1, // 기본값은 보통
            };

            try
            {
                await _api.CreateTodoAsync(ProjectId, title, priority);
                TitleEntry.Text = string.Empty;
                PriorityPicker.SelectedIndex = -1;
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

        private async void OnEditTodo(object sender, EventArgs e)
        {
            if (sender is Button item && item.BindingContext is TodoResponse todo)
            {
                var newTitle = await DisplayPromptAsync(
                    "할 일 수정",
                    "새 제목을 입력하세요",
                    initialValue: todo.Title);

                if (string.IsNullOrWhiteSpace(newTitle))
                    return;

                try
                {
                    await _api.UpdateTodoAsync(todo.ID, newTitle, todo.Priority);
                    await LoadTodoAsync();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("오류", $"수정 실패: {ex.Message}", "확인");
                }
            }
        }

        private async void OnDeleteTodo(object sender, EventArgs e)
        {
            if (sender is Button item && item.BindingContext is TodoResponse todo)
            {
                bool ok = await DisplayAlert(
                    "할 일 삭제",
                    $"'{todo.Title}'을(를) 삭제할까요?",
                    "삭제", "취소");

                if (!ok) return;

                try
                {
                    await _api.DeleteTodoAsync(todo.ID);
                    await LoadTodoAsync();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("오류", $"삭제 실패: {ex.Message}", "확인");
                }
            }
        }

        private async void OnChangePriority(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is TodoResponse todo)
            {
                string choice = await DisplayActionSheet(
                    "우선순위 변경",
                    "취소",
                    null,
                    "높음", "보통", "낮음");

                int priority = choice switch
                {
                    "높음" => 2,
                    "보통" => 1,
                    "낮음" => 0,
                    _ => -1
                };

                if (priority == -1 || priority == todo.Priority)
                    return;

                try
                {
                    await _api.UpdateTodoAsync(todo.ID, todo.Title, priority);
                    await LoadTodoAsync();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("오류", $"우선순위 변경 실패: {ex.Message}", "확인");
                }
            }
        }
    }
}