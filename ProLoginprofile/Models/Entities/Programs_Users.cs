using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProLoginprofile.Models.Entities
{
    public class Programs_Users
    {
        [Key]
        public int program_id { get; set; }
        public int user_id { get; set; }
    }
}