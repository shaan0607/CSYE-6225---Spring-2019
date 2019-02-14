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
using trial3.Authentication;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;

namespace trial3.Controllers
{
    public class ValuesController : ControllerBase
    {
       
        // GET api/values


        public string getUsername(){
            
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
            var username = credentials[0];
            return username;
        }
        
        [HttpGet]
        
        [Route("/")]
        [Authorize]
        
        public ActionResult Get()
        {
            return StatusCode(200, new{result  = DateTime.Now});
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
               { 
                return StatusCode(400, new{ result ="Something Went Wrong"} );}
                var s = HashPassword(u.Password);
                var user = new Users{Email= u.Email, Password = s};
                _context.Add(user);
                _context.SaveChanges();
                var Created = "User Created Successfully";
                return StatusCode(201, new {result = Created});
                }
                
                return StatusCode(400, new{ result =  "Something Went Wrong"} );
            }
            else{
                var conflict = "Email Already exists";
                return StatusCode(409, new{ result = conflict});
            }
        }
        [HttpPost]
        [Route("/note")]
        [Authorize]
        
        public ActionResult createNotes([FromBody] NOTES n){
            if(ModelState.IsValid){
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
            var username = credentials[0];

          //  string username = us.getUsername();
            NOTES notes = new NOTES{created_on= DateTime.Now ,content= n.content,title= n.title,last_updated_on= DateTime.Now,EMAIL= username};
            _context.Add(notes);
            _context.SaveChanges();
            return StatusCode(201, new{result =  "Created"});
            }
            else{
                var conflict = "Bad Request";
                return StatusCode(400, new{ result = conflict});
            }
        }
        [HttpGet]
        [Route("/note")]
        [Authorize]
        public ActionResult getNote(){
            IEnumerable<NOTES> notes = _context.notes.AsEnumerable();

            List<NOTE> note = new List<NOTE>();

            string username = getUsername();
            
                foreach(NOTES item in notes){
                    if(item.EMAIL == username){
                NOTE n =  new NOTE();

                n.ID = item.ID;
                n.content = item.content;
                n.created_on = item.created_on;
                n.title  = item.title;
                n.last_updated_on = item.last_updated_on;
                note.Add(n);}
            }
            if(note.Capacity !=0) {
            IEnumerable<NOTE> newnote = note;
            string Json = JsonConvert.SerializeObject(newnote, Formatting.Indented);
            return StatusCode(200, Json);
            }
            else{
                return StatusCode(200, new{result = "You Don't have any notes"});
            }

                
        }
         [HttpGet]
        [Route("/note/{id}")]
        [Authorize]
        public  ActionResult GetNotebyId(string id){

            
 
                string username = getUsername();
                NOTES notes =  _context.notes.Find(id);
                if(notes != null){
                if(notes.EMAIL == username)
                {
                    return StatusCode(200, new{ID= notes.ID, Content = notes.content,Title = notes.title, Created_On = notes.created_on, last_updated_on= notes.last_updated_on});
                }
                else
                {
                    return StatusCode(401, new{result = "Not Authorized"});
                }
                }
                else{
                    return StatusCode(404, new{result = "Not Found"});
                }
        }   

        [HttpPut]
        [Route("/note/{id}")]
        [Authorize]
        public ActionResult putnote(string id,[FromBody] NOTES n){

                if(ModelState.IsValid){
                  string username = getUsername();
                  NOTES note = _context.notes.Find(id);
                  if(note != null){
                  if(note.EMAIL == username){
                  var ID = note.ID;
                  var created = note.created_on;
                  _context.notes.Remove(note);
                  _context.SaveChanges();

            var notes = new NOTES{ID = ID ,created_on= created, content= n.content, title= n.title, last_updated_on= DateTime.Now, EMAIL= username};
            _context.Add(notes);
            _context.SaveChanges();
         return  StatusCode(204, new{Result= "Note Updated Successfully" });
        }
        else{
            return StatusCode(401, new{result = "Not Authorized"});
        }
                  }
                  else{
                      return StatusCode(404, new{result = "Not Found"});
                  }
                }
                else{
                    return StatusCode(400, new{result = "Bad Request"});
                }
        }
        

}

}