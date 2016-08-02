using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace UI
{
    public partial class F详细信息 : XtraForm
    {
        public F详细信息(string __内容)
        {
            InitializeComponent();
            this.memoEdit1.Text = __内容;
        }
    }
}
