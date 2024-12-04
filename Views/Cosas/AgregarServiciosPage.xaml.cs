using System.Net.Http.Json;
using Newtonsoft.Json;
using ProyectoBarberia.Models;

namespace ProyectoBarberia.Views.Cosas;

public partial class AgregarServiciosPage : ContentPage
{
    private readonly Service _serviceToEdit;
    private readonly bool _isEditing;

    public AgregarServiciosPage(Service service = null)
    {
        InitializeComponent();

        _serviceToEdit = service;
        _isEditing = service != null;

        if (_isEditing)
        {
            Title = "Editar Servicio";
            PopulateFields();
        }
        else
        {
            Title = "Agregar Servicio";
        }
    }

    private void PopulateFields()
    {
        if (_serviceToEdit != null)
        {
            NameEntry.Text = _serviceToEdit.Name;
            DescriptionEditor.Text = _serviceToEdit.Description;
            PriceEntry.Text = _serviceToEdit.Price.ToString("F2");
        }
    }

    private async void OnSaveServiceClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NameEntry.Text))
            {
                await DisplayAlert("Error", "El nombre del servicio es obligatorio.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(PriceEntry.Text) || !decimal.TryParse(PriceEntry.Text, out decimal price))
            {
                await DisplayAlert("Error", "Ingrese un precio válido.", "OK");
                return;
            }

            var httpClient = new HttpClient();

            if (!Preferences.ContainsKey("authToken"))
            {
                await DisplayAlert("Error", "No se encontró el token de autenticación.", "OK");
                return;
            }

            var authToken = Preferences.Get("authToken", string.Empty);
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            if (_isEditing)
            {
                // Actualización de un servicio existente
                var updatedService = new
                {
                    Name = NameEntry.Text,
                    Description = DescriptionEditor.Text,
                    Price = price
                };

                var response = await httpClient.PatchAsJsonAsync($"http://127.0.0.1:3333/api/v1/services/{_serviceToEdit.Id}", updatedService);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Éxito", "Servicio actualizado correctamente.", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorMessage = JsonConvert.DeserializeObject<dynamic>(errorContent)?.message ?? "Error desconocido";
                    await DisplayAlert("Error", $"No se pudo actualizar el Servicio: {errorContent}", "OK");
                }
            }
            else
            {
                // Creación de un nuevo servicio
                var newService = new
                {
                    Name = NameEntry.Text,
                    Description = DescriptionEditor.Text,
                    Price = price
                };

                var response = await httpClient.PostAsJsonAsync("http://127.0.0.1:3333/api/v1/services", newService);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Éxito", "Servicio agregado correctamente.", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorMessage = JsonConvert.DeserializeObject<dynamic>(errorContent)?.message ?? "Error desconocido";
                    await DisplayAlert("Error", $"No se pudo agregar el Servicio: {errorContent}", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
        }
    }
}
