using System;
using System.ServiceModel;
using LCHARMS.Document;
using LCHARMS.Identity;
namespace LCHARMS.Client
{
    [ServiceContract]
    public interface IClientService : ILClientProvider
    {
        [OperationContract]
        void AddIdentityToAccount(string ID, LCHARMS.Authentication.UserInfo IdentityToAdd, LCHARMS.LRI UserLRI = null);
        [OperationContract]
        ServiceResponse<ServiceCredentials> LoginID(LCHARMS.LRI userLRI, string passwordHash, bool LoginAll = true);
        [OperationContract]
        ServiceResponse<ServiceCredentials> RegisterNewAccount(string ServiceLRI, string DomainLRI, string Username, string passwordHash);
    }
    public interface IClientServiceChannel : IClientService, IClientChannel
    {
    }
}
