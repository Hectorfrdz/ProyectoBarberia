using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using ProyectoBarberia.Models;

namespace ProyectoBarberia.Views.Admin;

public partial class Appointments : ContentPage
{
    public ObservableCollection<Appointment> AppointmentsList { get; set; }

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
        await LoadPendingAppointments();
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
                        appointment.Status == "PENDING");

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

    private async void OnAcceptAppointmentClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Appointment selectedAppointment)
        {
            await Navigation.PushAsync(new AssignBarberPage(selectedAppointment));
        }
    }


}