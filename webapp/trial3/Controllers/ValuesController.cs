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
using System.IO;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace trial.Controllers
{
    
    public class ValuesController : ControllerBase
    {
       // public static Dictionary<String,User> userDetails = new Dictionary<String, User>();
        // GET api/values
     //  static UserServices us = new UserServices();
        private static IAmazonS3 s3Client;


        
        
        
        static int rand=  1;
        private CLOUD_CSYEContext _context;
        
        public string getUsername(){
            
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
            var username = credentials[0];
            return username;
        }
        public ValuesController(CLOUD_CSYEContext context)
        {
            _context = context;
            
           // _context.Database.EnsureCreated();
        }
        
        [HttpGet]
        [Authorize]
        [Route("/")]
        public ActionResult Get()
        {   try{
            return StatusCode(200, new{result =DateTime.Now});
        }
        catch{
            throw new Exception("Opps");
        }


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
               { var baDRequest = "Email cant be blank";
                return StatusCode(400,  new{result = baDRequest} );}
                var s = HashPassword(u.Password);
                var user = new Users{Email= u.Email, Password = s};
                _context.Add(user);
                _context.SaveChanges();
                var Created = "User Created Successfully";
                return StatusCode(201, new {result = Created});
                }
                var badRequest = "Either Email or Password was not in correct format, Please try again";
                return StatusCode(400,  new{result = badRequest} );
            }
            else{
                var conflict = "Email Already exists";
                return StatusCode(409, new{ result = conflict});
            }
            }
        [HttpPost("UploadFiles")]
        [Route("/note")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public ActionResult createNotes(NOTES n, IFormFile file){
            if(ModelState.IsValid){
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
            var username = credentials[0];

            string fileName = ( file.FileName + rand.ToString());
            rand++;
           // var uniqueFileName = GetUniqueFileName(file.FileName);
            var uploads = Path.Combine(Directory.GetCurrentDirectory(),fileName );

            var filePath = Path.Combine(uploads);
            if(file.Length > 0)
                    using (var stream = new FileStream(filePath, FileMode.Create))
        
            
             file.CopyToAsync(stream);

            var Attachment = new Attachments{url=uploads,FileName=file.FileName, length=file.Length, noteID = n.noteID};
            _context.Add(Attachment);
            mAttachments att =  new mAttachments();
            att.AID = Attachment.AID;
            att.url = Attachment.url;
            _context.SaveChanges();     
            var notes = new NOTES{EMAIL = username ,attachments = Attachment,content  =  n.content,created_on = DateTime.Now,title = n.title,last_updated_on= DateTime.Now };
            _context.Add(notes);
           // _context.Add(Attachment);
            _context.SaveChanges();


            IEnumerable<Attachments> at = _context.attachments.AsEnumerable();
            List<mAttachments> newat = new List<mAttachments>();
                foreach(Attachments attachment in at){
                    if(attachment.noteID==n.noteID){
                        mAttachments m = new mAttachments();
                        m.AID = attachment.AID;
                        m.url = attachment.url;
                        newat.Add(m);

                       
                    }
                }
            string Json = JsonConvert.SerializeObject(notes, Formatting.Indented);
   
          // var a1 = new mAttachments{AID = Attachment.AID ,url=Attachment.url};
          //  string username = us.getUsername();

            return StatusCode(201,new{noteId= notes.noteID, content  = n.content, created_on = DateTime.Now,title = n.title,last_updated_on= DateTime.Now,attachments = att});
                    }
            else{
                var conflict = "Bad Request";
                return StatusCode(409, new{ result = conflict});

            } 
    }

        [HttpGet]
        [Route("/note")]
        [Authorize]
       public ActionResult getNote(){
            IEnumerable<NOTES> notes = _context.notes.AsEnumerable();

            List<NOTE> note = new List<NOTE>();

            string username = getUsername();
            IEnumerable<Attachments> at = _context.attachments.AsEnumerable();
                foreach(NOTES item in notes){
                    if(item.EMAIL == username){
                NOTE n =  new NOTE();

                n.noteID = item.noteID;
                n.content = item.content;
                n.created_on = item.created_on;
                n.title  = item.title;
                n.last_updated_on = item.last_updated_on;

                
                List<mAttachments> newat = new List<mAttachments>();

                foreach(Attachments attachment in at){
                    if(attachment.noteID==n.noteID){
                        mAttachments m = new mAttachments();
                        m.AID = attachment.AID;
                        m.url = attachment.url;
                        newat.Add(m);

                       
                    }
                }
                n.attachments = newat;
                note.Add(n);}
            }
            if(note.Capacity !=0) {
            IEnumerable<NOTE> newnote = note;
            string Json = JsonConvert.SerializeObject(newnote, Formatting.Indented);
            return StatusCode(200, Json);
            }
            else{
                return StatusCode(404, new{result = "You Don't have any notes"});
            }
       }
        [HttpGet]
        [Route("/note/{id}")]
        [Authorize]
        public  ActionResult GetNotebyId(string id){

            
 
                string username = getUsername();
                NOTES notes =  _context.notes.Find(id);
                if(notes!=null){
                IEnumerable<Attachments> at = _context.attachments.AsEnumerable();
                List<mAttachments> newat = new List<mAttachments>();

                foreach(Attachments attachments in at){
                    if(attachments.noteID==notes.noteID){
                        mAttachments m = new mAttachments();
                        m.AID = attachments.AID;
                        m.url = attachments.url;
                        newat.Add(m);

                       
                    }
                }
                if(notes.EMAIL == username)
                {
                    return StatusCode(200, new{ID= notes.noteID, Content = notes.content,Title = notes.title, Created_On = notes.created_on, last_updated_on= notes.last_updated_on,attachments= newat});
                }
                else
                {
                    return StatusCode(401, new{result = "Not Authorized"});
                }
                }
                else{
                    return StatusCode(401, new{result = "Note Absent"});
                }
        }   
        [HttpGet]
        [Route("/note/{id}/attachments")]
        [Authorize]
        public  ActionResult GetNoteAttachmentbyId(string id){

            
 
                string username = getUsername();
                NOTES notes =  _context.notes.Find(id);
                IEnumerable<Attachments> at = _context.attachments.AsEnumerable();
                List<mAttachments> newat = new List<mAttachments>();

                foreach(Attachments attachments in at){
                    if(attachments.noteID==notes.noteID){
                        mAttachments m = new mAttachments();
                        m.AID = attachments.AID;
                        m.url = attachments.url;
                        newat.Add(m);

                       
                    }
                }
                if(notes.EMAIL == username)
                {
                    return StatusCode(200, new{attachments= newat});
                }
                else
                {
                    return StatusCode(401, new{result = "Not Authorized"});
                }
        }

        [HttpPut]
        [Route("/note/{id}")]
        [Authorize]
        public ActionResult putnote(string id,[FromBody] NOTES n){

                  string username = getUsername();
                  NOTES note = _context.notes.Find(id);
                  if(note!=null){
                  if(note.EMAIL == username){
                  var ID = note.noteID;
                //IEnumerable<Attachments> a = _context.attachments.AsEnumerable();
                  var created = note.created_on;
                  _context.notes.Remove(note);
                  _context.SaveChanges();

            var notes = new NOTES{noteID = ID ,created_on= created, content= n.content, title= n.title, last_updated_on= DateTime.Now, EMAIL= username};
            _context.Add(notes);
            _context.SaveChanges();
         return  StatusCode(204, new{Result= "Note Updated Successfully" });
        }
        else{
            return StatusCode(401, new{result = "Not Authorized"});
        }
                  }else{
                      return StatusCode(401, new{result = "note Absent"});
                  }
        }
        
        [HttpDelete]
        [Route("/note/{id}")]
        [Authorize]
        public ActionResult Deletenote(string id){

                    string username = getUsername();

                    NOTES note = _context.notes.Find(id);
                    if(note!=null){
                    IEnumerable<Attachments> at = _context.attachments.AsEnumerable();

                    if(note.EMAIL == username){
                                var path = "";
                    foreach(Attachments atchm in at){
                        
                        if(atchm.noteID == id){
                     path = Path.Combine(Directory.GetCurrentDirectory(),atchm.FileName);

                    if(System.IO.File.Exists(path))
                    {               
                        System.IO.File.Delete(path);
                    }
                            _context.attachments.Remove(atchm);
                            
                        }
                    }

            
                    _context.notes.Remove(note);
                   
                    _context.SaveChanges();


                return  StatusCode(204, new{Result= "Note Deleted Successfully" });
                    }
                    else{
                        return StatusCode(401, new{result = "Not Authorized"});
                    }
                    }
                    else{
                      return StatusCode(401, new{result = "note Absent"});
                  }
        }

        [HttpPost]
        [Route("/note/{id}/attachments")]
        //[Consumes("multipart/form-data")]
        [Authorize]
        public  ActionResult AttachImage(string id, IFormFile file){
            
            var fileTransferUtility =
                    new TransferUtility(s3Client);
    
            string fileName = ( file.FileName + rand.ToString());
            rand++;
           // var uniqueFileName = GetUniqueFileName(file.FileName);
            var filePath = Path.Combine(file.FileName);
              var uploads = Path.Combine(Directory.GetCurrentDirectory(),file.FileName);
                     using (var stream = new FileStream(uploads, FileMode.Create))
                {
                   // fileTransferUtility.UploadAsync(stream,bucketName, keyName);
                    file.CopyToAsync(stream);
                }  
           
         
           
            string username = getUsername();
        
                Console.WriteLine("Upload 1 completed");
            if(file.Length > 0){

            }

                  NOTES note = _context.notes.Find(id);

                  var Attachment = new Attachments{url=uploads,FileName=file.FileName, length=file.Length, noteID = note.noteID};
                  _context.Add(Attachment);
                  _context.SaveChanges(); 

             IEnumerable<Attachments> a1 = _context.attachments.AsEnumerable();
             List<mAttachments> am = new List<mAttachments>();
             foreach(Attachments at in a1){
                 if(at.noteID == id){
                     mAttachments mA = new mAttachments();
                     mA.AID = at.AID;
                     mA.url = at.url;
                     am.Add(mA);
                 }
            }
            
            IEnumerable<mAttachments> newA = am;
    
            if(ModelState.IsValid){
           var a11 = new mAttachments{AID = Attachment.AID ,url=Attachment.url};
          //  string username = us.getUsername();

            return StatusCode(201, new{ a11});
                    }
            else{
                var conflict = "Bad Request";
                return StatusCode(409, new{ result = conflict});

            }

    }
    
        [HttpPut]
        [Route("/note/{id}/attachments/{aid}")]
        [Authorize]
        public ActionResult putnoteAttachent(string id,IFormFile file, string aid){
              NOTES note = _context.notes.Find(id);
              Attachments a = _context.attachments.Find(aid);
              if(note!=null && a!=null){
            string fileName = ( file.FileName + rand.ToString());
            rand++;
           // var uniqueFileName = GetUniqueFileName(file.FileName);
            var uploads = Path.Combine(Directory.GetCurrentDirectory(),fileName );

            var filePath = Path.Combine(uploads);
            if(file.Length > 0)
                    using (var stream = new FileStream(filePath, FileMode.Create))

            
             file.CopyToAsync(stream);

                  string username = getUsername();
                  
                 
                  if(note.EMAIL == username){
                  
                  
                  var newaid = a.AID;
                  var noteid = a.noteID;
                  _context.attachments.Remove(a);
                  _context.SaveChanges();
                  var newa = new Attachments{AID = newaid,noteID = noteid,url = filePath,FileName = file.FileName,length = file.Length};
                  _context.Add(newa);
                  _context.SaveChanges();

         return  StatusCode(204, new{Result= "Note Updated Successfully" });
        }
        else{
            return StatusCode(401, new{result = "Not Authorized"});
        }          
              }else{
                  return StatusCode(401, new{result = "Note Absent"});
              }
        }
       
        [HttpDelete]
        [Route("/note/{id}/attachments/{atid}")]
        [Authorize]
        public ActionResult Deletenoteattchment(string id,string atid){

                string username = getUsername();

                    NOTES note = _context.notes.Find(id);
                    if(note!=null){
                    Attachments a = _context.attachments.Find(atid);

                    var path = Path.Combine(Directory.GetCurrentDirectory(),a.FileName);
                 
        if (System.IO.File.Exists(path)){
          System.IO.File.Delete(path);   }   
         


                    if(note.EMAIL == username){
                        if(a.noteID == note.noteID && a.AID == atid){
                            _context.attachments.Remove(a);
                        }

                    _context.SaveChanges();

                return  StatusCode(204, new{Result= "Note Deleted Successfully" });
                    }
                   
                    else{
                        return StatusCode(401, new{result = "Not Authorized"});
                    }
                    }
                    else{
                      return StatusCode(401, new{result = "note Absent"});
                  }
        }


        [HttpPost]
        [Route("/reset")]
     public async void passwordreset([FromBody] Users u){
           Users a =  _context.Users.Find(u.Email);
            
            
             var client = new AmazonSimpleNotificationServiceClient();
            var request = new ListTopicsRequest();
            var response = new ListTopicsResponse();
                        
                    
                response = await client.ListTopicsAsync();
           

  foreach (var topic in response.Topics)
  {
    Console.WriteLine("Topic: \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\{0}", topic.TopicArn);

  }

            //  var request = new PublishRequest
            // {
            //     TopicArn = "",
            //     Message = "Test Message"
            // };

            //  snsClient.PublishAsync(request);
          
        }
    

 }
}
