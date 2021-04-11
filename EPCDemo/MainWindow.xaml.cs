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

namespace EPCDemo
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
        EPCEncode helper;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            helper = new EPCEncode("EPC_Binary_Coding_Table.xml", "Partition_Table.xml");
        }

        private void txtPureURI_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (helper == null) return;
            string[] tagSchemes = null;
            if (helper.SetURI(txtPureURI.Text, ref tagSchemes) == null)
            {
                cbTagScheme.Items.Clear();
                foreach (string scheme in tagSchemes)
                {
                    cbTagScheme.Items.Add(scheme);
                }
                cbTagScheme.SelectedIndex = 0;
            }
        }

        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            string tagURI = null;
            string binEncoding = null;
            string res = helper.GetTagURIAndBinEncoding(byte.Parse(cbFilter.Text),
                cbTagScheme.Text, ref tagURI, ref binEncoding);
            if (res == null)
            {
                tbMsg.Text = binEncoding;
            }
            else
            {
                tbMsg.Text = res;
            }
        }
    }
}
