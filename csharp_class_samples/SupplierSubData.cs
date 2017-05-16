using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feedAndSWUAccounting.Models
{
    public class SupplierSubData : AccountData
    {
        public int SupplierSubId { get; set; }
        public int SupplierId { get; private set; }
        public string AccountCode { get; set; }
        public RISCode RISCode { get; private set; }

        public SupplierSubData() { }

        public SupplierSubData(int Id)
        {
            LoadData(Id);
        }

        public SupplierSubData(SupplierSub entityObject)
        {
            LoadData(entityObject);
        }

        public void LoadData(int Id)
        {
            using (var db = new FeedSWUContext())
            {
                SupplierSub entityObject = (from a in db.SupplierSubs
                                 where a.SupplierSubId == Id
                                           select a).FirstOrDefault<SupplierSub>();
                LoadData(entityObject);
            }
        }

        public void LoadData(SupplierSub entityObject)
        {
            if (entityObject != null)
            {
                this.LoadAccountData(entityObject.Account);
                this.SupplierSubId = entityObject.SupplierSubId;
                this.SupplierId = entityObject.SupplierId;

                this.RISCode = entityObject.RISCode;
            }
        }

        public static SupplierSubData AddSupplierSub(SupplierData parentSupplier,
            RISCode risCode,
            string AccountCode,
            string Label,
            string ReportLabel,
            string Address = "",
            string City = "",
            string RegionStateCode = "",
            string ZipCode = "",
            string Country = "")
        {
            AccountData act = AccountData.AddAccount(Label, ReportLabel, Address, City, RegionStateCode, ZipCode, Country);
            SupplierSub supplierSub = new SupplierSub
            {
                AccountId = act.AccountId,
                SupplierId = parentSupplier.SupplierId,
                AccountCode = AccountCode,
                RISCodeId = risCode.RISCodeId
            };
            return AddSupplierSub(supplierSub);
        }

        public static SupplierSubData AddSupplierSub(SupplierSub supplierSub)
        {
            using (var db = new FeedSWUContext())
            {
                db.SupplierSubs.Add(supplierSub);
                db.SaveChanges();
                db.Entry(supplierSub).GetDatabaseValues();

                return new SupplierSubData(supplierSub.SupplierSubId);
            }
        }
    }
}
