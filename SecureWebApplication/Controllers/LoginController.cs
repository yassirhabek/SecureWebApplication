using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Logging;
using SecureWebApplication.Models;
using SecureWebApplication.Helper;
using LogHelper = SecureWebApplication.Helper.LogHelper;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Logic.Helpers;

namespace SecureWebApplication.Controllers
{

    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly IConfiguration _config;
        public LoginController(ILogger<LoginController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }
            
        public IActionResult LoginView()
        {
            return View();
        }

        public IActionResult LoginUnsafe(string username, string password)
        {
            LoginModel Login = new LoginModel();
            Login.Username = username;
            Login.Password = password;

            using (SqlConnection sqlConnection = new SqlConnection("Server=mssqlstud.fhict.local;Database=dbi485050_semester4;User Id=dbi485050_semester4;Password=student;TrustServerCertificate=True;"))
            {
                string cmd = "SELECT [UserName] FROM [dbi485050_semester4].[dbo].[User] WHERE [Username] = '"
                    + Login.Username
                    + "' AND [Password]='"
                    + Login.Password
                    + "' ";
                try
                {
                    using (SqlCommand sqlCommand = new SqlCommand(cmd, sqlConnection))
                    {
                        sqlConnection.Open();
                        if (sqlCommand.ExecuteScalar() == null)
                        {
                            Login.Message = "Invalid Login.";
                            new Exception(Login.Message);
                            sqlConnection.Close();
                            return StatusCode(StatusCodes.Status401Unauthorized, Login.Message);
                        }
                        else
                        {
                            string Username = sqlCommand.ExecuteScalar().ToString();
                            sqlConnection.Close();
                            Response.Cookies.Append("role", GetUserRole(Login.Username, Login.Password, false));
                            return Ok(Login.Message = Username);
                        }
                    }
                }
                catch (SqlException ex)
                {
                    sqlConnection.Close();
                    return StatusCode(StatusCodes.Status500InternalServerError, Login.Message = ex.Message.ToString());
                }
            }
        }

        public IActionResult LoginSafe(string username, string password)
        {
            LoginModel Login = new LoginModel();
            if (username == null || password == null)
            {
                Login.Message = "Missing Username/Password.";
                return StatusCode(StatusCodes.Status400BadRequest, Login.Message);
            }
            Login.Username = username;
            Login.Password = password;
            LogHelper logHelper = new LogHelper();

            // Establish a connection to the database
            using (SqlConnection sqlConnection = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]))
            {
                // SQL command to retrieve the username from the User table
                string cmd = "SELECT [UserName] FROM [dbo].[User] WHERE [Username] = @user AND [Password]= @pass";
                try
                {
                    // Execute the SQL command
                    using (SqlCommand sqlCommand = new SqlCommand(cmd, sqlConnection))
                    {
                        sqlCommand.Parameters.AddWithValue("@user", Login.Username);
                        sqlCommand.Parameters.AddWithValue("@pass", Login.Password);

                        sqlConnection.Open();

                        // Check if the login is invalid
                        if (sqlCommand.ExecuteScalar() == null)
                        {
                            Login.Message = "Invalid Login.";

                            // Create an exception with the login message
                            Exception exception = new Exception(Login.Message);

                            // Close the database connection
                            sqlConnection.Close();

                            // Return an Unauthorized status code with the login message
                            return StatusCode(StatusCodes.Status401Unauthorized, Login.Message);
                        }
                        else
                        {
                            // Retrieve the username from the SQL command
                            string Username = sqlCommand.ExecuteScalar().ToString();

                            // Close the database connection
                            sqlConnection.Close();

                            JwtHelper jwtHelper = new JwtHelper();
                            string token = jwtHelper.Generate(GetUserRole(Login.Username, Login.Password, true));
                            Response.Cookies.Append("role", token, new CookieOptions
                            {
                                HttpOnly = true,
                                SameSite = SameSiteMode.Strict,
                                Secure = true
                            });

                            // Return a successful response with a welcome message
                            return Ok(Login.Message = "Welcome " + Username);
                        }
                    }
                }
                catch (SqlException ex)
                {
                    // Close the database connection
                    sqlConnection.Close();

                    // Create a log entry for the SQL   
                    logHelper.CreateLog(ex, System.Diagnostics.EventLogEntryType.Error);

                    // Return a server error status code
                    return StatusCode(StatusCodes.Status500InternalServerError, "Server Error");
                }
            }
        }

        private string GetUserRole(string username, string password, bool isSafe)
        {
            LoginModel Login = new LoginModel();
            Login.Username = username;
            Login.Password = password;
            string cmd;
            
            // Establish a connection to the database
            using (SqlConnection sqlConnection = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]))
            {
                // SQL command to retrieve the username from the User table
                if (isSafe == true)
                {
                    cmd = "SELECT [Role] FROM [dbo].[User] WHERE [Username] = @user AND [Password]= @pass";
                }
                else
                {
                    cmd = "SELECT [UserName] FROM [dbi485050_semester4].[dbo].[User] WHERE [Username] = '"
                        + Login.Username
                        + "' AND [Password]='"
                        + Login.Password
                        + "' ";
                }
                
                try
                {
                    // Execute the SQL command
                    using (SqlCommand sqlCommand = new SqlCommand(cmd, sqlConnection))
                    {
                        if (isSafe == true)
                        {
                            sqlCommand.Parameters.AddWithValue("@user", Login.Username);
                            sqlCommand.Parameters.AddWithValue("@pass", Login.Password);
                        }

                        sqlConnection.Open();

                        // Check if the login is invalid
                        if (sqlCommand.ExecuteScalar() == null)
                        {
                            Login.Message = "Invalid Login.";

                            // Create an exception with the login message
                            Exception exception = new Exception(Login.Message);

                            // Close the database connection
                            sqlConnection.Close();
                            return Login.Message;
                        }
                        else
                        {
                            // Retrieve the username from the SQL command
                            string Role = sqlCommand.ExecuteScalar().ToString();

                            // Close the database connection
                            sqlConnection.Close();


                            // Return a successful response with a welcome message
                            return Role;
                        }
                    }
                }
                catch (SqlException ex)
                {
                    // Close the database connection
                    sqlConnection.Close();


                    // Return a server error status code
                    throw new Exception();
                }
            }
        }
    }
}       