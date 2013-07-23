using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LCHARMS.Document
{
    public class ServiceResponse<T>
    {
        public T ResponseObject;
        public bool Error = false;
        public string Message = "";
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
