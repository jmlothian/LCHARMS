using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LCHARMS.Authentication;
using LCHARMS.Document;
using LCHARMS.DB.CouchDB;
using LCHARMS.UI.Workspace;
using Newtonsoft.Json;
using LCHARMS.Config;
using LCHARMS.Security;
using LCHARMS.Identity;
using LCHARMS.LIdentityProvider;

namespace LCHARMS.Client
{
    //handles client accounts, loading, saving, etc.
    public class ClientAccountManager
    {
        Dictionary<LRI, ClientAccount> ClientAccountLookup = new Dictionary<LRI, ClientAccount>();
        Dictionary<string, ClientAccount> ClientAccountLookupByAcctID = new Dictionary<string, ClientAccount>();
        Dictionary<string, ClientAccount> ClientAccountLookupBySessionKey = new Dictionary<string, ClientAccount>();
        List<ClientAccount> ClientAccounts = new List<ClientAccount>();
        List<string> SessionKeys = new List<string>();
        List<string> AccountIDs = new List<string>();
        IDManager IDMgr;
        LDocumentManager DocManager;
        public ClientAccountManager(IDManager IDMgr, LDocumentManager DocMgr)
        {
            this.IDMgr = IDMgr;
            DocManager = DocMgr;
            LoadAccounts();
        }
        private string CreateNewClientSessionKey()
        {
            
            string guidStr = "";
            lock (SessionKeys)
            {
                while (guidStr == "" || SessionKeys.Contains(guidStr))
                {
                    Guid guid = Guid.NewGuid();
                    guidStr = guid.ToString();
                }
                SessionKeys.Add(guidStr);
            }
            return guidStr;
        }
        public void LoadAccounts()
        {
            LDBList<ClientAccount> accts = CouchDBMgr.GetClientAccounts();
            foreach (LDBListRow<ClientAccount> row in accts.rows)
            {
                row.decoded = false;
                ClientAccount acct = row.decodedValue;
                acct.AccountHeader = DocManager.GetDocHeader(acct.AccountLRI);
                ClientAccounts.Add(row.decodedValue);
                AccountIDs.Add(row.value._id);
                ClientAccountLookupByAcctID[row.value._id] = acct;
                foreach (UserInfo info in row.decodedValue.Identities)
                {
                    //wire-up LRI lookups
                    LRI UserLRI = new LRI(info.Identity.UserLRI);
                    //lri->user
                    acct.IdentitiesByLRI[UserLRI] = info;
                    //lri->account
                    ClientAccountLookup[UserLRI] = acct;
                }
                //wire up LUI data headers
                foreach (LWorkspace ws in acct.Workspaces)
                {
                    foreach (LUICollection col in ws.OpenCollections)
                    {
                        col.DocumentHeader = DocManager.GetDocHeader(col.DocumentLRI);
                    }
                    foreach (LUIDocument doc in ws.OpenDocuments)
                    {
                        doc.DocumentHeader = DocManager.GetDocHeader(doc.DocumentLRI);
                    }
                    foreach (LUIHierarchy hier in ws.OpenHierarchies)
                    {
                        hier.DocumentHeader = DocManager.GetDocHeader(hier.DocumentLRI);
                    }
                }
            }
        }
        //save all accounts to the DB
        public void SaveAccounts()
        {
            foreach (ClientAccount Acct in ClientAccounts)
            {
                SaveAccount(Acct);
            }
        }
        //save a single account to the DB
        public void SaveAccount(LRI lri)
        {
            if (ClientAccountLookup.ContainsKey(lri))
            {
                SaveAccount(ClientAccountLookup[lri]);
            }
        }
        public void SaveAccount(ClientAccount Acct)
        {
            LDocumentHeader head = Acct.AccountHeader;
            
            Acct.AccountHeader = null; //we don't want this extra data saved here
            LDocumentPart part = new LDocumentPart();
            part.SequenceNumber = 0;
            part.DocType = DocumentType.CLIENTACCOUNT;
            part._id = Acct._id;
            part.DocumentID = Acct._id;
            List<LDocumentPart> parts = new List<LDocumentPart>();
            part.Data = DocManager.SerializeToJSONByteArray<ClientAccount>(Acct);
            parts.Add(part);
            DocManager.SaveDocument(new LRI(head.DocumentLRI), head, parts);
            //reattach head
            Acct.AccountHeader = head;
        }
        //add an identity to the following account ID
        public void AddIdentityToAccount(string ID, UserInfo IdentityToAdd, LRI UserLRI=null)
        {
            if (ClientAccountLookupByAcctID.ContainsKey(ID))
            {
                ClientAccountLookupByAcctID[ID].Identities.Add(IdentityToAdd);
                if(UserLRI == null)
                    UserLRI = new LRI(IdentityToAdd.Identity.UserLRI);
                ClientAccountLookupByAcctID[ID].IdentitiesByLRI[UserLRI] = IdentityToAdd;
                ClientAccountLookup[UserLRI] = ClientAccountLookupByAcctID[ID];
            }
        }

