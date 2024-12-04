using Newtonsoft.Json;
using ProyectoBarberia.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ProyectoBarberia.Views.Clients;

public partial class Historial : ContentPage
{
    public ObservableCollection<Appointment> AppointmentsList { get; set; }
    private Users Client { get; set; }

    public Historial()
    {
        InitializeComponent();
        AppointmentsList = new ObservableCollection<Appointment>();
        BindingContext = this;
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
            await LoadAppointmentsHistory();
        }
    }
    public class ResponseModel
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public Users Data { get; set; } // Cambiar List<Users> a Users
    }

    public class ResponseModel2
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public List<Appointment> Data { get; set; }
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

    private async Task LoadAppointmentsHistory()
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

                var filteredAppointments = allAppointments
                    .Where(appointment =>
                        appointment.UserClient.Id == Client.Id && (appointment.Status == "COMPLETED" || appointment.Status == "CANCELED"));

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    AppointmentsList.Clear();
                    foreach (var appointment in filteredAppointments)
                    {
                        AppointmentsList.Add(appointment);
                    }
                });
            }
            else
            {
                await DisplayAlert("Error", "No se pudieron cargar las citas.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al cargar las citas: {ex.Message}", "OK");
        }
    }
}
