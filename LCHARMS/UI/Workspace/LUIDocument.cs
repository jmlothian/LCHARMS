using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LCHARMS.Document;
using LCHARMS.Controls;
using System.Runtime.Serialization;

namespace LCHARMS.UI.Workspace
{

    //we don't store the actual document here, just the meta properties
    // need a document manager to actually -get- them
    [DataContract]
    public class LUIDocument
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
        public int NotificationsPending = 0;
        [DataMember]
        public List<UINotification> Notifications = new List<UINotification>();
    }
}
