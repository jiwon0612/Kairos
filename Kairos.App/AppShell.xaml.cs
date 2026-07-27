using Kairos.App.Services;
using System.Threading.Tasks;

namespace Kairos.App
{
    public partial class AppShell : Shell
    {
        private readonly ApiService _api = new();

        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("TodoPage", typeof(Views.TodoPage));

            CheckLoginState();
        }

        private async Task CheckLoginState()
        {
            if (await _api.IsLoggedInAsync())
            {
                await GoToAsync("//MainPage");
            }
        }
    }
}
