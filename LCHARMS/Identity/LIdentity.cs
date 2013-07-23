using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace LCHARMS.Identity
{
    [DataContract]
    [FlagsAttribute]
    [Serializable]
    public enum LIdentityType
    {
        [EnumMember]
        NONE = 0,
        [EnumMember]
        USER = 1,
        [EnumMember]
        GROUP = 2,
        [EnumMember]
        PROVIDER_MANAGED = 4, //the doc provider manages this group, otherwise the group is specific to a user, always set for users (for now)
        [EnumMember]
        PUBLIC = 8 //special user type to indicate publically available docs or information, used primarily by ACLs
    }

    [DataContract]
    [Serializable]
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
    [Serializable]
    public class LIdentity : IComparable, IEquatable<LIdentity>
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
        public string ParentBaseLRI = "";
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

        //for all intents and purposes, if the LRIs match, the represent the same user
        public int CompareTo(object obj)
        {
            if (obj is string)
            {
                return UserLRI.CompareTo((string)obj);
            }
            else
            {
                return UserLRI.CompareTo(((LIdentity)obj).UserLRI);
            }
        }
        public bool Equals(LIdentity ident)
        {
            return UserLRI == ident.UserLRI;
        }
        public override int GetHashCode()
        {
            return UserLRI.GetHashCode();
        }

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
