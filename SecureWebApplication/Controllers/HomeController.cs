using DAL;
using Logic.Helpers;
using Microsoft.AspNetCore.Mvc;
using SecureWebApplication.Models;
using System.Data;
using System.Diagnostics;

namespace SecureWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Secret()
        {
            JwtHelper jwtHelper = new JwtHelper();
            try
            {
                var cookie = Request.Cookies["role"];
                if (cookie == null)
                {
                    return StatusCode(StatusCodes.Status401Unauthorized, "Je hebt geen Cookie ga naar Login pagina om eentje te verkrijgen.");
                }

                if (cookie.Length > 20)
                {
                    var token = jwtHelper.Verify(cookie);
                    string role = token.Issuer.ToString().Trim();
                    if (role == "Admin")
                    {
                        return View();
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status401Unauthorized, "Je hebt geen toegang tot deze pagina.");
                    }
                }
                else
                {
                    cookie = cookie.Trim();
                    if (cookie == "Admin")
                    {
                        return View();
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status401Unauthorized, "Je hebt geen toegang tot deze pagina.");
                    }
                }
                
                

                
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Server error");
            }

        }
        
        
        public IActionResult CheckConnection()
        {
            DbHelper connection = new DbHelper();

            try
            {
                connection.openConn();
                return StatusCode(200, "Connection succes");
            }
            catch (Exception ex)
            {

                return StatusCode(500, ex.Message);
            }
        }
    }
}