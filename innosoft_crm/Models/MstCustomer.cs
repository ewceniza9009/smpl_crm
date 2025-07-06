using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smpl_crm.Models
{
    public class MstCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string ContactPerson { get; set; }
        public string Telephone { get; set; }
        public string Fax { get; set; }
    }
}