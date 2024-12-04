using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace ProyectoBarberia.Views;

public partial class Login : ContentPage
{
    private readonly HttpClient _httpClient;

    public Login()
    {
        InitializeComponent();
        _httpClient = new HttpClient();
    }

    private async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text;
        string password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Error", "Por favor, ingrese su correo y contraseña.", "OK");
            return;
        }

        // Llamar al endpoint de inicio de sesión
        string token = await LoginAsync(email, password);
        if (!string.IsNullOrEmpty(token))
        {
            // Obtener el rol del usuario
            string role = await GetRoleAsync(token);
            if (!string.IsNullOrEmpty(role))
            {
                // Navegar según el rol
                await NavigateToRolePage(role);
            }
            else
            {
                await DisplayAlert("Error", "No se pudo determinar el rol del usuario.", "OK");
            }
        }
        else
        {
            await DisplayAlert("Error", "Inicio de sesión fallido. Verifique sus credenciales.", "OK");
        }
    }

    private async Task<string> LoginAsync(string email, string password)
    {
        try
        {
            var url = "http://127.0.0.1:3333/api/auth/login";
            var payload = new
            {
                email,
                password
            };

            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(payload), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(responseString);

                if (jsonResponse["status"]?.ToString() == "OK")
                {
                    // Extraer el token de la respuesta
                    Preferences.Set("authToken", jsonResponse["data"]?["token"]?.ToString());
                    return jsonResponse["data"]?["token"]?.ToString();
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al comunicarse con el servidor: {ex.Message}", "OK");
        }

        return null;
    }

    private async Task<string> GetRoleAsync(string token)
    {
        try
        {
            var url = "http://127.0.0.1:3333/api/auth/me";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(responseString);

                if (jsonResponse["status"]?.ToString() == "OK")
                {
                    // Extraer el rol de la respuesta
                    return jsonResponse["data"]?["role"]?["name"]?.ToString();
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al obtener el rol del usuario: {ex.Message}", "OK");
        }

        return null;
    }

    private async Task NavigateToRolePage(string role)
    {
        switch (role)
        {
            case "ADMIN":
                await Shell.Current.GoToAsync("//Admin");
                break;
            case "BARBER":
                await Shell.Current.GoToAsync("//Barber");
                break;
            case "CLIENT":
                await Shell.Current.GoToAsync("//Client");
                break;
            default:
                await DisplayAlert("Error", "Rol no reconocido.", "OK");
                break;
        }
    }
    private async Task<bool> IsTokenValidAsync()
    {
        string token = Preferences.Get("authToken", null);

        if (string.IsNullOrEmpty(token))
        {
            return false; 
        }

        try
        {
            var url = "http://127.0.0.1:3333/api/auth/me";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        bool isTokenValid = await IsTokenValidAsync();

        if (isTokenValid)
        {
            // Token válido, navegar a la página según el rol
            string token = Preferences.Get("authToken", null);
            string role = await GetRoleAsync(token);
            await NavigateToRolePage(role);
        }
        else
        {
            // Token inválido, mostrar pantalla de inicio de sesión
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }

    private async void OnCrearCuentaTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Register());

    }


}
