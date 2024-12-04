using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ProyectoBarberia.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [JsonProperty("appointment_date")]
        public DateTime AppointmentDate { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        // Relaciones
        [JsonProperty("user_barber")]
        public Users UserBarber { get; set; }

        [JsonProperty("user_customer")]
        public Users UserClient { get; set; }

        [JsonProperty("service")]
        public Service Service { get; set; }
    }

}
