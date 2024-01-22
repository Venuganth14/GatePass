using My_receipt_gate_pass.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace My_receipt_gate_pass.Controllers
{
    public class Myreceipt : Controller
    {
        private readonly ILogger<Myreceipt> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public Myreceipt(ILogger<Myreceipt> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult Index()
        {
            List<MyreceiptModel> requests = new List<MyreceiptModel>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT r.Request_ref_no, r.Sender_service_no, r.In_location_name, r.Out_location_name, " +
                        "r.Receiver_service_no, r.Created_date, r.ExO_service_no, r.Carrier_nic_no," +
                        "CONCAT(tu.Employee_initials, ' ', tu.Employee_firstname, ' ', tu.Employee_surname) AS Name " +
                        "FROM Requests r " +
                        "INNER JOIN TempUsers tu ON r.Sender_service_no = tu.Employee_number " +
                        "WHERE r.Receiver_service_no IS NOT NULL " +
                        "AND r.Request_ref_no IN (SELECT Request_ref_no FROM Workprogress WHERE Stage_id = 7 )";


                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MyreceiptModel request = new MyreceiptModel
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
                            };

                            requests.Add(request);
                        }
                    }
                }

                return View(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching data for Index.");
                return RedirectToAction("Error");
            }
        }

        public IActionResult myreceiptdetails(int id)
        {
            List<myreceiptdetailsModel> requests = new List<myreceiptdetailsModel>();


            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT i.Item_id, i.Item_serial_no, i.Item_name, i.Item_description, i.Returnable_status, i.Request_ref_no, i.Attaches " +
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
                                myreceiptdetailsModel request = new myreceiptdetailsModel
                                {
                                    Item_id = reader.GetInt32(0), // Include Item_id
                                    Item_serial_no = reader.GetString(1),
                                    Item_name = reader.GetString(2),
                                    Item_description = reader.GetString(3),
                                    Returnable_status = reader.GetString(4),
                                    Request_ref_no = reader.GetInt32(5), // Include Request_ref_no
                                    Attaches = reader["Attaches"] as byte[],
                                };

                                requests.Add(request);
                            }
                        }
                    }
                    return View(requests);

                }


                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while fetching data for myreceiptdetails.");
                    return RedirectToAction("Error");
                }
            }
        }


        [HttpPost]
        public IActionResult Receive(int requestRefNo)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string updateQuery = "UPDATE Workprogress SET Stage_id = 9, Update_date = GETDATE(), Progress_status = 'Rc Received' WHERE Request_ref_no = @requestRefNo";

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@requestRefNo", requestRefNo);
                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the Receive action.");
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
                    string updateQuery = "UPDATE Workprogress SET Stage_id = 10, Update_date = GETDATE(), Any_comment = @rejectComment, Progress_status = 'Rc Rejected' WHERE Request_ref_no = @requestRefNo";


                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@requestRefNo", requestRefNo);
                        command.Parameters.AddWithValue("@rejectComment", rejectComment);
                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the Reject action.");
                return RedirectToAction("Error");
            }
        }


    }
}