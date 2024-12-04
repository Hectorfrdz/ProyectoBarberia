using System.Collections.ObjectModel;
using Newtonsoft.Json;
using ProyectoBarberia.Models;
using ProyectoBarberia.Views.Cosas;

namespace ProyectoBarberia.Views.Admin;

public partial class Calendario : ContentPage
{
    private ObservableCollection<Users> Barbers { get; set; } = new();

    public Calendario()
    {
        InitializeComponent();
    }

    // Este método se llama cuando la página aparece en la vista
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadBarbersAsync();  // Recargar la lista de barberos cada vez que la página aparezca
    }

    private async Task LoadBarbersAsync()
    {
        try
        {
            // Obtener el token desde las preferencias
            if (!Preferences.ContainsKey("authToken"))
            {
                Console.WriteLine("Error: No se encontró el token de autenticación en Preferences.");
                return;
            }

            var authToken = Preferences.Get("authToken", string.Empty);

            // Crear el cliente HTTP y configurar los encabezados
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Realizar la solicitud a la API
            var response = await httpClient.GetStringAsync("http://127.0.0.1:3333/api/v1/users");

            // Deserializar la respuesta
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<Users>>(response);

            if (apiResponse != null && apiResponse.Status == "OK")
            {
                var allUsers = apiResponse.Data;

                var barbers = allUsers.Where(user => user.Role.Name == "BARBER");

                //barbers = barbers.Where(barbers => barbers.Active == true);

                var activeBarbers = barbers.Where(user => user.Active);
                var inactiveBarbers = barbers.Where(user => !user.Active);
                var sortedBarbers = activeBarbers.Concat(inactiveBarbers).ToList();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Barbers = new ObservableCollection<Users>(sortedBarbers);
                    BarbersCollectionView.ItemsSource = Barbers;
                });
            }
            else
            {
                Console.WriteLine("Error: La respuesta de la API no es válida o no tiene estatus OK.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar barberos: {ex.Message}");
        }
    }

    private async void OnAddBarberClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AgregarBarberoPage());
    }

    private async void OnDeleteBarberClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Users barber)
        {
            bool confirm = await DisplayAlert("Eliminar Barbero", 
                $"¿Estás seguro de que deseas desactivar a {barber.Name} {barber.Lastname}?", 
                "Sí", "No");

            if (confirm)
            {
                try
                {
                    var authToken = Preferences.Get("authToken", string.Empty);

                    var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

                    // URL de la API para desactivar el barbero
                    var url = $"http://127.0.0.1:3333/api/v1/users/{barber.Id}"; // Suponiendo que hay un endpoint de desactivación

                    var response = await httpClient.DeleteAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Éxito", "El barbero ha sido desactivado correctamente.", "OK");
                        // Recargar la lista de barberos
                        await LoadBarbersAsync();
                    }
                    else
                    {
                        await DisplayAlert("Error", "Hubo un error al intentar desactivar al barbero.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al desactivar al barbero: {ex.Message}");
                    await DisplayAlert("Error", "Ocurrió un error al desactivar al barbero.", "OK");
                }
            }
        }
    }

    private async void OnViewScheduleClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Users barber)
        {
            // Navegar a la página BarberSchedulePage pasando el barbero
            await Navigation.PushAsync(new BarberSchedulePage(barber));
        }
    }

    private async void OnToggleActiveClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Users barber)
        {
            bool confirm = await DisplayAlert(
                barber.Active ? "Desactivar Barbero" : "Activar Barbero",
                $"¿Estás seguro de que deseas {(barber.Active ? "desactivar" : "activar")} a {barber.Name} {barber.Lastname}?",
                "Sí", "No");

            if (confirm)
            {
                try
                {
                    var authToken = Preferences.Get("authToken", string.Empty);

                    var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

                    // Cambiar el estado del barbero en la API
                    var url = $"http://127.0.0.1:3333/api/v1/users/{barber.Id}";
                    var response = await httpClient.DeleteAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Éxito", $"El barbero ha sido {(barber.Active ? "desactivado" : "activado")} correctamente.", "OK");
                        await LoadBarbersAsync(); // Recargar la lista
                    }
                    else
                    {
                        await DisplayAlert("Error", "Hubo un error al intentar actualizar el estado del barbero.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al actualizar el estado del barbero: {ex.Message}");
                    await DisplayAlert("Error", "Ocurrió un error al actualizar el estado del barbero.", "OK");
                }
            }
        }
    }

}
