using NLog;
using PcapDissectorWPF.Models;
using TaifexLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcapDissectorWPF.Transform
{
    public class FutureCT
    {
        CommonLibrary.ILogger logger;
        Dictionary<string, Product> dicp = new Dictionary<string, Product>();
        Dictionary<string, List<string>> dicData = new Dictionary<string, List<string>>();

        Dictionary<string, FutureProduct> dic = new Dictionary<string, FutureProduct>();

        public FutureCT(CommonLibrary.ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// 對應IP 225.0.140.140
        /// </summary>
        public void Read(List<PcpaData> pcpaDataList)
        {
            MDInstrumentDefinitionFuture10 mDInstrumentDefinitionFuture10 = new MDInstrumentDefinitionFuture10(dic);
            MDIncrementalRefreshTradeSummary24 mDIncrementalRefreshTradeSummary24 = new MDIncrementalRefreshTradeSummary24(dic, logger);
            MDIncrementalRefreshBook81 mDIncrementalRefreshBook81 = new MDIncrementalRefreshBook81(dic, logger);
            MDIncrementalFullRefreshBook83 mDIncrementalFullRefreshBook83 = new MDIncrementalFullRefreshBook83(dic, logger);
            foreach (var item in pcpaDataList)
            {
                int offset = 0;
                var bytes = item.Bytes;
                var pcapTime = item.PcapTime;

                if (bytes[offset] == 27)
                {
                    int Len = ((bytes[offset + 17] & 0xf) + ((bytes[offset + 17] >> 4) & 0xF) * 10) * 100
                            + ((bytes[offset + 18] & 0xf) + ((bytes[offset + 18] >> 4) & 0xF) * 10) + 22;

                    byte[] buffer = new byte[Len];
                    Array.Copy(bytes, offset, buffer, 0, Len);
                    DirectBuffer directbuffer = new DirectBuffer(buffer);

                    //五檔
                    if ((buffer[1] == BCD.Hex_2 || buffer[1] == BCD.Hex_5) && buffer[2] == BCD.Hex_A)
                    {
                        mDIncrementalRefreshBook81.WaroForDecode(directbuffer);
                        if (dic.ContainsKey(mDIncrementalRefreshBook81.PROD_ID) == false)
                        {
                            FutureProduct futures = new FutureProduct() { Symbol = mDIncrementalRefreshBook81.PROD_ID, Product = new Futures(), DecimalLocator = 0.01m };
                            dic.Add(futures.Symbol, futures);
                            List<string> list = new List<string>();
                            list.Add("PcapTime,Last,Vol,BID1,BIDSZ1,BID2,BIDSZ2,BID3,BIDSZ3,BID4,BIDSZ4,BID5,BIDSZ5,ASK1,ASKSZ1,ASK2,ASKSZ2,ASK3,ASKSZ3,ASK4,ASKSZ4,ASK5,ASKSZ5,Tick,Volume,Time,LastTime");
                            dicData.Add(mDIncrementalRefreshBook81.PROD_ID, list);
                            dicp.Add(mDIncrementalRefreshBook81.PROD_ID, new Product());
                        }

                        mDIncrementalRefreshBook81.Update();
                        ITaifexPrice product = dic[mDIncrementalRefreshBook81.PROD_ID];
                        dicData[mDIncrementalRefreshBook81.PROD_ID].Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}"
                                          , pcapTime
                                          , product.Last, product.LastSiz
                                          , product.BID_1, product.BIDSiz_1
                                          , product.BID_2, product.BIDSiz_2
                                          , product.BID_3, product.BIDSiz_3
                                          , product.BID_4, product.BIDSiz_4
                                          , product.BID_5, product.BIDSiz_5
                                          , product.ASK_1, product.ASKSiz_1
                                          , product.ASK_2, product.ASKSiz_2
                                          , product.ASK_3, product.ASKSiz_3
                                          , product.ASK_4, product.ASKSiz_4
                                          , product.ASK_5, product.ASKSiz_5
                                          , "0"
                                          , product.Volume
                                          , mDIncrementalRefreshBook81.INFORMATION_TIME, ""));
                    }
                    //成交
                    else if ((buffer[1] == BCD.Hex_2 || buffer[1] == BCD.Hex_5) && buffer[2] == BCD.Hex_D)
                    {
                        mDIncrementalRefreshTradeSummary24.WaroForDecode(directbuffer);

                        if (dic.ContainsKey(mDIncrementalRefreshTradeSummary24.PROD_ID) == false)
                        {
                            FutureProduct futures = new FutureProduct() { Symbol = mDIncrementalRefreshTradeSummary24.PROD_ID, Product = new Futures(), DecimalLocator = 0.01m };
                            dic.Add(futures.Symbol, futures);
                            List<string> list = new List<string>();
                            list.Add("PcapTime,Last,Vol,BID1,BIDSZ1,BID2,BIDSZ2,BID3,BIDSZ3,BID4,BIDSZ4,BID5,BIDSZ5,ASK1,ASKSZ1,ASK2,ASKSZ2,ASK3,ASKSZ3,ASK4,ASKSZ4,ASK5,ASKSZ5,Tick,Volume,Time,LastTime");
                            dicData.Add(mDIncrementalRefreshTradeSummary24.PROD_ID, list);
                            dicp.Add(mDIncrementalRefreshTradeSummary24.PROD_ID, new Product());
                        }

                        mDIncrementalRefreshTradeSummary24.Update();
                        ITaifexPrice product = dic[mDIncrementalRefreshTradeSummary24.PROD_ID];
                        dicData[mDIncrementalRefreshTradeSummary24.PROD_ID].Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}"
                                          , pcapTime
                                          , product.Last, product.LastSiz
                                          , product.BID_1, product.BIDSiz_1
                                          , product.BID_2, product.BIDSiz_2
                                          , product.BID_3, product.BIDSiz_3
                                          , product.BID_4, product.BIDSiz_4
                                          , product.BID_5, product.BIDSiz_5
                                          , product.ASK_1, product.ASKSiz_1
                                          , product.ASK_2, product.ASKSiz_2
                                          , product.ASK_3, product.ASKSiz_3
                                          , product.ASK_4, product.ASKSiz_4
                                          , product.ASK_5, product.ASKSiz_5
                                          , mDIncrementalRefreshTradeSummary24.CALCULATED_FLAG ? "0" : "1"
                                          , product.Volume
                                          , mDIncrementalRefreshTradeSummary24.INFORMATION_TIME, mDIncrementalRefreshTradeSummary24.MATCH_TIME));
                    }
                    else if ((buffer[1] == BCD.Hex_2 || buffer[1] == BCD.Hex_5) && buffer[2] == BCD.Hex_B)
                    {
                        mDIncrementalFullRefreshBook83.WaroForDecode(directbuffer);
                        if (dic.ContainsKey(mDIncrementalFullRefreshBook83.PROD_ID) == false)
                        {
                            FutureProduct futures = new FutureProduct() { Symbol = mDIncrementalFullRefreshBook83.PROD_ID, Product = new Futures(), DecimalLocator = 0.01m };
                            dic.Add(futures.Symbol, futures);
                            List<string> list = new List<string>();
                            list.Add("PcapTime,Last,Vol,BID1,BIDSZ1,BID2,BIDSZ2,BID3,BIDSZ3,BID4,BIDSZ4,BID5,BIDSZ5,ASK1,ASKSZ1,ASK2,ASKSZ2,ASK3,ASKSZ3,ASK4,ASKSZ4,ASK5,ASKSZ5,Tick,Volume,Time,LastTime");
                            dicData.Add(mDIncrementalFullRefreshBook83.PROD_ID, list);
                            dicp.Add(mDIncrementalFullRefreshBook83.PROD_ID, new Product());
                        }
                        mDIncrementalFullRefreshBook83.Update();
                        ITaifexPrice product = dic[mDIncrementalFullRefreshBook83.PROD_ID];
                        dicData[mDIncrementalFullRefreshBook83.PROD_ID].Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}"
                                          , pcapTime
                                          , product.Last, product.LastSiz
                                          , product.BID_1, product.BIDSiz_1
                                          , product.BID_2, product.BIDSiz_2
                                          , product.BID_3, product.BIDSiz_3
                                          , product.BID_4, product.BIDSiz_4
                                          , product.BID_5, product.BIDSiz_5
                                          , product.ASK_1, product.ASKSiz_1
                                          , product.ASK_2, product.ASKSiz_2
                                          , product.ASK_3, product.ASKSiz_3
                                          , product.ASK_4, product.ASKSiz_4
                                          , product.ASK_5, product.ASKSiz_5
                                          , "0"
                                          , product.Volume
                                          , mDIncrementalFullRefreshBook83.INFORMATION_TIME, ""));
                    }
                    else if ((buffer[1] == BCD.Hex_1 || buffer[1] == BCD.Hex_4) && buffer[2] == BCD.Hex_1)
                    {
                        mDInstrumentDefinitionFuture10.WaroForDecode(directbuffer);
                        if (dic.ContainsKey(mDInstrumentDefinitionFuture10.PROD_ID) == false)
                        {
                            FutureProduct futures = new FutureProduct() { Symbol = mDInstrumentDefinitionFuture10.PROD_ID, Product = new Futures(), DecimalLocator = 0.01m };
                            dic.Add(futures.Symbol, futures);
                            List<string> list = new List<string>();
                            list.Add("PcapTime,Last,Vol,BID1,BIDSZ1,BID2,BIDSZ2,BID3,BIDSZ3,BID4,BIDSZ4,BID5,BIDSZ5,ASK1,ASKSZ1,ASK2,ASKSZ2,ASK3,ASKSZ3,ASK4,ASKSZ4,ASK5,ASKSZ5,Tick,Volume,Time,LastTime");
                            dicData.Add(mDInstrumentDefinitionFuture10.PROD_ID, list);
                            dicp.Add(mDInstrumentDefinitionFuture10.PROD_ID, new Product());
                        }
                        mDInstrumentDefinitionFuture10.Update();
                    }
                }
            }
        }


        public void Save()
        {
            LogManager.Configuration.Variables["varmarket"] = "FutureCT";
            foreach (var pd in dicData)
            {
                var log = LogManager.GetLogger(pd.Key.Replace('/', '-'));
                foreach (var data in pd.Value)
                {
                    log.Info(data);
                }
            }
        }
    }
}
