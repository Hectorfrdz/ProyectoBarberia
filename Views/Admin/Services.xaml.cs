using System.Collections.ObjectModel;
using Newtonsoft.Json;
using ProyectoBarberia.Models;
using ProyectoBarberia.Views.Cosas;

namespace ProyectoBarberia.Views.Admin;

public partial class Services : ContentPage
{
    private ObservableCollection<Service> ServiceList { get; set; } = new();

    public Services()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadServicesAsync();
    }

    private async Task LoadServicesAsync()
    {
        try
        {
            if (!Preferences.ContainsKey("authToken"))
            {
                Console.WriteLine("Error: No se encontró el token de autenticación en Preferences.");
                return;
            }

            var authToken = Preferences.Get("authToken", string.Empty);

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var response = await httpClient.GetStringAsync("http://127.0.0.1:3333/api/v1/services");

            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<Service>>(response);

            if (apiResponse != null && apiResponse.Status == "OK")
            {
                var allServices = apiResponse.Data;
                var activeServices = allServices.Where(service => service.Active);
                var inactiveServices = allServices.Where(service => !service.Active);
                var sortedServices = activeServices.Concat(inactiveServices).ToList();
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    ServiceList = new ObservableCollection<Service>(allServices);
                    ServicesCollectionView.ItemsSource = ServiceList;
                });
            }
            else
            {
                Console.WriteLine("Error: La respuesta de la API no es válida o no tiene estatus OK.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar servicios: {ex.Message}");
        }
    }


    private async void OnAddServiceClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AgregarServiciosPage());
    }
    private async void OnEditServiceClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Service service)
        {
            await Navigation.PushAsync(new AgregarServiciosPage(service));
        }
    }


    private async void OnToggleActiveClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Service service)
        {
            bool confirm = await DisplayAlert(
                service.Active ? "Desactivar Barbero" : "Activar Barbero",
                $"¿Estás seguro de que deseas {(service.Active ? "desactivar" : "activar")} a {service.Name}?",
                "Sí", "No");

            if (confirm)
            {
                try
                {
                    var authToken = Preferences.Get("authToken", string.Empty);

                    var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

                    var url = $"http://127.0.0.1:3333/api/v1/services/{service.Id}";
                    var response = await httpClient.DeleteAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Éxito", $"El Servicio ha sido {(service.Active ? "desactivado" : "activado")} correctamente.", "OK");
                        await LoadServicesAsync();
                    }
                    else
                    {
                        await DisplayAlert("Error", "Hubo un error al intentar actualizar el estado del Servicio.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al actualizar el estado del Servicio: {ex.Message}");
                    await DisplayAlert("Error", "Ocurrió un error al actualizar el estado del Servicio.", "OK");
                }
            }
        }
    }
}