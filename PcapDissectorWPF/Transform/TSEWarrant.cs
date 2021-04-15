using NLog;
using PcapDissectorWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcapDissectorWPF.Transform
{
    public class TSEWarrant
    {
        Dictionary<string, Product> dicp = new Dictionary<string, Product>();
        Dictionary<string, List<string>> dicData = new Dictionary<string, List<string>>();
        CommonLibrary.ILogger logger;

        public TSEWarrant(CommonLibrary.ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// 對應IP 224.2.100.100
        /// </summary>
        public void Read(List<PcpaData> pcpaDataList)
        {
            int LastSeq = 0;
            long lasttime = 0;

            new Shared().WarrantBatch(pcpaDataList, dicData, dicp, ref LastSeq, ref lasttime);
        }

        public void Save()
        {
            LogManager.Configuration.Variables["varmarket"] = "TSEWarrant";
            foreach (var pd in dicData)
            {
                var log = LogManager.GetLogger(pd.Key);
                foreach (var data in pd.Value)
                {
                    log.Info(data);
                }
            }
        }
    }
}
