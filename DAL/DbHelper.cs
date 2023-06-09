using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class DbHelper
    {
        private readonly IConfiguration _config;
        protected SqlConnection connection;

        public DbHelper()
        {
            connection = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);
        }

        public bool openConn()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }   
        }
        
        public bool closeConn()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
