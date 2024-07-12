using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BuildBazaar.Models;
using BCrypt.Net;
using MySql.Data.MySqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IO;

namespace BuildBazaar.Controllers
{
    public class HomeController : Controller
    {
        private readonly string CONNECTIONSTRING = "server=localhost;port=3306;user=root;password=password;database=bazaarDB;";
        private readonly string SECRETKEY = "Fkuw4aEYKFJM&X&RhZGuv^6oNCJ35^7f";
        private readonly string LOCALHOST = "https://localhost:44327/";
        //private readonly string UPLOADPATH = "~/Users";
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Builds()
        {
            return View();
        }

        private void CreateExampleBuild(int userID, MySqlConnection connection)
        {
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    var buildName = "Example Build";
                    var buildQuery = "INSERT INTO Builds (buildName, userID) VALUES (@buildName, @userID); SELECT LAST_INSERT_ID()";
                    int buildID;
                    using (MySqlCommand command = new MySqlCommand(buildQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@buildName", buildName);
                        command.Parameters.AddWithValue("@userID", userID);
                        buildID = Convert.ToInt32(command.ExecuteScalar());
                    }

                    // Paths and filenames
                    var exampleImageFileName = "kirac.png";
                    var userImageFileName = $"{Guid.NewGuid()}.png"; // Generate a unique filename for the user's copy
                    var contentImagePath = Server.MapPath("~/Content/ExampleBuild/" + exampleImageFileName);
                    var userImageFilePath = "../Users/" + userID + "/Thumbnails/" + userImageFileName;
                    var userImageSavePath = Server.MapPath(userImageFilePath);
                    var userBuildPath = Server.MapPath("~/Users/" + userID + "/" + buildID);
                    var exampleNotesFilePath = Server.MapPath("~/Content/ExampleBuild/notes.txt");

                    // Insert image record
                    var imageQuery = "INSERT INTO Images (filePath, imageOrder, typeID, buildID, userID) VALUES (@filePath, 0, 1, @buildID, @userID)";
                    using (MySqlCommand command = new MySqlCommand(imageQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@filePath", userImageFilePath);
                        command.Parameters.AddWithValue("@buildID", buildID);
                        command.Parameters.AddWithValue("@userID", userID);
                        command.ExecuteNonQuery();
                    }

                    var noteFilePath = "../Users/" + userID + "/" + buildID + "/notes.txt";
                    var noteSavePath = Server.MapPath(noteFilePath);
                    var noteQuery = "INSERT INTO Notes (filePath, buildID) VALUES (@filePath, @buildID)";
                    using (MySqlCommand command = new MySqlCommand(noteQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@filePath", noteFilePath);
                        command.Parameters.AddWithValue("@buildID", buildID);
                        command.ExecuteNonQuery();
                    }

                    var userBuildImagesFilePath = "../Users/" + userID + "/" + buildID;
                    var exampleImagesPath = Server.MapPath("~/Content/ExampleBuild/images");
                    int orderCounter = 0;
                    foreach (var filePath in Directory.GetFiles(exampleImagesPath))
                    {
                        var fileName = Path.GetFileName(filePath);
                        var userFilePath = userBuildImagesFilePath + "/" + fileName;

                        imageQuery = "INSERT INTO Images (filePath, imageOrder, typeID, buildID, userID) VALUES (@filePath, @imageOrder, 2, @buildID, @userID)";
                        using (MySqlCommand command = new MySqlCommand(imageQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@filePath", userFilePath);
                            command.Parameters.AddWithValue("@imageOrder", orderCounter);
                            command.Parameters.AddWithValue("@buildID", buildID);
                            command.Parameters.AddWithValue("@userID", userID);
                            
                            command.ExecuteNonQuery();
                        }
                        orderCounter++;
                    }

                    transaction.Commit();

                    // Create directory if it doesn't exist
                    Directory.CreateDirectory(Path.GetDirectoryName(userImageSavePath));
                    // Copy example image to user-specific folder
                    System.IO.File.Copy(contentImagePath, userImageSavePath);

                    var combinedNotesPath = Path.Combine(userBuildPath, "notes.txt");
                    Directory.CreateDirectory(Path.GetDirectoryName(combinedNotesPath));
                    System.IO.File.Copy(exampleNotesFilePath, combinedNotesPath);
                    foreach (var filePath in Directory.GetFiles(exampleImagesPath))
                    {
                        var fileName = Path.GetFileName(filePath);
                        System.IO.File.Copy(filePath, Path.Combine(userBuildPath, fileName));
                    }

                 
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

        public ActionResult CreateBuild(BuildModel build)
        {
            JwtSecurityToken token = ValidateToken();
            if (token != null)
            {
                int buildID =-1;
                using (MySqlConnection connection = new MySqlConnection(CONNECTIONSTRING))
                {
                    var userIDClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            if (Request.Files.Count > 0)
                            {
                                HttpPostedFileBase file = Request.Files[0];
                                if (file != null && file.ContentLength > 0)
                                {
                                    var buildQuery = "INSERT INTO Builds (buildName, userID) VALUES (@buildName, @userID); SELECT LAST_INSERT_ID()";
                                    using (MySqlCommand command = new MySqlCommand(buildQuery, connection, transaction))
                                    {
                                        command.Parameters.AddWithValue("@buildName", build.buildName);
                                        command.Parameters.AddWithValue("@userID", userIDClaim);
                                        //buildID = Convert.ToInt32(command.ExecuteNonQuery());
                                        buildID = Convert.ToInt32(command.ExecuteScalar());
                                    }
                                                                        
                                    //using (MySqlCommand command = new MySqlCommand("SELECT LAST_INSERT_ID()", connection, transaction))
                                    //{
                                    //    buildID = Convert.ToInt32(command.ExecuteScalar());
                                    //}

                                    var imageFileName = Path.GetFileName(file.FileName);
                                    var imageFilePath = "../Users/" + userIDClaim + "/Thumbnails/" + imageFileName;
                                    var imageSavePath = Server.MapPath(imageFilePath);
                                    var imageQuery = "INSERT INTO Images (filePath, typeID, buildID, userID) VALUES (@filePath, 1, @buildID, @userID); SELECT LAST_INSERT_ID()";
                                    using (MySqlCommand command = new MySqlCommand(imageQuery, connection, transaction))
                                    {
                                        command.Parameters.AddWithValue("@filePath", imageFilePath);
                                        command.Parameters.AddWithValue("@buildID", buildID);
                                        command.Parameters.AddWithValue("@userID", userIDClaim);
                                        command.ExecuteNonQuery();
                                    }

                                    var noteFilePath = "../Users/" + userIDClaim + "/" + buildID + "/notes.txt";
                                    var noteSavePath = Server.MapPath(noteFilePath);
                                    var noteQuery = "INSERT INTO Notes (filePath, buildID) VALUES (@filePath, @buildID); SELECT LAST_INSERT_ID()";
                                    using (MySqlCommand command = new MySqlCommand(noteQuery, connection, transaction))
                                    {
                                        command.Parameters.AddWithValue("@filePath", noteFilePath);
                                        command.Parameters.AddWithValue("@buildID", buildID);
                                        command.ExecuteNonQuery();
                                    }

                                    transaction.Commit();

                                    Directory.CreateDirectory(Path.GetDirectoryName(imageSavePath));
                                    file.SaveAs(imageSavePath);

                                    Directory.CreateDirectory(Path.GetDirectoryName(noteSavePath));
                                    using (var sw = new StreamWriter(noteSavePath,false))
                                    {
                                        sw.WriteLine(build.buildName + " notes here!");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw ex;
                        }
                    }

                }
                return Json(new { success = true, message = buildID});
            }
            else
            {
                // Token does not exist, redirect to login page
                return Json(new { success = false, message = "Invalid Token." });
            }
        }

        public ActionResult EditBuild(BuildModel build)
        {
            JwtSecurityToken token = ValidateToken();
            if (token != null)
            {
                using (MySqlConnection connection = new MySqlConnection(CONNECTIONSTRING))
                {
                    var userIDClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Check if the build exists and belongs to the user
                            var checkBuildQuery = "SELECT COUNT(*) FROM Builds WHERE buildID = @buildID AND userID = @userID";
                            int buildCount;
                            using (MySqlCommand command = new MySqlCommand(checkBuildQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@buildID", build.buildID);
                                command.Parameters.AddWithValue("@userID", userIDClaim);
                                buildCount = Convert.ToInt32(command.ExecuteScalar());
                            }

                            if (buildCount == 1)
                            {
                                // Update build name if provided
                                if (!string.IsNullOrEmpty(build.buildName))
                                {
                                    var updateBuildQuery = "UPDATE Builds SET buildName = @buildName WHERE buildID = @buildID AND userID = @userID";
                                    using (MySqlCommand command = new MySqlCommand(updateBuildQuery, connection, transaction))
                                    {
                                        command.Parameters.AddWithValue("@buildName", build.buildName);
                                        command.Parameters.AddWithValue("@buildID", build.buildID);
                                        command.Parameters.AddWithValue("@userID", userIDClaim);
                                        command.ExecuteNonQuery();
                                    }
                                }

                                // Update build image if provided
                                if (Request.Files.Count > 0)
                                {
                                    HttpPostedFileBase file = Request.Files[0];
                                    if (file != null && file.ContentLength > 0)
                                    {
                                        var imageFileName = Path.GetFileName(file.FileName);
                                        var imageFilePath = "../Users/" + userIDClaim + "/Thumbnails/" + imageFileName;
                                        var imageSavePath = Server.MapPath(imageFilePath);

                                        var updateImageQuery = "UPDATE Images SET filePath = @filePath WHERE buildID = @buildID AND userID = @userID";
                                        using (MySqlCommand command = new MySqlCommand(updateImageQuery, connection, transaction))
                                        {
                                            command.Parameters.AddWithValue("@filePath", imageFilePath);
                                            command.Parameters.AddWithValue("@buildID", build.buildID);
                                            command.Parameters.AddWithValue("@userID", userIDClaim);
                                            command.ExecuteNonQuery();
                                        }

                                        Directory.CreateDirectory(Path.GetDirectoryName(imageSavePath));
                                        file.SaveAs(imageSavePath);
                                    }
                                }

                                transaction.Commit();
                            }
                            else
                            {
                                return Json(new { success = false, message = "Build not found or you do not have permission to edit this build." });
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw ex;
                        }
                    }
                }

                return Json(new { success = true });
            }
            else
            {
                // Token does not exist, redirect to login page
                return Json(new { success = false, message = "Invalid Token." });
            }
        }

        public ActionResult DeleteBuild(int buildID)
        {
            JwtSecurityToken token = ValidateToken();
            if (token != null)
            {
                using (MySqlConnection connection = new MySqlConnection(CONNECTIONSTRING))
                {
                    var userIDClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Check if the build exists and belongs to the user
                            var checkBuildQuery = "SELECT COUNT(*) FROM Builds WHERE buildID = @buildID AND userID = @userID";
                            int buildCount;
                            using (MySqlCommand command = new MySqlCommand(checkBuildQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@buildID", buildID);
                                command.Parameters.AddWithValue("@userID", userIDClaim);
                                buildCount = Convert.ToInt32(command.ExecuteScalar());
                            }

                            if (buildCount == 1)
                            {
                                // Delete from Images table
                                var deleteImagesQuery = "DELETE FROM Images WHERE buildID = @buildID AND userID = @userID";
                                using (MySqlCommand command = new MySqlCommand(deleteImagesQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@buildID", buildID);
                                    command.Parameters.AddWithValue("@userID", userIDClaim);
                                    command.ExecuteNonQuery();
                                }

                                // Delete from BuildUrls table
                                var deleteBuildUrlsQuery = "DELETE FROM BuildUrls WHERE buildID = @buildID";
                                using (MySqlCommand command = new MySqlCommand(deleteBuildUrlsQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@buildID", buildID);
                                    command.ExecuteNonQuery();
                                }

                                // Delete from Notes table
                                var deleteNotesQuery = "DELETE FROM Notes WHERE buildID = @buildID";
                                using (MySqlCommand command = new MySqlCommand(deleteNotesQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@buildID", buildID);
                                    command.ExecuteNonQuery();
                                }

                                // Delete from Builds table
                                var deleteBuildQuery = "DELETE FROM Builds WHERE buildID = @buildID AND userID = @userID";
                                using (MySqlCommand command = new MySqlCommand(deleteBuildQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@buildID", buildID);
                                    command.Parameters.AddWithValue("@userID", userIDClaim);
                                    command.ExecuteNonQuery();
                                }

                                // Commit the transaction
                                transaction.Commit();
                            }
                            else
                            {
                                return Json(new { success = false, message = "Build not found or you do not have permission to delete this build." });
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw ex;
                        }
                    }
                }

                return Json(new { success = true });
            }
            else
            {
                // Token does not exist, return an error message
                return Json(new { success = false, message = "Invalid Token." });
            }
        }



        public ActionResult GetBuilds()
        {
            JwtSecurityToken token = ValidateToken();
            if (token != null)
            {
                try
                {
                    List<BuildModel> builds = new List<BuildModel>();
                    using (MySqlConnection connection = new MySqlConnection(CONNECTIONSTRING))
                    {
                        var userIDClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                        connection.Open();
                        string query = "SELECT * FROM Images Join Builds ON Images.buildID = Builds.buildID  WHERE Images.userID = @userID AND Images.typeID = 1";
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@userID", userIDClaim);
                            using (MySqlDataReader reader = command.ExecuteReader())
                                while (reader.Read())
                                {
                                    BuildModel build = new BuildModel();
                                    build.buildID = Convert.ToUInt32(reader["buildID"]);
                                    build.userID = Convert.ToUInt32(reader["userID"]);
                                    build.imageID = Convert.ToUInt32(reader["imageID"]);
                                    build.buildName = reader["buildName"].ToString();
                                    build.filePath = reader["filePath"].ToString();
                                    builds.Add(build);
                                }
                        }
                    }
                    return Json(new { success = true, builds });
                }
                catch (MySqlException ex)
                {
                    return Json(new { success = false, errorMessage = ex.Message });
                }
            }
            else
            {
                // Token does not exist, redirect to login page
                return Json(new { success = false, message = "Invalid Token." });
            }
        }

        public ActionResult GetNote(int buildID)
        {
            JwtSecurityToken token = ValidateToken();
            if (token != null)
            {
                try
                {
                    NoteModel note = new NoteModel();
                    using (MySqlConnection connection = new MySqlConnection(CONNECTIONSTRING))
                    {
                        //var userIDClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                        connection.Open();
                        string query = "SELECT * FROM Notes WHERE Notes.buildID = @buildID LIMIT 1;";
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@buildID", buildID);
                            using (MySqlDataReader reader = command.ExecuteReader())
                                while (reader.Read())
                                {
                                    note.noteID = Convert.ToUInt32(reader["noteID"]);
                                    note.buildID = Convert.ToUInt32(reader["buildID"]);
                                    note.filePath = reader["filePath"].ToString();
                                }
                        }
                    }
                    return Json(new { success = true, note });
                }
                catch (MySqlException ex)
                {
                    return Json(new { success = false, errorMessage = ex.Message });
                }
            }
            else
            {
                // Token does not exist, redirect to login page
                return Json(new { success = false, message = "Invalid Token." });
            }
        }

        public ActionResult SetNote(int buildID, string noteContent)
        {
            JwtSecurityToken token = ValidateToken();
            if (token != null)
            {
                try
                {
                    string filePath = "";
                    using (MySqlConnection connection = new MySqlConnection(CONNECTIONSTRING))
                    {
                        //var userIDClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                        connection.Open();
                        string query = "SELECT * FROM Notes WHERE Notes.buildID = @buildID LIMIT 1;";
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@buildID", buildID);
                            using (MySqlDataReader reader = command.ExecuteReader())
                                while (reader.Read())
                                {
                                    filePath = reader["filePath"].ToString();
                                }
                        }
                    }
                    try
                    {
                        System.IO.File.WriteAllText(Server.MapPath(filePath), noteContent);

                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, errorMessage = ex.Message });
                    }
                    return Json(new { success = true });
                }
                catch (MySqlException ex)
                {
                    return Json(new { success = false, errorMessage = ex.Message });
                }
            }
            else
            {
                // Token does not exist, redirect to login page
                return Json(new { success = false, message = "Invalid Token." });
            }
        }

        public ActionResult UpdateBuildUrl(uint? buildUrlID, uint buildID, string buildUrl, string buildUrlName)
        {
            JwtSecurityToken token = ValidateToken();
            if (token != null)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(CONNECTIONSTRING))
                    {
                        connection.Open();
                        string query = string.Empty;

                        if (buildUrlID == null)
                        {
                            // Create new record
                            query = "INSERT INTO BuildUrls (buildID, buildUrl, buildUrlName) VALUES (@buildID, @buildUrl, @buildUrlName);";
                        }
                        else
                        {
                            // Update existing record
                            query = "UPDATE BuildUrls SET buildUrl = @buildUrl, buildUrlName = @buildUrlName WHERE buildUrlID = @buildUrlID;";
                        }

                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@buildUrl", buildUrl);
                            command.Parameters.AddWithValue("@buildUrlName", buildUrlName);

                            if (buildUrlID != null)
                            {
                                command.Parameters.AddWithValue("@buildUrlID", buildUrlID);
                            }
                            else
                            {
                                command.Parameters.AddWithValue("@buildID", buildID);
                            }

                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                return Json(new { success = true });
                            }
                            else
                            {
                                return Json(new { success = false, errorMessage = "No records updated." });
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    return Json(new { success = false, errorMessage = ex.Message });
                }
            }
            else
            {
                // Token does not exist, redirect to login page
                return Json(new { success = false, message = "Invalid Token." });
            }
        }

        public ActionResult GetBuildUrls(uint buildID)
        {
            JwtSecurityToken token = ValidateToken();
            if (token != null)
            {
                try
                {
                    List<BuildUrlModel> buildUrls = new List<BuildUrlModel>();

                    using (MySqlConnection connection = new MySqlConnection(CONNECTIONSTRING))
                    {
                        connection.Open();
                        string query = "SELECT buildUrlID, buildID, buildUrlName, buildUrl FROM BuildUrls WHERE buildID = @buildID";

                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@buildID", buildID);

                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    BuildUrlModel buildUrl = new BuildUrlModel
                                    {
                                        buildUrlID = Convert.ToUInt32(reader["buildUrlID"]),
                                        buildID = Convert.ToUInt32(reader["buildID"]),
                                        buildUrlName = reader["buildUrlName"].ToString(),
                                        buildUrl = reader["buildUrl"].ToString()
                                    };
                                    buildUrls.Add(buildUrl);
                                }
                            }
                        }
                    }

                    return Json(new { success = true, buildUrls });
                }
                catch (MySqlException ex)
                {
                    return Json(new { success = false, errorMessage = ex.Message });
                }
            }
            else
            {
                // Token does not exist, redirect to login page
                return Json(new { success = false, message = "Invalid Token." });
            }
        }

