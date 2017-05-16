using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feedAndSWUAccounting.Models
{
    public class SiteData : AccountData
    {
        public int SiteId { get; private set; }
        public int CustomerId { get; private set; }
        public List<ReactorData> Reactors { get; private set; }

        public SiteData() { }

        public SiteData(int Id)
        {
            LoadData(Id);
        }

        public SiteData(Site entityObject)
        {
            LoadData(entityObject);
        }

        public void LoadData(int Id)
        {
            using (var db = new FeedSWUContext())
            {
                Site entityObject = (from a in db.Sites
                                 where a.SiteId == Id
                                 select a).FirstOrDefault<Site>();
                LoadData(entityObject);
            }
        }

        public void LoadData(Site entityObject)
        {
            if (entityObject != null)
            {
                this.LoadAccountData(entityObject.Account);
                this.SiteId = entityObject.SiteId;
                this.CustomerId = entityObject.CustomerId;
            }
        }

        public static SiteData AddSite(CustomerData parentCustomer,
            string Label,
            string ReportLabel,
            string Address = "",
            string City = "",
            string RegionStateCode = "",
            string ZipCode = "",
            string Country = "")
        {
            AccountData act = AccountData.AddAccount(Label, ReportLabel, Address, City, RegionStateCode, ZipCode, Country);
            Site site = new Site
            {
                AccountId = act.AccountId,
                CustomerId = parentCustomer.CustomerId
            };
            return AddSite(site);
        }

        public static SiteData AddSite(Site site)
        {
            using (var db = new FeedSWUContext())
            {
                db.Sites.Add(site);
                db.SaveChanges();
                db.Entry(site).GetDatabaseValues();

                return new SiteData(site.SiteId);
            }
        }

        public Site GetSite()
        {
            using (var db = new FeedSWUContext())
            {
                return (from c in db.Sites where c.SiteId == this.SiteId select c).FirstOrDefault();
            }
        }

        public Customer GetCustomer()
        {
            using (var db = new FeedSWUContext())
            {
                return (from c in db.Customers where c.CustomerId == this.CustomerId select c).FirstOrDefault();
            }
        }

        public ReactorData AddReactor(RISCode code,
            string Label,
            string Address,
            string City,
            string RegionStateCode,
            string Country)
        {
            ReactorData reactor = ReactorData.AddReactor(this, code, Label, Address, City, RegionStateCode, Country);
            this.Reactors.Add(reactor);
            return reactor;
        }

        public ReactorData AddReactor(Reactor existingReactor)
        {
            ReactorData reactor = new ReactorData(existingReactor);
            this.Reactors.Add(reactor);
            return reactor;
        }

        public ReactorData AddReactor(ReactorData reactor)
        {
            this.Reactors.Add(reactor);
            return reactor;
        }

        public bool RemoveReactor(ReactorData RemoveReactor)
        {
            using (var db = new FeedSWUContext())
            {
                Reactor entityObject = (from a in db.Reactors
                                    where a.SiteId == RemoveReactor.ReactorId
                                    select a).FirstOrDefault<Reactor>();
                db.Reactors.Remove(entityObject);
                db.SaveChanges();

                this.Reactors.Remove(RemoveReactor);

                return true;
            }
        }

        public FeedAndSWU FeedAndSWUSummary(DateTime EffectiveDate)
        {
            using (var db = new FeedSWUContext())
            {
                return base.FeedAndSWUSummary((from c in db.AccountPosts where c.EffectiveOn <= EffectiveDate && c.SiteId == this.SiteId select c).ToList());
            }
        }
    }
}
