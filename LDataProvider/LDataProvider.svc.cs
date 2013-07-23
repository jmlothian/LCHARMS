using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using LCHARMS.Logging;
using LCHARMS.Document;
using LCHARMS.Identity;
using LCHARMS.Client;
using LCHARMS.LIdentityProvider;
using LCHARMS;
using LCHARMS.Security;
using LCHARMS.Document.DocumentTransferManager;
using LCHARMS.Collection;
using LCHARMS.Hierarchy;

namespace LDataProvider
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    public class LDataProvider : ILDataProvider
    {
        //initialize service
        private static LConnectionManager ConnManager = new LConnectionManager();
        private static ILDocumentManagerChannel DocManager;
        private static ILIdentityProvider IDProvider;
        private static string IDProvLRI = "net.sytes.rhalin/Sites/LCHARMS/IDProvider/";
        private static string DocManagerLRI = "127.0.0.1:8002";

        private bool CanAccessDocument(ServiceCredentials Credentials, LRI DocumentLRI, LDocACLPermission Permission)
        {
            bool Valid = true;
            LIdentity id = AuthorizationManager.GetAuthIdentityFromLRI(Credentials.UserLRIString);
            Valid = DocManager.CheckPermission(id, DocumentLRI, Permission);
            return Valid;
        }
        private bool ValidateSession(ServiceCredentials Credentials, LRI fileLRI)
        {
            bool Valid = true; //todo: change to default of false, uncomment below
            if (fileLRI.SystemDatabase == true) //system databases are not accessible to users in this manner
            {
                Valid = false;
            }
            else
            {
                //validate file access
                
            }
            //Valid = IDProvider.ValidateParentSession(Credentials.UserLRIString, Credentials.SessionKey);
            return Valid;
        }

        public LDataProvider()
        {
            lock (DocManager)
            {
                if (DocManager == null)
                {
                    DocManager = ConnManager.GetDocManagerConnection(new LRI(DocManagerLRI, true));
                    IDProvider = ConnManager.GetIDConnection(new LRI(IDProvLRI));
                }
            }
            FDebugLog.WriteLog("Starting Log - LDataProvider");
        }

        //LIdentity UserID, string FQDT, string filename, List<string> Tags, string ParentLRI=""
        public ServiceResponse<LDocumentHeader> NewFile(ServiceCredentials Credentials, string FQDT, string filename, List<string> Tags, string ParentLRI = "")
        {
            if (!filename.Contains("~")) //cannot create a system database entry this way
            {
                if (ValidateSession(Credentials, new LRI(filename)))
                {
                    LIdentity ident = AuthorizationManager.GetAuthIdentityFromLRI(Credentials.UserLRIString);
                    ServiceResponse<LDocumentHeader> Rep = new ServiceResponse<LDocumentHeader>(DocManager.NewFile(ident, FQDT, filename, Tags, ParentLRI));
                    if (((LDocumentHeader)Rep.ResponseObject).DocumentLRI == "")
                    {
                        Rep.Error = true;
                        Rep.Message = "Unable to create a new file";
                    }
                    return Rep;
                }
                else return ServiceResponse<LDocumentHeader>.InvalidCredentails();
            }
            else return ServiceResponse<LDocumentHeader>.InvalidCredentails();
        }



        public TransferUpdate CheckTransferStatus(ServiceCredentials Credentials, string TicketKey)
        {
            return null;
        }
        public List<TransferUpdate> CheckTransferStatuses(ServiceCredentials Credentials)
        {
            List<TransferUpdate> Updates = null;

            return Updates;
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
            if (ValidateSession(Credentials, lri) && CanAccessDocument(Credentials,lri, LDocACLPermission.READ))
            {
                ServiceResponse<LDocumentHeader> Rep = new ServiceResponse<LDocumentHeader>(DocManager.GetDocHeader(lri));
                if (((LDocumentHeader)Rep.ResponseObject).DocumentLRI == "")
                {
                    Rep.Error = true;
                    Rep.Message = "Unable to get document header or document does not exist";
                }
                return Rep;
            }
            else return ServiceResponse<LDocumentHeader>.InvalidCredentails();
        }

        public ServiceResponse<LDocumentVersionInfo> GetDocVersionInfo(ServiceCredentials Credentials, LRI lri)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<DocumentPartResponse> GetDocPart(ServiceCredentials Credentials, LRI lri, int Version, int SequenceNumber)
        {
            if (ValidateSession(Credentials, lri) && CanAccessDocument(Credentials, lri, LDocACLPermission.READ))
            {
                ServiceResponse<DocumentPartResponse> Rep = new ServiceResponse<DocumentPartResponse>(DocManager.GetDocPart(lri,Version, SequenceNumber));
                if (((DocumentPartResponse)Rep.ResponseObject).Count == 0)
                {
                    Rep.Error = true;
                    Rep.Message = "FILL IN REAL ERROR HERE";
                }
                return Rep;
            }
            else return ServiceResponse<DocumentPartResponse>.InvalidCredentails();
        }

        public ServiceResponse<DocumentPartResponse> GetDocParts(ServiceCredentials Credentials, LRI lri, int Version)
        {
            if (ValidateSession(Credentials, lri) && CanAccessDocument(Credentials, lri, LDocACLPermission.READ))
            {
                ServiceResponse<DocumentPartResponse> Rep = new ServiceResponse<DocumentPartResponse>(DocManager.GetDocParts(lri,Version));
                if (((DocumentPartResponse)Rep.ResponseObject).Count == 0)
                {
                    Rep.Error = true;
                    Rep.Message = "FILL IN REAL ERROR HERE";
                }
                return Rep;
            }
            else return ServiceResponse<DocumentPartResponse>.InvalidCredentails();
        }

        public ServiceResponse<List<LDocumentVersionInfo>> GetFileVersionHistory(ServiceCredentials Credentials, LRI lri)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<List<LCollection>> GetCollections(ServiceCredentials Credentials, LRI lri)
        {
            if (ValidateSession(Credentials, lri) && CanAccessDocument(Credentials, lri, LDocACLPermission.READ))
            {
                LIdentity ident = AuthorizationManager.GetAuthIdentityFromLRI(Credentials.UserLRIString);
                ServiceResponse<List<LCollection>> Rep = new ServiceResponse<List<LCollection>>(DocManager.GetCollections(lri));

                return Rep;
            }
            else return ServiceResponse<List<LCollection>>.InvalidCredentails();
        }

        public ServiceResponse<LCHARMS.Collection.LCollection> GetCurrentCollection(ServiceCredentials Credentials, LRI lri, LRI CollectionLRI)
        {
            if (ValidateSession(Credentials, lri) && CanAccessDocument(Credentials, lri, LDocACLPermission.READ))
            {
                ServiceResponse<LCollection> Rep = new ServiceResponse<LCollection>(DocManager.GetCurrentCollection(lri, CollectionLRI));
                if (((LCollection)Rep.ResponseObject) == null)
                {
                    Rep.Error = true;
                    Rep.Message = "FILL IN REAL ERROR HERE";
                }
                return Rep;
            }
            else return ServiceResponse<LCollection>.InvalidCredentails();
        }

        public ServiceResponse<LCHARMS.Hierarchy.LHierarchy> GetCurrentCollectionHierarchy(ServiceCredentials Credentials, LRI lri, LRI CollectionLRI)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<bool> AddTag(ServiceCredentials Credentials, LRI lri, string tag)
        {
            if (ValidateSession(Credentials, lri) && CanAccessDocument(Credentials, lri, LDocACLPermission.READ))
            {
                LIdentity ident = AuthorizationManager.GetAuthIdentityFromLRI(Credentials.UserLRIString);
                DocManager.AddTag(ident, lri, tag);
                ServiceResponse<bool> Rep = new ServiceResponse<bool>(true);
                return Rep;
            }
            else return ServiceResponse<bool>.InvalidCredentails();
        }

        public ServiceResponse<bool> RemoveTag(ServiceCredentials Credentials, LRI lri, string tag)
        {
            if (ValidateSession(Credentials, lri))
            {
                LIdentity ident = AuthorizationManager.GetAuthIdentityFromLRI(Credentials.UserLRIString);
                LRI tagLRI = DocManager.GetTagDocumentLRI(ident, tag);
                if (tagLRI != null)
                {
                    if (CanAccessDocument(Credentials, tagLRI, LDocACLPermission.WRITE))
                    {
                        DocManager.RemoveTag(ident, lri, tag);
                        ServiceResponse<bool> Rep = new ServiceResponse<bool>(true);
                        return Rep;
                    }
                    else
                    {
                        return ServiceResponse<bool>.InvalidCredentails();
                    }
                }
                else
                {
                    ServiceResponse<bool> Rep = new ServiceResponse<bool>(false);
                    Rep.Message = "Tag does not exist";
                    return Rep;
                }
            }
            else return ServiceResponse<bool>.InvalidCredentails();
        }

        public ServiceResponse<List<string>> GetTags(ServiceCredentials Credentials, LRI lri)
        {
            //even if you have a tag applied, if you can't get to the doc, you cant see it.  Too bad.
            if (ValidateSession(Credentials, lri) && CanAccessDocument(Credentials, lri, LDocACLPermission.READ))
            {
                LIdentity ident = AuthorizationManager.GetAuthIdentityFromLRI(Credentials.UserLRIString);
                ServiceResponse<List<string>> Rep = new ServiceResponse<List<string>>(DocManager.GetTags(ident, lri));
                //todo: if we really want, we can check perms on each tag returned here...

                return Rep;
            }
            else return ServiceResponse<List<string>>.InvalidCredentails();
        }

        public ServiceResponse<LDocumentHeader> SaveNewVersion(ServiceCredentials Credentials, LRI lri)
        {
            //only need read access to copy a file!
            if (ValidateSession(Credentials, lri) && CanAccessDocument(Credentials, lri, LDocACLPermission.READ))
            {
                LIdentity ident = AuthorizationManager.GetAuthIdentityFromLRI(Credentials.UserLRIString);

                ServiceResponse<LDocumentHeader> Rep = new ServiceResponse<LDocumentHeader>(DocManager.SaveNewVersion(ident, lri));
                if (Rep.ResponseObject == null || Rep.ResponseObject.DocumentLRI == "")
                {
                    Rep.Error = true;
                    Rep.Message = "FILL IN REAL ERROR HERE";
                }
                return Rep;
            }
            else return ServiceResponse<LDocumentHeader>.InvalidCredentails();
        }

        public ServiceResponse<bool> UpdateDoc(ServiceCredentials Credentials, LRI lri, List<LDocumentPart> parts)
        {
            if (ValidateSession(Credentials, lri) && CanAccessDocument(Credentials, lri, LDocACLPermission.WRITE))
            {
                DocManager.UpdateDoc(lri, parts);
                ServiceResponse<bool> Rep = new ServiceResponse<bool>(true);
                return Rep;
            }
            else return ServiceResponse<bool>.InvalidCredentails();
        }

        public ServiceResponse<bool> SavePart(ServiceCredentials Credentials, LRI lri, LDocumentPart part, int SequenceNumber)
        {
            if (ValidateSession(Credentials, lri) && CanAccessDocument(Credentials, lri, LDocACLPermission.WRITE))
            {
                DocManager.SavePart(lri, part, SequenceNumber);
                ServiceResponse<bool> Rep = new ServiceResponse<bool>(true);
                return Rep;
            }
            else return ServiceResponse<bool>.InvalidCredentails();
        }

        public ServiceResponse<bool> DeleteFileLC(ServiceCredentials Credentials, LRI lri)
        {
            throw new NotImplementedException();
        }







        public ServiceResponse<bool> AppendChild(ServiceCredentials Credentials, LRI hierarchyLRI, LRI parentLRI, LRI childLRI)
        {
            if (ValidateSession(Credentials, hierarchyLRI) && CanAccessDocument(Credentials, hierarchyLRI, LDocACLPermission.WRITE))
            {
                //todo: validate read access to parent and child lri as well
                DocManager.AppendChild(hierarchyLRI, parentLRI, childLRI);
                ServiceResponse<bool> Rep = new ServiceResponse<bool>(true);
                return Rep;
            }
            else return ServiceResponse<bool>.InvalidCredentails();
        }

        public ServiceResponse<bool> PrependChild(ServiceCredentials Credentials, LRI hierarchyLRI, LRI parentLRI, LRI childLRI)
        {
            if (ValidateSession(Credentials, hierarchyLRI) && CanAccessDocument(Credentials, hierarchyLRI, LDocACLPermission.WRITE))
            {
                //todo: validate read access to parent and child lri as well
                DocManager.PrependChild(hierarchyLRI, parentLRI, childLRI);
                ServiceResponse<bool> Rep = new ServiceResponse<bool>(true);
                return Rep;
            }
            else return ServiceResponse<bool>.InvalidCredentails();
        }

        public ServiceResponse<bool> InsertChild(ServiceCredentials Credentials, LRI hierarchyLRI, LRI parentLRI, LRI childLRI, int index)
        {
            if (ValidateSession(Credentials, hierarchyLRI) && CanAccessDocument(Credentials, hierarchyLRI, LDocACLPermission.WRITE))
            {
                //todo: validate read access to parent and child lri as well
                DocManager.InsertChild(hierarchyLRI, parentLRI, childLRI,index);
                ServiceResponse<bool> Rep = new ServiceResponse<bool>(true);
                return Rep;
            }
            else return ServiceResponse<bool>.InvalidCredentails();
        }

        public ServiceResponse<bool> RemoveChild(ServiceCredentials Credentials, LRI hierarchyLRI, LRI parentLRI, LRI childLRI)
        {
            if (ValidateSession(Credentials, hierarchyLRI) && CanAccessDocument(Credentials, hierarchyLRI, LDocACLPermission.WRITE))
            {
                //todo: validate read access to parent and child lri as well
                DocManager.RemoveChild(hierarchyLRI, parentLRI, childLRI);
                ServiceResponse<bool> Rep = new ServiceResponse<bool>(true);
                return Rep;
            }
            else return ServiceResponse<bool>.InvalidCredentails();
        }

        public ServiceResponse<LHierarchyNode> GetNextSibling(ServiceCredentials Credentials, LRI hierarchyLRI, LRI childLRI)
        {
            if (ValidateSession(Credentials, hierarchyLRI) && CanAccessDocument(Credentials, hierarchyLRI, LDocACLPermission.READ))
            {
                //todo: validate read access to parent and child lri as well
                LHierarchyNode node = DocManager.GetNextSibling(hierarchyLRI, childLRI);
                ServiceResponse<LHierarchyNode> Rep = new ServiceResponse<LHierarchyNode>(node);
                return Rep;
            }
            else return ServiceResponse<LHierarchyNode>.InvalidCredentails();
        }

        public ServiceResponse<LHierarchyNode> GetPreviousSibling(ServiceCredentials Credentials, LRI hierarchyLRI, LRI childLRI)
        {
            if (ValidateSession(Credentials, hierarchyLRI) && CanAccessDocument(Credentials, hierarchyLRI, LDocACLPermission.READ))
            {
                //todo: validate read access to parent and child lri as well
                LHierarchyNode node = DocManager.GetPreviousSibling(hierarchyLRI, childLRI);
                ServiceResponse<LHierarchyNode> Rep = new ServiceResponse<LHierarchyNode>(node);
                return Rep;
            }
            else return ServiceResponse<LHierarchyNode>.InvalidCredentails();
        }

        public ServiceResponse<List<LHierarchyNode>> GetChildren(ServiceCredentials Credentials, LRI hierarchyLRI, LRI childLRI)
        {
            if (ValidateSession(Credentials, hierarchyLRI) && CanAccessDocument(Credentials, hierarchyLRI, LDocACLPermission.READ))
            {
                //todo: validate read access to parent and child lri as well
                List<LHierarchyNode> nodes = DocManager.GetChildren(hierarchyLRI, childLRI);
                ServiceResponse<List<LHierarchyNode>> Rep = new ServiceResponse<List<LHierarchyNode>>(nodes);
                return Rep;
            }
            else return ServiceResponse<List<LHierarchyNode>>.InvalidCredentails();
        }

        public ServiceResponse<LHierarchyNode> GetParent(ServiceCredentials Credentials, LRI hierarchyLRI, LRI childLRI)
        {
            if (ValidateSession(Credentials, hierarchyLRI) && CanAccessDocument(Credentials, hierarchyLRI, LDocACLPermission.READ))
            {
                //todo: validate read access to parent and child lri as well
                LHierarchyNode node = DocManager.GetParent(hierarchyLRI, childLRI);
                ServiceResponse<LHierarchyNode> Rep = new ServiceResponse<LHierarchyNode>(node);
                return Rep;
            }
            else return ServiceResponse<LHierarchyNode>.InvalidCredentails();
        }
    }
}
