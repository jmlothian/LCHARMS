using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace LCHARMS.Identity
{
    [DataContract]
    [FlagsAttribute]
    public enum LIdentityType
    {
        NONE = 0,
        USER = 1,
        GROUP = 2,
        PROVIDER_MANAGED = 4 //the doc provider manages this group, otherwise the group is specific to a user, always set for users (for now)
    }

    [DataContract]
    public class ChildIdentity
    {
        [DataMember]
        public string ParentLRI = ""; //parent is "this" system
        [DataMember]
        public string ChildGeneratedKey = "";
        [DataMember]
        public string ChildLRI = "";
        [DataMember]
        public int ChildPINHash = 0;
    }

    [DataContract]
    public class LIdentity
    {
        [DataMember]
        public string Username = "";
        [DataMember]
        public string DomainLRI = "";
        [DataMember]
        public string UserID = "";
        [DataMember]
        public string UserLRI = "";
        [DataMember]
        public string ParentUserID = "";
        [DataMember]
        public string ParentDomainLRI = "";
        //hash of this userid and parent userid
        [DataMember]
        public string UserHash = "";
        [DataMember]
        public LIdentityType IDType = LIdentityType.USER | LIdentityType.PROVIDER_MANAGED;
        //responsible party for this ID on its registered system.  Generally used for user-managed user-groups
        [DataMember]
        public string OwnerUserID = "";
        [DataMember]
        public string OwnerDomainLRI = "";
        [DataMember]
        public string KeyForParent = "";

    }

    [DataContract]
    public class UserInGroup
    {
        [DataMember]
        public LIdentity Group = new LIdentity();
        [DataMember]
        public LIdentity User = new LIdentity();
    }

    //core identity, used only on the lcharms core ID server
    [DataContract]
    public class LCoreIdentity
    {
        [DataMember]
        LIdentity Identity = new LIdentity();
        [DataMember]
        public string FirstName = "";
        [DataMember]
        public string LastName = "";
        [DataMember]
        public string PrimaryEmailAddress = "";
    }
}
