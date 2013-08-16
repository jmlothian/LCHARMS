using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LCHARMS.Document;
using System.Runtime.Serialization;

namespace LCHARMS.UI.Workspace
{
    [DataContract]
    public enum WorkspaceStateFlags
    {
        [EnumMember]
        CLOSED = 0,
        [EnumMember]
        OPEN = 1,
        [EnumMember]
        ACTIVE = 2
    }
    [DataContract]
    public enum WorkspaceMetricStateFlags
    {
        [EnumMember]
        COLLAPSED = 0, //collapsed, minimal representation
        [EnumMember]
        MINIMIZED = 1, //minimized, not expanded with details
        [EnumMember]
        EXPANDED = 3, //expanded with extra details shown
        [EnumMember]
        MAXIMIZED = 4 //this is the currently active document in the workspace.  There _can_ be multiple active docs, if the workspace metrics allow, but this will be implemented later.
    }

    [DataContract]
    public class LWorkspace
    {
        [DataMember]
        public int WorkspaceID = -1;
        [DataMember]
        public LRI WorkspaceLRI = null;
        [DataMember]
        public LRI UserLRI = null;
        [DataMember]
        public int ActiveDocumentID = -1;
        [DataMember]
        public int ActiveHierarchyID = -1;
        [DataMember]
        public int ActiveCollectionID = -1;
        [DataMember]
        public List<LUIDocument> OpenDocuments = new List<LUIDocument>();
        [DataMember]
        public List<LUICollection> OpenCollections = new List<LUICollection>();
        [DataMember]
        public List<LUIHierarchy> OpenHierarchies = new List<LUIHierarchy>();
    }
}
