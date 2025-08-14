using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace smpl_crm.Models
{
    public class MstUser
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }
        [Required]
        public string FullName { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string OrigPassword { get; set; }
    }
}