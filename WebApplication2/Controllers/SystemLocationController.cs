using GatePass_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Diagnostics;

namespace GatePass.Controllers
{
    public class SystemLocationController : Controller
    {
        private readonly SqlConnection _connection;



        public SystemLocationController(SqlConnection connection)
        {
            _connection = connection;
        }
        // GET: SystemLocationController
        public IActionResult SystemLocation()
        {
            // Fetch data from the database for Locations
            List<LocationsModel> locations = new List<LocationsModel>();
            string fetchLocationsSql = "SELECT Loc_id, Loc_name FROM Locations WHERE IsDeleted IS NULL";

            using (SqlCommand cmd = new SqlCommand(fetchLocationsSql, _connection))
            {
                _connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
#pragma warning disable CS8601 // Possible null reference assignment.
                        LocationsModel location = new LocationsModel
                        {
                            Loc_id = Convert.ToInt32(reader["Loc_id"]),
                            Loc_name = reader["Loc_name"].ToString()
                        };
#pragma warning restore CS8601 // Possible null reference assignment.
                        locations.Add(location);
                    }
                }
            }
            _connection.Close();

            ViewBag.Locations = locations;

            return View();
        }

        public IActionResult CreateSystemLocation()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UploadLocationCSV(IFormFile csvFile)
        {
            // Validation to check if csv file selected
            if (csvFile == null || csvFile.Length == 0)
            {
                TempData["LocationMessage"] = "No CSV file selected for upload.";
                return RedirectToAction("SystemLocation");
            }

            try
            {
                // Using the StreamReader class to read characters from the csv file
                using (var streamReader = new StreamReader(csvFile.OpenReadStream()))
                {
                    try
                    {
                        while (!streamReader.EndOfStream)
                        {
                            var line = streamReader.ReadLine();
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                // Validation to check if the CSV file has more than one column
                                if (line.Split(',').Length != 1)
                                {
                                    TempData["LocationMessage"] = "Invalid CSV file format. Location Data can only contain Location Name.";
                                    return RedirectToAction("SystemLocation");
                                }

                                // using trim to get rid of any whitespaces
                                string locName = line.Trim();

                                // Check if the location already exists
                                if (LocationExists(locName))
                                {
                                    TempData["LocationMessage"] = $"Location '{locName}' already exists. Duplicate values are not allowed.";
                                    return RedirectToAction("SystemLocation");
                                }

                                string insertSql = "INSERT INTO Locations (Loc_name) VALUES (@LocName)";

                                using (var sqlCommand = new SqlCommand(insertSql, _connection))
                                {
                                    sqlCommand.Parameters.AddWithValue("@LocName", locName);

                                    _connection.Open();
                                    sqlCommand.ExecuteNonQuery();
                                    _connection.Close();
                                }
                            }
                        }
                    }
                    finally
                    {
                        // Making sure the connection is closed even if an exception occurs during CSV processing.
                        _connection.Close();
                    }
                }

                TempData["LocationMessage"] = "CSV data imported successfully into Locations.";
            }
            catch (Exception ex)
            {
                TempData["LocationMessage"] = "An error occurred while importing CSV data into Locations: " + ex.Message;
            }
            finally
            {
                _connection.Close();
            }

            return RedirectToAction("SystemLocation");
        }

        // Helper method to check if the location already exists in the database
        private bool LocationExists(string locName)
        {
            string query = "SELECT COUNT(*) FROM Locations WHERE Loc_name = @LocName";
            using (var sqlCommand = new SqlCommand(query, _connection))
            {
                sqlCommand.Parameters.AddWithValue("@LocName", locName);
                _connection.Open();
                int count = (int)sqlCommand.ExecuteScalar();
                _connection.Close();
                return count > 0;
            }
        }


        [HttpPost]
        public IActionResult NewSystemLocation(LocationsModel model)
        {
            // Validation to Check if the Loc_name is empty or null
            if (string.IsNullOrWhiteSpace(model.Loc_name))
            {
                TempData["LocationMessage"] = "Location name cannot be empty.";
                return RedirectToAction("SystemLocation");
            }

            // Check if the location name already exists in the database
            string checkSql = "SELECT COUNT(*) FROM Locations WHERE Loc_name = @Loc_name";

            using (SqlCommand checkCommand = new SqlCommand(checkSql, _connection))
            {
                checkCommand.Parameters.AddWithValue("@Loc_name", model.Loc_name);
                _connection.Open();

                int existingCount = (int)checkCommand.ExecuteScalar();

                _connection.Close();

                if (existingCount > 0)
                {
                    TempData["LocationMessage"] = "Location with the same name already exists.";
                    return RedirectToAction("SystemLocation");
                }
            }

            // Use the new Loc_id in your insert operation
            string sql = "INSERT INTO Locations (Loc_name) VALUES (@Loc_name)";

            using (SqlCommand command = new SqlCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@Loc_name", model.Loc_name);

                _connection.Open();
                command.ExecuteNonQuery();
                TempData["LocationMessage"] = "Location created successfully.";
                _connection.Close();
            }

            return RedirectToAction("SystemLocation");
        }



        [HttpPost]
        public IActionResult DeleteLocation(int locationId)
        {
            string updateSql = "UPDATE Locations SET IsDeleted = 1 WHERE Loc_id = @LocationId";

            using (SqlCommand command = new SqlCommand(updateSql, _connection))
            {
                command.Parameters.AddWithValue("@LocationId", locationId);

                try
                {
                    _connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        // Soft deletion was successful
                        TempData["LocationMessage"] = "Location soft-deleted successfully.";
                    }
                    else
                    {
                        // Location with the specified ID not found
                        TempData["LocationMessage"] = "Location with the specified ID not found.";
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that may occur during the soft deletion process
                    TempData["LocationMessage"] = "An error occurred while soft-deleting the location: " + ex.Message;
                }
                finally
                {
                    _connection.Close();
                }
            }

            return RedirectToAction("SystemLocation");
        }






        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}