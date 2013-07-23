using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using LCHARMS.Hierarchy;
using LCHARMS.Collection;
using LCHARMS.Identity;

namespace LCHARMS.Document
{
    //this interface uses basically the same interface as ILDocumentManager - except, 
    //it checks sessions / permission and returns a Response object with appropriate codes
    // in the case where docmanager doesn't return something, ServiceResponse wraps a bool, set to false for "no", true for "yes".
    [ServiceContract]
    public interface ILDataProvider
    {
        //identity provider used for authentication validations
        // all calls to a dataprovider will check the user LRI and session key against the dataprovider's ID Provider
        string IdentityProvider { get; set; }
        // in the future, we can probably cache these requests for a little while
        // how long, in seconds, should 
        int IDProviderCacheResponseTime { get; set; }


        [OperationContract]
        ServiceResponse<LDocumentHeader> GetDocHeader(ServiceCredentials Credentials, LRI lri);
        [OperationContract]
        ServiceResponse<LDocumentVersionInfo> GetDocVersionInfo(ServiceCredentials Credentials, LRI lri);
        //GetFileMetaData		LRI, Version
        [OperationContract]
        ServiceResponse<DocumentPartResponse> GetDocPart(ServiceCredentials Credentials, LRI lri, int Version, int SequenceNumber);
        [OperationContract]
        ServiceResponse<DocumentPartResponse> GetDocParts(ServiceCredentials Credentials, LRI lri, int Version);
        //getthumb
        [OperationContract]
        ServiceResponse<List<LDocumentVersionInfo>> GetFileVersionHistory(ServiceCredentials Credentials, LRI lri);
        [OperationContract]
        ServiceResponse<List<LCollection>> GetCollections(ServiceCredentials Credentials, LRI lri);
        [OperationContract]
        ServiceResponse<LCollection> GetCurrentCollection(ServiceCredentials Credentials, LRI lri, LRI CollectionLRI);
        //not sure this makes sense.. how do we know which heirarchy?  Are all collections part of a hierarchy?
        [OperationContract]
        ServiceResponse<LHierarchy> GetCurrentCollectionHierarchy(ServiceCredentials Credentials, LRI lri, LRI CollectionLRI);

        //tags
        [OperationContract]
        ServiceResponse<bool> AddTag(ServiceCredentials Credentials, LRI lri, string tag);
        [OperationContract]
        ServiceResponse<bool> RemoveTag(ServiceCredentials Credentials, LRI lri, string tag);
        [OperationContract]
        ServiceResponse<List<string>> GetTags(ServiceCredentials Credentials, LRI lri);

        //versions
        //returns the new version lri
        [OperationContract]
        ServiceResponse<LDocumentHeader> SaveNewVersion(ServiceCredentials Credentials, LRI lri);
        //update cache, "save local"
        [OperationContract]
        ServiceResponse<bool> UpdateDoc(ServiceCredentials Credentials, LRI lri, List<LDocumentPart> parts);
        [OperationContract]
        ServiceResponse<bool> SavePart(ServiceCredentials Credentials, LRI lri, LDocumentPart part, int SequenceNumber);
        [OperationContract]
        ServiceResponse<bool> DeleteFileLC(ServiceCredentials Credentials, LRI lri);
        [OperationContract]
        ServiceResponse<LDocumentHeader> NewFile(ServiceCredentials Credentials, string FQDT, string filename, List<string> Tags, string ParentLRI);

        //hierarchy
        [OperationContract]
        ServiceResponse<bool> AppendChild(ServiceCredentials Credentials, LRI hierarchyLRI, LRI parentLRI, LRI childLRI);
        [OperationContract]
        ServiceResponse<bool> PrependChild(ServiceCredentials Credentials, LRI hierarchyLRI, LRI parentLRI, LRI childLRI);
        [OperationContract]
        ServiceResponse<bool> InsertChild(ServiceCredentials Credentials, LRI hierarchyLRI, LRI parentLRI, LRI childLRI, int index);
        [OperationContract]
        ServiceResponse<bool> RemoveChild(ServiceCredentials Credentials, LRI hierarchyLRI, LRI parentLRI, LRI childLRI);
        [OperationContract]
        ServiceResponse<LHierarchyNode> GetNextSibling(ServiceCredentials Credentials, LRI hierarchyLRI, LRI childLRI);
        [OperationContract]
        ServiceResponse<LHierarchyNode> GetPreviousSibling(ServiceCredentials Credentials, LRI hierarchyLRI, LRI childLRI);
        [OperationContract]
        ServiceResponse<List<LHierarchyNode>> GetChildren(ServiceCredentials Credentials, LRI hierarchyLRI, LRI childLRI);
        [OperationContract]
        ServiceResponse<LHierarchyNode> GetParent(ServiceCredentials Credentials, LRI hierarchyLRI, LRI childLRI);



    }
}
