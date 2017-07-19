using System;
using System.Collections.Generic;

namespace Catalogue.Data.Model
{
    public class PublicationInfo
    {
        public OpenDataPublicationInfo OpenData { get; set; }
    }

    public class OpenDataPublicationInfo
    {
        public Assessment Assessment { get; set; }
        public SignOff SignOff { get; set; }

        public PublicationAttempt LastAttempt { get; set; }
        public PublicationAttempt LastSuccess { get; set; }

        public List<Resource> Resources { get; set; }

        /// <summary>
        /// Don't publish this record, for the time being.
        /// </summary>
        public bool Paused { get; set; }
    }

    public class SignOff
    {
        public DateTime DateUtc { get; set; }
        public string User { get; set; }
        public string Comment { get; set; }
    }

    public class Assessment
    {
        public bool InitialAssessmentWasDoneOnSpreadsheet { get; set; }
        public bool Completed { get; set; }
        public string CompletedBy { get; set; }
        public DateTime CompletionDateUtc { get; set; }
    }

    public class Resource
    {
        public string Path { get; set; }
    }

    public class PublicationAttempt
    {
        public DateTime DateUtc { get; set; }
        public string Message { get; set; }
    }

}
