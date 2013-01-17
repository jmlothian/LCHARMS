using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using LIdentityProvider.Session;
using LIdentityProvider.Authentication;
using LCHARMS;

namespace LIdentityProvider
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    public class LIdentityProvider : ILIdentityProvider
    {


#region CALLED BY CHILD
        public string RequestParentIDAuth(string ChildUserLRI, string Username, string ParentPINHash, string KeyFromChild, string SessionKey)
        {
            string userid = "";
            if (UserManager.VerifyUserAccount(Username, ParentPINHash))
            {
                //account is real
                userid = UserManager.SecurityPINHashes[Username].Identity.UserID;
                UserManager.AddChildToParent(UserManager.SecurityPINHashes[Username], KeyFromChild, ChildUserLRI);
            }
            return userid;
        }


        /*public string RequestUserID(int ParentPinHash, string username)
        {
            
            return info;
        }*/
#endregion
#region CALLED BY LOCAL
        private string RetrieveUserParentAuth(LRI FromLRI, string Username, string FromParentPinHash,string KeyFromChild, string SessionKey, string ChildLRI)
        {
            string userid = "";
            var myBinding = new BasicHttpBinding();
            var myEndpoint = new EndpointAddress(FromLRI.URI);
            var myChannelFactory = new ChannelFactory<ILIdentityProvider>(myBinding, myEndpoint);

            ILIdentityProvider client = null;

            try
            {
                client = myChannelFactory.CreateChannel();
                userid = client.RequestParentIDAuth(ChildLRI, Username, FromParentPinHash, KeyFromChild, SessionKey);
                ((ICommunicationObject)client).Close();
            }
            catch
            {
                if (client != null)
                {
                    ((ICommunicationObject)client).Abort();
                }
            }

            return userid;
        }

        public SessionInfo CreateIdentity(string ParentDomain, string ParentUser, string ParentPINHash, string username, string passwordhash, string ChildPinHash, string SessionKey)
        {
            SessionInfo sessinfo = null;
            //create temporary user w/ key (reserve userid)
            string ReservationKey = Guid.NewGuid().ToString();
            IDRequestInfo info = UserManager.ReserveGUID(ReservationKey);

            string UserLRI = UserManager.DomainLRI + "/~users/" + info.GUID;
            string UserID = info.GUID;

            //generate child key
            string ChildKey = Guid.NewGuid().ToString();


            //construct parentLRI
            LRI ParentLRI = new LRI(ParentDomain);
            //get parent userid from parent domain
            string parentUserID = RetrieveUserParentAuth(ParentLRI, ParentUser, ParentPINHash, ChildKey, SessionKey,UserLRI);
            if (ParentUser != "")
            {

                //CreateChildIdentity
                bool addSucceed = UserManager.AddChildIdentity(ParentLRI.LRIString, username, UserLRI, passwordhash, ChildPinHash, ChildKey, info);
                //login user
                sessinfo = LoginID(UserLRI, passwordhash);
            }

            return sessinfo;
        }

        //private GetParentUserID(string ParentDomain, string ParentUsername, )
        //{
        //}

        //this may not be in the interface...


        //returns session info
        public SessionInfo LoginID(string UserLRI, string passwordhash, bool LoginChildren = false)
        {
            SessionInfo info = new SessionInfo();
            //validate user credentials

            //validate parent ID is logged in

            //login user
                //generate session key
                //fill in info
               
            //login children
            return info;
        }

        public bool ValidateParentID(string ChildIDLRI, string ParentKey, string KeyFromChild)
        {
            return true;
        }

        public bool LoginChild(string ParentLRI, string ChildUserLRI, string KeyFromChild, bool LoginChildren = true)
        {
            //login this account

            //login all known children for this account
            return true;
        }

        public int LCHARMSIDProviderVersion()
        {
            return 1;
        }
        public bool Ping()
        {
            return true;
        }

#endregion

    }
}
