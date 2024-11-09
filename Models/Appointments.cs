using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ProyectoBarberia.Models
{
    internal class Appointments
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long UserBarberId { get; set; }

        [Required]
        public long UserClientId { get; set; }

        [Required]
        public long ServiceId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        public bool Status { get; set; }

        // Relaciones
        public Users UserBarber { get; set; }
        public Users UserClient { get; set; }
        public Services Service { get; set; }
    }
}
