using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using LCHARMS;
using LCHARMS.Session;

namespace LCHARMS.LIdentityProvider
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface ILIdentityProvider
    {

        //returns parent generated key
        [OperationContract]
        string RequestParentIDAuth(string ChildUserLRI, string Username, string ParentPINHash, string KeyFromChild, string SessionKey);

        //[OperationContract]
        //string RequestUserID(int ParentPinHash, string username);

        //[OperationContract]
        //string RetrieveUserID(LRI FromLRI, string Username, int FromParentPinHash);
        //simple for now, no async
        //[OperationContract]
        //void ReceiveParentIDAuth(string ChildUserLRI, bool Success, string KeyFromParent);
        [OperationContract]
        SessionInfo CreateIdentity(string ParentLRI, string ParentUser, string ParentPINHash, string username, string passwordhash, string ChildPinHash, string SessionKey);


        //returns session info
        [OperationContract]
        SessionInfo LoginID(string UserLRI, string passwordhash, string SessionKey="",bool LoginChildren = false);

        [OperationContract]
        bool ValidateParentSession(string ParentLRI, string SessionKey);
        [OperationContract]
        bool LoginChild(string ParentLRI, string ChildUserLRI, string KeyFromChild, string ParentSessionKey, bool LoginChildren = true);

        [OperationContract]
        void Logout(string LRI, string SessionKey);

        [OperationContract]
        int LCHARMSIDProviderVersion();
        [OperationContract]
        bool Ping();

        [OperationContract]
        LRI ParseLRI(string lri, bool IsURI = false);
    
    }

}
