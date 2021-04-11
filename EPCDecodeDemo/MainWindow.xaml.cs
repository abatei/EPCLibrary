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
using EPCLib;

namespace EPCDecodeDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        EPCDecode decode = new EPCDecode("EPC_Binary_Coding_Table.xml", "Partition_Table.xml");
        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            byte[] binEncoding = StringToBytes(txtBinEncoding.Text);
            decode.BinCode = binEncoding;
            string pureURI = null;
            decode.Decode(ref pureURI);
            txtPureURI.Text = pureURI;
            txtFilter.Text = decode.Filter.ToString();
            txtTagScheme.Text = decode.TagScheme;
        }

        /// <summary>
        /// 将字符串转换为字节数组，要求各字节间使用空格分隔
        /// </summary>
        /// <param name="byteString">表示字节数组的字符串</param>
        /// <returns>成功返回转换后的字节数组，失败返回null</returns>
        private byte[] StringToBytes(string byteString)
        {
            if (byteString == "") return null;
            string[] strs = byteString.Split(' ');
            byte[] bytes = new byte[strs.Length];
            for (int i = 0; i < strs.Length; i++)
            {
                bytes[i] = Convert.ToByte(strs[i], 16);
            }
            return bytes;
        }
    }
}
