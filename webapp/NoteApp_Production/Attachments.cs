using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace trial3
{
    public partial class Attachments
    {
        //public Users(string )
        [Key]
        public string AID { get; set; }

        public string url {get; set;}

        public string FileName {get;set;}

        public long length {get; set;}

        public string noteID {get; set;}

    }
}