        public ActionResult DeleteBuildUrl(uint buildUrlID)
        {
            JwtSecurityToken token = ValidateToken();
            if (token != null)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(CONNECTIONSTRING))
                    {
                        connection.Open();

                        // Delete the record from the database
                        string query = "DELETE FROM BuildUrls WHERE buildUrlID = @buildUrlID;";
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@buildUrlID", buildUrlID);

                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                return Json(new { success = true });
                            }
                            else
                            {
                                return Json(new { success = false, errorMessage = "No records deleted." });
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    return Json(new { success = false, errorMessage = ex.Message });
                }
            }
            else
            {
                // Token does not exist, redirect to login page
                return Json(new { success = false, message = "Invalid Token." });
            }
        }

        public ActionResult UploadReferenceImage()
        {
            JwtSecurityToken token = ValidateToken();
            if (token != null)
            {
                using (MySqlConnection connection = new MySqlConnection(CONNECTIONSTRING))
                {
                    var userIDClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            if (Request.Files.Count > 0)
                            {
                                HttpPostedFileBase file = Request.Files[0];
                                if (file != null && file.ContentLength > 0)
                                {
                                    

                                    int buildID = int.Parse(Request.Form["buildID"]);
                                    var imageFileName = Path.GetFileName(file.FileName);
                                    var imageFilePath = "../Users/" + userIDClaim + "/" + buildID + "/" + imageFileName;
                                    var imageSavePath = Server.MapPath(imageFilePath);

                                    int maxOrder;
                                    var maxOrderQuery = "SELECT IFNULL(MAX(imageOrder), 0) FROM Images WHERE buildID = @buildID";
                                    using (MySqlCommand command = new MySqlCommand(maxOrderQuery, connection, transaction))
                                    {
                                        command.Parameters.AddWithValue("@buildID", buildID);
                                        maxOrder = Convert.ToInt32(command.ExecuteScalar());
                                    }

                                    var imageQuery = "INSERT INTO Images (filePath, imageOrder, typeID, buildID, userID) VALUES (@filePath, @imageOrder, 2, @buildID, @userID)";
                                    using (MySqlCommand command = new MySqlCommand(imageQuery, connection, transaction))
                                    {
                                        command.Parameters.AddWithValue("@filePath", imageFilePath);
                                        command.Parameters.AddWithValue("@buildID", buildID);
                                        command.Parameters.AddWithValue("@userID", userIDClaim);
                                        command.Parameters.AddWithValue("@imageOrder", maxOrder + 1);
                                        command.ExecuteNonQuery();
                                    }

                                    transaction.Commit();

                                    Directory.CreateDirectory(Path.GetDirectoryName(imageSavePath));
                                    file.SaveAs(imageSavePath);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw ex;
                        }
                    }

                }
                return Json(new { success = true });
            }
            else
            {
                // Token does not exist, redirect to login page
                return Json(new { success = false, message = "Invalid Token." });
            }
        }

        public ActionResult GetReferenceImages(int buildID)
        {
            JwtSecurityToken token = ValidateToken();
            if (token != null)
            {
                try
                {
                    List<ImageModel> images = new List<ImageModel>();
                    using (MySqlConnection connection = new MySqlConnection(CONNECTIONSTRING))
                    {
                        
                        var userIDClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                        connection.Open();
                        string query = "SELECT * FROM Images WHERE Images.userID = @userID AND Images.typeID = 2 AND buildID = @buildID";
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@userID", userIDClaim);
                            command.Parameters.AddWithValue("@buildID", buildID);
                            using (MySqlDataReader reader = command.ExecuteReader())
                                while (reader.Read())
                                {
                                    ImageModel image = new ImageModel();
                                    image.imageID = Convert.ToUInt32(reader["imageID"]);
                                    image.buildID = Convert.ToUInt32(reader["buildID"]);
                                    image.filePath = reader["filePath"].ToString();
                                    image.imageOrder = Convert.ToInt32(reader["imageOrder"]);
                                    images.Add(image);
                                }
                        }
                    }
                    return Json(new { success = true, images });
                }
                catch (MySqlException ex)
                {
                    return Json(new { success = false, errorMessage = ex.Message });
                }
            }
            else
            {
                // Token does not exist, redirect to login page
                return Json(new { success = false, message = "Invalid Token." });
            }
        }

        [HttpPost]
        public JsonResult SaveImageOrder(List<ImageOrder> newOrder, int buildID)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(CONNECTIONSTRING))
                {
                    connection.Open();

                    foreach (var item in newOrder)
                    {
                        var query = "UPDATE Images SET imageOrder = @imageOrder WHERE imageID = @imageID AND buildID = @buildID";
                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@imageOrder", item.imageOrder);
                            command.Parameters.AddWithValue("@imageID", item.imageID);
                            command.Parameters.AddWithValue("@buildID", buildID);
                            command.ExecuteNonQuery();
                        }
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        public ActionResult DeleteReferenceImage(uint imageID)
        {
            JwtSecurityToken token = ValidateToken();
            if (token != null)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(CONNECTIONSTRING))
                    {
                        var userIDClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                        connection.Open();

                        // Retrieve the file path from the database based on the imageID
                        string filePathQuery = "SELECT filePath FROM Images WHERE imageID = @imageID AND userID = @userID;";
                        using (MySqlCommand filePathCommand = new MySqlCommand(filePathQuery, connection))
                        {
                            filePathCommand.Parameters.AddWithValue("@imageID", imageID);
                            filePathCommand.Parameters.AddWithValue("@userID", userIDClaim);
                            string filePath = filePathCommand.ExecuteScalar()?.ToString();

                            // Delete the file from the server
                            if (!string.IsNullOrEmpty(filePath))
                            {
                                string fullPath = Server.MapPath(filePath);
                                if (System.IO.File.Exists(fullPath))
                                {
                                    System.IO.File.Delete(fullPath);
                                }
                            }
                        }

                        // Delete the record from the database
                        string query = "DELETE FROM Images WHERE imageID = @imageID AND userID = @userID;";
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@imageID", imageID);
                            command.Parameters.AddWithValue("@userID", userIDClaim);

                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                return Json(new { success = true });
                            }
                            else
                            {
                                return Json(new { success = false, errorMessage = "No records deleted." });
                            }
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    return Json(new { success = false, errorMessage = ex.Message });
                }
            }
            else
            {
                // Token does not exist, redirect to login page
                return Json(new { success = false, message = "Invalid Token." });
            }
        }

        private JwtSecurityToken ValidateToken()
        {
            var token = Request.Headers["Authorization"];
            if (token != null)
            {
                try
                {
                    // Validate token
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.UTF8.GetBytes(SECRETKEY);
                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = LOCALHOST,
                        ValidAudience = LOCALHOST,
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    }, out SecurityToken validatedToken);

                    return (JwtSecurityToken)validatedToken;
                }
                catch (Exception ex)
                {
                    JwtSecurityToken badToken = null;
                    return badToken;
                }
            }
            else
            {
                JwtSecurityToken badToken = null;
                return badToken;
            }
        }
       
        public ActionResult WebValidateToken()
        {
            JwtSecurityToken token = ValidateToken();
            if (token != null)
            {
                return Json(new { success = true});
            }
            else
            {
                return Json(new { success = false, message = "Invalid Token." });
            }
        }

        [HttpPost]
        public ActionResult CreateUser(UserModel user)
        {
            try
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.password);
                using (MySqlConnection connection = new MySqlConnection(CONNECTIONSTRING))
                {
                    connection.Open();

                    // check if email already exists
                    string query = "SELECT COUNT(*) FROM Users WHERE UPPER(email) = @email";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@email", user.email.ToUpper());
                        int count = Convert.ToInt32(command.ExecuteScalar());

                        if (count > 0)
                        {
                            // email already exists, return error message
                            return Json(new { success = false, message = "Email already registered." });
                        }
                    }

                    query = "SELECT COUNT(*) FROM Users WHERE UPPER(username) = @username";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", user.username.ToUpper());
                        int count = Convert.ToInt32(command.ExecuteScalar());

                        if (count > 0)
                        {
                            // username already exists, return error message
                            return Json(new { success = false, message = "Username already taken." });
                        }
                    }

                    //add user to db
                    query = "INSERT INTO Users (username, email, password) VALUES (@username, @email, @password)";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", user.username);
                        command.Parameters.AddWithValue("@email", user.email);
                        command.Parameters.AddWithValue("@password", hashedPassword);
                        command.ExecuteNonQuery();
                    }

                    // Retrieve the userID of the newly created user
                    int userID;
                    query = "SELECT LAST_INSERT_ID()";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        userID = Convert.ToInt32(command.ExecuteScalar());
                    }

                    // Create an example build for the new user
                    CreateExampleBuild(userID, connection);
                }

                return Json(new { success = true });
            }
            catch (MySqlException ex)
            {
                return Json(new { success = false, errorMessage = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Login(UserModel user)
        {
            try
            {
                // Get user from database using username or email
                using (MySqlConnection connection = new MySqlConnection(CONNECTIONSTRING))
                {
                    connection.Open();
                    string query = "SELECT * FROM Users WHERE UPPER(username) = @username";/* OR UPPER(email) = @email";*/
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        if (user.username != null)
                            command.Parameters.AddWithValue("@username", user.username.ToUpper());
                        else
                            command.Parameters.AddWithValue("@username", null);
                        //if (user.email != null) 
                        //    command.Parameters.AddWithValue("@email", user.email.ToUpper());
                        //else
                        //    command.Parameters.AddWithValue("@email", null);
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Verify password using bcrypt
                                if (BCrypt.Net.BCrypt.Verify(user.password, reader.GetString("password")))
                                {
                                    // Create claims for the user
                                    var claims = new[]
                                    {
                                        new Claim(ClaimTypes.NameIdentifier, reader.GetUInt32("userID").ToString()),
                                        new Claim(ClaimTypes.Name, reader.GetString("username")),
                                        new Claim(ClaimTypes.Email, reader.GetString("email"))
                                    };

                                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SECRETKEY));
                                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                                    // Generate token
                                    var token = new JwtSecurityToken(
                                        issuer: LOCALHOST,
                                        audience: LOCALHOST,
                                        claims: claims,
                                        expires: DateTime.Now.AddDays(30),
                                        signingCredentials: creds);

                                    // Log user in
                                    Session["UserId"] = reader.GetUInt32("userID");
                                    Session["Username"] = reader.GetString("username");
                                    Session["Email"] = reader.GetString("email");

                                    return Json(new { success = true, token = new JwtSecurityTokenHandler().WriteToken(token) });
                                }
                            }
                        }
                    }
                }
                // Login failed, return error message
                return Json(new { success = false, message = "Invalid username or password." });
            }
            catch (MySqlException ex)
            {
                return Json(new { success = false, errorMessage = ex.Message });
            }
        }
    }
}