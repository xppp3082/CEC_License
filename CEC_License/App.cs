#region Namespaces
using System;
using System.Windows;
using System.IO;
using System.Xaml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;//For Assembly
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;//For BitmapImage ,Namespace = PresentationCore�BSystem.Xaml

using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI.Events;
using System.Windows.Interop;
using System.Windows;
using CECLicense;

#endregion

namespace CEC_License
{
    class App : IExternalApplication
    {
        string AssemblyFullName { get { return Assembly.GetExecutingAssembly().Location; } }// ���o�ثedll�ɪ�������|+�ɦW
        //string AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); // ���o�ثedll�ɪ���Ƨ���m
        //string TargetPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins";
        static Assembly app = Assembly.GetExecutingAssembly();
        string assemblyInfo = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        AssemblyName assemName = app.GetName();//Ū��RevitAPI����
        List<RibbonItem> only3Ditem = new List<RibbonItem>();//�Ω�`��3D���Ϥ~��ϥΪ����s
        List<RibbonItem> only2Ditem = new List<RibbonItem>();//�Ω�`��2D���Ϥ~��ϥΪ����s
        List<RibbonItem> checkitem = new List<RibbonItem>();//�`���n������s
        public Result OnStartup(UIControlledApplication a)
        {
            string versionNumber = a.ControlledApplication.VersionNumber;
            //���o�D�{���X���a��A�p�G���}�ݭn�A�Q�Q
            //string TargetPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\2019";
            string TargetPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\";//C:\Users\�u��\AppData\Roaming\Autodesk\Revit\Addins

            //�гyextendPath
            string ApiDllPath_Hanger = TargetPath + versionNumber + @"\AutoHangerCreation_ButtonCreate.dll";
            string ApiDllPath_BeamCast = TargetPath + versionNumber + @"\BeamCasing_ButtonCreate.dll";
            string ApiDllPath_WallCast = TargetPath + versionNumber + @"\CEC_WallCast.dll";
            string ApiDllPath_PreFab = TargetPath + versionNumber + @"\CEC_PreFabric.dll";
            string ApiDllPath_BlockTrans = TargetPath + versionNumber + @"\CEC_CADBlockTrans.dll";
            string ApiDllPath_PipeTags = TargetPath + versionNumber + @"\PipeTagger.dll";
            //string ApiDllPath_Hanger = TargetPath + @"\AutoHangerCreation_ButtonCreate.dll";
            //string ApiDllPath_BeamCast = TargetPath  + @"\BeamCasing_ButtonCreate.dll";
            //string ApiDllPath_WallCast = TargetPath + @"\CEC_WallCast.dll";
            //string ApiDllPath_PreFab = TargetPath  + @"\CEC_PreFabric.dll";
            //string ApiDllPath_BlockTrans = TargetPath + @"\CEC_CADBlockTrans.dll";

            string RevitApiDllPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\CEC_API\CEC_RevitAPI2019.dll";
            string ThisDllPath = AssemblyFullName;

            //����Dll���޿�γ\�i�H�ܦ�-->�ЫؤT�Ӹ�Ƨ�CEC MEP>2019�B2021�B2023�A�M��Q�Φb���J��Ū��Revit�����i��n�έ��@��dll���P�_
            const string RIBBON_TAB = "��CEC-���qAPI(2)";
            const string RIBBON_PANEL1 = "�ަQ�[";
            const string RIBBON_PANEL2 = "��ٶ}�f";
            const string RIBBON_PANEL3 = "���CSD&SEM";
            const string RIBBON_PANEL4 = "����}�f";
            const string RIBBON_PANEL5 = "����CSD&SEM";
            const string RIBBON_PANEL6 = "�޽u�w��";
            const string RIBBON_PANEL7 = "�϶��ഫ";
            const string RIBBON_PANEL8 = "�����޼���";
            const string RIBBON_Regis = "���v���U";

            #region �P�_���v
            //�P�_���v
            String lbProductID = ComputerInfo.GetComputerId();
            KeyManager km = new KeyManager(lbProductID);
            LicenseInfo lic = new LicenseInfo();
            //C:\Users\�u��\AppData\Roaming\Autodesk\Revit\Addins\CEClicens.lic
            int value = km.LoadSuretyFile(string.Format(@"{0}CEClicense.lic", TargetPath), ref lic);
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
                            LicenseSetting.Default.CheckLicense = true;
                        }
                        else
                        {
                            LicenseSetting.Default.CheckLicense = false;
                            if (File.Exists(TargetPath + @"\CEClicense.lic"))
                            {
                                try
                                {
                                    File.Delete(TargetPath + @"\CEClicense.lic");
                                }
                                catch { }
                            }
                        }
                    }
                    else
                    {
                        //MessageBox.Show("�w���v");
                        LicenseSetting.Default.CheckLicense = true;
                    }
                }
            }
            //�гyribbon tab
            else
            {
                LicenseSetting.Default.CheckLicense = false;
            }
            try
            {
                a.CreateRibbonTab(RIBBON_TAB);
            }
            catch (Exception) { }
            #endregion
            //���ϧ��ܻP�Ұʮɤ��нT�{���v
            a.ViewActivated += Application_ViewActivated;

            //�إ�panel-->�O�_�ݭn��������ݽT�{
            RibbonPanel panel1 = a.CreateRibbonPanel(RIBBON_TAB, RIBBON_PANEL1);//�ަQ�[
            RibbonPanel panel2 = a.CreateRibbonPanel(RIBBON_TAB, RIBBON_PANEL2);//��ٶ}�f
            RibbonPanel panel3 = a.CreateRibbonPanel(RIBBON_TAB, RIBBON_PANEL3);//���CSD&SEM
            RibbonPanel panel4 = a.CreateRibbonPanel(RIBBON_TAB, RIBBON_PANEL4);//����}�f
            RibbonPanel panel5 = a.CreateRibbonPanel(RIBBON_TAB, RIBBON_PANEL5);//����CSD&SEM
            RibbonPanel panel6 = a.CreateRibbonPanel(RIBBON_TAB, RIBBON_PANEL6);//�޽u�w��
            RibbonPanel panel7 = a.CreateRibbonPanel(RIBBON_TAB, RIBBON_PANEL7);//�϶��ഫ
            RibbonPanel panel8 = a.CreateRibbonPanel(RIBBON_TAB, RIBBON_PANEL8);//�����޼���
            RibbonPanel panelRegis = a.CreateRibbonPanel(RIBBON_TAB, RIBBON_Regis);//���U��T

            #region[Panel1] - �g����qBIM-�ަQ�[
            //�Ыس�ަQ�[
            System.Drawing.Image image_Single = Properties.Resources.���_�h��V2__�ഫ__02__96dpi;
            ImageSource imgSrc = GetImageSource(image_Single);
            //�Ыئh�ަQ�[
            System.Drawing.Image image_Multi = Properties.Resources.���_�h��V2__�ഫ__01__96dpi;
            ImageSource imgSrc2 = GetImageSource(image_Multi);
            //�]�w��ަQ�[
            System.Drawing.Image image_SetUp = Properties.Resources.��ަQ�[�]�w_32pix;
            ImageSource imgSrc3 = GetImageSource(image_SetUp);

            PushButtonData btnData = new PushButtonData("MyButton_Single", "�Ы�\n��ަQ�[", ApiDllPath_Hanger, "AutoHangerCreation_ButtonCreate.AddHangerByMouseLink");
            {
                btnData.ToolTip = "�I��ެq�Ыس�ަQ�[";
                btnData.LongDescription = $"�I��ݭn�Ыت��ެq�A�ͦ���ަQ�[({versionNumber})";
                btnData.LargeImage = imgSrc;
            };
            PushButtonData btnData2 = new PushButtonData("MyButton_Multi", "�Ы�\n�h�ަQ�[", ApiDllPath_Hanger, "AutoHangerCreation_ButtonCreate.MultiHangerCreationV3");
            {
                btnData2.ToolTip = "�I��ެq�Ыئh�ަQ�[";
                btnData2.LongDescription = $"�I��ݭn�Ыت��ެq�A�ͦ��h�ަQ�[�A�榸�̦h��ܤK���({versionNumber})";
                btnData2.LargeImage = imgSrc2;
            }
            PushButtonData btnData3 = new PushButtonData("MyButton_SetUp", "�]�w\n�Q�[", ApiDllPath_Hanger, "AutoHangerCreation_ButtonCreate.PipeHangerSetUp");
            {
                btnData3.ToolTip = "�]�w�Q�[�����P���Z";
                btnData3.LongDescription = $"�]�w�۰ʩ�m��ަQ�[�һݪ��Q�[�����P���Z�A�]�w��~��ϥγ�ަQ�[�\��({assemblyInfo})";
                btnData3.LargeImage = imgSrc3;
            }
            PushButton button = panel1.AddItem(btnData) as PushButton;
            PushButton button2 = panel1.AddItem(btnData2) as PushButton;
            PushButton button3 = panel1.AddItem(btnData3) as PushButton;
            checkitem.Add(button);
            checkitem.Add(button2);
            checkitem.Add(button3);
            #endregion

            #region[Panel2] - �g����qBIM-��ٶ}�f
            System.Drawing.Image image_CreateST = Properties.Resources.��ٮM��ICON�X��_ST;
            ImageSource imgSrcST = GetImageSource(image_CreateST);
            System.Drawing.Image image_CreateSTLink = Properties.Resources.��ٮM��ICON�X��_STlink;
            ImageSource imgSrcSTLink = GetImageSource(image_CreateSTLink);
            System.Drawing.Image image_Create = Properties.Resources.��ٮM��ICON�X��_RC;
            ImageSource imgSrcRC = GetImageSource(image_Create);
            System.Drawing.Image image_CreateLink = Properties.Resources.��ٮM��ICON�X��_RClink;
            ImageSource imgSrcRCLink = GetImageSource(image_CreateLink);
            System.Drawing.Image image_Update = Properties.Resources.��ٮM��ICON�X��_��s;
            ImageSource imgSrcUpdate = GetImageSource(image_Update);
            System.Drawing.Image image_UpdatePart = Properties.Resources.�ھڵ��Ͻd���s��ٸ�T_;
            ImageSource imgSrcUpdatePart = GetImageSource(image_UpdatePart);
            System.Drawing.Image image_SetCast = Properties.Resources.��ٮM��ICON�X��_�]�w;
            ImageSource imgSrcSetCast = GetImageSource(image_SetCast);
            System.Drawing.Image image_Num = Properties.Resources.��ٮM��ICON�X��_�s��2;
            ImageSource imgSrcNum = GetImageSource(image_Num);
            System.Drawing.Image image_ReNum = Properties.Resources.��ٮM��ICON�X��_���s��2;
            ImageSource imgSrcReNum = GetImageSource(image_ReNum);
            System.Drawing.Image image_Copy = Properties.Resources.�Ƭ�ٮM��ICON�X��_�ƻs;
            ImageSource imgSrcCopy = GetImageSource(image_Copy);
            System.Drawing.Image image_CopyPart = Properties.Resources.��ٮM��ICON�X��_�v�h�ƻs;
            ImageSource imgSrcCopyPart = GetImageSource(image_CopyPart);
            System.Drawing.Image image_Rect = Properties.Resources.��ٮM��ICON�X��_��ζ}�f;
            ImageSource imgSrcRect = GetImageSource(image_Rect);
            System.Drawing.Image image_RectLink = Properties.Resources.��ٮM��ICON�X��_��ζ}�flink;
            ImageSource imgSrcRectLink = GetImageSource(image_RectLink);
            System.Drawing.Image image_MultiOpen = Properties.Resources.��ٮM��ICON�X��_�h�޶}�f;
            ImageSource imgSrcMultiOpen = GetImageSource(image_MultiOpen);
            System.Drawing.Image image_MultiLink = Properties.Resources.��ٮM��ICON�X��_�h�޶}�flink;
            ImageSource imgSrcMultiLink = GetImageSource(image_MultiLink);

            PushButtonData btnDataST = new PushButtonData(
            "MyButton_CastCreateST",
            "���c�}��",
            ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.CreateBeamCastSTV2"//���s�����W-->�n�̷ӻݭn�ѷӪ�command���J
            );
            {
                btnDataST.ToolTip = "�I��޻P�~�Ѽ٥ͦ���ٶ}�f";
                btnDataST.LongDescription = $"���I��ݭn�Ыت��ެq�A�A�I����L���~�Ѽ١A�ͦ���ٮM��({assemblyInfo})";
                btnDataST.LargeImage = imgSrcST;
            };

            PushButtonData btnDataSTLink = new PushButtonData(
            "MyButton_CastCreateSTLink",
            "���c�}��(�s��)",
            ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.CreateBeamCastLinkST"//���s�����W-->�n�̷ӻݭn�ѷӪ�command���J
            );
            {
                btnDataSTLink.ToolTip = "�I��~�Ѻ޻P�~�Ѽ٥ͦ���ٶ}�f";
                btnDataSTLink.LongDescription = $"���I��ݭn�Ыت��ެq�A�A�I����L���~�Ѽ١A�ͦ���ٮM��({assemblyInfo})";
                btnDataSTLink.LargeImage = imgSrcSTLink;
            };

            PushButtonData btnDataRC = new PushButtonData(
            "MyButton_CastCreate",
            "RC�M��",
            ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.CreateBeamCastV2"//���s�����W-->�n�̷ӻݭn�ѷӪ�command���J
            );
            {
                btnDataRC.ToolTip = "�I��޻P�~�Ѽ٥ͦ���ٮM��";
                btnDataRC.LongDescription = $"���I��ݭn�Ыت��ެq�A�A�I����L���~�Ѽ١A�ͦ���ٮM��({assemblyInfo})";
                btnDataRC.LargeImage = imgSrcRC;
            };

            PushButtonData btnDataRCLink = new PushButtonData(
            "MyButton_CastCreateLink",
            "RC�M��(�s��)",
            ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.CreateBeamCastLink"//���s�����W-->�n�̷ӻݭn�ѷӪ�command���J
            );
            {
                btnDataRCLink.ToolTip = "�I��~�Ѻ޻P�~�Ѽ٥ͦ���ٮM��";
                btnDataRCLink.LongDescription = $"���I��ݭn�Ыت��ެq�A�A�I����L���~�Ѽ١A�ͦ���ٮM��({assemblyInfo})";
                btnDataRCLink.LargeImage = imgSrcRCLink;
            };

            PushButtonData btnDataReNew = new PushButtonData(
             "MyButton_CastUpdate",
             "��s\n��ٸ�T",
             ApiDllPath_BeamCast,
             "BeamCasing_ButtonCreate.CastInfromUpdateV4"
             );
            {
                btnDataReNew.ToolTip = "�@���s��ٮM�޻P��ٶ}�f��T";
                btnDataReNew.LongDescription = $"�̷ӥ��׳]�w������h�A��s��ٮM�޸�T (�������]�w��٭�h��i�ϥ�)({assemblyInfo})";
                btnDataReNew.LargeImage = imgSrcUpdate;
            }

            PushButtonData btnDataUpdatePart = new PushButtonData(
            "MyButton_CastUpdatePart",
            "�~����s\n��ٸ�T",
            ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.CastInfromUpdatePart"
            );
            {
                btnDataUpdatePart.ToolTip = "�̷ӥثe���Ͻd��A������s��ٮM�޸�T";
                btnDataUpdatePart.LongDescription = $"�̷ӥثe���Ͻd��A������s��ٮM�޸�T (�������]�w��٭�h��i�ϥ�)({assemblyInfo})";
                btnDataUpdatePart.LargeImage = imgSrcUpdatePart;
            }

            PushButtonData btnDataSetCast = new PushButtonData(
            "MyButton_CastSetUp",
            "�]�w\n��٭�h   ",
            ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.BeamCastSetUp"
             );
            {
                btnDataSetCast.ToolTip = "�]�w��٭�h����";
                btnDataSetCast.LongDescription = $"�̾ڱM�׻ݨD�A�]�w���ת���٭�h��T({assemblyInfo})";
                btnDataSetCast.LargeImage = imgSrcSetCast;
            }

            PushButtonData btnDataRect = new PushButtonData(
            "MyButton_CastRect",
            "��μٶ}��",
            ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.CreateRectBeamCast"
            );
            {
                btnDataRect.ToolTip = "�I��޻P�~�Ѽ٥ͦ���ٮM��";
                btnDataRect.LongDescription = $"���I��ݭn�Ыت��ެq�A�A�I����L���~�Ѽ١A�ͦ���ٮM��({assemblyInfo})";
                btnDataRect.LargeImage = imgSrcRect;
            }

            PushButtonData btnDataRectLink = new PushButtonData(
            "MyButton_CastRectLink",
            "��μٶ}��(�s��)",
            ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.CreateRectBeamCastLink"
            );
            {
                btnDataRectLink.ToolTip = "�I��~�Ѻ޻P�~�Ѽ٥ͦ���ٮM��";
                btnDataRectLink.LongDescription = $"���I��ݭn�Ыت��ެq�A�A�I����L���~�Ѽ١A�ͦ���ٮM��({assemblyInfo})";
                btnDataRectLink.LargeImage = imgSrcRectLink;
            }

            PushButtonData btnDataMultiOpen = new PushButtonData(
            "MyButton_MultiRect",
            "�h�޼ٶ}��",
            ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.MultiBeamRectCast"
            );
            {
                btnDataMultiOpen.ToolTip = "�I��h��޻P�~�Ѽ٥ͦ������}�f";
                btnDataMultiOpen.LongDescription = $"���I��ݭn�Ыت��ެq(�Ƽ�)�A�A�I����L���~����A�ͦ���٤�}�f({assemblyInfo})";
                btnDataMultiOpen.LargeImage = imgSrcMultiOpen;
            }

            PushButtonData btnDataMultiLink = new PushButtonData(
            "MyButton_MultiRectLink",
            "�h�޼ٶ}��(�s��)",
            ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.MultiBeamRectCastLink"
            );
            {
                btnDataMultiLink.ToolTip = "�I��h��~�Ѻ޻P�~�Ѽ٥ͦ������}�f";
                btnDataMultiLink.LongDescription = $"���I��ݭn�Ыت��ެq(�Ƽ�)�A�A�I����L���~����A�ͦ���٤�}�f({assemblyInfo})";
                btnDataMultiLink.LargeImage = imgSrcMultiLink;
            }

            //��s��ٸ�T(��s&�]�w)
            SplitButtonData setUpButtonData = new SplitButtonData("CastSetUpButton", "��ٮM�ާ�s");
            SplitButton splitButton1 = panel2.AddItem(setUpButtonData) as SplitButton;
            PushButton buttonRenew = splitButton1.AddPushButton(btnDataReNew);
            PushButton buttonUpdatePart = splitButton1.AddPushButton(btnDataUpdatePart);
            PushButton buttonSetCast = splitButton1.AddPushButton(btnDataSetCast);
            checkitem.Add(buttonRenew);
            only2Ditem.Add(buttonUpdatePart);
            checkitem.Add(buttonSetCast);

            //�Ыج�ٮM��(ST&RC)
            SplitButtonData STButtonData = new SplitButtonData("CreateCastST", "���c�}��");
            SplitButton splitButtonST = panel2.AddItem(STButtonData) as SplitButton;
            PushButton STbutton = splitButtonST.AddPushButton(btnDataST);
            PushButton STbuttonLink = splitButtonST.AddPushButton(btnDataSTLink);
            checkitem.Add(STbutton);
            checkitem.Add(STbuttonLink);

            SplitButtonData RCButtonData = new SplitButtonData("CreateCast", "RC�M��");
            SplitButton splitButtonRC = panel2.AddItem(RCButtonData) as SplitButton;
            PushButton RCbutton = splitButtonRC.AddPushButton(btnDataRC);
            PushButton RCbuttonLink = splitButtonRC.AddPushButton(btnDataRCLink);
            checkitem.Add(RCbutton);
            checkitem.Add(RCbuttonLink);

            SplitButtonData rectCastButtonData = new SplitButtonData("RectCastButton", "��μٶ}�f");
            SplitButton splitButtonRect = panel2.AddItem(rectCastButtonData) as SplitButton;
            PushButton buttonRect = splitButtonRect.AddPushButton(btnDataRect);
            PushButton buttonRectLink = splitButtonRect.AddPushButton(btnDataRectLink);
            PushButton buttonMultiOpen = splitButtonRect.AddPushButton(btnDataMultiOpen);
            PushButton buttonMultiLink = splitButtonRect.AddPushButton(btnDataMultiLink);
            checkitem.Add(buttonRect);
            checkitem.Add(buttonRectLink);
            checkitem.Add(buttonMultiOpen);
            checkitem.Add(buttonMultiLink);
            #endregion

            #region[Panel3] - �g����qBIM-���CSD&SEM
            PushButtonData btnDataNum = new PushButtonData(
            "MyButton_CastNum",
            "��ٮM��\n�s��",
            ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.UpdateCastNumber"
            );
            {
                btnDataNum.ToolTip = "��ٮM�ަ۰ʽs��";
                btnDataNum.LongDescription = $"�ھڨC�h�Ӫ��}�f�ƶq�P��m�A�̧Ǧ۰ʱa�J�s���A�ĤG���W�J�s���ɫh�|���L�w�g��J�s�����M��({assemblyInfo})";
                btnDataNum.LargeImage = imgSrcNum;
            }

            PushButtonData btnDataReNum = new PushButtonData(
            "MyButton_ReNum",
            "��ٮM��\n���s�s��",
            ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.ReUpdateCastNumber"
            );
            {
                btnDataReNum.ToolTip = "��ٮM�ޭ��s�s��";
                btnDataReNum.LongDescription = $"�ھڨC�h�Ӫ��}�f�ƶq�A���s�a�J�s��({assemblyInfo})";
                btnDataReNum.LargeImage = imgSrcReNum;
            }

            PushButtonData btnDataCopy = new PushButtonData(
            "MyButton_CopyLinked",
            "�ƻs�~��\n��ٮM��",
           ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.CopyAllCast"
            );
            {
                btnDataCopy.ToolTip = "�ƻs�Ҧ��s���ҫ������M��";
                btnDataCopy.LongDescription = $"�ƻs�Ҧ��s���ҫ������M�ޡA�H��SEM�}�f�s����({assemblyInfo})";
                btnDataCopy.LargeImage = imgSrcCopy;
            }


            PushButtonData btnDataCopyPart = new PushButtonData(
            "MyButton_CopyPartLinked",
            "�̼Ӽh�ƻs\n��ٮM��",
           ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.CopyPartCast"
            );
            {
                btnDataCopyPart.ToolTip = "�̵��ϰѦҼӼh�ƻs�s���ҫ������M��";
                btnDataCopyPart.LongDescription = $"�̵��ϰѦҼӼh�ƻs�s���ҫ������M�ޡA�H��SEM�}�f�s����({assemblyInfo})";
                btnDataCopyPart.LargeImage = imgSrcCopyPart;
            }

            //�ƻs�Ҧ��M��&�����ƻs
            SplitButtonData copyCastButtonData = new SplitButtonData("CopyCastButton", "�ƻs�~��\n   ��ٮM��");
            SplitButton splitButtonCopyCast = panel3.AddItem(copyCastButtonData) as SplitButton;
            PushButton buttonCopy = splitButtonCopyCast.AddPushButton(btnDataCopy);
            PushButton buttonCopyPart = splitButtonCopyCast.AddPushButton(btnDataCopyPart);
            //PushButton buttonCopy = panel3.AddItem(btnDataCopy) as PushButton;
            checkitem.Add(buttonCopy);
            only2Ditem.Add(buttonCopyPart);

            //��ٮM�޽s��(�s��&���s)
            SplitButtonData setNumButtonData = new SplitButtonData("CastSetNumButton", "��ٮM�޽s��");
            SplitButton splitButtonSetNum = panel3.AddItem(setNumButtonData) as SplitButton;
            PushButton buttonNum = splitButtonSetNum.AddPushButton(btnDataNum);
            PushButton buttonReNum = splitButtonSetNum.AddPushButton(btnDataReNum);
            checkitem.Add(buttonNum);
            checkitem.Add(buttonReNum);
            #endregion

            #region[Panel4] - �g����qBIM-����}�f
            //��s�����T
            System.Drawing.Image image_UpdateWall = Properties.Resources.����M��ICON�X��_��s_svg;
            ImageSource imgSrcUpdateWall = GetImageSource(image_UpdateWall);
            //������s�����T
            System.Drawing.Image image_UpdateWallPart = Properties.Resources.����M��ICON�X��_������s_svg;
            ImageSource imgSrcUpdateWallPart = GetImageSource(image_UpdateWallPart);
            //��m����M��
            System.Drawing.Image image_WallCast = Properties.Resources.����M��ICON�X��_��m_svg;
            ImageSource imgSrcWallCast = GetImageSource(image_WallCast);
            //��m����M��(�s��)
            System.Drawing.Image image_WallCastLink = Properties.Resources.����M��ICON�X��_��mlink_svg;
            ImageSource imgSrcWallCastLink = GetImageSource(image_WallCastLink);
            //�ƻs�~�ѮM��
            System.Drawing.Image image_CopyWallCast = Properties.Resources.����M��ICON�X��_�ƻs�~��_svg;
            ImageSource imgSrcCopyWallCast = GetImageSource(image_CopyWallCast);
            //�ƻs�~�ѮM��(�̼Ӽh)
            System.Drawing.Image image_CopyWallCastPart = Properties.Resources.����M��ICON�X��_�ƻs�v�h_svg;
            ImageSource imgSrcCopyWallCastPart = GetImageSource(image_CopyWallCastPart);
            //����M�޽s��
            System.Drawing.Image image_WallNum = Properties.Resources.����M��ICON�X��_�s��_svg;
            ImageSource imgSrcSetNum = GetImageSource(image_WallNum);
            //����M�ޭ��s�s��
            System.Drawing.Image image_ReWallNum = Properties.Resources.����M��ICON�X��_���s�s��_svg;
            ImageSource imgSrcReWallNum = GetImageSource(image_ReWallNum);
            //��m�����}�f
            System.Drawing.Image image_WallRect = Properties.Resources.����M��ICON�X��_��}�f_svg;
            ImageSource imgSrcWallRect = GetImageSource(image_WallRect);
            //��m�����}�f(�s��)
            System.Drawing.Image image_WallRectLink = Properties.Resources.����M��ICON�X��_��}�flink_svg;
            ImageSource imgSrcWallRectLink = GetImageSource(image_WallRectLink);
            //��m�h�ޤ�}�f
            System.Drawing.Image image_MultiWallRect = Properties.Resources.����M��ICON�X��_�h�ޤ�}�f_svg;
            ImageSource imgSrcMultiWallRect = GetImageSource(image_MultiWallRect);
            //��m�h�ޤ�}�f(�s��)
            System.Drawing.Image image_MultiRectLink = Properties.Resources.����M��ICON�X��_�h�ޤ�}�flink_svg;
            ImageSource imgSrcMultiRectLink = GetImageSource(image_MultiRectLink);

            PushButtonData btnDataWallCastUpdate = new PushButtonData(
            "MyButton_WallCastUpdate",
            "��s\n�����T",
            ApiDllPath_WallCast,
            "CEC_WallCast.WallCastUpdate"//���s�����W-->�n�̷ӻݭn�ѷӪ�command���J
            );
            {
                btnDataWallCastUpdate.ToolTip = "�@���s����}�f��T";
                btnDataWallCastUpdate.LongDescription = $"�@���s����}�f��T({assemblyInfo})";
                btnDataWallCastUpdate.LargeImage = imgSrcUpdateWall;
            };
            PushButtonData btnDataWallCastUpdatePart = new PushButtonData(
            "MyButton_WallCastUpdatePart",
            "�~����s\n�����T",
            ApiDllPath_WallCast,
            "CEC_WallCast.WallCastUpdatePart"//���s�����W-->�n�̷ӻݭn�ѷӪ�command���J
            );
            {
                btnDataWallCastUpdatePart.ToolTip = "�̷ӥثe���Ͻd��A������s����}�f��T";
                btnDataWallCastUpdatePart.LongDescription = $"�̷ӥثe���Ͻd��A������s����}�f��T({assemblyInfo})";
                btnDataWallCastUpdatePart.LargeImage = imgSrcUpdateWallPart;
            };
            PushButtonData btnDataWallCastCreate = new PushButtonData(
            "MyButton_WallCastCreate",
            "����M��",
            ApiDllPath_WallCast,
            "CEC_WallCast.CreateWallCastV2"//���s�����W-->�n�̷ӻݭn�ѷӪ�command���J
            );
            {
                btnDataWallCastCreate.ToolTip = "�I��޻P�~����ͦ�����M��";
                btnDataWallCastCreate.LongDescription = $"���I��ݭn�Ыت��ެq�A�A�I����L���~����A�ͦ�����M��({assemblyInfo})";
                btnDataWallCastCreate.LargeImage = imgSrcWallCast;
            };
            PushButtonData btnDataWallCastlink = new PushButtonData(
            "MyButton_WallCastCreateLink",
            "����M��(�s��)",
            ApiDllPath_WallCast,
            "CEC_WallCast.CreateWallCastLink"//���s�����W-->�n�̷ӻݭn�ѷӪ�command���J
            );
            {
                btnDataWallCastlink.ToolTip = "�I��~�Ѻ޻P�~����ͦ�����M��";
                btnDataWallCastlink.LongDescription = $"���I��ݭn�Ыت��ެq�A�A�I����L���~����A�ͦ�����M��({assemblyInfo})";
                btnDataWallCastlink.LargeImage = imgSrcWallCastLink;
            };
            PushButtonData btnDataWallCastRect = new PushButtonData(
            "MyButton_WallCastRect",
            "�諬��}�f",
            ApiDllPath_WallCast,
            "CEC_WallCast.CreateRectWallCast"
            );
            {
                btnDataWallCastRect.ToolTip = "�I��޻P�~����ͦ������}�f";
                btnDataWallCastRect.LongDescription = $"���I��ݭn�Ыت��ެq�A�A�I����L���~����A�ͦ������}�f({assemblyInfo})";
                btnDataWallCastRect.LargeImage = imgSrcWallRect;
            }
            PushButtonData btnDataWallCastRectLink = new PushButtonData(
            "MyButton_WallCastRectLink",
            "�諬��}�f(�s��)",
            ApiDllPath_WallCast,
            "CEC_WallCast.CreateRectWallCastLink"
            );
            {
                btnDataWallCastRectLink.ToolTip = "�I��~�Ѻ޻P�~����ͦ������}�f";
                btnDataWallCastRectLink.LongDescription = $"���I��ݭn�Ыت��~�Ѻެq�A�A�I����L���~����A�ͦ������}�f({assemblyInfo})";
                btnDataWallCastRectLink.LargeImage = imgSrcWallRectLink;
            }
            PushButtonData btnDataWallCastRectMulti = new PushButtonData(
            "MyButton_WallCastRectMulti",
            "�h����}�f",
            ApiDllPath_WallCast,
            "CEC_WallCast.MultiWallRectCast"
            );
            {
                btnDataWallCastRectMulti.ToolTip = "�I��~����P�h��ޥͦ������}�f";
                btnDataWallCastRectMulti.LongDescription = $"���I��ݭn�Ыت��ެq(�Ƽ�)�A�A�I����L���~����A�ͦ������}�f({assemblyInfo})";
                btnDataWallCastRectMulti.LargeImage = imgSrcMultiWallRect;
            }
            PushButtonData btnDataMultiRectLink = new PushButtonData(
            "MyButton_WallCastRectMultiLink",
            "�h����}�f(�s��)",
             ApiDllPath_WallCast,
            "CEC_WallCast.MultiWallRectCastLink"
            );
            {
                btnDataMultiRectLink.ToolTip = "�I��~����P�h��ޥͦ������}�f";
                btnDataMultiRectLink.LongDescription = $"���I��ݭn�Ыت��ެq(�Ƽ�)�A�A�I����L���~����A�ͦ������}�f({assemblyInfo})";
                btnDataMultiRectLink.LargeImage = imgSrcMultiRectLink;
            }

            //��s����M��
            SplitButtonData updateButtonData = new SplitButtonData("UpdateCast", "��s\n   �����T");
            SplitButton splitButtonWallUpdate = panel4.AddItem(updateButtonData) as SplitButton;
            PushButton buttonWallCastUpdate = splitButtonWallUpdate.AddPushButton(btnDataWallCastUpdate);
            PushButton buttonWallCastUpdatePart = splitButtonWallUpdate.AddPushButton(btnDataWallCastUpdatePart);
            checkitem.Add(buttonWallCastUpdate);
            only2Ditem.Add(buttonWallCastUpdatePart);

            //�Ыج���M��(��)
            SplitButtonData wallCastButtonData = new SplitButtonData("WallCast", "����M��");
            SplitButton splitButtonWallCast = panel4.AddItem(wallCastButtonData) as SplitButton;
            PushButton buttonWallCast = splitButtonWallCast.AddPushButton(btnDataWallCastCreate);
            PushButton buttonWallCastLink = splitButtonWallCast.AddPushButton(btnDataWallCastlink);
            checkitem.Add(buttonWallCast);
            checkitem.Add(buttonWallCastLink);

            //�Ыج���M��(��)
            SplitButtonData wallRectCastButtonData = new SplitButtonData("WallCastRect", "�諬��}�f");
            SplitButton splitButtonWallRect = panel4.AddItem(wallRectCastButtonData) as SplitButton;
            PushButton buttonWallCastRect = splitButtonWallRect.AddPushButton(btnDataWallCastRect);
            PushButton buttonWallCastRectLink = splitButtonWallRect.AddPushButton(btnDataWallCastRectLink);
            PushButton buttonWallCastRectMulti = splitButtonWallRect.AddPushButton(btnDataWallCastRectMulti);
            PushButton buttonWallMultiRectLink = splitButtonWallRect.AddPushButton(btnDataMultiRectLink);
            checkitem.Add(buttonWallCastRect);
            checkitem.Add(buttonWallCastRectLink);
            checkitem.Add(buttonWallCastRectMulti);
            checkitem.Add(buttonWallMultiRectLink);
            #endregion

            #region[Panel5] - �g����qBIM-����CSD&SEM
            PushButtonData btnDataCopyWallCast = new PushButtonData(
            "MyButton_WallCastCopy",
            "�ƻs�~��\n����M��",
            ApiDllPath_WallCast,
            "CEC_WallCast.CopyAllWallCast"
            );
            {
                btnDataCopyWallCast.ToolTip = "�ƻs�Ҧ��s���ҫ������M��";
                btnDataCopyWallCast.LongDescription = $"�ƻs�Ҧ��s���ҫ������M�ޡA�H��SEM�}�f�s����({assemblyInfo})";
                btnDataCopyWallCast.LargeImage = imgSrcCopyWallCast;
            }
            PushButtonData btnDataCopyWallCastPart = new PushButtonData(
            "MyButton_WallCastCopyPart",
            "�̼Ӽh�ƻs\n����M��",
            ApiDllPath_WallCast,
            "CEC_WallCast.CopyPartWallCast"
            );
            {
                btnDataCopyWallCastPart.ToolTip = "�̵��ϰѦҼӼh�ƻs�s���ҫ������M��";
                btnDataCopyWallCastPart.LongDescription = $"�̵��ϰѦҼӼh�ƻs�s���ҫ������M�ޡA�H��SEM�}�f�s����({assemblyInfo})";
                btnDataCopyWallCastPart.LargeImage = imgSrcCopyWallCastPart;
            }

            PushButtonData btnDataWallCastNum = new PushButtonData(
            "MyButton_WallCastNum",
            "����M��\n�s��",
            ApiDllPath_WallCast,
            "CEC_WallCast.UpdateWallCastNumber"
            );
            {
                btnDataWallCastNum.ToolTip = "����M�ަ۰ʽs��";
                btnDataWallCastNum.LongDescription = $"�ھڨC�h�Ӫ��}�f�ƶq�P��m�A�̧Ǧ۰ʱa�J�s���A�ĤG���W�J�s���ɫh�|���L�w�g��J�s�����M��({assemblyInfo})";
                btnDataWallCastNum.LargeImage = imgSrcSetNum;
            }
            PushButtonData btnDataWallCastReNum = new PushButtonData(
            "MyButton_WallCastReNum",
            "����M��\n���s�s��",
            ApiDllPath_WallCast,
            "CEC_WallCast.ReUpdateWallCastNumber"
            );
            {
                btnDataWallCastReNum.ToolTip = "����M�ޭ��s�s��";
                btnDataWallCastReNum.LongDescription = $"�ھڨC�h�Ӫ��}�f�ƶq�A���s�a�J�s��({assemblyInfo})";
                btnDataWallCastReNum.LargeImage = imgSrcReWallNum;
            }

            //�ƻs�Ҧ�����M��
            SplitButtonData copyWallCastButtonData = new SplitButtonData("CopyWallCastButton","�ƻs�~��\n����M��");
            SplitButton splitButtonCopyWallCast = panel5.AddItem(copyWallCastButtonData) as SplitButton;
            PushButton buttonWallCastCopy = splitButtonCopyWallCast.AddPushButton(btnDataCopyWallCast);
            PushButton buttonWallCastCoPart = splitButtonCopyWallCast.AddPushButton(btnDataCopyWallCastPart);
            //PushButton buttonWallCastCopy = panel5.AddItem(btnDataCopyWallCast) as PushButton;
            checkitem.Add(buttonWallCastCopy);
            only2Ditem.Add(buttonWallCastCoPart);

            //��ٮM�޽s��(�s��&���s)
            SplitButtonData setWallNumButtonData = new SplitButtonData("WallCastSetNumButton", "����M�޽s��");
            SplitButton splitButtonWallNum = panel5.AddItem(setNumButtonData) as SplitButton;
            PushButton buttonWallCastNum = splitButtonWallNum.AddPushButton(btnDataWallCastNum);
            PushButton buttonWallCasReNum = splitButtonWallNum.AddPushButton(btnDataWallCastReNum);
            checkitem.Add(buttonWallCastNum);
            checkitem.Add(buttonWallCasReNum);
            #endregion

            #region[Panel6] - �޽u�w��
            System.Drawing.Image image_CreateISO = Properties.Resources.�w��ICON_����ISO��_svg_32;
            ImageSource imgSrcCreateISO = GetImageSource(image_CreateISO);
            System.Drawing.Image image_CleanUpNumber = Properties.Resources.�w��ICON_�M���s��_svg_32;
            ImageSource imgSrcCleanUpNumber = GetImageSource(image_CleanUpNumber);
            System.Drawing.Image image_CleanTags = Properties.Resources.�w��ICON_�M������_svg_32;
            ImageSource imgSrcCleanTags = GetImageSource(image_CleanTags);
            System.Drawing.Image image_Renumber = Properties.Resources.�w��ICON_���s�s��_svg_32;
            ImageSource imgSrcReNumber = GetImageSource(image_Renumber);

            PushButtonData btnDataCreateISO = new PushButtonData(
            "MyButton_CreateFabISO",
            "�Ы�\n �w��ISO",
            ApiDllPath_PreFab,
            "CEC_PreFabric.CreateISO"
             );
            {
                btnDataCreateISO.ToolTip = "�ؿ�ϰ첣�͹w��ISO��";
                btnDataCreateISO.LongDescription = $"�ؿ�Ʊ沣�͹w��ISO�PBOM���ϰ�A�ͦ�ISO�ϻPBOM��({assemblyInfo})";
                btnDataCreateISO.LargeImage = imgSrcCreateISO;
            }

            PushButtonData btnDataCleanUpNumber = new PushButtonData(
            "MyButton_ClenUpFabNum",
            "�M���s��",
            ApiDllPath_PreFab,
            "CEC_PreFabric.CleanUpNumbers"
            );
            {
                btnDataCleanUpNumber.ToolTip = "�M��ISO�Ϥ����ެq�s��";
                btnDataCleanUpNumber.LongDescription = $"�Ф�����e���Ϧܱ��ק�s�����w�յ��ϡA�@��M���s��({assemblyInfo})";
                btnDataCleanUpNumber.LargeImage = imgSrcCleanUpNumber;
            }

            PushButtonData btnDataCleanTags = new PushButtonData(
             "MyButton_DeleteAllTags",
             "�M������",
            ApiDllPath_PreFab,
             "CEC_PreFabric.DeleteAllTags"
             );
            {
                btnDataCleanTags.ToolTip = "�M��ISO�Ϥ����s������";
                btnDataCleanTags.LongDescription = $"�Ф�����e���Ϧܱ��M�����Ҫ��w�յ��ϡA�@��M������({assemblyInfo})";
                btnDataCleanTags.LargeImage = imgSrcCleanTags;
            }

            PushButtonData btnDataReNumber = new PushButtonData(
            "MyButton_ISOReNumber",
            "���s�s��",
            ApiDllPath_PreFab,
            "CEC_PreFabric.ReNumber"
            );
            {
                btnDataReNumber.ToolTip = "�w��ISO�Ϥ����ެq�i�歫�s�s��";
                btnDataReNumber.LongDescription = $"�Ф�����e���Ϧܱ����s�s�����w�յ��ϡA�妸���s�s��({assemblyInfo})";
                btnDataReNumber.LargeImage = imgSrcReNumber;
            }
            PushButton buttonCreateISO = panel6.AddItem(btnDataCreateISO) as PushButton;
            PushButton buttonCleanUpNumber = panel6.AddItem(btnDataCleanUpNumber) as PushButton;
            PushButton buttonCleanTags = panel6.AddItem(btnDataCleanTags) as PushButton;
            PushButton buttonReNumber = panel6.AddItem(btnDataReNumber) as PushButton;
            checkitem.Add(buttonCreateISO);
            only3Ditem.Add(buttonCleanUpNumber);
            only3Ditem.Add(buttonCleanTags);
            only3Ditem.Add(buttonReNumber);
            #endregion

            #region[Panel7] - �϶��ഫ
            //�϶��ഫ
            System.Drawing.Image image_BlkTrans = Properties.Resources.�϶��ഫicon_32pix;
            ImageSource imgSrcBlkTrans = GetImageSource(image_BlkTrans);
            PushButtonData btnDataBlkTrans = new PushButtonData(
            "MyButton_CADBlockTrans",
            "�϶�\n�妸�ഫ",
            ApiDllPath_BlockTrans,
            "CEC_CADBlockTrans.Command"//���s�����W-->�n�̷ӻݭn�ѷӪ�command���J
            );
            {
                btnDataBlkTrans.ToolTip = "CAD�϶��妸�ഫ";
                btnDataBlkTrans.LongDescription = "CAD�϶��妸�ഫ";
                btnDataBlkTrans.LargeImage = imgSrcBlkTrans;
            };
            PushButton buttonBlkTrans = panel7.AddItem(btnDataBlkTrans) as PushButton;
            only2Ditem.Add(buttonBlkTrans);
            #endregion

            #region [Panel8] - �����޼���
            System.Drawing.Image image_horizontalTagSet = Properties.Resources._1__�ޱƼ��ҳ]�w96;
            ImageSource imgSrchHoriTagSet = GetImageSource(image_horizontalTagSet);
            PushButtonData btnHoriTagSet = new PushButtonData(
            "MyButton_HoriTagSet",
            "�]�w\n   �����޼���   ",
            ApiDllPath_PipeTags,
            "PipeTagger.Tag_setting"//���s�����W-->�n�̷ӻݭn�ѷӪ�command���J
            );
            {
                btnHoriTagSet.ToolTip = "�]�w�����޼���";
                btnHoriTagSet.LongDescription = "�]�w�s������޼���";
                btnHoriTagSet.LargeImage = imgSrchHoriTagSet;
            };

            System.Drawing.Image image_horizontalTagPlace = Properties.Resources._2__��m�ޱƺެq����96;
            ImageSource imgSrcHoriTagPlace = GetImageSource(image_horizontalTagPlace);
            PushButtonData btnHoriPlace = new PushButtonData(
            "MyButton_HoriTagPlace",
            "��m\n   �����޼���   ",
            ApiDllPath_PipeTags,
            "PipeTagger.Place_tag"//���s�����W-->�n�̷ӻݭn�ѷӪ�command���J
            );
            {
                btnHoriPlace.ToolTip = "��m�����޼���";
                btnHoriPlace.LongDescription = "��m�s������޼���";
                btnHoriPlace.LargeImage = imgSrcHoriTagPlace;
            };

            PushButton buttonHTagSet = panel8.AddItem(btnHoriTagSet) as PushButton;
            PushButton buttonHTagPlace = panel8.AddItem(btnHoriPlace) as PushButton;
            only2Ditem.Add(buttonHTagSet);
            only2Ditem.Add(buttonHTagPlace);

            #endregion

            #region[Panel Regis] - �g����qBIM-���U��T
            System.Drawing.Image image_Regis = Properties.Resources.Image20220111111226;
            ImageSource imgRegis = GetImageSource(image_Regis);
            PushButtonData btnRegis = new PushButtonData("MyButton_Regist", "���U��T", Assembly.GetExecutingAssembly().Location, "CEC_License.Command");
            {
                btnRegis.ToolTip = "�I����U�i��CEC MEP API���v�{��";
                btnRegis.LongDescription = $"�I����U�i��CEC MEP API���v�{��({versionNumber})";
                btnRegis.LargeImage = imgRegis;
            }
            PushButton pushButtonRegistration = panelRegis.AddItem(btnRegis) as PushButton;
            #endregion

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
        private void Application_ViewActivated(object sender, Autodesk.Revit.UI.Events.ViewActivatedEventArgs e)
        {
            UIApplication uiApp = sender as UIApplication;
            Document doc = uiApp.ActiveUIDocument.Document;
            RibbonItem rItem = null;

            //�u��b3D����
            foreach (RibbonItem item in only3Ditem)
            {
                rItem = item;
                if (doc.ActiveView.ViewType == ViewType.ThreeD && rItem != null && LicenseSetting.Default.CheckLicense)
                    rItem.Enabled = true;

                else
                    rItem.Enabled = false;
            }
            //�u��b2D����
            foreach (RibbonItem item in only2Ditem)
            {
                rItem = item;

                // if (doc.ActiveView.ViewType != ViewType.ThreeD && rItem != null)//�쥻�g�k�O�u�n���O3D���ϧY�i
                if (doc.ActiveView.ViewType == ViewType.FloorPlan && rItem != null && LicenseSetting.Default.CheckLicense) //�令�u���ӪO�����ϥi�H�ϥ�
                    rItem.Enabled = true;

                else
                    rItem.Enabled = false;
            }

            //�T�{item
            foreach (RibbonItem item in checkitem)
            {
                rItem = item;
                if (LicenseSetting.Default.CheckLicense)
                {
                    rItem.Enabled = true;
                }
                else
                {
                    rItem.Enabled = false;
                }
            }
        }
        private BitmapSource GetImageSource(Image img)
        {
            //�s�@�@��function�M���ӳB�z�Ϥ�
            BitmapImage bmp = new BitmapImage();

            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Png);
                ms.Position = 0;

                bmp.BeginInit();

                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = null;
                bmp.StreamSource = ms;

                bmp.EndInit();
            }

            return bmp;
        }
    }
}
