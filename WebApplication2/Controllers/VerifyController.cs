using GatePass_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace GatePass_Project.Controllers
{
    public class VerifyController : Controller
    {
        private readonly ILogger<VerifyController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public VerifyController(ILogger<VerifyController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        //public IActionResult Verify()
        //{
        //    List<VerifyModel> requests = new List<VerifyModel>();

        //    using (SqlConnection connection = new SqlConnection(_connectionString))
        //    {
        //        connection.Open();
        //        string query = "SELECT * FROM Requests WHERE Request_ref_no IN (SELECT Request_ref_no FROM Workprogress WHERE Stage_id = 2) ORDER BY Created_date DESC";

        //        try
        //        {
        //            using (SqlCommand command = new SqlCommand(query, connection))
        //            {
        //                using (SqlDataReader reader = command.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        VerifyModel request = new VerifyModel
        //                        {
        //                            Request_ref_no = reader.GetInt32(0),
        //                            Sender_service_no = reader.GetString(1),
        //                            In_location_name = reader.GetString(2),
        //                            Out_location_name = reader.GetString(3),
        //                            Receiver_service_no = reader.GetString(4),
        //                            Created_date = reader.GetDateTime(5),
        //                            ExO_service_no = reader.GetString(6),
        //                            Carrier_nic_no = reader.GetString(7),
        //                            Status = "Pending" // Assuming you have a Status property
        //                        };

        //                        requests.Add(request);
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "An error occurred while retrieving requests for verification.");
        //            return RedirectToAction("Error");
        //        }
        //    }

        //    return View(requests);
        //}


        public IActionResult Verify()
        {
            List<VerifyModel> requests = new List<VerifyModel>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT r.Request_ref_no, r.Sender_service_no, r.In_location_name, r.Out_location_name, " +
                        "r.Receiver_service_no, r.Created_date, r.ExO_service_no, r.Carrier_nic_no, " +
                        "CONCAT(tu.Employee_initials, ' ', tu.Employee_firstname, ' ', tu.Employee_surname) AS Name " +
                        "FROM Requests r " +
                        "INNER JOIN TempUsers tu ON r.Sender_service_no = tu.Employee_number " +
                        "WHERE r.Request_ref_no IN (SELECT Request_ref_no FROM Workprogress WHERE Stage_id = 2) ORDER BY Created_date DESC";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            VerifyModel request = new VerifyModel
                            {
                                Request_ref_no = reader.GetInt32(0),
                                Sender_service_no = reader.GetString(1),
                                In_location_name = reader.GetString(2),
                                Out_location_name = reader.GetString(3),
                                Receiver_service_no = reader.IsDBNull(4) ? "No Specific Receiver" : reader.GetString(4),
                                Created_date = reader.GetDateTime(5),
                                ExO_service_no = reader.GetString(6),
                                Carrier_nic_no = reader.IsDBNull(7) ? "No Specific Carrier" : reader.GetString(7),
                                Name = reader.GetString(8)
                            };

                            requests.Add(request);
                        }
                    }
                }

                return View(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving requests for verification.");
                return RedirectToAction("Error");
            }
        }


        [HttpPost]
        public IActionResult Approve(int requestRefNo)
        {
            try
            {
                // Update the Workprogress table with Stage_id = 5 and Update_date = current date for the specified Request_ref_no
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string updateQuery = "UPDATE Workprogress SET Stage_id = 5, Update_date = GETDATE(), Progress_status = 'DO Verified', Viewed = 0 WHERE Request_ref_no = @requestRefNo";

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@requestRefNo", requestRefNo);
                        command.ExecuteNonQuery();
                    }
                }

                // Redirect back to the Verify page or any other appropriate page
                return RedirectToAction("Verify");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while approving the request.");
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        public IActionResult Reject(int requestRefNo, string rejectComment)
        {
            try
            {
                // Update the Workprogress table with Stage_id = 6, Update_date = current date, and Any_comment for the specified Request_ref_no
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string updateQuery = "UPDATE Workprogress SET Stage_id = 6, Update_date = GETDATE(), Any_comment = @rejectComment, Progress_status = 'DO Rejected' , Viewed = 0 WHERE Request_ref_no = @requestRefNo";

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@requestRefNo", requestRefNo);
                        command.Parameters.AddWithValue("@rejectComment", rejectComment);
                        command.ExecuteNonQuery();
                    }
                }

                // Redirect back to the ExeApprove page or any other appropriate page
                return RedirectToAction("Verify");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while rejecting the request.");
                return RedirectToAction("Error");
            }
        }
        public IActionResult ViewPendingDetails(int id)
        {
            // Retrieve item details for the given Request_ref_no
            List<VerifyModel> items = GetDetailsById(id);

            if (items.Count == 0)
            {
                // Handle the case where no details are found for the given id, e.g., show an error message or redirect to an error page.
                return RedirectToAction("Error");
            }

            return View(items);
        }

        public IActionResult Pending()
        {
            List<VerifyModel> pendingRequests = GetRequestsByStageId(2);
            return View("Verify", pendingRequests);
        }

        public IActionResult Verified()
        {
            List<VerifyModel> verifiedRequests = GetRequestsByStageId(5);
            return View("Verify", verifiedRequests);
        }

        public IActionResult Rejected()
        {
            List<VerifyModel> rejectedRequests = GetRequestsByStageId(6);
            return View("Verify", rejectedRequests);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Retrieve item details for a given Request_ref_no
        private List<VerifyModel> GetDetailsById(int id)
        {
            List<VerifyModel> itemsList = new List<VerifyModel>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT i.Item_id, i.Item_serial_no, i.Item_name, i.Item_description, i.Returnable_status, " +
                            " i.Request_ref_no,  i.Attaches " +
                            "FROM Items i " +
                            
                            "WHERE i.Request_ref_no = @id";
                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                VerifyModel request = new VerifyModel
                                {
                                    Item_id = reader.GetInt32(0), // Include Item_id
                                    Item_serial_no = reader.GetString(1),
                                    Item_name = reader.GetString(2),
                                    Item_Description = reader.GetString(3),
                                    Returnable_status = reader.GetString(4),
                                    Request_ref_no = reader.GetInt32(5),// Include Request_ref_no
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
                    // Handle the error as needed, e.g., return an error view or redirect to an error page.
                }
            }

            return itemsList;
        }


        //private List<VerifyModel> GetRequestsByStageId(int stageId)
        //{
        //    List<VerifyModel> requests = new List<VerifyModel>();

        //    using (SqlConnection connection = new SqlConnection(_connectionString))
        //    {
        //        connection.Open();
        //        string query = "SELECT * FROM Requests WHERE Request_ref_no IN (SELECT Request_ref_no FROM Workprogress WHERE Stage_id = @stageId)";

        //        try
        //        {
        //            using (SqlCommand command = new SqlCommand(query, connection))
        //            {
        //                command.Parameters.AddWithValue("@stageId", stageId);

        //                using (SqlDataReader reader = command.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        VerifyModel request = new VerifyModel
        //                        {
        //                            Request_ref_no = reader.GetInt32(0),
        //                            Sender_service_no = reader.GetString(1),
        //                            In_location_name = reader.GetString(2),
        //                            Out_location_name = reader.GetString(3),
        //                            Receiver_service_no = reader.GetString(4),
        //                            Created_date = reader.GetDateTime(5),
        //                            ExO_service_no = reader.GetString(6),
        //                            Carrier_nic_no = reader.GetString(7)
        //                        };

        //                        requests.Add(request);
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, $"An error occurred while retrieving requests with stage_id {stageId}.");
        //            // Handle the error as needed, e.g., return an error view or redirect to an error page.
        //        }
        //    }

        //    return requests;
        //}

        // Filtering according to the stageId
        private List<VerifyModel> GetRequestsByStageId(int stageId)
        {
            List<VerifyModel> requests = new List<VerifyModel>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"
            SELECT 
                r.Request_ref_no, 
                r.Sender_service_no, 
                r.In_location_name, 
                r.Out_location_name, 
                r.Receiver_service_no, 
                r.Created_date, 
                r.ExO_service_no, 
                r.Carrier_nic_no, 
                CONCAT(tu.Employee_initials, ' ', tu.Employee_firstname, ' ', tu.Employee_surname) AS Name 
            FROM 
                Requests r
                INNER JOIN TempUsers tu ON r.Sender_service_no = tu.Employee_number
            WHERE 
                r.Request_ref_no IN (SELECT Request_ref_no FROM Workprogress WHERE Stage_id = @stageId)";

                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@stageId", stageId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                VerifyModel request = new VerifyModel
                                {
                                    Request_ref_no = reader.GetInt32(0),
                                    Sender_service_no = reader.GetString(1),
                                    In_location_name = reader.GetString(2),
                                    Out_location_name = reader.GetString(3),
                                    Receiver_service_no = reader.IsDBNull(4) ? "No Specific Receiver" : reader.GetString(4),
                                    Created_date = reader.GetDateTime(5),
                                    ExO_service_no = reader.GetString(6),
                                    Carrier_nic_no = reader.IsDBNull(7) ? "No Specific Carrier" : reader.GetString(7),
                                    Name = reader.GetString(8)
                                };

                                requests.Add(request);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred while retrieving requests with stage_id {stageId}.");
                    // Handle the error as needed, e.g., return an error view or redirect to an error page.
                }
            }

            return requests;
        }
    }
}
