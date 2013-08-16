using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LCHARMS.Document;
using System.Runtime.Serialization;

namespace LCHARMS.UI.Workspace
{
    [DataContract]
    public class LUIHierarchy
    {
        public string DocumentID = "";
        public LRI DocumentLRI = null;
        public LDocumentHeader DocumentHeader = null;
        public int Index = -1; //index in display list
    }
}
