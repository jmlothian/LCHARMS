using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using LCHARMS.Document;
using System.Security.Cryptography;
using LCHARMS.Identity;
using LCHARMS.Config;
using LCHARMS.Collection;
using LCHARMS.Hierarchy;
using LCHARMS.Client;

namespace LCHARMS.DB.CouchDB
{

    public static class CouchDBMgr
    {
        private static string DBURL = LCHARMSConfig.GetSection().DBServer;// "http://192.168.1.2:5984";
        private static SHA1 Hasher = null;
        public static string Request(string path, string operation, string data = null)
        {
            string retVal = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(MakeURL(path));
            request.Method = operation;
            request.Timeout = 200000;
            if (data != null)
            {
                byte[] postBytes = UTF8Encoding.UTF8.GetBytes(data.ToString());
                request.ContentLength = postBytes.Length;
                using (Stream postStream = request.GetRequestStream())
                {
                    postStream.Write(postBytes, 0, postBytes.Length);
                }
            }
            try
            {
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    retVal = reader.ReadToEnd();
                }
                response.Close();
            }
            catch (WebException ex)
            {
                //log
                if (ex.Response != null)
                {
                    using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        retVal = reader.ReadToEnd();
                        Console.WriteLine(retVal);
                        retVal = "";
                    }
                }
            }
            return retVal;
        }
        private static SHA1 GetHasher()
        {
            if(Hasher == null)
                Hasher = SHA1.Create();
            return Hasher;
        }
        public static void WriteDocument(LDocumentHeader DocHeader, LDocumentPart [] Parts)
        {
            //the header should actually have the ID of the document itself, maybe?  since each part is going to have a different ID
            JsonSerializerSettings set = new JsonSerializerSettings();
            set.NullValueHandling = NullValueHandling.Ignore;
            string CumulativeHash = "";
            DocHeader.DataLength = 0;
            DocHeader.DataHash = "";
            for (int i = 0; i < Parts.Length; i++)
            {
                //precalc
                Parts[i].DataLength = Parts[i].Data.Length;
                Parts[i].DataHash = BitConverter.ToString(GetHasher().ComputeHash(Parts[i].Data)).Replace("-",string.Empty);
                CumulativeHash += Parts[i].DataHash;
                DocHeader.DataLength += Parts[i].DataLength;
            }
            DocHeader.DataHash = BitConverter.ToString(GetHasher().ComputeHash(System.Text.Encoding.ASCII.GetBytes(CumulativeHash))).Replace("-", string.Empty);

            WriteString(DocHeader.DocumentID, JsonConvert.SerializeObject(DocHeader,set));
            //TODO: Here we need to seperate L-CHARMS native data from existing binary formats such as images.
            //  In CouchDB, these other formats should not be written as part of the data itself, but as an attachment.
            for (int i = 0; i < Parts.Length; i++)
            {
                Parts[i].DocumentID = DocHeader.DocumentID;
                Parts[i].SequenceNumber = i; //just to make sure...
                WriteString(Parts[i]._id, JsonConvert.SerializeObject(Parts[i],set));
            }
            //return WriteString(DocHeader.DocumentID, JsonConvert.SerializeObject(data));
        }
        public static string DeleteFile(string id)
        {
            LDocPartIDList lst = CouchDBMgr.GetLastDocRev(id);
            if (lst.rows.Count > 0)
            {
                return Request(id + "?rev=" + lst.rows[0].value, "DELETE");
            }
            return "ERROR - file has no revision data!";
        }
        public static string WriteString(string id, string data)
        {
            return Request(id, "PUT", data);
        }
        public static LDocumentHeader ReadDocumentHeader(string id)
        {
            return ReadDocument<LDocumentHeader>(id);
        }
        
        public static LDocumentPart[] ReadDocumentParts(string id)
        {
            List<LDocumentPart> parts = new List<LDocumentPart>();
            return parts.ToArray();
        }
        
        public static T ReadDocument<T>(string id)
        {
            string str = ReadString(id);
            if (str != "")
                return JsonConvert.DeserializeObject<T>(ReadString(id));
            else return default(T);
        }
        public static string ReadString(string id)
        {
            return Request(id, "GET");
        }
        
        
        
        public static string MakeURL(string id)
        {
            return DBURL + "/documents/" + id;
        }

        public static LDocHeaderList GetAllDocIDs()
        {
            string retVal = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(MakeURL("_all_docs"));
            request.Method = "GET";
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                retVal = reader.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<LDocHeaderList>(retVal);
        }

        public static LDocHeaderList GetIdentities()
        {
            //http://192.168.1.2:5984/documents/_design/~LDOCUMENTHEADER/_view/ListIdentities
            string retVal = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(MakeURL("_design/~LDOCUMENTHEADER/_view/ListIdentities"));
            request.Method = "GET";
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                retVal = reader.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<LDocHeaderList>(retVal);
        }
        public static LDocHeaderList GetPermissionACEs()
        {
            //http://192.168.1.2:5984/documents/_design/~LDOCUMENTHEADER/_view/ListIdentities
            string retVal = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(MakeURL("_design/~LDOCUMENTHEADER/_view/DocPerms"));
            request.Method = "GET";
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                retVal = reader.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<LDocHeaderList>(retVal);
        }

        /// <summary>
        /// Returns all document parts for the given document
        /// </summary>
        /// <param name="iDocumentIDd"></param>
        /// <returns></returns>
        public static LDocPartList GetDocumentParts(string DocumentID)
        {
            string results = Request("_design/~LDOCUMENTHEADER/_view/DocParts?key=\"" + DocumentID + "\"", "GET");
            if (results != "")
                return JsonConvert.DeserializeObject<LDocPartList>(results);
            else return null;
        }
        /// <summary>
        /// Returns all documentpart IDs for the given document
        /// </summary>
        /// <param name="DocumentID"></param>
        /// <returns></returns>
        public static LDocPartIDList GetDocumentPartIDs(string DocumentID)
        {
            string results = Request("_design/~LDOCUMENTHEADER/_view/DocPartID?key=\"" + DocumentID + "\"", "GET");
            if (results != "")
                return JsonConvert.DeserializeObject<LDocPartIDList>(results);
            else return null;
        }

        /// <summary>
        /// Returns a specific document part, use with GetDocumentPartIDs
        /// </summary>
        /// <param name="PartID"></param>
        /// <returns></returns>
        public static LDocPartList GetDocumentPart(string PartID)
        {
            string results = Request("_design/~LDOCUMENTHEADER/_view/DocPart?key=\"" + PartID + "\"", "GET");
            if (results != "")
                return JsonConvert.DeserializeObject<LDocPartList>(results);
            else return null;
        }

        /// <summary>
        /// Returns all documentpart IDs for the given document
        /// </summary>
        /// <param name="DocumentID"></param>
        /// <returns></returns>
        public static LDocPartIDList GetLastDocRev(string DocumentID)
        {
            string results = Request("_design/~LDOCUMENTHEADER/_view/LastRev?key=\"" + DocumentID + "\"", "GET");
            if (results != "")
                return JsonConvert.DeserializeObject<LDocPartIDList>(results);
            else return null;
        }

        /// <summary>
        /// returns all collections
        /// /// </summary>
        /// <param name="PartID"></param>
        /// <returns></returns>
        public static LDBList<LCollection> GetCollections()
        {
            string results = Request("_design/~LDOCUMENTHEADER/_view/Collections", "GET");
            if (results != "")
                return JsonConvert.DeserializeObject<LDBList<LCollection>>(results);
            else return null;
        }

        /// <summary>
        /// returns all collections
        /// /// </summary>
        /// <param name="PartID"></param>
        /// <returns></returns>
        public static LDBList<LHierarchy> GetHierarchies()
        {
            string results = Request("_design/~LDOCUMENTHEADER/_view/Hierarchies", "GET");
            if (results != "")
                return JsonConvert.DeserializeObject<LDBList<LHierarchy>>(results);
            else return null;
        }

        /// <summary>
        /// returns all ClientAccounts
        /// /// </summary>
        /// <param name="PartID"></param>
        /// <returns></returns>
        public static LDBList<ClientAccount> GetClientAccounts()
        {
            string results = Request("_design/~LDOCUMENTHEADER/_view/ClientAccounts", "GET");
            if (results != "")
                return JsonConvert.DeserializeObject<LDBList<ClientAccount>>(results);
            else return null;
        }
    }
}
