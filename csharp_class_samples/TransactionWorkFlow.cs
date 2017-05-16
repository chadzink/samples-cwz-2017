using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feedAndSWUAccounting.Models
{
    public class TransactionWorkFlow
    {
        public TransactionStatu CurrentStatus { get; set; }
        public List<TransactionStatu> stages { get; private set; }

        public TransactionWorkFlow(int TransactionStatusId = 1)
        {
            using (var db = new FeedSWUContext())
            {
                this.stages = (from s in db.TransactionStatus
                               orderby s.Sequence ascending
                               select s).ToList<TransactionStatu>();
                
                //find the current status based on the passed transaction status id
                this.CurrentStatus = (from s in this.stages
                                      where s.TransactionStatusId == TransactionStatusId
                                      select s).FirstOrDefault<TransactionStatu>();
            }
        }

        // next states - get allowable status values from current sequence
        public List<TransactionStatu> GetNextStages()
        {
            using (var db = new FeedSWUContext())
            {
                return (from s in db.TransactionStatus
                        where s.Sequence == (this.CurrentStatus.Sequence + 1)
                        select s).ToList<TransactionStatu>();
            }
        }
    }
}
