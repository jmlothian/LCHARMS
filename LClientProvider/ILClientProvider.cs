using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using LCHARMS.Document;
using LCHARMS.LIdentityProvider;
using LCHARMS;
using LCHARMS.Identity;
using LCHARMS.Session;
using LCHARMS.Collection;
using LCHARMS.Hierarchy;

namespace LClientProvider
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface ILClientProvider : ILDataProvider, ILIdentityProvider
    {

        // TODO: Add your service operations here
    }



}
