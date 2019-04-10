using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
//using trial3.Controller.Model;
using StatsdClient;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Globalization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using NoteApp_Production;
using BCrypt.Net;
using NoteApp_Production;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.IO;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon;
using Amazon.Runtime;
using Amazon.S3.Model;
using trial3;
using StatsN;
using JustEat.StatsD;
using Amazon.SimpleNotificationService;

using Amazon.SimpleNotificationService.Model;

namespace trial.Controllers
{
    
    public class ValuesController : ControllerBase
    {
       // public static Dictionary<String,User> userDetails = new Dictionary<String, User>();
        // GET api/values
        private readonly ILogger<ValuesController> _log;
    
  
        private static IAmazonS3 s3Client;
      

        public NStatsD.Client  nc;
        private static String[] arguments = Environment.GetCommandLineArgs();

        private string bucketName = arguments[1];
     
        
         public StatsDConfiguration statsDConfig;
        public IStatsDPublisher statsDPublisher;
        static int rand=  1;
        private CLOUD_CSYEContext _context;
         private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USEast1;

         private readonly AWSCredentials credentials;
         
         
        public string getUsername(){
            
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
            var username = credentials[0];
            
            return username;
        }
        public ValuesController(CLOUD_CSYEContext context,ILogger<ValuesController> log)
        {

             _log = log;
            _context = context;
            s3Client = new AmazonS3Client(bucketRegion);
           
            statsDConfig = new  StatsDConfiguration{ Host = "localhost", Port = 8125 };
            statsDPublisher = new StatsDPublisher(statsDConfig);
            
             
        }
        
        [HttpGet]
      [Authorize]
        [Route("/")]
        public ActionResult Get()
        {   try{
          //   Console.WriteLine((EnvironmentVariablesAWSCredentials.ENVIRONMENT_VARIABLE_SECRETKEY));
  
                _log.LogInformation( "Listing all items");
                

            statsDPublisher.Increment("GET");
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
                _log.LogInformation("USER is inserted");
                Console.WriteLine("User is registered");
                
                statsDPublisher.Increment("_USER_API");
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
                   _log.LogInformation("NOTE is inserted");
                   statsDPublisher.Increment("_NOTE_API");
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
            var username = credentials[0];
            var fileTransferUtility =
                new TransferUtility(s3Client);
   
            string fileName = (rand.ToString() +file.FileName);
            rand++;
            var uploads = Path.Combine(Directory.GetCurrentDirectory(), file.FileName);

            var filePath = Path.Combine(uploads);
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
           { 
               file.CopyToAsync(stream);
               fileTransferUtility.UploadAsync(stream, bucketName, fileName);
           }
            GetPreSignedUrlRequest request = new GetPreSignedUrlRequest();
            request.BucketName = bucketName;
            request.Key = fileName;
            request.Expires    = DateTime.Now.AddYears(2);
            request.Protocol   = Protocol.HTTP;
            string url =  fileTransferUtility.S3Client.GetPreSignedURL(request);

            var Attachment = new Attachments{url=url,FileName=fileName, length=file.Length, noteID = n.noteID};
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
                    _log.LogInformation("Getting the node");
                   statsDPublisher.Increment("_NOTE_GET_API");

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
                return StatusCode(200, new{result = "You Don't have any notes"});
            }
       }
        [HttpGet]
        [Route("/note/{id}")]
        [Authorize]
        public  ActionResult GetNotebyId(string id){
                    _log.LogInformation("NOTE is inserted");
                   statsDPublisher.Increment("_NOTE_GETBYID_API");
            
 
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
                    return StatusCode(200, new{ID= notes.noteID, Content = notes.content,Title = notes.title, Created_On = notes.created_on, last_updated_on= notes.last_updated_on,attachments= newat});
                }
                else
                {
                    return StatusCode(401, new{result = "Not Authorized"});
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
        }
        
