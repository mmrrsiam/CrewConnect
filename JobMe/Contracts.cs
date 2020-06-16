using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JobMe
{
    public class UserInfo
    {
        public int UserID;
        public string IP;
        public string Username;
        public string FullName;
        public string Photo;
        public string Email;
        public bool Active;
        public static UserInfo FromTable(User user)
        {
            using (JobMeEntities dbc = new JobMeEntities())
            {
                return new UserInfo() { UserID = user.UserID, FullName = user.Name, Username = user.Username, Active = user.Active };
            }
        }
    }
}