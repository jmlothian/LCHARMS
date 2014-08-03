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
    
    public class LRI : IComparable, IEquatable<LRI>
    {
        [DataMember]
        public string URI = "";
        [DataMember]
        public string LRIString = "";
        [DataMember]
        public string ServiceLRI = "";
        [DataMember]
        public string ServiceURI = "";
        [DataMember]
        public string LRIDomain = "";
        [DataMember]
        public string URIDomain = "";
        [DataMember]
        public string BaseLRI = ""; //domain plus root directories until ~ directory
        [DataMember]
        public string BaseURI = ""; //domain plus root directories until ~ directory
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

        public override string ToString()
        {
            return LRIString;
        }
        public bool Equals(LRI lri)
        {
            return LRIString == lri.LRIString;                
        }
        public override int GetHashCode()
        {
            return LRIString.GetHashCode();
        }
        public int CompareTo(object obj)
        {
            if (obj is string)
            {
                return LRIString.CompareTo((string)obj);
            }
            else
            {
                return LRIString.CompareTo(((LRI)obj).LRIString);
            }
        }
        public LRI(string lri, bool IsURI = false)
        {
            if (lri == "")
            {
                ValidLRI = true; // this is the "public" user LRI
            }
            else
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
                        ParseDB(parts, false);
                        //ParseID(parts);
                    }
                }
                if (IsURI)
                {
                    URI = lri;
                    if (Port != 80)
                    {
                        LRIString = LRIDomain + ":" + Port.ToString() + "/" + Database + "/" + DocumentID;
                        ServiceLRI = LRIDomain + ":" + Port.ToString() + "/" + Database;
                        ServiceURI = URI.Remove(URI.Length - DocumentID.Length, DocumentID.Length);
                    }
                    else
                    {
                        LRIString = LRIDomain + "/" + Database + "/" + DocumentID;
                        ServiceLRI = LRIDomain + "/" + Database;
                        ServiceURI = URIDomain + "/" + Database;
                        ServiceURI = URI.Remove(URI.Length - DocumentID.Length, DocumentID.Length);
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
                        ServiceURI = URIDomain + ":" + Port.ToString() + "/" + Database;
                        ServiceLRI = LRIDomain + ":" + Port.ToString() + "/" + Database;
                    }
                    else
                    {
                        URI = URIDomain + "/" + Database + "/" + DocumentID;
                        ServiceURI = URIDomain + "/" + Database;
                        ServiceLRI = LRIDomain + "/" + Database;
                    }
                    if (!Version.Latest)
                    {
                        URI += "#" + Version.Version.ToString();
                    }
                }
                ValidLRI = true;
            }
        }

        private void ParseID(string[] parts)
        {
            string IDPart = parts[parts.Length - 1];
            if (IDPart.Contains('#'))
            {
                DocumentID = IDPart.Split('#')[0];
                Version.DocumentID = DocumentID;
                try
                {
                    Version.Version = int.Parse(IDPart.Split('#')[1]);
                }
                catch (Exception ex) { }
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
            bool FoundRootDB = false;
            BaseLRI = LRIDomain;
            BaseURI = URIDomain;
            if (Port != 80)
            {
                BaseLRI += ":" + Port.ToString();
                BaseURI += ":" + Port.ToString();
            }
            for (int i = 1; i < len; i++)
            {
                if (parts[i].Length > 0 && parts[i][0] == '~')
                {
                    SystemDatabase = true;
                    FoundRootDB = true;
                }
                if (FoundRootDB)
                {
                    Database += parts[i] + "/";
                    Databases.Add(parts[i]);
                    if (parts[i] == "~users")
                    {
                        ParseID(parts); //parse rest of string
                        break; //we're done here.
                    }
                }
                else
                {
                    if (i == len - 1)
                    {
                        //last item, not a /, must be an ID
                        ParseID(parts); //parse rest of string
                        break; //we're done here.
                    }
                    else
                    {
                        BaseLRI += "/" + parts[i];
                        BaseURI += "/" + parts[i];
                    }
                }
            }
            if (Database.Length > 0)
            {
                Database = Database.Substring(0, Database.Length - 1);
            }
        }

    }
}
