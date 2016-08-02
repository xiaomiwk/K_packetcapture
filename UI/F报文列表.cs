using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using Utility.通用;
using 项目解码接口;
using PacketDotNet;
using SharpPcap;

namespace UI
{
    public partial class F报文列表 : XtraForm
    {
        private readonly M抓包配置 _配置;

        private readonly ICaptureDevice _网卡;

        private readonly object _报文处理同步对象 = new object();

        private List<RawCapture> _待处理抓包 = new List<RawCapture>();

        private readonly DataTable _数据表缓存 = new DataTable();

        private Thread _后台线程;

        private bool _停止后台处理;

        private bool _锁定最后一行 = false;

        private string _业务类型缓存;

        private int _显示上限 = 1000000;

        private object _数据锁 = new object();

        public F报文列表(M抓包配置 __配置)
        {
            SplashScreenManager.ShowForm(this.FindForm(), typeof(DevExpress.XtraWaitForm.DemoWaitForm), false, true, false);
            InitializeComponent();
            _网卡 = __配置.网卡;
            _配置 = __配置;

            _数据表缓存.Columns.Add("时间", typeof(DateTime));
            _数据表缓存.Columns.Add("业务", typeof(string));
            _数据表缓存.Columns.Add("发送方", typeof(string));
            _数据表缓存.Columns.Add("接收方", typeof(string));
            _数据表缓存.Columns.Add("概述", typeof(string));
            _数据表缓存.Columns.Add("报文", typeof(M解码后报文));

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Text += "  -  " + Assembly.GetExecutingAssembly().GetName().Version.ToString();

            this.out表格控件.DataSource = _数据表缓存;
            

            var __业务列表 = new List<string>(_配置.项目.业务类型列表);
            __业务列表.Insert(0, "所有");
            this.in业务.Properties.Items.AddRange(__业务列表);
            this.in业务.SelectedIndex = 0;

            this.do测试用例.Click += do测试用例_Click;
            this.do清空.Click += do清空_Click;
            this.do停止.Click += do停止_Click;
            this.out解码.MouseDoubleClick += out解码_MouseDoubleClick;
            this.in业务.SelectedIndexChanged += (sender1, e1) =>
            {
                if ((string)this.in业务.SelectedItem != _业务类型缓存)
                {
                    var __新业务类型 = (string)this.in业务.SelectedItem;
                    if (!string.IsNullOrEmpty(_业务类型缓存))
                    {
                        if (__新业务类型 != "所有")
                        {
                            this.in关键字.Text = this.in关键字.Text.Replace(string.Format("业务 = '{0}'", _业务类型缓存), string.Format("业务 = '{0}'", __新业务类型));
                        }
                        else
                        {
                            this.in关键字.Text = this.in关键字.Text.Replace(string.Format(" AND 业务 = '{0}'", _业务类型缓存), "").Replace(string.Format("业务 = '{0}'", _业务类型缓存), "");
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(this.in关键字.Text.Trim()))
                        {
                            this.in关键字.Text = string.Format("业务 = '{0}'", __新业务类型);
                        }
                        else
                        {
                            this.in关键字.Text += string.Format(" AND 业务 = '{0}'", __新业务类型);
                        }
                    }
                    _业务类型缓存 = __新业务类型;
                }
                设置过滤();
            };
            this.do菜单_锁定.Click += (sender1, e1) =>
            {
                _锁定最后一行 = true; 
                this.out列表.MoveLast();        
            };
            this.do菜单_解除锁定.Click += (sender1, e1) => { _锁定最后一行 = false; };

            this.out列表.FocusedRowChanged += out列表_FocusedRowChanged;
            this.in关键字.Properties.ButtonClick += Properties_ButtonClick;
            this.in关键字.Properties.KeyDown += Properties_KeyDown;
            //this.in关键字.Properties.Items.AddRange(new object[]
            //{
            //    "时间:\"??:??\"", 
            //    "发送方:\"???\" +接收方:\"???\"", 
            //    "概述:\"???\"", 
            //    "概述:\"-心跳 -传输确认 -操作成功\"",
            //});
            this.in关键字.Properties.Items.AddRange(new object[]
            {
                "时间 > '??:??'", 
                "发送方 = '??' AND 接收方 = '??'", 
                "业务 = '??'", 
                "概述 IN ('??', '??')",
                "业务 = '??' AND 概述 IN ('??', '??')", 
            });
            this.do查询示例.MouseHover += (sender1, e1) => this.out查询提示内容.ShowBeakForm(MousePosition);
            this.out表格控件.ContextMenuStrip = this.out菜单;
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
            DevExpress.Data.CurrencyDataController.CatchRowUpdatedExceptions = false;

            var __过滤 = new StringBuilder();
            //if (_配置.项目.默认业务过滤 != null && _配置.项目.默认业务过滤.Count > 0)
            //{
            //    _配置.项目.默认业务过滤.ForEach(q => __过滤.AppendFormat(" -业务:\"{0}\"", q));
                
            //}
            //if (_配置.项目.默认内容过滤 != null && _配置.项目.默认内容过滤.Count > 0)
            //{
            //    _配置.项目.默认内容过滤.ForEach(q => __过滤.AppendFormat(" -概述:\"{0}\"", q));
            //}

            if (_配置.项目.默认业务过滤 != null && _配置.项目.默认业务过滤.Count > 0)
            {
                __过滤.Append("业务 NOT IN (");
                _配置.项目.默认业务过滤.ForEach(q => __过滤.AppendFormat("'{0}',", q));
                __过滤.Append(")");
            }
            if (_配置.项目.默认内容过滤 != null && _配置.项目.默认内容过滤.Count > 0)
            {
                _配置.项目.默认内容过滤.ForEach(q =>
                {
                    if (__过滤.Length > 0)
                    {
                        __过滤.AppendFormat(" AND 概述 NOT LIKE '*{0}*'", q);
                    }
                    else
                    {
                        __过滤.AppendFormat("概述 NOT LIKE '*{0}*'", q);
                    }
                });
            }

            this.in关键字.Text = __过滤.ToString().Trim();

            设置过滤();
            开始();

            SplashScreenManager.CloseForm(false);
        }

