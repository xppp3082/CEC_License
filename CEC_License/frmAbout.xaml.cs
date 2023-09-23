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
using System.Reflection;
using CECLicense;

namespace CEC_License
{
    /// <summary>
    /// frmAbout.xaml 的互動邏輯
    /// </summary>
    public partial class frmAbout :Window
    {
        string TargetPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins";
        public frmAbout()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void frmAbout_Load(object sender, RoutedEventArgs e)
        {
            lbProductID.Text = ComputerInfo.GetComputerId();
            KeyManager km = new KeyManager(lbProductID.Text);
            LicenseInfo lic = new LicenseInfo();
            Assembly app = Assembly.GetExecutingAssembly();
            AssemblyName assemName = app.GetName();//讀取Revit版本
            //讀取CEC_RevitAPI版本
            string strResult = assemName.Version.Major.ToString() + "." + assemName.Version.Minor.ToString() + "." + assemName.Version.Build.ToString();
            int value = km.LoadSuretyFile(string.Format(@"{0}\CEClicense.lic", TargetPath), ref lic);
            string productKey = lic.ProductKey;
            if (km.ValidKey(ref productKey))
            {
                KeyValuesClass kv = new KeyValuesClass();
                if (km.DisassembleKey(productKey, ref kv))
                {
                    lbProductName.Text = "CEC-API(V." + strResult + ")";
                    lbProductKey.Text = productKey;
                    if (kv.Type == LicenseType.TRIAL)
                    {
                        lblicenseType.Text = string.Format("使用期還有 {0}天", (kv.Expiration - DateTime.Now.Date).Days);
                    }
                    else
                    {
                        lblicenseType.Text = "已授權";

                    }
                }
            }
            else
            {
                MessageBox.Show("授權無效", "錯誤訊息");
                this.Close();
            }
        }
    }
}
