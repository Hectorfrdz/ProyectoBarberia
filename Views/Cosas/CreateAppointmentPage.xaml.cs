using Newtonsoft.Json;
using System.Net.Http.Headers;
using ProyectoBarberia.Models;
using Microsoft.Maui.ApplicationModel.Communication;
using System.Net.Http.Json;
using System.Xml.Linq;
using System.Collections.ObjectModel;

namespace ProyectoBarberia.Views.Clients;

public partial class CreateAppointmentPage : ContentPage
{
    private Users Client { get; set; }

    public CreateAppointmentPage()
    {
        InitializeComponent();
        _ = LoadUser();
        _ = LoadServices();

    }
    public class ResponseModel
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public Users Data { get; set; }
    }

    private async Task LoadUser()
    {
        try
        {
            var authToken = Preferences.Get("authToken", string.Empty);
            if (string.IsNullOrEmpty(authToken))
            {
                await DisplayAlert("Error", "No se encontró el token de autenticación.", "OK");
                return;
            }

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authToken);

            var response = await httpClient.GetStringAsync("http://127.0.0.1:3333/api/auth/me");
            var responseData = JsonConvert.DeserializeObject<ResponseModel>(response);

            if (responseData != null && responseData.Status == "OK")
            {
                Client = responseData.Data;
            }
            else
            {
                await DisplayAlert("Error", "No se pudo cargar el usuario logueado.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al cargar el usuario: {ex.Message}", "OK");
        }
    }

    private async void OnCreateButtonClicked(object sender, EventArgs e)
    {
        var appointmentDate = AppointmentDatePicker.Date.ToString("yyyy-MM-dd HH:mm:ss");
        var selectedService = ServicePicker.SelectedItem as Service;

        if (string.IsNullOrEmpty(appointmentDate) || selectedService == null)
        {
            await DisplayAlert("Error", "Por favor, completa todos los campos.", "OK");
            return;
        }

        try
        {
            var httpClient = new HttpClient();

            if (!Preferences.ContainsKey("authToken"))
            {
                await DisplayAlert("Error", "No se encontró el token de autenticación.", "OK");
                return;
            }

            var authToken = Preferences.Get("authToken", string.Empty);
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authToken);

            var appointment = new
            {
                user_customer_id = Client.Id,
                service_id = selectedService.Id,
                appointment_date = appointmentDate,
                status = "PENDING",
            };

            var response = await httpClient.PostAsJsonAsync("http://127.0.0.1:3333/api/v1/appointments", appointment);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Éxito", "Cita creada correctamente.", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = JsonConvert.DeserializeObject<dynamic>(errorContent)?.message ?? "Error desconocido";
                await DisplayAlert("Error", $"No se pudo crear la cita: {errorContent}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al crear la cita: {ex.Message}", "OK");
        }
    }


    private async Task LoadServices()
    {
        try
        {
            var authToken = Preferences.Get("authToken", string.Empty);
            if (string.IsNullOrEmpty(authToken))
            {
                await DisplayAlert("Error", "No se encontró el token de autenticación.", "OK");
                return;
            }

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authToken);

            var response = await httpClient.GetStringAsync("http://127.0.0.1:3333/api/v1/services");

            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<Service>>(response);
            if (apiResponse != null && apiResponse.Status == "OK")
            {
                var allServices = apiResponse.Data;
                var activeServices = allServices.Where(service => service.Active);
                ServicePicker.ItemsSource = new ObservableCollection<Service>(activeServices);
                ServicePicker.ItemDisplayBinding = new Binding("Name");
            }
            else
            {
                await DisplayAlert("Error", "No se pudo cargar la lista de servicios.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al cargar los servicios: {ex.Message}", "OK");
        }
    }


}
