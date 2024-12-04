using Newtonsoft.Json;
using ProyectoBarberia.Models;
using ProyectoBarberia.Views.Cosas;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;

namespace ProyectoBarberia.Views.Clients;

public partial class Appointments : ContentPage
{
    public ObservableCollection<Appointment> AppointmentsList { get; set; }
    private Users Client { get; set; }

    public Appointments()
    {
        InitializeComponent();
        AppointmentsList = new ObservableCollection<Appointment>();
        AppointmentsCollectionView.ItemsSource = AppointmentsList;

        InitializeData();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        InitializeData();
    }

    private async void InitializeData()
    {
        await LoadUser();
        if (Client != null)
        {
            await LoadPendingAppointments();
        }
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

    public class ResponseModel2
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public List<Appointment> Data { get; set; }
    }

    private async Task LoadPendingAppointments()
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

            var response = await httpClient.GetStringAsync("http://127.0.0.1:3333/api/v1/appointments");

            var apiResponse = JsonConvert.DeserializeObject<ResponseModel2>(response);

            if (apiResponse != null && apiResponse.Status == "OK")
            {
                var allAppointments = apiResponse.Data;

                var userAppointments = allAppointments
                    .Where(appointment =>
                        appointment.UserClient != null &&
                        appointment.UserClient.Id == Client.Id &&
                        appointment.Status == "ACEPTED" || appointment.Status == "PENDING");

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    AppointmentsList.Clear();
                    foreach (var appointment in userAppointments)
                    {
                        AppointmentsList.Add(appointment);
                    }
                });
            }
            else
            {
                await DisplayAlert("Error", "No se pudieron cargar las citas pendientes.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al cargar las citas: {ex.Message}", "OK");
        }
    }
    private async void OnCreateAppointmentClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CreateAppointmentPage());
    }

    private async void OnCancelAppointmentClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var appointment = (Appointment)button.BindingContext;

        var appointmentSend = new
        {
            user_customer_id = appointment.UserClient.Id,
            service_id = appointment.Service.Id,
            appointment_date = appointment.AppointmentDate.ToString("yyyy-MM-dd HH:mm:ss"),
            status = "CANCELED"
        };

        var confirm = await DisplayAlert("Confirmar", "¿Estás seguro de que deseas cancelar esta cita?", "Sí", "No");

        if (!confirm)
            return;

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


            var content = new StringContent(JsonConvert.SerializeObject(appointmentSend), Encoding.UTF8, "application/json");

            var response = await httpClient.PatchAsync($"http://127.0.0.1:3333/api/v1/appointments/{appointment.Id}", content);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Éxito", "La cita ha sido cancelada.", "OK");
                await LoadPendingAppointments();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = JsonConvert.DeserializeObject<dynamic>(errorContent)?.message ?? "Error desconocido";
                await DisplayAlert("Error", $"No se pudo cancelar la cita: {errorContent}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al cancelar la cita: {ex.Message}", "OK");
        }
    }



}
