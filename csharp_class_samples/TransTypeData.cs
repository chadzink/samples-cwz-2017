using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feedAndSWUAccounting.Models
{
    public class TransTypeData
    {
        public int TransTypeId { get; private set; }
        public string Label { get; private set; }
        public string TableName { get; private set; }

        public TransTypeData() { }

        public TransTypeData(int TransTypeId)
        {
            LoadData(TransTypeId);
        }

        public TransTypeData(TransactionType type)
        {
            if (type != null) LoadData(type);
            else this.TransTypeId = -1;
        }

        public void LoadData(int TransTypeId)
        {
            using (var db = new FeedSWUContext())
            {
                TransactionType type = (from a in db.TransactionTypes
                                        where a.TransTypeId == TransTypeId
                                        select a).FirstOrDefault<TransactionType>();
                LoadData(type);
            }
        }

        public void LoadData(TransactionType type)
        {
            if (type != null)
            {
                this.TransTypeId = type.TransTypeId;
                this.Label = type.Label;
                this.TableName = type.TableName;
            }
        }
    }
}
