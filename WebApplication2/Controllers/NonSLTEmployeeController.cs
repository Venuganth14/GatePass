using GatePass_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;

namespace GatePass_Project.Controllers
{
    public class NonSLTEmployeeController : Controller
    {
        private readonly SqlConnection _connection;



        public NonSLTEmployeeController(SqlConnection connection)
        {
            _connection = connection;
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult CreateNewNonSLTEmployee()
        {

            // Fetch data from the database for Roles
            List<RolesModel> roles = new List<RolesModel>();
            string fetchRolesSql = "SELECT Role_id, Role_duty FROM Roles";

            using (SqlCommand cmd = new SqlCommand(fetchRolesSql, _connection))
            {
                _connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        RolesModel role = new RolesModel
                        {
                            Role_id = Convert.ToInt32(reader["Role_id"]),
                            Role_duty = reader["Role_duty"].ToString()
                        };
                        roles.Add(role);
                    }
                }
            }
            _connection.Close();

            // Pass the roles data to the view
            ViewBag.Roles = new SelectList(roles, "Role_id", "Role_duty");

            return View();
        }

        public IActionResult NonSLTEmployee()
        {
            // Fetch data from the database for Locations
            List<NonSLTEmployeeModel> nonemployees = new List<NonSLTEmployeeModel>();
            string fetchLocationsSql = "SELECT Non_slt_Id, Role_id, Non_slt_name, NIC FROM Non_SLT_Users";

            using (SqlCommand cmd = new SqlCommand(fetchLocationsSql, _connection))
            {
                _connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        NonSLTEmployeeModel nonemployee = new NonSLTEmployeeModel
                        {
                            Non_slt_Id = reader["Non_slt_Id"].ToString(),
                            Role_id = Convert.ToInt32(reader["Role_id"]),
                            Non_slt_name = reader["Non_slt_name"].ToString(),
                            NIC = reader["NIC"].ToString()
                        };
                        nonemployees.Add(nonemployee);
                    }
                }
            }
            _connection.Close();

            ViewBag.Nonemployees = nonemployees;

            return View();
        }

        [HttpPost]
        public IActionResult NewNonSLTEmployee(NonSLTEmployeeModel model)
        {
            // Validation to Check if any of the fields are empty or null
            if (string.IsNullOrWhiteSpace(model.Non_slt_name) || model.Role_id == 0 || string.IsNullOrWhiteSpace(model.NIC))
            {
                TempData["NonSLTMessage"] = "All fields must be filled out";
                return RedirectToAction("NonSLTEmployee");
            }

            // Use the new Loc_id in your insert operation
            string sql = "INSERT INTO Non_SLT_Users (Role_id, Non_slt_name, NIC) VALUES (@Role_id, @Non_slt_name, @NIC)";

            using (SqlCommand command = new SqlCommand(sql, _connection))
            {
                _connection.Open();
                command.Parameters.AddWithValue("@Non_slt_name", model.Non_slt_name);
                command.Parameters.AddWithValue("@Role_id", model.Role_id);
                command.Parameters.AddWithValue("@NIC", model.NIC);

                command.ExecuteNonQuery();
                TempData["NonSLTMessage"] = "Non-SLT Employee added successfully.";
                _connection.Close();
            }

            return RedirectToAction("NonSLTEmployee");
        }



        [HttpPost]
        public IActionResult DeleteNonSLTEmployee(int Non_slt_id)
        {
            string deleteSql = "DELETE FROM Non_SLT_Users WHERE Non_slt_Id = @NonSLTId";

            using (SqlCommand command = new SqlCommand(deleteSql, _connection))
            {
                command.Parameters.AddWithValue("@NonSLTId", Non_slt_id);

                try
                {
                    _connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        // Deletion was successful

                        TempData["NonSLTMessage"] = "Employee deleted successfully.";
                    }
                    else
                    {
                        // Category with the specified ID not found
                        TempData["NonSLTMessage"] = "Employee with the specified ID not found.";
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that may occur during the deletion process
                    TempData["NonSLTMessage"] = "An error occurred while deleting the employee: " + ex.Message;
                }
                finally
                {
                    _connection.Close();
                }
            }

            return RedirectToAction("NonSLTEmployee");
        }
    }
}