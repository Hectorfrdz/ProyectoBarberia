using Newtonsoft.Json;

namespace ProyectoBarberia.Models
{
    public class Service
    {
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        public string ButtonText => Active ? "Desactivar" : "Activar";
        public Color ButtonColor => Active ? Colors.Red : Colors.Green;
    }
}
