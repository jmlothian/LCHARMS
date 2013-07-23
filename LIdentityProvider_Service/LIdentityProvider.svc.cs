using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using LCHARMS;
using LCHARMS.Identity;
using LCHARMS.Session;
using LCHARMS.Authentication;
using LCHARMS.LIdentityProvider;
using LCHARMS.Logging;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace LIdentityProvider_Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    public class LIdentityProvider : ILIdentityProvider, IDisposable
    {
        SessionManager SessManager = null;
        public LIdentityProvider()
        {
            SessManager = new SessionManager();
            //if (File.Exists("c:\\logs\\bin\\SessionManager.bin"))
            //{
            //    IFormatter formatter = new BinaryFormatter();
            //    Stream stream = new FileStream("c:\\logs\\bin\\SessionManager.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
            //    SessManager = (SessionManager)formatter.Deserialize(stream);
            //    stream.Close();
            //}
            FDebugLog.WriteLog("Starting Log - LIDProvider");
            UserManager.LoadIdentities();
            //load session manager
        }
        public void Dispose()
        {
            //IFormatter formatter = new BinaryFormatter();
            //Stream stream = new FileStream("c:\\logs\\bin\\SessionManager.bin", FileMode.Create, FileAccess.Write, FileShare.None);
            //formatter.Serialize(stream, SessManager);
            //stream.Close();
            FDebugLog.WriteLog("Ending Log - LIDProvider");
        }
        #region CALLED BY CHILD

        public string RequestParentIDAuth(string ChildUserLRI, string Username, string ParentPINHash, string KeyFromChild, string SessionKey)
        {
            string userid = "";
            FDebugLog.WriteLog("Requesting Parent Auth:" + ChildUserLRI + " : " + Username);
            if (UserManager.VerifyUserAccount(ParentPINHash, Username))
            {
                //account is real
                FDebugLog.WriteLog("Account Verified");
                userid = UserManager.SecurityPINHashes[Username].Identity.UserID;
                UserManager.AddChildToParent(UserManager.SecurityPINHashes[Username], KeyFromChild, ChildUserLRI);
            }
            else
            {

            }
            return userid;
        }
        public bool ValidateParentSession(string ParentLRI, string SessionKey)
        {
            FDebugLog.WriteLog("Requesting Parent Validation:" + ParentLRI + " : " + SessionKey);
            return SessManager.VerifySessionKey(SessionKey, new LRI(ParentLRI));
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
            var myEndpoint = new EndpointAddress("http://" + FromLRI.BaseURI);
            var myChannelFactory = new ChannelFactory<ILIdentityProvider>(myBinding, myEndpoint);
            FDebugLog.WriteLog("Requesting Parent Auth From:" + FromLRI.LRIString);
            ILIdentityProvider client = null;

            try
            {
                client = myChannelFactory.CreateChannel();
                FDebugLog.WriteLog(ChildLRI + " " + Username + " " + FromParentPinHash + " " + KeyFromChild + " " + SessionKey);
                userid = client.RequestParentIDAuth(ChildLRI, Username, FromParentPinHash, KeyFromChild, SessionKey);
                ((ICommunicationObject)client).Close();
            }
            catch (Exception ex)
            {
                FDebugLog.WriteLog("Error: " + ex.Message + " " + ex.StackTrace);
                if (client != null)
                {
                    ((ICommunicationObject)client).Abort();
                }
            }

            return userid;
        }

        public SessionInfo CreateIdentity(string ParentLRI, string ParentUser, string ParentPINHash, string username, string passwordhash, string ChildPinHash, string SessionKey)
        {
            SessionInfo sessinfo = new SessionInfo();
            //create temporary user w/ key (reserve userid)
            string ReservationKey = Guid.NewGuid().ToString();
            IDRequestInfo info = UserManager.ReserveGUID(ReservationKey);

            string UserLRI = UserManager.DomainLRI + "/~users/" + info.GUID;
            string UserID = info.GUID;

            //generate child key
            string ChildKey = Guid.NewGuid().ToString();
            FDebugLog.WriteLog("CreateIdentity Requested: ParentDomain-" + ParentLRI + " ParentUser-" + ParentUser + " username-" + username );
            if (ParentLRI != null && ParentLRI != "")
            {
                //construct parentLRI
                FDebugLog.WriteLog("Create ID From Parent: " + ParentLRI + "("+ParentUser+")");
                LRI ParentLRIParsed = new LRI(ParentLRI);
                //get parent userid from parent domain
                string parentUserID = RetrieveUserParentAuth(ParentLRIParsed, ParentUser, ParentPINHash, ChildKey, SessionKey, UserLRI);
                if (parentUserID != "")
                {
                    FDebugLog.WriteLog("Parent Located");
                    //CreateChildIdentity
                    bool addSucceed = UserManager.AddChildIdentity(ParentLRIParsed.LRIString, username, UserLRI, passwordhash, ChildPinHash, ChildKey, info);
                    //login user
                    sessinfo = LoginID(UserLRI, passwordhash, SessionKey);
                }
                else
                {
                    FDebugLog.WriteLog("Parent not found");
                    sessinfo.Error = true;
                    sessinfo.ErrorType = SESSION_ERROR.INVALID_PARENT_CREDENTIALS;
                }
            }
            else
            {
                FDebugLog.WriteLog("No Parent: Creating CORE User.");
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
        public SessionInfo LoginID(string UserLRI, string passwordhash, string ParentSessionKey="", bool LoginChildren = false)
        {
            SessionInfo info = new SessionInfo();
            bool parentLoggedIn = false;
            ILIdentityProvider client = null;
            //validate user credentials
            FDebugLog.WriteLog("Login Request: " + UserLRI);
            if (UserManager.VerifyLocalUserAccount(passwordhash, new LRI(UserLRI)))
            {
                UserInfo uinfo = UserManager.Identities[UserLRI];
                if (uinfo.Identity.ParentBaseLRI == "~LCHARMS-CORE~")
                {
                    FDebugLog.WriteLog("Parent Login Skipped - CORE account");
                    parentLoggedIn = true;
                }
                else
                {
                    //uinfo.Identity.ParentDomainLRI
                    //validate parent ID is logged in
                    FDebugLog.WriteLog("Checking Parent Login: " + uinfo.Identity.ParentBaseLRI);
                    FDebugLog.WriteLog("Checking URI: " + new LRI(uinfo.Identity.ParentBaseLRI).BaseURI);
                    var myBinding = new BasicHttpBinding();
                    //var myIdent = new DnsEndpointIdentity(new LRI(uinfo.Identity.ParentBaseLRI).URIDomain);
                    var myEndpoint = new EndpointAddress(new Uri("http://" + new LRI(uinfo.Identity.ParentBaseLRI).BaseURI));//, 
                        //EndpointIdentity.CreateDnsIdentity(new LRI(uinfo.Identity.ParentBaseLRI).URIDomain));
                    var myChannelFactory = new ChannelFactory<ILIdentityProvider>(myBinding, myEndpoint);



                    try
                    {
                        FDebugLog.WriteLog("CHECKING..." + uinfo.Identity.ParentBaseLRI + "/~users/" + uinfo.Identity.ParentUserID);
                        client = myChannelFactory.CreateChannel();
                        parentLoggedIn = client.ValidateParentSession(uinfo.Identity.ParentBaseLRI + "/~users/" + uinfo.Identity.ParentUserID, ParentSessionKey);
                        ((ICommunicationObject)client).Close();
                    }
                    catch (Exception ex)
                    {
                        FDebugLog.WriteLog("Parent Login Check Failed" + ex.ToString());
                        if (client != null)
                        {
                            FDebugLog.WriteLog("Checked URI:" + myEndpoint.Uri);
                            ((ICommunicationObject)client).Abort();
                        }
                    }
                }
                if (parentLoggedIn)
                {
                    FDebugLog.WriteLog("Parent Logged In - Authenticating user...");
                    //login user
                    //generate session key
                    info = SessManager.NewSession(uinfo.Identity);
                    info.Identity.KeyForParent = "";

                    //login children
                    if (uinfo.Children.Count > 0)
                    {
                        FDebugLog.WriteLog("Logging In Children...");
                        foreach (KeyValuePair<string, ChildIdentity> child in uinfo.Children)
                        {
                            var childBinding = new BasicHttpBinding();
                            var childEndpoint = new EndpointAddress("http://" + new LRI(child.Value.ChildLRI).BaseURI);
                            var childChannelFactory = new ChannelFactory<ILIdentityProvider>(childBinding, childEndpoint);
                            try
                            {
                                client = childChannelFactory.CreateChannel();
                                client.LoginChild(uinfo.Identity.ParentUserID, child.Value.ChildLRI, info.Identity.KeyForParent, info.SessionKey, true);//need a version of this that allows the childpin to be used!
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
                else
                {
                    FDebugLog.WriteLog("Parent Not Logged In");
                    info.Error = true;
                    info.ErrorType = SESSION_ERROR.PARENT_NOT_LOGGED_IN;
                }
            }
            else
            {
                FDebugLog.WriteLog("Invalid Credentials - Rejecting Login");
                info.Error = true;
                info.ErrorType = SESSION_ERROR.INVALID_CREDENTIALS;
            }
            return info;
        }



        public bool LoginChild(string ParentLRI, string ChildUserLRI, string KeyFromChild, string ParentSessionKey, bool LoginChildren = true)
        {
            //login this account
            FDebugLog.WriteLog("Child Login Request: \n   parent-" + ParentLRI + " \n   child-" + ChildUserLRI + "(" + KeyFromChild + ")");
            SessionInfo info = new SessionInfo();
            //validate user credentials
            if (UserManager.VerifyChildUserAccount(KeyFromChild, new LRI(ChildUserLRI)))
            {
                FDebugLog.WriteLog("Checking Parent Login(child): " + ParentLRI);
                UserInfo uinfo = UserManager.Identities[ChildUserLRI];
                //validate parent ID is logged in
                var myBinding = new BasicHttpBinding();
                var myEndpoint = new EndpointAddress("http://" + new LRI(uinfo.Identity.ParentBaseLRI).BaseURI);
                var myChannelFactory = new ChannelFactory<ILIdentityProvider>(myBinding, myEndpoint);
                ILIdentityProvider client = null;

                bool parentLoggedIn = false;
                try
                {
                    client = myChannelFactory.CreateChannel();
                    parentLoggedIn = client.ValidateParentSession(uinfo.Identity.ParentUserID, ParentSessionKey);
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
                    FDebugLog.WriteLog("Parent Logged In(child) - Authenticating user...");
                    //login user
                    //generate session key
                    info = SessManager.NewSession(uinfo.Identity);
                    info.Identity.KeyForParent = "";
                    //login all known children for this account
                    if (uinfo.Children.Count > 0)
                    {
                        foreach (KeyValuePair<string, ChildIdentity> child in uinfo.Children)
                        {
                            var childBinding = new BasicHttpBinding();
                            var childEndpoint = new EndpointAddress("http://" + new LRI(child.Value.ChildLRI).BaseURI);
                            var childChannelFactory = new ChannelFactory<ILIdentityProvider>(childBinding, childEndpoint);
                            try
                            {
                                client = childChannelFactory.CreateChannel();
                                client.LoginChild(uinfo.Identity.ParentUserID, child.Value.ChildLRI, child.Value.ChildGeneratedKey, info.SessionKey, true);//need a version of this that allows the childpin to be used!
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
        public void Logout(string LRI, string SessionKey)
        {
            if (SessManager.VerifySessionKey(SessionKey, new LRI(LRI)))
            {
                SessManager.Logout(SessionKey);
            }
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

