using Newtonsoft.Json;
using ProyectoBarberia.Models;
using System.Text;

namespace ProyectoBarberia.Views;

public partial class Register : ContentPage
{
    public Register()
    {
        InitializeComponent();
    }

    private async void OnSaveUserClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(EmailEntry.Text) ||
                string.IsNullOrWhiteSpace(UsernameEntry.Text) ||
                string.IsNullOrWhiteSpace(NameEntry.Text) ||
                string.IsNullOrWhiteSpace(LastnameEntry.Text) ||
                string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                await DisplayAlert("Error", "Todos los campos son obligatorios.", "OK");
                return;
            }

            var newUser = new
            {
                email = EmailEntry.Text,
                username = UsernameEntry.Text,
                password = PasswordEntry.Text,
                name = NameEntry.Text,
                lastname = LastnameEntry.Text,
                role_id = 4
            };

            var authToken = Preferences.Get("authToken", string.Empty);
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var response = await httpClient.PostAsync("http://127.0.0.1:3333/api/v1/users",
                new StringContent(JsonConvert.SerializeObject(newUser), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Éxito", "Usuario registrado correctamente.", "OK");
                await Shell.Current.GoToAsync("//LoginPage");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", $"No se pudo registrar el usuario: {errorContent}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al guardar el usuario: {ex.Message}", "OK");
        }
    }
}
