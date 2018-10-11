using Catalogue.Data.Model;

namespace Catalogue.Data
{
    public static class Helpers
    {
        public static string AddCollection(string id)
        {
            return "records/" + id;
        }

        public static string RemoveCollection(string id)
        {
            return id.Replace("records/", "");
        }

        public static Record AddCollectionToId(Record record)
        {
            record.Id = "records/" + record.Id;
            return record;
        }

        public static Record RemoveCollectionFromId(Record record)
        {
            record.Id = record.Id.Replace("records/", "");
            return record;
        }
    }
}