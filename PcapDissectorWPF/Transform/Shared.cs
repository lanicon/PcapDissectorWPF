using PcapDissectorWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcapDissectorWPF.Transform
{
    public class Shared
    {
        public bool WarrantBatch(List<PcpaData> pcpaDataList, Dictionary<string, List<string>> dicData, Dictionary<string, Product> dicp, ref int LastSeq, ref long lasttime)
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



        public string GetBrokerName(string WarrantName)
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
