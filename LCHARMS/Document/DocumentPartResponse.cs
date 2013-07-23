using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace LCHARMS.Document
{
    [DataContract]
    public class DocumentPartResponse
    {
        [DataMember]
        DOCPART_RESPONSE_TYPE ResponseType = DOCPART_RESPONSE_TYPE.DOCUMENT_NOT_FOUND;
        [DataMember]
        public List<LDocumentPart> DocumentParts = new List<LDocumentPart>();
        [DataMember]
        public int Count = 0;
        [DataMember]
        public string WebLink = "";
        [DataMember]
        public string Torrent = "";
        [DataMember]
        public string TicketKey = "";
    }
}
