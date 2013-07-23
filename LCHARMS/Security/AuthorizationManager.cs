using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LCHARMS.Identity;
using LCHARMS.Document;
using LCHARMS.Config;
using Newtonsoft.Json;
using LCHARMS.DB.CouchDB;
using LCHARMS.Logging;

namespace LCHARMS.Security
{
    public class AuthorizationManager
    {
        //lookup ACLs by user
        Dictionary<LIdentity, List<LDocACEMap>> ACLsByIdentity = new Dictionary<LIdentity, List<LDocACEMap>>();
        //lookup ACLs by doc LRI
        Dictionary<LRI, Dictionary<LIdentity, LDocACEMap>> ACLsByDocumentLRI = new Dictionary<LRI, Dictionary<LIdentity, LDocACEMap>>();

        //public read list
        Dictionary<LDocACLPermission, List<LRI>> PublicAccess = new Dictionary<LDocACLPermission, List<LRI>>();

        LIdentity PublicID = new LIdentity();

        LDocumentManager DocManager;
        JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();

        public static LIdentity GetAuthIdentityFromLRI(string lri)
        {
            LIdentity ident = new LIdentity();
            LRI IdentLRI = new LRI(lri);
            ident.DomainLRI = IdentLRI.LRIDomain;
            ident.UserLRI = lri;
            ident.UserID = IdentLRI.DocumentID;
            return ident;
        }

        public AuthorizationManager(LDocumentManager DocMgr)
        {
            //wire up the doc manager
            DocManager = DocMgr;

            //setup the json serializer settings
            jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;

            //setup the public user
            PublicID.IDType = LIdentityType.PROVIDER_MANAGED | LIdentityType.PUBLIC;
            //setup the public list
            PublicAccess[LDocACLPermission.ACCESS_NEXT_VERSION] = new List<LRI>();
            PublicAccess[LDocACLPermission.ACCESS_PREV_VERSION] = new List<LRI>();
            PublicAccess[LDocACLPermission.DENY] = new List<LRI>();
            PublicAccess[LDocACLPermission.EXECUTE] = new List<LRI>();
            PublicAccess[LDocACLPermission.GRANT] = new List<LRI>();
            PublicAccess[LDocACLPermission.NONE] = new List<LRI>();
            PublicAccess[LDocACLPermission.READ] = new List<LRI>();
            PublicAccess[LDocACLPermission.WRITE] = new List<LRI>();


            //load all document IDs

            //load all ACLs
                //parse for "public" entries
            LoadACEs();
            //load all UsersInGroups

            //I guess we don't -have- to check for system databases, we can deny access to system files directly here
            //  for instance, groups are system files, but the normal ACL lists would be useful to manage them.
        }

        public void LoadACEs()
        {
            //retrieve all the document headers associate with permissions
            LDocHeaderList hlist = CouchDBMgr.GetPermissionACEs();
            FDebugLog.WriteLog("Permission Header Retrieved " + hlist.total_rows.ToString() + " entries.");
            foreach (LDocHeaderListRow row in hlist.rows)
            {
                FDebugLog.WriteLog("Retrieving Entry: " + row.id);
                LDocPartList parts = CouchDBMgr.GetDocumentParts(row.id);
                LDocACEMap Map = JsonConvert.DeserializeObject<LDocACEMap>(System.Text.Encoding.UTF8.GetString(parts.rows[0].value.Data));
                if (Map != null)
                {
                    LRI docLRI = new LRI(Map.ACE.DocumentLRI);
                    FDebugLog.WriteLog("Loaded " +parts.rows[0].value.DataHash +":" + Map.Identity.UserLRI + " -> " + Map.ACE.DocumentLRI);
                    if (Map.Identity.UserLRI == "")
                    {
                        if (Map.Identity.IDType.HasFlag(LIdentityType.PUBLIC))
                        {
                            if (Map.ACE.Permissions.HasFlag(LDocACLPermission.ACCESS_NEXT_VERSION))
                                PublicAccess[LDocACLPermission.ACCESS_NEXT_VERSION].Add(docLRI);
                            if (Map.ACE.Permissions.HasFlag(LDocACLPermission.ACCESS_PREV_VERSION))
                                PublicAccess[LDocACLPermission.ACCESS_PREV_VERSION].Add(docLRI);
                            if (Map.ACE.Permissions.HasFlag(LDocACLPermission.DENY))
                                PublicAccess[LDocACLPermission.DENY].Add(docLRI);
                            if (Map.ACE.Permissions.HasFlag(LDocACLPermission.EXECUTE))
                                PublicAccess[LDocACLPermission.EXECUTE].Add(docLRI);
                            if (Map.ACE.Permissions.HasFlag(LDocACLPermission.NONE))
                                PublicAccess[LDocACLPermission.NONE].Add(docLRI);
                            if (Map.ACE.Permissions.HasFlag(LDocACLPermission.READ))
                                PublicAccess[LDocACLPermission.READ].Add(docLRI);
                            if (Map.ACE.Permissions.HasFlag(LDocACLPermission.WRITE))
                                PublicAccess[LDocACLPermission.WRITE].Add(docLRI);
                        }
                    }
                    else
                    {
                        if (!ACLsByDocumentLRI.ContainsKey(docLRI))
                        {
                            ACLsByDocumentLRI[docLRI] = new Dictionary<LIdentity, LDocACEMap>();
                        }
                        if (!ACLsByIdentity.ContainsKey(Map.Identity))
                        {
                            ACLsByIdentity[Map.Identity] = new List<LDocACEMap>();
                        }
                        ACLsByIdentity[Map.Identity].Add(Map);
                        ACLsByDocumentLRI[docLRI][Map.Identity] = Map;
                    }
                }
            }
        }

