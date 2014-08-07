using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using LCHARMS.Document;
using LCHARMS.LIdentityProvider;
using LCHARMS;
using LCHARMS.Identity;
using LCHARMS.Session;
using LCHARMS.Collection;
using LCHARMS.Hierarchy;
using LCHARMS.Client;
using LCHARMS.Authentication;

namespace LClientProvider
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    public class LClientProvider : IClientService
    {
        IClientServiceChannel Client;
		string ClientServiceAddress = "127.0.0.1:8002/LDocClientHost/"; //"127.0.0.1:8002/LDocClientHost/"; //"192.168.1.11:5984"

        LConnectionManager ConnManager = new LConnectionManager();
        public LClientProvider()
        {
            Client = ConnManager.GetProvider<IClientServiceChannel>(new LRI(ClientServiceAddress, true));
        }

        public void AddIdentityToAccount(string ID, UserInfo IdentityToAdd, LRI UserLRI = null)
        {
            Client.AddIdentityToAccount(ID, IdentityToAdd, UserLRI);
        }

        public ServiceResponse<ServiceCredentials> LoginID(LRI userLRI, string passwordHash, bool LoginAll = true)
        {
            return Client.LoginID(userLRI, passwordHash, LoginAll);
        }

        public ServiceResponse<ServiceCredentials> RegisterNewAccount(string ServiceLRI, string DomainLRI, string Username, string passwordHash)
        {
            return Client.RegisterNewAccount(ServiceLRI, DomainLRI, Username, passwordHash);
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
            return Client.GetDocHeader(Credentials, lri);
        }

        public ServiceResponse<LDocumentVersionInfo> GetDocVersionInfo(ServiceCredentials Credentials, LRI lri)
        {
            return Client.GetDocVersionInfo(Credentials, lri);
        }

        public ServiceResponse<DocumentPartResponse> GetDocPart(ServiceCredentials Credentials, LRI lri, int Version, int SequenceNumber)
        {
            return Client.GetDocPart(Credentials, lri, Version, SequenceNumber);
        }

        public ServiceResponse<DocumentPartResponse> GetDocParts(ServiceCredentials Credentials, LRI lri, int Version)
        {
            return Client.GetDocParts(Credentials, lri, Version);
        }

        public ServiceResponse<List<LDocumentVersionInfo>> GetFileVersionHistory(ServiceCredentials Credentials, LRI lri)
        {
            return GetFileVersionHistory(Credentials, lri);
        }

        public ServiceResponse<List<LCollection>> GetCollections(ServiceCredentials Credentials, LRI lri)
        {
            return Client.GetCollections(Credentials, lri);
        }

        public ServiceResponse<LCollection> GetCurrentCollection(ServiceCredentials Credentials, LRI lri, LRI CollectionLRI)
        {
            return Client.GetCurrentCollection(Credentials, lri, CollectionLRI);
        }

        public ServiceResponse<LHierarchy> GetCurrentCollectionHierarchy(ServiceCredentials Credentials, LRI lri, LRI CollectionLRI)
        {
            return GetCurrentCollectionHierarchy(Credentials, lri, CollectionLRI);
        }

        public ServiceResponse<bool> AddTag(ServiceCredentials Credentials, LRI lri, string tag)
        {
            return Client.AddTag(Credentials, lri, tag);
        }

        public ServiceResponse<bool> RemoveTag(ServiceCredentials Credentials, LRI lri, string tag)
        {
            return Client.RemoveTag(Credentials, lri, tag);
        }

        public ServiceResponse<List<string>> GetTags(ServiceCredentials Credentials, LRI lri)
        {
            return Client.GetTags(Credentials, lri);
        }

        public ServiceResponse<LDocumentHeader> SaveNewVersion(ServiceCredentials Credentials, LRI lri)
        {
            return Client.SaveNewVersion(Credentials, lri);
        }

        public ServiceResponse<bool> UpdateDoc(ServiceCredentials Credentials, LRI lri, List<LDocumentPart> parts)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<bool> SavePart(ServiceCredentials Credentials, LRI lri, LDocumentPart part, int SequenceNumber)
        {
            return Client.SavePart(Credentials, lri, part, SequenceNumber);
        }

        public ServiceResponse<bool> DeleteFileLC(ServiceCredentials Credentials, LRI lri)
        {
            return Client.DeleteFileLC(Credentials, lri);
        }

        public ServiceResponse<LDocumentHeader> NewFile(ServiceCredentials Credentials, string FQDT, string filename, List<string> Tags, string ParentLRI)
        {
            return Client.NewFile(Credentials, FQDT, filename, Tags, ParentLRI);
        }

        public ServiceResponse<bool> AppendChild(ServiceCredentials Credentials, LRI hierarchyLRI, LRI parentLRI, LRI childLRI)
        {
            return Client.AppendChild(Credentials, hierarchyLRI, parentLRI, childLRI);
        }

        public ServiceResponse<bool> PrependChild(ServiceCredentials Credentials, LRI hierarchyLRI, LRI parentLRI, LRI childLRI)
        {
            return Client.PrependChild(Credentials, hierarchyLRI, parentLRI, childLRI);
        }

        public ServiceResponse<bool> InsertChild(ServiceCredentials Credentials, LRI hierarchyLRI, LRI parentLRI, LRI childLRI, int index)
        {
            return Client.InsertChild(Credentials, hierarchyLRI, parentLRI, childLRI, index);
        }

        public ServiceResponse<bool> RemoveChild(ServiceCredentials Credentials, LRI hierarchyLRI, LRI parentLRI, LRI childLRI)
        {
            return Client.RemoveChild(Credentials, hierarchyLRI, parentLRI, childLRI);
        }

        public ServiceResponse<LHierarchyNode> GetNextSibling(ServiceCredentials Credentials, LRI hierarchyLRI, LRI childLRI)
        {
            return Client.GetNextSibling(Credentials, hierarchyLRI, childLRI);
        }

        public ServiceResponse<LHierarchyNode> GetPreviousSibling(ServiceCredentials Credentials, LRI hierarchyLRI, LRI childLRI)
        {
            return Client.GetPreviousSibling(Credentials, hierarchyLRI, childLRI);
        }

        public ServiceResponse<List<LHierarchyNode>> GetChildren(ServiceCredentials Credentials, LRI hierarchyLRI, LRI childLRI)
        {
            return Client.GetChildren(Credentials, hierarchyLRI, childLRI);
        }

		
        public ServiceResponse<LHierarchyNode> GetParent(ServiceCredentials Credentials, LRI hierarchyLRI, LRI childLRI)
        {
            return Client.GetParent(Credentials, hierarchyLRI, childLRI);
        }
    }
}
