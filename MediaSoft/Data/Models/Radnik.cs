using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Text;

namespace MediaSoft.Data.Models
{
    public class Radnik
    {
        public string Korisnicko_ime { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Lozinka { get; set; }
        public string Pwd { get; set; }
    }
}
