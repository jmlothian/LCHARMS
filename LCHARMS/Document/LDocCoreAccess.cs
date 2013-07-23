using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LCHARMS.DB.CouchDB;

namespace LCHARMS.Document
{

    public static class LDocCoreAccess
    {
        static SortedDictionary<LRI, LDocumentHeader> LoadedDocs = new SortedDictionary<LRI, LDocumentHeader>();
        static SortedDictionary<string, List<LDocumentPart>> LoadedDocParts = new SortedDictionary<string, List<LDocumentPart>>();
        static SortedDictionary<string, LDocHeaderListRow> DocIndex = new SortedDictionary<string, LDocHeaderListRow>();
        public static bool GUIDAvailable(string guid)
        {
            return !DocIndex.ContainsKey(guid);
        }
        public static void LoadIndex()
        {
            LDocHeaderList hList = CouchDBMgr.GetAllDocIDs();
            for (int i = 0; i < hList.rows.Count; i++)
            {
                DocIndex[hList.rows[i].id] = (hList.rows[i]);
            }
        }
        public static bool LoadDocHeader(string id)
        {
            LDocumentHeader header = CouchDBMgr.ReadDocumentHeader(id);
            if (header != null)
            {
                LoadedDocs[new LRI(header.DocumentLRI)] = header;
                return true;
            }
            return false;
        }
        public static LDocumentHeader GetDocHeader(LRI lri)
        {
            if (LoadedDocs.ContainsKey(lri))
            {
                return LoadedDocs[lri];
            }
            else
            {
                //try to load the doc
                if (LoadDocHeader(lri.DocumentID))
                {
                    return LoadedDocs[lri];
                }
            }
            return null;
        }
        public static List<LDocumentPart> GetDocParts(string id)
        {
            List<LDocumentPart> parts = null;
            //this has to work
            if (LoadDocHeader(id))
            {
                if (LoadedDocParts.ContainsKey(id))
                {
                    parts = LoadedDocParts[id];
                }
                else
                {
                    parts = new List<LDocumentPart>();
                    LoadedDocParts[id] = parts;
                }
            }
            return parts;
        }

        public static void CreateDoc(LRI lri, LDocumentHeader header, List<LDocumentPart> parts)
        {
            LoadedDocs[lri] = header;
            if (LoadedDocParts.ContainsKey(header.DocumentID))
            {
                LoadedDocParts[header.DocumentID].Clear();
            }
            else
            {
                LoadedDocParts[header.DocumentID] = new List<LDocumentPart>();
            }
            for (int i = 0; i < parts.Count; i++)
            {
                parts[i].SequenceNumber = i;
                parts[i].DocumentID = header.DocumentID;
                if (parts[i]._id == "")
                {
                    parts[i]._id = RequestGUID();
                }
                LoadedDocParts[header.DocumentID].Add(parts[i]);
            }
        }
        public static void SaveDoc(LRI lri, LDocumentHeader header, List<LDocumentPart> parts)
        {
            LoadedDocs[lri] = header;
            LoadedDocParts[header.DocumentID] = parts;
            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i]._id == "")
                {
                    parts[i]._id = RequestGUID();
                }
            }
            //save to db
            CouchDBMgr.WriteDocument(header, parts.ToArray());
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
    }
}
