﻿using TaifexLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcapDissectorWPF
{
    public class Product
    {
        public decimal Last;
        public int LastQty;
        public int Volume;

        public decimal BID1;
        public decimal BID2;
        public decimal BID3;
        public decimal BID4;
        public decimal BID5;

        public int BIDSiz1;
        public int BIDSiz2;
        public int BIDSiz3;
        public int BIDSiz4;
        public int BIDSiz5;

        public decimal ASK1;
        public decimal ASK2;
        public decimal ASK3;
        public decimal ASK4;
        public decimal ASK5;

        public int ASKSiz1;
        public int ASKSiz2;
        public int ASKSiz3;
        public int ASKSiz4;
        public int ASKSiz5;

        public decimal DecimalLocator;
    }
    public class Futures : ITaifexPrice
    {
        public string Symbol { get; set; }
        public double BID_1 { get; set; }
        public double BID_2 { get; set; }
        public double BID_3 { get; set; }
        public double BID_4 { get; set; }
        public double BID_5 { get; set; }
        public double ImpliedBID { get; set; }
        public int BIDSiz_1 { get; set; }
        public int BIDSiz_2 { get; set; }
        public int BIDSiz_3 { get; set; }
        public int BIDSiz_4 { get; set; }
        public int BIDSiz_5 { get; set; }
        public int ImpliedBIDSiz { get; set; }
        public double ASK_1 { get; set; }
        public double ASK_2 { get; set; }
        public double ASK_3 { get; set; }
        public double ASK_4 { get; set; }
        public double ASK_5 { get; set; }
        public double ImpliedASK { get; set; }
        public int ASKSiz_1 { get; set; }
        public int ASKSiz_2 { get; set; }
        public int ASKSiz_3 { get; set; }
        public int ASKSiz_4 { get; set; }
        public int ASKSiz_5 { get; set; }
        public int ImpliedASKSiz { get; set; }
        public double Last { get; set; }
        public double Open { get; set; }
        public int LastSiz { get; set; }
        public double HighLimit { get; set; }
        public double LowLimit { get; set; }
        public double Yesterday { get; set; }
        public int Volume { get; set; }
        public decimal DecimalLocator { get; set; }
        public double doubleLocator { get; set; }
        public long UpdateTime { get; set; }
        public uint PROD_MSG_SEQ { get; set; }
        public long MATCH_TIME { get; set; }
        public bool CALCULATED_FLAG { get; set; }
        public bool IsBaseComplete { get; set; }
        public bool IsReFlashComplete { get; set; }
        public object _lock { get; set; }
        public void Update()
        {
            //throw new NotImplementedException();
        }
        public void Update(uint Seq)
        {
            //throw new NotImplementedException();
        }
    }
    public class WarrantBase
    {
        public string WarrantID { get; set; }
        public string StrikePrice { get; set; }
        public string Ratio { get; set; }
        public string Expiration { get; set; }
    }
}
