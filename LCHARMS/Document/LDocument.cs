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
        COLLECTION=1,
        [EnumMember]
        HIERARCHY=2,
        [EnumMember]
        DOC_PART=3,
        [EnumMember]
        DOC_HEADER=4,
        [EnumMember]
        DOC_ACL = 5,
        [EnumMember]
        LRT=6
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
    public enum DocumentStatus
    {
        [EnumMember]
        ALIVE,
        [EnumMember]
        TO_DELETE, //marked for deletion *also used by "nice" providers to tell other providers that a file is going away and they may want to make a copy.  Should provide a "notify" interface to make this an active process when we know other providers with this file
        [EnumMember]
        DELETED, //delete, but keep the header
        [EnumMember]
        PURGE, //delete and purge the header
        [EnumMember]
        PENDING //The file is not yet available, but has been requested from somewhere else.

    };


    [DataContract]
    public class LDocumentHeader
    {
        //internal data
        [DataMember]
        public string _id = "";
        [DataMember]
        public string _rev = null;
        [DataMember]
        public string DocumentID = "";
        [DataMember]
        public string DocumentLRI = "";
        [DataMember]
        public string CurrentVersionLRI = ""; //blank, otherwise this is an old archived version of the document referenced here
        [DataMember]
        public string DocumentParentLRI = ""; //similar to above, but intentionally copied as a fork or the original
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
        public DocumentType DocType = DocumentType.DOC_HEADER;
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
        public DocumentStatus DocStatus = DocumentStatus.ALIVE;
        [DataMember]
        public DocumentPartDataType DocPartDataType = DocumentPartDataType.NONE;
        [DataMember]
        LDocumentVersionInfo CurrentVersionInfo = new LDocumentVersionInfo();
        [DataMember]
        public string DataHash = "";
        [DataMember]
        public int DataLength = 0;
        [DataMember]
        public bool RemotelySubscribed = false; //is this file an external one we are subscribed to recieve updates for?
    }

    [DataContract]
    public class LDocumentPart
    {
        //internal ID for this part
        [DataMember]
        public string _id = "";
        [DataMember]
        public string _rev = null;
        [DataMember]
        public DocumentType DocType = DocumentType.DOC_PART;

        [DataMember]
        public string DocumentID = "";
        [DataMember]
        public int SequenceNumber = 0;
        [DataMember]
        public byte[] Data = null;
        [DataMember]
        LDocumentVersionInfo VersionInfo = new LDocumentVersionInfo();
        [DataMember]
        public string DataHash = "";
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
    public class LDocHeaderListRowVal
    {
        public string rev = "";
    }
    public class LDocHeaderListRow
    {
        public string id = "";
        public string key = "";
        public LDocHeaderListRowVal value = new LDocHeaderListRowVal();
    }
    public class LDocHeaderList
    {
        public int total_rows = 0;
        public int offset = 0;
        public List<LDocHeaderListRow> rows = new List<LDocHeaderListRow>();
    }
    public class LDocPartListRow
    {
        public string id = "";
        public string key = "";
        public LDocumentPart value = new LDocumentPart();
    }
    public class LDocPartList
    {
        public int total_rows = 0;
        public int offset = 0;
        public List<LDocPartListRow> rows = new List<LDocPartListRow>();
    }

    public class LDocPartIDListRow
    {
        public string id = "";      //the document part ID
        public string key = "";     //the document ID
        public string value = "";   //should be the same as ID
    }
    public class LDocPartIDList
    {
        public int total_rows = 0;
        public int offset = 0;
        public List<LDocPartIDListRow> rows = new List<LDocPartIDListRow>();
    }


    public class LDBListRow<T>
    {
        public string id = "";      //the document part ID
        public string key = "";     //the document ID
        public T value;             //whatever we're looking for
    }
    public class LDBList<T>
    {
        public int total_rows = 0;
        public int offset = 0;
        public List<LDBListRow<T>> rows = new List<LDBListRow<T>>();
    }
}
