using Bank.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Configuration;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNet.SignalR.Infrastructure;
using MySql.Data.MySqlClient;

namespace Bank.Controllers
{
    public class HomeController : Controller
    {

        SqlCommand comm = new SqlCommand();
        SqlDataReader sdr;
        SqlConnection conn = new SqlConnection();

        List<Account> accounts = new List<Account>();

        public string acc_num;

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            conn.ConnectionString = Properties.Resources.ConnectionString;
        }

        [HttpPost]
        public ActionResult Index(IFormFile formFile)
        {

            var path = Path.Combine(
                             Directory.GetCurrentDirectory(), "UploadedFiles",
                              formFile.FileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                formFile.CopyToAsync(stream);
            }

            string[] contents = System.IO.File.ReadAllLines(path);

            string connectionString = Properties.Resources.ConnectionString;
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();

            foreach (string line in contents)
            {
                string[] split = line.Split(',');

                SqlCommand cmd = con.CreateCommand();
                cmd.CommandText = "INSERT INTO dat_files (id, reference, date, description ,credit_debit, balance, transaction_type, account_number) VALUES(@id, @reference, @date, @description, @credit_debit, @balance, @transaction_type, @account_number)";

                acc_num = split[6].ToString();

                cmd.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
                cmd.Parameters.AddWithValue("@reference", split[0]);
                cmd.Parameters.AddWithValue("@date", split[1]);
                cmd.Parameters.AddWithValue("@description", split[2]);
                cmd.Parameters.AddWithValue("@credit_debit", split[3]);
                cmd.Parameters.AddWithValue("@balance", split[4]);
                cmd.Parameters.AddWithValue("@transaction_type", split[5]);
                cmd.Parameters.AddWithValue("@account_number", split[6]);
                cmd.ExecuteNonQuery();

            }

            FetchData();

            con.Close();

            return View(accounts);

        }

        public IActionResult Index()
        {
            return View();
        }

        private void FetchData()
        {

            if (accounts.Count > 0)
            {
                accounts.Clear();
            }

            try
            {
                conn.Open();
                comm.Connection = conn;
                comm.CommandText = "SELECT * FROM dat_files WHERE account_number = " + acc_num;
                sdr = comm.ExecuteReader();

                while (sdr.Read())
                {

                    if (!accounts.Contains(new Account()
                    {
                        refernce = sdr["reference"].ToString(),
                        date = sdr["date"].ToString(),
                        description = sdr["description"].ToString(),
                        credit_debit = sdr["credit_debit"].ToString(),
                        balance = sdr["balance"].ToString(),
                        transaction_type = sdr["transaction_type"].ToString(),
                        account_number = sdr["account_number"].ToString()
                    }))
                    {
                        accounts.Add(new Account()
                        {
                            refernce = sdr["reference"].ToString(),
                            date = sdr["date"].ToString(),
                            description = sdr["description"].ToString(),
                            credit_debit = sdr["credit_debit"].ToString(),
                            balance = sdr["balance"].ToString(),
                            transaction_type = sdr["transaction_type"].ToString(),
                            account_number = sdr["account_number"].ToString()
                        });
                    }


                }

                conn.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}