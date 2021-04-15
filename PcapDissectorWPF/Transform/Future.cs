using NLog;
using PcapDissectorWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcapDissectorWPF.Transform
{
    public class Future
    {
        Dictionary<string, decimal> dicPreClose = new Dictionary<string, decimal>();
        Dictionary<string, Product> dicp = new Dictionary<string, Product>();
        Dictionary<string, List<string>> dicData = new Dictionary<string, List<string>>();
        CommonLibrary.ILogger logger;

        public Future(CommonLibrary.ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// 對應IP 225.0.100.100
        /// </summary>
        public void Read(List<PcpaData> pcpaDataList)
        {
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
            catch (Exception ex)
            {
                throw;
            }
        }



        public void Save()
        {
            LogManager.Configuration.Variables["varmarket"] = "Future";
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
    }
}
