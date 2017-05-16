using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using feedAndSWUAccounting.Controllers;

namespace feedAndSWUAccounting.Models
{
    public class PostData
    {
        public int PostId { get; private set; }
        public int TransactionId { get; private set; }
        public int AccountId { get; private set; }
        public bool IsDebit { get; private set; }
        public bool IsCredit { get; private set; }
        public string MaterialType { get; private set; }
        public decimal KgU { get; private set; }
        public decimal KgU235 { get; private set; }
        public decimal EnrichmentAssay { get; private set; }
        public decimal TailsAssay { get; private set; }
        public decimal NaturalFeedAssay { get; private set; }
        public decimal AdjustToFeed { get; private set; }
        public decimal AdjustToSWU { get; private set; }
        public decimal TailsAdjustment { get; private set; }
        public DateTime EffectiveOn { get; private set; }
        public Obligation Obligation { get; private set; }
        public Origin MiningOrigin { get; private set; }
        public Origin ConversionOrigin { get; private set; }
        public Origin EnrichmentOrigin { get; private set; }
        public OffspecType OffspecType { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string CreatedBy { get; private set; }
        public DateTime RemovedAt { get; private set; }
        public string RemovedBy { get; private set; }

        public PostData() { }

        public PostData(int PostId)
        {
            LoadPostData(PostId);
        }

        public PostData(Post post)
        {
            LoadPostData(post);
        }

        public void LoadPostData(int PostId)
        {
            using (var db = new FeedSWUContext())
            {
                Post post = (from a in db.Posts
                               where a.PostId == PostId
                               select a).FirstOrDefault<Post>();
                LoadPostData(post);
            }
        }

        public void LoadPostData(Post post)
        {
            if (post != null)
            {
                this.PostId = post.PostId;
                this.TransactionId = post.TransactionId;
                this.AccountId = post.AccountId;
                this.IsDebit = post.IsDebit.GetValueOrDefault(false);
                this.IsCredit = post.IsCredit.GetValueOrDefault(false);
                this.MaterialType = post.MaterialType;
                this.KgU = post.KgU.GetValueOrDefault(0);
                this.KgU235 = post.KgU235.GetValueOrDefault(0);
                this.EnrichmentAssay = post.EnrichmentAssay.GetValueOrDefault(0);
                this.TailsAssay = post.TailsAssay.GetValueOrDefault(0);
                this.NaturalFeedAssay = post.NaturalFeedAssay.GetValueOrDefault(0.711M);
                this.AdjustToFeed = post.AdjustToFeed.GetValueOrDefault(0);
                this.AdjustToSWU = post.AdjustToSWU.GetValueOrDefault(0);
                this.TailsAdjustment = post.TailsAdjustment.GetValueOrDefault(0);
                this.EffectiveOn = post.EffectiveOn.GetValueOrDefault();
                this.Obligation = ObligationController.FindById(post.ObligationId.GetValueOrDefault(-1));
                this.MiningOrigin = OriginController.FindById(post.MiningOriginId.GetValueOrDefault(-1));
                this.ConversionOrigin = OriginController.FindById(post.ConversionOriginId.GetValueOrDefault(-1));
                this.EnrichmentOrigin = OriginController.FindById(post.EnrichmentOriginId.GetValueOrDefault(-1));
                this.OffspecType = OffspecTypeController.FindById(post.OffspecTypeId.GetValueOrDefault(-1));
                this.CreatedAt = post.CreatedAt.HasValue ? post.CreatedAt.Value : DateTime.MinValue;
                this.CreatedBy = post.CreatedBy;
                this.RemovedAt = post.RemovedAt.HasValue ? post.RemovedAt.Value : DateTime.MaxValue;
                this.RemovedBy = post.RemovedBy;
            }
        }

        public bool SaveChanges()
        {
            using (var db = new FeedSWUContext())
            {
                Post post = (from a in db.Posts
                               where a.PostId == this.PostId
                               select a).FirstOrDefault<Post>();

                if (post != null)
                {
                    post.TransactionId = this.TransactionId;
                    post.AccountId = this.AccountId;
                    post.IsDebit = this.IsDebit;
                    post.IsCredit = this.IsCredit;
                    post.MaterialType = this.MaterialType;
                    post.KgU = this.KgU;
                    post.KgU235 = this.KgU235;
                    post.EnrichmentAssay = this.EnrichmentAssay;
                    post.TailsAssay = this.TailsAssay;
                    post.NaturalFeedAssay = this.NaturalFeedAssay;
                    post.AdjustToFeed = this.AdjustToFeed;
                    post.AdjustToSWU = this.AdjustToSWU;
                    post.TailsAdjustment = this.TailsAdjustment;
                    post.EffectiveOn = this.EffectiveOn;
                    post.ObligationId = this.Obligation.ObligationId;
                    post.MiningOriginId = this.MiningOrigin.OriginId;
                    post.ConversionOriginId = this.ConversionOrigin.OriginId;
                    post.EnrichmentOriginId = this.EnrichmentOrigin.OriginId;
                    post.OffspecTypeId = this.OffspecType.OffspecTypeId;
                    post.CreatedAt = this.CreatedAt;
                    post.CreatedBy = this.CreatedBy;
                    post.RemovedAt = this.RemovedAt;
                    post.RemovedBy = this.RemovedBy;

                    db.SaveChanges();
                    return true;
                }
            }

            return false;
        }
    }
}
