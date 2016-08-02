using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using SharpPcap;
using SharpPcap.LibPcap;
using Utility.存储;
using UI.IBLL;
using Utility.扩展;
using 项目解码接口;

namespace UI
{
    public partial class F配置 : DevExpress.XtraEditors.XtraForm
    {
        private readonly B项目 _B项目 = new B项目();

        private M项目 _当前项目;

        private BindingSource _数据源;

        private readonly List<string> _设备类型列表 = new List<string>();


        public F配置()
        {
            SplashScreenManager.ShowForm(this.FindForm(), typeof(DevExpress.XtraWaitForm.DemoWaitForm), false, true, false);
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Text += "  -  " + Assembly.GetExecutingAssembly().GetName().Version.ToString();

            this.out来源_文件.Enabled = false;

            var __网卡列表 = CaptureDeviceList.Instance.ToList();
            this.in网卡.Properties.DisplayFormat.Format = new B显示声卡();
            this.in网卡.Properties.Items.AddRange(__网卡列表);
            var __当前网卡索引 = H程序配置.获取Int32值("当前网卡索引");
            if (__网卡列表.Count > __当前网卡索引)
            {
                this.in网卡.SelectedIndex = __当前网卡索引;
            }
            var __项目列表 = _B项目.获取所有();
            this.in项目.Properties.Items.AddRange(__项目列表);
            var __当前项目索引 = H程序配置.获取Int32值("当前项目索引");
            if (__项目列表.Count > __当前项目索引)
            {
                this.in项目.SelectedIndex = __当前项目索引;
            }

            _当前项目 = 浅复制(__项目列表[__当前项目索引]);
            _设备类型列表.AddRange(_当前项目.设备类型列表);
            //this.in设备类型1.DataSource = _设备类型列表;
            //this.in设备类型2.DataSource = _设备类型列表;
            this.in设备类型11.Items.AddRange(_设备类型列表);
            this.in设备类型22.Items.AddRange(_设备类型列表);

            this._数据源 = new BindingSource { DataSource = _当前项目.当前通信设备, AllowNew = true };
            //this.out设备.DataSource = _数据源;
            this.out表格控件.DataSource = _数据源;

            this.do分析.Click += do分析_Click;
            this.do仅录像.Click += do仅录像_Click;
            this.do加载模板.Click += do加载模板_Click;
            this.do网卡_详细信息.Click += do网卡_详细信息_Click;
            this.do文件_导入文件.Click += do文件_导入文件_Click;
            this.in项目.SelectedIndexChanged += in项目_SelectedIndexChanged;
            this.in来源_文件.CheckedChanged += in来源_CheckedChanged;
            //this.in来源_网卡.CheckedChanged += in来源_CheckedChanged;

            this.out列表.ValidatingEditor += out列表_ValidatingEditor;
            SplashScreenManager.CloseForm(false);
        }

        void out列表_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            var __列 = this.out列表.FocusedColumn.FieldName;
            if (__列 == "IP1" || __列 == "IP2")
            {
                IPAddress __ip;
                if (IPAddress.TryParse(e.Value.ToString(), out __ip))
                {
                    e.Valid = true;
                    e.Value = __ip;
                }
                else
                {
                    e.Valid = false;
                    e.ErrorText = "IP格式不正确";
                }
            }
        }

        void in来源_CheckedChanged(object sender, EventArgs e)
        {
            this.out来源_网卡.Enabled = this.in来源_网卡.Checked;
            this.out来源_文件.Enabled = this.in来源_文件.Checked;
            this.do仅录像.Enabled = !this.in来源_文件.Checked;
            if (this.in来源_文件.Checked)
            {
                this.do文件_导入文件.PerformClick();
            }
        }

        void in项目_SelectedIndexChanged(object sender, EventArgs e)
        {
            this._数据源.Clear();
            var __项目 = (M项目)this.in项目.SelectedItem;
            _当前项目 = 浅复制(__项目);
            _设备类型列表.Clear();
            _设备类型列表.AddRange(_当前项目.设备类型列表);
            //this.in设备类型1.DataSource = _设备类型列表;
            //this.in设备类型2.DataSource = _设备类型列表;
            this.in设备类型11.Items.AddRange(_设备类型列表);
            this.in设备类型22.Items.AddRange(_设备类型列表);
            this._数据源.DataSource = _当前项目.当前通信设备;
            this._数据源.ResetBindings(false);
        }

        void do文件_导入文件_Click(object sender, EventArgs e)
        {
            var __对话框 = new OpenFileDialog() { InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "录像") };
            if (__对话框.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            this.in文件.Text = __对话框.FileName;
        }

