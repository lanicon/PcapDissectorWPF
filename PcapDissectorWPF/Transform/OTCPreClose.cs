using NLog;
using PcapDissectorWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcapDissectorWPF.Transform
{
    public class OTCPreClose
    {
        Dictionary<string, decimal> dicPreClose = new Dictionary<string, decimal>();
        CommonLibrary.ILogger logger;

        public OTCPreClose(CommonLibrary.ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// 對應IP 224.0.30.30
        /// </summary>
        public void Read(List<PcpaData> pcpaDataList, int date)
        {
            try
            {
                foreach (var item in pcpaDataList)
                {
                    int offset = 0;
                    var bytes = item.Bytes;
                    var pcapTime = item.PcapTime;

                    if (bytes[offset] == 27)
                    {
                        int Len = Convert.ToInt32(bytes[offset + 1].ToString("X2") + bytes[offset + 2].ToString("X2"));

                        #region 0201
                        if (bytes[offset + 3] == 0x02 && bytes[offset + 4] == 0x01)
                        {
                            byte[] buf = new byte[Len];
                            Array.Copy(bytes, offset, buf, 0, Len);
                            string productID = Encoding.ASCII.GetString(buf, 10, 6).Trim(); //商品代號
                            decimal close = 0;
                            if (date >= 20200323)
                                close = Convert.ToDecimal(buf[40].ToString("X2") + buf[41].ToString("X2") + buf[42].ToString("X2") + "." + buf[43].ToString("X2") + buf[44].ToString("X2"));
                            else
                                close = Convert.ToDecimal(buf[40].ToString("X2") + buf[41].ToString("X2") + "." + buf[42].ToString("X2"));

                            if (dicPreClose.ContainsKey(productID) == false)
                            {
                                dicPreClose.Add(productID, close);
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        //Console.WriteLine("解析錯誤");
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Save(string date)
        {
            LogManager.Configuration.Variables["vardate"] = date;
            LogManager.Configuration.Variables["varmarket"] = "YesterdayPrice";
            var log2 = LogManager.GetLogger("OTC");

            foreach (var pd in dicPreClose)
            {
                log2.Info(pd.Key + "," + pd.Value);
            }
        }
    }
}
