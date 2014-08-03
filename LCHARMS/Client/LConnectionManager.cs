using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using LCHARMS.LIdentityProvider;
using LCHARMS.Logging;
using LCHARMS.Document;

namespace LCHARMS.Client
{
    public class LConnectionInfo
    {
        public enum BIND_TYPE {HTTP, TCP};
        public enum SERVICE_TYPE { PROV_ID, PROV_DOC, PROV_DATA};
        public BIND_TYPE BindType = BIND_TYPE.HTTP;
        public Binding ChannelBinding = null;
        public EndpointAddress EndpointAddr = null;
        public object Client = null;
        public void ConnClosed(Object sender, EventArgs e)
        {
            FDebugLog.WriteLog("[LConnectionManager][" + EndpointAddr.Uri + "]: Closed");
        }
        public void ConnFault(Object sender, EventArgs e)
        {
            FDebugLog.WriteLog("[LConnectionManager][" + EndpointAddr.Uri + "]: Fault");
        }
    }

    public class LConnectionManager
    {
        public T GetProvider<T>(LRI lri) where T : IClientChannel
        {
            if (Connections.ContainsKey(lri.ServiceLRI))
            {
                try
                {
                    Connections[lri.ToString()].Client =
                        ((ChannelFactory<T>)new ChannelFactory<T>(Connections[lri.ServiceLRI].ChannelBinding, Connections[lri.ServiceLRI].EndpointAddr)).CreateChannel();
                    return (T)Connections[lri.ServiceLRI].Client;
                }
                catch (Exception ex)
                {
                    FDebugLog.WriteLog("[LConnectionManager]: Network Fault Creating Channel " + ex.Message + " " + ex.StackTrace);
                }
                return default(T);
            }
            else
            {
                try
                {
                    LConnectionInfo info = new LConnectionInfo();
                    info.EndpointAddr = new EndpointAddress(new Uri("net.tcp://" + lri.ServiceURI),
                            EndpointIdentity.CreateDnsIdentity(lri.URIDomain));
                    info.ChannelBinding = new NetTcpBinding();
                    var myChannelFactory = new ChannelFactory<T>(info.ChannelBinding, info.EndpointAddr);
                    Connections[lri.ServiceLRI] = info;
                    Connections[lri.ServiceLRI].Client = myChannelFactory.CreateChannel();
                    myChannelFactory.Closed += new EventHandler(Connections[lri.ServiceLRI].ConnClosed);
                    myChannelFactory.Faulted += new EventHandler(Connections[lri.ServiceLRI].ConnFault);
                    return (T)Connections[lri.ServiceLRI].Client;
                }
                catch (Exception ex)
                {
                    FDebugLog.WriteLog("[LConnectionManager]: Network Fault Creating Channel " + ex.Message + " " + ex.StackTrace);
                }
                return default(T);
            }
        }

        public ILDocumentManagerChannel GetDocManagerConnection(LRI lri)
        {
            if (Connections.ContainsKey(lri.ToString()))
            {
                try
                {
                    Connections[lri.ToString()].Client =
                        ((ChannelFactory<ILDocumentManagerChannel>)new ChannelFactory<ILDocumentManagerChannel>(Connections[lri.ToString()].ChannelBinding, Connections[lri.ToString()].EndpointAddr)).CreateChannel();
                    return (ILDocumentManagerChannel)Connections[lri.ToString()].Client;
                }
                catch (Exception ex)
                {
                    FDebugLog.WriteLog("[LConnectionManager]: Network Fault Creating Channel " + ex.Message + " " + ex.StackTrace);
                }
                return null;
            }
            else
            {
                try
                {
                    LConnectionInfo info = new LConnectionInfo();
                    info.EndpointAddr = new EndpointAddress(new Uri("net.tcp://" + lri.URI),
                            EndpointIdentity.CreateDnsIdentity(lri.URIDomain));
                    info.ChannelBinding = new NetTcpBinding();
                    var myChannelFactory = new ChannelFactory<ILDocumentManagerChannel>(info.ChannelBinding, info.EndpointAddr);
                    Connections[lri.ToString()] = info;
                    Connections[lri.ToString()].Client = myChannelFactory.CreateChannel();
                    myChannelFactory.Closed += new EventHandler(Connections[lri.ToString()].ConnClosed);
                    myChannelFactory.Faulted += new EventHandler(Connections[lri.ToString()].ConnFault);
                    return (ILDocumentManagerChannel)Connections[lri.ToString()].Client;
                }
                catch (Exception ex)
                {
                    FDebugLog.WriteLog("[LConnectionManager]: Network Fault Creating Channel " + ex.Message + " " + ex.StackTrace);
                }
                return null;
            }
        }
        public ILIdentityProvider GetIDConnection(LRI lri)
        {
            //reconnect
            if (Connections.ContainsKey(lri.BaseURI))
            {
                try
                {
                    Connections[lri.BaseURI].Client =
                        ((ChannelFactory<ILIdentityProvider>)new ChannelFactory<ILIdentityProvider>(Connections[lri.BaseURI].ChannelBinding, Connections[lri.BaseURI].EndpointAddr)).CreateChannel();
                    return (ILIdentityProvider)Connections[lri.BaseURI].Client;
                }
                catch (Exception ex)
                {
                    FDebugLog.WriteLog("[LConnectionManager]: Network Fault Creating Channel " + ex.Message + " " + ex.StackTrace);
                }
                return null;
            }
            else
            {
                try
                {
                    LConnectionInfo info = new LConnectionInfo();
                    info.EndpointAddr = new EndpointAddress(new Uri("http://" + lri.URI.Replace("//","/")),
                            EndpointIdentity.CreateDnsIdentity(lri.URIDomain));
                    info.ChannelBinding = new BasicHttpBinding();
                    var myChannelFactory = new ChannelFactory<ILIdentityProvider>(info.ChannelBinding, info.EndpointAddr);
                    Connections[lri.BaseURI] = info;
                    Connections[lri.BaseURI].Client = myChannelFactory.CreateChannel();
                    myChannelFactory.Closed += new EventHandler(Connections[lri.BaseURI].ConnClosed);
                    myChannelFactory.Faulted += new EventHandler(Connections[lri.BaseURI].ConnFault);
                    return (ILIdentityProvider)Connections[lri.BaseURI].Client;
                }
                catch (Exception ex)
                {
                    FDebugLog.WriteLog("[LConnectionManager]: Network Fault Creating Channel " + ex.Message + " " + ex.StackTrace);
                }
                return null;
            }
        }

        private SortedDictionary<string, LConnectionInfo> Connections = new SortedDictionary<string, LConnectionInfo>();
    }
}
