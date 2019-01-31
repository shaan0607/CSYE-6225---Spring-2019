using System;
using trial3.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using trial3;
using trial3.Authentication;

namespace unitest
{
    public class UnitTest1
    {
         public CLOUD_CSYEContext _context;
    
        [Fact]
        public void Test1()
        {
            
            var controller = new ValuesController(_context);

            var result = controller.Get();

            Assert.IsType<System.DateTime>(result);


        }
        [Fact]
        public  void Test2(){
           
            Users u = new Users();
            u.Email = "safghfgsddkk@gmail.com";
            u.Password = "SH@344!!!ss";
        }

        
    }
    
}
