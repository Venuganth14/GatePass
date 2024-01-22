using System;
using System.ComponentModel.DataAnnotations;

namespace GatePass_Project.Models
{
    public class NewRequestModel
    {
        [Key]
        public int Request_ref_no { get; set; }

        [Required]
        [Display(Name = "Sender Service No")]
        public string? Sender_service_no { get; set; }

        [Required]
        [Display(Name = "In Location Name")]
        public string? In_location_name { get; set; }

        [Required]
        [Display(Name = "Out Location Name")]
        public string? Out_location_name { get; set; }

        [Required]
        [Display(Name = "Receiver Service No")]
        public string? Receiver_service_no { get; set; }

        [Required]
        [Display(Name = "Created Date")]
        public DateTime Created_date { get; set; }

        [Required]
        [Display(Name = "ExO Service No")]
        public string? ExO_service_no { get; set; }

        [Required]
        [Display(Name = "Carrier NIC No")]
        public string? Carrier_nic_no { get; set; }

    }
}
