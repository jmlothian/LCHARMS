using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Windows.Forms;
using LCHARMS;
using LCHARMS.Client;
using LCHARMS.Config;
using LCHARMS.Document;
using LCHARMS.Identity;
using LCHARMS.Logging;
using LCHARMS.Security;

namespace LTestApp
{
    public partial class Form1 : Form
    {
        //host the document service so we can persist @#$ASDF between calls.
        string ServiceAddress = "127.0.0.1:8001/LDocHost";
        LServiceHost<LDocumentService, ILDocumentManager> DocumentService;



        public Form1()
        {
            InitializeComponent();            
            Console.WriteLine(LCHARMSConfig.GetSection().LRI + " " + LCHARMSConfig.GetSection().DBServer);

            //create the document service
            // service thread automatically starts on init.  We are basically done in the host.
            DocumentService = new LServiceHost<LDocumentService, ILDocumentManager>("net.tcp://" + ServiceAddress, "net.tcp://" + ServiceAddress, new NetTcpBinding());



        }

        private void button1_Click(object sender, EventArgs e)
        {
            FDebugLog.WriteLog("Starting Log - LTestApp");
            LConnectionManager ConnManager = new LConnectionManager();
            IDManager IDMgr = new IDManager(ConnManager);
            //net.sytes.rhalin/Sites/LCHARMS/IDProvider/~users/79c64cd6-ac48-473b-b306-916e4b90f6b4
            bool FirstUser = IDMgr.CreateCoreID("coreID", new LRI("net.sytes.rhalin/Sites/LCHARMS/IDProvider/"), "password", "pin");
            if (FirstUser)
            {
                IDInfo parentInfo = IDMgr.Sessions.Last().Value;
                Console.WriteLine(parentInfo.LRI);
                IDMgr.CreateChildID(new LRI(parentInfo.Session.Identity.UserLRI), "pin", "childID", new LRI("localhost:31939/LIdentityProvider.svc"), "password2", "pin2");
                IDInfo childInfo = IDMgr.Sessions.Last().Value;
                Console.WriteLine(childInfo.LRI);
                IDMgr.Logout(new LRI(childInfo.LRI));
                IDMgr.Logout(new LRI(parentInfo.LRI));
                IDMgr.Login(new LRI(childInfo.LRI), "password2");
                IDMgr.Login(new LRI(parentInfo.LRI), "password");                 
            }
            /*
            Console.WriteLine(ConnManager.GetIDConnection(new LRI("net.sytes.rhalin/Sites/LCHARMS/IDProvider/")).Ping());
            Console.WriteLine(ConnManager.GetIDConnection(new LRI("localhost:31939/LIdentityProvider.svc")).Ping());
            Console.WriteLine(ConnManager.GetIDConnection(new LRI("net.sytes.rhalin/Sites/LCHARMS/IDProvider/")).Ping());
            Console.WriteLine(ConnManager.GetIDConnection(new LRI("localhost:31939/LIdentityProvider.svc")).Ping());
            ILIdentityProvider idProvSindri = ConnManager.GetIDConnection(new LRI("net.sytes.rhalin/Sites/LCHARMS/IDProvider/"));
            ILIdentityProvider idProvLocal = ConnManager.GetIDConnection(new LRI("localhost:31939/LIdentityProvider.svc"));
            SessionInfo info = idProvSindri.CreateIdentity("", "", "", "newuser", "1234", "1234", "1234");
            SessionInfo info2 = idProvLocal.CreateIdentity("net.sytes.rhalin/Sites/LCHARMS/IDProvider/", "newuser", "1234", "neweruser", "1234", "1234", "1234");
             */
            /*
            LDocHeaderList obj = CouchDBMgr.GetAllDocIDs();
            LTestApp.LIDProvider.LIdentityProviderClient client = new LTestApp.LIDProvider.LIdentityProviderClient();
            LTestApp.LIDService2.LIdentityProviderClient client2 = new LTestApp.LIDService2.LIdentityProviderClient();
            LRI lri = client2.ParseLRI("net.sytes.rhalin/Sites/LCHARMS/IDProvider/~web/db1/db2", false);


            SessionInfo info = client.CreateIdentity("", "", "", "newuser", "1234", "1234", "1234");

            SessionInfo info2 = client.CreateIdentity("net.sytes.rhalin/Sites/LCHARMS/IDProvider/", "newuser", "1234", "neweruser", "1234", "1234", "1234");
            Console.WriteLine(info2.SessionKey); */
        }

        private void btnTestDocService_Click(object sender, EventArgs e)
        {
            LConnectionManager ConnManager = new LConnectionManager();
            ILDocumentManagerChannel docmanager = ConnManager.GetDocManagerConnection(new LRI(ServiceAddress, true));
            DocumentPartResponse rep = docmanager.GetDocPart(new LRI("net.sytes.rhalin/doc/#docID"), 0, 1);

            //ILDocumentManagerCh ILDocumentManagerChannel channel = LDocumentService.CreateServiceChannel(ServiceAddress);
            //DocumentPartResponse rep = channel.GetDocPart(new LRI("net.sytes.rhalin/doc/#docID"), 0, 1);
        }

        private void btnCreateDoc_Click(object sender, EventArgs e)
        {
            LDocumentManager docMan = new LDocumentManager();
            LIdentity id = AuthorizationManager.GetAuthIdentityFromLRI("net.sytes.rhalin/~users/00000000001");
            LDocumentHeader head = docMan.NewFile(id, "lcharms.lrt", "Newer File", new List<string>());
            //docMan.AddPermission(new LRI(head.DocumentLRI), new LRI(id.UserLRI), new LRI("net.sytes.rhalin/~users/22222222"), LDocACLPermission.WRITE);
            //docMan.AddPermission(new LRI(head.DocumentLRI), new LRI(id.UserLRI), new LRI("net.sytes.rhalin/~users/333READ"), LDocACLPermission.READ);
            //docMan.RevokePermission(new LRI(head.DocumentLRI), new LRI(id.UserLRI), new LRI("net.sytes.rhalin/~users/333READ"), LDocACLPermission.READ);
            //docMan.AddPermission(new LRI(head.DocumentLRI), new LRI(id.UserLRI), new LRI(""), LDocACLPermission.WRITE);
            docMan.AddTag(id, new LRI(head.DocumentLRI), "Bacon");
            docMan.AddTag(id, new LRI(head.DocumentLRI), "Rock");
            docMan.AddTag(id, new LRI(head.DocumentLRI), "Fire");
            docMan.RemoveTag(id, new LRI(head.DocumentLRI), "Rock");
            head.FileName = "changed filename";
            docMan.SaveDocument(new LRI(head.DocumentLRI),head, new List<LDocumentPart>());
        }
    }
}
