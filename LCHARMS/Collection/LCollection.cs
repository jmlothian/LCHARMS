using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using LCHARMS.Document;

namespace LCHARMS.Collection
{

    //file parts of a collection are the headers of the files in the collection
    //collections can contain collections!
    [DataContract]
    public class LCollection
    {
        [DataMember]
        public string _id = "";
        [DataMember]
        public string _rev = null;
        [DataMember]
        public LDocumentHeader CollectionHeader = new LDocumentHeader();
        [DataMember]
        public LCollectionTag CollectionTags = new LCollectionTag();
        [DataMember]
        public List<string> DocumentLRIs = new List<string>();
        //Anony collections are those that are not defined by tags
        // generally used for temporary collections, or collections of otherwise unrelated files (which should instead be in a heirarchy)
        [DataMember]
        public bool AnonymousCollection = false;
        //System generated Collection, we should probably strongly id these somehow, use for everything from "marked for delete" to colors, date ranges, or "currently indexing", etc.
        [DataMember]
        public bool SystemCollection = false;
    }
    //workspace contains collections and other info
}
