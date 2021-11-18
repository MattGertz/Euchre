using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CSEuchre4
{
    /// <summary>
    /// Interaction logic for EuchreRules.xaml
    /// </summary>
    public partial class EuchreRules : Window
    {
        public EuchreRules()
        {
            InitializeComponent();

            this.Loaded += EuchreRules_Load;
            this.CloseBtn.Click += CloseBtn_Click;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void EuchreRules_Load(object sender, RoutedEventArgs e)
        {
            EuchreTable.SetIcon(this, Properties.Resources.Euchre);
            ResizeMode = ResizeMode.CanMinimize;

            MemoryStream memStream = new MemoryStream(ASCIIEncoding.UTF8.GetBytes(Properties.Resources.CSEuchreRules));

            TextRange range;
            range = new TextRange(RtfRules.Document.ContentStart, RtfRules.Document.ContentEnd);
            range.Load(memStream, DataFormats.Rtf);
        }
    }
}
