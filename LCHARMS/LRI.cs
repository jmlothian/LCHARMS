using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LCHARMS.Document;
using System.Runtime.Serialization;

namespace LCHARMS
{
    //(LRI://com.dreamsmithfoundry.web.en:80/database/documentidentifier#version).
    [DataContract]
    public enum LRI_TYPE { 
        [EnumMember]
        LRI_NONE=0,
        [EnumMember]
        LRI_DOMAIN=1,
        [EnumMember]
        LRI_DB = 2,
        [EnumMember]
        LRI_ID = 3
    };
    [DataContract]
    public class LRI
    {
        [DataMember]
        public string URI = "";
        [DataMember]
        public string LRIString = "";
        [DataMember]
        public string LRIDomain = "";
        [DataMember]
        public string URIDomain = "";
        [DataMember]
        public int Port = 80;
        [DataMember]
        public List<string> Databases = new List<string>();
        [DataMember]
        public string DocumentID = "";
        [DataMember]
        public LDocumentVersionInfo Version = new LDocumentVersionInfo();
        [DataMember]
        public bool ValidLRI = false;
        [DataMember]
        public LRI_TYPE LRI_Type = LRI_TYPE.LRI_NONE;
        [DataMember]
        public string Database = "";
        [DataMember]
        public bool SystemDatabase = false;

        public LRI(string lri, bool IsURI = false)
        {
            string[] parts = lri.Split('/');
            if (parts[0].Contains(':'))
            {
                Port = int.Parse(parts[0].Split(':')[1]);
                parts[0] = parts[0].Split(':')[0];
            }
            if (IsURI)
            {
                //parse uri
                //no prefix
                // technically /database/../ is optional
                //  www.something.tld/database/database/id#version


                URIDomain = parts[0];
                URI = lri;
                //reverse domain
                LRIDomain = string.Join(".", URIDomain.Split('.').Reverse());


            }
            else
            {
                //parse lri
                LRIDomain = parts[0];
                URIDomain = string.Join(".", LRIDomain.Split('.').Reverse());
            }


            if (parts.Length == 1)
            {
                LRI_Type = LRI_TYPE.LRI_DOMAIN;
            }
            else
            {

                if (lri[lri.Length - 1] == '/')
                {
                    LRI_Type = LRI_TYPE.LRI_DB;
                    ParseDB(parts);
                }
                else
                {
                    LRI_Type = LRI_TYPE.LRI_ID;
                    ParseDB(parts, true);
                    ParseID(parts);
                }
            }
            if (IsURI)
            {
                URI = lri;
                if (Port != 80)
                {
                    LRIString = LRIDomain + ":" + Port.ToString() + "/" + Database + "/" + DocumentID;
                }
                else
                {
                    LRIString = LRIDomain + "/" + Database + "/" + DocumentID;
                }
                if (!Version.Latest)
                {
                    LRIString += "#" + Version.Version.ToString();
                }
            }
            else
            {
                LRIString = lri;
                if (Port != 80)
                {
                    URI = URIDomain + ":" + Port.ToString() + "/" + Database + "/" + DocumentID;
                }
                else
                {
                    URI = URIDomain + "/" + Database + "/" + DocumentID;
                }
                if (!Version.Latest)
                {
                    URI += "#" + Version.Version.ToString();
                }
            }
            ValidLRI = true;

        }

        private void ParseID(string[] parts)
        {
            string IDPart = parts[parts.Length - 1];
            if (IDPart.Contains('#'))
            {
                DocumentID = IDPart.Split('#')[0];
                Version.DocumentID = DocumentID;
                Version.Version = int.Parse(IDPart.Split('#')[1]);
                Version.Latest = false;
            }
            else
            {
                DocumentID = IDPart;
            }
        }

        private void ParseDB(string[] parts, bool LastIsID=false)
        {
            int len = LastIsID == true ? parts.Length - 1 : parts.Length;
            for (int i = 1; i < len; i++)
            {
                if (i == 1 && parts[i].Length > 0 && parts[i][0] == '~')
                    SystemDatabase = true;
                Database += parts[i] + "/";
                Databases.Add(parts[i]);
            }
            Database = Database.Substring(0, Database.Length - 1);
        }
    }
}
