using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace feedAndSWUAccounting.Models
{
    public class ReloadData
    {
        public int ReloadId { get; private set; }
        public int ReactorId { get; private set; }
        public string ProjectNumber { get; set; }
        public string Cycle { get; set; }

        public ReloadData() { }

        public ReloadData(int Id)
        {
            LoadData(Id);
        }

        public ReloadData(Reload entityObject)
        {
            LoadData(entityObject);
        }

        public void LoadData(int Id)
        {
            using (var db = new FeedSWUContext())
            {
                Reload entityObject = (from a in db.Reloads
                                 where a.ReloadId == Id
                                      select a).FirstOrDefault<Reload>();
                LoadData(entityObject);
            }
        }

        public void LoadData(Reload entityObject)
        {
            if (entityObject != null)
            {
                this.Cycle = entityObject.Cycle;
                this.ReactorId = entityObject.ReactorId;
                this.ReloadId = entityObject.ReloadId;
            }
        }

        public static ReloadData AddReload(ReactorData parentReactor,
            string ProjectNumber,
            string Cycle)
        {
            Reload reload = new Reload
            {
                ReactorId = parentReactor.ReactorId,
                ProjectNumber = ProjectNumber,
                Cycle = Cycle
            };
            return AddReload(reload);
        }

        public static ReloadData AddReload(Reload reload)
        {
            using (var db = new FeedSWUContext())
            {
                db.Reloads.Add(reload);
                db.SaveChanges();
                db.Entry(reload).GetDatabaseValues();

                return new ReloadData(reload.ReloadId);
            }
        }
    }
}
