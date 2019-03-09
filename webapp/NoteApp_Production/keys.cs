using System;

namespace NoteApp_Production

{
    public class keys
    {
        public string server { get; set; }
        public string database { get; set; }
        public string username { get; set; }
        public string password{ get; set; }
        public string bucketname { get; set; }
        

        public keys()
        {
            server = "csye6225-webapp-spring2019.cagfy3cjztoy.us-east-1.rds.amazonaws.com";
            database = "csye6225master";
            username = "csye6225master";
            password = "csye6225password";
            bucketname = "csye6225-spring2019-deosthales.me.csye6225.com";
        }
        
    }
}
