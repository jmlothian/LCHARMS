using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Threading;
using System.ServiceModel.Channels;

namespace LCHARMS
{
    public class LServiceHost<TService,TContract>
    {
        const int SleepTime = 100;
        private ServiceHost serviceHost = null;
        private Thread myThread;
        private string ServiceAddress;
        private string EndpointAddress;
        private Binding binding;
        private bool Running = false;
        public LServiceHost(string ServiceAddress, string endpointAddress, Binding binding)
        {
            this.ServiceAddress = ServiceAddress;
            this.EndpointAddress = endpointAddress;
            this.binding = binding;
            myThread = new Thread(new ThreadStart(RunService));
            myThread.Start();
        }
        private void RunService()
        {
            try
            {
                Running = true;
                serviceHost = new ServiceHost(typeof(TService), new Uri(ServiceAddress));
                serviceHost.AddServiceEndpoint(typeof(TContract), binding, EndpointAddress);
                serviceHost.Open();
                while (Running)
                {
                    Thread.Sleep(SleepTime);
                }
                serviceHost.Close();
            }
            catch (Exception ex)
            {
                if (serviceHost != null)
                    serviceHost.Close();
            }
        }
        public void Stop()
        {
            lock (this)
            {
                Running = false;
            }
        }
    }
}
