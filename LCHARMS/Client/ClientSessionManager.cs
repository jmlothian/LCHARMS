using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LCHARMS.Client
{
    //this class must be persistant.  It is expected to manage all clients and sessions
    public class ClientSessionManager
    {
        LConnectionManager ConnectionManager = new LConnectionManager();
        //reserves and returns a new session key for all future transmissions
        public string CreateSession()
        {
            return "";
        }

        public void DestroySession()
        {
        }
    }
}
