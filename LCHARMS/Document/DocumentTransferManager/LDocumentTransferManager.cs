using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LCHARMS.Document.DocumentTransferManager
{
    //two classes, one sends from remote server, one recieves to local server
    // a client will "request" to the local server, which will ask the remote server for a file
    // if the request is local, it will send the appropriate data to the client and this should never be called
    public class LDocumentSendTransferManager : ILDocumentTransferManager
    {
        private ILDocumentManager DocumentManager = null;

        private int MaxInlineSize = 40; //kB

        public DocumentPartResponse RequestDocumentPart(LRI lri, int Version = -1, int SequenceNumber = 0)
        {
            LDocumentHeader head = DocumentManager.GetDocHeader(lri);
            

            throw new NotImplementedException();
        }

        public DocumentPartResponse RequestDocumentParts(LRI lri, int Version = -1, int StartingSequenceNumber = -1, int Count = -1)
        {
            throw new NotImplementedException();
        }

        public void SetDocumentManager(ILDocumentManager DocManager)
        {
            //wire up the doc manager, we don't want to mess with the DB ourselves
            //we should request whatever we need from the docmanager, so headers and whatnot load correctly there
            DocumentManager = DocManager;
        }


        public TransferUpdate GetStatus(string TransferTicket)
        {
            throw new NotImplementedException();
        }

        public TransferUpdate CreateTorrent(LRI lri)
        {
            throw new NotImplementedException();
        }
    }
    public class LDocumentReceiveTransferManager : ILDocumentTransferManager
    {

        public DocumentPartResponse RequestDocumentPart(LRI lri, int Version = -1, int SequenceNumber = 0)
        {
            throw new NotImplementedException();
        }

        public DocumentPartResponse RequestDocumentParts(LRI lri, int Version = -1, int StartingSequenceNumber = -1, int Count = -1)
        {
            throw new NotImplementedException();
        }

        public void SetDocumentManager(ILDocumentManager DocManager)
        {
            throw new NotImplementedException();
        }


        public TransferUpdate GetStatus(string TransferTicket)
        {
            throw new NotImplementedException();
        }

        public TransferUpdate CreateTorrent(LRI lri)
        {
            throw new NotImplementedException();
        }
    }
}
