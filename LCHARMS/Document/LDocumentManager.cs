using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LCHARMS.Collection;
using LCHARMS.Hierarchy;
using System.Runtime.Serialization;
using System.ServiceModel;
using LCHARMS.Document.DocumentTransferManager;
using LCHARMS.Config;
using LCHARMS.DB.CouchDB;
using LCHARMS.Security;
using Newtonsoft.Json;
using LCHARMS.Identity;
using LCHARMS.Logging;

namespace LCHARMS.Document
{
    [DataContract]
    enum DOCPART_RESPONSE_TYPE {
        [EnumMember]
        DOCUMENT_NOT_FOUND=0, //doc not found on this system, don't have any more info
        [EnumMember]
        DATA_INCLUDED=1, //include the data in the response.  Can be a single part, or many.  Client is responsible for requesting more if needed
        [EnumMember]
        WEB_LINK=2,      //link to file on a network location via URI, WebLink field is filled out
        [EnumMember]
        TORRENT=3,       //Torrent file supplied to get file, Torrent field is filled out
        [EnumMember]
        TRANSFER_TICKET=4, //Your request is delayed, here's a ticket-key so the service can notify you later when transfer is ready.  
                           //This notification comes as a DocumentPartResponse _pushed_ to the user, and it may contain any of the other responses (or even another ticket)
                           //TicketKey is filled out
        [EnumMember]
        LRI_REDIRECT=5    //We don't have this file, but we know where it is.  Go look here!
    };






    public class LDocumentManager : ILDocumentManager
    {
        #region Internal Variables
        //datetime is the last access time so we can prune
        private SortedDictionary<LRI, Tuple<DateTime, LCollection>> Collections = new SortedDictionary<LRI, Tuple<DateTime, LCollection>>();
        private SortedDictionary<string, List<LCollection>> CollectionByDocLRI = new SortedDictionary<string, List<LCollection>>();

        //not really using these for caching yet
        private SortedDictionary<LRI, Tuple<DateTime, LDocumentHeader>> OpenDocuments = new SortedDictionary<LRI, Tuple<DateTime, LDocumentHeader>>();
        private SortedDictionary<LRI, Tuple<DateTime, LHierarchy>> Hierarchies = new SortedDictionary<LRI, Tuple<DateTime, LHierarchy>>();
        private SortedDictionary<LRI, LDocPartList> OpenDocumentParts = new SortedDictionary<LRI, LDocPartList>();

        static SortedDictionary<string, LDocHeaderListRow> DocIndex = new SortedDictionary<string, LDocHeaderListRow>();
        //lri string,ldocheader
        private SortedDictionary<string, LDocumentHeader> IndexedHeaders = new SortedDictionary<string, LDocumentHeader>();
        //unassociate collections, have doc LRIs by list<string>
        private SortedDictionary<string, LCollection> IndexedCollection = new SortedDictionary<string, LCollection>();
        //private SortedDictionary<string, List<LCollection>> CollectionsByDocLRI = new SortedDictionary<string, List<LCollection>>();

        //lri->userlri->tags
        private SortedDictionary<string, SortedDictionary<string, List<string>>> TagsByDocLRI =
            new SortedDictionary<string, SortedDictionary<string, List<string>>>();

        //tag->userlri->doclri
        private SortedDictionary<string, SortedDictionary<string, LCollection>> CollectionsByTagAndUser =
            new SortedDictionary<string, SortedDictionary<string, LCollection>>();


        private SortedDictionary<LRI, LHierarchy> IndexedHierarchies = new SortedDictionary<LRI, LHierarchy>();

        JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
        public AuthorizationManager AuthManager 
        { 
            get 
            { 
                return _AuthManager; 
            }
            private set
            {
                _AuthManager = value;
            }
        }
        private AuthorizationManager _AuthManager;

        public bool CheckPermission(LIdentity ID, LRI documentLRI, LDocACLPermission permission)
        {
            return AuthManager.CheckKey(documentLRI, ID, permission);
        }

