using Kairos.App.Services;
using Kairos.App.ViewModels;
using Kairos.Shared.DTOs;
using System.Collections.ObjectModel;

namespace Kairos.App.Views
{
    [QueryProperty(nameof(ProjectId), "projectId")]
    public partial class TodoPage : ContentPage
    {
        private readonly ApiService _api = new();
        private readonly ObservableCollection<TodoViewModel> _todos = new();

        public int ProjectId { get; set; }

        private int _selectedPriority = 1;

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
                    _todos.Add(new TodoViewModel(t));

            }
            catch (Exception ex)
            {
                await DisplayAlert("오류", $"불러오기 실패: {ex.Message}", "확인");
            }
        }

        private void OnSelectPriority(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is string p)
            {
                _selectedPriority = int.Parse(p);

                UpdatePriorityButtons();
            }
        }

        private void UpdatePriorityButtons()
        {
            // 다 회색으로
            PriHigh.BackgroundColor = Color.FromArgb("#1A1C21");
            PriHigh.TextColor = Color.FromArgb("#8894A5");
            PriMid.BackgroundColor = Color.FromArgb("#1A1C21");
            PriMid.TextColor = Color.FromArgb("#8894A5");
            PriLow.BackgroundColor = Color.FromArgb("#1A1C21");
            PriLow.TextColor = Color.FromArgb("#8894A5");

            // 선택된 것만 민트
            Button selected = _selectedPriority switch
            {
                2 => PriHigh,
                1 => PriMid,
                0 => PriLow,
                _ => PriMid
            };
            selected.BackgroundColor = Color.FromArgb("#5EEAD4");
            selected.TextColor = Color.FromArgb("#16181D");
            selected.FontAttributes = FontAttributes.Bold;
        }

        private async void OnAddClicked(object sender, EventArgs e)
        {
            var title = TitleEntry.Text;
            if (string.IsNullOrWhiteSpace(title))
                return;

            int priority = _selectedPriority;

            DateTime? dueDate = null;
            bool hasDueTime = false;

            if (UseDueDateCheck.IsToggled)
            {
                var date = DuePicker.Date;
                if (UseDueTimeCheck.IsToggled)
                {
                    var time = DueTimePicker.Time;
                    var local = date.Date + time;
                    dueDate = local.ToUniversalTime();
                    hasDueTime = true;
                }
                else
                {
                    dueDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Local).ToUniversalTime();
                    hasDueTime = false;
                }
            }

            try
            {
                await _api.CreateTodoAsync(ProjectId, title, priority, dueDate, hasDueTime);
                TitleEntry.Text = string.Empty;
                
                _selectedPriority = 1;
                UpdatePriorityButtons();

                UseDueDateCheck.IsToggled = false;
                UseDueTimeCheck.IsToggled = false;

                await LoadTodoAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("오류", $"추가 실패: {ex.Message}", "확인");
            }
        }

        private async void OnCheckChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox checkbox && checkbox.BindingContext is TodoViewModel todo)
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
            if (sender is Button item && item.BindingContext is TodoViewModel todo)
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
            if (sender is Button item && item.BindingContext is TodoViewModel todo)
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
            if (sender is Element btn && btn.BindingContext is TodoViewModel todo)
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

        private void OnUseDueDateChanged(object sender, ToggledEventArgs e)
        {
            DueDateArea.IsVisible = e.Value;
        }

        private void OnUseDueTimeChanged(object sender, ToggledEventArgs e)
        {
            DueTimePicker.IsVisible = e.Value;
        }
    }
}