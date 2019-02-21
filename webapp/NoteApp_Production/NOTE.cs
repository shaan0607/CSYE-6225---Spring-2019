using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace trial3
{
    public partial class NOTE
    {
        //public Users(string )
        [Key]
        public string noteID { get; set; }
        [Required]
        public string content { get; set; }
        [Required]
        public string title {get; set;}
        
        public System.DateTime created_on {get; set;}
        public System.DateTime last_updated_on {get; set;}

       public  IEnumerable<mAttachments> attachments {get; set;}

    }
}
