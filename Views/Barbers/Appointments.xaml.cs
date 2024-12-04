using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using ProyectoBarberia.Models;

namespace ProyectoBarberia.Views.Barbers;

public partial class Appointments : ContentPage
{
    public ObservableCollection<Appointment> AppointmentsList { get; set; }
    private Users Barber { get; set; }

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
        if (Barber != null)
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
                Barber = responseData.Data;
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
                        appointment.UserBarber != null &&
                        appointment.UserBarber.Id == Barber.Id &&
                        appointment.Status == "ACEPTED");

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

    private async void OnAceptAppointmentClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var appointment = (Appointment)button.BindingContext;

        var appointmentSend = new
        {
            status = "COMPLETED"
        };

        var confirm = await DisplayAlert("Confirmar", "¿Estás seguro de que deseas completar esta cita?", "Sí", "No");

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
                await DisplayAlert("Éxito", "La cita ha sido completada.", "OK");
                await LoadPendingAppointments();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = JsonConvert.DeserializeObject<dynamic>(errorContent)?.message ?? "Error desconocido";
                await DisplayAlert("Error", $"No se pudo completar la cita: {errorContent}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al completar la cita: {ex.Message}", "OK");
        }
    }
}