using GatePass_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace GatePass_Project.Controllers
{
    public class NewRequestController : Controller
    {
        private readonly IConfiguration _configuration;

        public NewRequestController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult NewRequest()
        {
            // Fetch data from the database for Item Categories
            List<string> itemCategories = new List<string>();
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Category_name FROM Item_Category", connection))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
#pragma warning disable CS8604 // Possible null reference argument.
                            itemCategories.Add(reader["Category_name"].ToString());
#pragma warning restore CS8604 // Possible null reference argument.
                        }
                    }
                }
            }


            // Fetch data from the database for Locations
            List<string> locations = new List<string>();
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Loc_name FROM Locations WHERE IsDeleted IS NULL", connection))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
#pragma warning disable CS8604 // Possible null reference argument.
                            locations.Add(reader["Loc_name"].ToString());
#pragma warning restore CS8604 // Possible null reference argument.
                        }
                    }
                }
            }


            // Fetch executive officers from the database with "grade one" filter
            List<string> executiveOfficers = new List<string>();
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT CONCAT(Employee_title, ' ', Employee_initials, ' ', Employee_surname) AS ExecutiveOfficer FROM TempUsers WHERE Employee_salary_grade = 'grade one'", connection))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
#pragma warning disable CS8604 // Possible null reference argument.
                            executiveOfficers.Add(reader["ExecutiveOfficer"].ToString());
