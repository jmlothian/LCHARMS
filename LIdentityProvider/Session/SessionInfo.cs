using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LCHARMS.Identity;
using System.Runtime.Serialization;
using LCHARMS;

namespace LIdentityProvider.Session
{
    [DataContract]
    public enum SESSION_ERROR {NONE=0, INVALID_CREDENTIALS=1, PARENT_NOT_LOGGED_IN=2 }

    [DataContract]
    public class SessionInfo
    {
        [DataMember]
        public DateTime Created = DateTime.Now;
        [DataMember]
        public DateTime Expires;
        [DataMember]
        public LIdentity Identity = new LIdentity();
        [DataMember]
        public string SessionKey = "";
        [DataMember]
        public bool Error = false;
        [DataMember]
        SESSION_ERROR ErrorType = SESSION_ERROR.NONE;
    }

    public static class SessionManager
    {
        public static SortedDictionary<string, SessionInfo> Sessions = new SortedDictionary<string, SessionInfo>();
        public static SessionInfo NewSession(LIdentity ID)
        {
            Random rnd = new Random();
            SessionInfo info = new SessionInfo();
            info.Identity = ID;
            info.Created = DateTime.Now;
            info.Expires = DateTime.Now.AddHours(1);
            string sessionkey = Guid.NewGuid().ToString();
            while(Sessions.ContainsKey(sessionkey))
                sessionkey = Guid.NewGuid().ToString();
            info.SessionKey = sessionkey;
            Sessions[info.SessionKey] = info;
            return info;
        }
        public static bool VerifySessionKey(string sessionkey, LRI UserLRI)
        {
            if (Sessions.ContainsKey(UserLRI.LRIString))
            {
                if (Sessions[UserLRI.LRIString].SessionKey == sessionkey)
                    return true;
            }
            return false;
        }
        public static void RefreshTime(string sessionkey)
        {
            //todo: throttle refresh requests
            Sessions[sessionkey].Expires = DateTime.Now.AddHours(1);
        }

        public static void CleanupExpired()
        {

        }
    }

}