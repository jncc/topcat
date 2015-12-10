﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalogue.Data.Model
{
    public class PublicationInfo
    {
        public OpenDataPublicationInfo OpenData { get; set; }
    }

    public class OpenDataPublicationInfo
    {
        public List<PublicationAttempt> Attempts { get; set; }         
    }

    public class PublicationAttempt
    {
        public DateTime DateUtc { get; set; }
        public bool Successful { get; set; }
        public string Message { get; set; }
    }

}
