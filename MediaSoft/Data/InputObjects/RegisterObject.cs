using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MediaSoft.Data.InputObjects
{
    public class RegisterObject
    {
        [Required]
        [MaxLength(50)]
        public string Ime { get; set; }
        [Required]
        [MaxLength(50)]
        public string Prezime { get; set; }
        [Required]
        [MaxLength(50)]
        public string Korisnicko_ime { get; set; }
        [Required]
        [MaxLength(10)]
        public string PWD { get; set; }
        [Required]
        [MaxLength(50)]
        public string Lozinka { get; set; }

        public string Claim { get; set; }
    }
}
