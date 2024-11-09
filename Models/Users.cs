using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProyectoBarberia.Models;

namespace ProyectoBarberia.Models
{
    internal class Users
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Lastname { get; set; }

        public bool Active { get; set; }

        // Relaciones
        public List<Users_roles> UserRoles { get; set; }
        public List<Schedules> Schedules { get; set; }
        public List<Appointments> AppointmentsAsBarber { get; set; }
        public List<Appointments> AppointmentsAsClient { get; set; }
    }

}
