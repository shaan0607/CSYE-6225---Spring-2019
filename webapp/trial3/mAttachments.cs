using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace trial3
{
    public partial class mAttachments
    {
        //public Users(string )
        [Key]
        public string AID { get; set; }

        public string url {get; set;}

    }
}