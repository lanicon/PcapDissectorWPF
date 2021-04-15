using NLog;
using PcapDissectorWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcapDissectorWPF.Transform
{
    public class FuturePreClose
    {
        Dictionary<string, decimal> dicPreClose = new Dictionary<string, decimal>();
        CommonLibrary.ILogger logger;

        public FuturePreClose(CommonLibrary.ILogger logger)
        {
            this.logger = logger;
        }


        /// <summary>
        /// 對應IP 225.0.100.100
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
                        int Len = ((bytes[offset + 14] & 0xf) + ((bytes[offset + 14] >> 4) & 0xF) * 10) * 100
                                + ((bytes[offset + 15] & 0xf) + ((bytes[offset + 15] >> 4) & 0xF) * 10) + 19;

                        int HH = ((bytes[offset + 3] & 0xf) + ((bytes[offset + 3] >> 4) & 0xF) * 10);
                        int mm = ((bytes[offset + 4] & 0xf) + ((bytes[offset + 4] >> 4) & 0xF) * 10);
                        int ss = ((bytes[offset + 5] & 0xf) + ((bytes[offset + 5] >> 4) & 0xF) * 10);
                        int time = HH * 10000 + mm * 100 + ss;

                        //if (time > 83000)
                        //    break;

                        byte[] buf = new byte[Len];

                        Array.Copy(bytes, offset, buf, 0, Len);

                        if (buf.Length > 36 && buf[1] == 49 && buf[2] == 49)
                        {
                            string Key = Encoding.ASCII.GetString(buf, 16, 10).Trim();

                            decimal DecimalLocator = Convert.ToDecimal(Math.Pow(10, buf[62] * -1));
                            decimal YesterdayPrice = Convert.ToDecimal(buf[31].ToString("X2") + buf[32].ToString("X2") + buf[33].ToString("X2") + buf[34].ToString("X2") + buf[35].ToString("X2")) * DecimalLocator;
                            if (dicPreClose.ContainsKey(Key) == false)
                            {
                                dicPreClose.Add(Key, YesterdayPrice);
                            }
                        }
                    }
                    else
                    {
                        //Console.WriteLine("解析錯誤");
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void Save(string date)
        {
            LogManager.Configuration.Variables["vardate"] = date;
            LogManager.Configuration.Variables["varmarket"] = "YesterdayPrice";
            var log2 = LogManager.GetLogger("Future");
            foreach (var pd in dicPreClose)
            {
                log2.Info(pd.Key + "," + pd.Value);
            }
        }
    }
}
