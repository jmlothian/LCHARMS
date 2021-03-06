﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LCHARMS.Identity;
using LCHARMS;
using LCHARMS.DB.CouchDB;
using System.Runtime.Serialization;
using LCHARMS.Document;

namespace LIdentityProvider.Authentication
{
    [DataContract]
    public class UserInfo
    {
        [DataMember]
        public string passwordHash = "";
        [DataMember]
        public LIdentity Identity = new LIdentity();
        [DataMember]
        public Dictionary<string, ChildIdentity> Children = new Dictionary<string, ChildIdentity>();
        [DataMember]
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
        public static bool VerifyLocalUserAccount(string passwordhash, LRI UserLRI)
        {
            if (Identities.ContainsKey(UserLRI.LRIString))
            {
                if (Identities[UserLRI.LRIString].passwordHash == passwordhash)
                    return true;
            }
            return false; ;
        }
        public static bool VerifyChildUserAccount(string ChildKey, LRI UserLRI)
        {
            if (Identities.ContainsKey(UserLRI.LRIString))
            {
                if (Identities[UserLRI.LRIString].Identity.KeyForParent == ChildKey)
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
                info.Identity.KeyForParent = KeyFromChild;
                Identities[info.Identity.UserLRI] = info;
                Usernames.Add(username);
                RequestedIDs.Remove(request.GUID);

                SaveIdentity(childParsedLRI);
                return true;
            }
            return false;
        }

        public static IDRequestInfo ReserveGUID(string reservationKey)
        {
            IDRequestInfo info = new IDRequestInfo();
            info.ReservationKey = reservationKey;
            info.GUID = LDocManager.RequestGUID();
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

                SaveIdentity(childParsedLRI);
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
            LDocumentHeader header = LDocManager.GetDocHeader(userLRI);
            if (header == null)
            {
                //create it
                header = new LDocumentHeader();
                header.DocumentLRI = userLRI.LRIString;
                header.FQDT = "lcharms.user";
                header.DocumentID = userLRI.DocumentID;
                header.DocType = DocumentType.DOC_HEADER;
                List<LDocumentPart> parts = new List<LDocumentPart>();
                LDocumentPart idPart = new LDocumentPart();
                idPart.DocumentID = userLRI.DocumentID;
                idPart.SequenceNumber = 0;
                idPart.DocType = DocumentType.DOC_PART;
                parts.Add(idPart);
                LDocManager.CreateDoc(userLRI, header, parts);
            }
            else
            {

            }
            //CouchDBMgr.WriteDocument(userLRI.DocumentID, Identities[userLRI.LRIString]);
        }
        public static void LoadIdentities()
        {
        }
    }
}