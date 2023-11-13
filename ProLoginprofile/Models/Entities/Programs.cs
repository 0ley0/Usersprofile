using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProLoginprofile.Models.Entities
{
    public class Programs
    {
        public int id { get; set; }
        public string? name { get; set; }
        public bool Isactive { get; set; } = false;
        public int user_id { get; set; }
    }
}