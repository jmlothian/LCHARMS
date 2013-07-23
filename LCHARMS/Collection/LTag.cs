using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace LCHARMS.Collection
{
    [DataContract]
    public class LTag
    {
        [DataMember]
        public string Tag = "";
    }
}
