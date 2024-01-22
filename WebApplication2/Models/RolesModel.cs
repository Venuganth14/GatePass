using System.ComponentModel.DataAnnotations;

namespace GatePass_Project.Models
{
    public class RolesModel
    {
        [Key]
        public int Role_id { get; set; }
        public string Role_duty { get; set; }
    }
}
