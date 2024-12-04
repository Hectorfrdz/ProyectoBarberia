using System.Net.Http.Headers;
using System.Windows.Input;
using Newtonsoft.Json.Linq;

namespace ProyectoBarberia
{
    public partial class AppShell : Shell
    {
        private readonly HttpClient _httpClient;

        public AppShell()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            this.Navigated += OnNavigated;
        }

        private void OnNavigated(object sender, ShellNavigatedEventArgs e)
        {
            var currentRoute = e.Current.Location.OriginalString;

            if (currentRoute.Contains("LoginPage") || currentRoute.Contains("RegisterPage"))
            {
                if (ToolbarItems.Contains(LogoutToolbarItem))
                {
                    ToolbarItems.Remove(LogoutToolbarItem);
                }
            }
            else
            {
                if (!ToolbarItems.Contains(LogoutToolbarItem))
                {
                    ToolbarItems.Add(LogoutToolbarItem);
                }
            }
        }


        public async Task NavigateToRolePage(string token)
        {
            // Simulación: Decodificar el token para obtener el rol.
            string role = await GetRoleFromToken(token);

            // Navegar según el rol.
            switch (role)
            {
                case "Cliente":
                    await GoToAsync("//Client");
                    break;
                case "Barbero":
                    await GoToAsync("//Barber");
                    break;
                case "Admin":
                    await GoToAsync("//Admin");
                    break;
                default:
                    await GoToAsync("//LoginPage");
                    break;
            }
        }

        private Task<string> GetRoleFromToken(string token)
        {
            // Aquí decodifica el token y obtiene el rol.
            // Por ejemplo, usa una librería JWT para extraer el rol.
            // Simulación: devolver rol basado en token.
            return Task.FromResult(token == "adminToken" ? "Admin" :
                                   token == "barberToken" ? "Barbero" : "Cliente");
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            try
            {
                // Lógica para cerrar sesión
                string token = Preferences.Get("authToken", null);

                if (!string.IsNullOrEmpty(token))
                {
                    var url = "http://127.0.0.1:3333/api/auth/logout";
                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await client.PostAsync(url, new StringContent(""));

                    if (response.IsSuccessStatusCode)
                    {
                        Preferences.Remove("authToken");
                        await GoToAsync("//LoginPage");
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudo cerrar la sesión.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cerrar sesión: {ex.Message}", "OK");
            }
        }

    }
}