        public static bool GUIDAvailable(string guid)
        {
            return !DocIndex.ContainsKey(guid);
        }
        //this should NOT go into the final product.
        public static void PurgeAllFiles()
        {
            if (DocIndex.Count == 0)
            {
                LoadIndex();
            }
            lock (DocIndex)
            {
                foreach (KeyValuePair<string, LDocHeaderListRow> head in DocIndex)
                {
                    //keep users around...
                    if (!head.Value.id.Contains("~users"))
                    {
                        CouchDBMgr.DeleteFile(head.Value.id);
                    }
                    //CouchDBMgr.DeleteFile(head.Value.DocumentID);
                }
            }
        }
        public static void LoadIndex()
        {
            lock (DocIndex)
            {
                DocIndex.Clear();
                LDocHeaderList hList = CouchDBMgr.GetAllDocIDs();
                for (int i = 0; i < hList.rows.Count; i++)
                {
                    if (!hList.rows[i].id.Contains("_design"))
                    {
                        DocIndex[hList.rows[i].id] = (hList.rows[i]);
                    }
                }
            }
        }
        private void LoadHeaders()
        {
            LDocHeaderList hList = CouchDBMgr.GetAllDocIDs();
            for (int i = 0; i < hList.rows.Count; i++)
            {
                if (!hList.rows[i].id.Contains("_design"))
                {
                    LDocumentHeader head = CouchDBMgr.ReadDocumentHeader(hList.rows[i].id);
                    if (head.DocumentLRI != "")
                    {
                        IndexedHeaders[head.DocumentLRI] = head;
                    }
                    else
                    {
                        FDebugLog.WriteLog("Error: LRI not included in doc: " + head.DocumentID + "-" + head.DocType.ToString());
                        //todo: remove this bandaid
                        if (head.DocType == DocumentType.COLLECTION)
                        {
                            head.DocumentLRI = (new LRI(LCHARMSConfig.GetSection().LRI + "/" + head.DocumentID).ToString());
                            IndexedHeaders[head.DocumentLRI] = head;
                        }
                    }
                }
            }
        }
        public byte[] SerializeToJSONByteArray<T>(T Data)
        {
            return System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Data, jsonSerializerSettings));
        }
        private void LoadCollections()
        {
            LDBList<LCollection> collections = CouchDBMgr.GetCollections();
            foreach (LDBListRow<LCollection> col in collections.rows)
            {
                //get header
                LCollection coll = col.decodedValue;
                if (col.key.Contains("/"))
                {
                    coll.CollectionHeader = IndexedHeaders[col.key];
                }
                else
                {
                    string localID = LCHARMSConfig.GetSection().LRI + "/" + col.key;
                    coll.CollectionHeader = IndexedHeaders[localID];
                }
                IndexedCollection[col.id] = coll;
                string tag = coll.CollectionTags.Tag.Tag;
                List<string> owners = AuthManager.GetOwnersForDocument(new LRI(coll.CollectionHeader.DocumentLRI));


                //add to lookup by doc
                foreach (string lri in coll.DocumentLRIs)
                {
                    if (!CollectionByDocLRI.ContainsKey(lri))
                    {
                        CollectionByDocLRI[lri] = new List<LCollection>();
                        TagsByDocLRI[lri] = new SortedDictionary<string, List<string>>();
                    }
                    CollectionByDocLRI[lri].Add(coll);

                    foreach (string ownerLRI in owners)
                    {
                        if (!TagsByDocLRI[lri].ContainsKey(ownerLRI))
                        {
                            TagsByDocLRI[lri][ownerLRI] = new List<string>();
                        }
                        TagsByDocLRI[lri][ownerLRI].Add(coll.CollectionTags.Tag.Tag);
                    }
                }

                //"tag" collections won't be shared, will they?
                // yeah, they will...
                foreach (string ownerLRI in owners)
                {
                    if (!CollectionsByTagAndUser.ContainsKey(tag))
                    {
                        CollectionsByTagAndUser[coll.CollectionTags.Tag.Tag] = new SortedDictionary<string, LCollection>();
                    }
                    CollectionsByTagAndUser[tag][ownerLRI] = coll;

                    //

                }
            }
            //get all collections
            //iterate through, loading collections
            //create CollectionsByTagAndUser mappings
            //create TagsByDocLRI mappings
        }
        private void LoadHierarchies()
        {

        }
        private void SaveCollection(LCollection coll)
        {
            //remember to set the header = to null first
            LDocumentHeader head = coll.CollectionHeader;
            coll.CollectionHeader = null; //we don't want this extra data saved here
            LDocumentPart part = new LDocumentPart();
            part.SequenceNumber = 0;
            part.DocType = DocumentType.COLLECTION;
            part._id = coll._id;
            List<LDocumentPart> parts = new List<LDocumentPart>();
            part.Data = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(coll, jsonSerializerSettings));
            parts.Add(part);
            SaveDocument(new LRI(head.DocumentLRI), head, parts);

            //reattach head
            coll.CollectionHeader = head;
            IndexedCollection[head.DocumentLRI] = coll;
        }

        ILDocumentTransferManager DocTransferManager = null;
        #endregion
        public LDocumentManager()
        {
            //wire up the authorization manager
            AuthManager = new AuthorizationManager(this);
            //setup the json serializer settings
            jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;

            //create a new ILDocumentTransferManager. Here is ok for now.  Later, we'll want some kind of plugin thing.
            DocTransferManager = new LDocumentSendTransferManager();
            DocTransferManager.SetDocumentManager(this);

            //load the index of all docs we know about
            LoadIndex();
            LoadHeaders();
            LoadCollections();
            LoadHierarchies();
        }

        public LDocumentHeader GetDocHeader(LRI lri)
        {
            //is LRI local?
            if (lri.BaseLRI == LCHARMSConfig.GetSection().LRI)
            {
                //load from index, cannot load files marked for delete, or deleted, or purge, etc.
                if (IndexedHeaders.ContainsKey(lri.LRIString) && IndexedHeaders[lri.LRIString].DocStatus == DocumentStatus.ALIVE)
                    return IndexedHeaders[lri.LRIString];
                return null;
            }
            else
            {
                //not local?
                //setup transfer
                throw new NotImplementedException("Cannot get a remote document header at this time");
            }
        }

        public LDocumentVersionInfo GetDocVersionInfo(LRI lri)
        {
            throw new NotImplementedException();
        }

        public DocumentPartResponse GetDocPart(LRI lri, int Version, int SequenceNumber)
        {
            //here we can change something into a ~web file and transmit the URL instead of the data
            //return DocTransferManager.RequestDocumentPart(lri, Version, SequenceNumber); // trying to remember if this is what we need

            //if (OpenDocuments.ContainsKey(lri)) //do nothing now
            //is LRI local?
            if (lri.BaseLRI == LCHARMSConfig.GetSection().LRI)
            {
                //load from DB
                if (!IndexedHeaders.ContainsKey(lri.LRIString))
                    return null;
                LDocumentHeader header = IndexedHeaders[lri.LRIString];
                LDocPartIDList parts = CouchDBMgr.GetDocumentPartIDs(header.DocumentID);
                if (SequenceNumber <= parts.rows.Count)
                {
                    //get the part
                    LDocPartList docpart = CouchDBMgr.GetDocumentPart(parts.rows[SequenceNumber].value);
                    DocumentPartResponse Resp = new DocumentPartResponse();
                    Resp.Count = 1;
                    List<LDocumentPart> DocumentParts = new List<LDocumentPart>();
                    DocumentParts.Add(docpart.rows[0].value);
                    Resp.DocumentParts = DocumentParts;
                    return Resp;
                }
                else
                {
                }

                /*
                if (header.DataLength < 10000)
                {
                    //just send the data
                }
                else if (header.DataLength < 100000)
                {
                    //uhh.  Save it to a file and send the link.
                }
                else
                {
                    //setup a torrent
                }*/

                
            }
            else
            {
                //not local?
                //setup transfer
            }
            //throw new NotImplementedException();
            return new DocumentPartResponse();
        }

        public DocumentPartResponse GetDocParts(LRI lri, int Version)
        {
            if (lri.BaseLRI == LCHARMSConfig.GetSection().LRI)
            {
                //load from DB
                if (!IndexedHeaders.ContainsKey(lri.LRIString))
                    return null;
                LDocumentHeader header = IndexedHeaders[lri.LRIString];
                //get the part
                LDocPartList docpart = CouchDBMgr.GetDocumentParts(header.DocumentID);
                DocumentPartResponse Resp = new DocumentPartResponse();
                Resp.Count = docpart.rows.Count;
                List<LDocumentPart> DocumentParts = new List<LDocumentPart>();
                for (int i = 0; i < docpart.rows.Count; i++)
                {
                    DocumentParts.Add(docpart.rows[i].value);
                }
                Resp.DocumentParts = DocumentParts;
                return Resp;
            }
            else
            {
                //dont support remote transfers atm
            }
            throw new NotImplementedException();
        }

        public List<LDocumentVersionInfo> GetFileVersionHistory(LRI lri)
        {
            throw new NotImplementedException();
        }
        #region Collection functions
        //parent responsible for checking perms and removing items
        public List<LCollection> GetCollections(LRI lri)
        {
            if (CollectionByDocLRI.ContainsKey(lri.LRIString))
                return CollectionByDocLRI[lri.LRIString];
            return null;
        }

        public LCollection GetCurrentCollection(LRI lri, LRI CollectionLRI)
        {
            if (IndexedCollection.ContainsKey(CollectionLRI.LRIString))
                return IndexedCollection[CollectionLRI.LRIString];
            return null;
        }

        public LHierarchy GetCurrentCollectionHierarchy(LRI lri, LRI CollectionLRI)
        {
            throw new NotImplementedException();
        }

        private LCollection CreateCollection(LIdentity id, string tag="")
        {
            LCollection Collection = new LCollection();
            if (tag == "")
                Collection.AnonymousCollection = true;
            else
                Collection.AnonymousCollection = false;
            
   
            Collection.SystemCollection = false;
            Collection._id = RequestGUID();
            Collection.CollectionTags = new LCollectionTag();
            Collection.CollectionTags.IsComposite = false;
            Collection.CollectionTags.Tag.Tag = tag;

            
            //create a header for the collection
            string ID = RequestGUID();
            LDocumentHeader NewFileHeader = new LDocumentHeader();
            LRI lri = new LRI(LCHARMSConfig.GetSection().LRI + "/" + ID);
            NewFileHeader.DocType = DocumentType.DOC_HEADER;
            NewFileHeader.DocumentID = ID;
            NewFileHeader.FQDT = "lcharms.collection";
            NewFileHeader.FileName = tag;
            NewFileHeader.DocumentLRI = lri.ToString();
            NewFileHeader.IsCopy = false;
            NewFileHeader.LastAccessDate = DateTime.Now;
            NewFileHeader.DataLength = 0;

            //create an ACL for this new file
            // assign it to the creation user
            AuthManager.CreateACE(ID, id, LDocACLPermission.GRANT |
                                LDocACLPermission.WRITE |
                                LDocACLPermission.READ |
                                LDocACLPermission.ACCESS_NEXT_VERSION |
                                LDocACLPermission.ACCESS_PREV_VERSION);
            // automatically add "deny-public" 
            AuthManager.CreateACE(ID, AuthManager.PublicIdentity, LDocACLPermission.DENY);
            Collection.CollectionHeader = NewFileHeader;

            SaveCollection(Collection);
            return Collection;
        }
        //we need to pass ID so we know which user is making the request
        //  this is needed in case the collection doesn't exist and we need to create it
        public void AddTag(LIdentity id, LRI lri, string tag)
        {
            //does collection exist?
            if (CollectionsByTagAndUser.ContainsKey(tag))
            {
                if (CollectionsByTagAndUser[tag].ContainsKey(id.UserLRI))
                {
                    CollectionsByTagAndUser[tag][id.UserLRI].DocumentLRIs.Add(tag);
                }
                else
                {
                    CollectionsByTagAndUser[tag][id.UserLRI] = CreateCollection(id,tag);
                    CollectionsByTagAndUser[tag][id.UserLRI].DocumentLRIs.Add(lri.LRIString);
                }
            }
            else
            {
                CollectionsByTagAndUser[tag] = new SortedDictionary<string,LCollection>();
                CollectionsByTagAndUser[tag][id.UserLRI] = CreateCollection(id,tag);
                CollectionsByTagAndUser[tag][id.UserLRI].DocumentLRIs.Add(lri.LRIString);
            }

            //tag index
            if (TagsByDocLRI.ContainsKey(lri.LRIString))
            {
                if (!TagsByDocLRI[lri.LRIString].ContainsKey(id.UserLRI))
                {
                    TagsByDocLRI[lri.LRIString][id.UserLRI] = new List<string>();
                }
            }
            else
            {
                TagsByDocLRI[lri.LRIString] = new SortedDictionary<string, List<string>>();
                TagsByDocLRI[lri.LRIString][id.UserLRI] = new List<string>();

            }
            TagsByDocLRI[lri.LRIString][id.UserLRI].Add(tag);

            //save collection will update all the appropriate indexes
            SaveCollection(CollectionsByTagAndUser[tag][id.UserLRI]);
        }

        public void RemoveTag(LIdentity id, LRI lri, string tag)
        {
            if (CollectionsByTagAndUser.ContainsKey(tag))
            {
                if (CollectionsByTagAndUser[tag].ContainsKey(id.UserLRI))
                {
                    CollectionsByTagAndUser[tag][id.UserLRI].DocumentLRIs.Remove(lri.LRIString);
                    TagsByDocLRI[lri.LRIString][id.UserLRI].Remove(tag);
                    SaveCollection(CollectionsByTagAndUser[tag][id.UserLRI]);
                }
            }
        }

        public LRI GetTagDocumentLRI(LIdentity id, string tag)
        {
            LRI lri = null;
            if (CollectionsByTagAndUser.ContainsKey(tag))
            {
                if (CollectionsByTagAndUser[tag].ContainsKey(id.UserLRI))
                {
                    lri = new LRI(CollectionsByTagAndUser[tag][id.UserLRI].CollectionHeader.DocumentLRI);
                }
            }
            return lri;
        }
        public List<string> GetTags(LIdentity id, LRI lri)
        {
            List<string> Tags = new List<string>();
            if (TagsByDocLRI.ContainsKey(lri.LRIString))
            {
                if (TagsByDocLRI[lri.LRIString].ContainsKey(id.UserLRI))
                {
                    return TagsByDocLRI[lri.LRIString][id.UserLRI];
                }
            }
            return Tags;
        }
        #endregion
        #region File Manipulations
        public LDocumentHeader SaveNewVersion(LIdentity ID, LRI lri)
        {
            //copy old version to a totally new LRI
            // save reference as CurrentVersionLRI
            LDocumentHeader head = GetDocHeader(lri);
            //clone head
            //argh, clone...todo
            //get doc parts
            //clone parts
            //reassemble, call save
            // --need to handle permissions specially so that they aren't the default, but instead copied.
            throw new NotImplementedException();
        }

        public void UpdateDoc(LRI lri, List<LDocumentPart> parts)
        {
            if (IndexedHeaders.ContainsKey(lri.LRIString))
            {
                LDocumentHeader header = IndexedHeaders[lri.LRIString];
                SaveDocument(lri, header, parts);
            }
        }

        public void SavePart(LRI lri, LDocumentPart part, int SequenceNumber)
        {
            throw new NotImplementedException();
        }

        public void SaveDocument(LRI lri, LDocumentHeader header, List<LDocumentPart> parts)
        {
            //todo: caching
            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i]._id == null || parts[i]._id == "")
                {
                    parts[i]._id = RequestGUID();
                    parts[i].SequenceNumber = i;
                }
                else
                {
                    if (parts[i]._id != null)
                    {
                        LDocPartIDList lst = CouchDBMgr.GetLastDocRev(parts[i]._id);
                        if (lst.rows.Count> 0)
                        {
                            parts[i]._rev = lst.rows[0].value;
                        }
                    }
                }
            }
            //save to db
            header._id = header.DocumentID;
            LDocPartIDList lst2 = CouchDBMgr.GetLastDocRev(header.DocumentID);
            if(lst2.rows.Count > 0)
                header._rev = lst2.rows[0].value;
            CouchDBMgr.WriteDocument(header, parts.ToArray());
        }

        public void DeleteFileLC(LRI lri)
        {
            //files are...never really deleted from lcharms
            //set fileheader to "deleted"
            //delete document parts

            //get all member collections
            //remove from collections

            //get all member heirarchies
            //if leaf node, remove from hierarchy
            //  if root node, delete hierarchy?  (LEAVE DOCS, JUST DELETE THE HIERARCHY FILE)
            throw new NotImplementedException();
        }

        public static string RequestGUID()
        {
            string GUID = Guid.NewGuid().ToString();
            while (DocIndex.ContainsKey(GUID))
            {
                GUID = Guid.NewGuid().ToString();
            }
            DocIndex[GUID] = new LDocHeaderListRow();
            DocIndex[GUID].id = GUID;
            DocIndex[GUID].key = GUID;
            return GUID;
        }

        public LDocumentHeader NewFile(LIdentity UserID, string FQDT, string filename, List<string> Tags, string ParentLRI="")
        {
            //create new header 
            //get a new ID for the doc
            string ID = RequestGUID();
            LDocumentHeader NewFileHeader = new LDocumentHeader();
            LRI lri = new LRI(LCHARMSConfig.GetSection().LRI + "/" + ID);
            NewFileHeader.DocType = DocumentType.DOC_HEADER;
            NewFileHeader.DocumentID = ID;
            NewFileHeader.FQDT = FQDT;
            NewFileHeader.FileName = filename;
            NewFileHeader.DocumentLRI = lri.ToString();
            NewFileHeader.IsCopy = false;
            NewFileHeader.LastAccessDate = DateTime.Now;
            NewFileHeader.DocumentParentLRI = ParentLRI;
            NewFileHeader.DataLength = 0;

            //create an ACL for this new file
            // assign it to the creation user
            AuthManager.CreateACE(ID, UserID, LDocACLPermission.GRANT |
                                LDocACLPermission.WRITE |
                                LDocACLPermission.READ |
                                LDocACLPermission.ACCESS_NEXT_VERSION |
                                LDocACLPermission.ACCESS_PREV_VERSION);
            // automatically add "deny-public" 
            AuthManager.CreateACE(ID, AuthManager.PublicIdentity, LDocACLPermission.DENY);

            SaveDocument(lri, NewFileHeader, new List<LDocumentPart>());

            //add it to the open documents
            OpenDocuments.Add(lri,new Tuple<DateTime,LDocumentHeader>(DateTime.Now,NewFileHeader));
            OpenDocumentParts.Add(lri, new LDocPartList());
            //add to the index
            IndexedHeaders[lri.LRIString] = NewFileHeader;

            

            //LDocHeaderListRow hlistrow = new LDocHeaderListRow();
            //LDocHeaderListRowVal hlistrowval = new LDocHeaderListRowVal();
            //hlistrow.value
            //DocIndex[ID] = (hList.rows[i]);

            return NewFileHeader;
        }
        public LDocumentHeader NewSystemFileHeader(string FQDT, string filename, DocumentType DocType)
        {
            string ID = RequestGUID();
            LDocumentHeader NewFileHeader = new LDocumentHeader();
            LRI lri = new LRI(LCHARMSConfig.GetSection().LRI + "/" + ID);

            NewFileHeader.DocType = DocType;
            NewFileHeader.DocumentID = ID;
            NewFileHeader.FileName = filename;
            NewFileHeader.FQDT = FQDT;
            NewFileHeader.DocumentLRI = lri.ToString();
            IndexedHeaders[lri.LRIString] = NewFileHeader;
            return NewFileHeader;
        }
        #endregion
        #region Hierarchy
        public LHierarchy CreateHierarchy(LIdentity id, string Name = "")
        {
            LHierarchy Hierarchy = new LHierarchy();

            Hierarchy.HierarchyName = Name;
            Hierarchy._id = RequestGUID();

            /*
            Collection.SystemCollection = false;
            Collection._id = RequestGUID();
            Collection.CollectionTags = new LCollectionTag();
            Collection.CollectionTags.IsComposite = false;
            Collection.CollectionTags.Tag.Tag = tag;
            */

            //create a header for the collection
            string ID = RequestGUID();
            LDocumentHeader NewFileHeader = new LDocumentHeader();
            LRI lri = new LRI(LCHARMSConfig.GetSection().LRI + "/" + ID);
            NewFileHeader.DocType = DocumentType.DOC_HEADER;
            NewFileHeader.DocumentID = ID;
            NewFileHeader.FQDT = "lcharms.collection";
            NewFileHeader.FileName = Name;
            NewFileHeader.DocumentLRI = lri.ToString();
            NewFileHeader.IsCopy = false;
            NewFileHeader.LastAccessDate = DateTime.Now;
            NewFileHeader.DataLength = 0;

            //create an ACL for this new file
            // assign it to the creation user
            AuthManager.CreateACE(ID, id, LDocACLPermission.GRANT |
                                LDocACLPermission.WRITE |
                                LDocACLPermission.READ |
                                LDocACLPermission.ACCESS_NEXT_VERSION |
                                LDocACLPermission.ACCESS_PREV_VERSION);
            // automatically add "deny-public" 
            AuthManager.CreateACE(ID, AuthManager.PublicIdentity, LDocACLPermission.DENY);

            Hierarchy.HierarchyHeader = NewFileHeader;

            SaveHierarchy(Hierarchy);

            //update indexes
            IndexedHierarchies[new LRI(Hierarchy.HierarchyHeader.DocumentID)] = Hierarchy;

            return Hierarchy;
        }
        private void SaveHierarchy(LHierarchy hierarchy)
        {
            //remember to set the header = to null first
            LDocumentHeader head = hierarchy.HierarchyHeader;
            hierarchy.HierarchyHeader = null; //we don't want this extra data saved here
            LHierarchyNode Root = hierarchy.RootNode;
            hierarchy.RootNode = null;
            LDocumentPart part = new LDocumentPart();
            part.SequenceNumber = 0;
            part.DocType = DocumentType.COLLECTION;
            part._id = hierarchy._id;
            List<LDocumentPart> parts = new List<LDocumentPart>();
            part.Data = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(hierarchy, jsonSerializerSettings));
            parts.Add(part);
            SaveDocument(new LRI(head.DocumentLRI), head, parts);

            //reattach head and root node
            hierarchy.HierarchyHeader = head;
            hierarchy.RootNode = Root;
            //IndexedCollection[head.DocumentLRI] = hierarchy;
        }


        public void AppendChild(LRI hierarchyLRI, LRI parentLRI, LRI childLRI)
        {
            //if parentLRI is blank, we assume the root node
            if(IndexedHierarchies.ContainsKey(hierarchyLRI))
            {
                if (parentLRI == null || parentLRI.LRIString == "")
                {
                    LHierarchyNode newRootNode = new LHierarchyNode();
                        newRootNode.DocumentLRI = childLRI.LRIString;
                        IndexedHierarchies[hierarchyLRI].SetRoot(newRootNode);
                }
                else
                {
                    if (IndexedHierarchies[hierarchyLRI].NodesByLRI.ContainsKey(parentLRI.LRIString))
                    {
                        if (IndexedHierarchies[hierarchyLRI].NodesByLRI.ContainsKey(childLRI.LRIString))
                        {
                            //this node is already in the hierarchy at a different location, don't make a new one
                            IndexedHierarchies[hierarchyLRI].NodesByLRI[parentLRI.LRIString].AppendChild(
                                IndexedHierarchies[hierarchyLRI].NodesByLRI[childLRI.LRIString]
                            );
                        }
                        else
                        {
                            LHierarchyNode newNode = new LHierarchyNode();
                            newNode.DocumentLRI = childLRI.LRIString;
                            IndexedHierarchies[hierarchyLRI].NodesByLRI[parentLRI.LRIString].AppendChild(newNode);
                        }
                    }
                }
            }
        }
        public void PrependChild(LRI hierarchyLRI, LRI parentLRI, LRI childLRI)
        {
            //if parentLRI is blank, we assume the root node
            if (IndexedHierarchies.ContainsKey(hierarchyLRI))
            {
                if (parentLRI == null || parentLRI.LRIString == "")
                {
                    LHierarchyNode newRootNode = new LHierarchyNode();
                    newRootNode.DocumentLRI = childLRI.LRIString;
                    IndexedHierarchies[hierarchyLRI].SetRoot(newRootNode);
                }
                else
                {
                    if (IndexedHierarchies[hierarchyLRI].NodesByLRI.ContainsKey(parentLRI.LRIString))
                    {
                        if (IndexedHierarchies[hierarchyLRI].NodesByLRI.ContainsKey(childLRI.LRIString))
                        {
                            //this node is already in the hierarchy at a different location, don't make a new one
                            IndexedHierarchies[hierarchyLRI].NodesByLRI[parentLRI.LRIString].PrependChild(
                                IndexedHierarchies[hierarchyLRI].NodesByLRI[childLRI.LRIString]
                            );
                        }
                        else
                        {
                            LHierarchyNode newNode = new LHierarchyNode();
                            newNode.DocumentLRI = childLRI.LRIString;
                            IndexedHierarchies[hierarchyLRI].NodesByLRI[parentLRI.LRIString].PrependChild(newNode);
                        }
                    }
                }
            }
        }
        public void InsertChild(LRI hierarchyLRI, LRI parentLRI, LRI childLRI, int index)
        {
            //if parentLRI is blank, we assume the root node
            if (IndexedHierarchies.ContainsKey(hierarchyLRI))
            {
                if (parentLRI == null || parentLRI.LRIString == "")
                {
                    LHierarchyNode newRootNode = new LHierarchyNode();
                    newRootNode.DocumentLRI = childLRI.LRIString;
                    IndexedHierarchies[hierarchyLRI].SetRoot(newRootNode);
                }
                else
                {
                    if (IndexedHierarchies[hierarchyLRI].NodesByLRI.ContainsKey(parentLRI.LRIString))
                    {
                        if (IndexedHierarchies[hierarchyLRI].NodesByLRI.ContainsKey(childLRI.LRIString))
                        {
                            //this node is already in the hierarchy at a different location, don't make a new one
                            IndexedHierarchies[hierarchyLRI].NodesByLRI[parentLRI.LRIString].InsertAtChild(
                                IndexedHierarchies[hierarchyLRI].NodesByLRI[childLRI.LRIString], index
                            );
                        }
                        else
                        {
                            LHierarchyNode newNode = new LHierarchyNode();
                            newNode.DocumentLRI = childLRI.LRIString;
                            IndexedHierarchies[hierarchyLRI].NodesByLRI[parentLRI.LRIString].InsertAtChild(newNode,index);
                        }
                    }
                }
            }
        }
        public void RemoveChild(LRI hierarchyLRI, LRI parentLRI, LRI childLRI)
        {
            //if parentLRI is blank, we assume the root node
            //check if actually exists
            if (IndexedHierarchies.ContainsKey(hierarchyLRI))
            {
                //is the child really a member
                LHierarchy h = IndexedHierarchies[hierarchyLRI];
                if (h.NodesByLRI.ContainsKey(childLRI.LRIString))
                {
                    //get the child
                    LHierarchyNode childNode = h.NodesByLRI[childLRI.LRIString];
                    //make sure the client isn't confused and the parent is real
                    if ((parentLRI == null || parentLRI.LRIString == "") && childNode.Parent == null && childNode == h.RootNode)
                    {
                        //it thinks this is a root node, we check many things above.
                        h.SetRoot(null);
                    }
                    else
                    {
                        //has a parent, and it matches what its supposed to
                        if (childNode.Parent.DocumentLRI == parentLRI.LRIString)
                        {
                            h.NodesByLRI[parentLRI.LRIString].RemoveChild(childNode);
                        }
                    }
                }
            }
        }

        public LHierarchyNode GetNextSibling(LRI hierarchyLRI, LRI childLRI)
        {
            if (IndexedHierarchies.ContainsKey(hierarchyLRI))
            {
                //is the child really a member
                LHierarchy h = IndexedHierarchies[hierarchyLRI];
                if (h.NodesByLRI.ContainsKey(childLRI.LRIString))
                {
                    return h.NodesByLRI[childLRI.LRIString].NextSibling;
                }
            }
            return null;
        }

        public LHierarchyNode GetPreviousSibling(LRI hierarchyLRI, LRI childLRI)
        {
            if (IndexedHierarchies.ContainsKey(hierarchyLRI))
            {
                //is the child really a member
                LHierarchy h = IndexedHierarchies[hierarchyLRI];
                if (h.NodesByLRI.ContainsKey(childLRI.LRIString))
                {
                    return h.NodesByLRI[childLRI.LRIString].PreviousSibling;
                }
            }
            return null;
        }

        public List<LHierarchyNode> GetChildren(LRI hierarchyLRI, LRI childLRI)
        {
            if (IndexedHierarchies.ContainsKey(hierarchyLRI))
            {
                //is the child really a member
                LHierarchy h = IndexedHierarchies[hierarchyLRI];
                if (h.NodesByLRI.ContainsKey(childLRI.LRIString))
                {
                    return h.NodesByLRI[childLRI.LRIString].Children;
                }
            }
            return null;
        }

        public LHierarchyNode GetParent(LRI hierarchyLRI, LRI childLRI)
        {
            if (IndexedHierarchies.ContainsKey(hierarchyLRI))
            {
                //is the child really a member
                LHierarchy h = IndexedHierarchies[hierarchyLRI];
                if (h.NodesByLRI.ContainsKey(childLRI.LRIString))
                {
                    return h.NodesByLRI[childLRI.LRIString].Parent;
                }
            }
            return null;
        }
        #endregion
        #region Permissions
        public void AddPermission(LRI DocumentLRI, LRI FromIdentity, LRI ToIdentity, LDocACLPermission Permission)
        {
            LIdentity FromID = new LIdentity();
            FromID.UserLRI = FromIdentity.LRIString;
            LIdentity ToID = new LIdentity();
            ToID.UserLRI = ToIdentity.LRIString;
            if (ToID.UserLRI == "")
                ToID = AuthManager.PublicIdentity;
            AuthManager.AddPermission(DocumentLRI, FromID, ToID, Permission);
        }

        public void RevokePermission(LRI DocumentLRI, LRI FromIdentity, LRI ToIdentity, LDocACLPermission Permission)
        {
            LIdentity FromID = new LIdentity();
            FromID.UserLRI = FromIdentity.LRIString;
            LIdentity ToID = new LIdentity();
            ToID.UserLRI = ToIdentity.LRIString;
            if (ToID.UserLRI == "")
                ToID = AuthManager.PublicIdentity;
            AuthManager.RevokePermission(DocumentLRI, FromID, ToID, Permission);
        }
        #endregion 
    }
}
