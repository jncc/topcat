using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Gemini.Templates;
using Catalogue.Utilities.Clone;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Catalogue.Data;
using Catalogue.Robot.Publishing;
using Catalogue.Robot.Publishing.Data;
using Catalogue.Robot.Publishing.Gov;
using Catalogue.Robot.Publishing.Hub;

namespace Catalogue.Tests.Slow.Catalogue.Robot
{
    public class robot_uploader_specs : CleanDbTest
    {
        Record readyToUploadRecord = new Record().With(r =>
        {
            r.Id = Helpers.AddCollection(Guid.NewGuid().ToString());
            r.Path = @"X:\path\to\uploader\test";
            r.Validation = Validation.Gemini;
            r.Gemini = Library.Example().With(m =>
            {
                m.MetadataDate = new DateTime(2017, 09, 26);
            });
            r.Publication = new PublicationInfo
            {
                Assessment = new AssessmentInfo
                {
                    Completed = true,
                    CompletedOnUtc = new DateTime(2017, 09, 25)
                },
                SignOff = new SignOffInfo
                {
                    DateUtc = new DateTime(2017, 09, 26)
                },
                Data = new DataInfo
                {
                    Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } }
                },
                Target = new TargetInfo
                {
                    Hub = new HubPublicationInfo
                    {
                        Publishable = true
                    },
                    Gov = new GovPublicationInfo
                    {
                        Publishable = true
                    }
                }
            };
            r.Footer = new Footer();
        });

        [Test]
        public void assessed_and_signed_off_ready_to_upload()
        {
            var assessedAndSignedOffRecord = readyToUploadRecord;

            TestRecordReturned(assessedAndSignedOffRecord);
        }

        [Test]
        public void assessed_but_not_signed_off_record()
        {
            var assessedRecord = readyToUploadRecord.With(r => r.Publication.SignOff = null);

            TestRecordNotReturned(assessedRecord);
        }

        [Test]
        public void null_publication_info()
        {
            var nullPublicationRecord = readyToUploadRecord.With(r => r.Publication = null);

            TestRecordNotReturned(nullPublicationRecord);
        }

        [Test]
        public void assessed_and_signed_off_record_publishable_to_hub_only()
        {
            var publishToHub1 = readyToUploadRecord.With(r =>
            {
                r.Publication.Target.Hub.Publishable = true;
                r.Publication.Target.Gov = null;
            });

            TestRecordReturned(publishToHub1);

            var publishToHub2 = readyToUploadRecord.With(r =>
            {
                r.Publication.Target.Hub.Publishable = true;
                r.Publication.Target.Gov.Publishable = null;
            });

            TestRecordReturned(publishToHub2);

            var publishToHub3 = readyToUploadRecord.With(r =>
            {
                r.Publication.Target.Hub.Publishable = true;
                r.Publication.Target.Gov.Publishable = false;
            });

            TestRecordReturned(publishToHub3);
        }

        [Test]
        public void assessed_and_signed_off_record_publishable_to_gov_only()
        {
            var publishToGov1 = readyToUploadRecord.With(r =>
            {
                r.Publication.Target.Hub = null;
                r.Publication.Target.Gov.Publishable = true;
            });

            TestRecordReturned(publishToGov1);

            var publishToGov2 = readyToUploadRecord.With(r =>
            {
                r.Publication.Target.Hub.Publishable = null;
                r.Publication.Target.Gov.Publishable = true;
            });

            TestRecordReturned(publishToGov2);

            var publishToGov3 = readyToUploadRecord.With(r =>
            {
                r.Publication.Target.Hub.Publishable = false;
                r.Publication.Target.Gov.Publishable = true;
            });

            TestRecordReturned(publishToGov3);
        }

        [Test]
        public void assessed_and_signed_off_record_publishable_to_hub_and_gov()
        {
            var publishToBoth = readyToUploadRecord.With(r =>
            {
                r.Publication.Target.Hub.Publishable = true;
                r.Publication.Target.Gov.Publishable = true;
            });

            TestRecordReturned(publishToBoth);
        }

        [Test]
        public void hub_and_gov_unpublishable()
        {
            var bothUnpublishable = readyToUploadRecord.With(r =>
            {
                r.Publication.Target.Hub.Publishable = false;
                r.Publication.Target.Gov.Publishable = false;
            });

            TestRecordNotReturned(bothUnpublishable);
        }

        [Test]
        public void hub_unpublishable_and_gov_null()
        {
            var hubUnpublishable = readyToUploadRecord.With(r =>
            {
                r.Publication.Target.Hub.Publishable = false;
                r.Publication.Target.Gov = null;
            });

            TestRecordNotReturned(hubUnpublishable);
        }

        [Test]
        public void hub_null_and_gov_unpublishable()
        {
            var govUnpublishable = readyToUploadRecord.With(r =>
            {
                r.Publication.Target.Hub = null;
                r.Publication.Target.Gov.Publishable = false;
            });

            TestRecordNotReturned(govUnpublishable);
        }

        [Test]
        public void hub_and_gov_both_null()
        {
            var undecidedRecord = readyToUploadRecord.With(r =>
            {
                r.Publication.Target.Hub.Publishable = null;
                r.Publication.Target.Gov.Publishable = null;
            });

            TestRecordNotReturned(undecidedRecord);
        }

        [Test]
        public void record_signed_off_but_not_assessed()
        {
            var signedOffOnlyRecord = readyToUploadRecord.With(r =>
                r.Publication.Assessment.Completed = false);

            TestRecordNotReturned(signedOffOnlyRecord);
        }

        [Test]
        public void failed_data_transfer_attempt_for_hub_and_gov_publish()
        {
            var assessedAndSignedOffRecord = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection(Guid.NewGuid().ToString());
                r.Path = @"X:\path\to\uploader\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 26);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 09, 25)
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 09, 26)
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
                        LastAttempt = new PublicationAttempt
                        {
                            DateUtc = new DateTime(2017, 09, 20)
                        }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo { Publishable = true },
                        Gov = new GovPublicationInfo { Publishable = true }
                    }
                };
                r.Footer = new Footer();
            });

            TestRecordReturned(assessedAndSignedOffRecord);
        }

        [Test]
        public void failed_data_transfer_attempt_for_hub_and_gov_republish()
        {
            var attemptedButFailedRecord = readyToUploadRecord.With(r =>
            {
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 27);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 09, 25)
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 09, 26)
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
                        LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 27) },
                        LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 22) }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true,
                            Url = "http://hub.jncc.gov.uk/assets/guid-here",
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 23) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 23) }
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true,
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 24) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 24) }
                        }
                    }
                };
            });

            TestRecordReturned(attemptedButFailedRecord);
        }

        [Test]
        public void failed_hub_attempt_for_hub_and_gov_publish()
        {
            var hubFailure = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection(Guid.NewGuid().ToString());
                r.Path = @"X:\path\to\uploader\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 26);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 09, 25)
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 09, 26)
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
                        LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 23) },
                        LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 23) }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true,
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 24) }
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true
                        }
                    }
                };
                r.Footer = new Footer();
            });

            TestRecordReturned(hubFailure);
        }

        [Test]
        public void failed_hub_attempt_for_hub_and_gov_republish()
        {
            var attemptedButFailedRecord = readyToUploadRecord.With(r =>
            {
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 28);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 09, 25)
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 09, 26)
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
                        LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 27) },
                        LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 27) }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true,
                            Url = "http://hub.jncc.gov.uk/assets/guid-here",
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 28) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 24) }
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true,
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 24) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 24) }
                        }
                    }
                };
            });

            TestRecordReturned(attemptedButFailedRecord);
        }

        [Test]
        public void failed_gov_attempt_for_hub_and_gov_publish()
        {
            var attemptedButFailedRecord = readyToUploadRecord.With(r =>
            {
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 29);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 09, 25)
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 09, 26)
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
                        LastAttempt = new PublicationAttempt{DateUtc = new DateTime(2017, 09, 27)},
                        LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 27) }
                    },
                    Target = new TargetInfo {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true,
                            Url = "http://hub.jncc.gov.uk/assets/guid-here",
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 28) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 28) }
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true,
                            LastAttempt = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2017, 09, 29)
                            }
                        }
                    }
                };
            });

            TestRecordReturned(attemptedButFailedRecord);
        }

        [Test]
        public void failed_gov_attempt_for_hub_and_gov_republish()
        {
            var attemptedButFailedRecord = readyToUploadRecord.With(r =>
            {
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 29);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 09, 25)
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 09, 26)
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
                        LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 27) },
                        LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 27) }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true,
                            Url = "http://hub.jncc.gov.uk/assets/guid-here",
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 28) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 28) }
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true,
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 29) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 24) }
                        }
                    }
                };
            });

            TestRecordReturned(attemptedButFailedRecord);
        }

        [Test]
        public void already_uploaded_for_hub_and_gov_publish()
        {
            var alreadyUploadedRecord = readyToUploadRecord.With(r =>
            {
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 29);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 09, 25)
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 09, 26)
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
                        LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 27) },
                        LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 27) }
                    },
                    Target = new TargetInfo {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true,
                            Url = "http://hub.jncc.gov.uk/assets/guid-here",
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 28) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 28) }
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true,
                            LastAttempt = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2017, 09, 29)
                            },
                            LastSuccess = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2017, 09, 29)
                            }
                        }
                    }
                };
            });

            TestRecordNotReturned(alreadyUploadedRecord);
        }

        [Test]
        public void already_uploaded_for_hub_publish()
        {
            var alreadyUploadedRecord = readyToUploadRecord.With(r =>
            {
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 29);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 09, 25)
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 09, 26)
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
                        LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 27) },
                        LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 27) }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true,
                            Url = "http://hub.jncc.gov.uk/assets/guid-here",
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 29) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 29) }
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = false,
                            LastAttempt = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2017, 09, 24)
                            },
                            LastSuccess = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2017, 09, 24)
                            }
                        }
                    }
                };
            });

            TestRecordNotReturned(alreadyUploadedRecord);
        }

        [Test]
        public void already_uploaded_for_gov_publish()
        {
            var alreadyUploadedRecord = readyToUploadRecord.With(r =>
            {
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 29);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 09, 25)
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 09, 26)
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
                        LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 27) },
                        LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 27) }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = false,
                            Url = "http://hub.jncc.gov.uk/assets/guid-here",
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 24) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 24) }
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true,
                            LastAttempt = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2017, 09, 29)
                            },
                            LastSuccess = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2017, 09, 29)
                            }
                        }
                    }
                };
            });

            TestRecordNotReturned(alreadyUploadedRecord);
        }

        [Test]
        public void hub_and_gov_published_and_out_of_date_record()
        {
            var publishedAndOutOfDateRecord = readyToUploadRecord.With(r =>
            {
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 30);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 09, 25)
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 09, 26)
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
                        LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 27) },
                        LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 27) }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true,
                            Url = "http://hub.jncc.gov.uk/assets/guid-here",
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 28) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 28) }
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true,
                            LastAttempt = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2017, 09, 29)
                            },
                            LastSuccess = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2017, 09, 29)
                            }
                        }
                    }
                };
            });

            TestRecordNotReturned(publishedAndOutOfDateRecord);
        }

        [Test]
        public void ready_to_republish_to_hub_and_gov_again()
        {
            var readyToRepublishRecord = readyToUploadRecord.With(r =>
            {
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 29);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 09, 28)
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 09, 29)
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
                        LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 25) },
                        LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 25) }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true,
                            Url = "http://hub.jncc.gov.uk/assets/guid-here",
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 26) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 26) }
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true,
                            LastAttempt = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2017, 09, 27)
                            },
                            LastSuccess = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2017, 09, 27)
                            }
                        }
                    }
                };
            });

            TestRecordReturned(readyToRepublishRecord);
        }

        [Test]
        public void published_to_hub_and_gov_and_now_republish_to_hub_only()
        {
            var readyToRepublishRecord = readyToUploadRecord.With(r =>
            {
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 29);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 09, 28)
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 09, 29)
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
                        LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 25) },
                        LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 25) }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true,
                            Url = "http://hub.jncc.gov.uk/assets/guid-here",
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 26) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 26) }
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = false,
                            LastAttempt = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2017, 09, 27)
                            },
                            LastSuccess = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2017, 09, 27)
                            }
                        }
                    }
                };
            });

            TestRecordReturned(readyToRepublishRecord);
        }

        [Test]
        public void published_to_hub_and_gov_and_now_republish_to_gov_only()
        {
            var readyToRepublishRecord = readyToUploadRecord.With(r =>
            {
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 29);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 09, 28)
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 09, 29)
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
                        LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 25) },
                        LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 25) }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = false,
                            Url = "http://hub.jncc.gov.uk/assets/guid-here",
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 26) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 26) }
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true,
                            LastAttempt = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2017, 09, 27)
                            },
                            LastSuccess = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2017, 09, 27)
                            }
                        }
                    }
                };
            });

            TestRecordReturned(readyToRepublishRecord);
        }

        [Test]
        public void published_to_hub_and_now_publish_to_gov_only()
        {
            var readyToRepublishRecord = readyToUploadRecord.With(r =>
            {
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 29);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 09, 28)
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 09, 29)
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
                        LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 25) },
                        LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 25) }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = false,
                            Url = "http://hub.jncc.gov.uk/assets/guid-here",
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 26) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 26) }
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true
                        }
                    }
                };
            });

            TestRecordReturned(readyToRepublishRecord);
        }

        [Test]
        public void published_to_gov_and_now_publish_to_hub_only()
        {
            var readyToRepublishRecord = readyToUploadRecord.With(r =>
            {
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 29);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 09, 28)
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 09, 29)
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
                        LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 25) },
                        LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 25) }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = false,
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 26) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 26) }
                        }
                    }
                };
            });

            TestRecordReturned(readyToRepublishRecord);
        }

        [Test]
        public void published_to_gov_and_now_publish_to_hub_and_gov()
        {
            var readyToRepublishRecord = readyToUploadRecord.With(r =>
            {
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 29);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 09, 28)
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 09, 29)
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
                        LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 25) },
                        LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 25) }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true,
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 26) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 26) }
                        }
                    }
                };
            });

            TestRecordReturned(readyToRepublishRecord);
        }

        [Test]
        public void published_to_hub_and_now_publish_to_hub_and_gov()
        {
            var readyToRepublishRecord = readyToUploadRecord.With(r =>
            {
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 29);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 09, 28)
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 09, 29)
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
                        LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 25) },
                        LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 25) }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true,
                            Url = "http://hub.jncc.gov.uk/assets/guid-here",
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 26) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 26) }
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true
                        }
                    }
                };
            });

            TestRecordReturned(readyToRepublishRecord);
        }

        [Test]
        public void reupload_with_now_unpublishable_record()
        {
            var readyToRepublishRecord = new Record().With(r =>
            {
                r.Id = Helpers.AddCollection(Guid.NewGuid().ToString());
                r.Path = @"X:\path\to\uploader\test";
                r.Validation = Validation.Gemini;
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 29);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 09, 28)
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 09, 29)
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
                        LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 25) },
                        LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 25) }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = false,
                            Url = "http://hub.jncc.gov.uk/assets/guid-here",
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 26) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 26) }
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = false,
                            LastAttempt = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2017, 09, 27)
                            },
                            LastSuccess = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2017, 09, 27)
                            }
                        }
                    }
                };
                r.Footer = new Footer();
            });

            TestRecordNotReturned(readyToRepublishRecord);
        }

        [Test]
        public void previously_published_and_reassessed_but_not_signed_off()
        {
            var publishedAndReassessedRecord = readyToUploadRecord.With(r =>
            {
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 28);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 09, 28)
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 09, 26)
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
                        LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 25) },
                        LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 25) }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true,
                            Url = "http://hub.jncc.gov.uk/assets/guid-here",
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 26) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 26) }
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true,
                            LastAttempt = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2017, 09, 27)
                            },
                            LastSuccess = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2017, 09, 27)
                            }
                        }
                    }
                };
            });

            TestRecordNotReturned(publishedAndReassessedRecord);
        }

        [Test]
        public void previously_published_assessed_and_signed_off_and_now_out_of_date()
        {
            var publishedAssessedSignedOffThenEditedRecord = readyToUploadRecord.With(r =>
            {
                r.Gemini = Library.Example().With(m =>
                {
                    m.MetadataDate = new DateTime(2017, 09, 30);
                });
                r.Publication = new PublicationInfo
                {
                    Assessment = new AssessmentInfo
                    {
                        Completed = true,
                        CompletedOnUtc = new DateTime(2017, 09, 28)
                    },
                    SignOff = new SignOffInfo
                    {
                        DateUtc = new DateTime(2017, 09, 29)
                    },
                    Data = new DataInfo
                    {
                        Resources = new List<Resource> { new Resource { Name = "File resource", Path = @"X:\path\to\uploader\test.txt" } },
                        LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 25) },
                        LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 25) }
                    },
                    Target = new TargetInfo
                    {
                        Hub = new HubPublicationInfo
                        {
                            Publishable = true,
                            Url = "http://hub.jncc.gov.uk/assets/guid-here",
                            LastAttempt = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 26) },
                            LastSuccess = new PublicationAttempt { DateUtc = new DateTime(2017, 09, 26) }
                        },
                        Gov = new GovPublicationInfo
                        {
                            Publishable = true,
                            LastAttempt = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2017, 09, 27)
                            },
                            LastSuccess = new PublicationAttempt
                            {
                                DateUtc = new DateTime(2017, 09, 27)
                            }
                        }
                    }
                };
            });

            TestRecordNotReturned(publishedAssessedSignedOffThenEditedRecord);
        }

        private void TestRecordNotReturned(Record record)
        {
            var pendingRecords = PendingUploadTest(record);
            pendingRecords.Count.Should().Be(0);
        }

        private void TestRecordReturned(Record record)
        {
            var pendingRecords = PendingUploadTest(record);
            pendingRecords.Count.Should().Be(1);
            pendingRecords.Contains(record).Should().BeTrue();
        }

        private List<Record> PendingUploadTest(Record record)
        {
            using (var db = ReusableDocumentStore.OpenSession())
            {
                db.Store(record);
                db.SaveChanges();
                WaitForIndexing(ReusableDocumentStore);

                var uploadServiceMock = new Mock<IPublishingUploadRecordService>();
                var dataUploaderMock = new Mock<IDataUploader>();
                var metadataUploaderMock = new Mock<IMetadataUploader>();
                var hubServiceMock = new Mock<IHubService>();
                var recordRedactorMock = new Mock<IRecordRedactor>();
                var robotUploader = new RobotPublisher(db, recordRedactorMock.Object, uploadServiceMock.Object, dataUploaderMock.Object, metadataUploaderMock.Object, hubServiceMock.Object);

                var result = robotUploader.GetRecordsPendingUpload();
                return result;
            }
        }
    }
}
