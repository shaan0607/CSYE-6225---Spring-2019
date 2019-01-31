using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
//using trial3.Controller.Model;
using System.Text.RegularExpressions;
using System.Globalization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using trial3;
using BCrypt.Net;

namespace trial.Controllers
{
    public class ValuesController : ControllerBase
    {
       
        // GET api/values

        
        [HttpGet]
        
        [Route("/")]
        [Authorize]
        
        public System.DateTime Get()
        {
            return DateTime.Now;
        }
        private CLOUD_CSYEContext _context;

        public ValuesController(CLOUD_CSYEContext context)
        {
            _context = context;
           // _context.Database.EnsureCreated();
        }
         public string GetRandomSalt()
        {
          
           return BCrypt.Net.BCrypt.GenerateSalt(12); ;
        }
        public string HashPassword(string password)
        {
            
            return  BCrypt.Net.BCrypt.HashPassword(password, GetRandomSalt());

            //return hashedPassword;
            // return BCrypt.HashPassword(password, GetRandomSalt());
        }

        [HttpPost]
        [Route("/user/register")]
        public ActionResult signup([FromBody] Users u)
        {

           Users us =  _context.Users.Find(u.Email);
            if(us == null){
                if(ModelState.IsValid){
                
                if (string.IsNullOrWhiteSpace(u.Email))
               { return StatusCode(400, "Something Went Wrong");}
                var s = HashPassword(u.Password);
                var user = new Users{Email= u.Email, Password = s};
                _context.Add(user);
                _context.SaveChanges();
                var Created = "User Created Successfully";
                return StatusCode(201, new {result = Created});
                }
                return StatusCode(400, "Something Went Wrong");
            }
            else{
                var conflict = "Email Already exists";
                return StatusCode(409, new{ result = conflict});
            }}
    }
}