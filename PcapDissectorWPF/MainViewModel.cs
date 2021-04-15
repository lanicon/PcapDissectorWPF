using NLog;
using PcapngUtils;
using PcapngUtils.Common;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using PcapDissectorWPF.Models;

namespace PcapDissectorWPF
{
    public class MainViewModel : NotifyPropertyBase
    {
        public static bool Runing = true;
        private static Dictionary<string, List<PcpaData>> DicPcapData = new Dictionary<string, List<PcpaData>>();
        private static ConcurrentQueue<string> HomeLogQueue = new ConcurrentQueue<string>();
        private static ConcurrentQueue<string> SystemLogQueue = new ConcurrentQueue<string>();
        private static CommonLibrary.ILogger _Logger;

        private Transform.TSE _TSE = new Transform.TSE(_Logger);
        private Transform.TSEPreClose _TSEPreClose = new Transform.TSEPreClose(_Logger);
        private Transform.TSEODD _TSEODD = new Transform.TSEODD(_Logger);
        private Transform.OTC _OTC = new Transform.OTC(_Logger);
        private Transform.OTCPreClose _OTCPreClose = new Transform.OTCPreClose(_Logger);
        private Transform.OTCODD _OTCODD = new Transform.OTCODD(_Logger);
        private Transform.TSEWarrant _TSEWarrant = new Transform.TSEWarrant(_Logger);
        private Transform.TSEWarrantTag _TSEWarrantTag = new Transform.TSEWarrantTag(_Logger);
        private Transform.OTCWarrant _OTCWarrant = new Transform.OTCWarrant(_Logger);
        private Transform.OTCWarrantTag _OTCWarrantTag = new Transform.OTCWarrantTag(_Logger);
        private Transform.Future _Future = new Transform.Future(_Logger);
        private Transform.FuturePreClose _FuturePreClose = new Transform.FuturePreClose(_Logger);
        private Transform.FutureCT _FutureCT = new Transform.FutureCT(_Logger);
        private Transform.Option _Option = new Transform.Option(_Logger);


        public MainViewModel()
        {
            HomeLog = new ObservableCollection<string>();
            SystemLog = new ObservableCollection<string>();

            BindingOperations.EnableCollectionSynchronization(HomeLog, new object());
            BindingOperations.EnableCollectionSynchronization(SystemLog, new object());

            AddSystemLog("System Running!");
            PrcessLog();
        }

        public void AddHomeLog(string msg)
        {
            StringBuilder sb = new StringBuilder();
            var nowDate = DateTime.UtcNow.AddHours(8).ToString("HH:mm:ss");
            sb.Append(nowDate);
            sb.Append("->");
            sb.Append(msg);
            HomeLogQueue.Enqueue(sb.ToString());
        }

