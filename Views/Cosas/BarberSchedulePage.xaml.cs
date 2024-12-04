using System.Collections.ObjectModel;
using Newtonsoft.Json;
using ProyectoBarberia.Models;

namespace ProyectoBarberia.Views.Cosas;

public partial class BarberSchedulePage : ContentPage
{
    private Users Barber { get; set; }
    private ObservableCollection<Schedules> SchedulesList { get; set; } = new();

    public BarberSchedulePage(Users barber)
    {
        InitializeComponent();
        Barber = barber;

        Title = $"Horarios de {Barber.Name} {Barber.Lastname}";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadScheduleAsync();
    }

    private async Task LoadScheduleAsync()
    {
        try
        {
            var authToken = Preferences.Get("authToken", string.Empty);

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var response = await httpClient.GetStringAsync($"http://127.0.0.1:3333/api/v1/schedules");

            var responseData = JsonConvert.DeserializeObject<ResponseModel>(response);

            var schedules = responseData.Data
                .Where(s => s.User_Barber.Id == Barber.Id)
                .Where(s => s.Active == true)
                .ToList();

            if (schedules == null || !schedules.Any())
            {
                Console.WriteLine("No se encontraron horarios.");
                await DisplayAlert("Información", "No hay horarios disponibles para mostrar.", "OK");
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (schedules.Any())
                {
                    SchedulesList = new ObservableCollection<Schedules>(schedules);
                    ScheduleListView.ItemsSource = SchedulesList;
                }
                else
                {
                    Console.WriteLine("No se encontraron horarios para el barbero.");
                }
            });


        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar los horarios: {ex.Message}");
            await DisplayAlert("Error", "No se pudieron cargar los horarios.", "OK");
        }
    }


    private async void OnAddScheduleClicked(object sender, EventArgs e)
    {
        var addSchedulePage = new AddSchedulePage(Barber);

        await Navigation.PushAsync(addSchedulePage);
    }

    private async void OnEditScheduleClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Schedules service)
        {
            await Navigation.PushAsync(new AddSchedulePage(Barber, service));
        }
        else
        {
            await DisplayAlert("Error", "No se pudo obtener la información del horario.", "OK");
        }
    }

    private async void OnToggleActiveClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Schedules service)
        {
            bool confirm = await DisplayAlert(
                service.Active ? "Desactivar Barbero" : "Activar Barbero",
                $"¿Estás seguro de que deseas {(service.Active ? "desactivar" : "activar")} el dia: {service.Day}?",
                "Sí", "No");

            if (confirm)
            {
                try
                {
                    var authToken = Preferences.Get("authToken", string.Empty);

                    var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);


                    var url = $"http://127.0.0.1:3333/api/v1/schedules/{service.Id}";
                    var response = await httpClient.DeleteAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Éxito", $"El Horario ha sido {(service.Active ? "desactivado" : "activado")} correctamente.", "OK");
                        await LoadScheduleAsync();
                    }
                    else
                    {
                        await DisplayAlert("Error", "Hubo un error al intentar actualizar el estado del Horario.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al actualizar el estado del Horario: {ex.Message}");
                    await DisplayAlert("Error", "Ocurrió un error al actualizar el estado del Horario.", "OK");
                }
            }
        }
    }


    public class ResponseModel
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public List<Schedules> Data { get; set; }
    }

}
