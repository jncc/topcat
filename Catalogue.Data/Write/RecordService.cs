using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Raven.Client;

namespace Catalogue.Data.Write
{
    public interface IRecordService
    {
        void Insert(Record record);
    }

    public class RecordService : IRecordService
    {
        readonly IDocumentSession db;

        public RecordService(IDocumentSession db)
        {
            this.db = db;
        }

        public void Insert(Record record)
        {
            db.Store(record);
        }
    }
}
