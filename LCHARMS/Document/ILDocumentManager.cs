using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using LCHARMS.Collection;
using LCHARMS.Hierarchy;
using LCHARMS.Identity;
using LCHARMS.Security;

namespace LCHARMS.Document
{
    [ServiceContract]
    public interface ILDocumentManager
    {
        [OperationContract]
        LDocumentHeader GetDocHeader(LRI lri);
        [OperationContract]
        LDocumentVersionInfo GetDocVersionInfo(LRI lri);
        //GetFileMetaData		LRI, Version
        [OperationContract]
        DocumentPartResponse GetDocPart(LRI lri, int Version, int SequenceNumber);
        [OperationContract]
        DocumentPartResponse GetDocParts(LRI lri, int Version);
        //getthumb
        [OperationContract]
        List<LDocumentVersionInfo> GetFileVersionHistory(LRI lri);
        [OperationContract]
        List<LCollection> GetCollections(LRI lri);
        [OperationContract]
        LCollection GetCurrentCollection(LRI lri, LRI CollectionLRI);
        //not sure this makes sense.. how do we know which heirarchy?  Are all collections part of a hierarchy?
        [OperationContract]
        LHierarchy GetCurrentCollectionHierarchy(LRI lri, LRI CollectionLRI);

        //tags
        [OperationContract]
        void AddTag(LIdentity id, LRI lri, string tag);
        [OperationContract]
        void RemoveTag(LIdentity id, LRI lri, string tag);
        [OperationContract]
        List<string> GetTags(LIdentity id, LRI lri);
        [OperationContract]
        LRI GetTagDocumentLRI(LIdentity id, string tag);

        //versions
        //returns the new version lri
        [OperationContract]
        LDocumentHeader SaveNewVersion(LIdentity ID, LRI lri);
        //update cache, "save local"
        [OperationContract]
        void UpdateDoc(LRI lri, List<LDocumentPart> parts);
        [OperationContract]
        void SavePart(LRI lri, LDocumentPart part, int SequenceNumber);
        [OperationContract]
        void DeleteFileLC(LRI lri);
        [OperationContract]
        LDocumentHeader NewFile(LIdentity UserID, string FQDT, string filename, List<string> Tags, string ParentLRI);
        [OperationContract]
        void SaveDocument(LRI lri, LDocumentHeader header, List<LDocumentPart> parts);

        //hierarchy
        [OperationContract]
        void AppendChild(LRI hierarchyLRI, LRI parentLRI, LRI childLRI);
        [OperationContract]
        void PrependChild(LRI hierarchyLRI, LRI parentLRI, LRI childLRI);
        [OperationContract]
        void InsertChild(LRI hierarchyLRI, LRI parentLRI, LRI childLRI, int index);
        [OperationContract]
        void RemoveChild(LRI hierarchyLRI, LRI parentLRI, LRI childLRI);
        [OperationContract]
        LHierarchyNode GetNextSibling(LRI hierarchyLRI, LRI childLRI);
        [OperationContract]
        LHierarchyNode GetPreviousSibling(LRI hierarchyLRI, LRI childLRI);
        [OperationContract]
        List<LHierarchyNode> GetChildren(LRI hierarchyLRI, LRI childLRI);
        [OperationContract]
        LHierarchyNode GetParent(LRI hierarchyLRI, LRI childLRI);


        //permissions
        [OperationContract]
        void AddPermission(LRI DocumentLRI, LRI FromIdentity, LRI ToIdentity, LDocACLPermission Permission);
        [OperationContract]
        void RevokePermission(LRI DocumentLRI, LRI FromIdentity, LRI ToIdentity, LDocACLPermission Permission);
        [OperationContract]
        bool CheckPermission(LIdentity ID, LRI documentLRI, LDocACLPermission permission);
        //search funcs here

        //subscription funcs here
        //a user can subscribe to a document on another server
        // for stand-alone docs, the other server pushes the latest Version to the user's server
        // for collections and hiarchies, the hierarchy or collection file is pushed and the user's server can choose to automatically download the member files or keep them at "pending" waiting for the user
        // remember collections are NEVER sorted!  To sort a collection, a heirarchy must be created (probably a defined type) that includes how the collection is sorted or filtered as the root-file whose solo child is the collection file
        // likewise, the system can subscribe for certain tasks (for instance, an IM client -> the chat is a hierarchy that is pushed back and forth)
        //    may want to consider having "diff" updates for hierarchies somehow, these could get unwieldy going back and forth
    }

}
