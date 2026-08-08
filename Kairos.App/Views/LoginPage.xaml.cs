using Kairos.App.Services;

namespace Kairos.App.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly ApiService _api = new();
        private readonly GoogleAuthService _googleAuthService = new();
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text;
            var password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("오류", "이메일과 비밀번호를 입력해주세요.", "확인");
                return;
            }

            try
            {
                var success = await _api.LoginAsync(email, password);
                if (success)
                {
                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    await DisplayAlert("로그인 실패", "이메일 또는 비밀번호가 올바르지 않습니다.", "확인");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("오류", $"로그인 중 오류 발생: {ex.Message}", "확인");
            }
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text;
            var password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("오류", "이메일과 비밀번호를 입력해주세요.", "확인");
                return;
            }

            try
            {
                var succes = await _api.RegisterAsync(email, password);
                if (succes)
                {
                    await DisplayAlert("회원가입 성공", "회원가입이 완료되었습니다. 로그인해주세요.", "확인");
                }
                else
                {
                    await DisplayAlert("회원가입 실패", "회원가입에 실패했습니다. 다시 시도해주세요.", "확인");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("오류", $"회원가입 중 오류 발생: {ex.Message}", "확인");
            }
        }

        private async void OnGoogleLoginClicked(object sender, EventArgs e)
        {
            try
            {
                var idToken = await _googleAuthService.SignInAsync();
                if (string.IsNullOrEmpty(idToken))
                {
                    await DisplayAlert("로그인 실패", "Google 로그인에 실패했습니다.", "확인");
                    return;
                }

                var success = await _api.GoogleLoginAsync(idToken);
                if (success)
                    await Shell.Current.GoToAsync("//MainPage");
                else
                    await DisplayAlert("로그인 실패", "Google 로그인에 실패했습니다.", "확인");
            }
            catch (Exception ex)
            {
                await DisplayAlert("오류", ex.Message, "확인");
            }
        }
    }
}
