using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feedAndSWUAccounting.Models
{
    public class ReactorData : AccountData
    {
        public int ReactorId { get; private set; }
        public int SiteId { get; private set; }
        public RISCode RISCode { get; private set; }
        public List<ReloadData> Reloads { get; private set; }
        public string AccountCode { get; private set; }

        public ReactorData() { }

        public ReactorData(int Id)
        {
            LoadData(Id);
        }

        public ReactorData(Reactor entityObject)
        {
            LoadData(entityObject);
        }

        public void LoadData(int Id)
        {
            using (var db = new FeedSWUContext())
            {
                Reactor entityObject = (from a in db.Reactors
                                 where a.ReactorId == Id
                                    select a).FirstOrDefault<Reactor>();
                LoadData(entityObject);
            }
        }

        public void LoadData(Reactor entityObject)
        {
            if (entityObject != null)
            {
                this.LoadAccountData(entityObject.Account);
                this.SiteId = entityObject.SiteId;
                this.ReactorId = entityObject.ReactorId;
                this.AccountCode = entityObject.AccountCode;

                this.RISCode = entityObject.RISCode;
            }
        }

        public static ReactorData AddReactor(SiteData parentSite,
            RISCode code,
            string Label,
            string ReportLabel,
            string AccountCode,
            string Address = "",
            string City = "",
            string RegionStateCode = "",
            string ZipCode = "",
            string Country = "")
        {
            AccountData act = AccountData.AddAccount(Label, ReportLabel, Address, City, RegionStateCode, ZipCode, Country);
            Reactor reactor = new Reactor
            {
                AccountId = act.AccountId,
                SiteId = parentSite.SiteId,
                RISCodeId = code.RISCodeId,
                AccountCode = AccountCode
            };
            return AddReactor(reactor);
        }

        public static ReactorData AddReactor(Reactor reactor)
        {
            using (var db = new FeedSWUContext())
            {
                db.Reactors.Add(reactor);
                db.SaveChanges();
                db.Entry(reactor).GetDatabaseValues();

                return new ReactorData(reactor.ReactorId);
            }
        }

        public ReloadData AddReload(string ProjectNumber,
            string Cycle)
        {
            ReloadData reload = ReloadData.AddReload(this, ProjectNumber, Cycle);
            this.Reloads.Add(reload);
            return reload;
        }

        public ReloadData AddReload(Reload existingReload)
        {
            ReloadData reload = new ReloadData(existingReload);
            this.Reloads.Add(reload);
            return reload;
        }

        public ReloadData AddReload(ReloadData reload)
        {
            this.Reloads.Add(reload);
            return reload;
        }

        public bool RemoveReload(ReloadData RemoveReload)
        {
            using (var db = new FeedSWUContext())
            {
                Reload entityObject = (from a in db.Reloads
                                      where a.ReloadId == RemoveReload.ReloadId
                                      select a).FirstOrDefault<Reload>();
                db.Reloads.Remove(entityObject);
                db.SaveChanges();

                this.Reloads.Remove(RemoveReload);

                return true;
            }
        }

        public FeedAndSWU FeedAndSWUSummary(DateTime EffectiveDate)
        {
            using (var db = new FeedSWUContext())
            {
                return base.FeedAndSWUSummary((from c in db.AccountPosts where c.EffectiveOn <= EffectiveDate && c.ReactorId == this.ReactorId select c).ToList());
            }
        }
    }
}
