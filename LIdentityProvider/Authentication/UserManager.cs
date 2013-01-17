using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LCHARMS.Identity;
using LCHARMS;

namespace LIdentityProvider.Authentication
{
    public class UserInfo
    {
        public string passwordHash = "";
        public LIdentity Identity = new LIdentity();
        public Dictionary<string, ChildIdentity> Children = new Dictionary<string, ChildIdentity>();
        public string pinHash = "";
    }
    public class IDRequestInfo
    {
        public string ReservationKey = "";
        public string GUID = "";
    }

    public static class UserManager
    {
        //all known Identities to this service
        public static Dictionary<string, UserInfo> Identities = new Dictionary<string, UserInfo>();
        public static Dictionary<string, IDRequestInfo> RequestedIDs = new Dictionary<string, IDRequestInfo>();
        public static List<string> Usernames = new List<string>();
        public static string DomainLRI = "";

        //username, hash
        public static Dictionary<string, UserInfo> SecurityPINHashes = new Dictionary<string, UserInfo>();


        //determine if this user exists here, and the pin is valid
        //todo: fix username request race condition
        public static bool VerifyUserAccount(string UserPinHash, LRI UserLRI)
        {
            if (Identities.ContainsKey(UserLRI.LRIString))
            {
                if (Identities[UserLRI.LRIString].pinHash == UserPinHash)
                    return true;
            }
            return false;
        }
        public static bool VerifyUserAccount(string UserPinHash, string username)
        {
            if (SecurityPINHashes.ContainsKey(username))
            {
                if (SecurityPINHashes[username].pinHash == UserPinHash)
                    return true;
            }
            return false;
        }
        public static void AddChildToParent(UserInfo info, string KeyFromChild, string ChildUserLRI)
        {
            ChildIdentity cident = new ChildIdentity();
            cident.ChildGeneratedKey = KeyFromChild;
            cident.ChildLRI = ChildUserLRI;
            cident.ParentLRI = info.Identity.UserLRI;
            info.Children[ChildUserLRI] = cident;
            SaveIdentity(new LRI(info.Identity.UserLRI));
        }
        public static bool AddChildIdentity(string ParentLRI, string username, string ChildUserLRI, string passwordhash, string ChildPinHash, string KeyFromChild, IDRequestInfo request, bool IsGroup=false)
        {
            if (RequestedIDs[request.GUID].ReservationKey == request.ReservationKey)
            {
                LRI parentParsedLRI = new LRI(ParentLRI);
                LRI childParsedLRI = new LRI(ChildUserLRI);
                UserInfo info = new UserInfo();
                info.Identity.DomainLRI = DomainLRI;
                info.Identity.OwnerDomainLRI = DomainLRI;
                info.Identity.ParentDomainLRI = parentParsedLRI.LRIDomain;
                info.Identity.ParentUserID = parentParsedLRI.DocumentID;
                info.Identity.UserID = childParsedLRI.DocumentID;
                info.Identity.Username = username;
                info.Identity.UserLRI = childParsedLRI.LRIString;
                info.passwordHash = passwordhash;
                info.pinHash = ChildPinHash;

                Identities[info.Identity.UserLRI] = info;
                Usernames.Add(username);
                RequestedIDs.Remove(request.GUID);

                //SaveIdentity(childParsedLRI);
                return true;
            }
            return false;
        }

        private static bool GUIDAvailable(string guid)
        {
            return !(RequestedIDs.ContainsKey(guid) | Identities.ContainsKey(guid));
        }
        public static IDRequestInfo ReserveGUID(string reservationKey)
        {
            IDRequestInfo info = new IDRequestInfo();
            info.ReservationKey = reservationKey;
            info.GUID = Guid.NewGuid().ToString();
            while (!GUIDAvailable(info.GUID))
            {
                info.GUID = Guid.NewGuid().ToString();
            }
            RequestedIDs[info.GUID] = info;
            return info;
        }
        //should ONLY be called by LCHARMS-CORE ID service
        public static bool AddIdentity(string username, string ChildUserLRI, string passwordhash, string ChildPinHash, string KeyFromChild, IDRequestInfo request, bool IsGroup = false)
        {
            if (RequestedIDs[request.GUID].ReservationKey == request.ReservationKey)
            {
                LRI childParsedLRI = new LRI(ChildUserLRI);
                UserInfo info = new UserInfo();
                info.Identity.DomainLRI = DomainLRI;
                info.Identity.OwnerDomainLRI = DomainLRI;
                info.Identity.ParentDomainLRI = "~LCHARMS-CORE~";
                info.Identity.ParentUserID = "";
                info.Identity.UserID = childParsedLRI.DocumentID;
                info.Identity.Username = username;
                info.Identity.UserLRI = childParsedLRI.LRIString;
                info.passwordHash = passwordhash;
                info.pinHash = ChildPinHash;

                Identities[info.Identity.UserLRI] = info;
                Usernames.Add(username);
                RequestedIDs.Remove(request.GUID);

                //SaveIdentity(childParsedLRI);
                return true;
            }
            return false;
        }

        //public bool CreateChildIdentity(string ParentLRI, string ChildUserLRI, string passwordhash, string ChildPinHash, string KeyFromChild)
        //{


        //    return true;
        //}
        //should be overloaded, called when an ID is added to save it to a persistant location
        public static void SaveIdentity(LRI userLRI)
        {
        }
        public static void LoadIdentities()
        {
        }
    }
}