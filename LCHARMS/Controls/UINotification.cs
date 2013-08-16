using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace LCHARMS.Controls
{
    [DataContract]
    public enum NOTIFY_TYPE
    {
        [EnumMember]
        MESSAGE = 0, //only docs can receive a message
        [EnumMember]
        CHANGED = 1,  //docs, hierchies, and collections can all signal that something has changed (blue or green notify?  maybe white?)
        [EnumMember]
        ATTENTION = 2 //docs, hierchies, and collections can all request attention (red notify)

    }

    [DataContract]
    public class UINotification
    {
        [DataMember]
        public string NotificationText = "";
        [DataMember]
        public DateTime Posted = DateTime.Now;
        [DataMember]
        public NOTIFY_TYPE NotificationType = NOTIFY_TYPE.ATTENTION;
    }
}
