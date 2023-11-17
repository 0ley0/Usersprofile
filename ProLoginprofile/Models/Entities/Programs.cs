using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProLoginprofile.Models.Entities
{
    public class Programs
    {
        [Key]
        public int id { get; set; }
        public string? name { get; set; }
        public bool Isactive { get; set; } = false;
        public int program_type_id { get; set; }
        
    }
}