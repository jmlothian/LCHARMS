using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace LCHARMS.Identity
{
    [DataContract]
    [Serializable]
    public class ServiceCredentials
    {
        [DataMember]
        public string UserLRIString = "";
        [DataMember]
        public string SessionKey = "";
        [DataMember]
        public LRI ClientAccountLRI;
        [DataMember]
        public string ClientSessionKey = "";

        public ServiceCredentials(string lristr, string session)
        {
            UserLRIString = lristr;
            SessionKey = session;
        }
    }
}
