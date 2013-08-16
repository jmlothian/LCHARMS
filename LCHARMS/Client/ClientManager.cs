using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LCHARMS.UI.Workspace;
using LCHARMS.Document;
using LCHARMS.Authentication;
using System.ServiceModel;
using LCHARMS.Identity;
using LCHARMS.Hierarchy;
using LCHARMS.Session;
using LCHARMS.Collection;

namespace LCHARMS.Client
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ClientManager : IClientService
    {
        public LConnectionManager ConnMgr = new LConnectionManager();
        public LDocumentManager DocManager = new LDocumentManager();
        public ClientAccountManager ClientAcctManager;
        public IDManager IDMgr;
        public WorkspaceManager WorkspaceMgr;
        public ClientManager()
        {
            //wire them up in sequence
            IDMgr = new IDManager(ConnMgr);
            ClientAcctManager = new ClientAccountManager(IDMgr, DocManager);
            WorkspaceMgr = new WorkspaceManager(ClientAcctManager);
        }
        public void AddIdentityToAccount(string ID, UserInfo IdentityToAdd, LRI UserLRI = null)
        {
            ClientAcctManager.AddIdentityToAccount(ID, IdentityToAdd, UserLRI);
        }
        public ServiceResponse<string> LoginID(LRI userLRI, string passwordHash, bool LoginAll = true)
        {
            return ClientAcctManager.LoginID(userLRI, passwordHash, LoginAll);
        }
        public ServiceResponse<string> RegisterNewAccount(string ServiceLRI, string DomainLRI, string Username, string passwordHash)
        {
            return ClientAcctManager.RegisterNewAccount(ServiceLRI, DomainLRI, Username, passwordHash);
        }


        public string IdentityProvider
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int IDProviderCacheResponseTime
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }



        public ServiceResponse<LDocumentHeader> GetDocHeader(ServiceCredentials Credentials, LRI lri)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<LDocumentVersionInfo> GetDocVersionInfo(ServiceCredentials Credentials, LRI lri)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<DocumentPartResponse> GetDocPart(ServiceCredentials Credentials, LRI lri, int Version, int SequenceNumber)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<DocumentPartResponse> GetDocParts(ServiceCredentials Credentials, LRI lri, int Version)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<List<LDocumentVersionInfo>> GetFileVersionHistory(ServiceCredentials Credentials, LRI lri)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<List<LCollection>> GetCollections(ServiceCredentials Credentials, LRI lri)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<LCollection> GetCurrentCollection(ServiceCredentials Credentials, LRI lri, LRI CollectionLRI)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<LHierarchy> GetCurrentCollectionHierarchy(ServiceCredentials Credentials, LRI lri, LRI CollectionLRI)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<bool> AddTag(ServiceCredentials Credentials, LRI lri, string tag)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<bool> RemoveTag(ServiceCredentials Credentials, LRI lri, string tag)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<List<string>> GetTags(ServiceCredentials Credentials, LRI lri)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<LDocumentHeader> SaveNewVersion(ServiceCredentials Credentials, LRI lri)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<bool> UpdateDoc(ServiceCredentials Credentials, LRI lri, List<LDocumentPart> parts)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<bool> SavePart(ServiceCredentials Credentials, LRI lri, LDocumentPart part, int SequenceNumber)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<bool> DeleteFileLC(ServiceCredentials Credentials, LRI lri)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<LDocumentHeader> NewFile(ServiceCredentials Credentials, string FQDT, string filename, List<string> Tags, string ParentLRI)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<bool> AppendChild(ServiceCredentials Credentials, LRI hierarchyLRI, LRI parentLRI, LRI childLRI)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<bool> PrependChild(ServiceCredentials Credentials, LRI hierarchyLRI, LRI parentLRI, LRI childLRI)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<bool> InsertChild(ServiceCredentials Credentials, LRI hierarchyLRI, LRI parentLRI, LRI childLRI, int index)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<bool> RemoveChild(ServiceCredentials Credentials, LRI hierarchyLRI, LRI parentLRI, LRI childLRI)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<LHierarchyNode> GetNextSibling(ServiceCredentials Credentials, LRI hierarchyLRI, LRI childLRI)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<LHierarchyNode> GetPreviousSibling(ServiceCredentials Credentials, LRI hierarchyLRI, LRI childLRI)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<List<LHierarchyNode>> GetChildren(ServiceCredentials Credentials, LRI hierarchyLRI, LRI childLRI)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<LHierarchyNode> GetParent(ServiceCredentials Credentials, LRI hierarchyLRI, LRI childLRI)
        {
            throw new NotImplementedException();
        }

        public string RequestParentIDAuth(string ChildUserLRI, string Username, string ParentPINHash, string KeyFromChild, string SessionKey)
        {
            throw new NotImplementedException();
        }

        public SessionInfo CreateIdentity(string ParentLRI, string ParentUser, string ParentPINHash, string username, string passwordhash, string ChildPinHash, string SessionKey)
        {
            throw new NotImplementedException();
        }

        public SessionInfo LoginID(string UserLRI, string passwordhash, string SessionKey = "", bool LoginChildren = false)
        {
            throw new NotImplementedException();
        }

        public bool ValidateParentSession(string ParentLRI, string SessionKey)
        {
            throw new NotImplementedException();
        }

        public bool LoginChild(string ParentLRI, string ChildUserLRI, string KeyFromChild, string ParentSessionKey, bool LoginChildren = true)
        {
            throw new NotImplementedException();
        }

        public void Logout(string LRI, string SessionKey)
        {
            throw new NotImplementedException();
        }

        public int LCHARMSIDProviderVersion()
        {
            throw new NotImplementedException();
        }

        public bool Ping()
        {
            throw new NotImplementedException();
        }

        public LRI ParseLRI(string lri, bool IsURI = false)
        {
            throw new NotImplementedException();
        }

        public LRI GetUserLRI(string ServiceLRI, string Username, string passwordHash)
        {
            throw new NotImplementedException();
        }

        public LIdentity GetUserIdentity(string ServiceLRI, string Username, string passwordHash)
        {
            throw new NotImplementedException();
        }
    }
}
