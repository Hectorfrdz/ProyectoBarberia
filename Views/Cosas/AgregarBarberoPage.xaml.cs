using System.Net.Http.Json;
using Newtonsoft.Json;

namespace ProyectoBarberia.Views.Admin;

public partial class AgregarBarberoPage : ContentPage
{
    public AgregarBarberoPage()
    {
        InitializeComponent();
    }

    private async void OnSaveBarberClicked(object sender, EventArgs e)
    {
        string name = NameEntry.Text;
        string lastName = LastNameEntry.Text;
        string email = EmailEntry.Text;
        string username = $"{name.ToLower()}.{lastName.ToLower()}";
        string password = name.ToLower()+".pass123";

        // Validar campos obligatorios
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlert("Error", "Por favor completa todos los campos obligatorios.", "OK");
            return;
        }

        try
        {
            // Crear el cliente HTTP
            var httpClient = new HttpClient();

            // Obtener el token de autenticación desde las preferencias
            if (!Preferences.ContainsKey("authToken"))
            {
                await DisplayAlert("Error", "No se encontró el token de autenticación.", "OK");
                return;
            }

            var authToken = Preferences.Get("authToken", string.Empty);
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Crear el objeto del barbero
            var barber = new
            {
                email = email,
                username = username,
                name = name,
                lastname = lastName,
                password = password,
                role_id = 3
            };

            // Enviar la solicitud POST
            var response = await httpClient.PostAsJsonAsync("http://127.0.0.1:3333/api/v1/users", barber);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Éxito", "Barbero agregado correctamente.", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                // Leer el error devuelto por la API
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = JsonConvert.DeserializeObject<dynamic>(errorContent)?.message ?? "Error desconocido";
                await DisplayAlert("Error", $"No se pudo agregar el barbero: {errorMessage}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al agregar el barbero: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync(); // Cierra la página actual
    }
}
