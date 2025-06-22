using SplitwiseCloneWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace SplitwiseCloneWebAPI.Controllers
{
    [EnableCors(origins: "https://nice-sea-044c05c1e.6.azurestaticapps.net", headers: "*", methods: "*")]

    public class UsersController : ApiController
    {
        private AppDbContext db = new AppDbContext();
        ExpensesController objcommon = new ExpensesController();

        [HttpPost]
        [Route("api/Users/HelloWolrd")]
        public string HelloWolrd()
        {
            return "HelloWolrd";
        }
        // POST: api/Users/Register
        [HttpPost]
        [Route("api/Users/Register")]
        public IHttpActionResult Register(User user)
        {
            /*if (!ModelState.IsValid)
                return BadRequest("Invalid data.");*/

            // Check if email already exists
            //var existingUser = db.Users.FirstOrDefault(u => u.Email == user.Email);
            //if (existingUser != null)
            //    return BadRequest("Email already exists.");

            string str = "";
            str += " select * from UserMaster where Email = '" + user.Email + "' and IsDeleted = 'N'";
            DataTable DT = objcommon.GetVSDataTable(str, false);
            if(DT.Rows.Count > 0)
            {
                return BadRequest("Email already exists.");
            }
            else
            {
                str = "";
                str += " insert into UserMaster values(NEWID(),'" + user.Name + "','" + user.Email + "','" + user.PasswordHash + "','N',GETDATE()) ";
                objcommon.ExecuteMyQuery(str);
                return Ok("User registered successfully.");
            }
        }

        // POST: api/Users/Login
        [HttpPost]
        [Route("api/Users/Login")]
        public IHttpActionResult Login(User user)
        {
            /*var foundUser = db.Users
                .FirstOrDefault(u => u.Email == user.Email && u.PasswordHash == user.PasswordHash);*/

            string str = " select * from UserMaster where Email = '" + user.Email + "' and PasswordHash = '" + user.PasswordHash + "' and IsDeleted='N' ";
            DataTable DT = objcommon.GetVSDataTable(str, false);
            if (DT.Rows.Count > 0)
            {
                string token = Guid.NewGuid().ToString();
                return Ok(new
                {
                    Id = DT.Rows[0]["Id"].ToString(),
                    Name = DT.Rows[0]["Name"].ToString(),
                    Email = DT.Rows[0]["Email"].ToString(),
                    Token = token
                });
            }
            else
            {
                return Unauthorized();
            }
        }
        [HttpGet]
        [Route("api/Users")]
        public IHttpActionResult GetAllUsers()
        {
            var users = db.Users.Select(u => new {
                u.Id,
                u.Name,
                u.Email
            }).ToList();

            return Ok(users);
        }
    }
}