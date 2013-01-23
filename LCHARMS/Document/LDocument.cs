using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace LCHARMS.Document
{
    [DataContract]
    public enum DocumentType { 
        [EnumMember]
        INDEX = 0,
        [EnumMember]
        COLLECTION,
        [EnumMember]
        HIERARCHY,
        [EnumMember]
        LRT 
    };  //some headers are index only, in the case of deleted files

    [DataContract]
    public enum DocumentPartDataType {
        [EnumMember]
        NONE,
        [EnumMember]
        BASE_64,
        [EnumMember]
        FILE_SYSTEM_LINK 
    };

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
        //Fully Qualified Document Type
        // Examples: 
        //   image.compressed.lossless.png
        //   text.lrt
        //   text.xml
        //   data.table.csv
        //   data.table.sql.mysql.rows
        //   data.document.nosql.json
        //   data.list.csv
        [DataMember]
        public string FQDT = "";
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
        //internal ID for this part
        [DataMember]
        public string _id = "";
        [DataMember]
        public string _rev = "";

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
        [DataMember]
        public bool VersionOnly = true;
        [DataMember]
        public bool Latest = true;
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
