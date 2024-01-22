using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GatePass_Project.Models
{
    public class NonSLTEmployeeModel
    {
        [Key]
        public string Non_slt_Id { get; set; }
        [Required]
        public int Role_id { get; set; }
        [Required]
        public string Non_slt_name { get; set; }
        [Required]
        public string NIC { get; set; }

        [ForeignKey("Role_id")] // Specifying the foreign key property
        public RolesModel Role { get; set; }
    }
}
