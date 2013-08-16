using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LCHARMS.Identity;
using LCHARMS.Authentication;
using LCHARMS.UI.Workspace;
using System.Runtime.Serialization;
using LCHARMS.Document;

namespace LCHARMS.Client
{
    //one for each account (multiple identities)
    //the class managing the client accounts should provide a lookup
    [DataContract]
    public class ClientAccount
    {
        public LDocumentHeader AccountHeader = new LDocumentHeader();
        [DataMember]
        public string _id = "";
        [DataMember]
        public string _rev = null;
        [DataMember]
        public string ClientSessionKey = "";
        [DataMember]
        public LRI AccountLRI;
        [NonSerialized] //this is session info, we should never save it.  Make the user login again.
        public Dictionary<LRI,ServiceCredentials> ServiceCredentialsByLRI = new Dictionary<LRI,ServiceCredentials>();
        [NonSerialized] //just a lookup, don't save it.
        public Dictionary<LRI, UserInfo> IdentitiesByLRI = new Dictionary<LRI, UserInfo>();
        //primary list
        [DataMember]
        public List<UserInfo> Identities = new List<UserInfo>();
        [DataMember]
        public List<LWorkspace> Workspaces = new List<LWorkspace>();
        [DataMember]
        public List<string> ActiveWorkspaceIDs = new List<string>();
        public ClientAccount()
        {
            AccountLRI = new LRI("");
            _rev = null;
        }
    }
}
