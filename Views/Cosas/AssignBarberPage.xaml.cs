using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using ProyectoBarberia.Models;

namespace ProyectoBarberia.Views.Admin;

public partial class AssignBarberPage : ContentPage
{
    private Appointment CurrentAppointment;

    public ObservableCollection<Users> Barbers { get; set; }

    public AssignBarberPage(Appointment appointment)
    {
        InitializeComponent();
        CurrentAppointment = appointment;
        Barbers = new ObservableCollection<Users>();
        BindingContext = this;

        LoadAvailableBarbersAsync();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAvailableBarbersAsync();
    }

    public class ResponseModel
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public List<Schedules> Data { get; set; }
    }

    private async Task LoadAvailableBarbersAsync()
    {
        try
        {
            var authToken = Preferences.Get("authToken", string.Empty);

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Obtener los horarios de los barberos
            var response = await httpClient.GetStringAsync($"http://127.0.0.1:3333/api/v1/schedules");
            var responseData = JsonConvert.DeserializeObject<ResponseModel>(response);

            if (responseData == null || responseData.Data == null)
            {
                await DisplayAlert("Error", "No se pudieron cargar los horarios.", "OK");
                return;
            }

            // Obtener el número del día de la semana de la cita (1 para lunes, 2 para martes, etc.)
            var appointmentDay = (int)CurrentAppointment.AppointmentDate.DayOfWeek;
            if (appointmentDay == 8) appointmentDay = 1;

            // Filtrar los barberos disponibles para el día de la cita
            var availableBarbers = responseData.Data
                .Where(s => s.Active == true && s.Day == appointmentDay)
                .Select(s => s.User_Barber)
                .Distinct()
                .ToList();

            if (!availableBarbers.Any())
            {
                await DisplayAlert("Información", "No hay barberos disponibles para este día." + availableBarbers, "OK");
                return;
            }

            // Actualizar la lista de barberos
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Barbers.Clear();
                foreach (var barber in availableBarbers)
                {
                    Barbers.Add(barber);
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar los barberos: {ex.Message}");
            await DisplayAlert("Error", "No se pudieron cargar los barberos disponibles.", "OK");
        }
    }


    private async void OnAssignBarberClicked(object sender, EventArgs e)
    {
        if (BarberPicker.SelectedItem is Users selectedBarber)
        {
            try
            {
                // Asignar el barbero a la cita
                CurrentAppointment.UserBarber = selectedBarber;
                CurrentAppointment.Status = "ACCEPTED";

                var authToken = Preferences.Get("authToken", string.Empty);
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

                // Crear el objeto para enviar
                var updatedAppointment = new
                {
                    user_barber_id= selectedBarber.Id,
                    status = "ACEPTED"
                };

                // Enviar la actualización al servidor
                var content = new StringContent(JsonConvert.SerializeObject(updatedAppointment), System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PatchAsync($"http://127.0.0.1:3333/api/v1/appointments/{CurrentAppointment.Id}", content);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Éxito", $"Se asignó {selectedBarber.Name} a la cita.", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo actualizar la cita. Inténtalo nuevamente.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al asignar el barbero: {ex.Message}");
                await DisplayAlert("Error", "Ocurrió un error al asignar el barbero.", "OK");
            }
        }
        else
        {
            await DisplayAlert("Error", "Selecciona un barbero.", "OK");
        }
    }

}
