using Newtonsoft.Json;
using System.Text;
using ProyectoBarberia.Models;

namespace ProyectoBarberia.Views.Cosas;

public partial class AddSchedulePage : ContentPage
{
    private readonly Schedules _scheduleToEdit;
    private readonly bool _isEditing;
    private readonly Users _barber;
    private Dictionary<string, int> _dayNumberMapping;

    public AddSchedulePage(Users barber, Schedules schedule = null)
    {
        InitializeComponent();

        _barber = barber;
        _scheduleToEdit = schedule;
        _isEditing = schedule != null;

        Title = _isEditing ? "Editar Horario" : "Agregar Horario";

        if (_isEditing)
        {
            PopulateFields();
        }
    }


    private void PopulateFields()
    {
        if (_scheduleToEdit != null)
        {
            DayPicker.SelectedItem = _dayNumberMapping.FirstOrDefault(x => x.Value == _scheduleToEdit.Day).Key;
            StartTimePicker.Time = TimeSpan.Parse(_scheduleToEdit.StartTime);
            StartRestTimePicker.Time = TimeSpan.Parse(_scheduleToEdit.StartRestTime);
            EndRestTimePicker.Time = TimeSpan.Parse(_scheduleToEdit.EndRestTime);
            EndTimePicker.Time = TimeSpan.Parse(_scheduleToEdit.EndTime);
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAvailableDaysAsync();
    }

    private async Task LoadAvailableDaysAsync()
    {
        try
        {
            var authToken = Preferences.Get("authToken", string.Empty);

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var response = await httpClient.GetStringAsync("http://127.0.0.1:3333/api/v1/schedules");

            var responseData = JsonConvert.DeserializeObject<ResponseModel>(response);
            var schedules = responseData.Data.Where(s => s.User_Barber.Id == _barber.Id);
            var schedulesList = schedules.Where(s => s.Active == true).ToList();

            var occupiedDays = schedulesList.Select(s => s.Day).ToList();
            var allDays = new List<(string DayName, int DayNumber)>
            {
                ("Lunes", 1), ("Martes", 2), ("Miércoles", 3),
                ("Jueves", 4), ("Viernes", 5), ("Sábado", 6), ("Domingo", 7)
            };

            var availableDays = allDays
                .Where(day => !occupiedDays.Contains(day.DayNumber) || (_isEditing && day.DayNumber == _scheduleToEdit?.Day))
                .Select(day => day.DayName)
                .ToList();

            _dayNumberMapping = allDays.ToDictionary(day => day.DayName, day => day.DayNumber);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                DayPicker.ItemsSource = availableDays;
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar los horarios: {ex.Message}");
            await DisplayAlert("Error", "No se pudieron cargar los horarios.", "OK");
        }
    }

    private async void OnSaveScheduleClicked(object sender, EventArgs e)
    {
        try
        {
            var selectedDayName = DayPicker.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedDayName))
            {
                await DisplayAlert("Error", "Seleccione un día.", "OK");
                return;
            }

            var selectedDay = _dayNumberMapping[selectedDayName];
            var scheduleData = new
            {
                user_barber_id = _barber.Id,
                day = selectedDay,
                start_time = StartTimePicker.Time.ToString(@"hh\:mm\:ss"),
                end_time = EndTimePicker.Time.ToString(@"hh\:mm\:ss"),
                start_rest_time = StartRestTimePicker.Time.ToString(@"hh\:mm\:ss"),
                end_rest_time = EndRestTimePicker.Time.ToString(@"hh\:mm\:ss")
            };


            var authToken = Preferences.Get("authToken", string.Empty);
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            HttpResponseMessage response;

            if (_isEditing)
            {
                response = await httpClient.PatchAsync($"http://127.0.0.1:3333/api/v1/schedules/{_scheduleToEdit.Id}",
                    new StringContent(JsonConvert.SerializeObject(scheduleData), Encoding.UTF8, "application/json"));
            }
            else
            {
                response = await httpClient.PostAsync("http://127.0.0.1:3333/api/v1/schedules",
                    new StringContent(JsonConvert.SerializeObject(scheduleData), Encoding.UTF8, "application/json"));
            }


            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Éxito", _isEditing ? "Horario actualizado correctamente." : "Horario agregado correctamente.", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Error", $"No se pudo guardar el horario. Detalles: {errorContent}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al guardar el horario: {ex.Message}", "OK");
        }
    }

    public class ResponseModel
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public List<Schedules> Data { get; set; }
    }
}
