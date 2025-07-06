namespace smpl_crm.Models
{
    public class TrnCallAction
    {
        public int CallActionId { get; set; }
        public int CallId { get; set; }
        public int ActedBy { get; set; }
        public string Action { get; set; }
        public string EncodedDate { get; set; }
        public string TargetDate { get; set; }
        public string TargetTime { get; set; }
        public string AcceptedDate { get; set; }
        public string AcceptedTime { get; set; }
        public string AcceptedBy { get; set; }
        public int ActionTypeId { get; set; }
        public decimal Cost { get; set; }
        public decimal NumberOfHours { get; set; }
        public bool Done { get; set; }
    }
}