        [HttpDelete]
        [Route("/note/{id}")]
        [Authorize]
        public ActionResult Deletenote(string id){
            
            var fileTransferUtility =
                new TransferUtility(s3Client);

                    string username = getUsername();

                    NOTES note = _context.notes.Find(id);

                    IEnumerable<Attachments> at = _context.attachments.AsEnumerable();
                    string key = "";
                    if(note.EMAIL == username)
                    {
                        
                    foreach(Attachments atchm in at){
                        if(atchm.noteID == id)
                        {
                            key = atchm.FileName;
                            
                            fileTransferUtility.S3Client.DeleteObjectAsync(new Amazon.S3.Model.DeleteObjectRequest() { BucketName = bucketName, Key =  key });
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

        [HttpPost]
        [Route("/note/{id}/attachments")]
        //[Consumes("multipart/form-data")]
        [Authorize]
        public  ActionResult AttachImage(string id, IFormFile file){
                      var fileTransferUtility =
                    new TransferUtility(s3Client);
         
    
            string fileName = (rand.ToString() + file.FileName );
            rand++;
           // var uniqueFileName = GetUniqueFileName(file.FileName);
            var filePath = Path.Combine(file.FileName);
              var uploads = Path.Combine(Directory.GetCurrentDirectory(),fileName );
                     using (var stream = new FileStream(filePath, FileMode.Create))
                {
                   // fileTransferUtility.UploadAsync(stream,bucketName, keyName);
                    file.CopyToAsync(stream);
                }  
                   
                     fileTransferUtility.UploadAsync(uploads, bucketName, fileName);
                     GetPreSignedUrlRequest request = new GetPreSignedUrlRequest();
                     request.BucketName =  bucketName;
                     request.Key = fileName;
                     request.Expires    = DateTime.Now.AddYears((2));
                     request.Protocol   = Protocol.HTTP;
                     string url =  fileTransferUtility.S3Client.GetPreSignedURL(request);         
            string username = getUsername();
        // Console.WriteLine(arguments[1]);
                Console.WriteLine("Upload 1 completed");
            if(file.Length > 0){

            }

                  NOTES note = _context.notes.Find(id);

                  var Attachment = new Attachments{url=url,FileName=fileName, length=file.Length, noteID = note.noteID};
                  _context.Add(Attachment);
                  _context.SaveChanges(); 

             IEnumerable<Attachments> a1 = _context.attachments.AsEnumerable();
             List<mAttachments> am = new List<mAttachments>();
             string key = "";
             foreach(Attachments at in a1){
                 if(at.noteID== id)
                 {
                     key = at.FileName;
                     mAttachments mA = new mAttachments();
                     mA.AID = at.AID;
                     mA.url = at.url;
                     am.Add(mA);
                 }
            }
             
           //  fileTransferUtility.S3Client.DeleteObjectAsync(new Amazon.S3.Model.DeleteObjectRequest() { BucketName = bucketname, Key =  key });
            
            IEnumerable<mAttachments> newA = am;
    
            if(ModelState.IsValid){
           var a11 = new mAttachments{AID = Attachment.AID ,url=Attachment.url};
          //  string username = us.getUsername();

            return StatusCode(201, new{ a11});
                    }
            else{
                var conflict = "Bad Request";
                return StatusCode(409, new{ result = conflict});

            }  }

    
        [HttpPut]
        [Route("/note/{id}/attachments/{aid}")]
        [Authorize]
        public ActionResult putnoteAttachent(string id,IFormFile file, string aid){
            var fileTransferUtility =
                new TransferUtility(s3Client);
            string fileName = (rand.ToString() + file.FileName );
            rand++;
           // var uniqueFileName = GetUniqueFileName(file.FileName);
            var uploads = Path.Combine(Directory.GetCurrentDirectory(),fileName );

            var filePath = Path.Combine(uploads);
            if(file.Length > 0)
                    using (var stream = new FileStream(filePath, FileMode.Create))

           
            file.CopyToAsync(stream);
            
            Attachments a1 = _context.attachments.Find(aid);
            string key = a1.FileName;
            fileTransferUtility.S3Client.DeleteObjectAsync(new Amazon.S3.Model.DeleteObjectRequest() { BucketName = bucketName, Key =  key });
            fileTransferUtility.UploadAsync(uploads, bucketName, fileName);
            
            GetPreSignedUrlRequest request = new GetPreSignedUrlRequest();
            request.BucketName = bucketName;
            request.Key = fileName;
            request.Expires    = DateTime.Now.AddYears(4);
            request.Protocol   = Protocol.HTTP;
            string url =  fileTransferUtility.S3Client.GetPreSignedURL(request);

                  string username = getUsername();
                  NOTES note = _context.notes.Find(id);
                  if(note.EMAIL == username){
                  Attachments a = _context.attachments.Find(aid);
                  var newaid = a.AID;
                  var noteid = a.noteID;
                  _context.attachments.Remove(a);
                  _context.SaveChanges();
                  var newa = new Attachments{AID = newaid,noteID = noteid,url = url,FileName = fileName,length = file.Length};
                  _context.Add(newa);
                  _context.SaveChanges();

         return  StatusCode(204, new{Result= "Note Updated Successfully" });
        }
        else{
            return StatusCode(401, new{result = "Not Authorized"});
        }
        }
       
        [HttpDelete]
        [Route("/note/{id}/attachments/{atid}")]
        [Authorize]
        public ActionResult Deletenoteattchment(string id,string atid){
            var fileTransferUtility =
                new TransferUtility(s3Client);
                string username = getUsername();

                    NOTES note = _context.notes.Find(id);

                    Attachments a = _context.attachments.Find(atid);
                    string key = a.FileName;
                    if(note.EMAIL == username){
                        if(a.noteID == note.noteID && a.AID == atid){
                            _context.attachments.Remove(a);
                        }
                        fileTransferUtility.S3Client.DeleteObjectAsync(new Amazon.S3.Model.DeleteObjectRequest() { BucketName = bucketName, Key =  key });
                    _context.SaveChanges();

                return  StatusCode(204, new{Result= "Note Deleted Successfully" });
                    }
                   
                    else{
                        return StatusCode(401, new{result = "You are Not Authorized"});
                    }

        }

 [HttpPost]
        [Route("/reset")]
        public async void passwordreset([FromBody] Users u){
           Users a =  _context.Users.Find(u.Email);
           _log.LogInformation( "Listing all items");
                
            Console.WriteLine("Hello inside the reset");
            if(a!=null){
             var client = new AmazonSimpleNotificationServiceClient(RegionEndpoint.USEast1);
            var request = new ListTopicsRequest();
            var response = new ListTopicsResponse();
                            _log.LogInformation( "going inside for");
                    
                response = await client.ListTopicsAsync();
                 foreach (var topic in response.Topics)
                {
                   
                    _log.LogInformation( topic.TopicArn);
                  if( topic.TopicArn.EndsWith("SNSTopicResetPassword")){
                       _log.LogInformation( topic.TopicArn);
             var respose = new PublishRequest
            {
                TopicArn =topic.TopicArn,
                Message = a.Email
            };

             await client.PublishAsync(respose);
                  }
                  
                } 
            }  
        }
 }
}
