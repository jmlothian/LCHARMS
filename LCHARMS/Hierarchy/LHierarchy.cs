using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using LCHARMS.Document;

namespace LCHARMS.Hierarchy
{
    [DataContract]
    public class LHierarchyNode
    {
        [DataMember]
        public string HierarchyLRI = "";
        [DataMember]
        public string DocumentLRI = "";
        [DataMember]
        public string ParentDocumentLRI = "";
        [DataMember]
        public List<string> ChildLRIs = new List<string>();
    }

    [DataContract]
    public class LHierarchy
    {
        //hierarchy parts are the node infos for each file in the heirarchy
        [DataMember]
        public LDocumentHeader HierarchyHeader = new LDocumentHeader();
        [DataMember]
        public LHierarchyNode RootNode = new LHierarchyNode();
    }
}
