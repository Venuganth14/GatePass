using GatePass_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace GatePass_Project.Controllers
{
    public class MyRequestController : Controller
    {
        private readonly ILogger<MyRequestController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public MyRequestController(ILogger<MyRequestController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult MyRequest()
        {
            List<MyRequestModel> requests = new List<MyRequestModel>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT r.Request_ref_no, r.Sender_service_no, r.In_location_name, r.Out_location_name, " +
                        "r.Receiver_service_no, r.Created_date, r.ExO_service_no, r.Carrier_nic_no, " +
                        "CONCAT(tu.Employee_initials, ' ', tu.Employee_firstname, ' ', tu.Employee_surname) AS Name " +
               "FROM Requests r " +
               "INNER JOIN TempUsers tu ON r.Sender_service_no = tu.Employee_number " +
               "ORDER BY r.Created_date DESC";


                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MyRequestModel request = new MyRequestModel
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
            }
            return View(requests);
        }

        private List<MyRequestModel> GetDetailsById(int id)
        {
            List<MyRequestModel> itemsList = new List<MyRequestModel>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT i.Item_id, i.Item_serial_no, i.Item_name, i.Item_description, i.Returnable_status, " +
                               "i.Request_ref_no, i.Attaches " +
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
                                MyRequestModel request = new MyRequestModel
                                {
                                    Item_id = reader.GetInt32(0),
                                    Item_serial_no = reader.GetString(1),
                                    Item_name = reader.GetString(2),
                                    Item_Description = reader.GetString(3),
                                    Returnable_status = reader.GetString(4),
                                    Request_ref_no = reader.GetInt32(5),
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

        public IActionResult RequestStatus(int id)
        {
            MyRequestModel requestStatus = GetRequestStatusById(id);

            if (requestStatus == null)
            {
                return RedirectToAction("Error");
            }

            // Assuming you have the correct Request_ref_no available
            // If not, you might need to modify GetRequestStatusById method to include it in the returned MyRequestModel
            int requestRefNo = GetRequestRefNoById(id);
            requestStatus.Request_ref_no = requestRefNo;

            return View(requestStatus);
        }

        private MyRequestModel GetRequestStatusById(int id)
        {
            MyRequestModel requestStatus = new MyRequestModel();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Progress_status FROM Workprogress WHERE Request_ref_no = @id ORDER BY Update_date DESC";

                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                requestStatus.Progress_status = reader["Progress_status"] as string;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while retrieving request status.");
                }
            }

            return requestStatus;
        }

        private int GetRequestRefNoById(int id)
        {
            int requestRefNo = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Request_ref_no FROM Requests WHERE Request_ref_no = @id";

                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                requestRefNo = reader.GetInt32(0);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while retrieving request reference number.");
                }
            }

            return requestRefNo;
        }

        public IActionResult MyRequestItemDetail(int id)
        {
            List<MyRequestModel> items = GetDetailsById(id);

            if (items.Count == 0)
            {
                return RedirectToAction("Error");
            }

            return View(items);
        }
    }
}
