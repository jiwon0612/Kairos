namespace Kairos.App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("TodoPage", typeof(Views.TodoPage));
        }
    }
}
