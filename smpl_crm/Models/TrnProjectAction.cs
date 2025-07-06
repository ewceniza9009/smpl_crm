using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smpl_crm.Models
{
    public class TrnProjectAction
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int Programmer { get; set; }
        public string Issue { get; set; }
        public string Action { get; set; }
        public DateTime EncodedDateTime { get; set; }
        public DateTime StartedDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public decimal NumberOfHours { get; set; }
        public bool Done { get; set; }
    }
}