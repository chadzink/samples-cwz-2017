using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Data.Entity;
using System.Linq;

namespace feedAndSWUAccounting.Models
{
	public class User
	{
        private IList<UserFeatureAccess> _UserFeatureAccess;
        public List<string> ApplicationFeaturesCodes
        {
            get {
                return (from u in this._UserFeatureAccess select u.ShortCode.ToString()).ToList<string>();
            }
        }
        public bool IsRegisterUser
        {
            get {
                return _UserFeatureAccess.Count > 0;
            }
        }

        public int UserRoleId { get; private set; }

        private bool? _CanAuthorizeUsers = null;
        public bool CanAuthorizeUsers
        {
            get
            {
                if (_CanAuthorizeUsers == null)
                {
                    using (var db = new FeedSWUContext())
                    {
                        this._CanAuthorizeUsers = (from ur in db.UserRoles
                                join r in db.Roles
                                    on ur.RoleId equals r.RoleId
                                where ur.UserRoleId == this.UserRoleId
                                select r.CanAuthorize).FirstOrDefault();
                    }
                }

                return this._CanAuthorizeUsers == null ? false : this._CanAuthorizeUsers.Value;
            }
        }

        public int UsersPendingAuthorization
        {
            get
            {
                if (this.CanAuthorizeUsers)
                {
                    using (var db = new FeedSWUContext())
                    {
                        return (from u in db.UserRoles
                                     where u.RequestedRoleId != u.RoleId
                                     select u).Count<UserRole>();
                    }
                }
                return 0;
            }
        }

        // Active Directory properties

        private string _adLoginName;
        public string UserName
        {
            get { return this._adLoginName; }
            private set { this._adLoginName = value; }
        }

        private string _adDisplayName;
        public string DisplayName
        {
            get { return this._adDisplayName; }
            private set { this._adDisplayName = value; }
        }

        //Constructor START

		public User()
		{
            this.DisplayName = UserPrincipal.Current.DisplayName;
            this.UserName = UserPrincipal.Current.Name;

            Init();
		}

        public void Init()
        {
            using (var db = new FeedSWUContext())
            {
                this._UserFeatureAccess = (from r in db.UserFeatureAccesses
                                           where r.ActiveDirectoryUserName == this.UserName
                                           select r).ToList<UserFeatureAccess>();
                if (this.IsRegisterUser)
                {
                    List<UserRole> query = (from r in db.UserRoles
                                     where r.ActiveDirectoryUserName == this.UserName
                                     select r).ToList<UserRole>();
                    if (query.Count > 0)
                    {
                        this.UserRoleId = query.First<UserRole>().UserRoleId;
                    }
                }
            }
        }
	}
}