        //login the ID and other IDs associated with the account.
        public ServiceResponse<string> LoginID(LRI userLRI, string passwordHash, bool LoginAll = true)
        {
            //get account that matches
            if (ClientAccountLookup.ContainsKey(userLRI))
            {
                //login ID
                if(IDMgr.LoginWithHash(userLRI,passwordHash))
                {
                    //populate ServiceCredentials
                    ServiceCredentials creds = 
                        new ServiceCredentials(userLRI.ToString(),IDMgr.Sessions[userLRI.ToString()].Session.SessionKey );
                    //get acct
                    ClientAccount acct = ClientAccountLookup[userLRI];
                    //if this is the first login for this account, create a SessionKey
                    if (acct.ClientSessionKey == "")
                    {
                        acct.ClientSessionKey = Guid.NewGuid().ToString();
                    }
                    if (LoginAll)
                    {
                        //if other accounts not logged in, log them in? (LoginAll)
                    }
                    //return session key in the service response
                    ServiceResponse<string> resp = new ServiceResponse<string>();
                    resp.ResponseObject = acct.ClientSessionKey;
                    resp.Message = "OK";
                    return resp;
                } else
                {
                    return new ServiceResponse<string>(true);
                }
            }
            else
            {
                return new ServiceResponse<string>(true);
            }
        }
        public ServiceResponse<string> RegisterNewAccount(string ServiceLRI, string DomainLRI, string Username, string passwordHash)
        {
            //get LRI from domain / username / hash
            LRI UserLRI = IDMgr.GetUserLRI(new LRI(ServiceLRI), DomainLRI, Username, passwordHash);
            if (UserLRI == null)
            {
                return new ServiceResponse<string>(true);
            }
            else
            {
                if (ClientAccountLookup.ContainsKey(UserLRI))
                {
                    ServiceResponse<string> Resp = new ServiceResponse<string>();
                    Resp.Error = true;
                    Resp.ErrorCode = 2;
                    Resp.Message = "A user with that LRI is already registered with this system.";
                    Resp.ResponseObject = "";
                    return Resp;
                }
                else
                {
                    //we need this info
                    UserInfo info = new UserInfo();//UserManager.Identities[UserLRI.LRIString];
                    info.passwordHash = passwordHash;
                    info.Identity = IDMgr.GetUserLIdentity(new LRI(ServiceLRI), DomainLRI, Username, passwordHash);

                    //create new account and add this LRI info
                    ClientAccount Acct = new ClientAccount();
                    Acct._id = LDocumentManager.RequestGUID();
                    Acct.AccountLRI = new LRI(LCHARMSConfig.GetSection().LRI + "/" + Acct._id);
                    ClientAccountLookupByAcctID[Acct._id] = Acct;
                    AddIdentityToAccount(Acct._id, info, UserLRI);
                    //ServiceCredentials sc = new ServiceCredentials();
                    //Acct.ServiceCredentialsByLRI[userlri] = 

                    //create a header for the account
                    string ID = LDocumentManager.RequestGUID();
                    LDocumentHeader NewFileHeader = new LDocumentHeader();
                    LRI hlri = new LRI(LCHARMSConfig.GetSection().LRI + "/" + ID);
                    NewFileHeader.DocType = DocumentType.DOC_HEADER;
                    NewFileHeader.DocumentID = ID;
                    NewFileHeader.FQDT = "lcharms.client.account";
                    NewFileHeader.FileName = Username.ToLower() + ".client.account";
                    NewFileHeader.DocumentLRI = hlri.ToString();
                    NewFileHeader.IsCopy = false;
                    NewFileHeader.LastAccessDate = DateTime.Now;
                    NewFileHeader.DataLength = 0;

                    //create an ACL for this new file
                    // assign it to the creation user

                    DocManager.AuthManager.CreateACE(ID, info.Identity, LDocACLPermission.GRANT |
                                        LDocACLPermission.WRITE |
                                        LDocACLPermission.READ |
                                        LDocACLPermission.ACCESS_NEXT_VERSION |
                                        LDocACLPermission.ACCESS_PREV_VERSION);
                    DocManager.AuthManager.CreateACE(ID, DocManager.AuthManager.PublicIdentity, LDocACLPermission.DENY);

                    Acct.AccountHeader = NewFileHeader;
                    SaveAccount(Acct);

                    return LoginID(UserLRI, passwordHash,false);
                }                
            }
        }


        //--------should not be public via service----------
        public ClientAccount GetAccount(string ClientSessionKey)
        {
            throw new NotImplementedException();
        }
    }
}
