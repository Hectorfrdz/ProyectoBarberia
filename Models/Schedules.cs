using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoBarberia.Models
{
    internal class Schedules
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long UserBarberId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public DateTime? StartRestTime { get; set; }
        public DateTime? EndRestTime { get; set; }

        [Required]
        [MaxLength(20)]
        public string Day { get; set; }

        // Relación
        public Users UserBarber { get; set; }
    }
}
