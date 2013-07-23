using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace LCHARMS.Document.DocumentTransferManager
{
    /*
     sequence
        Client--[request file]->clientservice--[request file]->dataservice--[request file]->docmanager
     --small files just transfered
     --medium files, written to disk and sent as a link
     --large files create a torrent
    dataservice->docmanager
     
     
     
     
     */
    [ServiceContract]
    public interface ILDocumentTransferManager
    {
        //request a specific part
        //version == -1 means "latest version"
        [OperationContract]
        DocumentPartResponse RequestDocumentPart(LRI lri, int Version = -1, int SequenceNumber = 0);
        //Request parts from Start to Count (inclusive).  -1 start means "get all parts" and count is ignored.
        //Implementations of ILDocumentTransferManager DO NOT have to honor the count.  StartingSequence is only honored responsetype=DATA_INCLUDED, otherwise complete file is implied
        //   The client is responsible for making sure it gets all of the parts.
        [OperationContract]
        DocumentPartResponse RequestDocumentParts(LRI lri, int Version = -1, int StartingSequenceNumber = -1, int Count = -1);

        [OperationContract]
        TransferUpdate GetStatus(string TransferTicket);

        [OperationContract]
        TransferUpdate CreateTorrent(LRI lri);

        void SetDocumentManager(ILDocumentManager DocManager);
    }
}
