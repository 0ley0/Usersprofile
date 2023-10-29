using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProLoginprofile.Models.Entities
{
    public class Users
    {
        public int id { get; set; }
        public string? login { get; set; }
        public string? email { get; set; }
    }
}