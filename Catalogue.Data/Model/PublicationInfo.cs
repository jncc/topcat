using System;
using System.Collections.Generic;

namespace Catalogue.Data.Model
{
    public class PublicationInfo
    {
        /// <summary>
        /// Captures the record's "risk assessment" information. 
        /// </summary>
        public AssessmentInfo Assessment { get; set; }

        /// <summary>
        /// Is the record signed-off by the SIRO? (Null means no.)
        /// </summary>
        public SignOffInfo SignOff { get; set; }

        /// <summary>
        /// Info related to data transfer to data.jncc.gov.uk
        /// </summary>
        public DataInfo Data { get; set; } //todo: rename

        /// <summary>
        /// Contains info on all publishing targets
        /// </summary>
        public TargetInfo Target { get; set; }
    }

    public class DataInfo
    {
        /// <summary>
        /// Details of the last attempt to publish these resources.
        /// </summary>
        public PublicationAttempt LastAttempt { get; set; }

        /// <summary>
        /// Details about the last successful attempt to publish these resources.
        /// </summary>
        public PublicationAttempt LastSuccess { get; set; }
    }

    public class TargetInfo
    {
        /// <summary>
        /// Info related to publishing to hub.jncc.gov.uk
        /// </summary>
        public HubPublicationInfo Hub { get; set; }

        /// <summary>
        /// Info related to publishing to data.gov.uk
        /// </summary>
        public GovPublicationInfo Gov { get; set; }
    }

    public class HubPublicationInfo
    {
        /// <summary>
        /// Link to the Datahub page once successfully published
        /// </summary>
        public bool? Publishable { get; set; }

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
        /// Captures whether the record should be published to DGU. 
        /// </summary>
        public bool? Publishable { get; set; }

        /// <summary>
        /// Details of the last attempt to publish this record.
        /// </summary>
        public PublicationAttempt LastAttempt { get; set; }

        /// <summary>
        /// Details about the last successful attempt to publish this record.
        /// </summary>
        public PublicationAttempt LastSuccess { get; set; }
    }

    public class SignOffInfo
    {
        public DateTime DateUtc { get; set; }
        public UserInfo User { get; set; }
        public string Comment { get; set; }
    }

    public class AssessmentInfo
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
