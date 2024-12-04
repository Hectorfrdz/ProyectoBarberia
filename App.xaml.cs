using ProyectoBarberia.Views;

namespace ProyectoBarberia
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
