using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using LCHARMS.Hierarchy;
using LCHARMS.Collection;

namespace LCHARMS.Document
{
    [KnownType(typeof(LDocumentHeader))]
    [KnownType(typeof(LHierarchyNode))]
    [KnownType(typeof(List<LHierarchyNode>))]
    [KnownType(typeof(List<string>))]
    [KnownType(typeof(LHierarchy))]
    [KnownType(typeof(LCollection))]
    [KnownType(typeof(List<LCollection>))]
    [KnownType(typeof(List<LDocumentVersionInfo>))]
    [KnownType(typeof(LDocumentVersionInfo))]
    [KnownType(typeof(DocumentPartResponse))]
    [DataContract]
    public class ServiceResponse<T>
    {
        [DataMember]
        public T ResponseObject;
        [DataMember]
        public bool Error = false;
        [DataMember]
        public string Message = "";
        [DataMember]
        public int ErrorCode = -1;
        public ServiceResponse(T obj)
        {
            ResponseObject = obj;
        }
        public ServiceResponse(bool InvalidCredentials = false)
        {
            if (InvalidCredentials)
            {
                Error = true;
                Message = "File does not exist or invalid Credentials to access that database or file.";
                ErrorCode = 1;
            }
        }
        public static ServiceResponse<T> InvalidCredentails()
        {
            ServiceResponse<T> rep = new ServiceResponse<T>(false);
            return rep;
        }
    }
}
