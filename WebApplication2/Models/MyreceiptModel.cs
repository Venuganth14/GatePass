namespace My_receipt_gate_pass.Models
{
    public class MyreceiptModel
    {
        public int Request_ref_no { get; set; }
        public string Sender_service_no { get; set; }
        public string In_location_name { get; set; }
        public string Out_location_name { get; set; }
        public string Receiver_service_no { get; set; }
        public DateTime Created_date { get; set; }
        public string ExO_service_no { get; set; }
        public string Carrier_nic_no { get; set; }

        // Properties from TempUsers table
        public string Name { get; set; }
       
    }
}
