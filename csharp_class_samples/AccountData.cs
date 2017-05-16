using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Data.Entity;
using System.Linq;
using feedAndSWUAccounting;

namespace feedAndSWUAccounting.Models
{
    public class AccountData
    {
        public int AccountId { get; private set; }

        public string Label { get; set; }
        public string ReportLabel { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string RegionStateCode { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public string AccountType { get; set; }
        public bool Active { get; set; }

        public AccountData() { this.AccountId = -1; }

        public List<CurrentAccountInventory> AvailibleInventory
        {
            get
            {
                using (var db = new FeedSWUContext())
                {
                    return (from i in db.CurrentAccountInventories
                            where i.AccountId == this.AccountId && i.KgU > 0
                            select i).ToList();
                }
            }
        }

        public AccountData(int AccountId)
        {
            this.AccountId = -1;
            LoadAccountData(AccountId);
        }

        public AccountData(Account Account)
        {
            this.AccountId = -1;
            LoadAccountData(Account);
        }

        public void LoadAccountData(int AccountId)
        {
            using (var db = new FeedSWUContext())
            {
                Account act = (from a in db.Accounts
                               where a.AccountId == AccountId
                               select a).FirstOrDefault<Account>();
                LoadAccountData(act);
            }
        }

        public void LoadAccountData(Account act)
        {
            if (act != null)
            {
                this.AccountId = act.AccountId;
                this.Label = act.Label;
                this.ReportLabel = act.ReportLabel;
                this.Address = act.Address;
                this.City = act.City;
                this.RegionStateCode = act.RegionStateCode;
                this.ZipCode = act.ZipCode;
                this.Country = act.Country;
                this.Active = act.Active.Value;

                using (var db = new FeedSWUContext())
                {
                    this.AccountType = (from t in db.TypeOfAccounts where t.AccountId == act.AccountId select t.AccountType).FirstOrDefault();
                }
            }
        }

        public bool SaveChanges()
        {
            using (var db = new FeedSWUContext())
            {
                Account act = (from a in db.Accounts
                               where a.AccountId == this.AccountId
                               select a).FirstOrDefault<Account>();

                if (act != null)
                {
                    act.Label = this.Label;
                    act.ReportLabel = this.ReportLabel;
                    act.Address = this.Address;
                    act.City = this.City;
                    act.RegionStateCode = this.RegionStateCode;
                    act.ZipCode = this.ZipCode;
                    act.Country = this.Country;
                    act.Active = this.Active;

                    db.SaveChanges();
                    return true;
                }
            }

            return false;
        }

        public List<PostData> QueryDebits(DateTime fromDate, DateTime toDate)
        {
            using (var db = new FeedSWUContext())
            {
                return (from d in db.Posts
                        where d.AccountId == this.AccountId
                        && (d.EffectiveOn >= fromDate || d.EffectiveOn <= toDate)
                        && (d != null && d.RemovedAt == null)
                        && d.IsDebit == true
                        select new PostData(d)).ToList<PostData>();
            }
        }

        public List<PostData> QueryCredits(DateTime fromDate, DateTime toDate)
        {
            using (var db = new FeedSWUContext())
            {
                return (from d in db.Posts
                        where d.AccountId == this.AccountId
                        && (d.EffectiveOn >= fromDate || d.EffectiveOn <= toDate)
                        && (d != null && d.RemovedAt == null)
                        && d.IsCredit == true
                        select new PostData(d)).ToList<PostData>();
            }
        }

        public static AccountData AddAccount(string Label,
            string ReportLabel,
            string Address,
            string City,
            string RegionStateCode,
            string ZipCode,
            string Country)
        {
            Account account = new Account
            {
                Label = Label,
                ReportLabel = ReportLabel,
                Address = Address,
                City = City,
                RegionStateCode = RegionStateCode,
                ZipCode = ZipCode,
                Country = Country,
                Active = true
            };

            return AddAccount(account);
        }

        public static AccountData AddAccount(Account account)
        {
            using (var db = new FeedSWUContext())
            {
                db.Accounts.Add(account);
                db.SaveChanges();
                db.Entry(account).GetDatabaseValues();

                return new AccountData(account.AccountId);
            }
        }

        public FeedAndSWU DirectFeedAndSWUSummary(DateTime EffectiveDate)
        {
            using (var db = new FeedSWUContext())
            {
                return FeedAndSWUSummary((from c in db.Posts
                                          where c.EffectiveOn <= EffectiveDate && c.AccountId == this.AccountId
                                          select c).ToList());
            }
        }

        public FeedAndSWU FeedAndSWUSummary(List<Post> Source)
        {
            FeedAndSWU result = new FeedAndSWU() { Feed = 0, SWU = 0 };

            using (var db = new FeedSWUContext())
            {
                foreach (Post post in Source)
                {
                    FeedAndSWU Changes = CalculateFromPost(post);
                    result.Feed += Changes.Feed;
                    result.SWU += Changes.SWU;
                }
            }

            return result;
        }

        public FeedAndSWU FeedAndSWUSummary(List<AccountPost> Source)
        {
            FeedAndSWU result = new FeedAndSWU() { Feed = 0, SWU = 0 };

            using (var db = new FeedSWUContext())
            {
                foreach (AccountPost post in Source)
                {
                    FeedAndSWU Changes = CalculateFromPost(post);
                    result.Feed += Changes.Feed;
                    result.SWU += Changes.SWU;
                }
            }

            return result;
        }

        private FeedAndSWU CalculateFromPost(Post post)
        {
            FeedAndSWU result = new FeedAndSWU() { Feed = 0, SWU = 0 };

            /* Secure Calcualtion Content */

            return result;
        }

        private FeedAndSWU CalculateFromPost(AccountPost post)
        {
            FeedAndSWU result = new FeedAndSWU() { Feed = 0, SWU = 0 };

            /* Secure Calcualtion Content */

            return result;
        }
    }
}
