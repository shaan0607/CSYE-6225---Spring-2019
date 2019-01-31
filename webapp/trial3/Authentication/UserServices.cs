using System;
using BCrypt.Net;

namespace trial3.Authentication{
    public class UserServices : IUSerServices{

        private CLOUD_CSYEContext _context;
        
        public UserServices(CLOUD_CSYEContext context)
        {
            _context = context;
           // _context.Database.EnsureCreated();
        }
        public bool ValidatePassword(string password, string correctHash)
        {
           Boolean u =  BCrypt.Net.BCrypt.Verify(password, correctHash);
           return u;
           
        }
        public Users Authenticate(string Email,string Password){
        
            
        Users u = _context.Users.Find(Email);
        if(u != null){
        bool s = ValidatePassword(Password,u.Password);
        

        if(s == true){
            return u;
        }
        else{
            return null;
        }
        }
        else{
        return  null;
        }
    }
}
}

