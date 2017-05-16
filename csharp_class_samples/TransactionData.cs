using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feedAndSWUAccounting.Models
{
    public class TransactionData
    {
        public int TransactionId { get; private set; }
        public DateTime StartedDt { get; private set; }
        public string StartedBy { get; private set; }
        public TransTypeData TransType { get; private set; }
        public TransactionWorkFlow WorkFlow { get; set; }
        public List<PostData> Posts { get; private set; }

        public TransactionData() { }

        public TransactionData(int TransactionId)
        {
            LoadData(TransactionId);
        }

        public TransactionData(Transaction transaction)
        {
            LoadData(transaction);
        }

        public void LoadData(int TransactionId)
        {
            using (var db = new FeedSWUContext())
            {
                Transaction transaction = (from a in db.Transactions
                                           where a.TransactionId == TransactionId
                                           select a).FirstOrDefault<Transaction>();
                LoadData(transaction);
            }
        }

        public void LoadData(Transaction transaction)
        {
            if (transaction != null)
            {
                this.TransactionId = transaction.TransactionId;
                this.StartedDt = transaction.StartedDt.Value;
                this.StartedBy = transaction.StartedBy;
                this.TransType = new TransTypeData(transaction.TransactionType);
                this.WorkFlow = new TransactionWorkFlow(transaction.TransStatusId);

                foreach (Post post in transaction.Posts)
                {
                    this.Posts.Add(new PostData(post));
                }
            }
        }

        public static TransactionData CreateTransaction(TransTypeData TransType, TransactionWorkFlow TransWorkFlow)
        {
            Transaction trans = new Transaction
            {
                TransTypeId = TransType.TransTypeId,
                TransStatusId = TransWorkFlow.CurrentStatus.TransactionStatusId,
                StartedBy = Program.CurrentUser.UserName,
                StartedDt = DateTime.Now
            };
            return CreateTransaction(trans);
        }

        public static TransactionData CreateTransaction(Transaction trans)
        {
            using (var db = new FeedSWUContext())
            {
                db.Transactions.Add(trans);
                db.SaveChanges();
                db.Entry(trans).GetDatabaseValues();

                return new TransactionData(trans);
            }
        }
    }
}
