using NLog;
using TaifexLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcapDissectorWPF.Models;

namespace PcapDissectorWPF
{
    public class Transform2
    {        
        public delegate void MessageEventHandler(object sender, string message);
        public event MessageEventHandler MessageEvent;

        public delegate void ErrorEventHandler(object sender, string message);
        public event ErrorEventHandler ErrorEvent;


        public Transform2()
        {
            
        }

        /// <summary>
        /// 對應IP 224.0.100.100
        /// </summary>
        public void TSE(List<PcpaData> pcpaDataList)
        {
            Dictionary<string, Product> dicp = new Dictionary<string, Product>();
            Dictionary<string, List<string>> dicData = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> dicPreClose = new Dictionary<string, List<string>>();
            
            try
            {
                foreach (var item in pcpaDataList)
                {
                    int offset = 0;
                    bool indexflag = true;
                    int LastSeq = 0;
                    long lasttime = 0;
                    var bytes = item.Bytes;
                    var pcapTime = item.PcapTime;

                    if (bytes[offset] == 27 && offset + 4 < bytes.Length)
                    {
                        int Len = ((bytes[offset + 1] & 0xf) + ((bytes[offset + 1] >> 4) & 0xF) * 10) * 100
                                + ((bytes[offset + 2] & 0xf) + ((bytes[offset + 2] >> 4) & 0xF) * 10);

                        if (bytes[offset + 3] == 0x01 && bytes[offset + 4] == 0x06)
                        {
                            #region 0106

                            int seq = ((bytes[offset + 6] & 0xf) + ((bytes[offset + 6] >> 4) & 0xF) * 10) * 1000000
                                     + ((bytes[offset + 7] & 0xf) + ((bytes[offset + 7] >> 4) & 0xF) * 10) * 10000
                                     + ((bytes[offset + 8] & 0xf) + ((bytes[offset + 8] >> 4) & 0xF) * 10) * 100
                                     + ((bytes[offset + 9] & 0xf) + ((bytes[offset + 9] >> 4) & 0xF) * 10);

                            if (LastSeq == 0)
                            {
                                LastSeq = seq;
                            }
                            else
                            {
                                LastSeq = seq;
                            }

                            byte[] buf = new byte[Len];
                            //int seq = Convert.ToInt32(bytes[offset + 6].ToString("X2") + bytes[offset + 7].ToString("X2") + bytes[offset + 8].ToString("X2") + bytes[offset + 9].ToString("X2"));
                            Array.Copy(bytes, offset, buf, 0, Len);
                            string stockID = Encoding.ASCII.GetString(buf, 10, 6).Trim(); //股票代號
                            if (stockID == "8436")
                            {

                            }
                            if (stockID != "000000")
                            {
                                if (dicp.ContainsKey(stockID) == false)
                                {
                                    List<string> list = new List<string>();
                                    list.Add(string.Format("PcapTime,Last,Vol,BID1,BIDSZ1,BID2,BIDSZ2,BID3,BIDSZ3,BID4,BIDSZ4,BID5,BIDSZ5,ASK1,ASKSZ1,ASK2,ASKSZ2,ASK3,ASKSZ3,ASK4,ASKSZ4,ASK5,ASKSZ5,Tick,Volume,Time,LastTime"));
                                    dicData.Add(stockID, list);
                                    dicp.Add(stockID, new Product());
                                }

                                var product = dicp[stockID];

                                string dataTime = bytes[offset + 16].ToString("X2") + bytes[offset + 17].ToString("X2") + bytes[offset + 18].ToString("X2") + bytes[offset + 19].ToString("X2") + bytes[offset + 20].ToString("X2") + bytes[offset + 21].ToString("X2");

                                if (lasttime < Convert.ToInt64(dataTime))
                                {
                                    lasttime = Convert.ToInt64(dataTime);
                                }
                                else if (lasttime > Convert.ToInt64(dataTime) + 10000000000)
                                {
                                    break;
                                }

                                int bit0 = ((bytes[offset + 22] & 1) == 0) ? 0 : 1;
                                int bit1 = ((bytes[offset + 22] & 2) == 0) ? 0 : 1;
                                int bit2 = ((bytes[offset + 22] & 4) == 0) ? 0 : 1;
                                int bit3 = ((bytes[offset + 22] & 8) == 0) ? 0 : 1;
                                int bit4 = ((bytes[offset + 22] & 16) == 0) ? 0 : 1;
                                int bit5 = ((bytes[offset + 22] & 32) == 0) ? 0 : 1;
                                int bit6 = ((bytes[offset + 22] & 64) == 0) ? 0 : 1;
                                int bit7 = ((bytes[offset + 22] & 128) == 0) ? 0 : 1;

                                int buyCount = 0;
                                int sellCount = 0;
                                int dealCount = 0;

                                if (bit4 == 1)
                                {
                                    buyCount = 1;
                                }
                                if (bit5 == 1)
                                {
                                    buyCount = buyCount + 2;
                                }
                                if (bit6 == 1)
                                {
                                    buyCount = buyCount + 4;
                                }

                                if (bit1 == 1)
                                {
                                    sellCount = 1;
                                }
                                if (bit2 == 1)
                                {
                                    sellCount = sellCount + 2;
                                }
                                if (bit3 == 1)
                                {
                                    sellCount = sellCount + 4;
                                }

                                if (bit7 == 1)
                                {
                                    dealCount = 1;
                                }
                                else
                                {
                                    dealCount = 0;
                                }

                                int totalCount = dealCount + buyCount + sellCount;

                                int Volume = Convert.ToInt32(bytes[offset + 25].ToString("X2") + bytes[offset + 26].ToString("X2") + bytes[offset + 27].ToString("X2") + bytes[offset + 28].ToString("X2"));

                                decimal LastPrice = 0;
                                int LastQty = 0;
                                decimal BidPrice1 = 0;
                                decimal BidPrice2 = 0;
                                decimal BidPrice3 = 0;
                                decimal BidPrice4 = 0;
                                decimal BidPrice5 = 0;
                                int BidQty1 = 0;
                                int BidQty2 = 0;
                                int BidQty3 = 0;
                                int BidQty4 = 0;
                                int BidQty5 = 0;
                                decimal AskPrice1 = 0;
                                decimal AskPrice2 = 0;
                                decimal AskPrice3 = 0;
                                decimal AskPrice4 = 0;
                                decimal AskPrice5 = 0;
                                int AskQty1 = 0;
                                int AskQty2 = 0;
                                int AskQty3 = 0;
                                int AskQty4 = 0;
                                int AskQty5 = 0;

                                for (int i = 0; i < totalCount; i++)
                                {
                                    decimal price = (((bytes[29 + offset + i * 9] & 0xf) + ((bytes[29 + offset + i * 9] >> 4) & 0xF) * 10) * 100000000 +
                                                     ((bytes[30 + offset + i * 9] & 0xf) + ((bytes[30 + offset + i * 9] >> 4) & 0xF) * 10) * 1000000 +
                                                     ((bytes[31 + offset + i * 9] & 0xf) + ((bytes[31 + offset + i * 9] >> 4) & 0xF) * 10) * 10000 +
                                                     ((bytes[32 + offset + i * 9] & 0xf) + ((bytes[32 + offset + i * 9] >> 4) & 0xF) * 10) * 100 +
                                                     ((bytes[33 + offset + i * 9] & 0xf) + ((bytes[33 + offset + i * 9] >> 4) & 0xF) * 10)) * 0.0001m;

                                    int qty = ((bytes[34 + offset + i * 9] & 0xf) + ((bytes[34 + offset + i * 9] >> 4) & 0xF) * 10) * 1000000 +
                                              ((bytes[35 + offset + i * 9] & 0xf) + ((bytes[35 + offset + i * 9] >> 4) & 0xF) * 10) * 10000 +
                                              ((bytes[36 + offset + i * 9] & 0xf) + ((bytes[36 + offset + i * 9] >> 4) & 0xF) * 10) * 100 +
                                              ((bytes[37 + offset + i * 9] & 0xf) + ((bytes[37 + offset + i * 9] >> 4) & 0xF) * 10);

                                    if (i < dealCount)
                                    {
                                        LastPrice = price;
                                        LastQty = qty;
                                        product.Last = price;
                                        product.LastQty = qty;
                                    }
                                    else
                                    {
                                        if (i >= dealCount && i < (dealCount + buyCount))
                                        {
                                            switch (i - dealCount)
                                            {
                                                case 0:
                                                    BidPrice1 = price;
                                                    BidQty1 = qty;
                                                    break;
                                                case 1:
                                                    BidPrice2 = price;
                                                    BidQty2 = qty;
                                                    break;
                                                case 2:
                                                    BidPrice3 = price;
                                                    BidQty3 = qty;
                                                    break;
                                                case 3:
                                                    BidPrice4 = price;
                                                    BidQty4 = qty;
                                                    break;
                                                case 4:
                                                    BidPrice5 = price;
                                                    BidQty5 = qty;
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            switch (i - dealCount - buyCount)
                                            {
                                                case 0:
                                                    AskPrice1 = price;
                                                    AskQty1 = qty;
                                                    break;
                                                case 1:
                                                    AskPrice2 = price;
                                                    AskQty2 = qty;
                                                    break;
                                                case 2:
                                                    AskPrice3 = price;
                                                    AskQty3 = qty;
                                                    break;
                                                case 3:
                                                    AskPrice4 = price;
                                                    AskQty4 = qty;
                                                    break;
                                                case 4:
                                                    AskPrice5 = price;
                                                    AskQty5 = qty;
                                                    break;
                                            }
                                        }
                                    }
                                }

                                product.BID1 = BidPrice1;
                                product.BID2 = BidPrice2;
                                product.BID3 = BidPrice3;
                                product.BID4 = BidPrice4;
                                product.BID5 = BidPrice5;
                                product.BIDSiz1 = BidQty1;
                                product.BIDSiz2 = BidQty2;
                                product.BIDSiz3 = BidQty3;
                                product.BIDSiz4 = BidQty4;
                                product.BIDSiz5 = BidQty5;

                                product.ASK1 = AskPrice1;
                                product.ASK2 = AskPrice2;
                                product.ASK3 = AskPrice3;
                                product.ASK4 = AskPrice4;
                                product.ASK5 = AskPrice5;
                                product.ASKSiz1 = AskQty1;
                                product.ASKSiz2 = AskQty2;
                                product.ASKSiz3 = AskQty3;
                                product.ASKSiz4 = AskQty4;
                                product.ASKSiz5 = AskQty5;

                                if (LastQty > 0)
                                {
                                    dicData[stockID].Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27}"
                                          , pcapTime, product.Last, product.LastQty
                                          , product.BID1, product.BIDSiz1
                                          , product.BID2, product.BIDSiz2
                                          , product.BID3, product.BIDSiz3
                                          , product.BID4, product.BIDSiz4
                                          , product.BID5, product.BIDSiz5
                                          , product.ASK1, product.ASKSiz1
                                          , product.ASK2, product.ASKSiz2
                                          , product.ASK3, product.ASKSiz3
                                          , product.ASK4, product.ASKSiz4
                                          , product.ASK5, product.ASKSiz5
                                          , "1"
                                          , Volume
                                          , dataTime, dataTime
                                          , seq));
                                }
                                else
                                {
                                    dicData[stockID].Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27}"
                                          , pcapTime, product.Last, product.LastQty
                                          , product.BID1, product.BIDSiz1
                                          , product.BID2, product.BIDSiz2
                                          , product.BID3, product.BIDSiz3
                                          , product.BID4, product.BIDSiz4
                                          , product.BID5, product.BIDSiz5
                                          , product.ASK1, product.ASKSiz1
                                          , product.ASK2, product.ASKSiz2
                                          , product.ASK3, product.ASKSiz3
                                          , product.ASK4, product.ASKSiz4
                                          , product.ASK5, product.ASKSiz5
                                          , "0"
                                          , Volume
                                          , dataTime, ""
                                          , seq));
                                }
                            }
                            #endregion
                        }
                        else if (bytes[offset + 3] == 0x01 && bytes[offset + 4] == 0x03)
                        {
                            #region 0103

                            byte[] buf = new byte[Len];
                            Array.Copy(bytes, offset, buf, 0, Len);
                            string dataTime = buf[10].ToString("X2") + buf[11].ToString("X2") + buf[12].ToString("X2") + "000000";

                            if (dataTime == "133000000000")
                            {

                            }
                            else if (dataTime != "000000000000" && indexflag && Convert.ToInt64(dataTime) > 90000000000)
                            {
                                if (dataTime == "999999000000")
                                {
                                    dataTime = "133000000000";
                                    indexflag = false;
                                }

                                decimal TSEA = Convert.ToDecimal(buf[14].ToString("X2") + buf[15].ToString("X2") + buf[16].ToString("X2") + "." + buf[17].ToString("X2"));
                                decimal TEIDX = Convert.ToDecimal(buf[14 + 104].ToString("X2") + buf[15 + 104].ToString("X2") + buf[16 + 104].ToString("X2") + "." + buf[17 + 104].ToString("X2"));
                                decimal TFIDX = Convert.ToDecimal(buf[14 + 120].ToString("X2") + buf[15 + 120].ToString("X2") + buf[16 + 120].ToString("X2") + "." + buf[17 + 120].ToString("X2"));
                                decimal TWXI = Convert.ToDecimal(buf[14 + 132].ToString("X2") + buf[15 + 132].ToString("X2") + buf[16 + 132].ToString("X2") + "." + buf[17 + 132].ToString("X2"));
                                if (dicp.ContainsKey("TSEA") == false)
                                {
                                    LogManager.GetLogger("TSEA").Info("PcapTime,Last,Vol,BID1,BIDSZ1,BID2,BIDSZ2,BID3,BIDSZ3,BID4,BIDSZ4,BID5,BIDSZ5,ASK1,ASKSZ1,ASK2,ASKSZ2,ASK3,ASKSZ3,ASK4,ASKSZ4,ASK5,ASKSZ5,Tick,Volume,Time,LastTime");
                                    dicp.Add("TSEA", new Product());
                                }
                                if (dicp.ContainsKey("TEIDX") == false)
                                {
                                    LogManager.GetLogger("TEIDX").Info("PcapTime,Last,Vol,BID1,BIDSZ1,BID2,BIDSZ2,BID3,BIDSZ3,BID4,BIDSZ4,BID5,BIDSZ5,ASK1,ASKSZ1,ASK2,ASKSZ2,ASK3,ASKSZ3,ASK4,ASKSZ4,ASK5,ASKSZ5,Tick,Volume,Time,LastTime");
                                    dicp.Add("TEIDX", new Product());
                                }
                                if (dicp.ContainsKey("TFIDX") == false)
                                {
                                    LogManager.GetLogger("TFIDX").Info("PcapTime,Last,Vol,BID1,BIDSZ1,BID2,BIDSZ2,BID3,BIDSZ3,BID4,BIDSZ4,BID5,BIDSZ5,ASK1,ASKSZ1,ASK2,ASKSZ2,ASK3,ASKSZ3,ASK4,ASKSZ4,ASK5,ASKSZ5,Tick,Volume,Time,LastTime");
                                    dicp.Add("TFIDX", new Product());
                                }
                                if (dicp.ContainsKey("TWXI") == false)
                                {
                                    LogManager.GetLogger("TWXI").Info("PcapTime,Last,Vol,BID1,BIDSZ1,BID2,BIDSZ2,BID3,BIDSZ3,BID4,BIDSZ4,BID5,BIDSZ5,ASK1,ASKSZ1,ASK2,ASKSZ2,ASK3,ASKSZ3,ASK4,ASKSZ4,ASK5,ASKSZ5,Tick,Volume,Time,LastTime");
                                    dicp.Add("TWXI", new Product());
                                }
                                LogManager.GetLogger("TSEA").Info("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}"
                                             , pcapTime, TSEA, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "1", 0, dataTime, dataTime);

                                LogManager.GetLogger("TEIDX").Info("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}"
                                             , pcapTime, TEIDX, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "1", 0, dataTime, dataTime);

                                LogManager.GetLogger("TFIDX").Info("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}"
                                             , pcapTime, TFIDX, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "1", 0, dataTime, dataTime);

                                LogManager.GetLogger("TWXI").Info("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}"
                                             , pcapTime, TWXI, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "1", 0, dataTime, dataTime);
                            }
                            #endregion
                        }
                        else if (bytes[offset + 3] == 0x01 && bytes[offset + 4] == 0x22)
                        {

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


            foreach (var pd in dicData)
            {
                var log = LogManager.GetLogger(pd.Key);
                foreach (var data in pd.Value)
                {
                    log.Info(data);
                }
            }
        }

