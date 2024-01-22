using GatePass_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace GatePass.Controllers
{
    public class ItemCategoryController : Controller
    {
        private readonly SqlConnection _connection;



        public ItemCategoryController(SqlConnection connection)
        {
            _connection = connection;
        }
        public IActionResult ItemCategory()
        {
            // Fetch data from the database for Item Categories excluding entries with Removed_date
            List<ItemCategoryModel> itemcategorieswithoutremoved = new List<ItemCategoryModel>();
            string fetchCategoriesSql = "SELECT Category_id, Category_name, Category_type, Created_date, Removed_date FROM Item_Category WHERE Removed_date IS NULL";

            using (SqlCommand cmd = new SqlCommand(fetchCategoriesSql, _connection))
            {
                _connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ItemCategoryModel category = new ItemCategoryModel
                        {
                            Category_id = Convert.ToInt32(reader["Category_id"]),
                            Category_name = reader["Category_name"].ToString(),
                            Category_type = reader["Category_type"].ToString(),
                            Created_date = (reader["Created_date"] != DBNull.Value) ? Convert.ToDateTime(reader["Created_date"]) : (DateTime?)null,
                            Removed_date = (reader["Removed_date"] != DBNull.Value) ? Convert.ToDateTime(reader["Removed_date"]) : (DateTime?)null
                        };
                        itemcategorieswithoutremoved.Add(category);
                    }
                }
            }
            _connection.Close();

            // Fetch data from the database for Item Categories excluding entries with Removed_date
            List<ItemCategoryModel> itemcategorywithremoved = new List<ItemCategoryModel>();
            string fetchCattegoriesSql = "SELECT Category_id, Category_name, Category_type, Created_date, Removed_date FROM Item_Category WHERE Removed_date IS NOT NULL";

            using (SqlCommand cmd = new SqlCommand(fetchCattegoriesSql, _connection))
            {
                _connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ItemCategoryModel category = new ItemCategoryModel
                        {
                            Category_id = Convert.ToInt32(reader["Category_id"]),
                            Category_name = reader["Category_name"].ToString(),
                            Category_type = reader["Category_type"].ToString(),
                            Created_date = (reader["Created_date"] != DBNull.Value) ? Convert.ToDateTime(reader["Created_date"]) : (DateTime?)null,
                            Removed_date = (reader["Removed_date"] != DBNull.Value) ? Convert.ToDateTime(reader["Removed_date"]) : (DateTime?)null
                        };
                        itemcategorywithremoved.Add(category);
                    }
                }
            }
            _connection.Close();
            // Pass the data to the view
            ViewBag.ItemCategoriesWithRemoved = itemcategorywithremoved;
            ViewBag.ItemCategoriesWithoutRemoved = itemcategorieswithoutremoved;
            return View("ItemCategory");

        }

        public IActionResult CreateNewCategory()
        {
            return View();
        }

        public IActionResult WithRemoved()
        {
            // Fetch data from the database for Item Categories excluding entries with Removed_date
            List<ItemCategoryModel> itemcategorywithremoved = new List<ItemCategoryModel>();
            string fetchCattegoriesSql = "SELECT Category_id, Category_name, Category_type, Created_date, Removed_date FROM Item_Category";

            using (SqlCommand cmd = new SqlCommand(fetchCattegoriesSql, _connection))
            {
                _connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ItemCategoryModel category = new ItemCategoryModel
                        {
                            Category_id = Convert.ToInt32(reader["Category_id"]),
                            Category_name = reader["Category_name"].ToString(),
                            Category_type = reader["Category_type"].ToString(),
                            Created_date = (reader["Created_date"] != DBNull.Value) ? Convert.ToDateTime(reader["Created_date"]) : (DateTime?)null,
                            Removed_date = (reader["Removed_date"] != DBNull.Value) ? Convert.ToDateTime(reader["Removed_date"]) : (DateTime?)null
                        };
                        itemcategorywithremoved.Add(category);
                    }
                }
            }
            _connection.Close();
            // Pass the data to the view
            ViewBag.ItemCategoriesWithRemoved = itemcategorywithremoved;

            return View("ItemCategory");
        }

        public IActionResult UploadCsv(IFormFile csvFile)
        {
            // Validation to check if CSV file selected
            if (csvFile == null || csvFile.Length == 0)
            {
                TempData["ItemMessage"] = "No CSV file selected for upload.";
                return RedirectToAction("ItemCategory");
            }

            try
            {

                // Getting the current date and time to fill the Created_date field
                DateTime createdDate = DateTime.Now;

                // using the StreamReader class to read characters from the csv file
                using (var streamReader = new StreamReader(csvFile.OpenReadStream()))
                {
                    try
                    {
                        while (!streamReader.EndOfStream)
                        {
                            var line = streamReader.ReadLine();
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                var columns = line.Split(',');

                                // Validation to make sure the CSV has two categories as required by the ItemCategory table
                                if (columns.Length == 2)
                                {
                                    string categoryName = columns[0].Trim();
                                    string categoryType = columns[1].Trim();


                                    string insertSql = "INSERT INTO Item_Category (Category_name, Category_type, Created_date) VALUES (@CategoryName, @CategoryType, @CreatedDate)";

                                    using (var sqlCommand = new SqlCommand(insertSql, _connection))
                                    {
                                        sqlCommand.Parameters.AddWithValue("@CategoryName", categoryName);
                                        sqlCommand.Parameters.AddWithValue("@CategoryType", categoryType);
                                        sqlCommand.Parameters.AddWithValue("@CreatedDate", createdDate); // Set the Created_date field

                                        _connection.Open();
                                        sqlCommand.ExecuteNonQuery();
                                        _connection.Close();
                                    }
                                }
                                else
                                {
                                    TempData["ItemMessage"] = "Invalid CSV file format. Item Category Data must contain two columns.";
                                    return RedirectToAction("ItemCategory");
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

                TempData["ItemMessage"] = "CSV data imported successfully.";
            }
            catch (Exception ex)
            {
                TempData["ItemMessage"] = "An error occurred while importing CSV data: " + ex.Message;
            }
            finally
            {
                _connection.Close();
            }

            return RedirectToAction("ItemCategory");
        }

        [HttpPost]
        public IActionResult AddItemCategory(ItemCategoryModel model)
        {
            try
            {
                // Category name and Category type validation
                if (string.IsNullOrWhiteSpace(model.Category_name))
                {
                    TempData["ItemMessage"] = "Category name cannot be empty.";
                    return RedirectToAction("ItemCategory");
                }


                if (string.IsNullOrWhiteSpace(model.Category_type))
                {
                    TempData["ItemMessage"] = "Category type cannot be empty.";
                    return RedirectToAction("ItemCategory");
                }

                // Getting the current system date and time
                DateTime currentDate = DateTime.Now;

                string sql = $"INSERT INTO Item_Category (Category_name, Category_type, Created_date) " +
                             $"VALUES (@CategoryName, @CategoryType, @CreatedDate)";

                using (SqlCommand command = new SqlCommand(sql, _connection))
                {
                    command.Parameters.AddWithValue("@CategoryName", model.Category_name);
                    command.Parameters.AddWithValue("@CategoryType", model.Category_type);
                    command.Parameters.AddWithValue("@CreatedDate", currentDate); // Set the Created_date column with the current date and time

                    _connection.Open();
                    command.ExecuteNonQuery();
                    TempData["ItemMessage"] = "Category created successfully.";
                }
            }
            catch (Exception ex)
            {
                TempData["ItemMessage"] = "An error occurred while creating the category: " + ex.Message;
            }
            finally
            {
                _connection.Close();
            }

            return RedirectToAction("ItemCategory");
        }



        // Delete method was created as a post to update the ID field properly
        [HttpPost]
        public IActionResult DeleteItemCategory(int categoryId)
        {
            try
            {
                // Get the current system date and time
                DateTime currentDate = DateTime.Now;

                // Update the "Removed_date" column with the current date
                string updateSql = "UPDATE Item_Category SET Removed_date = @RemovedDate WHERE category_id = @CategoryId";

                using (SqlCommand updateCommand = new SqlCommand(updateSql, _connection))
                {
                    updateCommand.Parameters.AddWithValue("@RemovedDate", currentDate);
                    updateCommand.Parameters.AddWithValue("@CategoryId", categoryId);

                    _connection.Open();
                    int rowsAffected = updateCommand.ExecuteNonQuery();
                    _connection.Close();

                    if (rowsAffected > 0)
                    {
                        TempData["ItemMessage"] = "Category Removed from List.";
                    }
                    else
                    {
                        // Category with the specified ID not found
                        TempData["ItemMessage"] = "Category with the specified ID not found.";
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during the update process
                TempData["ItemMessage"] = "An error occurred while removing category: " + ex.Message;
            }
            finally
            {
                _connection.Close();
            }

            return RedirectToAction("ItemCategory");
        }
    }
}
