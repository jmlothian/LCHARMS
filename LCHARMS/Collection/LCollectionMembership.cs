using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace LCHARMS.Collection
{
    //basically a search index / reverse lookup, consider doing this another way
    //do we really need this?
    [DataContract]
    public class LCollectionMembership
    {
        [DataMember]
        public string DocumentLRI = "";
        [DataMember]
        public List<string> CollectionLRIs = new List<string>();
    }
}
