namespace GatePass_Project.Models
{
    public class Workprogress
    {
        public int WorkPro_id { get; set; }
        public int Request_ref_no { get; set; }
        public int Stage_id { get; set; }
        public string? Progress_status { get; set; }
        public DateTime Update_date { get; set; }
        public string? Any_comment { get; set; }


    }
}
