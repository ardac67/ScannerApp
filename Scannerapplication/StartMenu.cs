using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Scannerapplication
{
    public partial class StartMenu : Form
    {
        public StartMenu()
        {
            InitializeComponent();
        }
        MultiplePage mltppage= new MultiplePage();  
        Form1 frm1= new Form1();
        private void button1_Click(object sender, EventArgs e)
        {
            try { mltppage.Show(); }
            catch { MultiplePage pg =new MultiplePage();
                pg.Show();
            }
                
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try { frm1.Show(); }
            catch
            {
                Form1 frm2=new Form1();
                frm2.Show();
            }
        }
    }
}
