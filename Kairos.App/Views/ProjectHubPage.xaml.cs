using Kairos.App.Services;
using Kairos.App.ViewModels;
using Kairos.Shared.DTOs;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Kairos.App.Views;

public partial class ProjectHubPage : ContentPage
{
	private readonly ApiService _api = new();
	private readonly ObservableCollection<ProjectItemViewModel> _projects = new();

	private readonly ObservableCollection<TodoViewModel> _todos = new();

	private int _currentProjectId;
	private int _selectedPriority = 1;

	public ObservableCollection<ProjectItemViewModel> Projects => _projects;
	public ObservableCollection<TodoViewModel> Todos => _todos;

	public ProjectHubPage()
	{
		InitializeComponent();
		BindingContext = this;
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
            ProjectLoading.IsVisible = true;
            ProjectLoading.IsRunning = true;

            var projects = await _api.GetProjectsAsync();
			_projects.Clear();
			foreach (var p in projects)
				_projects.Add(new ProjectItemViewModel(p));
		}
		catch (Exception ex)
		{
			await DisplayAlert("오류", $"프로젝트 불러오기 실패: {ex.Message}", "확인");
		}
        finally
        {
            ProjectLoading.IsVisible = false;
            ProjectLoading.IsRunning = false;
        }
    }

	private async void OnProjectTapped(object sender, EventArgs e)
	{
		if (sender is Element el && el.BindingContext is ProjectItemViewModel project)
		{
			ListView.IsVisible = false;
			DetailView.IsVisible = true;
			await SelectProjectAsync(project);
		}
	}

	private async void OnSidebarProjectTapped(object sender, EventArgs e)
	{
		if (sender is Element el && el.BindingContext is ProjectItemViewModel project)
		{
			await SelectProjectAsync(project);
		}
	}

	private async Task SelectProjectAsync(ProjectItemViewModel project)
	{
		_currentProjectId = project.ID;
		DetailTitle.Text = project.Name;
		foreach (var p in _projects)
			p.IsSelected = false;
		project.IsSelected = true;
		await LoadTodosAsync();
	}

	private async Task LoadTodosAsync()
	{
		try
		{
            TodoLoading.IsVisible = true;
            TodoLoading.IsRunning = true;

            var all = await _api.GetTodosAsync();
			_todos.Clear();
			foreach (var t in all
				.Where(t => t.ProjectID == _currentProjectId)
				.OrderBy(t => t.IsCompleted)
				.ThenByDescending(t => t.Priority))
				_todos.Add(new TodoViewModel(t));
		}
		catch (Exception ex)
		{
			await DisplayAlert("오류", $"할 일 불러오기 실패: {ex.Message}", "확인");
		}
        finally
        {
            TodoLoading.IsVisible = false;
            TodoLoading.IsRunning = false;
        }
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
				await LoadTodosAsync();
			}
			catch (Exception ex)
			{
				await DisplayAlert("오류", $"변경 실패: {ex.Message}", "확인");
			}
		}
	}

	private void OnBackTapped(object sender, EventArgs e)
	{
		DetailView.IsVisible = false;
		ListView.IsVisible = true;
	}

	private async void OnAddProjectTapped(object sender, EventArgs e)
	{
		string name = await DisplayPromptAsync("새 프로젝트", "이름을 입력하세요");
		if (string.IsNullOrWhiteSpace(name))
			return;
		try
		{
			await _api.CreateProjectAsync(name);
			await LoadProjectsAsync();
		}
		catch (Exception ex)
		{
			await DisplayAlert("오류", $"추가 실패: {ex.Message}", "확인");
		}
	}

	private async void OnToggleToday(object sender, EventArgs e)
	{
		if (sender is Button btn && btn.BindingContext is ProjectItemViewModel project)
		{
			try
			{
				await _api.SetProjectTodayAsync(project.ID, !project.IsToday);
				await LoadProjectsAsync();
			}
			catch (Exception ex)
			{
				await DisplayAlert("오류", $"변경 실패: {ex.Message}", "확인");
			}
		}
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
				var local = date.Date + DueTimePicker.Time;
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
			await _api.CreateTodoAsync(_currentProjectId, title, priority, dueDate, hasDueTime);
			TitleEntry.Text = string.Empty;
			_selectedPriority = 1;
            UpdatePriorityButtons();
			UseDueDateCheck.IsToggled = false;
			UseDueTimeCheck.IsToggled = false;
			await LoadTodosAsync();
		}
		catch (Exception ex)
		{
            await DisplayAlert("오류", $"추가 실패: {ex.Message}", "확인");
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
        PriHigh.BackgroundColor = Color.FromArgb("#1A1C21");
        PriHigh.TextColor = Color.FromArgb("#8894A5");
        PriMid.BackgroundColor = Color.FromArgb("#1A1C21");
        PriMid.TextColor = Color.FromArgb("#8894A5");
        PriLow.BackgroundColor = Color.FromArgb("#1A1C21");
        PriLow.TextColor = Color.FromArgb("#8894A5");

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

	private void OnUseDueDateChanged(object sender, ToggledEventArgs e)
	{
		DueDateArea.IsVisible = e.Value;
	}

	private void OnUseDueTimeChanged(object sender, ToggledEventArgs e)
	{
		DueTimePicker.IsVisible = e.Value;
	}

	private async void OnChangePriority(object sender, EventArgs e)
	{
		if (sender is Element el && el.BindingContext is TodoViewModel todo)
		{
			string choice = await DisplayActionSheet("우선순위 변경", "취소", null, "높음", "보통", "낮음");
			int priority = choice switch { "높음" => 2, "보통" => 1, "낮음" => 0, _ => -1 };
			if (priority == -1 || priority == todo.Priority) return;
			try
			{
				await _api.UpdateTodoAsync(todo.ID, todo.Title, priority);
				await LoadTodosAsync();
			}
			catch (Exception ex)
			{
				await DisplayAlert("오류", $"변경 실패: {ex.Message}", "확인");
			}
		}
	}

	private async void OnEditTodo(object sender, EventArgs e)
	{
		if (sender is Button btn && btn.BindingContext is TodoViewModel todo)
		{
			string newTitle = await DisplayPromptAsync("할 일 수정", "새 제목", initialValue: todo.Title);
			if (string.IsNullOrWhiteSpace(newTitle)) return;
			try
			{
				await _api.UpdateTodoAsync(todo.ID, newTitle, todo.Priority);
				await LoadTodosAsync();
			}
			catch (Exception ex)
			{
                await DisplayAlert("오류", $"수정 실패: {ex.Message}", "확인");
            }
		}
	}

	private async void OnDeleteTodo(object sender, EventArgs e)
	{
		if (sender is Button btn && btn.BindingContext is TodoViewModel todo)
		{
			bool ok = await DisplayAlert("삭제", $"'{todo.Title}'을(를) 삭제할까요?", "삭제", "취소");
			if (!ok) return;
			try
			{
				await _api.DeleteTodoAsync(todo.ID);
				await LoadTodosAsync();
			}
			catch (Exception ex)
			{
                await DisplayAlert("오류", $"삭제 실패: {ex.Message}", "확인");
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

	private async void OnEditDueDate(object sender, EventArgs e)
	{
		if (sender is Element el && el.BindingContext is TodoViewModel todo)
		{
			var page = new EditDueDatePage(
				todo.ID,
				todo.DueDate,
				todo.HasDueTime,
				LoadTodosAsync);
			await Navigation.PushModalAsync(page);
		}
	}
}