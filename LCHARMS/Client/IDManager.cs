using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LCHARMS.Session;
using System.Security.Cryptography;

namespace LCHARMS.Client
{
    public class IDInfo
    {
        public IDInfo(string lri)
        {
            LRI = lri;
        }
        public enum ID_STATUS {CLOSED, OPEN, CONNECTING};
        public ID_STATUS Status = ID_STATUS.CLOSED;
        public string LRI = "";
        public SessionInfo Session = null;
    }

    //utility class that wraps an ILIdentityProvider
    public class IDManager
    {
        private LConnectionManager ConnectionManager = null;
        public IDManager(LConnectionManager ConnManager)
        {
            ConnectionManager = ConnManager;
        }
        public Dictionary<string, IDInfo> Sessions = new Dictionary<string, IDInfo>();
        public bool CreateCoreID(string username, LRI ServiceLRI, string Password, string Pin)
        {
            //this will hash it for you
            SHA1 hasher = SHA1.Create();
            return CreateCoreIDWithHash(username, ServiceLRI, 
                BitConverter.ToString(hasher.ComputeHash(System.Text.Encoding.UTF8.GetBytes(Password))).Replace("-",string.Empty),
                BitConverter.ToString(hasher.ComputeHash(System.Text.Encoding.UTF8.GetBytes(Pin))).Replace("-", string.Empty));
        }
        public bool CreateCoreIDWithHash(string username, LRI ServiceLRI, string PasswordHash, string PinHash)
        {
            SessionInfo info = ConnectionManager.GetIDConnection(ServiceLRI).CreateIdentity("", "", "", username, PasswordHash, PinHash, "");
            if (!info.Error)
            {
                IDInfo idinfo = new IDInfo(info.Identity.UserLRI);
                idinfo.Session = info;
                idinfo.Status = IDInfo.ID_STATUS.OPEN;
                Sessions[idinfo.LRI] = idinfo;
                return true;
            }
            return false;
        }
        //returns true on success
        public bool CreateChildID(LRI ParentLRI, string ParentPIN, string username, LRI ServiceLRI, string Password, string Pin)
        {
            //this will hash it for you
            SHA1 hasher = SHA1.Create();
            return CreateChildIDWithHash(ParentLRI,
                BitConverter.ToString(hasher.ComputeHash(System.Text.Encoding.UTF8.GetBytes(ParentPIN))).Replace("-", string.Empty),
                username, ServiceLRI,
                BitConverter.ToString(hasher.ComputeHash(System.Text.Encoding.UTF8.GetBytes(Password))).Replace("-", string.Empty),
                BitConverter.ToString(hasher.ComputeHash(System.Text.Encoding.UTF8.GetBytes(Pin))).Replace("-", string.Empty));
        }
        public void Logout(LRI lri)
        {
            ConnectionManager.GetIDConnection(new LRI(lri.BaseLRI)).Logout(lri.LRIString, Sessions[lri.LRIString].Session.SessionKey);
            Sessions[lri.LRIString].Status = IDInfo.ID_STATUS.CLOSED;
            Sessions[lri.LRIString].Session = null;
        }
        public bool Login(LRI lri, string Password)
        {
            SHA1 hasher = SHA1.Create();
            return LoginWithHash(lri,BitConverter.ToString(hasher.ComputeHash(System.Text.Encoding.UTF8.GetBytes(Password))).Replace("-", string.Empty));
        }
        public bool LoginWithHash(LRI lri, string PasswordHash)
        {
            SessionInfo info = ConnectionManager.GetIDConnection(new LRI(lri.BaseLRI)).LoginID(lri.LRIString, PasswordHash);
            if (!info.Error)
            {
                if (!Sessions.ContainsKey(info.Identity.UserLRI))
                {
                    IDInfo idinfo = new IDInfo(info.Identity.UserLRI);
                    Sessions[idinfo.LRI] = idinfo;
                }
                Sessions[info.Identity.UserLRI].Session = info;
                Sessions[info.Identity.UserLRI].Status = IDInfo.ID_STATUS.OPEN;
                return true;
            }
            return false;
        }
        public bool CreateChildIDWithHash(LRI ParentLRI, string ParentPINHash, string username, LRI ServiceLRI, string PasswordHash, string PinHash)
        {

            if (Sessions.ContainsKey(ParentLRI.LRIString))
            {
                IDInfo parent = Sessions[ParentLRI.LRIString];
                SessionInfo info = ConnectionManager.GetIDConnection(ServiceLRI).CreateIdentity(
                    ParentLRI.LRIString, parent.Session.Identity.Username, ParentPINHash, 
                    username, PasswordHash, PinHash, parent.Session.SessionKey);
                if (!info.Error)
                {
                    IDInfo idinfo = new IDInfo(info.Identity.UserLRI);
                    idinfo.Session = info;
                    idinfo.Status = IDInfo.ID_STATUS.OPEN;
                    Sessions[idinfo.LRI] = idinfo;
                    return true;
                } 
            }
            return false;
        }
    }
}
