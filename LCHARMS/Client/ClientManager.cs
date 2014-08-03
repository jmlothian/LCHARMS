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
using LCHARMS.Config;

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
        //LServiceHost<LDataProvider, ILDataProvider> DataProvider;

        public ClientManager()
        {
            //wire them up in sequence
            IDMgr = new IDManager(ConnMgr);
            ClientAcctManager = new ClientAccountManager(IDMgr, DocManager);
            WorkspaceMgr = new WorkspaceManager(ClientAcctManager);
        }
        public bool CheckSession(ServiceCredentials Creds)
        {
            return (ClientAcctManager.ValidSession(Creds.ClientSessionKey, Creds.ClientAccountLRI) == true ? true : false);
        }

        public void AddIdentityToAccount(string ID, UserInfo IdentityToAdd, LRI UserLRI = null)
        {
            ClientAcctManager.AddIdentityToAccount(ID, IdentityToAdd, UserLRI);
        }
        public ServiceResponse<ServiceCredentials> LoginID(LRI userLRI, string passwordHash, bool LoginAll = true)
        {
            return ClientAcctManager.LoginID(userLRI, passwordHash, LoginAll);
        }
        public ServiceResponse<ServiceCredentials> RegisterNewAccount(string ServiceLRI, string DomainLRI, string Username, string passwordHash)
        {
            return ClientAcctManager.RegisterNewAccount(ServiceLRI, DomainLRI, Username, passwordHash);
        }


        public ServiceResponse<LDocumentHeader> GetDocHeader(ServiceCredentials Credentials, LRI lri)
        {
            if (CheckSession(Credentials))
            {
                return ConnMgr.GetProvider<ILDataProviderChannel>(lri).GetDocHeader(Credentials, lri);
            }
            else
            {
                return new ServiceResponse<LDocumentHeader>(true);
            }
        }

        public ServiceResponse<LDocumentVersionInfo> GetDocVersionInfo(ServiceCredentials Credentials, LRI lri)
        {
            if (CheckSession(Credentials))
            {
                return ConnMgr.GetProvider<ILDataProviderChannel>(lri).GetDocVersionInfo(Credentials, lri);
            }
            else
            {
                return new ServiceResponse<LDocumentVersionInfo>(true);
            }
        }

        public ServiceResponse<DocumentPartResponse> GetDocPart(ServiceCredentials Credentials, LRI lri, int Version, int SequenceNumber)
        {
            
            if (CheckSession(Credentials))
            {
                return ConnMgr.GetProvider<ILDataProviderChannel>(lri).GetDocPart(Credentials, lri, Version, SequenceNumber);
            }
            else
            {
                return new ServiceResponse<DocumentPartResponse>(true);
            }
        }

        public ServiceResponse<DocumentPartResponse> GetDocParts(ServiceCredentials Credentials, LRI lri, int Version)
        {
            if (CheckSession(Credentials))
            {
                return ConnMgr.GetProvider<ILDataProviderChannel>(lri).GetDocParts(Credentials, lri, Version);
            }
            else
            {
                return new ServiceResponse<DocumentPartResponse>(true);
            }
        }

        public ServiceResponse<List<LDocumentVersionInfo>> GetFileVersionHistory(ServiceCredentials Credentials, LRI lri)
        {
            if (CheckSession(Credentials))
            {
                return ConnMgr.GetProvider<ILDataProviderChannel>(lri).GetFileVersionHistory(Credentials, lri);
            }
            else
            {
                return new ServiceResponse<List<LDocumentVersionInfo>>(true);
            }
        }

        public ServiceResponse<List<LCollection>> GetCollections(ServiceCredentials Credentials, LRI lri)
        {
            if (CheckSession(Credentials))
            {
                return ConnMgr.GetProvider<ILDataProviderChannel>(lri).GetCollections(Credentials, lri);
            }
            else
            {
                return new ServiceResponse<List<LCollection>>(true);
            }
        }

        public ServiceResponse<LCollection> GetCurrentCollection(ServiceCredentials Credentials, LRI lri, LRI CollectionLRI)
        {
            if (CheckSession(Credentials))
            {
                return ConnMgr.GetProvider<ILDataProviderChannel>(lri).GetCurrentCollection(Credentials, lri, CollectionLRI);
            }
            else
            {
                return new ServiceResponse<LCollection>(true);
            }
        }

        public ServiceResponse<LHierarchy> GetCurrentCollectionHierarchy(ServiceCredentials Credentials, LRI lri, LRI CollectionLRI)
        {
            if (CheckSession(Credentials))
            {
                return ConnMgr.GetProvider<ILDataProviderChannel>(lri).GetCurrentCollectionHierarchy(Credentials, lri, CollectionLRI);
            }
            else
            {
                return new ServiceResponse<LHierarchy>(true);
            }
        }

        public ServiceResponse<bool> AddTag(ServiceCredentials Credentials, LRI lri, string tag)
        {
            if (CheckSession(Credentials))
            {
                return ConnMgr.GetProvider<ILDataProviderChannel>(lri).AddTag(Credentials, lri, tag);
            }
            else
            {
                return new ServiceResponse<bool>(true);
            }
        }

        public ServiceResponse<bool> RemoveTag(ServiceCredentials Credentials, LRI lri, string tag)
        {
            if (CheckSession(Credentials))
            {
                return ConnMgr.GetProvider<ILDataProviderChannel>(lri).RemoveTag(Credentials, lri, tag);
            }
            else
            {
                return new ServiceResponse<bool>(true);
            }
        }

        public ServiceResponse<List<string>> GetTags(ServiceCredentials Credentials, LRI lri)
        {
            if (CheckSession(Credentials))
            {
                return ConnMgr.GetProvider<ILDataProviderChannel>(lri).GetTags(Credentials, lri);
            }
            else
            {
                return new ServiceResponse<List<string>>(true);
            }
        }

        public ServiceResponse<LDocumentHeader> SaveNewVersion(ServiceCredentials Credentials, LRI lri)
        {
            if (CheckSession(Credentials))
            {
                return ConnMgr.GetProvider<ILDataProviderChannel>(lri).SaveNewVersion(Credentials, lri);
            }
            else
            {
                return new ServiceResponse<LDocumentHeader>(true);
            }
        }

        public ServiceResponse<bool> UpdateDoc(ServiceCredentials Credentials, LRI lri, List<LDocumentPart> parts)
        {
            if (CheckSession(Credentials))
            {
                return ConnMgr.GetProvider<ILDataProviderChannel>(lri).UpdateDoc(Credentials, lri, parts);
            }
            else
            {
                return new ServiceResponse<bool>(true);
            }
        }

        public ServiceResponse<bool> SavePart(ServiceCredentials Credentials, LRI lri, LDocumentPart part, int SequenceNumber)
        {
            if (CheckSession(Credentials))
            {
                return ConnMgr.GetProvider<ILDataProviderChannel>(lri).SavePart(Credentials, lri, part, SequenceNumber);
            }
            else
            {
                return new ServiceResponse<bool>(true);
            }
        }

        public ServiceResponse<bool> DeleteFileLC(ServiceCredentials Credentials, LRI lri)
        {
            if (CheckSession(Credentials))
            {
                return ConnMgr.GetProvider<ILDataProviderChannel>(lri).DeleteFileLC(Credentials, lri);
            }
            else
            {
                return new ServiceResponse<bool>(true);
            }
        }

        public ServiceResponse<LDocumentHeader> NewFile(ServiceCredentials Credentials, string FQDT, string filename, List<string> Tags, string ParentLRI)
        {
            if (CheckSession(Credentials))
            {
                return ConnMgr.GetProvider<ILDataProviderChannel>(new LRI(LCHARMSConfig.GetSection().LRI)).NewFile(Credentials, FQDT, filename, Tags, ParentLRI);
            }
            else
            {
                return new ServiceResponse<LDocumentHeader>(true);
            }
        }

        public ServiceResponse<bool> AppendChild(ServiceCredentials Credentials, LRI hierarchyLRI, LRI parentLRI, LRI childLRI)
        {
            if (CheckSession(Credentials))
            {
                return ConnMgr.GetProvider<ILDataProviderChannel>(hierarchyLRI).AppendChild(Credentials, hierarchyLRI, parentLRI, childLRI);
            }
            else
            {
                return new ServiceResponse<bool>(true);
            }
        }

        public ServiceResponse<bool> PrependChild(ServiceCredentials Credentials, LRI hierarchyLRI, LRI parentLRI, LRI childLRI)
        {
            if (CheckSession(Credentials))
            {
                return ConnMgr.GetProvider<ILDataProviderChannel>(hierarchyLRI).PrependChild(Credentials, hierarchyLRI, parentLRI, childLRI);
            }
            else
            {
                return new ServiceResponse<bool>(true);
            }
        }

        public ServiceResponse<bool> InsertChild(ServiceCredentials Credentials, LRI hierarchyLRI, LRI parentLRI, LRI childLRI, int index)
        {
            if (CheckSession(Credentials))
            {
                return ConnMgr.GetProvider<ILDataProviderChannel>(hierarchyLRI).InsertChild(Credentials, hierarchyLRI, parentLRI, childLRI, index);
            }
            else
            {
                return new ServiceResponse<bool>(true);
            }
        }

        public ServiceResponse<bool> RemoveChild(ServiceCredentials Credentials, LRI hierarchyLRI, LRI parentLRI, LRI childLRI)
        {
            if (CheckSession(Credentials))
            {
                return ConnMgr.GetProvider<ILDataProviderChannel>(hierarchyLRI).RemoveChild(Credentials, hierarchyLRI, parentLRI, childLRI);
            }
            else
            {
                return new ServiceResponse<bool>(true);
            }
        }

        public ServiceResponse<LHierarchyNode> GetNextSibling(ServiceCredentials Credentials, LRI hierarchyLRI, LRI childLRI)
        {
            if (CheckSession(Credentials))
            {
                return ConnMgr.GetProvider<ILDataProviderChannel>(hierarchyLRI).GetNextSibling(Credentials, hierarchyLRI, childLRI);
            }
            else
            {
                return new ServiceResponse<LHierarchyNode>(true);
            }
        }

        public ServiceResponse<LHierarchyNode> GetPreviousSibling(ServiceCredentials Credentials, LRI hierarchyLRI, LRI childLRI)
        {
            if (CheckSession(Credentials))
            {
                return ConnMgr.GetProvider<ILDataProviderChannel>(hierarchyLRI).GetPreviousSibling(Credentials, hierarchyLRI, childLRI);
            }
            else
            {
                return new ServiceResponse<LHierarchyNode>(true);
            }
        }

        public ServiceResponse<List<LHierarchyNode>> GetChildren(ServiceCredentials Credentials, LRI hierarchyLRI, LRI childLRI)
        {
            if (CheckSession(Credentials))
            {
                return ConnMgr.GetProvider<ILDataProviderChannel>(hierarchyLRI).GetChildren(Credentials, hierarchyLRI, childLRI);
            }
            else
            {
                return new ServiceResponse<List<LHierarchyNode>>(true);
            }
        }

        public ServiceResponse<LHierarchyNode> GetParent(ServiceCredentials Credentials, LRI hierarchyLRI, LRI childLRI)
        {
            if (CheckSession(Credentials))
            {
                return ConnMgr.GetProvider<ILDataProviderChannel>(hierarchyLRI).GetParent(Credentials, hierarchyLRI, childLRI);
            }
            else
            {
                return new ServiceResponse<LHierarchyNode>(true);
            }
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
    }
}
