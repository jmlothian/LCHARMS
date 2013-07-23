using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace LCHARMS.Identity
{
    [DataContract]
    public class UserInGroup
    {
        [DataMember]
        public LIdentity Group = new LIdentity();
        [DataMember]
        public LIdentity User = new LIdentity();
    }
}
