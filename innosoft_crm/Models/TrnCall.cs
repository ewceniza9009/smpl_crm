using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smpl_crm.Models
{
    public class TrnCall
    {
        public int Id { get; set; }
        public string DateCalled { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public string Caller { get; set; }
        public string Issue { get; set; }
        public int CallStatusId { get; set; }
        public int AnsweredById { get; set; }
        public int AssignedtoId {get;set;}
        public TrnCallAction CallAction { get; set; }
    }
}