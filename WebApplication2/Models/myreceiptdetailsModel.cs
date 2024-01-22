namespace My_receipt_gate_pass.Models
{
    public class myreceiptdetailsModel

    {
        public int Request_ref_no { get; set; }
        //Pending Item Details
        public string? Item_serial_no { get; set; }
        public string? Item_name { get; set; }
        public string? Item_description { get; set; }

        public string Returnable_status { get; set; }

        public int Item_id { get; set; }

        //using the attachments table

        public byte[] Attaches { get; set; }



    }
}