        /// <summary>
        /// 對應IP 224.0.100.100
        /// </summary>
        public void TSEPreClose(List<PcpaData> pcpaDataList, int date)
        {
            LogManager.Configuration.Variables["varmarket"] = "YesterdayPrice";
            Dictionary<string, decimal> dicPreClose = new Dictionary<string, decimal>();
            Dictionary<string, Product> dicp = new Dictionary<string, Product>();           
            
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


            var log2 = LogManager.GetLogger("TSE");

            foreach (var pd in dicPreClose)
            {
                log2.Info(pd.Key + "," + pd.Value);
            }
        }

        /// <summary>
        /// 對應IP 224.8.100.100
        /// </summary>
        public void TSEODD(List<PcpaData> pcpaDataList)
        {
            Dictionary<string, Product> dicp = new Dictionary<string, Product>();
            Dictionary<string, List<string>> dicData = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> dicPreClose = new Dictionary<string, List<string>>();
           
            
            foreach (var item in pcpaDataList)
            {
                int offset = 0;
                int LastSeq = 0;
                long lasttime = 0;
                var bytes = item.Bytes;
                var pcapTime = item.PcapTime;

                if (bytes[offset] == 27 && offset + 4 < bytes.Length)
                {
                    int Len = ((bytes[offset + 1] & 0xf) + ((bytes[offset + 1] >> 4) & 0xF) * 10) * 100
                            + ((bytes[offset + 2] & 0xf) + ((bytes[offset + 2] >> 4) & 0xF) * 10);

                    if (bytes[offset + 3] == 0x01 && bytes[offset + 4] == 0x23)
                    {
                        #region 0123

                        int seq = ((bytes[offset + 6] & 0xf) + ((bytes[offset + 6] >> 4) & 0xF) * 10) * 1000000
                                 + ((bytes[offset + 7] & 0xf) + ((bytes[offset + 7] >> 4) & 0xF) * 10) * 10000
                                 + ((bytes[offset + 8] & 0xf) + ((bytes[offset + 8] >> 4) & 0xF) * 10) * 100
                                 + ((bytes[offset + 9] & 0xf) + ((bytes[offset + 9] >> 4) & 0xF) * 10);

                        if (LastSeq == 0)
                        {
                            LastSeq = seq;
                        }                        
                        else
                        {
                            LastSeq = seq;
                        }

                        byte[] buf = new byte[Len];
                        //int seq = Convert.ToInt32(bytes[offset + 6].ToString("X2") + bytes[offset + 7].ToString("X2") + bytes[offset + 8].ToString("X2") + bytes[offset + 9].ToString("X2"));
                        Array.Copy(bytes, offset, buf, 0, Len);
                        string stockID = Encoding.ASCII.GetString(buf, 10, 6).Trim(); //股票代號

                        if (stockID != "000000")
                        {
                            if (dicp.ContainsKey(stockID) == false)
                            {
                                List<string> list = new List<string>();
                                list.Add(string.Format("PcapTime,Last,Vol,BID1,BIDSZ1,BID2,BIDSZ2,BID3,BIDSZ3,BID4,BIDSZ4,BID5,BIDSZ5,ASK1,ASKSZ1,ASK2,ASKSZ2,ASK3,ASKSZ3,ASK4,ASKSZ4,ASK5,ASKSZ5,Tick,Volume,Time,LastTime"));
                                dicData.Add(stockID, list);
                                dicp.Add(stockID, new Product());
                            }

                            var product = dicp[stockID];

                            string dataTime = bytes[offset + 16].ToString("X2") + bytes[offset + 17].ToString("X2") + bytes[offset + 18].ToString("X2") + bytes[offset + 19].ToString("X2") + bytes[offset + 20].ToString("X2") + bytes[offset + 21].ToString("X2");

                            if (lasttime < Convert.ToInt64(dataTime))
                            {
                                lasttime = Convert.ToInt64(dataTime);
                            }
                            else if (lasttime > Convert.ToInt64(dataTime) + 10000000000)
                            {
                                break;
                            }

                            int bit0 = ((bytes[offset + 22] & 1) == 0) ? 0 : 1;
                            int bit1 = ((bytes[offset + 22] & 2) == 0) ? 0 : 1;
                            int bit2 = ((bytes[offset + 22] & 4) == 0) ? 0 : 1;
                            int bit3 = ((bytes[offset + 22] & 8) == 0) ? 0 : 1;
                            int bit4 = ((bytes[offset + 22] & 16) == 0) ? 0 : 1;
                            int bit5 = ((bytes[offset + 22] & 32) == 0) ? 0 : 1;
                            int bit6 = ((bytes[offset + 22] & 64) == 0) ? 0 : 1;
                            int bit7 = ((bytes[offset + 22] & 128) == 0) ? 0 : 1;
                            int dealflag = ((bytes[offset + 24] & 128) == 0) ? 1 : 0;
                            int buyCount = 0;
                            int sellCount = 0;
                            int dealCount = 0;

                            if (bit4 == 1)
                            {
                                buyCount = 1;
                            }
                            if (bit5 == 1)
                            {
                                buyCount = buyCount + 2;
                            }
                            if (bit6 == 1)
                            {
                                buyCount = buyCount + 4;
                            }

                            if (bit1 == 1)
                            {
                                sellCount = 1;
                            }
                            if (bit2 == 1)
                            {
                                sellCount = sellCount + 2;
                            }
                            if (bit3 == 1)
                            {
                                sellCount = sellCount + 4;
                            }

                            if (bit7 == 1)
                            {
                                dealCount = 1;
                            }
                            else
                            {
                                dealCount = 0;
                            }

                            int totalCount = dealCount + buyCount + sellCount;

                            int Volume = Convert.ToInt32(
                                  bytes[offset + 25].ToString("X2")
                                + bytes[offset + 26].ToString("X2")
                                + bytes[offset + 27].ToString("X2")
                                + bytes[offset + 28].ToString("X2")
                                + bytes[offset + 29].ToString("X2")
                                + bytes[offset + 30].ToString("X2"));

                            decimal LastPrice = 0;
                            int LastQty = 0;
                            decimal BidPrice1 = 0;
                            decimal BidPrice2 = 0;
                            decimal BidPrice3 = 0;
                            decimal BidPrice4 = 0;
                            decimal BidPrice5 = 0;
                            int BidQty1 = 0;
                            int BidQty2 = 0;
                            int BidQty3 = 0;
                            int BidQty4 = 0;
                            int BidQty5 = 0;
                            decimal AskPrice1 = 0;
                            decimal AskPrice2 = 0;
                            decimal AskPrice3 = 0;
                            decimal AskPrice4 = 0;
                            decimal AskPrice5 = 0;
                            int AskQty1 = 0;
                            int AskQty2 = 0;
                            int AskQty3 = 0;
                            int AskQty4 = 0;
                            int AskQty5 = 0;

                            for (int i = 0; i < totalCount; i++)
                            {
                                decimal price = (((bytes[31 + offset + i * 11] & 0xf) + ((bytes[31 + offset + i * 11] >> 4) & 0xF) * 10) * 100000000 +
                                                 ((bytes[32 + offset + i * 11] & 0xf) + ((bytes[32 + offset + i * 11] >> 4) & 0xF) * 10) * 1000000 +
                                                 ((bytes[33 + offset + i * 11] & 0xf) + ((bytes[33 + offset + i * 11] >> 4) & 0xF) * 10) * 10000 +
                                                 ((bytes[34 + offset + i * 11] & 0xf) + ((bytes[34 + offset + i * 11] >> 4) & 0xF) * 10) * 100 +
                                                 ((bytes[35 + offset + i * 11] & 0xf) + ((bytes[35 + offset + i * 11] >> 4) & 0xF) * 10)) * 0.0001m;

                                int qty = Convert.ToInt32(
                                  bytes[offset + 36 + i * 11].ToString("X2")
                                + bytes[offset + 37 + i * 11].ToString("X2")
                                + bytes[offset + 38 + i * 11].ToString("X2")
                                + bytes[offset + 39 + i * 11].ToString("X2")
                                + bytes[offset + 40 + i * 11].ToString("X2")
                                + bytes[offset + 41 + i * 11].ToString("X2"));

                                if (i < dealCount)
                                {
                                    LastPrice = price;
                                    LastQty = qty;
                                    product.Last = price;
                                    product.LastQty = qty;
                                }
                                else
                                {
                                    if (i >= dealCount && i < (dealCount + buyCount))
                                    {
                                        switch (i - dealCount)
                                        {
                                            case 0:
                                                BidPrice1 = price;
                                                BidQty1 = qty;
                                                break;
                                            case 1:
                                                BidPrice2 = price;
                                                BidQty2 = qty;
                                                break;
                                            case 2:
                                                BidPrice3 = price;
                                                BidQty3 = qty;
                                                break;
                                            case 3:
                                                BidPrice4 = price;
                                                BidQty4 = qty;
                                                break;
                                            case 4:
                                                BidPrice5 = price;
                                                BidQty5 = qty;
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        switch (i - dealCount - buyCount)
                                        {
                                            case 0:
                                                AskPrice1 = price;
                                                AskQty1 = qty;
                                                break;
                                            case 1:
                                                AskPrice2 = price;
                                                AskQty2 = qty;
                                                break;
                                            case 2:
                                                AskPrice3 = price;
                                                AskQty3 = qty;
                                                break;
                                            case 3:
                                                AskPrice4 = price;
                                                AskQty4 = qty;
                                                break;
                                            case 4:
                                                AskPrice5 = price;
                                                AskQty5 = qty;
                                                break;
                                        }
                                    }
                                }
                            }

                            product.BID1 = BidPrice1;
                            product.BID2 = BidPrice2;
                            product.BID3 = BidPrice3;
                            product.BID4 = BidPrice4;
                            product.BID5 = BidPrice5;
                            product.BIDSiz1 = BidQty1;
                            product.BIDSiz2 = BidQty2;
                            product.BIDSiz3 = BidQty3;
                            product.BIDSiz4 = BidQty4;
                            product.BIDSiz5 = BidQty5;

                            product.ASK1 = AskPrice1;
                            product.ASK2 = AskPrice2;
                            product.ASK3 = AskPrice3;
                            product.ASK4 = AskPrice4;
                            product.ASK5 = AskPrice5;
                            product.ASKSiz1 = AskQty1;
                            product.ASKSiz2 = AskQty2;
                            product.ASKSiz3 = AskQty3;
                            product.ASKSiz4 = AskQty4;
                            product.ASKSiz5 = AskQty5;

                            if (LastQty > 0)
                            {
                                dicData[stockID].Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27}"
                                      , pcapTime, product.Last, product.LastQty
                                      , product.BID1, product.BIDSiz1
                                      , product.BID2, product.BIDSiz2
                                      , product.BID3, product.BIDSiz3
                                      , product.BID4, product.BIDSiz4
                                      , product.BID5, product.BIDSiz5
                                      , product.ASK1, product.ASKSiz1
                                      , product.ASK2, product.ASKSiz2
                                      , product.ASK3, product.ASKSiz3
                                      , product.ASK4, product.ASKSiz4
                                      , product.ASK5, product.ASKSiz5
                                      , dealflag
                                      , Volume
                                      , dataTime, dataTime
                                      , seq));
                            }
                            else
                            {

                                dicData[stockID].Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27}"
                                      , pcapTime, product.Last, product.LastQty
                                      , product.BID1, product.BIDSiz1
                                      , product.BID2, product.BIDSiz2
                                      , product.BID3, product.BIDSiz3
                                      , product.BID4, product.BIDSiz4
                                      , product.BID5, product.BIDSiz5
                                      , product.ASK1, product.ASKSiz1
                                      , product.ASK2, product.ASKSiz2
                                      , product.ASK3, product.ASKSiz3
                                      , product.ASK4, product.ASKSiz4
                                      , product.ASK5, product.ASKSiz5
                                      , dealflag
                                      , Volume
                                      , dataTime, ""
                                      , seq));
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


            foreach (var pd in dicData)
            {
                var log = LogManager.GetLogger(pd.Key);
                foreach (var data in pd.Value)
                {
                    log.Info(data);
                }
            }
        }

