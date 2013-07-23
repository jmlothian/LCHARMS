using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace LCHARMS.Config
{
    public class LCHARMSConfig: ConfigurationSection
    {
        [ConfigurationProperty("DBServer")]
        public string DBServer
        {
            get { return this["DBServer"] as string; }
        }
        [ConfigurationProperty("LRI")]
        public string LRI
        {
            get { return this["LRI"] as string; }
        }
        public static LCHARMSConfig GetSection()
        {
            return ConfigurationManager.GetSection("LCHARMS") as LCHARMSConfig;
        }
    }
}
