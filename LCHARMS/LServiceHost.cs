using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Threading;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

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
        private Mutex CheckRun = new Mutex();
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
                serviceHost = new ServiceHost(typeof(TService), new Uri(ServiceAddress));
                serviceHost.AddServiceEndpoint(typeof(TContract), binding, EndpointAddress);

                //publish metadata
                var smb = serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
                if (smb == null)
                    smb = new ServiceMetadataBehavior();

                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                serviceHost.Description.Behaviors.Add(smb);

                serviceHost.AddServiceEndpoint(
                    ServiceMetadataBehavior.MexContractName,
                    MetadataExchangeBindings.CreateMexTcpBinding(),
                    "mex"
                );


                serviceHost.Open();
                lock (CheckRun)
                {
                    Running = true; 
                }
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
        public bool IsRunning()
        {
            bool run = false;
            lock (CheckRun)
            {
                run = Running;
            }
            return run;
        }
        public void WaitForRunning()
        {
            while (!IsRunning())
            {
                Thread.Sleep(50);
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
