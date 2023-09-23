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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using CECLicense;

namespace CEC_License
{
    /// <summary>
    /// frmRegistration.xaml 的互動邏輯
    /// </summary>
    public partial class frmRegistration : Window
    {
        string TargetPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins";
        public frmRegistration()
        {
            InitializeComponent();
            txtProductID.Text = ComputerInfo.GetComputerId();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            KeyManager km = new KeyManager(txtProductID.Text);
            string productKey = txtProductKey.Text;
            if (km.ValidKey(ref productKey))
            {
                KeyValuesClass kv = new KeyValuesClass();
                if (km.DisassembleKey(productKey, ref kv))
                {
                    LicenseInfo lic = new LicenseInfo();
                    lic.ProductKey = productKey;
                    lic.FullName = "CEC";
                    if (kv.Type == LicenseType.TRIAL)
                    {
                        lic.Day = kv.Expiration.Day;
                        lic.Month = kv.Expiration.Month;
                        lic.Year = kv.Expiration.Year;
                    }
                    km.SaveSuretyFile(string.Format(@"{0}\CEClicense.lic", TargetPath), lic);
                    System.Windows.Forms.MessageBox.Show("註冊成功，請切換視圖啟用授權");
                    CEC_License.LicenseSetting.Default.CheckLicense = true;
                    this.Close();
                }
                //
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("註冊失敗");
                this.Close();
            }
        }

        private void frmRegistration_Load(object sender, RoutedEventArgs e)
        {
            txtProductID.Text = ComputerInfo.GetComputerId();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
