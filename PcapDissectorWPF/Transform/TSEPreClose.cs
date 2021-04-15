using NLog;
using PcapDissectorWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcapDissectorWPF.Transform
{
    public class TSEPreClose
    {
        CommonLibrary.ILogger logger;
        Dictionary<string, decimal> dicPreClose = new Dictionary<string, decimal>();
        Dictionary<string, Product> dicp = new Dictionary<string, Product>();

        public TSEPreClose(CommonLibrary.ILogger logger)
        {
            this.logger = logger;
            LogManager.Configuration.Variables["varmarket"] = "YesterdayPrice";
        }

        /// <summary>
        /// 對應IP 224.0.100.100
        /// </summary>
        public void Read(List<PcpaData> pcpaDataList, int date)
        {            
            foreach (var item in pcpaDataList)
            {
                int offset = 0;
                var bytes = item.Bytes;
                var pcapTime = item.PcapTime;
                if (bytes[offset] == 27)
                {
                    int Len = ((bytes[offset + 1] & 0xf) + ((bytes[offset + 1] >> 4) & 0xF) * 10) * 100
                            + ((bytes[offset + 2] & 0xf) + ((bytes[offset + 2] >> 4) & 0xF) * 10);

                    if (bytes[offset + 3] == 0x01 && bytes[offset + 4] == 0x01)
                    {
                        byte[] buf = new byte[Len];
                        Array.Copy(bytes, offset, buf, 0, Len);
                        string productID = Encoding.ASCII.GetString(buf, 10, 6).Trim(); //商品代號
                        decimal close = 0;
                        if (date >= 20200323)
                            close = Convert.ToDecimal(buf[39].ToString("X2") + buf[40].ToString("X2") + buf[41].ToString("X2") + "." + buf[42].ToString("X2") + buf[43].ToString("X2"));
                        else
                            close = Convert.ToDecimal(buf[39].ToString("X2") + buf[40].ToString("X2") + "." + buf[41].ToString("X2"));
                        if (dicPreClose.ContainsKey(productID) == false)
                        {
                            dicPreClose.Add(productID, close);
                        }
                    }
                    else if (bytes[offset + 3] == 0x01 && bytes[offset + 4] == 0x03)
                    {
                        #region 0103

                        byte[] buf = new byte[Len];
                        Array.Copy(bytes, offset, buf, 0, Len);
                        string dataTime = buf[10].ToString("X2") + buf[11].ToString("X2") + buf[12].ToString("X2") + "000000";
                        if (dataTime == "000000000000")
                        {
                            decimal TSEA = Convert.ToDecimal(buf[14].ToString("X2") + buf[15].ToString("X2") + buf[16].ToString("X2") + "." + buf[17].ToString("X2"));
                            decimal TEIDX = Convert.ToDecimal(buf[14 + 104].ToString("X2") + buf[15 + 104].ToString("X2") + buf[16 + 104].ToString("X2") + "." + buf[17 + 104].ToString("X2"));
                            decimal TFIDX = Convert.ToDecimal(buf[14 + 120].ToString("X2") + buf[15 + 120].ToString("X2") + buf[16 + 120].ToString("X2") + "." + buf[17 + 120].ToString("X2"));
                            decimal TWXI = Convert.ToDecimal(buf[14 + 132].ToString("X2") + buf[15 + 132].ToString("X2") + buf[16 + 132].ToString("X2") + "." + buf[17 + 132].ToString("X2"));

                            if (dicp.ContainsKey("TSEA") == false)
                            {
                                LogManager.GetLogger("TSEA").Info("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}"
                                             , pcapTime, TSEA, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "1", 0, dataTime, dataTime);

                                dicp.Add("TSEA", new Product());
                            }

                            if (dicp.ContainsKey("TEIDX") == false)
                            {
                                LogManager.GetLogger("TEIDX").Info("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}"
                                             , pcapTime, TEIDX, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "1", 0, dataTime, dataTime);

                                dicp.Add("TEIDX", new Product());
                            }

                            if (dicp.ContainsKey("TFIDX") == false)
                            {
                                LogManager.GetLogger("TFIDX").Info("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}"
                                             , pcapTime, TFIDX, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "1", 0, dataTime, dataTime);

                                dicp.Add("TFIDX", new Product());
                            }

                            if (dicp.ContainsKey("TWXI") == false)
                            {
                                LogManager.GetLogger("TWXI").Info("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}"
                                             , pcapTime, TWXI, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "1", 0, dataTime, dataTime);

                                dicp.Add("TWXI", new Product());
                            }
                        }
                        #endregion
                    }
                }
                else
                {
                    //Console.WriteLine("解析錯誤");
                }
            }
        }

        public void Save(string date)
        {
            LogManager.Configuration.Variables["vardate"] = date;

            var log2 = LogManager.GetLogger("TSE");
            foreach (var pd in dicPreClose)
            {
                log2.Info(pd.Key + "," + pd.Value);
            }
        }
    }
}
