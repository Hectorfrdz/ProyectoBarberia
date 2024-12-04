using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProyectoBarberia.Models;

namespace ProyectoBarberia.Models
{
    public class Users
    {
        public int Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("lastname")]
        public string Lastname { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("role_id")]
        public int Role_id { get; set; }

        public Roles Role { get; set; }

        public string ButtonText => Active ? "Desactivar" : "Activar";
        public Color ButtonColor => Active ? Colors.Red : Colors.Green;
    }

}
