using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.XtraEditors;
using Utility.存储;
using Utility.通用;

namespace UI
{
    static class Program
    {
        private static readonly string _类型名 = typeof(Program).Name;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Thread.CurrentThread.Name = "UI";
            H调试.初始化();
            H调试.清除过期调试文件("录像");
            H异常.提示不可恢复异常 = 显示不可恢复异常;
            H异常.提示可恢复异常 = 显示可恢复异常;
            H异常.自定义处理 = q => {
                                 if (q is InvalidOperationException || q is NullReferenceException || q is ArgumentNullException)
                                 {
                                     H调试.记录异常(q);
                                     return true;
                                 }
                                 return false;
            };
            SkinManager.EnableFormSkins();
            //DevExpress.UserSkins.BonusSkins.Register();
            DevExpress.Utils.AppearanceObject.DefaultFont = new System.Drawing.Font("微软雅黑", 9F);
            UserLookAndFeel.Default.SetSkinStyle("Office 2013");
            //UserLookAndFeel.Default.SetSkinStyle("Metropolis");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new F配置());
        }

        static void 显示可恢复异常(string 基本信息, string 详细信息)
        {
            XtraMessageBox.Show("提示", 基本信息 + Environment.NewLine + 详细信息, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        static void 显示不可恢复异常(string 基本信息, string 详细信息)
        {
            XtraMessageBox.Show(基本信息 + Environment.NewLine + 详细信息, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

    }
}