#pragma warning restore CS8604 // Possible null reference argument.
                        }
                    }
                }
            }

            // Pass the data to the view
            ViewBag.ItemCategories = itemCategories;
            ViewBag.Locations = locations;
            ViewBag.ExecutiveOfficers = executiveOfficers;

            return View();

        }


        public IActionResult GetSenderDetails(string serviceNo)
        {
            // Retrieve the connection string from appsettings.json
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            // Create a SqlConnection using the connection string
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Construct a SQL query to retrieve sender details based on serviceNo
                    var query = $"SELECT Employee_section, Employee_firstname, Employee_surname, " +
                                $"Employee_group_name, Employee_designation, Employee_mobile_phone " +
                                $"FROM TempUsers WHERE Employee_number = @serviceNo";

                    using (var command = new SqlCommand(query, connection))
                    {
                        // Add the serviceNo parameter to the query
                        command.Parameters.AddWithValue("@serviceNo", serviceNo);

                        // Execute the query and retrieve the sender details
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                if (reader.Read())
                                {
                                    var data = new
                                    {
                                        section = reader["Employee_section"].ToString(),
                                        name = $"{reader["Employee_firstname"]} {reader["Employee_surname"]}",
                                        group = reader["Employee_group_name"].ToString(),
                                        designation = reader["Employee_designation"].ToString(),
                                        contactNo = reader["Employee_mobile_phone"].ToString(),

                                    };


                                    return Json(data);
                                }
                            }
                            else
                            {
                                // Return a custom JSON response for invalid service number
                                return Json(new { error = "Invalid Service Number" });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions if needed
                    return BadRequest(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }

            // Return an error response if sender details were not found
            return NotFound();

        }


        [HttpGet]
        public IActionResult GetReceiverDetails(string serviceNo)
        {
            // Retrieve the connection string from appsettings.json
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            // Create a SqlConnection using the connection string
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Construct a SQL query to retrieve receiver details based on serviceNo
                    var query = $"SELECT Employee_group_name, CONCAT(Employee_firstname, ' ', Employee_surname) AS ReceiverName, Employee_mobile_phone " +
                                $"FROM TempUsers WHERE Employee_number = @serviceNo";

                    using (var command = new SqlCommand(query, connection))
                    {
                        // Add the serviceNo parameter to the query
                        command.Parameters.AddWithValue("@serviceNo", serviceNo);

                        // Execute the query and retrieve the receiver details
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var data = new
                                {
                                    group = reader["Employee_group_name"].ToString(),
                                    name = reader["ReceiverName"].ToString(),
                                    contactNo = reader["Employee_mobile_phone"].ToString()
                                };

                                return Json(data);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions if needed
                    return BadRequest(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }

            // Return an error response if receiver details were not found
            return NotFound();

        }


        //Save request into database

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpPost("CheckLogin")]
        public IActionResult CheckLogin(
            string ServiceNo,
            string CarrierName,
            string NICNo,

            string ItemName1,
            string ItemName2,
            string ItemName3,
            string ItemName4,
            string ItemName5,
            string ItemName6,
            string ItemName7,
            string ItemName8,
            string ItemName9,
            string ItemName10,

            string ItemSerialNo1,
            string ItemSerialNo2,
            string ItemSerialNo3,
            string ItemSerialNo4,
            string ItemSerialNo5,
            string ItemSerialNo6,
            string ItemSerialNo7,
            string ItemSerialNo8,
            string ItemSerialNo9,
            string ItemSerialNo10,


            string ItemCategory1,
            string ItemCategory2,
            string ItemCategory3,
            string ItemCategory4,
            string ItemCategory5,
            string ItemCategory6,
            string ItemCategory7,
            string ItemCategory8,
            string ItemCategory9,
            string ItemCategory10,


            string ItemDescription1,
            string ItemDescription2,
            string ItemDescription3,
            string ItemDescription4,
            string ItemDescription5,
            string ItemDescription6,
            string ItemDescription7,
            string ItemDescription8,
            string ItemDescription9,
            string ItemDescription10,

            string LocationName,
            string SLTInLocation,
            string NonSLTInLocation,
            string OutLocation,
            string ExecutiveOfficer,
            string ReceiverServiceNo,

            IFormFile itemPhotos1,
            IFormFile itemPhotos2,// Use IFormFile for file upload
            IFormFile itemPhotos3,
            IFormFile itemPhotos4,
            IFormFile itemPhotos5,
            IFormFile itemPhotos6,
            IFormFile itemPhotos7,
            IFormFile itemPhotos8,
            IFormFile itemPhotos9,
            IFormFile itemPhotos10,

            string returnable1,
            string returnable2,
            string returnable3,
            string returnable4,
            string returnable5,
            string returnable6,
            string returnable7,
            string returnable8,
            string returnable9,
            string returnable10,

            string Contactno,
            bool VerifiedItems,
            int stageId
        )
        {
            int isdone = 0;
#pragma warning disable CS0168 // Variable is declared but never used
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                int categoryId1 = GetCategoryIdForItem(ItemCategory1);
                int categoryId2 = GetCategoryIdForItem(ItemCategory2);
                int categoryId3 = GetCategoryIdForItem(ItemCategory3);
                int categoryId4 = GetCategoryIdForItem(ItemCategory4);
                int categoryId5 = GetCategoryIdForItem(ItemCategory5);
                int categoryId6 = GetCategoryIdForItem(ItemCategory6);
                int categoryId7 = GetCategoryIdForItem(ItemCategory7);
                int categoryId8 = GetCategoryIdForItem(ItemCategory8);
                int categoryId9 = GetCategoryIdForItem(ItemCategory9);
                int categoryId10 = GetCategoryIdForItem(ItemCategory10);

                int requestRefNo = GetRequestRefNoForItem();
                int itemId = GetItemIdForAttach();

                string executiveOfficerServiceNo = GetEmployeeServiceNoByName(ExecutiveOfficer);


                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();



                    if (!string.IsNullOrEmpty(NICNo))
                    {

                        string insertSql2 = "INSERT INTO Carrier_Details (Carrier_name, NIC_no, Contact_no) VALUES (@Carriername, @Carriernic, @Carriercontact)";


                        // Create a SqlCommand object with the insert SQL statement and the SqlConnection
                        using (SqlCommand cmd2 = new SqlCommand(insertSql2, connection))
                        {

                            // Add parameters to the SQL command
                            cmd2.Parameters.AddWithValue("@Carriername", CarrierName);
                            cmd2.Parameters.AddWithValue("@Carriernic", NICNo);
                            cmd2.Parameters.AddWithValue("@Carriercontact", Contactno);


                            // Execute the SQL command
                            int rowsAffected = cmd2.ExecuteNonQuery();

                            // Check the result and handle errors if needed
                            if (rowsAffected > 0)
                            {
                                isdone = 1;
                            }

                            else
                            {
                                isdone = -1; //carrerdetails
                            }
                        }
                    }


                    {
                        byte[] imageBytes1;
                        using (MemoryStream memoryStream1 = new MemoryStream())
                        {
                            itemPhotos1.CopyTo(memoryStream1);
                            imageBytes1 = memoryStream1.ToArray();
                        }

                        string imageBase64 = Convert.ToBase64String(imageBytes1);

                        string insertSql3 = "INSERT INTO Items (Item_serial_no, Item_name, Item_description, Returnable_status, Created_date, Category_id, Request_ref_no, Verified_items, Attaches) VALUES (@Itemserialno1, @Itemname1, @Itemdescription1, @Returnablestatus1, @Duedate1, @Categoryid1, @Requestrefno1, @Verifieditems, @Attaches1)";

                        using (SqlCommand cmd3 = new SqlCommand(insertSql3, connection))
                        {

                            // Add parameters to the SQL command
                            cmd3.Parameters.AddWithValue("@Itemserialno1", ItemSerialNo1);
                            cmd3.Parameters.AddWithValue("@Itemname1", ItemName1);
                            cmd3.Parameters.AddWithValue("@Itemdescription1", ItemDescription1);

                            if (returnable1 == "Yes")
                            {
                                cmd3.Parameters.AddWithValue("@Returnablestatus1", "Yes");
                            }
                            else
                            {
                                cmd3.Parameters.AddWithValue("@Returnablestatus1", "No");
                            }

                            //cmd3.Parameters.AddWithValue("@Returnablestatus1", returnable1 ? );
                            cmd3.Parameters.AddWithValue("@Duedate1", System.DateTime.Now);
                            cmd3.Parameters.AddWithValue("@Categoryid1", categoryId1);

                            cmd3.Parameters.AddWithValue("@Requestrefno1", requestRefNo);
                            cmd3.Parameters.AddWithValue("@Verifieditems", VerifiedItems);
                            cmd3.Parameters.Add("@Attaches1", SqlDbType.VarBinary, -1).Value = imageBytes1;



                            // Execute the SQL command
                            int rowsAffected = cmd3.ExecuteNonQuery();


                            // Check the result and handle errors if needed
                            if (rowsAffected > 0)
                            {
                                isdone = 2;
                            }
                            else
                            {
                                isdone = -2; //item details
                            }
                        }
                    }


                    string insertSql = "INSERT INTO Requests (Sender_service_no, In_location_name, Out_location_name, Receiver_service_no, Created_date, ExO_service_no, Carrier_nic_no) VALUES (@ServiceNo, @SLTInLocation, @OutLocation, @Receiver_service_no, @dates, @ExecutiveOfficerServiceNo, @NICNo)";

                    using (SqlCommand cmd = new SqlCommand(insertSql, connection))
                    {

                        // Add parameters to the SQL command
                        cmd.Parameters.AddWithValue("@ServiceNo", ServiceNo);
                        cmd.Parameters.AddWithValue("@SLTInLocation", SLTInLocation);
                        cmd.Parameters.AddWithValue("@OutLocation", OutLocation);


                        if (!string.IsNullOrEmpty(ReceiverServiceNo) && ReceiverServiceNo != "null")
                        {
                            cmd.Parameters.AddWithValue("@Receiver_service_no", ReceiverServiceNo);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@Receiver_service_no", DBNull.Value);
                        }
                        cmd.Parameters.AddWithValue("@dates", System.DateTime.Now);
                        cmd.Parameters.AddWithValue("@ExecutiveOfficerServiceNo", executiveOfficerServiceNo);
                        // If NICNo is provided, use it; otherwise, set it to an empty string or some default value
                        cmd.Parameters.AddWithValue("@NICNo", string.IsNullOrEmpty(NICNo) ? (object)DBNull.Value : NICNo);


                        // Execute the SQL command
                        int rowsAffected = cmd.ExecuteNonQuery();

                        // Check the result and handle errors if needed
                        if (rowsAffected > 0)
                        {
                            isdone = 3;
                        }
                        else
                        {
                            isdone = -3; //senderdetails
                        }
                    }



                    string insertSql5 = "INSERT INTO Workprogress (Request_ref_no, Stage_id, Progress_status, Update_date, Viewed) VALUES (@Requestrefno, @Stageid, @ProgressStatus, @Updatedate, @Viewed)";

                    using (SqlCommand cmd5 = new SqlCommand(insertSql5, connection))
                    {

                        cmd5.Parameters.AddWithValue("@Requestrefno", requestRefNo);
                        cmd5.Parameters.AddWithValue("@Stageid", 1);
                        cmd5.Parameters.AddWithValue("@ProgressStatus", "Executive Pending");
                        cmd5.Parameters.AddWithValue("@Updatedate", System.DateTime.Now);
                        cmd5.Parameters.AddWithValue("@Viewed", 0);

                        // Execute the SQL command
                        int rowsAffected = cmd5.ExecuteNonQuery();

                        // Check the result and handle errors if needed
                        if (rowsAffected > 0)
                        {
                            isdone = 4;
                        }

                        else
                        {
                            isdone = -4; //workprogress details
                        }
                    }



                    // item 2
                    if (!string.IsNullOrEmpty(ItemName2))
                    {
                        byte[] imageBytes2;
                        using (MemoryStream memoryStream2 = new MemoryStream())
                        {
                            itemPhotos2.CopyTo(memoryStream2);
                            imageBytes2 = memoryStream2.ToArray();
                        }


                        string imageBase64 = Convert.ToBase64String(imageBytes2);

                        string insertSql6 = "INSERT INTO Items (Item_serial_no, Item_name, Item_description, Returnable_status, Created_date, Category_id, Request_ref_no, Verified_items, Attaches) VALUES (@Itemserialno2, @Itemname2, @Itemdescription2, @Returnablestatus2, @Duedate2, @Categoryid2, @Requestrefno2, @Verifieditems, @Attaches2)";


                        using (SqlCommand cmd6 = new SqlCommand(insertSql6, connection))
                        {

                            // Add parameters to the SQL command
                            cmd6.Parameters.AddWithValue("@Itemserialno2", ItemSerialNo2);
                            cmd6.Parameters.AddWithValue("@Itemname2", ItemName2);
                            cmd6.Parameters.AddWithValue("@Itemdescription2", ItemDescription2);

                            if (returnable2 == "Yes")
                            {
                                cmd6.Parameters.AddWithValue("@Returnablestatus2", "Yes");
                            }
                            else
                            {
                                cmd6.Parameters.AddWithValue("@Returnablestatus2", "No");
                            }

                            cmd6.Parameters.AddWithValue("@Duedate2", System.DateTime.Now);
                            cmd6.Parameters.AddWithValue("@Categoryid2", categoryId2);

                            cmd6.Parameters.AddWithValue("@Requestrefno2", requestRefNo);
                            cmd6.Parameters.AddWithValue("@Verifieditems", VerifiedItems);
                            cmd6.Parameters.Add("@Attaches2", SqlDbType.VarBinary, -1).Value = imageBytes2;



                            // Execute the SQL command
                            int rowsAffected = cmd6.ExecuteNonQuery();


                            // Check the result and handle errors if needed
                            if (rowsAffected > 0)
                            {
                                isdone = 5;
                            }
                            else
                            {
                                isdone = -5; //item details
                            }
                        }
                    }


                    // item 3
                    if (!string.IsNullOrEmpty(ItemName3))
                    {
                        byte[] imageBytes3;
                        using (MemoryStream memoryStream3 = new MemoryStream())
                        {
                            itemPhotos3.CopyTo(memoryStream3);
                            imageBytes3 = memoryStream3.ToArray();
                        }


                        string imageBase64 = Convert.ToBase64String(imageBytes3);

                        string insertSql7 = "INSERT INTO Items (Item_serial_no, Item_name, Item_description, Returnable_status, Created_date, Category_id, Request_ref_no, Verified_items, Attaches) VALUES (@Itemserialno3, @Itemname3, @Itemdescription3, @Returnablestatus3, @Duedate3, @Categoryid3, @Requestrefno3, @Verifieditems, @Attaches3)";


                        using (SqlCommand cmd7 = new SqlCommand(insertSql7, connection))
                        {

                            // Add parameters to the SQL command
                            cmd7.Parameters.AddWithValue("@Itemserialno3", ItemSerialNo3);
                            cmd7.Parameters.AddWithValue("@Itemname3", ItemName3);
                            cmd7.Parameters.AddWithValue("@Itemdescription3", ItemDescription3);

                            if (returnable3 == "Yes")
                            {
                                cmd7.Parameters.AddWithValue("@Returnablestatus3", "Yes");
                            }
                            else
                            {
                                cmd7.Parameters.AddWithValue("@Returnablestatus3", "No");
                            }

                            cmd7.Parameters.AddWithValue("@Duedate3", System.DateTime.Now);
                            cmd7.Parameters.AddWithValue("@Categoryid3", categoryId3);

                            cmd7.Parameters.AddWithValue("@Requestrefno3", requestRefNo);
                            cmd7.Parameters.AddWithValue("@Verifieditems", VerifiedItems);
                            cmd7.Parameters.Add("@Attaches3", SqlDbType.VarBinary, -1).Value = imageBytes3;



                            // Execute the SQL command
                            int rowsAffected = cmd7.ExecuteNonQuery();


                            // Check the result and handle errors if needed
                            if (rowsAffected > 0)
                            {
                                isdone = 6;
                            }
                            else
                            {
                                isdone = -6; //item details
                            }
                        }
                    }


                    // item 4
                    if (!string.IsNullOrEmpty(ItemName4))
                    {
                        byte[] imageBytes4;
                        using (MemoryStream memoryStream4 = new MemoryStream())
                        {
                            itemPhotos4.CopyTo(memoryStream4);
                            imageBytes4 = memoryStream4.ToArray();
                        }


                        string imageBase64 = Convert.ToBase64String(imageBytes4);

                        string insertSql8 = "INSERT INTO Items (Item_serial_no, Item_name, Item_description, Returnable_status, Created_date, Category_id, Request_ref_no, Verified_items, Attaches) VALUES (@Itemserialno4, @Itemname4, @Itemdescription4, @Returnablestatus4, @Duedate4, @Categoryid4, @Requestrefno4, @Verifieditems, @Attaches4)";


                        using (SqlCommand cmd8 = new SqlCommand(insertSql8, connection))
                        {

                            // Add parameters to the SQL command
                            cmd8.Parameters.AddWithValue("@Itemserialno4", ItemSerialNo4);
                            cmd8.Parameters.AddWithValue("@Itemname4", ItemName4);
                            cmd8.Parameters.AddWithValue("@Itemdescription4", ItemDescription4);

                            if (returnable4 == "Yes")
                            {
                                cmd8.Parameters.AddWithValue("@Returnablestatus4", "Yes");
                            }
                            else
                            {
                                cmd8.Parameters.AddWithValue("@Returnablestatus4", "No");
                            }

                            cmd8.Parameters.AddWithValue("@Duedate4", System.DateTime.Now);
                            cmd8.Parameters.AddWithValue("@Categoryid4", categoryId4);

                            cmd8.Parameters.AddWithValue("@Requestrefno4", requestRefNo);
                            cmd8.Parameters.AddWithValue("@Verifieditems", VerifiedItems);
                            cmd8.Parameters.Add("@Attaches4", SqlDbType.VarBinary, -1).Value = imageBytes4;



                            // Execute the SQL command
                            int rowsAffected = cmd8.ExecuteNonQuery();


                            // Check the result and handle errors if needed
                            if (rowsAffected > 0)
                            {
                                isdone = 7;
                            }
                            else
                            {
                                isdone = -7; //item details
                            }
                        }
                    }



                    // item 5
                    if (!string.IsNullOrEmpty(ItemName5))
                    {
                        byte[] imageBytes5;
                        using (MemoryStream memoryStream5 = new MemoryStream())
                        {
                            itemPhotos5.CopyTo(memoryStream5);
                            imageBytes5 = memoryStream5.ToArray();
                        }


                        string imageBase64 = Convert.ToBase64String(imageBytes5);

                        string insertSql9 = "INSERT INTO Items (Item_serial_no, Item_name, Item_description, Returnable_status, Created_date, Category_id, Request_ref_no, Verified_items, Attaches) VALUES (@Itemserialno5, @Itemname5, @Itemdescription5, @Returnablestatus5, @Duedate5, @Categoryid5, @Requestrefno5, @Verifieditems, @Attaches5)";


                        using (SqlCommand cmd9 = new SqlCommand(insertSql9, connection))
                        {

                            // Add parameters to the SQL command
                            cmd9.Parameters.AddWithValue("@Itemserialno5", ItemSerialNo5);
                            cmd9.Parameters.AddWithValue("@Itemname5", ItemName5);
                            cmd9.Parameters.AddWithValue("@Itemdescription5", ItemDescription5);

                            if (returnable5 == "Yes")
                            {
                                cmd9.Parameters.AddWithValue("@Returnablestatus5", "Yes");
                            }
                            else
                            {
                                cmd9.Parameters.AddWithValue("@Returnablestatus5", "No");
                            }

                            cmd9.Parameters.AddWithValue("@Duedate5", System.DateTime.Now);
                            cmd9.Parameters.AddWithValue("@Categoryid5", categoryId5);

                            cmd9.Parameters.AddWithValue("@Requestrefno5", requestRefNo);
                            cmd9.Parameters.AddWithValue("@Verifieditems", VerifiedItems);
                            cmd9.Parameters.Add("@Attaches5", SqlDbType.VarBinary, -1).Value = imageBytes5;



                            // Execute the SQL command
                            int rowsAffected = cmd9.ExecuteNonQuery();


                            // Check the result and handle errors if needed
                            if (rowsAffected > 0)
                            {
                                isdone = 8;
                            }
                            else
                            {
                                isdone = -8; //item details
                            }
                        }
                    }



                    // item 6
                    if (!string.IsNullOrEmpty(ItemName6))
                    {
                        byte[] imageBytes6;
                        using (MemoryStream memoryStream6 = new MemoryStream())
                        {
                            itemPhotos6.CopyTo(memoryStream6);
                            imageBytes6 = memoryStream6.ToArray();
                        }


                        string imageBase64 = Convert.ToBase64String(imageBytes6);

                        string insertSql10 = "INSERT INTO Items (Item_serial_no, Item_name, Item_description, Returnable_status, Created_date, Category_id, Request_ref_no, Verified_items, Attaches) VALUES (@Itemserialno6, @Itemname6, @Itemdescription6, @Returnablestatus6, @Duedate6, @Categoryid6, @Requestrefno6, @Verifieditems, @Attaches6)";


                        using (SqlCommand cmd10 = new SqlCommand(insertSql10, connection))
                        {

                            // Add parameters to the SQL command
                            cmd10.Parameters.AddWithValue("@Itemserialno6", ItemSerialNo6);
                            cmd10.Parameters.AddWithValue("@Itemname6", ItemName6);
                            cmd10.Parameters.AddWithValue("@Itemdescription6", ItemDescription6);

                            if (returnable6 == "Yes")
                            {
                                cmd10.Parameters.AddWithValue("@Returnablestatus6", "Yes");
                            }
                            else
                            {
                                cmd10.Parameters.AddWithValue("@Returnablestatus6", "No");
                            }

                            cmd10.Parameters.AddWithValue("@Duedate6", System.DateTime.Now);
                            cmd10.Parameters.AddWithValue("@Categoryid6", categoryId6);

                            cmd10.Parameters.AddWithValue("@Requestrefno6", requestRefNo);
                            cmd10.Parameters.AddWithValue("@Verifieditems", VerifiedItems);
                            cmd10.Parameters.Add("@Attaches6", SqlDbType.VarBinary, -1).Value = imageBytes6;



                            // Execute the SQL command
                            int rowsAffected = cmd10.ExecuteNonQuery();


                            // Check the result and handle errors if needed
                            if (rowsAffected > 0)
                            {
                                isdone = 9;
                            }
                            else
                            {
                                isdone = -9; //item details
                            }
                        }
                    }


                    // item 7
                    if (!string.IsNullOrEmpty(ItemName7))
                    {
                        byte[] imageBytes7;
                        using (MemoryStream memoryStream7 = new MemoryStream())
                        {
                            itemPhotos7.CopyTo(memoryStream7);
                            imageBytes7 = memoryStream7.ToArray();
                        }


                        string imageBase64 = Convert.ToBase64String(imageBytes7);

                        string insertSql11 = "INSERT INTO Items (Item_serial_no, Item_name, Item_description, Returnable_status, Created_date, Category_id, Request_ref_no, Verified_items, Attaches) VALUES (@Itemserialno7, @Itemname7, @Itemdescription7, @Returnablestatus7, @Duedate7, @Categoryid7, @Requestrefno7, @Verifieditems, @Attaches7)";


                        using (SqlCommand cmd11 = new SqlCommand(insertSql11, connection))
                        {

                            // Add parameters to the SQL command
                            cmd11.Parameters.AddWithValue("@Itemserialno7", ItemSerialNo7);
                            cmd11.Parameters.AddWithValue("@Itemname7", ItemName7);
                            cmd11.Parameters.AddWithValue("@Itemdescription7", ItemDescription7);

                            if (returnable7 == "Yes")
                            {
                                cmd11.Parameters.AddWithValue("@Returnablestatus7", "Yes");
                            }
                            else
                            {
                                cmd11.Parameters.AddWithValue("@Returnablestatus7", "No");
                            }

                            cmd11.Parameters.AddWithValue("@Duedate7", System.DateTime.Now);
                            cmd11.Parameters.AddWithValue("@Categoryid7", categoryId7);

                            cmd11.Parameters.AddWithValue("@Requestrefno7", requestRefNo);
                            cmd11.Parameters.AddWithValue("@Verifieditems", VerifiedItems);
                            cmd11.Parameters.Add("@Attaches7", SqlDbType.VarBinary, -1).Value = imageBytes7;



                            // Execute the SQL command
                            int rowsAffected = cmd11.ExecuteNonQuery();


                            // Check the result and handle errors if needed
                            if (rowsAffected > 0)
                            {
                                isdone = 10;
                            }
                            else
                            {
                                isdone = -10; //item details
                            }
                        }
                    }


                    // item 8
                    if (!string.IsNullOrEmpty(ItemName8))
                    {
                        byte[] imageBytes8;
                        using (MemoryStream memoryStream8 = new MemoryStream())
                        {
                            itemPhotos8.CopyTo(memoryStream8);
                            imageBytes8 = memoryStream8.ToArray();
                        }


                        string imageBase64 = Convert.ToBase64String(imageBytes8);

                        string insertSql12 = "INSERT INTO Items (Item_serial_no, Item_name, Item_description, Returnable_status, Created_date, Category_id, Request_ref_no, Verified_items, Attaches) VALUES (@Itemserialno8, @Itemname8, @Itemdescription8, @Returnablestatus8, @Duedate8, @Categoryid8, @Requestrefno8, @Verifieditems, @Attaches8)";


                        using (SqlCommand cmd12 = new SqlCommand(insertSql12, connection))
                        {

                            // Add parameters to the SQL command
                            cmd12.Parameters.AddWithValue("@Itemserialno8", ItemSerialNo8);
                            cmd12.Parameters.AddWithValue("@Itemname8", ItemName8);
                            cmd12.Parameters.AddWithValue("@Itemdescription8", ItemDescription8);

                            if (returnable8 == "Yes")
                            {
                                cmd12.Parameters.AddWithValue("@Returnablestatus8", "Yes");
                            }
                            else
                            {
                                cmd12.Parameters.AddWithValue("@Returnablestatus8", "No");
                            }

                            cmd12.Parameters.AddWithValue("@Duedate8", System.DateTime.Now);
                            cmd12.Parameters.AddWithValue("@Categoryid8", categoryId8);

                            cmd12.Parameters.AddWithValue("@Requestrefno8", requestRefNo);
                            cmd12.Parameters.AddWithValue("@Verifieditems", VerifiedItems);
                            cmd12.Parameters.Add("@Attaches8", SqlDbType.VarBinary, -1).Value = imageBytes8;



                            // Execute the SQL command
                            int rowsAffected = cmd12.ExecuteNonQuery();


                            // Check the result and handle errors if needed
                            if (rowsAffected > 0)
                            {
                                isdone = 11;
                            }
                            else
                            {
                                isdone = -11; //item details
                            }
                        }
                    }


                    // item 9
                    if (!string.IsNullOrEmpty(ItemName9))
                    {
                        byte[] imageBytes9;
                        using (MemoryStream memoryStream9 = new MemoryStream())
                        {
                            itemPhotos9.CopyTo(memoryStream9);
                            imageBytes9 = memoryStream9.ToArray();
                        }


                        string imageBase64 = Convert.ToBase64String(imageBytes9);

                        string insertSql13 = "INSERT INTO Items (Item_serial_no, Item_name, Item_description, Returnable_status, Created_date, Category_id, Request_ref_no, Verified_items, Attaches) VALUES (@Itemserialno9, @Itemname9, @Itemdescription9, @Returnablestatus9, @Duedate9, @Categoryid9, @Requestrefno9, @Verifieditems, @Attaches9)";


                        using (SqlCommand cmd13 = new SqlCommand(insertSql13, connection))
                        {

                            // Add parameters to the SQL command
                            cmd13.Parameters.AddWithValue("@Itemserialno9", ItemSerialNo9);
                            cmd13.Parameters.AddWithValue("@Itemname9", ItemName9);
                            cmd13.Parameters.AddWithValue("@Itemdescription9", ItemDescription9);

                            if (returnable9 == "Yes")
                            {
                                cmd13.Parameters.AddWithValue("@Returnablestatus9", "Yes");
                            }
                            else
                            {
                                cmd13.Parameters.AddWithValue("@Returnablestatus9", "No");
                            }

                            cmd13.Parameters.AddWithValue("@Duedate9", System.DateTime.Now);
                            cmd13.Parameters.AddWithValue("@Categoryid9", categoryId9);

                            cmd13.Parameters.AddWithValue("@Requestrefno9", requestRefNo);
                            cmd13.Parameters.AddWithValue("@Verifieditems", VerifiedItems);
                            cmd13.Parameters.Add("@Attaches9", SqlDbType.VarBinary, -1).Value = imageBytes9;



                            // Execute the SQL command
                            int rowsAffected = cmd13.ExecuteNonQuery();


                            // Check the result and handle errors if needed
                            if (rowsAffected > 0)
                            {
                                isdone = 12;
                            }
                            else
                            {
                                isdone = -12; //item details
                            }
                        }
                    }


                    // item 10
                    if (!string.IsNullOrEmpty(ItemName10))
                    {
                        byte[] imageBytes10;
                        using (MemoryStream memoryStream10 = new MemoryStream())
                        {
                            itemPhotos10.CopyTo(memoryStream10);
                            imageBytes10 = memoryStream10.ToArray();
                        }


                        string imageBase64 = Convert.ToBase64String(imageBytes10);

                        string insertSql14 = "INSERT INTO Items (Item_serial_no, Item_name, Item_description, Returnable_status, Created_date, Category_id, Request_ref_no, Verified_items, Attaches) VALUES (@Itemserialno10, @Itemname10, @Itemdescription10, @Returnablestatus10, @Duedate10, @Categoryid10, @Requestrefno10, @Verifieditems, @Attaches10)";


                        using (SqlCommand cmd14 = new SqlCommand(insertSql14, connection))
                        {

                            // Add parameters to the SQL command
                            cmd14.Parameters.AddWithValue("@Itemserialno10", ItemSerialNo10);
                            cmd14.Parameters.AddWithValue("@Itemname10", ItemName10);
                            cmd14.Parameters.AddWithValue("@Itemdescription10", ItemDescription10);

                            if (returnable10 == "Yes")
                            {
                                cmd14.Parameters.AddWithValue("@Returnablestatus10", "Yes");
                            }
                            else
                            {
                                cmd14.Parameters.AddWithValue("@Returnablestatus10", "No");
                            }

                            cmd14.Parameters.AddWithValue("@Duedate10", System.DateTime.Now);
                            cmd14.Parameters.AddWithValue("@Categoryid10", categoryId10);

                            cmd14.Parameters.AddWithValue("@Requestrefno10", requestRefNo);
                            cmd14.Parameters.AddWithValue("@Verifieditems", VerifiedItems);
                            cmd14.Parameters.Add("@Attaches10", SqlDbType.VarBinary, -1).Value = imageBytes10;



                            // Execute the SQL command
                            int rowsAffected = cmd14.ExecuteNonQuery();


                            // Check the result and handle errors if needed
                            if (rowsAffected > 0)
                            {
                                isdone = 13;
                            }
                            else
                            {
                                isdone = -13; //item details
                            }
                        }
                    }



                }

            }
            catch (Exception ex)
            {
                isdone = -14;

            }
