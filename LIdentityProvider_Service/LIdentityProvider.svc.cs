using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using LIdentityProvider;
using LIdentityProvider.Session;
using LCHARMS;
using LIdentityProvider.Authentication;
using LCHARMS.Identity;

namespace LIdentityProvider_Service
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
        public bool ValidateParentID(string ChildIDLRI, string KeyFromChild)
        {

            return true;
        }

        /*public string RequestUserID(int ParentPinHash, string username)
        {
            
            return info;
        }*/
        #endregion
        #region CALLED BY LOCAL
        private string RetrieveUserParentAuth(LRI FromLRI, string Username, string FromParentPinHash, string KeyFromChild, string SessionKey, string ChildLRI)
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
            SessionInfo sessinfo = new SessionInfo();
            //create temporary user w/ key (reserve userid)
            string ReservationKey = Guid.NewGuid().ToString();
            IDRequestInfo info = UserManager.ReserveGUID(ReservationKey);

            string UserLRI = UserManager.DomainLRI + "/~users/" + info.GUID;
            string UserID = info.GUID;

            //generate child key
            string ChildKey = Guid.NewGuid().ToString();

            if (ParentDomain != null && ParentDomain != "")
            {
                //construct parentLRI
                LRI ParentLRI = new LRI(ParentDomain);
                //get parent userid from parent domain
                string parentUserID = RetrieveUserParentAuth(ParentLRI, ParentUser, ParentPINHash, ChildKey, SessionKey, UserLRI);
                if (ParentUser != "")
                {

                    //CreateChildIdentity
                    bool addSucceed = UserManager.AddChildIdentity(ParentLRI.LRIString, username, UserLRI, passwordhash, ChildPinHash, ChildKey, info);
                    //login user
                    sessinfo = LoginID(UserLRI, passwordhash);
                }
            }
            else
            {

                //CreateChildIdentity
                bool addSucceed = UserManager.AddIdentity(username, UserLRI, passwordhash, ChildPinHash, ChildKey, info);
                //login user
                sessinfo = LoginID(UserLRI, passwordhash);
            }
            return sessinfo;
        }
        public LRI ParseLRI(string lri, bool IsURI = false)
        {
            return new LRI(lri, IsURI);
        }
        //private GetParentUserID(string ParentDomain, string ParentUsername, )
        //{
        //}


        //returns session info
        public SessionInfo LoginID(string UserLRI, string passwordhash, bool LoginChildren = false)
        {
            SessionInfo info = new SessionInfo();
            //validate user credentials
            if (UserManager.VerifyUserAccount(passwordhash, new LRI(UserLRI)))
            {
                UserInfo uinfo = UserManager.Identities[UserLRI];
                //uinfo.Identity.ParentDomainLRI
                //validate parent ID is logged in
                var myBinding = new BasicHttpBinding();
                var myEndpoint = new EndpointAddress(new LRI(uinfo.Identity.ParentDomainLRI).URI);
                var myChannelFactory = new ChannelFactory<ILIdentityProvider>(myBinding, myEndpoint);

                ILIdentityProvider client = null;

                bool parentLoggedIn = false;

                try
                {
                    client = myChannelFactory.CreateChannel();
                    parentLoggedIn = client.ValidateParentID(uinfo.Identity.UserLRI, uinfo.Identity.KeyForParent);
                    ((ICommunicationObject)client).Close();
                }
                catch
                {
                    if (client != null)
                    {
                        ((ICommunicationObject)client).Abort();
                    }
                }

                if (parentLoggedIn)
                {
                    //login user
                    //generate session key
                    info = SessionManager.NewSession(uinfo.Identity);
                    info.Identity.KeyForParent = "";

                    //login children
                    if (uinfo.Children.Count > 0)
                    {
                        foreach (KeyValuePair<string, ChildIdentity> child in uinfo.Children)
                        {
                            var childBinding = new BasicHttpBinding();
                            var childEndpoint = new EndpointAddress(new LRI(child.Value.ChildLRI).URI);
                            var childChannelFactory = new ChannelFactory<ILIdentityProvider>(childBinding, childEndpoint);
                            try
                            {
                                client = childChannelFactory.CreateChannel();
                                client.LoginChild(uinfo.Identity.ParentUserID, child.Value.ChildLRI, child.Value.ChildGeneratedKey, true);//need a version of this that allows the childpin to be used!
                                ((ICommunicationObject)client).Close();
                            }
                            catch
                            {
                                if (client != null)
                                {
                                    ((ICommunicationObject)client).Abort();
                                }
                            }
                        }
                    }
                }
            }
            return info;
        }



        public bool LoginChild(string ParentLRI, string ChildUserLRI, string KeyFromChild, bool LoginChildren = true)
        {
            //login this account
            SessionInfo info = new SessionInfo();
            //validate user credentials
            if (UserManager.VerifyChildUserAccount(KeyFromChild, new LRI(ChildUserLRI)))
            {
                UserInfo uinfo = UserManager.Identities[ChildUserLRI];
                //validate parent ID is logged in
                var myBinding = new BasicHttpBinding();
                var myEndpoint = new EndpointAddress(new LRI(uinfo.Identity.ParentDomainLRI).URI);
                var myChannelFactory = new ChannelFactory<ILIdentityProvider>(myBinding, myEndpoint);
                ILIdentityProvider client = null;

                bool parentLoggedIn = false;
                try
                {
                    client = myChannelFactory.CreateChannel();
                    parentLoggedIn = client.ValidateParentID(uinfo.Identity.UserLRI, uinfo.Identity.KeyForParent);
                    ((ICommunicationObject)client).Close();

                }
                catch
                {
                    if (client != null)
                    {
                        ((ICommunicationObject)client).Abort();
                    }
                }
                if (parentLoggedIn)
                {
                    //login user
                    //generate session key
                    info = SessionManager.NewSession(uinfo.Identity);
                    info.Identity.KeyForParent = "";
                    //login all known children for this account
                    if (uinfo.Children.Count > 0)
                    {
                        foreach (KeyValuePair<string, ChildIdentity> child in uinfo.Children)
                        {
                            var childBinding = new BasicHttpBinding();
                            var childEndpoint = new EndpointAddress(new LRI(child.Value.ChildLRI).URI);
                            var childChannelFactory = new ChannelFactory<ILIdentityProvider>(childBinding, childEndpoint);
                            try
                            {
                                client = childChannelFactory.CreateChannel();
                                client.LoginChild(uinfo.Identity.ParentUserID, child.Value.ChildLRI, child.Value.ChildGeneratedKey, true);//need a version of this that allows the childpin to be used!
                                ((ICommunicationObject)client).Close();
                            }
                            catch
                            {
                                if (client != null)
                                {
                                    ((ICommunicationObject)client).Abort();
                                }
                            }
                        }
                    }
                }
            }
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