        public LIdentity PublicIdentity
        {
            get { return PublicID; }
        }

        //returns user LRI strings that "own" this file
        public List<string> GetOwnersForDocument(LRI Document)
        {
            List<string> Owners = new List<string>();
            foreach (KeyValuePair<LIdentity, LDocACEMap> userMap in ACLsByDocumentLRI[Document])
            {
                if (userMap.Value.ACE.Permissions.HasFlag(LDocACLPermission.WRITE | LDocACLPermission.GRANT))
                {
                    Owners.Add(userMap.Key.UserLRI);
                }
            }
            return Owners;
        }

        public bool CheckKey(LRI Document, LIdentity Identity, LDocACLPermission perm)
        {
            bool resp = false;
            if (ACLsByDocumentLRI.ContainsKey(Document))
            {
                if (ACLsByDocumentLRI[Document].ContainsKey(Identity))
                {
                    //deny is always false... unless thats what we're looking for
                    if (perm == LDocACLPermission.DENY)
                    {
                        if (!ACLsByDocumentLRI[Document][Identity].ACE.Permissions.HasFlag(perm))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (!ACLsByDocumentLRI[Document][Identity].ACE.Permissions.HasFlag(LDocACLPermission.DENY))
                        {
                            if (!ACLsByDocumentLRI[Document][Identity].ACE.Permissions.HasFlag(perm))
                            {
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    if (PublicAccess[perm].Contains(Document))
                    {
                        resp = true;
                    }
                }
            }
            return resp;
        }
        public bool CanRead(LRI Document, LIdentity Identity)
        {
            return CheckKey(Document, Identity, LDocACLPermission.READ);
        }
        public bool CanWrite(LRI Document, LIdentity Identity)
        {
            return CheckKey(Document, Identity, LDocACLPermission.WRITE);
        }
        public bool CanExecute(LRI Document, LIdentity Identity)
        {
            return CheckKey(Document, Identity, LDocACLPermission.EXECUTE);
        }
        public bool CanGrant(LRI Document, LIdentity Identity, LDocACLPermission perm)
        {
            return CheckKey(Document, Identity, perm) && CheckKey(Document, Identity, LDocACLPermission.GRANT);
        }
        public bool CanAccessPreviousVersion(LRI Document, LIdentity Identity)
        {
            return CheckKey(Document, Identity, LDocACLPermission.ACCESS_PREV_VERSION);
        }
        public bool CanAccessNextVersion(LRI Document, LIdentity Identity)
        {
            return CheckKey(Document, Identity, LDocACLPermission.ACCESS_NEXT_VERSION);
        }

        //IDs can be explicitely denied access to otherwise public files
        public bool IsDenied(LRI Document, LIdentity Identity)
        {
            return CheckKey(Document, Identity, LDocACLPermission.DENY);
        }
        public void RevokePermission(LRI DocumentLRI, LIdentity FromIdentity, LIdentity ToIdentity, LDocACLPermission Permission)
        {
            if (ACLsByDocumentLRI.ContainsKey(DocumentLRI))
            {
                //can this user revoke permissions?
                if (ACLsByDocumentLRI[DocumentLRI].ContainsKey(FromIdentity)
                    && ACLsByDocumentLRI[DocumentLRI][FromIdentity].ACE.Permissions.HasFlag(LDocACLPermission.GRANT))
                {
                    //handle revoke read
                    if (ACLsByDocumentLRI[DocumentLRI][FromIdentity].ACE.Permissions.HasFlag(LDocACLPermission.GRANT) &&
                        ACLsByDocumentLRI[DocumentLRI][FromIdentity].ACE.Permissions.HasFlag(LDocACLPermission.READ) &&
                        Permission.HasFlag(LDocACLPermission.READ))
                    {
                        //do revoke read
                        if (ACLsByDocumentLRI[DocumentLRI].ContainsKey(ToIdentity))
                        {
                            ACLsByDocumentLRI[DocumentLRI][ToIdentity].ACE.Permissions &= ~LDocACLPermission.READ;
                            ACLsByDocumentLRI[DocumentLRI][ToIdentity].ACE.Permissions &= ~LDocACLPermission.WRITE;
                            ACLsByDocumentLRI[DocumentLRI][ToIdentity].ACE.Permissions &= ~LDocACLPermission.GRANT;
                        } //no entry to revoke from?
                    }

                    //revoke write
                    if (ACLsByDocumentLRI[DocumentLRI][FromIdentity].ACE.Permissions.HasFlag(LDocACLPermission.GRANT) &&
                        ACLsByDocumentLRI[DocumentLRI][FromIdentity].ACE.Permissions.HasFlag(LDocACLPermission.WRITE) &&
                        Permission.HasFlag(LDocACLPermission.WRITE))
                    {
                        if (ACLsByDocumentLRI[DocumentLRI].ContainsKey(ToIdentity))
                        {
                            ACLsByDocumentLRI[DocumentLRI][ToIdentity].ACE.Permissions &= ~LDocACLPermission.WRITE;
                            ACLsByDocumentLRI[DocumentLRI][ToIdentity].ACE.Permissions &= ~LDocACLPermission.GRANT;
                        } //no entry to revoke from?
                    }

                    //revoke grant, do we want to allow this?  Interface should check owner.
                    if (ACLsByDocumentLRI[DocumentLRI][FromIdentity].ACE.Permissions.HasFlag(LDocACLPermission.GRANT) &&
                        Permission.HasFlag(LDocACLPermission.GRANT))
                    {
                        if (ACLsByDocumentLRI[DocumentLRI].ContainsKey(ToIdentity))
                        {
                            ACLsByDocumentLRI[DocumentLRI][ToIdentity].ACE.Permissions &= ~LDocACLPermission.GRANT;
                        }
                    }
                }
                else
                {
                    //user can't do anything to this file, ABORT
                }

            }
            else
            {
                //error, file does not exist or is inaccessible due to no permissions
            }
        }

        //function assumes the file exists
        public void AddPermission(LRI DocumentLRI, LIdentity FromIdentity, LIdentity ToIdentity, LDocACLPermission Permission)
        {
            if (ACLsByDocumentLRI.ContainsKey(DocumentLRI))
            {
                //can this user add permissions?
                if (ACLsByDocumentLRI[DocumentLRI].ContainsKey(FromIdentity) 
                    && ACLsByDocumentLRI[DocumentLRI][FromIdentity].ACE.Permissions.HasFlag(LDocACLPermission.GRANT))
                {
                    //handle add read
                    if(ACLsByDocumentLRI[DocumentLRI][FromIdentity].ACE.Permissions.HasFlag(LDocACLPermission.GRANT) &&
                        ACLsByDocumentLRI[DocumentLRI][FromIdentity].ACE.Permissions.HasFlag(LDocACLPermission.READ) &&
                        Permission.HasFlag(LDocACLPermission.READ))
                    {
                        //do add read
                        if (!ACLsByDocumentLRI[DocumentLRI].ContainsKey(ToIdentity))
                        {
                            //no entry for this document/ident pair, create a new set
                            CreateACE(DocumentLRI.DocumentID, ToIdentity, LDocACLPermission.READ, DocumentLRI);
                        }
                        else
                        {
                            ACLsByDocumentLRI[DocumentLRI][ToIdentity].ACE.Permissions |= LDocACLPermission.READ;
                        }
                    }

                    //handle add write
                    if (ACLsByDocumentLRI[DocumentLRI][FromIdentity].ACE.Permissions.HasFlag(LDocACLPermission.GRANT) &&
                        ACLsByDocumentLRI[DocumentLRI][FromIdentity].ACE.Permissions.HasFlag(LDocACLPermission.WRITE) &&
                        Permission.HasFlag(LDocACLPermission.WRITE))
                    {
                        //do add read
                        if (!ACLsByDocumentLRI[DocumentLRI].ContainsKey(ToIdentity))
                        {
                            //no entry for this document/ident pair, create a new set
                            //we add read with write, just in case
                            CreateACE(DocumentLRI.DocumentID, ToIdentity, LDocACLPermission.WRITE | LDocACLPermission.READ, DocumentLRI);
                        }
                        else
                        {
                            ACLsByDocumentLRI[DocumentLRI][ToIdentity].ACE.Permissions |= LDocACLPermission.WRITE | LDocACLPermission.READ;
                        }
                    }

                    if (ACLsByDocumentLRI[DocumentLRI][FromIdentity].ACE.Permissions.HasFlag(LDocACLPermission.GRANT) &&
                        Permission.HasFlag(LDocACLPermission.GRANT))
                    {
                        //do add read
                        if (!ACLsByDocumentLRI[DocumentLRI].ContainsKey(ToIdentity))
                        {
                            //no entry for this document/ident pair, create a new set
                            //grant implies write, which implies read
                            CreateACE(DocumentLRI.DocumentID, ToIdentity, LDocACLPermission.WRITE | LDocACLPermission.GRANT | LDocACLPermission.READ, DocumentLRI);
                        }
                        else
                        {
                            ACLsByDocumentLRI[DocumentLRI][ToIdentity].ACE.Permissions |= LDocACLPermission.WRITE | LDocACLPermission.GRANT | LDocACLPermission.READ;
                        }
                    }
                }
                else
                {
                    //user can't do anything to this file, ABORT
                }

            }
            else
            {
                //error, file does not exist or is inaccessible due to no permissions
            }
        }
        public void SaveACE(LDocACEMap ACEFile)
        {
            LRI lri = new LRI(ACEFile.ACE.DocumentLRI);
            LDocumentHeader NewFileACL = DocManager.NewSystemFileHeader("lcharms.permission.ace", lri.DocumentID + "-ACE", DocumentType.DOC_ACL);
            List<LDocumentPart> ACEMapData = new List<LDocumentPart>();
            ACEMapData.Add(new LDocumentPart());
            ACEMapData[0]._id = LDocumentManager.RequestGUID();
            ACEMapData[0].SequenceNumber = 0;
            ACEMapData[0].Data = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ACEFile, jsonSerializerSettings));
            DocManager.SaveDocument(lri, NewFileACL , ACEMapData);
        }
        public void CreateACE(string FileID, LIdentity Identity, LDocACLPermission Permissions, LRI docLRI = null)
        {
            //acl header
            LDocumentHeader NewFileACL = DocManager.NewSystemFileHeader("lcharms.permission.ace", FileID + "-ACE", DocumentType.DOC_ACL);
            //acl data
            LDocACEMap ACEMap = new LDocACEMap();
            ACEMap.ACE.DocumentID = FileID;
            if (docLRI == null)
            {
                docLRI = new LRI(LCHARMSConfig.GetSection().LRI + "/" + FileID);
            }
            ACEMap.ACE.DocumentLRI = docLRI.ToString();
            ACEMap.Identity = Identity;
            //default permissions
            ACEMap.ACE.Permissions = Permissions;
            SaveACE(ACEMap);

            //add to indexes
            if (Identity != PublicID)
            {
                if (!ACLsByIdentity.ContainsKey(Identity))
                {
                    //easy case, ID doesn't have entries yet
                    ACLsByIdentity[Identity] = new List<LDocACEMap>();
                    ACLsByIdentity[Identity].Add(ACEMap);
                    if (!ACLsByDocumentLRI.ContainsKey(docLRI))
                    {
                        //new doc as well, yay easy cases!
                        ACLsByDocumentLRI[docLRI] = new Dictionary<LIdentity, LDocACEMap>();
                        ACLsByDocumentLRI[docLRI][Identity] = ACEMap;
                    }
                    else
                    {
                        //existing document
                        if (ACLsByDocumentLRI[docLRI].ContainsKey(Identity))
                        {
                            //contains ID already...
                            ACLsByDocumentLRI[docLRI][Identity].ACE.Permissions = 
                                AddMergePermissions(ACEMap.ACE.Permissions, ACLsByDocumentLRI[docLRI][Identity].ACE.Permissions);
                        }
                        else
                        {
                            //doesn't contain this id, easy case
                            ACLsByDocumentLRI[docLRI][Identity] = ACEMap;
                        }
                    }
                }
                else
                {
                    if (!ACLsByDocumentLRI.ContainsKey(docLRI))
                    {
                        //ease case, document has no entries yet
                        ACLsByDocumentLRI[docLRI] = new Dictionary<LIdentity, LDocACEMap>();
                        ACLsByDocumentLRI[docLRI][Identity] = ACEMap;
                    }
                    else
                    {
                        ACLsByDocumentLRI[docLRI][Identity].ACE.Permissions =
                            AddMergePermissions(ACEMap.ACE.Permissions, ACLsByDocumentLRI[docLRI][Identity].ACE.Permissions);
                    }
                }
            }
            else
            {
                //handle public ID access
                if (ACEMap.ACE.Permissions.HasFlag(LDocACLPermission.DENY))
                {
                    //remove all other permissions
                    PublicAccess[LDocACLPermission.ACCESS_NEXT_VERSION].Remove(docLRI);
                    PublicAccess[LDocACLPermission.ACCESS_PREV_VERSION].Remove(docLRI);
                    PublicAccess[LDocACLPermission.DENY].Remove(docLRI);
                    PublicAccess[LDocACLPermission.EXECUTE].Remove(docLRI);
                    PublicAccess[LDocACLPermission.GRANT].Remove(docLRI);
                    PublicAccess[LDocACLPermission.NONE].Remove(docLRI);
                    PublicAccess[LDocACLPermission.READ].Remove(docLRI);
                    PublicAccess[LDocACLPermission.WRITE].Remove(docLRI);
                }
                else
                {
                    if (ACEMap.ACE.Permissions.HasFlag(LDocACLPermission.READ) && !PublicAccess[LDocACLPermission.READ].Contains(docLRI))
                    {
                        PublicAccess[LDocACLPermission.READ].Add(docLRI);
                    }
                    if (ACEMap.ACE.Permissions.HasFlag(LDocACLPermission.WRITE) && !PublicAccess[LDocACLPermission.WRITE].Contains(docLRI))
                    {
                        PublicAccess[LDocACLPermission.WRITE].Add(docLRI);
                    }
                    if (ACEMap.ACE.Permissions.HasFlag(LDocACLPermission.EXECUTE) && !PublicAccess[LDocACLPermission.EXECUTE].Contains(docLRI))
                    {
                        PublicAccess[LDocACLPermission.EXECUTE].Add(docLRI);
                    }
                    if (ACEMap.ACE.Permissions.HasFlag(LDocACLPermission.GRANT) && !PublicAccess[LDocACLPermission.GRANT].Contains(docLRI))
                    {
                        //no, you can't grant to public.
                        //PublicAccess[LDocACLPermission.GRANT].Add(docLRI);
                    }
                    if (ACEMap.ACE.Permissions.HasFlag(LDocACLPermission.ACCESS_NEXT_VERSION) 
                        && !PublicAccess[LDocACLPermission.ACCESS_NEXT_VERSION].Contains(docLRI))
                    {
                        PublicAccess[LDocACLPermission.ACCESS_NEXT_VERSION].Add(docLRI);
                    }
                    if (ACEMap.ACE.Permissions.HasFlag(LDocACLPermission.ACCESS_PREV_VERSION)
                        && !PublicAccess[LDocACLPermission.ACCESS_PREV_VERSION].Contains(docLRI))
                    {
                        PublicAccess[LDocACLPermission.ACCESS_PREV_VERSION].Add(docLRI);
                    }
                }
            }
            
        }
        private LDocACLPermission AddMergePermissions(LDocACLPermission NewPerm, LDocACLPermission OldPerm)
        {
            LDocACLPermission ResPerm = LDocACLPermission.NONE;
            if (NewPerm.HasFlag(LDocACLPermission.DENY))
            {
                ResPerm = LDocACLPermission.DENY;
            }
            else
            {
                //we're adding a permission, so deny must be removed. Deny stands alone, so replace everything
                if(OldPerm.HasFlag(LDocACLPermission.DENY) && NewPerm != LDocACLPermission.NONE)
                {
                    ResPerm = NewPerm;
                } else
                {
                    //no funky conflict? merge them.
                    ResPerm = NewPerm | OldPerm;
                }
            }
            //correct discrepencies
            //write and execute imply read
            if (ResPerm.HasFlag(LDocACLPermission.EXECUTE))
                ResPerm |= LDocACLPermission.READ;
            if (ResPerm.HasFlag(LDocACLPermission.WRITE))
                ResPerm |= LDocACLPermission.READ;
            if (ResPerm.HasFlag(LDocACLPermission.GRANT)) //todo, fix this, we need two types of grant (read,write), for now implies WRITE access
                ResPerm |= LDocACLPermission.WRITE;

            return ResPerm;
        }
    }
}
