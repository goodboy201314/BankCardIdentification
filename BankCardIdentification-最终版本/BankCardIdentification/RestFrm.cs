using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BankCardIdentification
{
    public partial class RestFrm : Form
    {
        int[] num = null;
        public RestFrm(int[] postNum)
        {
            InitializeComponent();
            this.num = postNum;
        }

        private void RestFrm_Load(object sender, EventArgs e)
        {
            this.richTextBox1.Text = "";
            for (int i = 0; i < num.Length; i++)
            {
                this.richTextBox1.Text += "number=" + i + " : " + num[i] + "\n";
            }
        }
    }
}
