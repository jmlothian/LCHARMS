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
    public class LRI
    {
        [DataMember]
        public string URI = "";
        [DataMember]
        public string LRIString = "";
        [DataMember]
        public string LRIDomain = "";
        [DataMember]
        public int Port = 80;
        [DataMember]
        public List<string> Databases = new List<string>();
        [DataMember]
        public string DocumentID = "";
        [DataMember]
        public LDocumentVersionInfo Version = new LDocumentVersionInfo();

        public LRI(string lri, bool IsURI = false)
        {
            if (IsURI)
            {
                //parse uri
            }
            else
            {
                //parse lri
            }
        }
    }
}
