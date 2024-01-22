using System;

namespace GatePass_Project.Models
{
    public class VerifyModel
    {
        // Request Details
        public int Request_ref_no { get; set; }
        public string? Sender_service_no { get; set; }
        public string? In_location_name { get; set; }
        public string? Out_location_name { get; set; }
        public string? Receiver_service_no { get; set; }
        public DateTime Created_date { get; set; }
        public string? ExO_service_no { get; set; }
        public string? Carrier_nic_no { get; set; }

        public string Name { get; set; }

        // Pending Item Details
        public string? Item_serial_no { get; set; }
        public string? Item_name { get; set; }
        public string? Item_Description { get; set; }
        public int Item_id { get; set; }

        public string Returnable_status { get; set; }

        // Status of the request (Pending, Verified, Rejected)
        public string Status { get; set; }

        //using the attachments table
       
        public byte[] Attaches { get; set; }

    }
}
