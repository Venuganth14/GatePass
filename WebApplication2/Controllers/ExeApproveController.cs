using GatePass_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using My_receipt_gate_pass.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace GatePass_Project.Controllers
{
    public class ExeApproveController : Controller
    {
        private readonly ILogger<ExeApproveController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public ExeApproveController(ILogger<ExeApproveController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        //public IActionResult ExeApprove()
        //{
        //    List<ExeApproveModel> requests = new List<ExeApproveModel>();

        //    using (SqlConnection connection = new SqlConnection(_connectionString))
        //    {
        //        connection.Open();
        //        string query = "SELECT * FROM Requests WHERE Request_ref_no IN (SELECT Request_ref_no FROM Workprogress WHERE Stage_id = 1) ORDER BY Created_date DESC";

        //        try
        //        {
        //            using (SqlCommand command = new SqlCommand(query, connection))
        //            {
        //                using (SqlDataReader reader = command.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        ExeApproveModel request = new ExeApproveModel
        //                        {
        //                            Request_ref_no = reader.GetInt32(0),
        //                            Sender_service_no = reader.GetString(1),
        //                            In_location_name = reader.GetString(2),
        //                            Out_location_name = reader.GetString(3),
        //                            Receiver_service_no = reader.GetString(4),
        //                            Created_date = reader.GetDateTime(5),
        //                            ExO_service_no = reader.GetString(6),
        //                            Carrier_nic_no = reader.GetString(7),
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

        public IActionResult ExeApprove()
        {
            List<ExeApproveModel> requests = new List<ExeApproveModel>();

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
                        "WHERE r.Request_ref_no IN (SELECT Request_ref_no FROM Workprogress WHERE Stage_id = 1) ORDER BY Created_date DESC";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ExeApproveModel request = new ExeApproveModel
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
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string updateQuery = "UPDATE Workprogress SET Stage_id = 2, Update_date = GETDATE(), Progress_status = 'Executive Approved', Viewed = 0 WHERE Request_ref_no = @requestRefNo";

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@requestRefNo", requestRefNo);
                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("ExeApprove");
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
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string updateQuery = "UPDATE Workprogress SET Stage_id = 3, Update_date = GETDATE(),  Any_comment = @rejectComment, Progress_status = 'Executive Rejected', Viewed = 0 WHERE Request_ref_no = @requestRefNo";

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@requestRefNo", requestRefNo);
                        command.Parameters.AddWithValue("@rejectComment", rejectComment);
                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("ExeApprove");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while rejecting the request.");
                return RedirectToAction("Error");
            }
        }

        public IActionResult PendingDetails(int id)
        {
            // Retrieve item details for the given Request_ref_no
            List<ExeApproveModel> items = GetDetailsById(id);

            if (items.Count == 0)
            {
                // Handle the case where no details are found for the given id, e.g., show an error message or redirect to an error page.
                return RedirectToAction("Error");
            }

            return View(items);
        }

        public IActionResult Pending()
        {
            List<ExeApproveModel> pendingRequests = GetRequestsByStageId(1);
            return View("ExeApprove", pendingRequests);
        }

        public IActionResult Approved()
        {
            List<ExeApproveModel> approvedRequests = GetRequestsByStageId(2);
            return View("ExeApprove", approvedRequests);
        }

        public IActionResult Rejected()
        {
            List<ExeApproveModel> rejectedRequests = GetRequestsByStageId(3);
            return View("ExeApprove", rejectedRequests);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Displaying Notifications when rejecting a request
        public IActionResult Notification()
        {
            int commentCount = GetCommentCount();
            ViewData["CommentCount"] = commentCount;

            List<Workprogress> workProgressData = GetWorkProgressDataWithComments();
            ViewData["WorkProgressData"] = workProgressData;

            return View("Notification");
        }

        // Retrieve the count of comments
        private int GetCommentCount()
        {
            int commentCount = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string countQuery = "SELECT COUNT(Viewed) as CommentCount FROM Workprogress WHERE Any_comment IS NOT NULL AND Viewed = 0";

                try
                {
                    using (SqlCommand command = new SqlCommand(countQuery, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                commentCount = reader.GetInt32(0);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while retrieving the comment count.");
                    // Handle the error as needed, e.g., return an error view or redirect to an error page.
                }
            }

            return commentCount;
        }


        // Retrieve work progress data with comments, ordered by Update_date in descending order
        private List<Workprogress> GetWorkProgressDataWithComments()
        {
            List<Workprogress> workProgressData = new List<Workprogress>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Request_ref_no, Stage_id, Progress_status, Update_date, Any_comment " +
                               "FROM Workprogress WHERE Any_comment IS NOT NULL AND Viewed = 0 " +
                               "ORDER BY Update_date DESC";  // Order by Update_date in descending order

                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Workprogress progress = new Workprogress
                                {
                                    Request_ref_no = reader.GetInt32(0),
                                    Stage_id = reader.GetInt32(1),
                                    Progress_status = reader.GetString(2),
                                    Update_date = reader.GetDateTime(3),
                                    Any_comment = reader.IsDBNull(4) ? null : reader.GetString(4)
                                };

                                workProgressData.Add(progress);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while retrieving work progress data with comments.");
                    // Handle the error as needed, e.g., return an error view or redirect to an error page.
                }
            }

            return workProgressData;
        }


        //updating the workprogress table viewed column as 1

        [HttpPost]
        public IActionResult MarkNotificationAsViewed(int requestRefNo)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string updateQuery = "UPDATE Workprogress SET Viewed = 1 WHERE Request_ref_no = @requestRefNo";

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@requestRefNo", requestRefNo);
                        command.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the Viewed column.");
                return Json(new { success = false, error = "An error occurred while updating the Viewed column." });
            }
        }



        // Retrieve item details for a given Request_ref_no
        private List<ExeApproveModel> GetDetailsById(int id)
        {
            List<ExeApproveModel> itemsList = new List<ExeApproveModel>();

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
                                ExeApproveModel request = new ExeApproveModel
                                {
                                    Item_id = reader.GetInt32(0), // Include Item_id
                                    Item_serial_no = reader.GetString(1),
                                    Item_name = reader.GetString(2),
                                    Item_Description = reader.GetString(3),
                                    Returnable_status = reader.GetString(4),
                                    Request_ref_no = reader.GetInt32(5), // Include Request_ref_no
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

        //        // Filtering according to the stageId
        //        private List<ExeApproveModel> GetRequestsByStageId(int stageId)
        //        {
        //            List<ExeApproveModel> requests = new List<ExeApproveModel>();

        //            using (SqlConnection connection = new SqlConnection(_connectionString))
        //            {
        //                connection.Open();
        //                string query = "SELECT * FROM Requests WHERE Request_ref_no IN (SELECT Request_ref_no FROM Workprogress WHERE Stage_id = @stageId)";

        //                try
        //                {
        //                    using (SqlCommand command = new SqlCommand(query, connection))
        //                    {
        //                        command.Parameters.AddWithValue("@stageId", stageId);

        //                        using (SqlDataReader reader = command.ExecuteReader())
        //                        {
        //                            while (reader.Read())
        //                            {
        //                                ExeApproveModel request = new ExeApproveModel
        //                                {
        //                                    Request_ref_no = reader.GetInt32(0),
        //                                    Sender_service_no = reader.GetString(1),
        //                                    In_location_name = reader.GetString(2),
        //                                    Out_location_name = reader.GetString(3),
        //                                    Receiver_service_no = reader.GetString(4),
        //                                    Created_date = reader.GetDateTime(5),
        //                                    ExO_service_no = reader.GetString(6),
        //                                    Carrier_nic_no = reader.GetString(7),
        //                                    Name = reader.GetString(8)
        //                                };

        //                                requests.Add(request);
        //                            }


        //                        }
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    _logger.LogError(ex, $"An error occurred while retrieving requests with stage_id {stageId}.");
        //                    // Handle the error as needed, e.g., return an error view or redirect to an error page.
        //                }
        //            }

        //            return requests;
        //        }
        //    }
        //}

        // Filtering according to the stageId
        private List<ExeApproveModel> GetRequestsByStageId(int stageId)
        {
            List<ExeApproveModel> requests = new List<ExeApproveModel>();

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
                                ExeApproveModel request = new ExeApproveModel
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
