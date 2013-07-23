using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace LCHARMS.Document.DocumentTransferManager
{
    
    [DataContract]
    public enum TRANSFER_STATUS {
        [EnumMember]
        NONE,
        [EnumMember]
        QUEUED,
        [EnumMember]
        IN_PROGRESS,
        [EnumMember]
        COMPLETE
    };
    [DataContract]
    public class TransferUpdate
    {
        [DataMember]
        public int RemainingParts=0;
        [DataMember]
        public int RemainingSize=0;
        [DataMember]
        public TRANSFER_STATUS Status = TRANSFER_STATUS.NONE;
        [DataMember]
        public bool Error = false;
        [DataMember]
        public string ErrorMessage = "";
        [DataMember]
        public string TicketKey = "";
    }
}
