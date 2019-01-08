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
        /// <summary>
        /// Captures whether the record should be published as Open Data. 
        /// </summary>
        public bool? Publishable { get; set; }

        /// <summary>
        /// Captures the record's "risk assessment" information. 
        /// </summary>
        public OpenDataAssessmentInfo Assessment { get; set; }

        /// <summary>
        /// Is the record signed-off by the SIRO? (Null means no.)
        /// </summary>
        public OpenDataSignOffInfo SignOff { get; set; }

        /// <summary>
        /// The files to actually publish, if different to the record's path.
        /// </summary>
        public List<Resource> Resources { get; set; }

        /// <summary>
        /// Don't publish this record, for the time being.
        /// </summary>
        public bool Paused { get; set; }

        /// <summary>
        /// Details of the last attempt to publish this record.
        /// </summary>
        public PublicationAttempt LastAttempt { get; set; }

        /// <summary>
        /// Details about the last successful attempt to publish this record.
        /// </summary>
        public PublicationAttempt LastSuccess { get; set; }
    }

    public class OpenDataSignOffInfo
    {
        public DateTime DateUtc { get; set; }
        public UserInfo User { get; set; }
        public string Comment { get; set; }
    }

    public class OpenDataAssessmentInfo
    {
        public bool Completed { get; set; }
        public UserInfo CompletedByUser { get; set; }
        public DateTime CompletedOnUtc { get; set; }

        public bool InitialAssessmentWasDoneOnSpreadsheet { get; set; }
    }

    public class Resource
    {
        public string Path { get; set; }
        public string Url { get; set; }
    }

    public class PublicationAttempt
    {
        public DateTime DateUtc { get; set; }
        public string Message { get; set; }
    }

}
