using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace LCHARMS.Collection
{
    [DataContract]
    public class TagToDocument
    {
        [DataMember]
        LTag Tag = new LTag();
        [DataMember]
        public string DocumentLRI = "";
        [DataMember]
        public bool SystemGenerated = false;
    }
}
