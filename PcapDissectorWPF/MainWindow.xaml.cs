using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PcapDissectorWPF
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel _MainViewModel;
        public MainWindow()
        {
            InitializeComponent();

            _MainViewModel = new MainViewModel();
            DataContext = _MainViewModel;
        }

        private void Button_HomeGetFolder(object sender, RoutedEventArgs e)
        {
            try
            {
                var isError = true;
                var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                if (dialog.ShowDialog(this).GetValueOrDefault())
                {
                    var message = "所選資料夾為：" + dialog.SelectedPath + "。";
                    var pathArray = dialog.SelectedPath.Split('\\');
                    if (string.IsNullOrWhiteSpace(dialog.SelectedPath))
                    {
                        _MainViewModel.AddHomeLog(message + "請選擇資料夾路徑!");
                    }
                    else if (pathArray.Count() == 0)
                    {
                        _MainViewModel.AddHomeLog(message + "資料夾路徑異常!");
                    }
                    else if (!int.TryParse(pathArray[pathArray.Count() - 1], out _))
                    {
                        _MainViewModel.AddHomeLog(message + "資料夾最後一層路徑名稱錯誤! Ex:" + DateTime.Now.ToString("yyyyMMdd"));
                    }
                    else
                    {
                        isError = false;
                        _MainViewModel.SourceFolder = dialog.SelectedPath;
                        _MainViewModel.FolderDate = pathArray[pathArray.Count() - 1];
                        btnStart.Visibility = Visibility.Visible;
                    }
                }

                if (isError)
                {
                    btnStart.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                _MainViewModel.AddSystemLog(ex.Message);
            }
        }

        private void Button_StartProcess(object sender, RoutedEventArgs e)
        {
            _MainViewModel.Start();
        }
    }
}