        void do网卡_详细信息_Click(object sender, EventArgs e)
        {
            var __网卡 = (ICaptureDevice)this.in网卡.SelectedItem;
            XtraMessageBox.Show(__网卡.ToString(), "详细信息");
        }

        void do加载模板_Click(object sender, EventArgs e)
        {
            _当前项目.当前通信设备.Clear();
            _当前项目.当前通信设备.AddRange(H复制对象.深复制(_当前项目.设备通信模板));
            this._数据源.ResetBindings(false);
        }

        void do分析_Click(object sender, EventArgs e)
        {
            var __配置 = new M抓包配置
            {
                项目 = _当前项目,
                录像 = this.in来源_文件.Checked
            };
            _B项目.保存项目映射(_当前项目.名称, _当前项目.当前通信设备);
            H程序配置.设置("当前项目索引", this.in项目.SelectedIndex.ToString());
            if (__配置.录像)
            {
                var __录像名 = this.in文件.Text.Trim();
                if (!File.Exists(__录像名))
                {
                    XtraMessageBox.Show("请选择文件!");
                    return;
                }
                var __放映机 = new CaptureFileReaderDevice(__录像名);
                __配置.网卡 = __放映机;
                显示抓包列表窗口(__配置);
                __放映机.Close();
            }
            else
            {
                __配置.网卡 = (ICaptureDevice)this.in网卡.SelectedItem;
                if (__配置.网卡 == null)
                {
                    XtraMessageBox.Show("请选择网卡!");
                    return;
                }
                H程序配置.设置("当前网卡索引", this.in网卡.SelectedIndex.ToString());
                var __录像目录 = H路径.获取绝对路径("录像\\");
                if (!Directory.Exists(__录像目录))
                {
                    Directory.CreateDirectory(__录像目录);
                }
                var __录像机 = new CaptureFileWriterDevice(Path.Combine(__录像目录, _当前项目.名称 + " " + DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss")));
                PacketArrivalEventHandler __处理抓包 = (object sender1, CaptureEventArgs e1) => __录像机.Write(e1.Packet);
                __配置.网卡.OnPacketArrival += __处理抓包;
                显示抓包列表窗口(__配置);
                __配置.网卡.OnPacketArrival -= __处理抓包;
                __录像机.Close();
            }
        }

        private void do仅录像_Click(object sender, EventArgs e)
        {
            var _网卡 = (ICaptureDevice)this.in网卡.SelectedItem;
            var __录像目录 = H路径.获取绝对路径("录像\\");
            if (!Directory.Exists(__录像目录))
            {
                Directory.CreateDirectory(__录像目录);
            }
            var __录像机 = new CaptureFileWriterDevice(Path.Combine(__录像目录, _当前项目.名称 + " " + DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss")));
            PacketArrivalEventHandler __处理抓包 = (object sender1, CaptureEventArgs e1) => __录像机.Write(e1.Packet);
            _网卡.OnPacketArrival += __处理抓包;
            _网卡.Open();
            _网卡.Filter = H公共.获取过滤表达式(_当前项目.当前通信设备);
            _网卡.StartCapture();
            XtraMessageBox.Show(string.Format("开始时间: {0}, 按OK键终止录像!", DateTime.Now), "录像中", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _网卡.OnPacketArrival -= __处理抓包;
            _网卡.Close();
            __录像机.Close();
        }

        private void 显示抓包列表窗口(M抓包配置 __配置)
        {
            this.Hide();
            new F报文列表(__配置).ShowDialog();
            this.Show();
        }

        private M项目 浅复制(M项目 __源)
        {
            var __目的 = new M项目
            {
                当前通信设备 = new List<M设备映射>(__源.当前通信设备),
                功能码查询 = __源.功能码查询,
                解码器 = __源.解码器,
                名称 = __源.名称,
                设备类型列表 = new List<string>(__源.设备类型列表),
                设备通信模板 = new List<M设备映射>(__源.设备通信模板),
                业务类型列表 = new List<string>(__源.业务类型列表),
                默认内容过滤 = new List<string>(__源.默认内容过滤),
                默认业务过滤 = new List<string>(__源.默认业务过滤)
            };
            return __目的;
        }

        class B显示声卡 : IFormatProvider, ICustomFormatter
        {
            public object GetFormat(Type formatType)
            {
                if (formatType == typeof (ICustomFormatter))
                {
                    return this;
                }
                else
                {
                    return null;
                }
            }

            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                var __ICaptureDevice = arg as ICaptureDevice;
                if (__ICaptureDevice != null)
                {
                    return __ICaptureDevice.Description;
                }
                return null;
            }
        }
    }
}