#pragma warning restore CS0168 // Variable is declared but never used

            // Replace the current return statement in your controller with the following:
            return Ok(new { isSuccess = isdone > 0, message = GetResultMessage(isdone) });


        }

        private string GetEmployeeServiceNoByName(string executiveOfficerName)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                // Construct a SQL query to retrieve the Employee number by name
                var query = "SELECT Employee_number FROM TempUsers WHERE CONCAT(Employee_title, ' ', Employee_initials, ' ', Employee_surname) = @ExecutiveOfficerName";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ExecutiveOfficerName", executiveOfficerName);

                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
#pragma warning disable CS8603 // Possible null reference return.
                        return result.ToString();
#pragma warning restore CS8603 // Possible null reference return.
                    }
                }
            }

            // Return a default value if not found (you may want to handle this differently)
            return "Unknown";
        }


        private int GetRequestRefNoForItem()
        {
            // Implement logic to retrieve the RequestRefNo from the database
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                // Construct a SQL query to retrieve RequestRefNo
                var query = "SELECT MAX(Request_ref_no) + 1 FROM Requests";

                using (var command = new SqlCommand(query, connection))
                {
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }

            return 1;
        }


        private int GetCategoryIdForItem(string itemCategory)
        {
            // Implement logic to retrieve the CategoryId based on itemCategory
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                // Construct a SQL query to retrieve CategoryId based on itemCategory
                var query = "SELECT Category_id FROM Item_Category WHERE Category_name = @CategoryName";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CategoryName", itemCategory);

                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }

            return 1;
        }


        private int GetItemIdForAttach()
        {
            // Implement logic to retrieve the RequestRefNo from the database
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                // Construct a SQL query to retrieve RequestRefNo
                var query = "SELECT MAX(Item_id) + 1 FROM Items";

                using (var command = new SqlCommand(query, connection))
                {
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }

            return 1;
        }


        private string GetResultMessage(int isdone)
        {
            switch (isdone)
            {
                case 1:
                    return "Item 1 added successfully";
                case 2:
                    return "Item 2 added successfully";
                case 3:
                    return "Item 3 added successfully";
                case 4:
                    return "Item 4 added successfully";
                case 5:
                    return "Item 5 added successfully";
                case 6:
                    return "Item 6 added successfully";
                case 7:
                    return "Item 7 added successfully";
                case 8:
                    return "Item 8 added successfully";
                case 9:
                    return "Item 9 added successfully";
                case 10:
                    return "Item 10 added successfully";
                // Add cases for other possible values of isdone

                case -1:
                    return "Error adding Item 1";
                case -2:
                    return "Error adding Item 2";
                case -3:
                    return "Error adding Item 3";
                case -4:
                    return "Error adding Item 4";
                case -5:
                    return "Error adding Item 5";
                case -6:
                    return "Error adding Item 6";
                case -7:
                    return "Error adding Item 7";
                case -8:
                    return "Error adding Item 8";
                case -9:
                    return "Error adding Item 9";
                case -10:
                    return "Error adding Item 10";
                // Add cases for other possible error values of isdone

                default:
                    return "Unknown result";
            }
        }




    }

}