        void out解码_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var __内容 = (string)this.out解码.FocusedNode.GetValue(1);
            if (!string.IsNullOrEmpty(__内容) && __内容.Length > 30)
            {
                new F详细信息(__内容){ StartPosition = FormStartPosition.CenterParent }.ShowDialog();
            }
        }

        void Properties_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                设置过滤();
            }
        }

        void Properties_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            switch (e.Button.Index)
            {
                case 0:
                    this.in关键字.ResetText();
                    break;
                case 1:
                    设置过滤();
                    break;
            }
        }

        void out列表_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            var row = this.out列表.GetFocusedRow();
            显示行详细信息((DataRowView)row);
        }

        private void 设置过滤()
        {
            try
            {
                lock (_数据锁)
                {
                    _数据表缓存.DefaultView.RowFilter = this.in关键字.Text;
                }
                this.in关键字.BackColor = Color.White;
            }
            catch (Exception)
            {
                this.in关键字.BackColor = Color.Red;
                //XtraMessageBox.Show(ex.Message, "设置过滤失败");
            }

            //if ((string)this.in业务.SelectedItem != _业务类型缓存)
            //{
            //    if (!string.IsNullOrEmpty(_业务类型缓存))
            //    {
            //        this.in关键字.Text = this.in关键字.Text.Replace(string.Format("业务:\"{0}\" ", _业务类型缓存), "");
            //    }
            //    if (this.in业务.SelectedIndex != 0)
            //    {
            //        this.in关键字.Text = string.Format("业务:\"{0}\" {1}", this.in业务.SelectedItem, this.in关键字.Text);
            //    }
            //    _业务类型缓存 = (string)this.in业务.SelectedItem;
            //}
            //Debug.WriteLine("表格显示过滤条件: " + this.in关键字.Text);
            //while (true)
            //{
            //    try
            //    {
            //        this.out列表.ApplyFindFilter(this.in关键字.Text);
            //        break;
            //    }
            //    catch (InvalidOperationException ex)
            //    {
            //        Thread.Sleep(50);
            //        H调试.记录异常(ex, "线程冲突");
            //    }
            //}
        }

        void do清空_Click(object sender, EventArgs e)
        {
            try
            {
                _数据表缓存.Clear();
            }
            catch (Exception ex)
            {
                H调试.记录异常(ex);
            }
        }

        void do停止_Click(object sender, EventArgs e)
        {
            if (this.do停止.Text == "停止")
            {
                结束();
                if (_配置.录像)
                {
                    this.do停止.Enabled = false;
                    return;
                }
                this.do停止.Text = "开始";
            }
            else
            {
                开始();
                this.do停止.Text = "停止";
            }
        }

        void do测试用例_Click(object sender, EventArgs e)
        {
            XtraMessageBox.Show("该操作暂未实现!");
        }

        private void 显示行详细信息(DataRowView __row)
        {
            this.out结构.ResetText();
            this.out解包.ResetText();
            this.out解码.Nodes.Clear();
            if (__row == null)
            {
                return;
            }
            var __报文 = __row["报文"] as M解码后报文;
            if (__报文 == null)
            {
                return;
            }

            var __功能 = _配置.项目.功能码查询(__报文.发送方设备类型, __报文.接收方设备类型, __报文.功能码);
            if (__功能 != null)
            {
                var __模板内容 = new StringBuilder();
                递归详述(__功能.结构, __模板内容, "");
                this.out结构.Text = __模板内容.ToString();
            }

            this.out报文概述.Text = string.Format("{0}    [业务: {1}]     [功能码: {2}]", __报文.概述, __报文.业务, __报文.功能码);
            if (__报文.详述 != null && __功能 != null)
            {
                this.out解码.Nodes.Clear();
                递归详述(__报文.详述, this.out解码.Nodes, __功能.字典);
                this.out解码.ExpandAll();
            }

            this.out解包.Text += "传输方式: " + (__报文.TCP ? "TCP" : "UDP");
            this.out解包.Text += Environment.NewLine + "总字节数: " + __报文.码流.Length;
            this.out解包.Text += Environment.NewLine + "码流: ";
            this.out解包.Text += Environment.NewLine + BitConverter.ToString(__报文.码流);
        }

        private void 递归详述(List<M信息> 详述, StringBuilder __详述内容, string __缩进)
        {
            详述.ForEach(q =>
            {
                __详述内容.AppendFormat("{0}{1} :  {2}{3}", __缩进, q.键, q.值, Environment.NewLine);
                if (q.子节点 != null && q.子节点.Count > 0)
                {
                    递归详述(q.子节点, __详述内容, __缩进 + "--------");
                }
            });
        }

        private void 递归详述(List<M信息> 详述, TreeListNodes __nodes, Dictionary<string,string> __字典)
        {
            详述.ForEach(q =>
            {
                var __描述 = q.描述;
                if (string.IsNullOrEmpty(__描述) && __字典 != null && __字典.ContainsKey(q.键))
                {
                    __描述 = __字典[q.键];
                }
                var node = __nodes.Add(q.键, q.值, __描述);
                if (q.子节点 != null && q.子节点.Count > 0)
                {
                    递归详述(q.子节点, node.Nodes, __字典);
                }
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            结束();
        }

        private void 开始()
        {
            _停止后台处理 = false;
            _后台线程 = new Thread(处理原始报文);
            _后台线程.Start();

            _网卡.OnPacketArrival += 处理抓到报文;
            _网卡.OnCaptureStopped += 处理停止抓包;

            try
            {
                _网卡.Open();
                _网卡.Filter = H公共.获取过滤表达式(_配置.项目.当前通信设备);
                Debug.WriteLine("过滤条件: " + _网卡.Filter);
                _网卡.StartCapture();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("抓包出现错误, 请关闭后重试: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void 结束()
        {
            if (_网卡 != null)
            {
                _网卡.StopCapture();
                _网卡.Close();
                _网卡.OnPacketArrival -= 处理抓到报文;
                _网卡.OnCaptureStopped -= 处理停止抓包;

                _停止后台处理 = true;
                _后台线程.Join();
            }
        }

        void 处理抓到报文(object sender, CaptureEventArgs e)
        {
            lock (_报文处理同步对象)
            {
                _待处理抓包.Add(e.Packet);
            }
        }

        void 处理停止抓包(object sender, CaptureStoppedEventStatus status)
        {
            if (status != CaptureStoppedEventStatus.CompletedWithoutError)
            {
                XtraMessageBox.Show("抓包出错!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void 处理原始报文()
        {
            while (!_停止后台处理)
            {
                var __暂停 = true;

                lock (_报文处理同步对象)
                {
                    if (_待处理抓包.Count != 0)
                    {
                        __暂停 = false;
                    }
                }

                if (__暂停)
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    List<RawCapture> __待处理原始报文列表;
                    lock (_报文处理同步对象)
                    {
                        __待处理原始报文列表 = _待处理抓包;
                        _待处理抓包 = new List<RawCapture>();
                    }

                    Debug.WriteLineIf(__待处理原始报文列表.Count > 100, string.Format("待处理原始报文数量: {0}", __待处理原始报文列表.Count));
                    if (_配置.录像)
                    {
                        SplashScreenManager.ShowForm(this.FindForm(), typeof(DevExpress.XtraWaitForm.DemoWaitForm), false, true, false);
                    }
                    lock (_数据锁)
                    {
                        _数据表缓存.BeginLoadData();
                        foreach (RawCapture __原始包 in __待处理原始报文列表)
                        {
                            if (_停止后台处理)
                            {
                                break;
                            }
                            var __原始包缓存 = __原始包;
                            var __基础解析 = Packet.ParsePacket(__原始包缓存.LinkLayerType, __原始包缓存.Data);
                            var __IP层报文 = (IpPacket)__基础解析.Extract(typeof(IpPacket));
                            if (__IP层报文 == null)
                            {
                                continue;
                            }
                            var __解码前报文 = new M解码前报文
                            {
                                时间 = __原始包缓存.Timeval.Date.ToLocalTime(),
                                发送方IP = __IP层报文.SourceAddress,
                                接收方IP = __IP层报文.DestinationAddress
                            };
                            var __udp层报文 = (UdpPacket)__基础解析.Extract(typeof(UdpPacket));
                            if (__udp层报文 != null)
                            {
                                __解码前报文.TCP = false;
                                __解码前报文.发送方端口号 = __udp层报文.SourcePort;
                                __解码前报文.接收方端口号 = __udp层报文.DestinationPort;
                                __解码前报文.码流 = __udp层报文.PayloadData;
                            }
                            else
                            {
                                var __tcp层报文 = (TcpPacket)__基础解析.Extract(typeof(TcpPacket));
                                if (__tcp层报文 != null)
                                {
                                    __解码前报文.TCP = true;
                                    __解码前报文.发送方端口号 = __tcp层报文.SourcePort;
                                    __解码前报文.接收方端口号 = __tcp层报文.DestinationPort;
                                    __解码前报文.码流 = __tcp层报文.PayloadData;
                                }
                            }
                            if (__解码前报文.码流.Length == 0)
                            {
                                continue;
                            }
                            var __设备映射 = 获取设备类型(__解码前报文.TCP, new IPEndPoint(__解码前报文.发送方IP, __解码前报文.发送方端口号),
                                new IPEndPoint(__解码前报文.接收方IP, __解码前报文.接收方端口号));
                            if (__设备映射 == null)
                            {
                                continue;
                            }

                            __解码前报文.发送方设备类型 = __设备映射.Item1;
                            __解码前报文.接收方设备类型 = __设备映射.Item2;
                            var __解码后报文 = _配置.项目.解码器(__解码前报文);
                            //验证是否需要过滤
                            增加解码后报文(__解码后报文);
                        }
                        _数据表缓存.EndLoadData();
                        _数据表缓存.AcceptChanges();
                        if (!_配置.录像 && _数据表缓存.Rows.Count > _显示上限)
                        {
                            try
                            {
                                _数据表缓存.Clear();
                            }
                            catch (Exception ex)
                            {
                                H调试.记录异常(ex, "数据超过上限,清空");
                            }
                        }
                        if (_锁定最后一行)
                        {
                            this.BeginInvoke(new Action(() => this.out列表.MoveLast())).AsyncWaitHandle.WaitOne(1000);//滚动条始终定位于最下面
                        }
                    }
                    if (_配置.录像)
                    {
                        SplashScreenManager.CloseForm(false);
                    }
                }
            }
        }

        private Tuple<string, string> 获取设备类型(bool tcp, IPEndPoint __源, IPEndPoint __目的)
        {
            foreach (var __设备映射 in _配置.项目.当前通信设备)
            {
                if (!__设备映射.启用)
                {
                    continue;
                }
                if (tcp != __设备映射.TCP)
                {
                    continue;
                }
                if (验证节点匹配(__源, new IPEndPoint(__设备映射.IP1, __设备映射.端口1)) && 验证节点匹配(__目的, new IPEndPoint(__设备映射.IP2, __设备映射.端口2)))
                {
                    return new Tuple<string, string>(__设备映射.设备类型1, __设备映射.设备类型2);
                }
                if (验证节点匹配(__源, new IPEndPoint(__设备映射.IP2, __设备映射.端口2)) && 验证节点匹配(__目的, new IPEndPoint(__设备映射.IP1, __设备映射.端口1)))
                {
                    return new Tuple<string, string>(__设备映射.设备类型2, __设备映射.设备类型1);
                }
            }
            return null;
        }

        private bool 验证节点匹配(IPEndPoint __源, IPEndPoint __标准)
        {
            if (!__标准.Address.Equals(IPAddress.Any) && !__标准.Address.Equals(__源.Address))
            {
                return false;
            }
            if (__标准.Port != 0 && !__标准.Port.Equals(__源.Port))
            {
                return false;
            }
            return true;
        }

        private void 增加解码后报文(M解码后报文 __报文)
        {
            var __row = _数据表缓存.NewRow();
            __row["时间"] = __报文.时间;
            __row["业务"] = __报文.业务;
            __row["发送方"] = __报文.发送方;
            __row["接收方"] = __报文.接收方;
            __row["概述"] = __报文.概述;
            __row["报文"] = __报文;
            _数据表缓存.Rows.Add(__row);
        }
    }
}
