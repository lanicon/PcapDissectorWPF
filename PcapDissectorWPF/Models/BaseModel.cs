using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcapDissectorWPF.Models
{
    /// <summary>
    /// 存放Pcap檔資料
    /// </summary>
    public class PcpaData
    {
        public string PcapTime { set; get; }
        public byte[] Bytes { set; get; }
    }
}
