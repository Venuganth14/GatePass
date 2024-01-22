using GatePass_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace GatePass_Project.Controllers
{
    public class ItemTrackerController : Controller
    {
        private readonly ILogger<ItemTrackerController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public ItemTrackerController(ILogger<ItemTrackerController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult ItemTracker()
        {
            List<ItemTrackerModel> requests = new List<ItemTrackerModel>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT DISTINCT r.Request_ref_no, r.Sender_service_no, r.In_location_name, r.Out_location_name, " +
               "r.Receiver_service_no, r.Created_date, r.ExO_service_no, r.Carrier_nic_no, " +
               "CONCAT(tu.Employee_initials, ' ', tu.Employee_firstname, ' ', tu.Employee_surname) AS Name, " +
               "tu.Employee_mobile_phone " +
               "FROM Requests r " +
               "INNER JOIN TempUsers tu ON r.Sender_service_no = tu.Employee_number " +
               "INNER JOIN Items i ON r.Request_ref_no = i.Request_ref_no " +
               "INNER JOIN Workprogress wp ON r.Request_ref_no = wp.Request_ref_no " +
               "WHERE wp.Stage_id = 7 AND " +
               "(r.Receiver_service_no IS NOT NULL AND i.Returnable_status = 'Yes') OR " +
               "(r.Receiver_service_no IS NULL AND i.Returnable_status = 'Yes') " +
               "ORDER BY r.Created_date DESC";


                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ItemTrackerModel request = new ItemTrackerModel
                            {
                                Request_ref_no = reader.GetInt32(0),
                                Sender_service_no = reader.GetString(1),
                                In_location_name = reader.GetString(2),
                                Out_location_name = reader.GetString(3),
                                Receiver_service_no = reader.IsDBNull(4) ? "No Specific Receiver" : reader.GetString(4),
                                Created_date = reader.GetDateTime(5),
                                ExO_service_no = reader.GetString(6),
                                Carrier_nic_no = reader.IsDBNull(7) ? "No Specific Carrier" : reader.GetString(7),
                                Name = reader.GetString(8),
                                MobileNo = reader.GetInt32(9)
                            };

                            requests.Add(request);
                        }
                    }
                }
            }
            return View(requests);
        }

        private List<ItemTrackerModel> GetDetailsById(int id)
        {
            List<ItemTrackerModel> itemsList = new List<ItemTrackerModel>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT i.Item_id, i.Item_serial_no, i.Item_name, i.Item_description, " +
                "i.Request_ref_no, i.Attaches " +
                "FROM Items i " +
                "WHERE i.Request_ref_no = @id AND i.Returnable_status = 'Yes'";


                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ItemTrackerModel request = new ItemTrackerModel
                                {
                                    Item_id = reader.GetInt32(0),
                                    Item_serial_no = reader.GetString(1),
                                    Item_name = reader.GetString(2),
                                    Item_Description = reader.GetString(3),
                                    Request_ref_no = reader.GetInt32(4),
                                    Attaches = reader["Attaches"] as byte[],
                                };

                                itemsList.Add(request);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while retrieving item details.");
                }
            }

            return itemsList;
        }

        //[HttpPost]
        //public IActionResult MarkAsReturned(int requestRefNo)
        //{
        //    try
        //    {
        //        using (SqlConnection connection = new SqlConnection(_connectionString))
        //        {
        //            connection.Open();
        //            string updateQuery = "UPDATE Workprogress SET Stage_id = 11, Update_date = GETDATE(), Progress_status = 'Returned', Viewed = 0 WHERE Request_ref_no = @requestRefNo";

        //            using (SqlCommand command = new SqlCommand(updateQuery, connection))
        //            {
        //                command.Parameters.AddWithValue("@requestRefNo", requestRefNo);
        //                command.ExecuteNonQuery();
        //            }
        //        }

        //        return RedirectToAction("ItemTracker");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "An error occurred while updating the Workprogress table.");
        //        return RedirectToAction("Error");
        //    }
        //}


        [HttpPost]
        public IActionResult MarkAsReturned(int requestRefNo)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string updateQuery = "UPDATE Workprogress SET Stage_id = 11, Update_date = GETDATE(), Progress_status = 'Returned', Viewed = 0 WHERE Request_ref_no = @requestRefNo";

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@requestRefNo", requestRefNo);
                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("ItemTracker");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while approving the request.");
                return RedirectToAction("Error");
            }
        }

        public IActionResult PendingReturnableDetails(int id)
        {
            List<ItemTrackerModel> items = GetDetailsById(id);

            if (items.Count == 0)
            {
                return RedirectToAction("Error");
            }

            return View(items);
        }
    }
}
