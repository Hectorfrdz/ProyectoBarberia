using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProyectoBarberia.Models
{
    public class Schedules
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("user_barber_id")]
        public int UserBarberId { get; set; }

        [JsonProperty("start_time")]
        public string StartTime { get; set; }

        [JsonProperty("end_time")]
        public string EndTime { get; set; }

        [JsonProperty("start_rest_time")]
        public string StartRestTime { get; set; }

        [JsonProperty("end_rest_time")]
        public string EndRestTime { get; set; }

        [JsonProperty("day")]
        public int Day { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }
        public Users User_Barber { get; set; }

        public string DayName
        {
            get
            {
                return Day switch
                {
                    1 => "Lunes",
                    2 => "Martes",
                    3 => "Miércoles",
                    4 => "Jueves",
                    5 => "Viernes",
                    6 => "Sábado",
                    7 => "Domingo",
                    _ => "Día inválido",
                };
            }
        }
        public string ButtonText => Active ? "Desactivar" : "Activar";
        public Color ButtonColor => Active ? Colors.Red : Colors.Green;
    }

}
