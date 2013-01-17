using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace LCHARMS.Document
{
    [DataContract]
    public enum DocumentType { INDEX = 0, COLLECTION, HIERARCHY, LRT };  //some headers are index only, in the case of deleted files

    [DataContract]
    public enum DocumentPartDataType {NONE,BASE_64, FILE_SYSTEM_LINK };

    [DataContract]
    public class LDocumentHeader
    {
        [DataMember]
        public string DocumentID = "";
        [DataMember]
        public string DocumentLRI = "";
        [DataMember]
        public string DocumentParentLRI = "";
        [DataMember]
        public string FileName = "";
        [DataMember]
        public DateTime LastAccessDate;
        [DataMember]
        public DateTime LastAccessDateCurrentVersion;
        [DataMember]
        public bool IsCopy = false;
        [DataMember]
        public string CopyFromLRI = "";
        [DataMember]
        public DocumentType DocType = DocumentType.INDEX;
        [DataMember]
        public int DocSubType = -1;
        [DataMember]
        public bool IsDeleted = false;
        [DataMember]
        public DocumentPartDataType DocPartDataType = DocumentPartDataType.NONE;
        [DataMember]
        LDocumentVersionInfo CurrentVersionInfo = new LDocumentVersionInfo();
        [DataMember]
        public int DataHash = 0;
        [DataMember]
        public int DataLength = 0;
    }

    [DataContract]
    public class LDocumentPart
    {
        [DataMember]
        public string DocumentID = "";
        [DataMember]
        public int SequenceNumber = 0;
        [DataMember]
        public byte[] Data = null;
        [DataMember]
        LDocumentVersionInfo VersionInfo = new LDocumentVersionInfo();
        [DataMember]
        public int DataHash = 0;
        [DataMember]
        public int DataLength = 0;
    }

    [DataContract]
    public class LDocumentVersionInfo
    {
        [DataMember]
        public string DocumentID = "";
        [DataMember]
        public int Version = 1;
        [DataMember]
        public int Revision = 1;
        [DataMember]
        public int CumulativeRevision = 1;
    }

    public class VersionInformation
    {
        [DataMember]
        LDocumentVersionInfo VersionInfo = new LDocumentVersionInfo();
        [DataMember]
        public string VersionComment = "";
        [DataMember]
        public string ModifyUserID = "";
        [DataMember]
        public string ModifySystemLRI = "";
        [DataMember]
        public DateTime ModifiedOn;
        [DataMember]
        public bool TemporaryAutosaveVersion = false;

    }
}
