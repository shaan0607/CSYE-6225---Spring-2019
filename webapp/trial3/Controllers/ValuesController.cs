using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using trial3.Controller.Model;
using System.Text.RegularExpressions;
using System.Globalization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using trial3;

namespace trial.Controllers
{
    [ApiController]
    public class ValuesController : ControllerBase
    {
        public static Dictionary<String,User> userDetails = new Dictionary<String, User>();
        // GET api/values

        
        [HttpGet]
        
        [Route("/")]
        [Authorize]
        
        public System.DateTime Get()
        {
            return DateTime.Now;
        }
        [HttpPost]
        [Route("/token")]
        public ActionResult GetToken()
        {
            //Security Key
            string securityKey = "security_key$dfsf";

            //Symmetric security key
            var symmetriSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));

            //siging credential 
            var signingCredentials = new SigningCredentials(symmetriSecurityKey, SecurityAlgorithms.HmacSha256Signature);

            //create token
            var token = new JwtSecurityToken(
                issuer: "smesk.in",
                audience: "readers",
                expires: DateTime.Now.AddHours(1),
                signingCredentials : signingCredentials
                );
            //return token

            return Ok(new JwtSecurityTokenHandler().WriteToken(token));

        }
        [HttpPost]
        [Route("/user/register")]
        public ActionResult signup([FromBody] User u)
        {

            if(!userDetails.ContainsKey(u.email))
            {
                if(ModelState.IsValid){

                if (string.IsNullOrWhiteSpace(u.email))
               { return BadRequest();}
                userDetails.Add(u.email, u);
                var s =  GetToken();
                return s;  
                }
                return BadRequest();
            }
        return StatusCode(409);
        // eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE1NDg2MTY0NzIsImlzcyI6InNtZXNrLmluIiwiYXVkIjoicmVhZGVycyJ9.AGZncdwXyGDXt6p6Doq8Ec1bWC6_GnaR6H7rokXGQ7o
        // eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE1NDg2MTY1NTUsImlzcyI6InNtZXNrLmluIiwiYXVkIjoicmVhZGVycyJ9.6m4Ck2OS7qsHtcFElM27qPFDHE5CfVsNvBEgPApZyIs
        }
    }
}
