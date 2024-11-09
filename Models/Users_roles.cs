using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProyectoBarberia.Models;
using System.ComponentModel.DataAnnotations;

namespace ProyectoBarberia.Models
{
    internal class Users_roles
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        public long RoleId { get; set; }

        // Relaciones
        public Users User { get; set; }
        public Roles Role { get; set; }
    }
}
