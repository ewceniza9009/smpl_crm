using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smpl_crm.Models
{
    public class Select2PagedResult
    {
        public int Total { get; set; }
        public List<Select2Result> Results { get; set; }
    }
}