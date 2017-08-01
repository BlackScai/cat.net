using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.Unidal.Cat.Configuration;
using Org.Unidal.Cat.Util;

namespace Com.Dianping.Cat.Configuration
{
    /*
     * Simplified CAT configuration, which load domain and server address from System Environment Variable.
     */
    public class SimplifiedClientConfig : AbstractClientConfig
    {
        String catServer;
        public SimplifiedClientConfig()
        {
            bool loadDomainFromEnv = false;

            String domain = Environment.GetEnvironmentVariable("CAT_DOMAIN");
            if (!String.IsNullOrWhiteSpace(domain))
            {
                domain = domain.Trim();
                loadDomainFromEnv = true;
                
            }
            else
            {
                domain = "Unknown";
            }

            this.Domain = new Domain { Id = domain, Enabled = true };
            Logger.Initialize(this.Domain.Id);

            if (loadDomainFromEnv)
            {
                Logger.Info("Cat client domain is set to [" + domain + "] by ENV environment variable. ");
            }
            else
            {
                Logger.Info("Cat client domain is set to [" + domain + "] by Default");
            }

            String catServer = Environment.GetEnvironmentVariable("CAT_SERVER");
            if (!String.IsNullOrWhiteSpace(catServer))
            {
                catServer = catServer.Trim();
                Logger.Info("Cat server is set to [" + catServer + "] by ENV environment variable. ");
            }
            else
            {
                catServer = "127.0.0.1:2281";
                Logger.Info("Cat server is set to [" + catServer + "] by Default. ");
            }

            this.catServer = catServer;

            this.MaxQueueSize = GetMaxQueueSize();
            this.MaxQueueByteSize = GetMaxQueueByteSize();
        }

        protected override string GetCatRouterServiceURL(bool sync)
        {
            return "http://" + catServer + "/cat/s/router";
        }

        private int GetMaxQueueSize()
        {
            try
            {
                var maxQueueSizeStr = System.Configuration.ConfigurationManager.AppSettings["Cat.MaxQueueSize"];
                if (!String.IsNullOrWhiteSpace(maxQueueSizeStr))
                {
                    var maxQueueSize = int.Parse(maxQueueSizeStr);
                    if (maxQueueSize >= 0)
                    {
                        return maxQueueSize;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return DEFAULT_MAX_QUEUE_SIZE;
        }

        private int GetMaxQueueByteSize()
        {
            try
            {
                var maxQueueByteSizeStr = System.Configuration.ConfigurationManager.AppSettings["Cat.MaxQueueByteSize"];
                if (!String.IsNullOrWhiteSpace(maxQueueByteSizeStr))
                {
                    var maxQueueByteSize = int.Parse(maxQueueByteSizeStr);
                    if (maxQueueByteSize >= 0)
                    {
                        return maxQueueByteSize;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex); 
            }
            return DEFAULT_MAX_QUEUE_BYTE_SIZE;
        }

        public override string GetConfigHeartbeatMessage()
        {
            return null;
        }
    }
}