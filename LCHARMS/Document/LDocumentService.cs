using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LCHARMS.Collection;
using LCHARMS.Hierarchy;
using System.ServiceModel;

namespace LCHARMS.Document
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class LDocumentService : LDocumentManager, IDisposable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public static ILDocumentManagerChannel CreateServiceChannel(string ServiceAddress)
        {
            //proxy to the service
            EndpointAddress endPoint;
            // Creates the proper binding
            ChannelFactory<ILDocumentManagerChannel> channelFact;
            //binding type
            System.ServiceModel.Channels.Binding binding = new NetTcpBinding();

            //bind to the service internally?
            endPoint = new EndpointAddress(
                new Uri(ServiceAddress));
            channelFact = new ChannelFactory<ILDocumentManagerChannel>(binding, endPoint);
            channelFact.Open();

            var channel = channelFact.CreateChannel();
            channel.Open();

            return channel;
        }
    }

    //used to create a channel, doesn't need explicit implementation
    public interface ILDocumentManagerChannel : ILDocumentManager, IClientChannel
    {
    }

}
