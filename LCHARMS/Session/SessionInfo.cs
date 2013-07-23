using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LCHARMS.Identity;
using System.Runtime.Serialization;
using LCHARMS;
using LCHARMS.Logging;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace LCHARMS.Session
{
    [DataContract]
    [Serializable]
    public enum SESSION_ERROR {
        [EnumMember]
        NONE=0,
        [EnumMember]
        INVALID_CREDENTIALS=1,
        [EnumMember]
        PARENT_NOT_LOGGED_IN = 2, //also for session mis-match, we don't want them to know which
        [EnumMember]
        USERNAME_EXISTS = 3,
        [EnumMember]
        INVALID_USERNAME = 4,
        [EnumMember]
        PARENT_DOMAIN_RESTRICTED = 5, //we have a blacklist or a whitelist and your domain isn't allowed
        [EnumMember]
        PARENT_DOMAIN_USERLIMIT = 6, //sorry, you used this parent account too many times
        [EnumMember]
        INVALID_PARENT_CREDENTIALS = 7,

    }

    [DataContract]
    [Serializable]
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
        public SESSION_ERROR ErrorType = SESSION_ERROR.NONE;
        [DataMember]
        public string SessionErrorMessage = "";
    }

    [Serializable]
    public  class SessionManager
    {
        public SortedDictionary<string, SessionInfo> Sessions = new SortedDictionary<string, SessionInfo>();
        public SessionInfo NewSession(LIdentity ID)
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
            SaveSession(info.SessionKey);
            FDebugLog.WriteLog("New Session : "  + ID.UserLRI + " ("+sessionkey+")");
            return info;
        }
        public void SaveSession(string SessionKey)
        {
            //save each individual session to a file
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("c:\\logs\\bin\\Session-"+SessionKey+"-.bin", FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, Sessions[SessionKey]);
            stream.Close();
        }
        public void LoadSession(string SessionKey)
        {
            if (File.Exists("c:\\logs\\bin\\Session-"+SessionKey+"-.bin"))
            {
                FDebugLog.WriteLog("Loading Session: " + SessionKey);
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream("c:\\logs\\bin\\Session-" + SessionKey + "-.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
                SessionInfo info = (SessionInfo)formatter.Deserialize(stream);
                Sessions[SessionKey] = info;
                stream.Close();
            }
        }
        public void Logout(string SessionKey)
        {
            LoadSession(SessionKey);
            if (Sessions.ContainsKey(SessionKey))
            {
                Sessions.Remove(SessionKey);
                if (File.Exists("c:\\logs\\bin\\Session-"+SessionKey+"-.bin"))
                {
                    File.Delete("c:\\logs\\bin\\Session-" + SessionKey + "-.bin");
                }
                FDebugLog.WriteLog("Logging out: " + SessionKey);
            }
        }
        public bool VerifySessionKey(string sessionkey, LRI UserLRI)
        {
            LoadSession(sessionkey);
            if (Sessions.ContainsKey(sessionkey))
            {
                FDebugLog.WriteLog("Verifying Key : " + sessionkey);
                if (Sessions[sessionkey].Identity.UserLRI == UserLRI.LRIString)
                    return true;
            }
            return false;
        }
        public void RefreshTime(string sessionkey)
        {
            //todo: throttle refresh requests
            LoadSession(sessionkey);
            Sessions[sessionkey].Expires = DateTime.Now.AddHours(1);
            SaveSession(sessionkey);
        }

        public void CleanupExpired()
        {

        }
    }

}