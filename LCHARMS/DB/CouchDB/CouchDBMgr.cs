using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using LCHARMS.Document;

namespace LCHARMS.DB.CouchDB
{
    public static class CouchDBMgr
    {
        private static string DBURL = "http://192.168.1.2:5984";
        public static string Request(string id, string operation, string data = null)
        {
            string retVal = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(MakeURL(id));
            request.Method = operation;

            if (data != null)
            {
                byte[] postBytes = UTF8Encoding.UTF8.GetBytes(data.ToString());
                request.ContentLength = postBytes.Length;
                using (Stream postStream = request.GetRequestStream())
                {
                    postStream.Write(postBytes, 0, postBytes.Length);
                }
            }

            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                retVal = reader.ReadToEnd();
            }
            return retVal;
        }

        public static void WriteDocument(LDocumentHeader DocHeader, LDocumentPart [] Parts)
        {
            //the header should actually have the ID of the document itself, maybe?  since each part is going to have a different ID
            WriteString(DocHeader.DocumentID, JsonConvert.SerializeObject(DocHeader));
            //TODO: Here we need to seperate L-CHARMS native data from existing binary formats such as images.
            //  In CouchDB, these other formats should not be written as part of the data itself, but as an attachment.
            for (int i = 0; i < Parts.Length; i++)
            {
                WriteString(Parts[i]._id, JsonConvert.SerializeObject(Parts[i]));
            }
            //return WriteString(DocHeader.DocumentID, JsonConvert.SerializeObject(data));
        }
        public static string WriteString(string id, string data)
        {
            return Request(id, "PUT", data);
        }
        public static T ReadDocument<T>(string id)
        {
            return JsonConvert.DeserializeObject<T>(ReadString(id));
        }
        public static string ReadString(string id)
        {
            return Request(id, "GET");
        }
        public static string MakeURL(string id)
        {
            return DBURL + "/documents/" + id;
        }
    }
}
