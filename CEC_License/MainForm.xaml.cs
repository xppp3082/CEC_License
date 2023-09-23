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
using System.Windows.Forms;
using CECLicense;

namespace CEC_License
{
    /// <summary>
    /// MainForm.xaml 的互動邏輯
    /// </summary>
    public partial class MainForm : Window
    {
        //string TargetPath = @"C:\ProgramData\Autodesk\Revit\Addins";
        string TargetPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins";
        public MainForm()
        {
            InitializeComponent();
            //判斷授權
            string lbProductID = ComputerInfo.GetComputerId();
            KeyManager km = new KeyManager(lbProductID);
            LicenseInfo lic = new LicenseInfo();
            int value = km.LoadSuretyFile(string.Format(@"{0}\CEClicense.lic", TargetPath), ref lic);
            string productKey = lic.ProductKey;

            if (km.ValidKey(ref productKey))
            {
                KeyValuesClass kv = new KeyValuesClass();
                if (km.DisassembleKey(productKey, ref kv))
                {
                    if (kv.Type == LicenseType.TRIAL)
                    {
                        var D = (kv.Expiration - DateTime.Now.Date).Days;
                        if (D > 0)
                        {
                            btnRegistration.IsEnabled = false;
                        }
                        else
                        {
                            btnRegistration.IsEnabled = true;
                        }
                    }
                    else
                    {
                        btnRegistration.IsEnabled = false;
                    }
                }
            }
            else
            {
                btnRegistration.IsEnabled = true;
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            new frmRegistration().ShowDialog();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            new frmAbout().ShowDialog();
        }

        private void MainForm_Load(object sender, RoutedEventArgs e)
        {
            //判斷授權
            String lbProductID = ComputerInfo.GetComputerId();
            KeyManager km = new KeyManager(lbProductID);
            LicenseInfo lic = new LicenseInfo();
            int value = km.LoadSuretyFile(string.Format(@"{0}\CEClicense.lic", TargetPath), ref lic);
            string productKey = lic.ProductKey;
            if (km.ValidKey(ref productKey))
            {
                KeyValuesClass kv = new KeyValuesClass();
                if (km.DisassembleKey(productKey, ref kv))
                {
                    //lbProductName.Text = "CEC";
                    //lbProductKey.Text = productKey;
                    if (kv.Type == LicenseType.TRIAL)
                    {
                        //lblicenseType.Text = string.Format("試用期還有 {0}天", (kv.Expiration - DateTime.Now.Date).Days);
                        //CEC_RevitAPI.LicenseSetting.Default.CheckLicense = true;

                        var D = (kv.Expiration - DateTime.Now.Date).Days;
                        if (D > 0)
                        {
                            btnRegistration.IsEnabled = false;
                        }
                        else
                        {
                            btnRegistration.IsEnabled = true;
                        }
                    }
                    else
                    {
                        //lblicenseType.Text = "已授權";
                        //CEC_RevitAPI.LicenseSetting.Default.CheckLicense = true;
                        btnRegistration.IsEnabled = false;
                    }
                }
            }
            else
            {
                //MessageBox.Show("授權無效", "錯誤訊息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //CEC_RevitAPI.LicenseSetting.Default.CheckLicense = false;
                btnRegistration.IsEnabled = true;
            }
        }
    }
}
