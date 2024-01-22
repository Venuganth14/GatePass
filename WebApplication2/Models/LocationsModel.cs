using System.ComponentModel.DataAnnotations;

namespace GatePass_Project.Models
{
    public class LocationsModel
    {
        public int Loc_id { get; set; }
        [Required]
        public string Loc_name { get; set; }
        public bool IsDeleted { get; set; }
    }
}
