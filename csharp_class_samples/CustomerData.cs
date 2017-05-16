using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feedAndSWUAccounting.Models
{
    public class CustomerData : AccountData
    {
        public int CustomerId { get; private set; }
        public List<SiteData> Sites { get; private set; }
        public decimal TailsAssay { get; set; }
        public decimal LossAllowancePercentage { get; set; }
        public List<CustomerSub> SubAccounts { get; private set; }

        public CustomerData() { }

        public CustomerData(int Id)
        {
            LoadData(Id);
        }

        public CustomerData(Customer entityObject)
        {
            LoadData(entityObject);
        }

        public void LoadData(int Id)
        {
            using (var db = new FeedSWUContext())
            {
                Customer entityObject = (from a in db.Customers
                                 where a.CustomerId == Id
                                 select a).FirstOrDefault<Customer>();

                LoadData(entityObject);
            }
        }

        public void LoadData(Customer entityObject)
        {
            if (entityObject != null)
            {
                this.LoadAccountData(entityObject.Account);
                this.CustomerId = entityObject.CustomerId;
                this.TailsAssay = entityObject.TailsAssay.GetValueOrDefault(0.3M);
                this.LossAllowancePercentage = entityObject.LossAllowancePercentage.GetValueOrDefault(0);

                // load the customer sub accounts
                this.SubAccounts = entityObject.CustomerSubs.ToList();

                this.Sites = new List<SiteData>();
                foreach(Site site in entityObject.Sites)
                {
                    this.Sites.Add(new SiteData(site));
                }
            }
        }

        public void Refresh()
        {
            using (var db = new FeedSWUContext())
            {
                Customer customer = (from c in db.Customers where c.CustomerId == this.CustomerId select c).FirstOrDefault();
                if (customer != null)
                {
                    this.LoadData(customer);
                }
            }
        }

        public bool SaveChanges()
        {
            bool result = base.SaveChanges();

            using (var db = new FeedSWUContext())
            {
                Customer customer = (from c in db.Customers
                                     where c.CustomerId == this.CustomerId
                                     select c).FirstOrDefault();
                if (customer != null)
                {
                    customer.TailsAssay = this.TailsAssay;
                    customer.LossAllowancePercentage = this.LossAllowancePercentage;
                    customer.Account.Active = this.Active;

                    db.SaveChanges();
                    return result;
                }
            }

            return false;
        }

        public Customer GetCustomer()
        {
            using (var db = new FeedSWUContext())
            {
                return (from c in db.Customers where c.CustomerId == this.CustomerId select c).FirstOrDefault();
            }
        }

        public CustomerAccount GetCustomerAccount()
        {
            using (var db = new FeedSWUContext())
            {
                return (from c in db.CustomerAccounts where c.CustomerId == this.CustomerId select c).FirstOrDefault();
            }
        }

        public static CustomerData AddCustomer(string Label,
            string ReportLabel,
            string Address = "",
            string City = "",
            string RegionStateCode = "",
            string ZipCode = "",
            string Country = "",
            double TailsAssay = 0.003,
            double LossAllowance = 0.000)
        {
            AccountData act = AccountData.AddAccount(Label, ReportLabel, Address, City, RegionStateCode, ZipCode, Country);
            Customer customer = new Customer
            {
                AccountId = act.AccountId,
                TailsAssay = (decimal)TailsAssay,
                LossAllowancePercentage = (decimal)LossAllowance
            };
            return AddCustomer(customer);
        }

        public static CustomerData AddCustomer(Customer customer)
        {
            using (var db = new FeedSWUContext())
            {
                db.Customers.Add(customer);
                db.SaveChanges();
                db.Entry(customer).GetDatabaseValues();

                //SetupEnrichedWorkingStock(db, customer);
                //SetupNaturalWorkingStock(db, customer);

                return new CustomerData(customer.CustomerId);
            }
        }

        public SiteData AddSite(string Label,
            string Address,
            string City,
            string RegionStateCode,
            string Country)
        {
            SiteData site = SiteData.AddSite(this, Label, Address, City, RegionStateCode, Country);
            this.Sites.Add(site);
            return site;
        }

        public SiteData AddSite(Site existingSite)
        {
            SiteData site = new SiteData(existingSite);
            this.Sites.Add(site);
            return site;
        }

        public SiteData AddSite(SiteData site)
        {
            this.Sites.Add(site);
            return site;
        }

        public static string NaturalWorkingStockLabel = "Natural Working Stock";
        public static CustomerSub SetupNaturalWorkingStock(FeedSWUContext db, CustomerData Customer)
        {
            CustomerSubType NewSubType = (from t in db.CustomerSubTypes
                                          where t.Label == CustomerData.NaturalWorkingStockLabel
                                          select t).FirstOrDefault();
            if (NewSubType != null)
            {
                return CustomerData.SetupCustomerSub(db, Customer, NewSubType);
            }

            return null;
        }

        public static string EnrichedWorkingStockLabel = "Enriched Working Stock";
        public static CustomerSub SetupEnrichedWorkingStock(FeedSWUContext db, CustomerData Customer)
        {
            CustomerSubType NewSubType = (from t in db.CustomerSubTypes
                                          where t.Label == CustomerData.EnrichedWorkingStockLabel
                                          select t).FirstOrDefault();
            if (NewSubType != null)
            {
                return CustomerData.SetupCustomerSub(db, Customer, NewSubType);
            }

            return null;
        }

        public static CustomerSub SetupCustomerSub(FeedSWUContext db, CustomerData Customer, CustomerSubType SubType)
        {
            //check if there is at least 1 customer sub item setup already for the customer
            CustomerSub q = (from n in db.CustomerSubs
                             where n.CustomerId == Customer.CustomerId && n.CustomerSubTypeId == SubType.CustomerSubTypeId
                             select n).FirstOrDefault<CustomerSub>();
            if (q != null)
                return q;
            else
            {
                AccountData NewAccount = AccountData.AddAccount(String.Format("{0} ({1})", SubType.Label, Customer.Label), "", "", "", "", "", "");
                CustomerSub NewCustSub = new CustomerSub
                {
                    CustomerSubTypeId = SubType.CustomerSubTypeId,
                    CustomerId = Customer.CustomerId,
                    AccountId = NewAccount.AccountId
                };
                db.CustomerSubs.Add(NewCustSub);
                int added = db.SaveChanges();

                if (added > 0)
                {
                    db.Entry(NewCustSub).GetDatabaseValues();
                    return NewCustSub;
                }
            }

            return null;
        }

        public bool RemoveSite(SiteData RemoveSite)
        {
            using (var db = new FeedSWUContext())
            {
                Site entityObject = (from a in db.Sites
                                    where a.SiteId == RemoveSite.SiteId
                                    select a).FirstOrDefault<Site>();
                db.Sites.Remove(entityObject);
                db.SaveChanges();

                this.Sites.Remove(RemoveSite);

                return true;
            }
        }

        public List<AccountPost> CreditPosts()
        {
            List<AccountPost> result = new List<AccountPost>();

            using (var db = new FeedSWUContext())
            {
                result = (from p in db.AccountPosts
                          where p.CustomerId == this.CustomerId && p.IsCredit == true
                          select p).ToList();
            }

            return result;
        }

        public List<AccountPost> DebitPosts()
        {
            List<AccountPost> result = new List<AccountPost>();

            using (var db = new FeedSWUContext())
            {
                result = (from p in db.AccountPosts
                          orderby p.EffectiveOn descending
                          where p.CustomerId == this.CustomerId && p.IsDebit == true && p.IsCredit == false
                          select p).ToList();
            }

            return result;
        }

        public List<CustomerSubType> UnAssignedSubTypes()
        {
            List<CustomerSubType> result = new List<CustomerSubType>();

            using (var db = new FeedSWUContext())
            {
                foreach (CustomerSubType SubType in (from s in db.CustomerSubTypes select s).Distinct())
                {
                    int Exist = (from ss in db.CustomerSubs
                                 where ss.CustomerSubTypeId == SubType.CustomerSubTypeId
                                    && ss.CustomerId == this.CustomerId
                                 select ss.CustomerSubId).Count();
                    if (Exist == 0)
                    {
                        result.Add(SubType);
                    }
                }
            }

            return result;
        }
    }
}