        /// <summary>
        /// 對應IP 224.0.30.30
        /// </summary>
        public void OTC(List<PcpaData> pcpaDataList)
        {
            Dictionary<string, decimal> dicPreClose = new Dictionary<string, decimal>();
            Dictionary<string, Product> dicp = new Dictionary<string, Product>();
            Dictionary<string, List<string>> dicData = new Dictionary<string, List<string>>();        
            
            try
            {
                foreach (var item in pcpaDataList)
                {
                    int offset = 0;
                    bool indexflag = true;
                    int LastSeq = 0;
                    long lasttime = 0;
                    var bytes = item.Bytes;
                    var pcapTime = item.PcapTime;

                    if (bytes[offset] == 27)
                    {
                        int Len = Convert.ToInt32(bytes[offset + 1].ToString("X2") + bytes[offset + 2].ToString("X2"));
                        //int seq = Convert.ToInt32(bytes[offset + 6].ToString("X2") + bytes[offset + 7].ToString("X2") + bytes[offset + 8].ToString("X2") + bytes[offset + 9].ToString("X2"));
                        #region 0206
                        if (bytes[offset + 3] == 0x02 && bytes[offset + 4] == 0x06)
                        {
                            int seq = ((bytes[offset + 6] & 0xf) + ((bytes[offset + 6] >> 4) & 0xF) * 10) * 1000000
                                     + ((bytes[offset + 7] & 0xf) + ((bytes[offset + 7] >> 4) & 0xF) * 10) * 10000
                                     + ((bytes[offset + 8] & 0xf) + ((bytes[offset + 8] >> 4) & 0xF) * 10) * 100
                                     + ((bytes[offset + 9] & 0xf) + ((bytes[offset + 9] >> 4) & 0xF) * 10);

                            if (LastSeq == 0)
                            {
                                LastSeq = seq;
                            }
                            else
                            {
                                LastSeq = seq;
                            }

                            byte[] buf = new byte[Len];
                            Array.Copy(bytes, offset, buf, 0, Len);
                            string stockID = Encoding.ASCII.GetString(buf, 10, 6).Trim(); //股票代號

                            if (stockID != "000000")
                            {
                                if (dicp.ContainsKey(stockID) == false)
                                {
                                    List<string> list = new List<string>();
                                    list.Add("PcapTime,Last,Vol,BID1,BIDSZ1,BID2,BIDSZ2,BID3,BIDSZ3,BID4,BIDSZ4,BID5,BIDSZ5,ASK1,ASKSZ1,ASK2,ASKSZ2,ASK3,ASKSZ3,ASK4,ASKSZ4,ASK5,ASKSZ5,Tick,Volume,Time,LastTime");
                                    dicData.Add(stockID, list);
                                    dicp.Add(stockID, new Product());
                                }


                                var product = dicp[stockID];

                                string dataTime = bytes[offset + 16].ToString("X2") + bytes[offset + 17].ToString("X2") + bytes[offset + 18].ToString("X2") + bytes[offset + 19].ToString("X2") + bytes[offset + 20].ToString("X2") + bytes[offset + 21].ToString("X2");

                                if (lasttime < Convert.ToInt64(dataTime))
                                {
                                    lasttime = Convert.ToInt64(dataTime);
                                }
                                else if (lasttime > Convert.ToInt64(dataTime) + 10000000000)
                                {
                                    break;
                                }

                                int bit0 = ((bytes[offset + 22] & 1) == 0) ? 0 : 1;
                                int bit1 = ((bytes[offset + 22] & 2) == 0) ? 0 : 1;
                                int bit2 = ((bytes[offset + 22] & 4) == 0) ? 0 : 1;
                                int bit3 = ((bytes[offset + 22] & 8) == 0) ? 0 : 1;
                                int bit4 = ((bytes[offset + 22] & 16) == 0) ? 0 : 1;
                                int bit5 = ((bytes[offset + 22] & 32) == 0) ? 0 : 1;
                                int bit6 = ((bytes[offset + 22] & 64) == 0) ? 0 : 1;
                                int bit7 = ((bytes[offset + 22] & 128) == 0) ? 0 : 1;

                                int buyCount = 0;
                                int sellCount = 0;
                                int dealCount = 0;

                                if (bit4 == 1)
                                {
                                    buyCount = 1;
                                }
                                if (bit5 == 1)
                                {
                                    buyCount = buyCount + 2;
                                }
                                if (bit6 == 1)
                                {
                                    buyCount = buyCount + 4;
                                }

                                if (bit1 == 1)
                                {
                                    sellCount = 1;
                                }
                                if (bit2 == 1)
                                {
                                    sellCount = sellCount + 2;
                                }
                                if (bit3 == 1)
                                {
                                    sellCount = sellCount + 4;
                                }

                                if (bit7 == 1)
                                {
                                    dealCount = 1;
                                }
                                else
                                {
                                    dealCount = 0;
                                }

                                int totalCount = dealCount + buyCount + sellCount;

                                int Volume = Convert.ToInt32(bytes[offset + 25].ToString("X2") + bytes[offset + 26].ToString("X2") + bytes[offset + 27].ToString("X2") + bytes[offset + 28].ToString("X2"));

                                decimal LastPrice = 0;
                                int LastQty = 0;
                                decimal BidPrice1 = 0;
                                decimal BidPrice2 = 0;
                                decimal BidPrice3 = 0;
                                decimal BidPrice4 = 0;
                                decimal BidPrice5 = 0;
                                int BidQty1 = 0;
                                int BidQty2 = 0;
                                int BidQty3 = 0;
                                int BidQty4 = 0;
                                int BidQty5 = 0;
                                decimal AskPrice1 = 0;
                                decimal AskPrice2 = 0;
                                decimal AskPrice3 = 0;
                                decimal AskPrice4 = 0;
                                decimal AskPrice5 = 0;
                                int AskQty1 = 0;
                                int AskQty2 = 0;
                                int AskQty3 = 0;
                                int AskQty4 = 0;
                                int AskQty5 = 0;

                                for (int i = 0; i < totalCount; i++)
                                {
                                    decimal price = (((bytes[29 + offset + i * 9] & 0xf) + ((bytes[29 + offset + i * 9] >> 4) & 0xF) * 10) * 100000000 +
                                                     ((bytes[30 + offset + i * 9] & 0xf) + ((bytes[30 + offset + i * 9] >> 4) & 0xF) * 10) * 1000000 +
                                                     ((bytes[31 + offset + i * 9] & 0xf) + ((bytes[31 + offset + i * 9] >> 4) & 0xF) * 10) * 10000 +
                                                     ((bytes[32 + offset + i * 9] & 0xf) + ((bytes[32 + offset + i * 9] >> 4) & 0xF) * 10) * 100 +
                                                     ((bytes[33 + offset + i * 9] & 0xf) + ((bytes[33 + offset + i * 9] >> 4) & 0xF) * 10)) * 0.0001m;

                                    int qty = ((bytes[34 + offset + i * 9] & 0xf) + ((bytes[34 + offset + i * 9] >> 4) & 0xF) * 10) * 1000000 +
                                              ((bytes[35 + offset + i * 9] & 0xf) + ((bytes[35 + offset + i * 9] >> 4) & 0xF) * 10) * 10000 +
                                              ((bytes[36 + offset + i * 9] & 0xf) + ((bytes[36 + offset + i * 9] >> 4) & 0xF) * 10) * 100 +
                                              ((bytes[37 + offset + i * 9] & 0xf) + ((bytes[37 + offset + i * 9] >> 4) & 0xF) * 10);

                                    if (i < dealCount)
                                    {
                                        LastPrice = price;
                                        LastQty = qty;
                                        product.Last = price;
                                        product.LastQty = qty;
                                    }
                                    else
                                    {
                                        if (i >= dealCount && i < (dealCount + buyCount))
                                        {
                                            switch (i - dealCount)
                                            {
                                                case 0:
                                                    BidPrice1 = price;
                                                    BidQty1 = qty;
                                                    break;
                                                case 1:
                                                    BidPrice2 = price;
                                                    BidQty2 = qty;
                                                    break;
                                                case 2:
                                                    BidPrice3 = price;
                                                    BidQty3 = qty;
                                                    break;
                                                case 3:
                                                    BidPrice4 = price;
                                                    BidQty4 = qty;
                                                    break;
                                                case 4:
                                                    BidPrice5 = price;
                                                    BidQty5 = qty;
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            switch (i - dealCount - buyCount)
                                            {
                                                case 0:
                                                    AskPrice1 = price;
                                                    AskQty1 = qty;
                                                    break;
                                                case 1:
                                                    AskPrice2 = price;
                                                    AskQty2 = qty;
                                                    break;
                                                case 2:
                                                    AskPrice3 = price;
                                                    AskQty3 = qty;
                                                    break;
                                                case 3:
                                                    AskPrice4 = price;
                                                    AskQty4 = qty;
                                                    break;
                                                case 4:
                                                    AskPrice5 = price;
                                                    AskQty5 = qty;
                                                    break;
                                            }
                                        }
                                    }
                                }

                                product.BID1 = BidPrice1;
                                product.BID2 = BidPrice2;
                                product.BID3 = BidPrice3;
                                product.BID4 = BidPrice4;
                                product.BID5 = BidPrice5;
                                product.BIDSiz1 = BidQty1;
                                product.BIDSiz2 = BidQty2;
                                product.BIDSiz3 = BidQty3;
                                product.BIDSiz4 = BidQty4;
                                product.BIDSiz5 = BidQty5;

                                product.ASK1 = AskPrice1;
                                product.ASK2 = AskPrice2;
                                product.ASK3 = AskPrice3;
                                product.ASK4 = AskPrice4;
                                product.ASK5 = AskPrice5;
                                product.ASKSiz1 = AskQty1;
                                product.ASKSiz2 = AskQty2;
                                product.ASKSiz3 = AskQty3;
                                product.ASKSiz4 = AskQty4;
                                product.ASKSiz5 = AskQty5;

                                if (LastQty > 0)
                                {
                                    dicData[stockID].Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27}"
                                          , pcapTime, product.Last, product.LastQty
                                          , product.BID1, product.BIDSiz1
                                          , product.BID2, product.BIDSiz2
                                          , product.BID3, product.BIDSiz3
                                          , product.BID4, product.BIDSiz4
                                          , product.BID5, product.BIDSiz5
                                          , product.ASK1, product.ASKSiz1
                                          , product.ASK2, product.ASKSiz2
                                          , product.ASK3, product.ASKSiz3
                                          , product.ASK4, product.ASKSiz4
                                          , product.ASK5, product.ASKSiz5
                                          , "1"
                                          , Volume
                                          , dataTime, dataTime
                                          , seq));
                                }
                                else
                                {
                                    dicData[stockID].Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27}"
                                          , pcapTime, product.Last, product.LastQty
                                          , product.BID1, product.BIDSiz1
                                          , product.BID2, product.BIDSiz2
                                          , product.BID3, product.BIDSiz3
                                          , product.BID4, product.BIDSiz4
                                          , product.BID5, product.BIDSiz5
                                          , product.ASK1, product.ASKSiz1
                                          , product.ASK2, product.ASKSiz2
                                          , product.ASK3, product.ASKSiz3
                                          , product.ASK4, product.ASKSiz4
                                          , product.ASK5, product.ASKSiz5
                                          , "0"
                                          , Volume
                                          , dataTime, ""
                                          , seq));
                                }
                            }
                        }
                        #endregion

                        #region 0203
                        else if (bytes[offset + 3] == 0x02 && bytes[offset + 4] == 0x03)
                        {
                            byte[] buf = new byte[Len];
                            Array.Copy(bytes, offset, buf, 0, Len);
                            string dataTime = buf[10].ToString("X2") + buf[11].ToString("X2") + buf[12].ToString("X2") + "000000";

                            if (dataTime == "133000000000")
                            {

                            }
                            else if (dataTime != "000000000000" && indexflag && Convert.ToInt64(dataTime) > 90000000000)
                            {
                                if (dataTime == "999999000000")
                                {
                                    dataTime = "133000000000";
                                    indexflag = false;
                                }

                                decimal OTCA = Convert.ToDecimal(buf[14].ToString("X2") + buf[15].ToString("X2") + buf[16].ToString("X2") + "." + buf[17].ToString("X2"));

                                if (dicp.ContainsKey("OTCA") == false)
                                {
                                    LogManager.GetLogger("OTCA").Info("PcapTime,Last,Vol,BID1,BIDSZ1,BID2,BIDSZ2,BID3,BIDSZ3,BID4,BIDSZ4,BID5,BIDSZ5,ASK1,ASKSZ1,ASK2,ASKSZ2,ASK3,ASKSZ3,ASK4,ASKSZ4,ASK5,ASKSZ5,Tick,Volume,Time,LastTime");
                                    dicp.Add("OTCA", new Product());
                                }

                                LogManager.GetLogger("OTCA").Info("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25}"
                                             , pcapTime, OTCA, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "1", 0, dataTime, dataTime);
                            }
                        }
                        #endregion

                        #region 0201
                        else if (bytes[offset + 3] == 0x02 && bytes[offset + 4] == 0x01)
                        {
                            string productID = Encoding.ASCII.GetString(bytes, 10, 6).Trim(); //商品代號
                            decimal close = Convert.ToDecimal(bytes[40].ToString("X2") + bytes[41].ToString("X2") + bytes[42].ToString("X2") + "." + bytes[43].ToString("X2") + bytes[44].ToString("X2"));
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


            foreach (var pd in dicData)
            {
                var log = LogManager.GetLogger(pd.Key);
                foreach (var data in pd.Value)
                {
                    log.Info(data);
                }
            }

            LogManager.Configuration.Variables["varmarket"] = "YesterdayPrice";

            var log2 = LogManager.GetLogger("OTC");

            foreach (var pd in dicPreClose)
            {
                log2.Info(pd.Value);
            }
        }

        /// <summary>
        /// 對應IP 224.0.30.30
        /// </summary>
        public void OTCPreClose(List<PcpaData> pcpaDataList, int date)
        {
            Dictionary<string, decimal> dicPreClose = new Dictionary<string, decimal>();
            
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

            LogManager.Configuration.Variables["varmarket"] = "YesterdayPrice";

            var log2 = LogManager.GetLogger("OTC");

            foreach (var pd in dicPreClose)
            {
                log2.Info(pd.Key + "," + pd.Value);
            }
        }

        /// <summary>
        /// 對應IP 224.8.30.30
        /// </summary>
        public void OTCODD(List<PcpaData> pcpaDataList)
        {
            Dictionary<string, decimal> dicPreClose = new Dictionary<string, decimal>();
            Dictionary<string, Product> dicp = new Dictionary<string, Product>();
            Dictionary<string, List<string>> dicData = new Dictionary<string, List<string>>();
            
            try
            {
                foreach (var item in pcpaDataList)
                {
                    int offset = 0;
                    bool indexflag = true;
                    int LastSeq = 0;
                    long lasttime = 0;
                    var bytes = item.Bytes;
                    var pcapTime = item.PcapTime;

                    if (bytes[offset] == 27)
                    {
                        int Len = Convert.ToInt32(bytes[offset + 1].ToString("X2") + bytes[offset + 2].ToString("X2"));
                        //int seq = Convert.ToInt32(bytes[offset + 6].ToString("X2") + bytes[offset + 7].ToString("X2") + bytes[offset + 8].ToString("X2") + bytes[offset + 9].ToString("X2"));
                        #region 0206
                        if (bytes[offset + 3] == 0x02 && bytes[offset + 4] == 0x23)
                        {
                            int seq = ((bytes[offset + 6] & 0xf) + ((bytes[offset + 6] >> 4) & 0xF) * 10) * 1000000
                                     + ((bytes[offset + 7] & 0xf) + ((bytes[offset + 7] >> 4) & 0xF) * 10) * 10000
                                     + ((bytes[offset + 8] & 0xf) + ((bytes[offset + 8] >> 4) & 0xF) * 10) * 100
                                     + ((bytes[offset + 9] & 0xf) + ((bytes[offset + 9] >> 4) & 0xF) * 10);

                            if (LastSeq == 0)
                            {
                                LastSeq = seq;
                            }
                            else
                            {
                                LastSeq = seq;
                            }

                            byte[] buf = new byte[Len];
                            Array.Copy(bytes, offset, buf, 0, Len);
                            string stockID = Encoding.ASCII.GetString(buf, 10, 6).Trim(); //股票代號

                            if (stockID != "000000")
                            {
                                if (dicp.ContainsKey(stockID) == false)
                                {
                                    List<string> list = new List<string>();
                                    list.Add("Time,Last,Vol,BID1,BIDSZ1,BID2,BIDSZ2,BID3,BIDSZ3,BID4,BIDSZ4,BID5,BIDSZ5,ASK1,ASKSZ1,ASK2,ASKSZ2,ASK3,ASKSZ3,ASK4,ASKSZ4,ASK5,ASKSZ5,Tick,Volume,LastTime");
                                    dicData.Add(stockID, list);
                                    dicp.Add(stockID, new Product());
                                }

                                var product = dicp[stockID];

                                string dataTime = bytes[offset + 16].ToString("X2") + bytes[offset + 17].ToString("X2") + bytes[offset + 18].ToString("X2") + bytes[offset + 19].ToString("X2") + bytes[offset + 20].ToString("X2") + bytes[offset + 21].ToString("X2");

                                if (lasttime < Convert.ToInt64(dataTime))
                                {
                                    lasttime = Convert.ToInt64(dataTime);
                                }
                                else if (lasttime > Convert.ToInt64(dataTime) + 10000000000)
                                {
                                    break;
                                }

                                int bit0 = ((bytes[offset + 22] & 1) == 0) ? 0 : 1;
                                int bit1 = ((bytes[offset + 22] & 2) == 0) ? 0 : 1;
                                int bit2 = ((bytes[offset + 22] & 4) == 0) ? 0 : 1;
                                int bit3 = ((bytes[offset + 22] & 8) == 0) ? 0 : 1;
                                int bit4 = ((bytes[offset + 22] & 16) == 0) ? 0 : 1;
                                int bit5 = ((bytes[offset + 22] & 32) == 0) ? 0 : 1;
                                int bit6 = ((bytes[offset + 22] & 64) == 0) ? 0 : 1;
                                int bit7 = ((bytes[offset + 22] & 128) == 0) ? 0 : 1;
                                int dealflag = ((bytes[offset + 24] & 128) == 0) ? 1 : 0;
                                int buyCount = 0;
                                int sellCount = 0;
                                int dealCount = 0;

                                if (bit4 == 1)
                                {
                                    buyCount = 1;
                                }
                                if (bit5 == 1)
                                {
                                    buyCount = buyCount + 2;
                                }
                                if (bit6 == 1)
                                {
                                    buyCount = buyCount + 4;
                                }

                                if (bit1 == 1)
                                {
                                    sellCount = 1;
                                }
                                if (bit2 == 1)
                                {
                                    sellCount = sellCount + 2;
                                }
                                if (bit3 == 1)
                                {
                                    sellCount = sellCount + 4;
                                }

                                if (bit7 == 1)
                                {
                                    dealCount = 1;
                                }
                                else
                                {
                                    dealCount = 0;
                                }

                                int totalCount = dealCount + buyCount + sellCount;

                                int Volume = Convert.ToInt32(
                                    bytes[offset + 25].ToString("X2")
                                    + bytes[offset + 26].ToString("X2")
                                    + bytes[offset + 27].ToString("X2")
                                    + bytes[offset + 28].ToString("X2")
                                    + bytes[offset + 29].ToString("X2")
                                    + bytes[offset + 30].ToString("X2"));

                                decimal LastPrice = 0;
                                int LastQty = 0;
                                decimal BidPrice1 = 0;
                                decimal BidPrice2 = 0;
                                decimal BidPrice3 = 0;
                                decimal BidPrice4 = 0;
                                decimal BidPrice5 = 0;
                                int BidQty1 = 0;
                                int BidQty2 = 0;
                                int BidQty3 = 0;
                                int BidQty4 = 0;
                                int BidQty5 = 0;
                                decimal AskPrice1 = 0;
                                decimal AskPrice2 = 0;
                                decimal AskPrice3 = 0;
                                decimal AskPrice4 = 0;
                                decimal AskPrice5 = 0;
                                int AskQty1 = 0;
                                int AskQty2 = 0;
                                int AskQty3 = 0;
                                int AskQty4 = 0;
                                int AskQty5 = 0;

                                for (int i = 0; i < totalCount; i++)
                                {
                                    decimal price = (((bytes[31 + offset + i * 11] & 0xf) + ((bytes[31 + offset + i * 11] >> 4) & 0xF) * 10) * 100000000 +
                                                     ((bytes[32 + offset + i * 11] & 0xf) + ((bytes[32 + offset + i * 11] >> 4) & 0xF) * 10) * 1000000 +
                                                     ((bytes[33 + offset + i * 11] & 0xf) + ((bytes[33 + offset + i * 11] >> 4) & 0xF) * 10) * 10000 +
                                                     ((bytes[34 + offset + i * 11] & 0xf) + ((bytes[34 + offset + i * 11] >> 4) & 0xF) * 10) * 100 +
                                                     ((bytes[35 + offset + i * 11] & 0xf) + ((bytes[35 + offset + i * 11] >> 4) & 0xF) * 10)) * 0.0001m;

                                    int qty = Convert.ToInt32(
                                      bytes[offset + 36 + i * 11].ToString("X2")
                                    + bytes[offset + 37 + i * 11].ToString("X2")
                                    + bytes[offset + 38 + i * 11].ToString("X2")
                                    + bytes[offset + 39 + i * 11].ToString("X2")
                                    + bytes[offset + 40 + i * 11].ToString("X2")
                                    + bytes[offset + 41 + i * 11].ToString("X2"));

                                    if (i < dealCount)
                                    {
                                        LastPrice = price;
                                        LastQty = qty;
                                        product.Last = price;
                                        product.LastQty = qty;
                                    }
                                    else
                                    {
                                        if (i >= dealCount && i < (dealCount + buyCount))
                                        {
                                            switch (i - dealCount)
                                            {
                                                case 0:
                                                    BidPrice1 = price;
                                                    BidQty1 = qty;
                                                    break;
                                                case 1:
                                                    BidPrice2 = price;
                                                    BidQty2 = qty;
                                                    break;
                                                case 2:
                                                    BidPrice3 = price;
                                                    BidQty3 = qty;
                                                    break;
                                                case 3:
                                                    BidPrice4 = price;
                                                    BidQty4 = qty;
                                                    break;
                                                case 4:
                                                    BidPrice5 = price;
                                                    BidQty5 = qty;
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            switch (i - dealCount - buyCount)
                                            {
                                                case 0:
                                                    AskPrice1 = price;
                                                    AskQty1 = qty;
                                                    break;
                                                case 1:
                                                    AskPrice2 = price;
                                                    AskQty2 = qty;
                                                    break;
                                                case 2:
                                                    AskPrice3 = price;
                                                    AskQty3 = qty;
                                                    break;
                                                case 3:
                                                    AskPrice4 = price;
                                                    AskQty4 = qty;
                                                    break;
                                                case 4:
                                                    AskPrice5 = price;
                                                    AskQty5 = qty;
                                                    break;
                                            }
                                        }
                                    }
                                }

                                product.BID1 = BidPrice1;
                                product.BID2 = BidPrice2;
                                product.BID3 = BidPrice3;
                                product.BID4 = BidPrice4;
                                product.BID5 = BidPrice5;
                                product.BIDSiz1 = BidQty1;
                                product.BIDSiz2 = BidQty2;
                                product.BIDSiz3 = BidQty3;
                                product.BIDSiz4 = BidQty4;
                                product.BIDSiz5 = BidQty5;

                                product.ASK1 = AskPrice1;
                                product.ASK2 = AskPrice2;
                                product.ASK3 = AskPrice3;
                                product.ASK4 = AskPrice4;
                                product.ASK5 = AskPrice5;
                                product.ASKSiz1 = AskQty1;
                                product.ASKSiz2 = AskQty2;
                                product.ASKSiz3 = AskQty3;
                                product.ASKSiz4 = AskQty4;
                                product.ASKSiz5 = AskQty5;

                                if (LastQty > 0)
                                {
                                    dicData[stockID].Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27}"
                                          , pcapTime, product.Last, product.LastQty
                                          , product.BID1, product.BIDSiz1
                                          , product.BID2, product.BIDSiz2
                                          , product.BID3, product.BIDSiz3
                                          , product.BID4, product.BIDSiz4
                                          , product.BID5, product.BIDSiz5
                                          , product.ASK1, product.ASKSiz1
                                          , product.ASK2, product.ASKSiz2
                                          , product.ASK3, product.ASKSiz3
                                          , product.ASK4, product.ASKSiz4
                                          , product.ASK5, product.ASKSiz5
                                          , dealflag
                                          , Volume
                                          , dataTime, dataTime
                                          , seq));
                                }
                                else
                                {
                                    dicData[stockID].Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27}"
                                          , pcapTime, product.Last, product.LastQty
                                          , product.BID1, product.BIDSiz1
                                          , product.BID2, product.BIDSiz2
                                          , product.BID3, product.BIDSiz3
                                          , product.BID4, product.BIDSiz4
                                          , product.BID5, product.BIDSiz5
                                          , product.ASK1, product.ASKSiz1
                                          , product.ASK2, product.ASKSiz2
                                          , product.ASK3, product.ASKSiz3
                                          , product.ASK4, product.ASKSiz4
                                          , product.ASK5, product.ASKSiz5
                                          , dealflag
                                          , Volume
                                          , dataTime, ""
                                          , seq));
                                }
                            }
                        }
                        #endregion

                        #region 0203
                        else if (bytes[offset + 3] == 0x02 && bytes[offset + 4] == 0x03)
                        {
                            byte[] buf = new byte[Len];
                            Array.Copy(bytes, offset, buf, 0, Len);
                            string dataTime = buf[10].ToString("X2") + buf[11].ToString("X2") + buf[12].ToString("X2") + "000000";

                            if (dataTime == "133000000000")
                            {

                            }
                            else if (dataTime != "000000000000" && indexflag && Convert.ToInt64(dataTime) > 90000000000)
                            {
                                if (dataTime == "999999000000")
                                {
                                    dataTime = "133000000000";
                                    indexflag = false;
                                }

                                decimal OTCA = Convert.ToDecimal(buf[14].ToString("X2") + buf[15].ToString("X2") + buf[16].ToString("X2") + "." + buf[17].ToString("X2"));

                                if (dicp.ContainsKey("OTCA") == false)
                                {
                                    LogManager.GetLogger("OTCA").Info("PcapTime,Last,Vol,BID1,BIDSZ1,BID2,BIDSZ2,BID3,BIDSZ3,BID4,BIDSZ4,BID5,BIDSZ5,ASK1,ASKSZ1,ASK2,ASKSZ2,ASK3,ASKSZ3,ASK4,ASKSZ4,ASK5,ASKSZ5,Tick,Volume,Time,LastTime");
                                    dicp.Add("OTCA", new Product());
                                }

                                LogManager.GetLogger("OTCA").Info("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}"
                                             , pcapTime, OTCA, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "1", 0, dataTime, dataTime);
                            }
                        }
                        #endregion

                        #region 0201
                        else if (bytes[offset + 3] == 0x02 && bytes[offset + 4] == 0x01)
                        {
                            string productID = Encoding.ASCII.GetString(bytes, 10, 6).Trim(); //商品代號
                            decimal close = Convert.ToDecimal(bytes[40].ToString("X2") + bytes[41].ToString("X2") + bytes[42].ToString("X2") + "." + bytes[43].ToString("X2") + bytes[44].ToString("X2"));
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


            foreach (var pd in dicData)
            {
                var log = LogManager.GetLogger(pd.Key);
                foreach (var data in pd.Value)
                {
                    log.Info(data);
                }
            }

            LogManager.Configuration.Variables["varmarket"] = "YesterdayPrice";

            var log2 = LogManager.GetLogger("OTC");

            foreach (var pd in dicPreClose)
            {
                log2.Info(pd.Value);
            }
        }

        
        /// <summary>
        /// 對應IP 225.0.100.100
        /// </summary>
        public void Future(List<PcpaData> pcpaDataList)
        {
            Dictionary<string, decimal> dicPreClose = new Dictionary<string, decimal>();
            Dictionary<string, Product> dicp = new Dictionary<string, Product>();
            Dictionary<string, List<string>> dicData = new Dictionary<string, List<string>>();
                        
            try
            {
                int I020Lastseq = 0;
                int I080Lastseq = 0;
                int I220Lastseq = 0;
                int I280Lastseq = 0;

                foreach (var item in pcpaDataList)
                {
                    int offset = 0;                    
                    var bytes = item.Bytes;
                    var pcapTime = item.PcapTime;

                    if (bytes[offset] == 27)
                    {
                        int Len = ((bytes[offset + 14] & 0xf) + ((bytes[offset + 14] >> 4) & 0xF) * 10) * 100
                                + ((bytes[offset + 15] & 0xf) + ((bytes[offset + 15] >> 4) & 0xF) * 10) + 19;

                        byte[] buf = new byte[Len];
                        Array.Copy(bytes, offset, buf, 0, Len);

                        if (buf.Length > 36 && buf[1] == 50 && (buf[2] == 49 || buf[2] == 50 || buf[2] == 55 || buf[2] == 56))
                        {
                            string productID = Encoding.ASCII.GetString(buf, 16, 20).Trim().Replace('/', '-');
                            if (productID != "00")
                            {
                                if (dicp.ContainsKey(productID) == false)
                                {
                                    List<string> list = new List<string>();
                                    list.Add("PcapTime,Last,Vol,BID1,BIDSZ1,BID2,BIDSZ2,BID3,BIDSZ3,BID4,BIDSZ4,BID5,BIDSZ5,ASK1,ASKSZ1,ASK2,ASKSZ2,ASK3,ASKSZ3,ASK4,ASKSZ4,ASK5,ASKSZ5,Tick,Volume,Time,LastTime");
                                    dicData.Add(productID, list);
                                    dicp.Add(productID, new Product());
                                }
                                var product = dicp[productID];

                                int seq = (((buf[9] & 0xf) + ((buf[9] >> 4) & 0xF) * 10) * 1000000 +
                                           ((buf[10] & 0xf) + ((buf[10] >> 4) & 0xF) * 10) * 10000 +
                                           ((buf[11] & 0xf) + ((buf[11] >> 4) & 0xF) * 10) * 100 +
                                           ((buf[12] & 0xf) + ((buf[12] >> 4) & 0xF) * 10));


                                #region 成交
                                if (buf[2] == 49 || buf[2] == 55)
                                {
                                    if (buf[2] == 49)
                                    {
                                        if (I020Lastseq == 0)
                                        {
                                            I020Lastseq = seq;
                                        }
                                        else
                                        {
                                            I020Lastseq = seq;
                                        }
                                    }
                                    else if (buf[2] == 55)
                                    {
                                        if (I220Lastseq == 0)
                                        {
                                            I220Lastseq = seq;
                                        }
                                        else
                                        {
                                            I220Lastseq = seq;
                                        }
                                    }

                                    int HH = ((buf[3] & 0xf) + ((buf[3] >> 4) & 0xF) * 10);
                                    int mm = ((buf[4] & 0xf) + ((buf[4] >> 4) & 0xF) * 10);
                                    int ss = ((buf[5] & 0xf) + ((buf[5] >> 4) & 0xF) * 10);
                                    int ff1 = ((buf[6] & 0xf) + ((buf[6] >> 4) & 0xF) * 10);
                                    int ff2 = ((buf[7] & 0xf) + ((buf[7] >> 4) & 0xF) * 10);
                                    int ff3 = ((buf[8] & 0xf) + ((buf[8] >> 4) & 0xF) * 10);

                                    long informationTime1 = HH * 10000 +
                                                            mm * 100 +
                                                            ss;
                                    informationTime1 = informationTime1 * 1000000;
                                    long informationTime = ff1 * 10000 +
                                                           ff2 * 100 +
                                                           ff3;
                                    informationTime = informationTime + informationTime1;

                                    HH = ((buf[36] & 0xf) + ((buf[36] >> 4) & 0xF) * 10);
                                    mm = ((buf[37] & 0xf) + ((buf[37] >> 4) & 0xF) * 10);
                                    ss = ((buf[38] & 0xf) + ((buf[38] >> 4) & 0xF) * 10);
                                    ff1 = ((buf[39] & 0xf) + ((buf[39] >> 4) & 0xF) * 10);
                                    ff2 = ((buf[40] & 0xf) + ((buf[40] >> 4) & 0xF) * 10);
                                    ff3 = ((buf[41] & 0xf) + ((buf[41] >> 4) & 0xF) * 10);

                                    long matchTime1 = HH * 10000 +
                                                      mm * 100 +
                                                      ss;
                                    matchTime1 = matchTime1 * 1000000;
                                    long matchTime = ff1 * 10000 +
                                                     ff2 * 100 +
                                                     ff3;
                                    matchTime += matchTime1;

                                    int count = buf[52] % 128;
                                    int sign = 1;

                                    if (buf[65] != 0x98)
                                    {
                                        if (buf[42] == 0x2d)
                                        {
                                            sign = -1;
                                        }

                                        product.Last = sign * (((buf[43] & 0xf) + ((buf[43] >> 4) & 0xF) * 10) * 100000000 +
                                                        ((buf[44] & 0xf) + ((buf[44] >> 4) & 0xF) * 10) * 1000000 +
                                                        ((buf[45] & 0xf) + ((buf[45] >> 4) & 0xF) * 10) * 10000 +
                                                        ((buf[46] & 0xf) + ((buf[46] >> 4) & 0xF) * 10) * 100 +
                                                        ((buf[47] & 0xf) + ((buf[47] >> 4) & 0xF) * 10)) * 0.01m;


                                        product.LastQty = (((buf[48] & 0xf) + ((buf[48] >> 4) & 0xF) * 10) * 1000000 +
                                                           ((buf[49] & 0xf) + ((buf[49] >> 4) & 0xF) * 10) * 10000 +
                                                           ((buf[50] & 0xf) + ((buf[50] >> 4) & 0xF) * 10) * 100 +
                                                           ((buf[51] & 0xf) + ((buf[51] >> 4) & 0xF) * 10));

                                        product.Volume = (((buf[53 + count * 8] & 0xf) + ((buf[53 + count * 8] >> 4) & 0xF) * 10) * 1000000 +
                                                          ((buf[54 + count * 8] & 0xf) + ((buf[54 + count * 8] >> 4) & 0xF) * 10) * 10000 +
                                                          ((buf[55 + count * 8] & 0xf) + ((buf[55 + count * 8] >> 4) & 0xF) * 10) * 100 +
                                                          ((buf[56 + count * 8] & 0xf) + ((buf[56 + count * 8] >> 4) & 0xF) * 10));

                                        dicData[productID].Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}"
                                            , pcapTime
                                            , product.Last, product.LastQty
                                            , product.BID1, product.BIDSiz1
                                            , product.BID2, product.BIDSiz2
                                            , product.BID3, product.BIDSiz3
                                            , product.BID4, product.BIDSiz4
                                            , product.BID5, product.BIDSiz5
                                            , product.ASK1, product.ASKSiz1
                                            , product.ASK2, product.ASKSiz2
                                            , product.ASK3, product.ASKSiz3
                                            , product.ASK4, product.ASKSiz4
                                            , product.ASK5, product.ASKSiz5
                                            , "1"
                                            , product.Volume
                                            , informationTime, matchTime));


                                        //本次揭示第一個封包
                                        for (int i = 0; i < count; i++)
                                        {
                                            sign = 1;
                                            if (buf[53 + i * 8] == 0x2d) //因複式商品成交價包含正負數，故以此欄位標識價格之正負。 ｀0':正號 ｀-':負號
                                            {
                                                sign = -1;
                                            }

                                            product.Last = sign * (((buf[54 + i * 8] & 0xf) + ((buf[54 + i * 8] >> 4) & 0xF) * 10) * 100000000 +
                                                                   ((buf[55 + i * 8] & 0xf) + ((buf[55 + i * 8] >> 4) & 0xF) * 10) * 1000000 +
                                                                   ((buf[56 + i * 8] & 0xf) + ((buf[56 + i * 8] >> 4) & 0xF) * 10) * 10000 +
                                                                   ((buf[57 + i * 8] & 0xf) + ((buf[57 + i * 8] >> 4) & 0xF) * 10) * 100 +
                                                                   ((buf[58 + i * 8] & 0xf) + ((buf[58 + i * 8] >> 4) & 0xF) * 10)) * 0.01m;

                                            product.LastQty = (((buf[59 + i * 8] & 0xf) + ((buf[59 + i * 8] >> 4) & 0xF) * 10) * 100 +
                                                               ((buf[60 + i * 8] & 0xf) + ((buf[60 + i * 8] >> 4) & 0xF) * 10));

                                            dicData[productID].Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}"
                                             , pcapTime
                                             , product.Last, product.LastQty
                                             , product.BID1, product.BIDSiz1
                                             , product.BID2, product.BIDSiz2
                                             , product.BID3, product.BIDSiz3
                                             , product.BID4, product.BIDSiz4
                                             , product.BID5, product.BIDSiz5
                                             , product.ASK1, product.ASKSiz1
                                             , product.ASK2, product.ASKSiz2
                                             , product.ASK3, product.ASKSiz3
                                             , product.ASK4, product.ASKSiz4
                                             , product.ASK5, product.ASKSiz5
                                             , "1"
                                             , product.Volume
                                             , informationTime, matchTime));
                                        }

                                    }
                                }
                                #endregion
                                #region 五檔
                                else if (buf[2] == 50 || buf[2] == 56)
                                {
                                    // int informationSeq = Convert.ToInt32(buf[9].ToString("X2") + buf[10].ToString("X2") + buf[11].ToString("X2") + buf[12].ToString("X2")); //傳送訊息流水序號
                                    if (buf[2] == 50)
                                    {
                                        if (I080Lastseq == 0)
                                        {
                                            I080Lastseq = seq;
                                        }
                                        else
                                        {
                                            I080Lastseq = seq;
                                        }
                                    }
                                    else if (buf[2] == 56)
                                    {
                                        if (I280Lastseq == 0)
                                        {
                                            I280Lastseq = seq;
                                        }
                                        else
                                        {
                                            I280Lastseq = seq;
                                        }
                                    }

                                    int HH = ((buf[3] & 0xf) + ((buf[3] >> 4) & 0xF) * 10);
                                    int mm = ((buf[4] & 0xf) + ((buf[4] >> 4) & 0xF) * 10);
                                    int ss = ((buf[5] & 0xf) + ((buf[5] >> 4) & 0xF) * 10);
                                    int ff1 = ((buf[6] & 0xf) + ((buf[6] >> 4) & 0xF) * 10);
                                    int ff2 = ((buf[7] & 0xf) + ((buf[7] >> 4) & 0xF) * 10);
                                    int ff3 = ((buf[8] & 0xf) + ((buf[8] >> 4) & 0xF) * 10);

                                    long informationTime1 = HH * 10000 +
                                                            mm * 100 +
                                                            ss;
                                    informationTime1 = informationTime1 * 1000000;
                                    long informationTime = ff1 * 10000 +
                                                           ff2 * 100 +
                                                           ff3;
                                    informationTime = informationTime + informationTime1;

                                    decimal buyPrice1 = 0; //最佳買價1
                                    decimal buyPrice2 = 0; //最佳買價2
                                    decimal buyPrice3 = 0; //最佳買價3
                                    decimal buyPrice4 = 0; //最佳買價4
                                    decimal buyPrice5 = 0; //最佳買價5
                                    int buyQty1 = 0; //最佳買價1之委託量
                                    int buyQty2 = 0; //最佳買價2之委託量
                                    int buyQty3 = 0; //最佳買價3之委託量
                                    int buyQty4 = 0; //最佳買價4之委託量
                                    int buyQty5 = 0; //最佳買價5之委託量
                                    decimal sellPrice1 = 0; //最佳賣價1
                                    decimal sellPrice2 = 0; //最佳賣價2
                                    decimal sellPrice3 = 0; //最佳賣價3
                                    decimal sellPrice4 = 0; //最佳賣價4
                                    decimal sellPrice5 = 0; //最佳賣價5
                                    int sellQty1 = 0; //最佳賣價1之委託量
                                    int sellQty2 = 0; //最佳賣價2之委託量
                                    int sellQty3 = 0; //最佳賣價3之委託量
                                    int sellQty4 = 0; //最佳賣價4之委託量
                                    int sellQty5 = 0; //最佳賣價5之委託量
                                    decimal firstDerivedBuyPrice = 0; //衍生一檔委買價
                                    int firstDerivedBuyQty = 0; //衍生一檔委買價之委託量
                                    decimal firstDerivedSellPrice = 0; //衍生一檔委賣價
                                    int firstDerivedSellQty = 0; //衍生一檔委賣價之委託量                                    

                                    int sign = 1;
                                    decimal tmpPrice = 0;
                                    int tmpQty = 0;
                                    for (int i = 0; i < 5; i++)
                                    {
                                        sign = 1;
                                        if (buf[36 + i * 10] == 0x2d) { sign = -1; }
                                        tmpPrice = sign * (((buf[37 + i * 10] & 0xf) + ((buf[37 + i * 10] >> 4) & 0xF) * 10) * 100000000 +
                                                           ((buf[38 + i * 10] & 0xf) + ((buf[38 + i * 10] >> 4) & 0xF) * 10) * 1000000 +
                                                           ((buf[39 + i * 10] & 0xf) + ((buf[39 + i * 10] >> 4) & 0xF) * 10) * 10000 +
                                                           ((buf[40 + i * 10] & 0xf) + ((buf[40 + i * 10] >> 4) & 0xF) * 10) * 100 +
                                                           ((buf[41 + i * 10] & 0xf) + ((buf[41 + i * 10] >> 4) & 0xF) * 10)) * 0.01m;

                                        tmpQty = (((buf[42 + i * 10] & 0xf) + ((buf[42 + i * 10] >> 4) & 0xF) * 10) * 1000000 +
                                                    ((buf[43 + i * 10] & 0xf) + ((buf[43 + i * 10] >> 4) & 0xF) * 10) * 10000 +
                                                    ((buf[44 + i * 10] & 0xf) + ((buf[44 + i * 10] >> 4) & 0xF) * 10) * 100 +
                                                    ((buf[45 + i * 10] & 0xf) + ((buf[45 + i * 10] >> 4) & 0xF) * 10));

                                        switch (i)
                                        {
                                            case 0:
                                                buyPrice1 = tmpPrice;
                                                buyQty1 = tmpQty;
                                                break;
                                            case 1:
                                                buyPrice2 = tmpPrice;
                                                buyQty2 = tmpQty;
                                                break;
                                            case 2:
                                                buyPrice3 = tmpPrice;
                                                buyQty3 = tmpQty;
                                                break;
                                            case 3:
                                                buyPrice4 = tmpPrice;
                                                buyQty4 = tmpQty;
                                                break;
                                            case 4:
                                                buyPrice5 = tmpPrice;
                                                buyQty5 = tmpQty;
                                                break;
                                        }
                                    }

                                    for (int i = 0; i < 5; i++)
                                    {
                                        sign = 1;
                                        if (buf[86 + i * 10] == 0x2d) { sign = -1; }
                                        tmpPrice = sign * (((buf[87 + i * 10] & 0xf) + ((buf[87 + i * 10] >> 4) & 0xF) * 10) * 100000000 +
                                                           ((buf[88 + i * 10] & 0xf) + ((buf[88 + i * 10] >> 4) & 0xF) * 10) * 1000000 +
                                                           ((buf[89 + i * 10] & 0xf) + ((buf[89 + i * 10] >> 4) & 0xF) * 10) * 10000 +
                                                           ((buf[90 + i * 10] & 0xf) + ((buf[90 + i * 10] >> 4) & 0xF) * 10) * 100 +
                                                           ((buf[91 + i * 10] & 0xf) + ((buf[91 + i * 10] >> 4) & 0xF) * 10)) * 0.01m;

                                        tmpQty = (((buf[92 + i * 10] & 0xf) + ((buf[92 + i * 10] >> 4) & 0xF) * 10) * 1000000 +
                                                  ((buf[93 + i * 10] & 0xf) + ((buf[93 + i * 10] >> 4) & 0xF) * 10) * 10000 +
                                                  ((buf[94 + i * 10] & 0xf) + ((buf[94 + i * 10] >> 4) & 0xF) * 10) * 100 +
                                                  ((buf[95 + i * 10] & 0xf) + ((buf[95 + i * 10] >> 4) & 0xF) * 10));
                                        switch (i)
                                        {
                                            case 0:
                                                sellPrice1 = tmpPrice;
                                                sellQty1 = tmpQty;
                                                break;
                                            case 1:
                                                sellPrice2 = tmpPrice;
                                                sellQty2 = tmpQty;
                                                break;
                                            case 2:
                                                sellPrice3 = tmpPrice;
                                                sellQty3 = tmpQty;
                                                break;
                                            case 3:
                                                sellPrice4 = tmpPrice;
                                                sellQty4 = tmpQty;
                                                break;
                                            case 4:
                                                sellPrice5 = tmpPrice;
                                                sellQty5 = tmpQty;
                                                break;
                                        }
                                    }


                                    if (buf[136] == 0x01)
                                    {
                                        //虛擬委託單第一檔買進價格
                                        firstDerivedBuyPrice = (((buf[137] & 0xf) + ((buf[137] >> 4) & 0xF) * 10) * 100000000 +
                                                                ((buf[138] & 0xf) + ((buf[138] >> 4) & 0xF) * 10) * 1000000 +
                                                                ((buf[139] & 0xf) + ((buf[139] >> 4) & 0xF) * 10) * 10000 +
                                                                ((buf[140] & 0xf) + ((buf[140] >> 4) & 0xF) * 10) * 100 +
                                                                ((buf[141] & 0xf) + ((buf[141] >> 4) & 0xF) * 10)) * 0.01m;

                                        firstDerivedBuyQty = (((buf[142] & 0xf) + ((buf[142] >> 4) & 0xF) * 10) * 1000000 +
                                                              ((buf[143] & 0xf) + ((buf[143] >> 4) & 0xF) * 10) * 10000 +
                                                              ((buf[144] & 0xf) + ((buf[144] >> 4) & 0xF) * 10) * 100 +
                                                              ((buf[145] & 0xf) + ((buf[145] >> 4) & 0xF) * 10));

                                        //虛擬委託單第一檔賣出價格
                                        firstDerivedSellPrice = (((buf[146] & 0xf) + ((buf[146] >> 4) & 0xF) * 10) * 100000000 +
                                                                 ((buf[147] & 0xf) + ((buf[147] >> 4) & 0xF) * 10) * 1000000 +
                                                                 ((buf[148] & 0xf) + ((buf[148] >> 4) & 0xF) * 10) * 10000 +
                                                                 ((buf[149] & 0xf) + ((buf[149] >> 4) & 0xF) * 10) * 100 +
                                                                 ((buf[150] & 0xf) + ((buf[150] >> 4) & 0xF) * 10)) * 0.01m;

                                        firstDerivedSellQty = (((buf[151] & 0xf) + ((buf[151] >> 4) & 0xF) * 10) * 1000000 +
                                                               ((buf[152] & 0xf) + ((buf[152] >> 4) & 0xF) * 10) * 10000 +
                                                               ((buf[153] & 0xf) + ((buf[153] >> 4) & 0xF) * 10) * 100 +
                                                               ((buf[154] & 0xf) + ((buf[154] >> 4) & 0xF) * 10));
                                    }

                                    if (firstDerivedBuyPrice == buyPrice1)
                                    {
                                        buyQty1 += firstDerivedBuyQty;
                                    }
                                    else if (firstDerivedBuyPrice > buyPrice1 && firstDerivedBuyPrice != 0)
                                    {
                                        buyPrice5 = buyPrice4;
                                        buyPrice4 = buyPrice3;
                                        buyPrice3 = buyPrice2;
                                        buyPrice2 = buyPrice1;
                                        buyPrice1 = firstDerivedBuyPrice;

                                        buyQty5 = buyQty4;
                                        buyQty4 = buyQty3;
                                        buyQty3 = buyQty2;
                                        buyQty2 = buyQty1;
                                        buyQty1 = firstDerivedBuyQty;
                                    }

                                    if (firstDerivedSellPrice == sellPrice1)
                                    {
                                        sellQty1 += firstDerivedSellQty;
                                    }
                                    else if (firstDerivedSellPrice < sellPrice1 && firstDerivedSellPrice != 0)
                                    {
                                        sellPrice5 = sellPrice4;
                                        sellPrice4 = sellPrice3;
                                        sellPrice3 = sellPrice2;
                                        sellPrice2 = sellPrice1;
                                        sellPrice1 = firstDerivedSellPrice;

                                        sellQty5 = sellQty4;
                                        sellQty4 = sellQty3;
                                        sellQty3 = sellQty2;
                                        sellQty2 = sellQty1;
                                        sellQty1 = firstDerivedSellQty;
                                    }

                                    product.BID1 = buyPrice1;
                                    product.BID2 = buyPrice2;
                                    product.BID3 = buyPrice3;
                                    product.BID4 = buyPrice4;
                                    product.BID5 = buyPrice5;
                                    product.BIDSiz1 = buyQty1;
                                    product.BIDSiz2 = buyQty2;
                                    product.BIDSiz3 = buyQty3;
                                    product.BIDSiz4 = buyQty4;
                                    product.BIDSiz5 = buyQty5;

                                    product.ASK1 = sellPrice1;
                                    product.ASK2 = sellPrice2;
                                    product.ASK3 = sellPrice3;
                                    product.ASK4 = sellPrice4;
                                    product.ASK5 = sellPrice5;
                                    product.ASKSiz1 = sellQty1;
                                    product.ASKSiz2 = sellQty2;
                                    product.ASKSiz3 = sellQty3;
                                    product.ASKSiz4 = sellQty4;
                                    product.ASKSiz5 = sellQty5;

                                    dicData[productID].Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}"
                                               , pcapTime, product.Last, product.LastQty
                                               , product.BID1, product.BIDSiz1
                                               , product.BID2, product.BIDSiz2
                                               , product.BID3, product.BIDSiz3
                                               , product.BID4, product.BIDSiz4
                                               , product.BID5, product.BIDSiz5
                                               , product.ASK1, product.ASKSiz1
                                               , product.ASK2, product.ASKSiz2
                                               , product.ASK3, product.ASKSiz3
                                               , product.ASK4, product.ASKSiz4
                                               , product.ASK5, product.ASKSiz5
                                               , "0"
                                               , product.Volume
                                               , informationTime, ""));

                                }
                                #endregion
                            }
                        }
                        else if (buf.Length > 36 && buf[1] == 49 && buf[2] == 49)
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
            catch (Exception)
            {
                throw;
            }


            foreach (var pd in dicData)
            {
                var log = LogManager.GetLogger(pd.Key);
                foreach (var data in pd.Value)
                {
                    log.Info(data);
                }
            }

            LogManager.Configuration.Variables["varmarket"] = "YesterdayPrice";

            var log2 = LogManager.GetLogger("Future");

            foreach (var pd in dicPreClose)
            {
                log2.Info(pd.Value);
            }
        }


        /// <summary>
        /// 對應IP 225.0.100.100
        /// </summary>
        public void FuturePreClose(List<PcpaData> pcpaDataList, int date)
        {
            Dictionary<string, decimal> dicPreClose = new Dictionary<string, decimal>();
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

            LogManager.Configuration.Variables["varmarket"] = "YesterdayPrice";

            var log2 = LogManager.GetLogger("Future");

            foreach (var pd in dicPreClose)
            {
                log2.Info(pd.Key + "," + pd.Value);
            }
        }


        /// <summary>
        /// 對應IP 225.0.30.30
        /// </summary>
        public void Option(List<PcpaData> pcpaDataList)
        {
            Dictionary<string, Product> dicp = new Dictionary<string, Product>();
            Dictionary<string, List<string>> dicData = new Dictionary<string, List<string>>();
                        
            int I020Lastseq = 0;
            int I080Lastseq = 0;
            int I220Lastseq = 0;
            int I280Lastseq = 0;
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

                        byte[] buf = new byte[Len];
                        Array.Copy(bytes, offset, buf, 0, Len);

                        if (buf.Length > 36 && buf[1] == 53 && (buf[2] == 49 || buf[2] == 50 || buf[2] == 55 || buf[2] == 56))
                        {
                            string productID = Encoding.ASCII.GetString(buf, 16, 20).Trim();
                            if (productID != "00")
                            {
                                if (dicp.ContainsKey(productID) == false)
                                {
                                    List<string> list = new List<string>();
                                    list.Add("PcapTime,Last,Vol,BID1,BIDSZ1,BID2,BIDSZ2,BID3,BIDSZ3,BID4,BIDSZ4,BID5,BIDSZ5,ASK1,ASKSZ1,ASK2,ASKSZ2,ASK3,ASKSZ3,ASK4,ASKSZ4,ASK5,ASKSZ5,Tick,Volume,Time,LastTime");
                                    dicData.Add(productID, list);
                                    dicp.Add(productID, new Product());
                                }
                                var product = dicp[productID];

                                int seq = (((buf[9] & 0xf) + ((buf[9] >> 4) & 0xF) * 10) * 1000000 +
                                           ((buf[10] & 0xf) + ((buf[10] >> 4) & 0xF) * 10) * 10000 +
                                           ((buf[11] & 0xf) + ((buf[11] >> 4) & 0xF) * 10) * 100 +
                                           ((buf[12] & 0xf) + ((buf[12] >> 4) & 0xF) * 10));

                                #region 成交
                                if (buf[2] == 49 || buf[2] == 55)
                                {
                                    if (buf[2] == 49)
                                    {
                                        if (I020Lastseq == 0)
                                        {
                                            I020Lastseq = seq;
                                        }
                                        else
                                        {
                                            I020Lastseq = seq;
                                        }
                                    }
                                    else if (buf[2] == 55)
                                    {
                                        if (I220Lastseq == 0)
                                        {
                                            I220Lastseq = seq;
                                        }
                                        else
                                        {
                                            I220Lastseq = seq;
                                        }
                                    }

                                    int HH = ((buf[3] & 0xf) + ((buf[3] >> 4) & 0xF) * 10);
                                    int mm = ((buf[4] & 0xf) + ((buf[4] >> 4) & 0xF) * 10);
                                    int ss = ((buf[5] & 0xf) + ((buf[5] >> 4) & 0xF) * 10);
                                    int ff1 = ((buf[6] & 0xf) + ((buf[6] >> 4) & 0xF) * 10);
                                    int ff2 = ((buf[7] & 0xf) + ((buf[7] >> 4) & 0xF) * 10);
                                    int ff3 = ((buf[8] & 0xf) + ((buf[8] >> 4) & 0xF) * 10);

                                    long informationTime1 = HH * 10000 +
                                                            mm * 100 +
                                                            ss;
                                    informationTime1 = informationTime1 * 1000000;
                                    long informationTime = ff1 * 10000 +
                                                           ff2 * 100 +
                                                           ff3;
                                    informationTime = informationTime + informationTime1;

                                    HH = ((buf[36] & 0xf) + ((buf[36] >> 4) & 0xF) * 10);
                                    mm = ((buf[37] & 0xf) + ((buf[37] >> 4) & 0xF) * 10);
                                    ss = ((buf[38] & 0xf) + ((buf[38] >> 4) & 0xF) * 10);
                                    ff1 = ((buf[39] & 0xf) + ((buf[39] >> 4) & 0xF) * 10);
                                    ff2 = ((buf[40] & 0xf) + ((buf[40] >> 4) & 0xF) * 10);
                                    ff3 = ((buf[41] & 0xf) + ((buf[41] >> 4) & 0xF) * 10);

                                    long matchTime1 = HH * 10000 +
                                                      mm * 100 +
                                                      ss;
                                    matchTime1 = matchTime1 * 1000000;
                                    long matchTime = ff1 * 10000 +
                                                     ff2 * 100 +
                                                     ff3;
                                    matchTime += matchTime1;

                                    int count = buf[52] % 128;
                                    int sign = 1;

                                    if (buf[65] != 0x98)
                                    {
                                        if (buf[42] == 0x2d)
                                        {
                                            sign = -1;
                                        }

                                        product.Last = sign * (((buf[43] & 0xf) + ((buf[43] >> 4) & 0xF) * 10) * 100000000 +
                                                        ((buf[44] & 0xf) + ((buf[44] >> 4) & 0xF) * 10) * 1000000 +
                                                        ((buf[45] & 0xf) + ((buf[45] >> 4) & 0xF) * 10) * 10000 +
                                                        ((buf[46] & 0xf) + ((buf[46] >> 4) & 0xF) * 10) * 100 +
                                                        ((buf[47] & 0xf) + ((buf[47] >> 4) & 0xF) * 10)) * 0.001m;


                                        product.LastQty = (((buf[48] & 0xf) + ((buf[48] >> 4) & 0xF) * 10) * 1000000 +
                                                           ((buf[49] & 0xf) + ((buf[49] >> 4) & 0xF) * 10) * 10000 +
                                                           ((buf[50] & 0xf) + ((buf[50] >> 4) & 0xF) * 10) * 100 +
                                                           ((buf[51] & 0xf) + ((buf[51] >> 4) & 0xF) * 10));

                                        product.Volume = (((buf[53 + count * 8] & 0xf) + ((buf[53 + count * 8] >> 4) & 0xF) * 10) * 1000000 +
                                                          ((buf[54 + count * 8] & 0xf) + ((buf[54 + count * 8] >> 4) & 0xF) * 10) * 10000 +
                                                          ((buf[55 + count * 8] & 0xf) + ((buf[55 + count * 8] >> 4) & 0xF) * 10) * 100 +
                                                          ((buf[56 + count * 8] & 0xf) + ((buf[56 + count * 8] >> 4) & 0xF) * 10));

                                        dicData[productID].Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}"
                                            , pcapTime
                                            , product.Last, product.LastQty
                                            , product.BID1, product.BIDSiz1
                                            , product.BID2, product.BIDSiz2
                                            , product.BID3, product.BIDSiz3
                                            , product.BID4, product.BIDSiz4
                                            , product.BID5, product.BIDSiz5
                                            , product.ASK1, product.ASKSiz1
                                            , product.ASK2, product.ASKSiz2
                                            , product.ASK3, product.ASKSiz3
                                            , product.ASK4, product.ASKSiz4
                                            , product.ASK5, product.ASKSiz5
                                            , "1"
                                            , product.Volume
                                            , informationTime, matchTime));


                                        //本次揭示第一個封包
                                        for (int i = 0; i < count; i++)
                                        {
                                            sign = 1;
                                            if (buf[53 + i * 8] == 0x2d) //因複式商品成交價包含正負數，故以此欄位標識價格之正負。 ｀0':正號 ｀-':負號
                                            {
                                                sign = -1;
                                            }

                                            product.Last = sign * (((buf[54 + i * 8] & 0xf) + ((buf[54 + i * 8] >> 4) & 0xF) * 10) * 100000000 +
                                                                   ((buf[55 + i * 8] & 0xf) + ((buf[55 + i * 8] >> 4) & 0xF) * 10) * 1000000 +
                                                                   ((buf[56 + i * 8] & 0xf) + ((buf[56 + i * 8] >> 4) & 0xF) * 10) * 10000 +
                                                                   ((buf[57 + i * 8] & 0xf) + ((buf[57 + i * 8] >> 4) & 0xF) * 10) * 100 +
                                                                   ((buf[58 + i * 8] & 0xf) + ((buf[58 + i * 8] >> 4) & 0xF) * 10)) * 0.001m;

                                            product.LastQty = (((buf[59 + i * 8] & 0xf) + ((buf[59 + i * 8] >> 4) & 0xF) * 10) * 100 +
                                                               ((buf[60 + i * 8] & 0xf) + ((buf[60 + i * 8] >> 4) & 0xF) * 10));

                                            dicData[productID].Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}"
                                             , pcapTime
                                             , product.Last, product.LastQty
                                             , product.BID1, product.BIDSiz1
                                             , product.BID2, product.BIDSiz2
                                             , product.BID3, product.BIDSiz3
                                             , product.BID4, product.BIDSiz4
                                             , product.BID5, product.BIDSiz5
                                             , product.ASK1, product.ASKSiz1
                                             , product.ASK2, product.ASKSiz2
                                             , product.ASK3, product.ASKSiz3
                                             , product.ASK4, product.ASKSiz4
                                             , product.ASK5, product.ASKSiz5
                                             , "1"
                                             , product.Volume
                                             , informationTime, matchTime));
                                        }

                                    }
                                }
                                #endregion
                                #region 五檔
                                else if (buf[2] == 50 || buf[2] == 56)
                                {
                                    // int informationSeq = Convert.ToInt32(buf[9].ToString("X2") + buf[10].ToString("X2") + buf[11].ToString("X2") + buf[12].ToString("X2")); //傳送訊息流水序號
                                    if (buf[2] == 50)
                                    {
                                        if (I080Lastseq == 0)
                                        {
                                            I080Lastseq = seq;
                                        }
                                        else
                                        {
                                            I080Lastseq = seq;
                                        }
                                    }
                                    else if (buf[2] == 56)
                                    {
                                        if (I280Lastseq == 0)
                                        {
                                            I280Lastseq = seq;
                                        }
                                        else
                                        {
                                            I280Lastseq = seq;
                                        }
                                    }

                                    int HH = ((buf[3] & 0xf) + ((buf[3] >> 4) & 0xF) * 10);
                                    int mm = ((buf[4] & 0xf) + ((buf[4] >> 4) & 0xF) * 10);
                                    int ss = ((buf[5] & 0xf) + ((buf[5] >> 4) & 0xF) * 10);
                                    int ff1 = ((buf[6] & 0xf) + ((buf[6] >> 4) & 0xF) * 10);
                                    int ff2 = ((buf[7] & 0xf) + ((buf[7] >> 4) & 0xF) * 10);
                                    int ff3 = ((buf[8] & 0xf) + ((buf[8] >> 4) & 0xF) * 10);

                                    long informationTime1 = HH * 10000 +
                                                            mm * 100 +
                                                            ss;
                                    informationTime1 = informationTime1 * 1000000;
                                    long informationTime = ff1 * 10000 +
                                                           ff2 * 100 +
                                                           ff3;
                                    informationTime = informationTime + informationTime1;

                                    decimal buyPrice1 = 0; //最佳買價1
                                    decimal buyPrice2 = 0; //最佳買價2
                                    decimal buyPrice3 = 0; //最佳買價3
                                    decimal buyPrice4 = 0; //最佳買價4
                                    decimal buyPrice5 = 0; //最佳買價5
                                    int buyQty1 = 0; //最佳買價1之委託量
                                    int buyQty2 = 0; //最佳買價2之委託量
                                    int buyQty3 = 0; //最佳買價3之委託量
                                    int buyQty4 = 0; //最佳買價4之委託量
                                    int buyQty5 = 0; //最佳買價5之委託量
                                    decimal sellPrice1 = 0; //最佳賣價1
                                    decimal sellPrice2 = 0; //最佳賣價2
                                    decimal sellPrice3 = 0; //最佳賣價3
                                    decimal sellPrice4 = 0; //最佳賣價4
                                    decimal sellPrice5 = 0; //最佳賣價5
                                    int sellQty1 = 0; //最佳賣價1之委託量
                                    int sellQty2 = 0; //最佳賣價2之委託量
                                    int sellQty3 = 0; //最佳賣價3之委託量
                                    int sellQty4 = 0; //最佳賣價4之委託量
                                    int sellQty5 = 0; //最佳賣價5之委託量

                                    int sign = 1;
                                    decimal tmpPrice = 0;
                                    int tmpQty = 0;
                                    for (int i = 0; i < 5; i++)
                                    {
                                        sign = 1;
                                        if (buf[36 + i * 10] == 0x2d) { sign = -1; }
                                        tmpPrice = sign * (((buf[37 + i * 10] & 0xf) + ((buf[37 + i * 10] >> 4) & 0xF) * 10) * 100000000 +
                                                           ((buf[38 + i * 10] & 0xf) + ((buf[38 + i * 10] >> 4) & 0xF) * 10) * 1000000 +
                                                           ((buf[39 + i * 10] & 0xf) + ((buf[39 + i * 10] >> 4) & 0xF) * 10) * 10000 +
                                                           ((buf[40 + i * 10] & 0xf) + ((buf[40 + i * 10] >> 4) & 0xF) * 10) * 100 +
                                                           ((buf[41 + i * 10] & 0xf) + ((buf[41 + i * 10] >> 4) & 0xF) * 10)) * 0.001m;

                                        tmpQty = (((buf[42 + i * 10] & 0xf) + ((buf[42 + i * 10] >> 4) & 0xF) * 10) * 1000000 +
                                                    ((buf[43 + i * 10] & 0xf) + ((buf[43 + i * 10] >> 4) & 0xF) * 10) * 10000 +
                                                    ((buf[44 + i * 10] & 0xf) + ((buf[44 + i * 10] >> 4) & 0xF) * 10) * 100 +
                                                    ((buf[45 + i * 10] & 0xf) + ((buf[45 + i * 10] >> 4) & 0xF) * 10));

                                        switch (i)
                                        {
                                            case 0:
                                                buyPrice1 = tmpPrice;
                                                buyQty1 = tmpQty;
                                                break;
                                            case 1:
                                                buyPrice2 = tmpPrice;
                                                buyQty2 = tmpQty;
                                                break;
                                            case 2:
                                                buyPrice3 = tmpPrice;
                                                buyQty3 = tmpQty;
                                                break;
                                            case 3:
                                                buyPrice4 = tmpPrice;
                                                buyQty4 = tmpQty;
                                                break;
                                            case 4:
                                                buyPrice5 = tmpPrice;
                                                buyQty5 = tmpQty;
                                                break;
                                        }
                                    }

                                    for (int i = 0; i < 5; i++)
                                    {
                                        sign = 1;
                                        if (buf[86 + i * 10] == 0x2d) { sign = -1; }
                                        tmpPrice = sign * (((buf[87 + i * 10] & 0xf) + ((buf[87 + i * 10] >> 4) & 0xF) * 10) * 100000000 +
                                                           ((buf[88 + i * 10] & 0xf) + ((buf[88 + i * 10] >> 4) & 0xF) * 10) * 1000000 +
                                                           ((buf[89 + i * 10] & 0xf) + ((buf[89 + i * 10] >> 4) & 0xF) * 10) * 10000 +
                                                           ((buf[90 + i * 10] & 0xf) + ((buf[90 + i * 10] >> 4) & 0xF) * 10) * 100 +
                                                           ((buf[91 + i * 10] & 0xf) + ((buf[91 + i * 10] >> 4) & 0xF) * 10)) * 0.001m;

                                        tmpQty = (((buf[92 + i * 10] & 0xf) + ((buf[92 + i * 10] >> 4) & 0xF) * 10) * 1000000 +
                                                  ((buf[93 + i * 10] & 0xf) + ((buf[93 + i * 10] >> 4) & 0xF) * 10) * 10000 +
                                                  ((buf[94 + i * 10] & 0xf) + ((buf[94 + i * 10] >> 4) & 0xF) * 10) * 100 +
                                                  ((buf[95 + i * 10] & 0xf) + ((buf[95 + i * 10] >> 4) & 0xF) * 10));
                                        switch (i)
                                        {
                                            case 0:
                                                sellPrice1 = tmpPrice;
                                                sellQty1 = tmpQty;
                                                break;
                                            case 1:
                                                sellPrice2 = tmpPrice;
                                                sellQty2 = tmpQty;
                                                break;
                                            case 2:
                                                sellPrice3 = tmpPrice;
                                                sellQty3 = tmpQty;
                                                break;
                                            case 3:
                                                sellPrice4 = tmpPrice;
                                                sellQty4 = tmpQty;
                                                break;
                                            case 4:
                                                sellPrice5 = tmpPrice;
                                                sellQty5 = tmpQty;
                                                break;
                                        }
                                    }

                                    product.BID1 = buyPrice1;
                                    product.BID2 = buyPrice2;
                                    product.BID3 = buyPrice3;
                                    product.BID4 = buyPrice4;
                                    product.BID5 = buyPrice5;
                                    product.BIDSiz1 = buyQty1;
                                    product.BIDSiz2 = buyQty2;
                                    product.BIDSiz3 = buyQty3;
                                    product.BIDSiz4 = buyQty4;
                                    product.BIDSiz5 = buyQty5;

                                    product.ASK1 = sellPrice1;
                                    product.ASK2 = sellPrice2;
                                    product.ASK3 = sellPrice3;
                                    product.ASK4 = sellPrice4;
                                    product.ASK5 = sellPrice5;
                                    product.ASKSiz1 = sellQty1;
                                    product.ASKSiz2 = sellQty2;
                                    product.ASKSiz3 = sellQty3;
                                    product.ASKSiz4 = sellQty4;
                                    product.ASKSiz5 = sellQty5;

                                    dicData[productID].Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}"
                                               , pcapTime, product.Last, product.LastQty
                                               , product.BID1, product.BIDSiz1
                                               , product.BID2, product.BIDSiz2
                                               , product.BID3, product.BIDSiz3
                                               , product.BID4, product.BIDSiz4
                                               , product.BID5, product.BIDSiz5
                                               , product.ASK1, product.ASKSiz1
                                               , product.ASK2, product.ASKSiz2
                                               , product.ASK3, product.ASKSiz3
                                               , product.ASK4, product.ASKSiz4
                                               , product.ASK5, product.ASKSiz5
                                               , "0"
                                               , product.Volume
                                               , informationTime, ""));

                                }
                                #endregion
                            }
                        }
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


            foreach (var pd in dicData)
            {
                var log = LogManager.GetLogger(pd.Key.Replace('/', '-'));
                foreach (var data in pd.Value)
                {
                    log.Info(data);
                }
            }
        }


        /// <summary>
        /// 對應IP 225.0.140.140
        /// </summary>
        public void FutureCT(List<PcpaData> pcpaDataList)
        {
            //Dictionary<string, Product> dicp = new Dictionary<string, Product>();
            //Dictionary<string, List<string>> dicData = new Dictionary<string, List<string>>();

            //Dictionary<string, FutureProduct> dic = new Dictionary<string, FutureProduct>();

            //MDInstrumentDefinitionFuture10 mDInstrumentDefinitionFuture10 = new MDInstrumentDefinitionFuture10(dic);
            //MDIncrementalRefreshTradeSummary24 mDIncrementalRefreshTradeSummary24 = new MDIncrementalRefreshTradeSummary24(dic);
            //MDIncrementalRefreshBook81 mDIncrementalRefreshBook81 = new MDIncrementalRefreshBook81(dic);
            //MDIncrementalFullRefreshBook83 mDIncrementalFullRefreshBook83 = new MDIncrementalFullRefreshBook83(dic);
            
            
            //foreach (var item in pcpaDataList)
            //{
            //    int offset = 0;
            //    var bytes = item.Bytes;
            //    var pcapTime = item.PcapTime;

            //    if (bytes[offset] == 27)
            //    {
            //        int Len = ((bytes[offset + 17] & 0xf) + ((bytes[offset + 17] >> 4) & 0xF) * 10) * 100
            //                + ((bytes[offset + 18] & 0xf) + ((bytes[offset + 18] >> 4) & 0xF) * 10) + 22;

            //        byte[] buffer = new byte[Len];
            //        Array.Copy(bytes, offset, buffer, 0, Len);
            //        DirectBuffer directbuffer = new DirectBuffer(buffer);

            //        //五檔
            //        if ((buffer[1] == BCD.Hex_2 || buffer[1] == BCD.Hex_5) && buffer[2] == BCD.Hex_A)
            //        {
            //            mDIncrementalRefreshBook81.WaroForDecode(directbuffer);
            //            if (dic.ContainsKey(mDIncrementalRefreshBook81.PROD_ID) == false)
            //            {
            //                FutureProduct futures = new FutureProduct() { Symbol = mDIncrementalRefreshBook81.PROD_ID, Product = new Futures(), DecimalLocator = 0.01m };
            //                dic.Add(futures.Symbol, futures);
            //                List<string> list = new List<string>();
            //                list.Add("PcapTime,Last,Vol,BID1,BIDSZ1,BID2,BIDSZ2,BID3,BIDSZ3,BID4,BIDSZ4,BID5,BIDSZ5,ASK1,ASKSZ1,ASK2,ASKSZ2,ASK3,ASKSZ3,ASK4,ASKSZ4,ASK5,ASKSZ5,Tick,Volume,Time,LastTime");
            //                dicData.Add(mDIncrementalRefreshBook81.PROD_ID, list);
            //                dicp.Add(mDIncrementalRefreshBook81.PROD_ID, new Product());
            //            }

            //            mDIncrementalRefreshBook81.Update();
            //            ITaifexPrice product = dic[mDIncrementalRefreshBook81.PROD_ID];
            //            dicData[mDIncrementalRefreshBook81.PROD_ID].Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}"
            //                              , pcapTime
            //                              , product.Last, product.LastSiz
            //                              , product.BID_1, product.BIDSiz_1
            //                              , product.BID_2, product.BIDSiz_2
            //                              , product.BID_3, product.BIDSiz_3
            //                              , product.BID_4, product.BIDSiz_4
            //                              , product.BID_5, product.BIDSiz_5
            //                              , product.ASK_1, product.ASKSiz_1
            //                              , product.ASK_2, product.ASKSiz_2
            //                              , product.ASK_3, product.ASKSiz_3
            //                              , product.ASK_4, product.ASKSiz_4
            //                              , product.ASK_5, product.ASKSiz_5
            //                              , "0"
            //                              , product.Volume
            //                              , mDIncrementalRefreshBook81.INFORMATION_TIME, ""));
            //        }
            //        //成交
            //        else if ((buffer[1] == BCD.Hex_2 || buffer[1] == BCD.Hex_5) && buffer[2] == BCD.Hex_D)
            //        {
            //            mDIncrementalRefreshTradeSummary24.WaroForDecode(directbuffer);

            //            if (dic.ContainsKey(mDIncrementalRefreshTradeSummary24.PROD_ID) == false)
            //            {
            //                FutureProduct futures = new FutureProduct() { Symbol = mDIncrementalRefreshTradeSummary24.PROD_ID, Product = new Futures(), DecimalLocator = 0.01m };
            //                dic.Add(futures.Symbol, futures);
            //                List<string> list = new List<string>();
            //                list.Add("PcapTime,Last,Vol,BID1,BIDSZ1,BID2,BIDSZ2,BID3,BIDSZ3,BID4,BIDSZ4,BID5,BIDSZ5,ASK1,ASKSZ1,ASK2,ASKSZ2,ASK3,ASKSZ3,ASK4,ASKSZ4,ASK5,ASKSZ5,Tick,Volume,Time,LastTime");
            //                dicData.Add(mDIncrementalRefreshTradeSummary24.PROD_ID, list);
            //                dicp.Add(mDIncrementalRefreshTradeSummary24.PROD_ID, new Product());
            //            }

            //            mDIncrementalRefreshTradeSummary24.Update();
            //            ITaifexPrice product = dic[mDIncrementalRefreshTradeSummary24.PROD_ID];
            //            dicData[mDIncrementalRefreshTradeSummary24.PROD_ID].Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}"
            //                              , pcapTime
            //                              , product.Last, product.LastSiz
            //                              , product.BID_1, product.BIDSiz_1
            //                              , product.BID_2, product.BIDSiz_2
            //                              , product.BID_3, product.BIDSiz_3
            //                              , product.BID_4, product.BIDSiz_4
            //                              , product.BID_5, product.BIDSiz_5
            //                              , product.ASK_1, product.ASKSiz_1
            //                              , product.ASK_2, product.ASKSiz_2
            //                              , product.ASK_3, product.ASKSiz_3
            //                              , product.ASK_4, product.ASKSiz_4
            //                              , product.ASK_5, product.ASKSiz_5
            //                              , mDIncrementalRefreshTradeSummary24.CALCULATED_FLAG ? "0" : "1"
            //                              , product.Volume
            //                              , mDIncrementalRefreshTradeSummary24.INFORMATION_TIME, mDIncrementalRefreshTradeSummary24.MATCH_TIME));
            //        }
            //        else if ((buffer[1] == BCD.Hex_2 || buffer[1] == BCD.Hex_5) && buffer[2] == BCD.Hex_B)
            //        {
            //            mDIncrementalFullRefreshBook83.WaroForDecode(directbuffer);
            //            if (dic.ContainsKey(mDIncrementalFullRefreshBook83.PROD_ID) == false)
            //            {
            //                FutureProduct futures = new FutureProduct() { Symbol = mDIncrementalFullRefreshBook83.PROD_ID, Product = new Futures(), DecimalLocator = 0.01m };
            //                dic.Add(futures.Symbol, futures);
            //                List<string> list = new List<string>();
            //                list.Add("PcapTime,Last,Vol,BID1,BIDSZ1,BID2,BIDSZ2,BID3,BIDSZ3,BID4,BIDSZ4,BID5,BIDSZ5,ASK1,ASKSZ1,ASK2,ASKSZ2,ASK3,ASKSZ3,ASK4,ASKSZ4,ASK5,ASKSZ5,Tick,Volume,Time,LastTime");
            //                dicData.Add(mDIncrementalFullRefreshBook83.PROD_ID, list);
            //                dicp.Add(mDIncrementalFullRefreshBook83.PROD_ID, new Product());
            //            }
            //            mDIncrementalFullRefreshBook83.Update();
            //            ITaifexPrice product = dic[mDIncrementalFullRefreshBook83.PROD_ID];
            //            dicData[mDIncrementalFullRefreshBook83.PROD_ID].Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}"
            //                              , pcapTime
            //                              , product.Last, product.LastSiz
            //                              , product.BID_1, product.BIDSiz_1
            //                              , product.BID_2, product.BIDSiz_2
            //                              , product.BID_3, product.BIDSiz_3
            //                              , product.BID_4, product.BIDSiz_4
            //                              , product.BID_5, product.BIDSiz_5
            //                              , product.ASK_1, product.ASKSiz_1
            //                              , product.ASK_2, product.ASKSiz_2
            //                              , product.ASK_3, product.ASKSiz_3
            //                              , product.ASK_4, product.ASKSiz_4
            //                              , product.ASK_5, product.ASKSiz_5
            //                              , "0"
            //                              , product.Volume
            //                              , mDIncrementalFullRefreshBook83.INFORMATION_TIME, ""));
            //        }
            //        else if ((buffer[1] == BCD.Hex_1 || buffer[1] == BCD.Hex_4) && buffer[2] == BCD.Hex_1)
            //        {
            //            mDInstrumentDefinitionFuture10.WaroForDecode(directbuffer);
            //            if (dic.ContainsKey(mDInstrumentDefinitionFuture10.PROD_ID) == false)
            //            {
            //                FutureProduct futures = new FutureProduct() { Symbol = mDInstrumentDefinitionFuture10.PROD_ID, Product = new Futures(), DecimalLocator = 0.01m };
            //                dic.Add(futures.Symbol, futures);
            //                List<string> list = new List<string>();
            //                list.Add("PcapTime,Last,Vol,BID1,BIDSZ1,BID2,BIDSZ2,BID3,BIDSZ3,BID4,BIDSZ4,BID5,BIDSZ5,ASK1,ASKSZ1,ASK2,ASKSZ2,ASK3,ASKSZ3,ASK4,ASKSZ4,ASK5,ASKSZ5,Tick,Volume,Time,LastTime");
            //                dicData.Add(mDInstrumentDefinitionFuture10.PROD_ID, list);
            //                dicp.Add(mDInstrumentDefinitionFuture10.PROD_ID, new Product());
            //            }
            //            mDInstrumentDefinitionFuture10.Update();
            //        }
            //    }
            //}


            //foreach (var pd in dicData)
            //{
            //    var log = LogManager.GetLogger(pd.Key.Replace('/', '-'));
            //    foreach (var data in pd.Value)
            //    {
            //        log.Info(data);
            //    }
            //}
        }


        /// <summary>
        /// 對應IP 224.2.100.100
        /// </summary>
        public void TSEWarrant(List<PcpaData> pcpaDataList)
        {
            Dictionary<string, Product> dicp = new Dictionary<string, Product>();
            Dictionary<string, List<string>> dicData = new Dictionary<string, List<string>>();
            int LastSeq = 0;
            long lasttime = 0;

            WarrantBatch(pcpaDataList, dicData, dicp, ref LastSeq, ref lasttime);


            foreach (var pd in dicData)
            {
                var log = LogManager.GetLogger(pd.Key);
                foreach (var data in pd.Value)
                {
                    log.Info(data);
                }
            }
        }


        /// <summary>
        ///對應IP  224.2.100.100
        /// </summary>
        public void TSEWarrantTag(List<PcpaData> pcpaDataList, string date)
        {
            Dictionary<string, WarrantBase> dicWarrant = new Dictionary<string, WarrantBase>();
            Dictionary<string, string> dicName = new Dictionary<string, string>();
           
            WarrantBase(pcpaDataList, dicWarrant, dicName);


           
            Dictionary<string, string> diclog = new Dictionary<string, string>();
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
                    if (bytes[offset + 3] == 0x01 && bytes[offset + 4] == 0x14)
                    {
                        byte[] buf = new byte[Len];
                        Array.Copy(bytes, offset, buf, 0, Len);

                        //判斷第一碼是否為 ASCII 27
                        int asciiNumber = Convert.ToInt16(buf[0]);
                        if (asciiNumber != 27)
                        {
                            throw new Exception("傳入資料錯誤，第一碼需為 ASCII 27");
                        }
                        //判斷是否為 0114 封包
                        string transmissionCode = buf[3].ToString("X2"); // 01
                        string messageKind = buf[4].ToString("X2"); // 14
                        if (!(transmissionCode == "01" && messageKind == "14"))
                        {
                            throw new Exception("傳入資料錯誤，資料非0114格式");
                        }

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
                            string Broker = GetBrokerName(WarrantName);                                                        

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


        /// <summary>
        ///   224.2.30.30
        /// </summary>
        public void OTCWarrant(List<PcpaData> pcpaDataList)
        {
            Dictionary<string, Product> dicp = new Dictionary<string, Product>();
            Dictionary<string, List<string>> dicData = new Dictionary<string, List<string>>();
            int LastSeq = 0;
            long lasttime = 0;


            WarrantBatch(pcpaDataList, dicData, dicp, ref LastSeq, ref lasttime);

            

            foreach (var pd in dicData)
            {
                var log = LogManager.GetLogger(pd.Key);
                foreach (var data in pd.Value)
                {
                    log.Info(data);
                }
            }
        }


        /// <summary>
        ///   224.2.30.30
        /// </summary>
        public void OTCWarrantTag(List<PcpaData> pcpaDataList, string date)
        {                   
            Dictionary<string, WarrantBase> dicWarrant = new Dictionary<string, WarrantBase>();
            Dictionary<string, string> dicName = new Dictionary<string, string>();            

            OTCWarrantBase(pcpaDataList, dicWarrant, dicName);


            LogManager.Configuration.Variables["vardate"] = date;                      
            Dictionary<string, string> diclog = new Dictionary<string, string>();
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
                            string Broker = GetBrokerName(WarrantName);

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








        private void WarrantBase(List<PcpaData> pcpaDataList, Dictionary<string, WarrantBase> dicWarrant, Dictionary<string, string> dicName)
        {            
            foreach (var item in pcpaDataList)
            {
                int offset = 0;
                var bytes = item.Bytes;
                var pcapTime = item.PcapTime;
                if (bytes[offset] == 27)
                {
                    int Len = Convert.ToInt32(bytes[offset + 1].ToString("X2") + bytes[offset + 2].ToString("X2"));
                    if (bytes[offset + 3] == 0x01 && bytes[offset + 4] == 0x01)
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

                                string warrantFlag = Encoding.ASCII.GetString(buf, 63, 1).Trim(); //權證識別碼，紀錄值為 Y 時表示個股具有權證資料，SPACE時，權證資料欄位皆為0
                                                                                                  //    priceObject.warrantFlag = warrantFlag;
                                if (warrantFlag == "Y")
                                {
                                    pd.WarrantID = productID;

                                    pd.StrikePrice = buf[64].ToString("X2") + buf[65].ToString("X2") + buf[66].ToString("X2") + "." + buf[67].ToString("X2") + buf[68].ToString("X2");
                                    pd.Ratio = (Convert.ToDecimal(buf[84].ToString("X2") + buf[85].ToString("X2") + buf[86].ToString("X2") + "." + buf[87].ToString("X2")) / 1000).ToString(); //行使比率
                                    pd.Expiration = buf[98].ToString("X2") + buf[99].ToString("X2") + buf[100].ToString("X2") + buf[101].ToString("X2");//到期日
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


        private bool WarrantBatch(List<PcpaData> pcpaDataList, Dictionary<string, List<string>> dicData, Dictionary<string, Product> dicp, ref int LastSeq, ref long lasttime)
        {           
            foreach (var item in pcpaDataList)
            {
                int offset = 0;
                var bytes = item.Bytes;
                var pcapTime = item.PcapTime;

                if (bytes[offset] == 27)
                {
                    if (offset + 19 > bytes.Length)
                        break;

                    int Len = ((bytes[offset + 1] & 0xf) + ((bytes[offset + 1] >> 4) & 0xF) * 10) * 100
                         + ((bytes[offset + 2] & 0xf) + ((bytes[offset + 2] >> 4) & 0xF) * 10);

                    //double seq = Convert.ToInt32(bytes[offset + 6].ToString("X2") + bytes[offset + 7].ToString("X2") + bytes[offset + 8].ToString("X2") + bytes[offset + 9].ToString("X2"));
                    if ((bytes[offset + 3] == 0x01 || bytes[offset + 3] == 0x02) && bytes[offset + 4] == 0x17)
                    {
                        int seq = ((bytes[offset + 6] & 0xf) + ((bytes[offset + 6] >> 4) & 0xF) * 10) * 1000000
                                + ((bytes[offset + 7] & 0xf) + ((bytes[offset + 7] >> 4) & 0xF) * 10) * 10000
                                + ((bytes[offset + 8] & 0xf) + ((bytes[offset + 8] >> 4) & 0xF) * 10) * 100
                                + ((bytes[offset + 9] & 0xf) + ((bytes[offset + 9] >> 4) & 0xF) * 10);

                        if (LastSeq == 0)
                        {
                            LastSeq = seq;
                        }
                        else if (LastSeq >= seq)
                        {
                            offset += Len;
                            continue;
                        }
                        else
                        {
                            LastSeq = seq;
                        }

                        if (offset + Len > bytes.Length)
                            break;

                        byte[] buf = new byte[Len];
                        Array.Copy(bytes, offset, buf, 0, Len);
                        string stockID = Encoding.ASCII.GetString(buf, 10, 6).Trim(); //股票代號

                        if (stockID != "000000")
                        {
                            if (dicp.ContainsKey(stockID) == false)
                            {
                                dicp.Add(stockID, new Product());
                                List<string> list = new List<string>();
                                list.Add("PcapTime,Last,Vol,BID1,BIDSZ1,BID2,BIDSZ2,BID3,BIDSZ3,BID4,BIDSZ4,BID5,BIDSZ5,ASK1,ASKSZ1,ASK2,ASKSZ2,ASK3,ASKSZ3,ASK4,ASKSZ4,ASK5,ASKSZ5,Tick,Volume,Time,LastTime");
                                dicData.Add(stockID, list);
                            }
                            var product = dicp[stockID];

                            string dataTime = bytes[offset + 16].ToString("X2") + bytes[offset + 17].ToString("X2") + bytes[offset + 18].ToString("X2") + bytes[offset + 19].ToString("X2") + bytes[offset + 20].ToString("X2") + bytes[offset + 21].ToString("X2");

                            if (lasttime < Convert.ToInt64(dataTime))
                            {
                                lasttime = Convert.ToInt64(dataTime);
                            }
                            else if (lasttime > Convert.ToInt64(dataTime) + 10000000000)
                            {
                                return false;
                            }

                            int bit0 = ((bytes[offset + 22] & 1) == 0) ? 0 : 1;
                            int bit1 = ((bytes[offset + 22] & 2) == 0) ? 0 : 1;
                            int bit2 = ((bytes[offset + 22] & 4) == 0) ? 0 : 1;
                            int bit3 = ((bytes[offset + 22] & 8) == 0) ? 0 : 1;
                            int bit4 = ((bytes[offset + 22] & 16) == 0) ? 0 : 1;
                            int bit5 = ((bytes[offset + 22] & 32) == 0) ? 0 : 1;
                            int bit6 = ((bytes[offset + 22] & 64) == 0) ? 0 : 1;
                            int bit7 = ((bytes[offset + 22] & 128) == 0) ? 0 : 1;

                            int buyCount = 0;
                            int sellCount = 0;
                            int dealCount = 0;

                            if (bit4 == 1)
                            {
                                buyCount = 1;
                            }
                            if (bit5 == 1)
                            {
                                buyCount = buyCount + 2;
                            }
                            if (bit6 == 1)
                            {
                                buyCount = buyCount + 4;
                            }

                            if (bit1 == 1)
                            {
                                sellCount = 1;
                            }
                            if (bit2 == 1)
                            {
                                sellCount = sellCount + 2;
                            }
                            if (bit3 == 1)
                            {
                                sellCount = sellCount + 4;
                            }

                            if (bit7 == 1)
                            {
                                dealCount = 1;
                            }
                            else
                            {
                                dealCount = 0;
                            }

                            int totalCount = dealCount + buyCount + sellCount;

                            int Volume = Convert.ToInt32(bytes[offset + 25].ToString("X2") + bytes[offset + 26].ToString("X2") + bytes[offset + 27].ToString("X2") + bytes[offset + 28].ToString("X2"));

                            decimal LastPrice = 0;
                            int LastQty = 0;
                            decimal BidPrice1 = 0;
                            decimal BidPrice2 = 0;
                            decimal BidPrice3 = 0;
                            decimal BidPrice4 = 0;
                            decimal BidPrice5 = 0;
                            int BidQty1 = 0;
                            int BidQty2 = 0;
                            int BidQty3 = 0;
                            int BidQty4 = 0;
                            int BidQty5 = 0;
                            decimal AskPrice1 = 0;
                            decimal AskPrice2 = 0;
                            decimal AskPrice3 = 0;
                            decimal AskPrice4 = 0;
                            decimal AskPrice5 = 0;
                            int AskQty1 = 0;
                            int AskQty2 = 0;
                            int AskQty3 = 0;
                            int AskQty4 = 0;
                            int AskQty5 = 0;

                            for (int i = 0; i < totalCount; i++)
                            {
                                decimal price = (((bytes[29 + offset + i * 9] & 0xf) + ((bytes[29 + offset + i * 9] >> 4) & 0xF) * 10) * 100000000 +
                                                 ((bytes[30 + offset + i * 9] & 0xf) + ((bytes[30 + offset + i * 9] >> 4) & 0xF) * 10) * 1000000 +
                                                 ((bytes[31 + offset + i * 9] & 0xf) + ((bytes[31 + offset + i * 9] >> 4) & 0xF) * 10) * 10000 +
                                                 ((bytes[32 + offset + i * 9] & 0xf) + ((bytes[32 + offset + i * 9] >> 4) & 0xF) * 10) * 100 +
                                                 ((bytes[33 + offset + i * 9] & 0xf) + ((bytes[33 + offset + i * 9] >> 4) & 0xF) * 10)) * 0.0001m;

                                int qty = ((bytes[34 + offset + i * 9] & 0xf) + ((bytes[34 + offset + i * 9] >> 4) & 0xF) * 10) * 1000000 +
                                          ((bytes[35 + offset + i * 9] & 0xf) + ((bytes[35 + offset + i * 9] >> 4) & 0xF) * 10) * 10000 +
                                          ((bytes[36 + offset + i * 9] & 0xf) + ((bytes[36 + offset + i * 9] >> 4) & 0xF) * 10) * 100 +
                                          ((bytes[37 + offset + i * 9] & 0xf) + ((bytes[37 + offset + i * 9] >> 4) & 0xF) * 10);

                                if (i < dealCount)
                                {
                                    LastPrice = price;
                                    LastQty = qty;
                                    product.Last = price;
                                    product.LastQty = qty;
                                }
                                else
                                {
                                    if (i >= dealCount && i < (dealCount + buyCount))
                                    {
                                        switch (i - dealCount)
                                        {
                                            case 0:
                                                BidPrice1 = price;
                                                BidQty1 = qty;
                                                break;
                                            case 1:
                                                BidPrice2 = price;
                                                BidQty2 = qty;
                                                break;
                                            case 2:
                                                BidPrice3 = price;
                                                BidQty3 = qty;
                                                break;
                                            case 3:
                                                BidPrice4 = price;
                                                BidQty4 = qty;
                                                break;
                                            case 4:
                                                BidPrice5 = price;
                                                BidQty5 = qty;
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        switch (i - dealCount - buyCount)
                                        {
                                            case 0:
                                                AskPrice1 = price;
                                                AskQty1 = qty;
                                                break;
                                            case 1:
                                                AskPrice2 = price;
                                                AskQty2 = qty;
                                                break;
                                            case 2:
                                                AskPrice3 = price;
                                                AskQty3 = qty;
                                                break;
                                            case 3:
                                                AskPrice4 = price;
                                                AskQty4 = qty;
                                                break;
                                            case 4:
                                                AskPrice5 = price;
                                                AskQty5 = qty;
                                                break;
                                        }
                                    }
                                }
                            }

                            product.BID1 = BidPrice1;
                            product.BID2 = BidPrice2;
                            product.BID3 = BidPrice3;
                            product.BID4 = BidPrice4;
                            product.BID5 = BidPrice5;
                            product.BIDSiz1 = BidQty1;
                            product.BIDSiz2 = BidQty2;
                            product.BIDSiz3 = BidQty3;
                            product.BIDSiz4 = BidQty4;
                            product.BIDSiz5 = BidQty5;

                            product.ASK1 = AskPrice1;
                            product.ASK2 = AskPrice2;
                            product.ASK3 = AskPrice3;
                            product.ASK4 = AskPrice4;
                            product.ASK5 = AskPrice5;
                            product.ASKSiz1 = AskQty1;
                            product.ASKSiz2 = AskQty2;
                            product.ASKSiz3 = AskQty3;
                            product.ASKSiz4 = AskQty4;
                            product.ASKSiz5 = AskQty5;

                            if (LastQty > 0)
                            {
                                dicData[stockID].Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}"
                                      , pcapTime, product.Last, product.LastQty
                                      , product.BID1, product.BIDSiz1
                                      , product.BID2, product.BIDSiz2
                                      , product.BID3, product.BIDSiz3
                                      , product.BID4, product.BIDSiz4
                                      , product.BID5, product.BIDSiz5
                                      , product.ASK1, product.ASKSiz1
                                      , product.ASK2, product.ASKSiz2
                                      , product.ASK3, product.ASKSiz3
                                      , product.ASK4, product.ASKSiz4
                                      , product.ASK5, product.ASKSiz5
                                      , "1"
                                      , Volume
                                      , dataTime, dataTime));
                            }
                            else
                            {
                                dicData[stockID].Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}"
                                      , pcapTime, product.Last, product.LastQty
                                      , product.BID1, product.BIDSiz1
                                      , product.BID2, product.BIDSiz2
                                      , product.BID3, product.BIDSiz3
                                      , product.BID4, product.BIDSiz4
                                      , product.BID5, product.BIDSiz5
                                      , product.ASK1, product.ASKSiz1
                                      , product.ASK2, product.ASKSiz2
                                      , product.ASK3, product.ASKSiz3
                                      , product.ASK4, product.ASKSiz4
                                      , product.ASK5, product.ASKSiz5
                                      , "0"
                                      , Volume
                                      , dataTime, ""));
                            }
                        }
                    }

                }
                else
                {
                    //Console.WriteLine("解析錯誤");
                }
            }

            return true;
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

        private string GetBrokerName(string WarrantName)
        {
            string Broker = "";
            switch (WarrantName)
            {
                case string b when b.Contains("元大"):
                    Broker = "Yuanta";
                    break;
                case string b when b.Contains("凱基"):
                    Broker = "KGI";
                    break;
                case string b when b.Contains("富邦"):
                    Broker = "Fubon";
                    break;
                case string b when b.Contains("統一"):
                    Broker = "PSC";
                    break;
                case string b when b.Contains("永昌"):
                    Broker = "Entrust";
                    break;
                case string b when b.Contains("元富"):
                    Broker = "Masterlink";
                    break;
                case string b when b.Contains("兆豐"):
                    Broker = "Emega";
                    break;
                case string b when b.Contains("國泰"):
                    Broker = "Cathaysec";
                    break;
                case string b when b.Contains("第一"):
                    Broker = "Firstrade";
                    break;
                case string b when b.Contains("中信"):
                    Broker = "Win168";
                    break;
                case string b when b.Contains("群益"):
                    Broker = "Capital";
                    break;
                case string b when b.Contains("麥證"):
                    Broker = "Buywarrant";
                    break;
                case string b when b.Contains("國票"):
                    Broker = "WLS";
                    break;
                case string b when b.Contains("永豐"):
                    Broker = "Sinotrade";
                    break;
                case string b when b.Contains("台新"):
                    Broker = "Tssco";
                    break;
                case string b when b.Contains("日盛"):
                    Broker = "Jihsun";
                    break;
                case string b when b.Contains("元展"):
                    Broker = "Yuanta";
                    break;
                case string b when b.Contains("康和"):
                    Broker = "Concordfutures";
                    break;
                case string b when b.Contains("宏遠"):
                    Broker = "Honsec";
                    break;
                case string b when b.Contains("亞東"):
                    Broker = "OSC";
                    break;
                case string b when b.Contains("玉山"):
                    Broker = "Esunsec";
                    break;
                //case string b when b.Contains("富展"):
                //    Broker = "Fubon";
                //    break;
                default:
                    break;
            }

            return Broker;
        }
    }
}
