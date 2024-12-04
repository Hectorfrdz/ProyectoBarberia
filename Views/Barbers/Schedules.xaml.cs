using System.Collections.ObjectModel;
using Newtonsoft.Json;
using ProyectoBarberia.Models;

namespace ProyectoBarberia.Views.Barbers;

public partial class Schedules : ContentPage
{
    public ObservableCollection<Models.Schedules> SchedulesList { get; set; } = new();

    public Schedules()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSchedulesForLoggedInBarberAsync();
    }
    public class ResponseModel2
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public Users Data { get; set; }
    }

    private async Task LoadSchedulesForLoggedInBarberAsync()
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
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Obtener información del barbero logueado
            var userResponse = await httpClient.GetStringAsync("http://127.0.0.1:3333/api/auth/me");
            var responseData = JsonConvert.DeserializeObject<ResponseModel2>(userResponse);
            var barber = responseData.Data;

            if (barber == null)
            {
                await DisplayAlert("Error", "No se pudo cargar la información del barbero logueado.", "OK");
                return;
            }

            var scheduleResponse = await httpClient.GetStringAsync("http://127.0.0.1:3333/api/v1/schedules");
            var responseData2 = JsonConvert.DeserializeObject<ResponseModel>(scheduleResponse);

            if (responseData?.Data == null)
            {
                await DisplayAlert("Error", "No se pudieron cargar los horarios.", "OK");
                return;
            }


            // Filtrar los horarios del barbero logueado
            var schedules = responseData2.Data
                .Where(s => s.User_Barber.Id == barber.Id)
                .Where(s => s.Active == true)
                .ToList();

            // Actualizar la lista de horarios
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                SchedulesList.Clear();
                foreach (var schedule in schedules)
                {
                    SchedulesList.Add(schedule);
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar los horarios: {ex.Message}");
            await DisplayAlert("Error", "Ocurrió un error al cargar los horarios.", "OK");
        }
    }

    public class ResponseModel
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public List<Models.Schedules> Data { get; set; }
    }
}
