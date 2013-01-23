using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using LIdentityProvider.Session;
using LCHARMS;

namespace LIdentityProvider
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
        SessionInfo CreateIdentity(string ParentDomain, string ParentUser, string ParentPINHash, string username, string passwordhash, string ChildPinHash, string SessionKey);


        //returns session info
        [OperationContract]
        SessionInfo LoginID(string UserLRI, string passwordhash, bool LoginChildren = false);

        [OperationContract]
        bool ValidateParentID(string ChildIDLRI, string KeyFromChild);
        [OperationContract]
        bool LoginChild(string ParentLRI, string ChildUserLRI, string KeyFromChild, bool LoginChildren = true);

        [OperationContract]
        int LCHARMSIDProviderVersion();
        [OperationContract]
        bool Ping();

        [OperationContract]
        LRI ParseLRI(string lri, bool IsURI = false);
    
    }

}
