using Kairos.App.Services;

namespace Kairos.App.Views;

public partial class EditDueDatePage : ContentPage
{
	private readonly ApiService _api = new();
	private readonly int _todoId;
	private readonly Func<Task> _onSaved;


	public EditDueDatePage(int todoId, DateTime? currentDue, bool hasDueTime, Func<Task> onSaved)
	{
		InitializeComponent();
		_todoId = todoId;
		_onSaved = onSaved;

		if (currentDue != null)
		{
			UseDueDateCheck.IsToggled = true;
			DueDateArea.IsVisible = true;

			var local = currentDue.Value.ToLocalTime();
			DuePicker.Date = local.Date;

			if (hasDueTime)
			{
				UseDueDateCheck.IsToggled = true;
				DueTimePicker.IsVisible = true;
				DueTimePicker.Time = local.TimeOfDay;

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

	private async void OnSaveClicked(object sender, EventArgs e)
	{
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
			await _api.SetDueDateAsync(_todoId, dueDate, hasDueTime);
			await _onSaved();
			await Navigation.PopModalAsync();
		}
		catch (Exception ex)
        {
            await DisplayAlert("오류", $"저장 실패: {ex.Message}", "확인");
        }
	}

	private async void OnCancelClicked(object sender, EventArgs e)
	{
		await Navigation.PopModalAsync();
	}
}