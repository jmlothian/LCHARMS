using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LCHARMS.Document;
using System.Runtime.Serialization;

namespace LCHARMS.UI.Workspace
{
    [DataContract]
    public class LUICollection
    {
        [DataMember]
        public string DocumentID = "";
        [DataMember]
        public LRI DocumentLRI = null;
        [DataMember]
        public LDocumentHeader DocumentHeader = null;
        [DataMember]
        public int Index = -1; //index in display list
        [DataMember]
        public bool NeedsAttention = false;
    }
}
