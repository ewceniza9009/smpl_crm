using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smpl_crm.Models
{
    public class TrnProject
    {
        public int Id { get; set; }
        public DateTime ProjectDate { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int Programmer { get; set; }
        public string Particulars { get; set; }
        public DateTime AcceptanceDate { get; set; }
        public int ProjectStatusId { get; set; }
        public int AccountExecutive { get; set; }
 
    }
}