using NLog;
using PcapDissectorWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcapDissectorWPF.Transform
{
    public class OTCWarrantTag
    {
        Dictionary<string, WarrantBase> dicWarrant = new Dictionary<string, WarrantBase>();
        Dictionary<string, string> dicName = new Dictionary<string, string>();
        Dictionary<string, string> diclog = new Dictionary<string, string>();
        CommonLibrary.ILogger logger;

        public OTCWarrantTag(CommonLibrary.ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        ///   224.2.30.30
        /// </summary>
        public void Read(List<PcpaData> pcpaDataList, string date)
        {
            

            OTCWarrantBase(pcpaDataList, dicWarrant, dicName);


            LogManager.Configuration.Variables["vardate"] = date;            
            LogManager.GetLogger(date).Info("PcapTime,warrantID,StockID,StrikePrice,Ratio,Expiration,Broker");

            foreach (var item in pcpaDataList)
            {
                int offset = 0;
                var bytes = item.Bytes;
                var pcapTime = item.PcapTime;

                if (bytes[offset] == 27)
                {
                    int Len = Convert.ToInt32(bytes[offset + 1].ToString("X2") + bytes[offset + 2].ToString("X2"));
                    if (bytes[offset + 3] == 0x02 && bytes[offset + 4] == 0x14)
                    {
                        byte[] buf = new byte[Len];
                        Array.Copy(bytes, offset, buf, 0, Len);
                        int bodyLength = Convert.ToInt32(buf[1].ToString("X2") + buf[2].ToString("X2")); //訊息長度
                        int informationSeq = Convert.ToInt32(buf[6].ToString("X2") + buf[7].ToString("X2") + buf[8].ToString("X2") + buf[9].ToString("X2")); //傳送訊息流水序號
                        string warrantID = Encoding.ASCII.GetString(buf, 10, 6).Trim(); //權證代號
                        string fullName = Encoding.Default.GetString(buf, 16, 50).Trim(); //認購（售）權證全稱

                        if (dicWarrant.ContainsKey(warrantID) && diclog.ContainsKey(warrantID) == false)
                        {
                            diclog.Add(warrantID, warrantID);
                            var pd = dicWarrant[warrantID];
                            string WarrantName = fullName.Split('－')[0];
                            int index = fullName.IndexOf("－") + 1;
                            int length = fullName.Length - index;
                            string s = fullName.Substring(index, length).Trim();
                            int index2 = s.IndexOf("20") - 1;
                            string StockName = s.Substring(0, s.IndexOf("20")).Trim();
                            string Broker = new Shared().GetBrokerName(WarrantName);

                            if (dicName.ContainsKey(StockName))
                            {
                                var StockID = dicName[StockName];
                                LogManager.GetLogger(date).Info("{0},{1},{2},{3},{4},{5},{6}", pcapTime, warrantID, StockID, pd.StrikePrice, pd.Ratio, pd.Expiration, Broker);
                            }
                        }
                    }
                }
                else
                {
                    //Console.WriteLine("解析錯誤");
                }
            }

        }


        private void OTCWarrantBase(List<PcpaData> pcpaDataList, Dictionary<string, WarrantBase> dicWarrant, Dictionary<string, string> dicName)
        {
            foreach (var item in pcpaDataList)
            {
                int offset = 0;
                var bytes = item.Bytes;
                var pcapTime = item.PcapTime;

                if (bytes[offset] == 27)
                {
                    int Len = Convert.ToInt32(bytes[offset + 1].ToString("X2") + bytes[offset + 2].ToString("X2"));
                    if (bytes[offset + 3] == 0x02 && bytes[offset + 4] == 0x01)
                    {
                        byte[] buf = new byte[Len];
                        Array.Copy(bytes, offset, buf, 0, Len);
                        #region 
                        var pd = new WarrantBase();
                        //判斷第一碼是否為 ASCII 27
                        int asciiNumber = Convert.ToInt16(buf[0]);
                        if (asciiNumber != 27)
                        {
                            throw new Exception("傳入資料錯誤，第一碼需為 ASCII 27");
                        }
                        //判斷是否為 0101 封包
                        string transmissionCode = buf[3].ToString("X2"); // 01
                        string messageKind = buf[4].ToString("X2"); // 01
                        int informationSeq = Convert.ToInt32(buf[6].ToString("X2") + buf[7].ToString("X2") + buf[8].ToString("X2") + buf[9].ToString("X2")); //傳送訊息流水序號

                        string stockNote = Encoding.ASCII.GetString(buf, 36, 2).Trim().ToUpper(); //股票筆數註記
                        switch (stockNote)
                        {
                            case "AL": //股票總數
                                int TSEStockCount = Convert.ToInt32(Encoding.ASCII.GetString(buf, 10, 6).Trim());
                                break;
                            case "NE": //今日新增總數
                                int TSENewCount = Convert.ToInt32(Encoding.ASCII.GetString(buf, 10, 6).Trim());

                                break;
                            case "": //一般股票
                                string productID = Encoding.ASCII.GetString(buf, 10, 6).Trim(); //商品代號
                                string productName = Encoding.Default.GetString(buf, 16, 16).Trim(); //商品簡稱

                                if (dicName.ContainsKey(productName) == false)
                                {
                                    dicName.Add(productName, productID);
                                }

                                string warrantFlag = Encoding.ASCII.GetString(buf, 64, 1).Trim(); //權證識別碼，紀錄值為 Y 時表示個股具有權證資料，SPACE時，權證資料欄位皆為0
                                                                                                  //    priceObject.warrantFlag = warrantFlag;
                                if (warrantFlag == "Y")
                                {
                                    pd.WarrantID = productID;

                                    pd.StrikePrice = buf[65].ToString("X2") + buf[66].ToString("X2") + buf[67].ToString("X2") + "." + buf[68].ToString("X2") + buf[69].ToString("X2");
                                    pd.Ratio = (Convert.ToDecimal(buf[85].ToString("X2") + buf[86].ToString("X2") + buf[87].ToString("X2") + "." + buf[88].ToString("X2")) / 1000).ToString(); //行使比率
                                    pd.Expiration = buf[99].ToString("X2") + buf[100].ToString("X2") + buf[101].ToString("X2") + buf[102].ToString("X2");//到期日
                                    if (dicWarrant.ContainsKey(pd.WarrantID) == false)
                                    {
                                        dicWarrant.Add(pd.WarrantID, pd);
                                    }
                                }
                                break;
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
    }
}
