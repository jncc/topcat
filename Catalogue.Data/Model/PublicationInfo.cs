using System;
using System.Collections.Generic;

namespace Catalogue.Data.Model
{
    public class PublicationInfo
    {
        /// <summary>
        /// Captures the record's "risk assessment" information. 
        /// </summary>
        public OpenDataAssessmentInfo Assessment { get; set; }

        /// <summary>
        /// Is the record signed-off by the SIRO? (Null means no.)
        /// </summary>
        public OpenDataSignOffInfo SignOff { get; set; }

        /// <summary>
        /// Info related to data transfer to data.jncc.gov.uk
        /// </summary>
        public DataPublicationInfo Data { get; set; }

        /// <summary>
        /// Info related to publishing to hub.jncc.gov.uk
        /// </summary>
        public HubPublicationInfo Hub { get; set; }

        /// <summary>
        /// Info related to publishing to data.gov.uk
        /// </summary>
        public GovPublicationInfo Gov { get; set; }
    }

    public class DataPublicationInfo
    {
        /// <summary>
        /// List of publishable resources
        /// </summary>
        public List<Resource> Resources { get; set; }

        /// <summary>
        /// Details of the last attempt to publish these resources.
        /// </summary>
        public PublicationAttempt LastAttempt { get; set; }

        /// <summary>
        /// Details about the last successful attempt to publish these resources.
        /// </summary>
        public PublicationAttempt LastSuccess { get; set; }
    }

    public class HubPublicationInfo
    {
        /// <summary>
        /// Link to the Datahub page once successfully published
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Details of the last attempt to publish this record.
        /// </summary>
        public PublicationAttempt LastAttempt { get; set; }

        /// <summary>
        /// Details about the last successful attempt to publish this record.
        /// </summary>
        public PublicationAttempt LastSuccess { get; set; }
    }

    public class GovPublicationInfo
    {
        /// <summary>
        /// Captures whether the record should be published as Open Data. 
        /// </summary>
        public bool? Publishable { get; set; }

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
        public string Name { get; set; }
        public string Path { get; set; }
        public string PublishedUrl { get; set; }
    }

    public class PublicationAttempt
    {
        public DateTime DateUtc { get; set; }
        public string Message { get; set; }
    }

}