        public void AddSystemLog(string msg)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DateTime.UtcNow.AddHours(8).ToString("HH:mm:ss"));
            sb.Append("->");
            sb.Append(msg);
            SystemLogQueue.Enqueue(sb.ToString());
        }


        public void Start()
        {
            Thread thr = new Thread(ReadPcap);
            thr.Start();
        }

        /// <summary>
        /// 開始讀取Pcap
        /// </summary>
        private void ReadPcap()
        {
            try
            {
                AddHomeLog("開始解析Pcap...");
                CancellationTokenSource cts = new CancellationTokenSource();
                LogManager.Configuration.Variables["vardate"] = FolderDate;


                var index = 0;
                var dicFilenames = SortFileName(System.IO.Directory.GetFiles(SourceFolder).ToList());
                foreach (var item in dicFilenames)
                {
                    index = 0;
                    AddHomeLog("解析" + item.Key + "中...");

                    foreach (var fname in item.Value)
                    {
                        if (fname.IndexOf("p3p1.pcap") == -1)
                            continue;

                        //先把pcap檔讀取到記憶體
                        using (var reader = IReaderFactory.GetReader(fname))
                        {
                            reader.OnReadPacketEvent += reader_OnReadPacketEvent;
                            reader.ReadPackets(cts.Token);
                            reader.OnReadPacketEvent -= reader_OnReadPacketEvent;
                        }

                        //開始解析pcap                        
                        foreach (var itemPcap in DicPcapData)
                        {
                            ReadPcapData(itemPcap.Key, itemPcap.Value, FolderDate);
                        }

                        //清空DicPcapData，避免佔用太多記憶體
                        DicPcapData = new Dictionary<string, List<PcpaData>>();

                        index++;
                        if (index % 20 == 0)
                        {
                            var percent = (Convert.ToDouble(index) / Convert.ToDouble(item.Value.Count())).ToString("P");
                            AddHomeLog(item.Key + ":" + percent);
                        }
                        else if (index == item.Value.Count())
                        {
                            AddHomeLog(item.Key + ":" + "100%");
                        }
                    }
                }

                //各類型檔案開始儲存
                _TSE.Save();
                _TSEPreClose.Save(FolderDate);
                _TSEODD.Save();
                _OTC.Save();
                _OTCPreClose.Save(FolderDate);
                _OTCODD.Save();
                _TSEWarrant.Save();
                _OTCWarrant.Save();
                _Future.Save();
                _FuturePreClose.Save(FolderDate);
                _FutureCT.Save();
                _Option.Save();


                AddHomeLog("...Pcap解析完成!");
            }
            catch (Exception ex)
            {
                AddSystemLog("Error!" + ex.Message + "\nStackTrace:" + ex.StackTrace);
            }
        }


        /// <summary>
        /// 先把pcap檔讀取到記憶體
        /// </summary>
        /// <param name="context"></param>
        /// <param name="packet"></param>
        private void reader_OnReadPacketEvent(object context, IPacket packet)
        {
            string Source_Ip = packet.Data[26] + "." + packet.Data[27] + "." + packet.Data[28] + "." + packet.Data[29];
            string Destination_Ip = packet.Data[30] + "." + packet.Data[31] + "." + packet.Data[32] + "." + packet.Data[33];

            //實際封包傳輸內容
            byte[] body = new byte[packet.Data.Length - 42];

            //取出pcapTime
            Array.Copy(packet.Data, 42, body, 0, body.Length);
            DateTime dt = (new DateTime(1970, 1, 1, 0, 0, 0)).AddHours(8).AddSeconds(packet.Seconds);
            var pcapTime = dt.ToString("HHmmss");
            if (packet.Microseconds != 0)
            {
                pcapTime = dt.ToString("HHmmss") + packet.Microseconds;
            }


            var obj = new PcpaData()
            {
                PcapTime = pcapTime,
                Bytes = body
            };
            if (DicPcapData.ContainsKey(Destination_Ip))
            {
                var item = DicPcapData[Destination_Ip];
                item.Add(obj);
            }
            else
            {
                DicPcapData.Add(Destination_Ip, new List<PcpaData>());
                var item = DicPcapData[Destination_Ip];
                item.Add(obj);
            }
        }


        /// <summary>
        /// 開始解析pcap
        /// </summary>
        /// <param name="Destination_Ip"></param>
        /// <param name="pcpaData"></param>
        private void ReadPcapData(string Destination_Ip, List<PcpaData> pcpaDataList, string date)
        {
            //TSE = 上市
            //OTC = 上櫃

            switch (Destination_Ip)
            {
                case "224.0.100.100":
                    {
                        _TSE.Read(pcpaDataList);

                        string FileName = @".\YesterdayPrice\" + date + "\\TSE.csv";
                        if (System.IO.File.Exists(FileName) == false)
                        {
                            _TSEPreClose.Read(pcpaDataList, Convert.ToInt32(date));
                        }
                    }
                    break;
                case "224.8.100.100":
                    _TSEODD.Read(pcpaDataList);
                    break;
                case "224.0.30.30":
                    {
                        _OTC.Read(pcpaDataList);

                        string FileName = @".\YesterdayPrice\" + date + "\\OTC.csv";
                        if (System.IO.File.Exists(FileName) == false)
                        {
                            _OTCPreClose.Read(pcpaDataList, Convert.ToInt32(date));
                        }
                    }
                    break;
                case "224.8.30.30":
                    _OTCODD.Read(pcpaDataList);
                    break;
                case "224.2.100.100":
                    _TSEWarrant.Read(pcpaDataList);
                    _TSEWarrantTag.Read(pcpaDataList, date);
                    break;
                case "224.2.30.30":
                    _OTCWarrant.Read(pcpaDataList);
                    _OTCWarrantTag.Read(pcpaDataList, date);
                    break;
                case "225.0.100.100":
                    {
                        _Future.Read(pcpaDataList);

                        string FileName = @".\YesterdayPrice\" + date + "\\Future.csv";
                        if (System.IO.File.Exists(FileName) == false)
                        {
                            _FuturePreClose.Read(pcpaDataList, Convert.ToInt32(date));
                        }
                    }
                    break;
                case "225.0.30.30":
                    _Option.Read(pcpaDataList);
                    break;
                case "225.0.140.140":
                    _FutureCT.Read(pcpaDataList);
                    break;
                default:
                    break;
            }
        }










        #region Private Method

        private void ErrorEvent(object sender, string message)
        {
            AddSystemLog(message);
        }

        private void MessageEvent(object sender, string message)
        {
            AddHomeLog(message);
        }

        private void PrcessLog()
        {
            new Thread(() =>
            {
                while (Runing)
                {
                    if (SystemLogQueue.Count > 0)
                    {
                        SystemLogQueue.TryDequeue(out string msg);
                        SystemLog.Insert(0, msg);
                    }
                    if (HomeLogQueue.Count > 0)
                    {
                        HomeLogQueue.TryDequeue(out string msg);
                        HomeLog.Insert(0, msg);
                    }
                    //Thread.Sleep(100);
                }
            })
            { IsBackground = true }.Start();
        }

        /// <summary>
        /// 檔名排序
        /// </summary>
        private Dictionary<string, List<string>> SortFileName(List<string> filenames)
        {
            var tempDic = new Dictionary<string, List<string>>();
            var headList = filenames.GroupBy(x => Path.GetFileNameWithoutExtension(x)).Select(x => x.Key).OrderBy(x => x).ToList();
            foreach (var head in headList)
            {
                var targetList = filenames.Where(x => x.Contains(head)).OrderBy(x => Convert.ToInt32(Path.GetExtension(x).Replace(".pcap", "0"))).ToList();
                tempDic.Add(head, targetList);
            }

            return tempDic;
        }

        #endregion

        #region Property

        private ObservableCollection<string> _HomeLog;
        public ObservableCollection<string> HomeLog
        {
            get { return _HomeLog; }
            set { SetProperty(ref _HomeLog, value); }
        }

        private ObservableCollection<string> _SystemLog;
        public ObservableCollection<string> SystemLog
        {
            get { return _SystemLog; }
            set { SetProperty(ref _SystemLog, value); }
        }

        private string _SourceFolder;
        /// <summary>
        /// 來源資料夾
        /// </summary>
        public string SourceFolder
        {
            get { return _SourceFolder; }
            set { SetProperty(ref _SourceFolder, value); }
        }

        private string _FolderDate;
        /// <summary>
        /// 資料夾日期
        /// </summary>
        public string FolderDate
        {
            get { return _FolderDate; }
            set { SetProperty(ref _FolderDate, value); }
        }


        #endregion
    }
}
