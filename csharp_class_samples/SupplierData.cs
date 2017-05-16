using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using feedAndSWUAccounting.Controllers;

namespace feedAndSWUAccounting.Models
{
    public class SupplierData : AccountData
    {
        public int SupplierId { get; set; }
        public string AccountCode { get; set; }
        public RISCode RISCode { get; private set; }
        public decimal TailsAssay { get; private set; }
        public List<SupplierSubData> SupplierSubs { get; private set; }

        public SupplierData() { }

        public SupplierData(int Id)
        {
            LoadData(Id);
        }

        public SupplierData(Supplier entityObject)
        {
            LoadData(entityObject);
        }

        public void LoadData(int Id)
        {
            using (var db = new FeedSWUContext())
            {
                Supplier entityObject = (from a in db.Suppliers
                                 where a.SupplierId == Id
                                 select a).FirstOrDefault<Supplier>();
                LoadData(entityObject);
            }
        }

        public void LoadData(Supplier entityObject)
        {
            if (entityObject != null)
            {
                this.LoadAccountData(entityObject.Account);
                this.SupplierId = entityObject.SupplierId;
                this.AccountCode = entityObject.AccountCode;
                this.RISCode = entityObject.RISCode;
                this.TailsAssay = entityObject.TailsAssay.Value;

                this.SupplierSubs = new List<SupplierSubData>();
                foreach(SupplierSub subAcct in entityObject.SupplierSubs)
                {
                    this.SupplierSubs.Add(new SupplierSubData(subAcct));
                }
            }
        }

        public static SupplierData AddSupplier(RISCode risCode,
            string AccountCode,
            string Label,
            string ReportLabel,
            string Address = "",
            string City = "",
            string RegionStateCode = "",
            string ZipCode = "",
            string Country = "",
            double TailsAssay = 0.003)
        {
            AccountData act = AccountData.AddAccount(Label, ReportLabel, Address, City, RegionStateCode, ZipCode, Country);
            Supplier supplier = new Supplier
            {
                AccountId = act.AccountId,
                AccountCode = AccountCode,
                RISCodeId = risCode.RISCodeId,
                TailsAssay = (decimal)TailsAssay
            };
            return AddSupplier(supplier);
        }

        public static SupplierData AddSupplier(Supplier supplier)
        {
            using (var db = new FeedSWUContext())
            {
                db.Suppliers.Add(supplier);
                db.SaveChanges();
                db.Entry(supplier).GetDatabaseValues();

                return new SupplierData(supplier.SupplierId);
            }
        }

        public SupplierSubData AddSupplierSub(RISCode risCode,
            string AccountCode,
            string Label,
            string Address,
            string City,
            string RegionStateCode,
            string Country)
        {
            SupplierSubData supplierSub = SupplierSubData.AddSupplierSub(this, risCode, AccountCode, Label, Address, City, RegionStateCode, Country);
            this.SupplierSubs.Add(supplierSub);
            return supplierSub;
        }

        public SupplierSubData AddSupplierSub(SupplierSub existing)
        {
            SupplierSubData supplierSub = new SupplierSubData(existing);
            this.SupplierSubs.Add(supplierSub);
            return supplierSub;
        }

        public SupplierSubData AddSupplierSub(SupplierSubData supplierSub)
        {
            this.SupplierSubs.Add(supplierSub);
            return supplierSub;
        }

        public bool RemoveSupplierSub(SupplierSubData RemoveSupplierSub)
        {
            using (var db = new FeedSWUContext())
            {
                SupplierSub entityObject = (from a in db.SupplierSubs
                                           where a.SupplierSubId == RemoveSupplierSub.SupplierSubId
                                           select a).FirstOrDefault<SupplierSub>();
                db.SupplierSubs.Remove(entityObject);
                db.SaveChanges();

                this.SupplierSubs.Remove(RemoveSupplierSub);

                return true;
            }
        }

        public List<AccountPost> CreditPosts()
        {
            List<AccountPost> result = new List<AccountPost>();

            using (var db = new FeedSWUContext())
            {
                result = (from p in db.AccountPosts
                          where p.SupplierId == this.SupplierId && p.IsCredit == true
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
                          where p.SupplierId == this.SupplierId && p.IsDebit == true && p.IsCredit == false
                          select p).ToList();
            }

            return result;
        }
    }
}
