using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LCHARMS.Client;
using LCHARMS.Document;

namespace LCHARMS.UI.Workspace
{
    public class WorkspaceManager
    {
        ClientAccountManager ClientAcctManager;
        public WorkspaceManager(ClientAccountManager AcctMgr)
        {
            ClientAcctManager = AcctMgr;
        }
        public ServiceResponse<List<string>> GetWorkspaceList(string ClientSessionKey)
        {
            throw new NotImplementedException();
        }

    }
}
