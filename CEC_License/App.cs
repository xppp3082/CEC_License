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
using System.Windows.Media.Imaging;//For BitmapImage ,Namespace = PresentationCore、System.Xaml

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
        string AssemblyFullName { get { return Assembly.GetExecutingAssembly().Location; } }// 取得目前dll檔的完整路徑+檔名
        //string AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); // 取得目前dll檔的資料夾位置
        //string TargetPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins";
        static Assembly app = Assembly.GetExecutingAssembly();
        string assemblyInfo = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        AssemblyName assemName = app.GetName();//讀取RevitAPI版本
        List<RibbonItem> only3Ditem = new List<RibbonItem>();//用於蒐集3D視圖才能使用的按鈕
        List<RibbonItem> only2Ditem = new List<RibbonItem>();//用於蒐集2D視圖才能使用的按鈕
        List<RibbonItem> checkitem = new List<RibbonItem>();//蒐集要控制的按鈕
        public Result OnStartup(UIControlledApplication a)
        {
            string versionNumber = a.ControlledApplication.VersionNumber;
            //取得主程式碼的地方，如果分開需要再想想
            //string TargetPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\2019";
            string TargetPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\";//C:\Users\工號\AppData\Roaming\Autodesk\Revit\Addins

            //創造extendPath
            string ApiDllPath_Hanger = TargetPath + versionNumber + @"\AutoHangerCreation_ButtonCreate.dll";
            string ApiDllPath_BeamCast = TargetPath + versionNumber + @"\BeamCasing_ButtonCreate.dll";
            string ApiDllPath_WallCast = TargetPath + versionNumber + @"\CEC_WallCast.dll";
            string ApiDllPath_PreFab = TargetPath + versionNumber + @"\CEC_PreFabric.dll";
            string ApiDllPath_BlockTrans = TargetPath + versionNumber + @"\CEC_CADBlockTrans.dll";
            string ApiDllPath_PipeTags = TargetPath + versionNumber + @"\PipeTagger.dll";
            string ApiDllPath_EquipsCount = TargetPath + versionNumber + @"\CEC_Count.dll";
            string ApiDllPath_NumberRule = TargetPath + versionNumber + @"\CEC_NumRule.dll";

            string RevitApiDllPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\CEC_API\CEC_RevitAPI2019.dll";
            string ThisDllPath = AssemblyFullName;

            //拿取Dll的邏輯或許可以變成-->創建三個資料夾CEC MEP>2019、2021、2023，然後利用在載入時讀取Revit版本進行要用哪一個dll的判斷
            const string RIBBON_TAB = "●CEC-機電API(2)";
            const string RIBBON_PANEL1 = "管吊架";
            const string RIBBON_PANEL2 = "穿樑開口";
            const string RIBBON_PANEL3 = "穿樑CSD&SEM";
            const string RIBBON_PANEL4 = "穿牆開口";
            const string RIBBON_PANEL5 = "穿牆CSD&SEM";
            const string RIBBON_PANEL_Slab = "穿版開口";
            const string RIBBON_PANEL_SlabCSD = "穿版CSD&SEM";
            const string RIBBON_PANEL6 = "管線預組";
            const string RIBBON_PANEL7 = "圖塊轉換";
            const string RIBBON_PANEL8 = "水平管標籤";
            const string RIBBON_PANEL9 = "數量計算";
            const string RIBBON_Regis = "授權註冊";

            #region 判斷授權
            //判斷授權
            String lbProductID = ComputerInfo.GetComputerId();
            KeyManager km = new KeyManager(lbProductID);
            LicenseInfo lic = new LicenseInfo();
            //C:\Users\工號\AppData\Roaming\Autodesk\Revit\Addins\CEClicens.lic
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
                        //MessageBox.Show("已授權");
                        LicenseSetting.Default.CheckLicense = true;
                    }
                }
            }
            //創造ribbon tab
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
            //視圖改變與啟動時反覆確認授權
            a.ViewActivated += Application_ViewActivated;

            //建立panel-->是否需要防錯機制待確認
            RibbonPanel panel1 = a.CreateRibbonPanel(RIBBON_TAB, RIBBON_PANEL1);//管吊架
            RibbonPanel panel2 = a.CreateRibbonPanel(RIBBON_TAB, RIBBON_PANEL2);//穿樑開口
            RibbonPanel panel3 = a.CreateRibbonPanel(RIBBON_TAB, RIBBON_PANEL3);//穿樑CSD&SEM
            RibbonPanel panel4 = a.CreateRibbonPanel(RIBBON_TAB, RIBBON_PANEL4);//穿牆開口
            RibbonPanel panel5 = a.CreateRibbonPanel(RIBBON_TAB, RIBBON_PANEL5);//穿牆CSD&SEM
            RibbonPanel panelSlab = a.CreateRibbonPanel(RIBBON_TAB, RIBBON_PANEL_Slab);//穿版開口
            RibbonPanel panelSlabCSD = a.CreateRibbonPanel(RIBBON_TAB, RIBBON_PANEL_SlabCSD);//穿版CSD&SEM
            RibbonPanel panel6 = a.CreateRibbonPanel(RIBBON_TAB, RIBBON_PANEL6);//管線預組
            RibbonPanel panel7 = a.CreateRibbonPanel(RIBBON_TAB, RIBBON_PANEL7);//圖塊轉換
            RibbonPanel panel8 = a.CreateRibbonPanel(RIBBON_TAB, RIBBON_PANEL8);//水平管標籤
            RibbonPanel panel9 = a.CreateRibbonPanel(RIBBON_TAB, RIBBON_PANEL9);//數量計算
            RibbonPanel panelRegis = a.CreateRibbonPanel(RIBBON_TAB, RIBBON_Regis);//註冊資訊

            #region[Panel1] - 土木機電BIM-管吊架
            //創建單管吊架
            System.Drawing.Image image_Single = Properties.Resources.單管_多管V2__轉換__02__96dpi;
            ImageSource imgSrc = GetImageSource(image_Single);
            //創建多管吊架
            System.Drawing.Image image_Multi = Properties.Resources.單管_多管V2__轉換__01__96dpi;
            ImageSource imgSrc2 = GetImageSource(image_Multi);
            //設定單管吊架
            System.Drawing.Image image_SetUp = Properties.Resources.單管吊架設定_32pix;
            ImageSource imgSrc3 = GetImageSource(image_SetUp);

            PushButtonData btnData = new PushButtonData("MyButton_Single", "創建\n單管吊架", ApiDllPath_Hanger, "AutoHangerCreation_ButtonCreate.AddHangerByMouseLink");
            {
                btnData.ToolTip = "點選管段創建單管吊架";
                btnData.LongDescription = $"點選需要創建的管段，生成單管吊架({versionNumber})";
                btnData.LargeImage = imgSrc;
            };
            PushButtonData btnData2 = new PushButtonData("MyButton_Multi", "創建\n多管吊架", ApiDllPath_Hanger, "AutoHangerCreation_ButtonCreate.MultiHangerCreationV3");
            {
                btnData2.ToolTip = "點選管段創建多管吊架";
                btnData2.LongDescription = $"點選需要創建的管段，生成多管吊架，單次最多選擇八支管({versionNumber})";
                btnData2.LargeImage = imgSrc2;
            }
            PushButtonData btnData3 = new PushButtonData("MyButton_SetUp", "設定\n吊架", ApiDllPath_Hanger, "AutoHangerCreation_ButtonCreate.PipeHangerSetUp");
            {
                btnData3.ToolTip = "設定吊架類型與間距";
                btnData3.LongDescription = $"設定自動放置單管吊架所需的吊架類型與間距，設定後才能使用單管吊架功能({assemblyInfo})";
                btnData3.LargeImage = imgSrc3;
            }
            PushButton button = panel1.AddItem(btnData) as PushButton;
            ContextualHelp singleHangHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-a45761cab9f343f3980d08c52209e3ba?pvs=4");
            button.SetContextualHelp(singleHangHelp);
            PushButton button2 = panel1.AddItem(btnData2) as PushButton;
            ContextualHelp multiHangHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-6a6435f0574a42ed931e41c75bf5b9ec?pvs=4");
            button2.SetContextualHelp(multiHangHelp);
            PushButton button3 = panel1.AddItem(btnData3) as PushButton;
            ContextualHelp setHangHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-aa9267be3e6a4afa930c8529f3a6e8a3?pvs=4");
            button3.SetContextualHelp(setHangHelp);
            checkitem.Add(button);
            checkitem.Add(button2);
            checkitem.Add(button3);
            #endregion

            #region[Panel2] - 土木機電BIM-穿樑開口
            System.Drawing.Image image_CreateST = Properties.Resources.穿樑套管ICON合集_ST;
            ImageSource imgSrcST = GetImageSource(image_CreateST);
            System.Drawing.Image image_CreateSTLink = Properties.Resources.穿樑套管ICON合集_STlink;
            ImageSource imgSrcSTLink = GetImageSource(image_CreateSTLink);
            System.Drawing.Image image_Create = Properties.Resources.穿樑套管ICON合集_RC;
            ImageSource imgSrcRC = GetImageSource(image_Create);
            System.Drawing.Image image_CreateLink = Properties.Resources.穿樑套管ICON合集_RClink;
            ImageSource imgSrcRCLink = GetImageSource(image_CreateLink);
            System.Drawing.Image image_Update = Properties.Resources.穿樑套管ICON合集_更新;
            ImageSource imgSrcUpdate = GetImageSource(image_Update);
            System.Drawing.Image image_UpdatePart = Properties.Resources.根據視圖範圍更新穿樑資訊_;
            ImageSource imgSrcUpdatePart = GetImageSource(image_UpdatePart);
            System.Drawing.Image image_SetCast = Properties.Resources.穿樑套管ICON合集_設定;
            ImageSource imgSrcSetCast = GetImageSource(image_SetCast);
            System.Drawing.Image image_Num = Properties.Resources.穿樑套管ICON合集_編號2;
            ImageSource imgSrcNum = GetImageSource(image_Num);
            System.Drawing.Image image_ReNum = Properties.Resources.穿樑套管ICON合集_重編號2;
            ImageSource imgSrcReNum = GetImageSource(image_ReNum);
            System.Drawing.Image image_Copy = Properties.Resources.副穿樑套管ICON合集_複製;
            ImageSource imgSrcCopy = GetImageSource(image_Copy);
            System.Drawing.Image image_CopyPart = Properties.Resources.穿樑套管ICON合集_逐層複製;
            ImageSource imgSrcCopyPart = GetImageSource(image_CopyPart);
            System.Drawing.Image image_Rect = Properties.Resources.穿樑套管ICON合集_方形開口;
            ImageSource imgSrcRect = GetImageSource(image_Rect);
            System.Drawing.Image image_RectLink = Properties.Resources.穿樑套管ICON合集_方形開口link;
            ImageSource imgSrcRectLink = GetImageSource(image_RectLink);
            System.Drawing.Image image_MultiOpen = Properties.Resources.穿樑套管ICON合集_多管開口;
            ImageSource imgSrcMultiOpen = GetImageSource(image_MultiOpen);
            System.Drawing.Image image_MultiLink = Properties.Resources.穿樑套管ICON合集_多管開口link;
            ImageSource imgSrcMultiLink = GetImageSource(image_MultiLink);

            PushButtonData btnDataST = new PushButtonData(
            "MyButton_CastCreateST",
            "鋼構開孔",
            ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.CreateBeamCastSTV2"//按鈕的全名-->要依照需要參照的command打入
            );
            {
                btnDataST.ToolTip = "點選管與外參樑生成穿樑開口";
                btnDataST.LongDescription = $"先點選需要創建的管段，再點選其穿過的外參樑，生成穿樑套管({assemblyInfo})";
                btnDataST.LargeImage = imgSrcST;
            };

            PushButtonData btnDataSTLink = new PushButtonData(
            "MyButton_CastCreateSTLink",
            "鋼構開孔(連結)",
            ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.CreateBeamCastLinkST"//按鈕的全名-->要依照需要參照的command打入
            );
            {
                btnDataSTLink.ToolTip = "點選外參管與外參樑生成穿樑開口";
                btnDataSTLink.LongDescription = $"先點選需要創建的管段，再點選其穿過的外參樑，生成穿樑套管({assemblyInfo})";
                btnDataSTLink.LargeImage = imgSrcSTLink;
            };

            PushButtonData btnDataRC = new PushButtonData(
            "MyButton_CastCreate",
            "RC套管",
            ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.CreateBeamCastV2"//按鈕的全名-->要依照需要參照的command打入
            );
            {
                btnDataRC.ToolTip = "點選管與外參樑生成穿樑套管";
                btnDataRC.LongDescription = $"先點選需要創建的管段，再點選其穿過的外參樑，生成穿樑套管({assemblyInfo})";
                btnDataRC.LargeImage = imgSrcRC;
            };

            PushButtonData btnDataRCLink = new PushButtonData(
            "MyButton_CastCreateLink",
            "RC套管(連結)",
            ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.CreateBeamCastLink"//按鈕的全名-->要依照需要參照的command打入
            );
            {
                btnDataRCLink.ToolTip = "點選外參管與外參樑生成穿樑套管";
                btnDataRCLink.LongDescription = $"先點選需要創建的管段，再點選其穿過的外參樑，生成穿樑套管({assemblyInfo})";
                btnDataRCLink.LargeImage = imgSrcRCLink;
            };

            PushButtonData btnDataReNew = new PushButtonData(
             "MyButton_CastUpdate",
             "更新\n穿樑資訊",
             ApiDllPath_BeamCast,
             "BeamCasing_ButtonCreate.CastInfromUpdateV4"
             );
            {
                btnDataReNew.ToolTip = "一鍵更新穿樑套管與穿樑開口資訊";
                btnDataReNew.LongDescription = $"依照本案設定的穿梁原則，更新穿樑套管資訊 (必須先設定穿樑原則方可使用)({assemblyInfo})";
                btnDataReNew.LargeImage = imgSrcUpdate;
            }

            PushButtonData btnDataUpdatePart = new PushButtonData(
            "MyButton_CastUpdatePart",
            "居部更新\n穿樑資訊",
            ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.CastInfromUpdatePart"
            );
            {
                btnDataUpdatePart.ToolTip = "依照目前視圖範圍，局部更新穿樑套管資訊";
                btnDataUpdatePart.LongDescription = $"依照目前視圖範圍，局部更新穿樑套管資訊 (必須先設定穿樑原則方可使用)({assemblyInfo})";
                btnDataUpdatePart.LargeImage = imgSrcUpdatePart;
            }

            PushButtonData btnDataSetCast = new PushButtonData(
            "MyButton_CastSetUp",
            "設定\n穿樑原則   ",
            ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.BeamCastSetUp"
             );
            {
                btnDataSetCast.ToolTip = "設定穿樑原則限制";
                btnDataSetCast.LongDescription = $"依據專案需求，設定本案的穿樑原則資訊({assemblyInfo})";
                btnDataSetCast.LargeImage = imgSrcSetCast;
            }

            PushButtonData btnDataRect = new PushButtonData(
            "MyButton_CastRect",
            "方形樑開孔",
            ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.CreateRectBeamCast"
            );
            {
                btnDataRect.ToolTip = "點選管與外參樑生成穿樑套管";
                btnDataRect.LongDescription = $"先點選需要創建的管段，再點選其穿過的外參樑，生成穿樑套管({assemblyInfo})";
                btnDataRect.LargeImage = imgSrcRect;
            }

            PushButtonData btnDataRectLink = new PushButtonData(
            "MyButton_CastRectLink",
            "方形樑開孔(連結)",
            ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.CreateRectBeamCastLink"
            );
            {
                btnDataRectLink.ToolTip = "點選外參管與外參樑生成穿樑套管";
                btnDataRectLink.LongDescription = $"先點選需要創建的管段，再點選其穿過的外參樑，生成穿樑套管({assemblyInfo})";
                btnDataRectLink.LargeImage = imgSrcRectLink;
            }

            PushButtonData btnDataMultiOpen = new PushButtonData(
            "MyButton_MultiRect",
            "多管樑開孔",
            ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.MultiBeamRectCast"
            );
            {
                btnDataMultiOpen.ToolTip = "點選多支管與外參樑生成穿牆方開口";
                btnDataMultiOpen.LongDescription = $"先點選需要創建的管段(複數)，再點選其穿過的外參牆，生成穿樑方開口({assemblyInfo})";
                btnDataMultiOpen.LargeImage = imgSrcMultiOpen;
            }

            PushButtonData btnDataMultiLink = new PushButtonData(
            "MyButton_MultiRectLink",
            "多管樑開孔(連結)",
            ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.MultiBeamRectCastLink"
            );
            {
                btnDataMultiLink.ToolTip = "點選多支外參管與外參樑生成穿牆方開口";
                btnDataMultiLink.LongDescription = $"先點選需要創建的管段(複數)，再點選其穿過的外參牆，生成穿樑方開口({assemblyInfo})";
                btnDataMultiLink.LargeImage = imgSrcMultiLink;
            }

            //更新穿樑資訊(更新&設定)
            SplitButtonData setUpButtonData = new SplitButtonData("CastSetUpButton", "穿樑套管更新");
            SplitButton splitButton1 = panel2.AddItem(setUpButtonData) as SplitButton;
            PushButton buttonRenew = splitButton1.AddPushButton(btnDataReNew);
            ContextualHelp reNewHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-e8d941557c5b4c8297bea474826b0eac?pvs=4");
            buttonRenew.SetContextualHelp(reNewHelp);
            splitButton1.SetContextualHelp(reNewHelp);
            PushButton buttonUpdatePart = splitButton1.AddPushButton(btnDataUpdatePart);
            ContextualHelp updatePart = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-c89a508ab61c4bd8b0356dac67813317?pvs=4");
            buttonUpdatePart.SetContextualHelp(updatePart);
            PushButton buttonSetCast = splitButton1.AddPushButton(btnDataSetCast);
            ContextualHelp setCastHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-448ab69ca6a54afc844cbdb1872a83a1?pvs=4");
            buttonSetCast.SetContextualHelp(setCastHelp);
            checkitem.Add(buttonRenew);
            only2Ditem.Add(buttonUpdatePart);
            checkitem.Add(buttonSetCast);

            //創建穿樑套管(ST&RC)
            SplitButtonData STButtonData = new SplitButtonData("CreateCastST", "鋼構開孔");
            SplitButton splitButtonST = panel2.AddItem(STButtonData) as SplitButton;
            PushButton STbutton = splitButtonST.AddPushButton(btnDataST);
            ContextualHelp stButtonHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-a812fb60d2b44db3baad2c8445aa8151?pvs=4");
            STbutton.SetContextualHelp(stButtonHelp);
            splitButtonST.SetContextualHelp(stButtonHelp);
            PushButton STbuttonLink = splitButtonST.AddPushButton(btnDataSTLink);
            ContextualHelp stButtonLinkHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-a812fb60d2b44db3baad2c8445aa8151?pvs=4");
            STbuttonLink.SetContextualHelp(stButtonLinkHelp);
            checkitem.Add(STbutton);
            checkitem.Add(STbuttonLink);

            SplitButtonData RCButtonData = new SplitButtonData("CreateCast", "RC套管");
            SplitButton splitButtonRC = panel2.AddItem(RCButtonData) as SplitButton;
            PushButton RCbutton = splitButtonRC.AddPushButton(btnDataRC);
            ContextualHelp rcButtonHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-RC-05318fa9db3f47aabc66b4a082c1d4cb?pvs=4");
            RCbutton.SetContextualHelp(rcButtonHelp);
            splitButtonRC.SetContextualHelp(rcButtonHelp);
            PushButton RCbuttonLink = splitButtonRC.AddPushButton(btnDataRCLink);
            ContextualHelp rcButtonLinkHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-RC-05318fa9db3f47aabc66b4a082c1d4cb?pvs=4");
            RCbuttonLink.SetContextualHelp(rcButtonLinkHelp);
            checkitem.Add(RCbutton);
            checkitem.Add(RCbuttonLink);

            SplitButtonData rectCastButtonData = new SplitButtonData("RectCastButton", "方形樑開口");
            SplitButton splitButtonRect = panel2.AddItem(rectCastButtonData) as SplitButton;
            PushButton buttonRect = splitButtonRect.AddPushButton(btnDataRect);
            ContextualHelp rectHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-7223974d90e346c283bafb2ea6ae9550?pvs=4");
            buttonRect.SetContextualHelp(rectHelp);
            splitButtonRect.SetContextualHelp(rectHelp);
            PushButton buttonRectLink = splitButtonRect.AddPushButton(btnDataRectLink);
            ContextualHelp rectLinkHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-7223974d90e346c283bafb2ea6ae9550?pvs=4");
            buttonRectLink.SetContextualHelp(rectLinkHelp);
            PushButton buttonMultiOpen = splitButtonRect.AddPushButton(btnDataMultiOpen);
            ContextualHelp multiRectHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-336d33762f3c4760b08f0503dd517ffa?pvs=4");
            buttonMultiOpen.SetContextualHelp(multiRectHelp);
            PushButton buttonMultiLink = splitButtonRect.AddPushButton(btnDataMultiLink);
            ContextualHelp multiRectLinkHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-336d33762f3c4760b08f0503dd517ffa?pvs=4");
            buttonMultiOpen.SetContextualHelp(multiRectLinkHelp);
            checkitem.Add(buttonRect);
            checkitem.Add(buttonRectLink);
            checkitem.Add(buttonMultiOpen);
            checkitem.Add(buttonMultiLink);
            #endregion

            #region[Panel3] - 土木機電BIM-穿樑CSD&SEM
            PushButtonData btnDataNum = new PushButtonData(
            "MyButton_CastNum",
            "穿樑套管\n編號",
            ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.UpdateCastNumber"
            );
            {
                btnDataNum.ToolTip = "穿樑套管自動編號";
                btnDataNum.LongDescription = $"根據每層樓的開口數量與位置，依序自動帶入編號，第二次上入編號時則會略過已經填入編號的套管({assemblyInfo})";
                btnDataNum.LargeImage = imgSrcNum;
            }

            PushButtonData btnDataReNum = new PushButtonData(
            "MyButton_ReNum",
            "穿樑套管\n重新編號",
            ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.ReUpdateCastNumber"
            );
            {
                btnDataReNum.ToolTip = "穿樑套管重新編號";
                btnDataReNum.LongDescription = $"根據每層樓的開口數量，重新帶入編號({assemblyInfo})";
                btnDataReNum.LargeImage = imgSrcReNum;
            }

            PushButtonData btnDataCopy = new PushButtonData(
            "MyButton_CopyLinked",
            "複製外參\n穿樑套管",
           ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.CopyAllCast"
            );
            {
                btnDataCopy.ToolTip = "複製所有連結模型中的套管";
                btnDataCopy.LongDescription = $"複製所有連結模型中的套管，以供SEM開口編號用({assemblyInfo})";
                btnDataCopy.LargeImage = imgSrcCopy;
            }


            PushButtonData btnDataCopyPart = new PushButtonData(
            "MyButton_CopyPartLinked",
            "依樓層複製\n穿樑套管",
           ApiDllPath_BeamCast,
            "BeamCasing_ButtonCreate.CopyPartCast"
            );
            {
                btnDataCopyPart.ToolTip = "依視圖參考樓層複製連結模型中的套管";
                btnDataCopyPart.LongDescription = $"依視圖參考樓層複製連結模型中的套管，以供SEM開口編號用({assemblyInfo})";
                btnDataCopyPart.LargeImage = imgSrcCopyPart;
            }

            //複製所有套管&局部複製
            SplitButtonData copyCastButtonData = new SplitButtonData("CopyCastButton", "複製外參\n   穿樑套管");
            SplitButton splitButtonCopyCast = panel3.AddItem(copyCastButtonData) as SplitButton;
            PushButton buttonCopy = splitButtonCopyCast.AddPushButton(btnDataCopy);
            ContextualHelp copyHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-b0894ec0bf074798b42a227190beb1bd?pvs=4");
            buttonCopy.SetContextualHelp(copyHelp);
            splitButtonCopyCast.SetContextualHelp(copyHelp);
            PushButton buttonCopyPart = splitButtonCopyCast.AddPushButton(btnDataCopyPart);
            ContextualHelp copyPartHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-f0f15b474cce443e8b3c850a41494bfc?pvs=4");
            buttonCopyPart.SetContextualHelp(copyPartHelp);
            checkitem.Add(buttonCopy);
            only2Ditem.Add(buttonCopyPart);

            //穿樑套管編號(編號&重編)
            SplitButtonData setNumButtonData = new SplitButtonData("CastSetNumButton", "穿樑套管編號");
            SplitButton splitButtonSetNum = panel3.AddItem(setNumButtonData) as SplitButton;
            PushButton buttonNum = splitButtonSetNum.AddPushButton(btnDataNum);
            ContextualHelp numHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-024a30c861694ab1934a9422d988181a?pvs=4");
            buttonNum.SetContextualHelp(numHelp);
            splitButtonSetNum.SetContextualHelp(numHelp);
            PushButton buttonReNum = splitButtonSetNum.AddPushButton(btnDataReNum);
            ContextualHelp reNumHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-20ddb82dc1fd453987e2577ba4ec2513?pvs=4");
            buttonReNum.SetContextualHelp(reNumHelp);
            checkitem.Add(buttonNum);
            checkitem.Add(buttonReNum);
            #endregion

            #region[Panel4] - 土木機電BIM-穿牆開口
            //更新穿牆資訊
            System.Drawing.Image image_UpdateWall = Properties.Resources.穿牆套管ICON合集_更新_svg;
            ImageSource imgSrcUpdateWall = GetImageSource(image_UpdateWall);
            //局部更新穿牆資訊
            System.Drawing.Image image_UpdateWallPart = Properties.Resources.穿牆套管ICON合集_局部更新_svg;
            ImageSource imgSrcUpdateWallPart = GetImageSource(image_UpdateWallPart);
            //放置穿牆套管
            System.Drawing.Image image_WallCast = Properties.Resources.穿牆套管ICON合集_放置_svg;
            ImageSource imgSrcWallCast = GetImageSource(image_WallCast);
            //放置穿牆套管(連結)
            System.Drawing.Image image_WallCastLink = Properties.Resources.穿牆套管ICON合集_放置link_svg;
            ImageSource imgSrcWallCastLink = GetImageSource(image_WallCastLink);
            //複製外參套管
            System.Drawing.Image image_CopyWallCast = Properties.Resources.穿牆套管ICON合集_複製外參_svg;
            ImageSource imgSrcCopyWallCast = GetImageSource(image_CopyWallCast);
            //複製外參套管(依樓層)
            System.Drawing.Image image_CopyWallCastPart = Properties.Resources.穿牆套管ICON合集_複製逐層_svg;
            ImageSource imgSrcCopyWallCastPart = GetImageSource(image_CopyWallCastPart);
            //穿牆套管編號
            System.Drawing.Image image_WallNum = Properties.Resources.穿牆套管ICON合集_編號_svg;
            ImageSource imgSrcSetNum = GetImageSource(image_WallNum);
            //穿牆套管重新編號
            System.Drawing.Image image_ReWallNum = Properties.Resources.穿牆套管ICON合集_重新編號_svg;
            ImageSource imgSrcReWallNum = GetImageSource(image_ReWallNum);
            //放置方形牆開口
            System.Drawing.Image image_WallRect = Properties.Resources.穿牆套管ICON合集_方開口_svg;
            ImageSource imgSrcWallRect = GetImageSource(image_WallRect);
            //放置方形牆開口(連結)
            System.Drawing.Image image_WallRectLink = Properties.Resources.穿牆套管ICON合集_方開口link_svg;
            ImageSource imgSrcWallRectLink = GetImageSource(image_WallRectLink);
            //放置多管方開口
            System.Drawing.Image image_MultiWallRect = Properties.Resources.穿牆套管ICON合集_多管方開口_svg;
            ImageSource imgSrcMultiWallRect = GetImageSource(image_MultiWallRect);
            //放置多管方開口(連結)
            System.Drawing.Image image_MultiRectLink = Properties.Resources.穿牆套管ICON合集_多管方開口link_svg;
            ImageSource imgSrcMultiRectLink = GetImageSource(image_MultiRectLink);

            PushButtonData btnDataWallCastUpdate = new PushButtonData(
            "MyButton_WallCastUpdate",
            "更新\n穿牆資訊",
            ApiDllPath_WallCast,
            "CEC_WallCast.WallCastUpdate"//按鈕的全名-->要依照需要參照的command打入
            );
            {
                btnDataWallCastUpdate.ToolTip = "一鍵更新穿牆開口資訊";
                btnDataWallCastUpdate.LongDescription = $"一鍵更新穿牆開口資訊({assemblyInfo})";
                btnDataWallCastUpdate.LargeImage = imgSrcUpdateWall;
            };
            PushButtonData btnDataWallCastUpdatePart = new PushButtonData(
            "MyButton_WallCastUpdatePart",
            "居部更新\n穿牆資訊",
            ApiDllPath_WallCast,
            "CEC_WallCast.WallCastUpdatePart"//按鈕的全名-->要依照需要參照的command打入
            );
            {
                btnDataWallCastUpdatePart.ToolTip = "依照目前視圖範圍，局部更新穿牆開口資訊";
                btnDataWallCastUpdatePart.LongDescription = $"依照目前視圖範圍，局部更新穿牆開口資訊({assemblyInfo})";
                btnDataWallCastUpdatePart.LargeImage = imgSrcUpdateWallPart;
            };
            PushButtonData btnDataWallCastCreate = new PushButtonData(
            "MyButton_WallCastCreate",
            "穿牆套管",
            ApiDllPath_WallCast,
            "CEC_WallCast.CreateWallCastV2"//按鈕的全名-->要依照需要參照的command打入
            );
            {
                btnDataWallCastCreate.ToolTip = "點選管與外參牆生成穿牆套管";
                btnDataWallCastCreate.LongDescription = $"先點選需要創建的管段，再點選其穿過的外參牆，生成穿牆套管({assemblyInfo})";
                btnDataWallCastCreate.LargeImage = imgSrcWallCast;
            };
            PushButtonData btnDataWallCastlink = new PushButtonData(
            "MyButton_WallCastCreateLink",
            "穿牆套管(連結)",
            ApiDllPath_WallCast,
            "CEC_WallCast.CreateWallCastLink"//按鈕的全名-->要依照需要參照的command打入
            );
            {
                btnDataWallCastlink.ToolTip = "點選外參管與外參牆生成穿牆套管";
                btnDataWallCastlink.LongDescription = $"先點選需要創建的管段，再點選其穿過的外參牆，生成穿牆套管({assemblyInfo})";
                btnDataWallCastlink.LargeImage = imgSrcWallCastLink;
            };
            PushButtonData btnDataWallCastRect = new PushButtonData(
            "MyButton_WallCastRect",
            "方型牆開口",
            ApiDllPath_WallCast,
            "CEC_WallCast.CreateRectWallCast"
            );
            {
                btnDataWallCastRect.ToolTip = "點選管與外參牆生成穿牆方開口";
                btnDataWallCastRect.LongDescription = $"先點選需要創建的管段，再點選其穿過的外參牆，生成穿牆方開口({assemblyInfo})";
                btnDataWallCastRect.LargeImage = imgSrcWallRect;
            }
            PushButtonData btnDataWallCastRectLink = new PushButtonData(
            "MyButton_WallCastRectLink",
            "方型牆開口(連結)",
            ApiDllPath_WallCast,
            "CEC_WallCast.CreateRectWallCastLink"
            );
            {
                btnDataWallCastRectLink.ToolTip = "點選外參管與外參牆生成穿牆方開口";
                btnDataWallCastRectLink.LongDescription = $"先點選需要創建的外參管段，再點選其穿過的外參牆，生成穿牆方開口({assemblyInfo})";
                btnDataWallCastRectLink.LargeImage = imgSrcWallRectLink;
            }
            PushButtonData btnDataWallCastRectMulti = new PushButtonData(
            "MyButton_WallCastRectMulti",
            "多管牆開口",
            ApiDllPath_WallCast,
            "CEC_WallCast.MultiWallRectCast"
            );
            {
                btnDataWallCastRectMulti.ToolTip = "點選外參牆與多支管生成穿牆方開口";
                btnDataWallCastRectMulti.LongDescription = $"先點選需要創建的管段(複數)，再點選其穿過的外參牆，生成穿牆方開口({assemblyInfo})";
                btnDataWallCastRectMulti.LargeImage = imgSrcMultiWallRect;
            }
            PushButtonData btnDataMultiRectLink = new PushButtonData(
            "MyButton_WallCastRectMultiLink",
            "多管牆開口(連結)",
             ApiDllPath_WallCast,
            "CEC_WallCast.MultiWallRectCastLink"
            );
            {
                btnDataMultiRectLink.ToolTip = "點選外參牆與多支管生成穿牆方開口";
                btnDataMultiRectLink.LongDescription = $"先點選需要創建的管段(複數)，再點選其穿過的外參牆，生成穿牆方開口({assemblyInfo})";
                btnDataMultiRectLink.LargeImage = imgSrcMultiRectLink;
            }

            //更新穿牆套管
            SplitButtonData updateButtonData = new SplitButtonData("UpdateCast", "更新\n   穿牆資訊");
            SplitButton splitButtonWallUpdate = panel4.AddItem(updateButtonData) as SplitButton;
            PushButton buttonWallCastUpdate = splitButtonWallUpdate.AddPushButton(btnDataWallCastUpdate);
            ContextualHelp wallCastUpdateHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-13f7097a668947b8b161338d7e01868d?pvs=4");
            buttonWallCastUpdate.SetContextualHelp(wallCastUpdateHelp);
            splitButtonWallUpdate.SetContextualHelp(wallCastUpdateHelp);
            PushButton buttonWallCastUpdatePart = splitButtonWallUpdate.AddPushButton(btnDataWallCastUpdatePart);
            ContextualHelp wallCastUpdatePart = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-a210f66da98346eda94183b2b74b6fec?pvs=4");
            buttonWallCastUpdatePart.SetContextualHelp(wallCastUpdatePart);
            checkitem.Add(buttonWallCastUpdate);
            only2Ditem.Add(buttonWallCastUpdatePart);

            //創建穿牆套管(圓)
            SplitButtonData wallCastButtonData = new SplitButtonData("WallCast", "穿牆套管");
            SplitButton splitButtonWallCast = panel4.AddItem(wallCastButtonData) as SplitButton;
            PushButton buttonWallCast = splitButtonWallCast.AddPushButton(btnDataWallCastCreate);
            ContextualHelp wallCastHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-9ca40a440c284e12b81d4568c2553c51?pvs=4");
            buttonWallCast.SetContextualHelp(wallCastHelp);
            splitButtonWallCast.SetContextualHelp(wallCastHelp);
            PushButton buttonWallCastLink = splitButtonWallCast.AddPushButton(btnDataWallCastlink);
            ContextualHelp wallCastLinkHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-9ca40a440c284e12b81d4568c2553c51?pvs=4");
            buttonWallCastLink.SetContextualHelp(wallCastLinkHelp);
            checkitem.Add(buttonWallCast);
            checkitem.Add(buttonWallCastLink);

            //創建穿牆套管(方)
            SplitButtonData wallRectCastButtonData = new SplitButtonData("WallCastRect", "方型牆開口");
            SplitButton splitButtonWallRect = panel4.AddItem(wallRectCastButtonData) as SplitButton;
            PushButton buttonWallCastRect = splitButtonWallRect.AddPushButton(btnDataWallCastRect);
            ContextualHelp wallCastRectHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-fda59b168acf40f785ec0da94e31d7a4?pvs=4");
            buttonWallCastRect.SetContextualHelp(wallCastRectHelp);
            splitButtonWallRect.SetContextualHelp(wallCastRectHelp);
            PushButton buttonWallCastRectLink = splitButtonWallRect.AddPushButton(btnDataWallCastRectLink);
            ContextualHelp wallCastRectLinkHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-fda59b168acf40f785ec0da94e31d7a4?pvs=4");
            buttonWallCastRectLink.SetContextualHelp(wallCastRectLinkHelp);
            PushButton buttonWallCastRectMulti = splitButtonWallRect.AddPushButton(btnDataWallCastRectMulti);
            ContextualHelp wallCastRectMultiHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-336d33762f3c4760b08f0503dd517ffa?pvs=4");
            buttonWallCastRectMulti.SetContextualHelp(wallCastRectMultiHelp);
            PushButton buttonWallMultiRectLink = splitButtonWallRect.AddPushButton(btnDataMultiRectLink);
            buttonWallMultiRectLink.SetContextualHelp(wallCastRectMultiHelp);
            checkitem.Add(buttonWallCastRect);
            checkitem.Add(buttonWallCastRectLink);
            checkitem.Add(buttonWallCastRectMulti);
            checkitem.Add(buttonWallMultiRectLink);
            #endregion

            #region[Panel5] - 土木機電BIM-穿牆CSD&SEM
            PushButtonData btnDataCopyWallCast = new PushButtonData(
            "MyButton_WallCastCopy",
            "複製外參\n穿牆套管",
            ApiDllPath_WallCast,
            "CEC_WallCast.CopyAllWallCast"
            );
            {
                btnDataCopyWallCast.ToolTip = "複製所有連結模型中的套管";
                btnDataCopyWallCast.LongDescription = $"複製所有連結模型中的套管，以供SEM開口編號用({assemblyInfo})";
                btnDataCopyWallCast.LargeImage = imgSrcCopyWallCast;
            }
            PushButtonData btnDataCopyWallCastPart = new PushButtonData(
            "MyButton_WallCastCopyPart",
            "依樓層複製\n穿牆套管",
            ApiDllPath_WallCast,
            "CEC_WallCast.CopyPartWallCast"
            );
            {
                btnDataCopyWallCastPart.ToolTip = "依視圖參考樓層複製連結模型中的套管";
                btnDataCopyWallCastPart.LongDescription = $"依視圖參考樓層複製連結模型中的套管，以供SEM開口編號用({assemblyInfo})";
                btnDataCopyWallCastPart.LargeImage = imgSrcCopyWallCastPart;
            }

            PushButtonData btnDataWallCastNum = new PushButtonData(
            "MyButton_WallCastNum",
            "穿牆套管\n編號",
            ApiDllPath_WallCast,
            "CEC_WallCast.UpdateWallCastNumber"
            );
            {
                btnDataWallCastNum.ToolTip = "穿牆套管自動編號";
                btnDataWallCastNum.LongDescription = $"根據每層樓的開口數量與位置，依序自動帶入編號，第二次上入編號時則會略過已經填入編號的套管({assemblyInfo})";
                btnDataWallCastNum.LargeImage = imgSrcSetNum;
            }
            PushButtonData btnDataWallCastReNum = new PushButtonData(
            "MyButton_WallCastReNum",
            "穿牆套管\n重新編號",
            ApiDllPath_WallCast,
            "CEC_WallCast.ReUpdateWallCastNumber"
            );
            {
                btnDataWallCastReNum.ToolTip = "穿牆套管重新編號";
                btnDataWallCastReNum.LongDescription = $"根據每層樓的開口數量，重新帶入編號({assemblyInfo})";
                btnDataWallCastReNum.LargeImage = imgSrcReWallNum;
            }

            //複製所有穿牆套管
            SplitButtonData copyWallCastButtonData = new SplitButtonData("CopyWallCastButton", "複製外參\n穿牆套管");
            SplitButton splitButtonCopyWallCast = panel5.AddItem(copyWallCastButtonData) as SplitButton;
            PushButton buttonWallCastCopy = splitButtonCopyWallCast.AddPushButton(btnDataCopyWallCast);
            ContextualHelp wallCastCopyHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-c856f7e6a49c4bdba16651c764c25af5?pvs=4");
            buttonWallCastCopy.SetContextualHelp(wallCastCopyHelp);
            splitButtonCopyWallCast.SetContextualHelp(wallCastCopyHelp);
            PushButton buttonWallCastCoPart = splitButtonCopyWallCast.AddPushButton(btnDataCopyWallCastPart);
            ContextualHelp wallCastCoPartHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-03ec3ad1c3314ff98e91436d971a113a?pvs=4");
            buttonWallCastCoPart.SetContextualHelp(wallCastCoPartHelp);
            //PushButton buttonWallCastCopy = panel5.AddItem(btnDataCopyWallCast) as PushButton;
            checkitem.Add(buttonWallCastCopy);
            only2Ditem.Add(buttonWallCastCoPart);

            //穿牆套管編號(編號&重編)
            SplitButtonData setWallNumButtonData = new SplitButtonData("WallCastSetNumButton", "穿牆套管編號");
            SplitButton splitButtonWallNum = panel5.AddItem(setNumButtonData) as SplitButton;
            PushButton buttonWallCastNum = splitButtonWallNum.AddPushButton(btnDataWallCastNum);
            ContextualHelp wallCastNumHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-6315ccaa132543bbb0b9a2e40784712f?pvs=4");
            buttonWallCastNum.SetContextualHelp(wallCastNumHelp);
            splitButtonWallNum.SetContextualHelp(wallCastNumHelp);
            PushButton buttonWallCasReNum = splitButtonWallNum.AddPushButton(btnDataWallCastReNum);
            ContextualHelp wallCastReNumHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-249c616b7ad34e73bcdec7a3c2f3987c?pvs=4");
            buttonWallCasReNum.SetContextualHelp(wallCastReNumHelp);
            checkitem.Add(buttonWallCastNum);
            checkitem.Add(buttonWallCasReNum);
            #endregion

            #region[Panel_Slab] - 穿版開口
            System.Drawing.Image image_SlabUpdate = Properties.Resources.點選放置圓形版開口_更新32_svg;
            ImageSource imgSrcSlabUpdate = GetImageSource(image_SlabUpdate);
            System.Drawing.Image image_SlabUpdatePart = Properties.Resources.點選放置圓形版開口_局部更新32_svg;
            ImageSource imgSrcSlabUpdatePart = GetImageSource(image_SlabUpdatePart);
            System.Drawing.Image image_SlabCast = Properties.Resources.點選放置圓形版開口32_svg;
            ImageSource imgSrcSlabCast = GetImageSource(image_SlabCast);
            System.Drawing.Image image_RectSlabCast = Properties.Resources.點選放置方形版開口32_svg;
            ImageSource imgSrcRectSlabCast = GetImageSource(image_RectSlabCast);
            System.Drawing.Image image_MultiSlabCast = Properties.Resources.點選放置多管版開口32_svg;
            ImageSource imgSrcMultiSlabCast = GetImageSource(image_MultiSlabCast);

            PushButtonData btnDataSlabCastUpdate = new PushButtonData(
               "MyButton_SlabCastUpdate",
                "更新\n穿版資訊",
                 ApiDllPath_WallCast,
                 "CEC_WallCast.SlabCastUpdate"
                );
            {
                btnDataSlabCastUpdate.ToolTip = "一鍵更新穿版開口資訊";
                btnDataSlabCastUpdate.LongDescription = $"一鍵更新穿版開口資訊({assemblyInfo})";
                btnDataSlabCastUpdate.LargeImage = imgSrcSlabUpdate;
            }

            PushButtonData btnDataSlabUpdatePart = new PushButtonData(
               "MyButton_SlabUpdatePart",
               "局部更新\n穿版資訊",
               ApiDllPath_WallCast,
               "CEC_WallCast.SlabCastUpdatePart"
                );
            {
                btnDataSlabUpdatePart.ToolTip = "依照目前視圖範圍，局部更新穿牆開口資訊";
                btnDataSlabUpdatePart.LongDescription = $"依照目前視圖範圍，局部更新穿版開口資訊({assemblyInfo})";
                btnDataSlabUpdatePart.LargeImage = imgSrcSlabUpdatePart;
            }

            PushButtonData btnDataSlabCast = new PushButtonData(
                "MyButton_SlabCast",
                "穿版套管",
                 ApiDllPath_WallCast,
                 "CEC_WallCast.CreateSlabCast"
                );
            {
                btnDataSlabCast.ToolTip = "點選管與外參牆生成穿版套管";
                btnDataSlabCast.LongDescription = $"先點選需要創建的管段，再點選其穿過的外參牆，生成穿牆套管({assemblyInfo})";
                btnDataSlabCast.LargeImage = imgSrcSlabCast;
            }

            PushButtonData btnDataRectSlab = new PushButtonData(
                "MyButton_SlabRect",
                "方形穿版開口",
                ApiDllPath_WallCast,
                "CEC_WallCast.CreateRectSlabCast"
                );
            {
                btnDataRectSlab.ToolTip = "點選外參管與版生成穿牆方開口";
                btnDataRectSlab.LongDescription = $"先點選需要創建的外參管段，再點選其穿過的版，生成版開口({assemblyInfo})";
                btnDataRectSlab.LargeImage = imgSrcRectSlabCast;
            }

            PushButtonData btnDataMultiRectSlab = new PushButtonData(
                "MyButton_MultiSlabCast",
                "多管版開口",
                ApiDllPath_WallCast,
                "CEC_WallCast.MultiSlabRectCast"
                );
            {
                btnDataMultiRectSlab.ToolTip = "點選外參牆與多支管生成穿版方開口";
                btnDataMultiRectSlab.LongDescription = $"先點選需要創建的管段(複數)，再點選其穿過的外參版，生成穿版方開口({assemblyInfo})";
                btnDataMultiRectSlab.LargeImage = imgSrcMultiSlabCast;
            }

            //更新穿版套管
            SplitButtonData updateSlabButtonData = new SplitButtonData("UpdateSlabCast", "更新\n穿版資訊");
            SplitButton splitButtonSlabUpdate = panelSlab.AddItem(updateSlabButtonData) as SplitButton;
            PushButton buttonSlabCastUpdate = splitButtonSlabUpdate.AddPushButton(btnDataSlabCastUpdate);
            ContextualHelp slabCastUpdateHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-7bb0c4f60fd245a4976df2c4a1e5c71e?pvs=4");
            splitButtonSlabUpdate.SetContextualHelp(slabCastUpdateHelp);
            buttonSlabCastUpdate.SetContextualHelp(slabCastUpdateHelp);
            PushButton buttonSlabCastUpdatePart = splitButtonSlabUpdate.AddPushButton(btnDataSlabUpdatePart);
            ContextualHelp slabUpdatePartHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-edcc90f4da414b3ba3e833bc458507a2?pvs=4");
            buttonSlabCastUpdatePart.SetContextualHelp(slabUpdatePartHelp);
            checkitem.Add(buttonSlabCastUpdate);
            only2Ditem.Add(buttonSlabCastUpdatePart);

            //圓型穿版開口
            PushButton slabCastButton = panelSlab.AddItem(btnDataSlabCast) as PushButton;
            ContextualHelp slabCastHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-ed438e9388934a1db411e482038ce068?pvs=4");
            slabCastButton.SetContextualHelp(slabCastHelp);
            checkitem.Add(slabCastButton);

            //方形穿版開口
            SplitButtonData rectSlabSplitButton = new SplitButtonData("RectSlabCast", "方形\n穿版開口");
            SplitButton splitButtonRectSlabCast = panelSlab.AddItem(rectSlabSplitButton) as SplitButton;
            PushButton buttonRectSlabCast = splitButtonRectSlabCast.AddPushButton(btnDataRectSlab);
            ContextualHelp rectSlabHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-45681d8851a445f09a1a5464ecea15b1?pvs=4");
            splitButtonRectSlabCast.SetContextualHelp(rectSlabHelp);
            buttonRectSlabCast.SetContextualHelp(rectSlabHelp);
            PushButton buttonMultiRectSlab = splitButtonRectSlabCast.AddPushButton(btnDataMultiRectSlab);
            ContextualHelp multiSlabHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-9ca80aa1166e4dfeb0b9fbb68db3cfb9?pvs=4");
            buttonMultiRectSlab.SetContextualHelp(multiSlabHelp);
            checkitem.Add(buttonRectSlabCast);
            checkitem.Add(buttonMultiRectSlab);
            #endregion

            #region [Panel_SlabCSD] - 穿版開口CSD&SEM
            System.Drawing.Image image_SlabCopy = Properties.Resources.點選放置圓形版開口_複製32_svg;
            ImageSource imgSrcSlabCopy = GetImageSource(image_SlabCopy);
            System.Drawing.Image image_SlabCopyPart = Properties.Resources.點選放置圓形版開口_依樓層複製32_svg;
            ImageSource imgSrcSlabCopyPart = GetImageSource(image_SlabCopyPart);

            PushButtonData btnDataSlabCopy = new PushButtonData(
            "MyButton_SlabCopy",
            "複製外參\n穿版開口",
            ApiDllPath_WallCast,
            "CEC_WallCast.CopyAllSlabCast"
            );
            {
                btnDataSlabCopy.ToolTip = "複製所有連結模型中的版開口";
                btnDataSlabCopy.LongDescription = $"複製所有連結模型中的版開口，以供SEM開口編號用({assemblyInfo})";
                btnDataSlabCopy.LargeImage = imgSrcSlabCopy;
            }

            PushButtonData btnDataSlabCopyPart = new PushButtonData(
            "MyButton_SlabCopyPart",
            "依樓層複製\n穿版開口",
            ApiDllPath_WallCast,
            "CEC_WallCast.CopyPartSlabCast"
            );
            {
                btnDataSlabCopyPart.ToolTip = "依視圖參考樓層複製連結模型中的版開口";
                btnDataSlabCopyPart.LongDescription = $"依視圖參考樓層複製連結模型中的版開口，以供SEM開口編號用({assemblyInfo})";
                btnDataSlabCopyPart.LargeImage = imgSrcSlabCopyPart;
            }

            //複製穿版開口
            SplitButtonData copySlabButtonData = new SplitButtonData("copySlabCast", "複製外參\n穿版開口");
            SplitButton splitButtonCopySlabCast = panelSlabCSD.AddItem(copySlabButtonData) as SplitButton;
            PushButton buttonCopySlabCast = splitButtonCopySlabCast.AddPushButton(btnDataSlabCopy);
            ContextualHelp copySlabHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-a265fa144de34d2fbab57b59f8cde593?pvs=4");
            splitButtonCopySlabCast.SetContextualHelp(copySlabHelp);
            buttonCopySlabCast.SetContextualHelp(copySlabHelp);
            PushButton buttonCopySlabPart = splitButtonCopySlabCast.AddPushButton(btnDataSlabCopyPart);
            ContextualHelp copyPartSlabHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-1a8ca874a9b9497c869687a2fca58f90?pvs=4");
            buttonCopySlabPart.SetContextualHelp(copyPartSlabHelp);
            checkitem.Add(buttonCopySlabCast);
            only2Ditem.Add(buttonCopySlabPart);

            #endregion

            #region[Panel6] - 管線預組
            System.Drawing.Image image_CreateISO = Properties.Resources.預組ICON_產生ISO圖_svg_32;
            ImageSource imgSrcCreateISO = GetImageSource(image_CreateISO);
            System.Drawing.Image image_CleanUpNumber = Properties.Resources.預組ICON_清除編號_svg_32;
            ImageSource imgSrcCleanUpNumber = GetImageSource(image_CleanUpNumber);
            System.Drawing.Image image_CleanTags = Properties.Resources.預組ICON_清除標籤_svg_32;
            ImageSource imgSrcCleanTags = GetImageSource(image_CleanTags);
            System.Drawing.Image image_Renumber = Properties.Resources.預組ICON_重新編號_svg_32;
            ImageSource imgSrcReNumber = GetImageSource(image_Renumber);

            PushButtonData btnDataCreateISO = new PushButtonData(
            "MyButton_CreateFabISO",
            "創建\n 預組ISO",
            ApiDllPath_PreFab,
            "CEC_PreFabric.CreateISO"
             );
            {
                btnDataCreateISO.ToolTip = "框選區域產生預組ISO圖";
                btnDataCreateISO.LongDescription = $"框選希望產生預組ISO與BOM表的區域，生成ISO圖與BOM表({assemblyInfo})";
                btnDataCreateISO.LargeImage = imgSrcCreateISO;
            }

            PushButtonData btnDataCleanUpNumber = new PushButtonData(
            "MyButton_ClenUpFabNum",
            "清除編號",
            ApiDllPath_PreFab,
            "CEC_PreFabric.CleanUpNumbers"
            );
            {
                btnDataCleanUpNumber.ToolTip = "清除ISO圖中的管段編號";
                btnDataCleanUpNumber.LongDescription = $"請切換當前視圖至欲修改編號的預組視圖，一鍵清除編號({assemblyInfo})";
                btnDataCleanUpNumber.LargeImage = imgSrcCleanUpNumber;
            }

            PushButtonData btnDataCleanTags = new PushButtonData(
             "MyButton_DeleteAllTags",
             "清除標籤",
            ApiDllPath_PreFab,
             "CEC_PreFabric.DeleteAllTags"
             );
            {
                btnDataCleanTags.ToolTip = "清除ISO圖中的編號標籤";
                btnDataCleanTags.LongDescription = $"請切換當前視圖至欲清除標籤的預組視圖，一鍵清除標籤({assemblyInfo})";
                btnDataCleanTags.LargeImage = imgSrcCleanTags;
            }

            PushButtonData btnDataReNumber = new PushButtonData(
            "MyButton_ISOReNumber",
            "重新編號",
            ApiDllPath_PreFab,
            "CEC_PreFabric.ReNumber"
            );
            {
                btnDataReNumber.ToolTip = "針對ISO圖中的管段進行重新編號";
                btnDataReNumber.LongDescription = $"請切換當前視圖至欲重新編號的預組視圖，批次重新編號({assemblyInfo})";
                btnDataReNumber.LargeImage = imgSrcReNumber;
            }
            PushButton buttonCreateISO = panel6.AddItem(btnDataCreateISO) as PushButton;
            ContextualHelp createISOHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-ISO-b8a7796dac714f478b252a23a3760cf7?pvs=4");
            buttonCreateISO.SetContextualHelp(createISOHelp);
            PushButton buttonCleanUpNumber = panel6.AddItem(btnDataCleanUpNumber) as PushButton;
            ContextualHelp cleanUpNumberHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-bd6a07d7511643e190054e6cd2dfd264?pvs=4");
            buttonCleanUpNumber.SetContextualHelp(cleanUpNumberHelp);
            PushButton buttonCleanTags = panel6.AddItem(btnDataCleanTags) as PushButton;
            ContextualHelp cleanTagsHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-8a00be789e0f4b9fa4217f5fb465eaf4?pvs=4");
            buttonCleanTags.SetContextualHelp(cleanTagsHelp);
            PushButton buttonReNumber = panel6.AddItem(btnDataReNumber) as PushButton;
            ContextualHelp reNumberHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-6f0d7fdffbd34a4b9f7f40be37c79a09?pvs=4");
            buttonReNumber.SetContextualHelp(reNumberHelp);
            checkitem.Add(buttonCreateISO);
            only3Ditem.Add(buttonCleanUpNumber);
            only3Ditem.Add(buttonCleanTags);
            only3Ditem.Add(buttonReNumber);
            #endregion

            #region[Panel7] - 圖塊轉換
            //圖塊轉換
            System.Drawing.Image image_BlkTrans = Properties.Resources.圖塊轉換icon_32pix;
            ImageSource imgSrcBlkTrans = GetImageSource(image_BlkTrans);
            PushButtonData btnDataBlkTrans = new PushButtonData(
            "MyButton_CADBlockTrans",
            "圖塊\n批次轉換",
            ApiDllPath_BlockTrans,
            "CEC_CADBlockTrans.Command"//按鈕的全名-->要依照需要參照的command打入
            );
            {
                btnDataBlkTrans.ToolTip = "CAD圖塊批次轉換";
                btnDataBlkTrans.LongDescription = "CAD圖塊批次轉換";
                btnDataBlkTrans.LargeImage = imgSrcBlkTrans;
            };
            PushButton buttonBlkTrans = panel7.AddItem(btnDataBlkTrans) as PushButton;
            ContextualHelp blkTransHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-7627cf11c83a4e18ad9d8767469c8002?pvs=4");
            buttonBlkTrans.SetContextualHelp(blkTransHelp);
            only2Ditem.Add(buttonBlkTrans);
            #endregion

            #region [Panel8] - 水平管標籤
            System.Drawing.Image image_horizontalTagSet = Properties.Resources._1__管排標籤設定96;
            ImageSource imgSrchHoriTagSet = GetImageSource(image_horizontalTagSet);
            PushButtonData btnHoriTagSet = new PushButtonData(
            "MyButton_HoriTagSet",
            "設定\n水平管標籤",
            ApiDllPath_PipeTags,
            "PipeTagger.Tag_setting"//按鈕的全名-->要依照需要參照的command打入
            );
            {
                btnHoriTagSet.ToolTip = "設定水平管標籤";
                btnHoriTagSet.LongDescription = "設定連續水平管標籤";
                btnHoriTagSet.LargeImage = imgSrchHoriTagSet;
            };

            System.Drawing.Image image_horizontalTagPlace = Properties.Resources._2__放置管排管段標籤96;
            ImageSource imgSrcHoriTagPlace = GetImageSource(image_horizontalTagPlace);
            PushButtonData btnHoriPlace = new PushButtonData(
            "MyButton_HoriTagPlace",
            "放置\n水平管標籤",
            ApiDllPath_PipeTags,
            "PipeTagger.Place_tag"//按鈕的全名-->要依照需要參照的command打入
            );
            {
                btnHoriPlace.ToolTip = "放置水平管標籤";
                btnHoriPlace.LongDescription = "放置連續水平管標籤";
                btnHoriPlace.LargeImage = imgSrcHoriTagPlace;
            };

            PushButton buttonHTagSet = panel8.AddItem(btnHoriTagSet) as PushButton;
            ContextualHelp HTagSet = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-b7df19780c60459cb6e889679cc70ce4?pvs=4");
            buttonHTagSet.SetContextualHelp(HTagSet);
            PushButton buttonHTagPlace = panel8.AddItem(btnHoriPlace) as PushButton;
            ContextualHelp HTagPlace = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-45d15d9ccbbd4a33a522389ac1ad80a1?pvs=4");
            buttonHTagPlace.SetContextualHelp(HTagPlace);
            only2Ditem.Add(buttonHTagSet);
            only2Ditem.Add(buttonHTagPlace);
            #endregion

            #region [Panel8] - 數量計算
            System.Drawing.Image image_zoningEquipCount = Properties.Resources.分區數量計算_ai_x32;
            ImageSource imgSrcEquipCount = GetImageSource(image_zoningEquipCount);
            PushButtonData btnEquipCount = new PushButtonData(
                "MyButton_EquipCount",
                "分區\n數量計算",
                ApiDllPath_EquipsCount,
                "CEC_Count.EquipCount"//按鈕的全名-->要依照需要參照的command打入
                );
            {
                btnEquipCount.ToolTip = "依據分區進行數量計算";
                btnEquipCount.LongDescription = "依據外參連結中的量體進行分區數量計算";
                btnEquipCount.LargeImage = imgSrcEquipCount;
            };
            PushButton buttonEquipCount = panel9.AddItem(btnEquipCount) as PushButton;
            ContextualHelp equipCountHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-4dd6f8ac01124ffb94cc41890d347c85?pvs=4");
            buttonEquipCount.SetContextualHelp(equipCountHelp);
            checkitem.Add(buttonEquipCount);
            #endregion

            #region[Panel Regis] - 土木機電BIM-註冊資訊
            System.Drawing.Image image_Regis = Properties.Resources.Image20220111111226;
            ImageSource imgRegis = GetImageSource(image_Regis);
            PushButtonData btnRegis = new PushButtonData("MyButton_Regist", "註冊資訊", Assembly.GetExecutingAssembly().Location, "CEC_License.Command");
            {
                btnRegis.ToolTip = "點選註冊進行CEC MEP API授權認證";
                btnRegis.LongDescription = $"點選註冊進行CEC MEP API授權認證({versionNumber})";
                btnRegis.LargeImage = imgRegis;
            }
            PushButton pushButtonRegistration = panelRegis.AddItem(btnRegis) as PushButton;
            ContextualHelp registrationHelp = new ContextualHelp(ContextualHelpType.Url, "https://acute-soarer-f08.notion.site/RevitAPI-37ace8b819824de0b0b433633e5d7d64?pvs=4");
            pushButtonRegistration.SetContextualHelp(registrationHelp);
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

            //只能在3D執行
            foreach (RibbonItem item in only3Ditem)
            {
                rItem = item;
                if (doc.ActiveView.ViewType == ViewType.ThreeD && rItem != null && LicenseSetting.Default.CheckLicense)
                    rItem.Enabled = true;

                else
                    rItem.Enabled = false;
            }
            //只能在2D執行
            foreach (RibbonItem item in only2Ditem)
            {
                rItem = item;

                // if (doc.ActiveView.ViewType != ViewType.ThreeD && rItem != null)//原本寫法是只要不是3D視圖即可
                if (doc.ActiveView.ViewType == ViewType.FloorPlan && rItem != null && LicenseSetting.Default.CheckLicense) //改成只有樓板平面圖可以使用
                    rItem.Enabled = true;

                else
                    rItem.Enabled = false;
            }

            //確認item
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
            //製作一個function專門來處理圖片
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
