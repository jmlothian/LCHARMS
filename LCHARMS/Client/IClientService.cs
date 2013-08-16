using System;
using System.ServiceModel;
using LCHARMS.Document;
namespace LCHARMS.Client
{
    [ServiceContract]
    interface IClientService : ILClientProvider
    {
        [OperationContract]
        void AddIdentityToAccount(string ID, LCHARMS.Authentication.UserInfo IdentityToAdd, LCHARMS.LRI UserLRI = null);
        [OperationContract]
        ServiceResponse<string> LoginID(LCHARMS.LRI userLRI, string passwordHash, bool LoginAll = true);
        [OperationContract]
        ServiceResponse<string> RegisterNewAccount(string ServiceLRI, string DomainLRI, string Username, string passwordHash);
    }
}
