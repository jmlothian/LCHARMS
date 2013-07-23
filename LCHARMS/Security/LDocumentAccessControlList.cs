using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using LCHARMS.Document;
using LCHARMS.Identity;

namespace LCHARMS.Security
{
    [DataContract]
    [FlagsAttribute]
    public enum LDocACLPermission //todo, GRANT should be grant-read, grant-write.  Right now, it will imply WRITE access
    {
        NONE = 0,
        READ = 1,
        WRITE = 2,
        EXECUTE = 4,
        GRANT = 8,
        ACCESS_PREV_VERSION = 16,
        ACCESS_NEXT_VERSION = 32,
        DENY = 64, // deny overrides any other permission, so you can exclude people who might be in an allowed group
        OWNER = 128 //unused... still not sure what to do here
    }
    [DataContract]
    public class LDocumentAccessControlEntry
    {
        [DataMember]
        public string DocumentID = "";
        [DataMember]
        public string DocumentLRI = "";
        [DataMember]
        public LDocumentVersionInfo CurrentVersionInfo = new LDocumentVersionInfo();
        [DataMember]
        public LDocACLPermission Permissions = LDocACLPermission.READ | LDocACLPermission.ACCESS_NEXT_VERSION;
    }

    //document providers should provide role-based mappings of role to multiple ACLs
    [DataContract]
    public class LDocACEMap
    {
        [DataMember]
        public LDocumentAccessControlEntry ACE = new LDocumentAccessControlEntry();
        [DataMember]
        public LIdentity Identity = new LIdentity();
        [DataMember]
        public string ACLName = "";
    }
}
