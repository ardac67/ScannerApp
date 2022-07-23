using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WIATest;
using TwainDotNet;
using TwainDotNet.WinFroms;

namespace Scannerapplication
{
    using TwainDotNet.TwainNative;
    public partial class Form1 : Form
    {
       
        string MainPath = ConfigurationManager.AppSettings["MainPath"].ToString();
        string MainPathRetailCustomer;
        Twain _twain;
        ScanSettings _settings;
        int tabCount = 0;
        public Form1()
        {
            InitializeComponent();
            _twain = new Twain(new WinFormsWindowMessageHook(this));
            _twain.TransferImage += delegate (Object sender, TransferImageEventArgs args)
            {
                if (args.Image != null)
                {
                    TabPage tabPage = new TabPage();
                    PictureBox picBox = new PictureBox();
                    picBox.Dock = DockStyle.Fill;
                    picBox.Image = args.Image;
                    tabPage.Controls.Add(picBox);
                    picBox.Show();
                    picBox.SizeMode = PictureBoxSizeMode.StretchImage;
                    tabControl1.Controls.Add(tabPage);
                    CreateOrInsertFile(textBox1.Text, comboBox1.SelectedIndex, (Image)picBox.Image);
                    tabPage.Text = "Sayfa  " + tabCount + "";
                    tabCount++;
                }
            };
            _twain.ScanningComplete += delegate
            {
                Enabled = true;
            };
        }
        //button click event
        private void btn_scan_Click(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true || checkBox2.Checked==false)
            {
                try
                {
                    tabCount = 0;
                    _twain.SelectSource();
                    Enabled = false;

                    _settings = new ScanSettings();

                    //_settings.UseAutoScanCache = true;

                    _settings.ShouldTransferAllPages = true;
                    _settings.UseDuplex = true;



                    //_settings.ShowTwainUI = true;
                    try
                    {
                        _twain.StartScanning(_settings);

                        //pic_scan.Image = args.Image;
                        //picbox.SizeMode = PictureBoxSizeMode.StretchImage;
                        //
                    }
                    catch (TwainException ex)
                    {
                        MessageBox.Show(ex.Message);
                        Enabled = true;
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }
                tabCount++;
            }
           
        }


        private void Home_SizeChanged(object sender, EventArgs e)
        {
            //int pheight = this.Size.Height - 153;
            //pic_scan.Size = new Size(pheight - 150, pheight);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            List<string> devices = WIAScanner.GetDevices();
            comboBox1.SelectedIndex = 0;

            foreach (string device in devices)
            {
                lbDevices.Items.Add(device);
            }
            //check if device is not available
            if (lbDevices.Items.Count == 0)
            {
                MessageBox.Show("Tarayıcı veya tarayıcılar bulunamadı.");
                //this.Close();
            }
            else
            {
                lbDevices.SelectedIndex = 0;
            }
            CreateOrCheckDirectory();
            CheckConnection();
        }
        private void CheckConnection()
        {
            using (SqlConnection cnn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conn"].ToString()))
            {
                try
                {
                    cnn.Open();

                    if (cnn.State == ConnectionState.Open)
                    {
                        return;
                    }
                    else MessageBox.Show("Bağlantı sağlanamadı/Vpn veya internet bağlantısını kontrol edin.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void CreateOrCheckDirectory()
        {
            try
            {
                string ControlDirectory = Path.Combine(MainPath, "RetailCustomer");
                if (!Directory.Exists(ControlDirectory))
                {
                    Directory.CreateDirectory(ControlDirectory);
                }
                MainPathRetailCustomer = ControlDirectory;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private string  GetCustomer(string CustomerNumber)
        {
            
                try
                {

                    SqlConnection cnn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conn"].ToString());

                    string procName = ConfigurationManager.AppSettings["ProcedureName1"].ToString();

                    cnn.Open();

                    SqlCommand cmd = new SqlCommand(procName, cnn);

                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.Add("@CustomerNumber", System.Data.SqlDbType.VarChar, 50).Value = CustomerNumber;

                    SqlDataReader reader = cmd.ExecuteReader();

                    string customerName;


                    while (reader.Read())
                    {
                        customerName = reader["CustomerName"].ToString();

                        return customerName;
                    }

                    cnn.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    //return ex.Message;
                }

                return null;
            
        }

      

        private void CreateOrInsertFile(string CustomerNumber,int SelectedIndex,Image img)
        {
            try
            {
                string FileName = DateTime.Now.ToString("yyyy-MM-dd HHmmss");
                if (CustomerNumber != null)
                {
                    if (!Directory.Exists(Path.Combine(MainPathRetailCustomer, CustomerNumber)))
                    {
                        Directory.CreateDirectory(Path.Combine(MainPathRetailCustomer, CustomerNumber));

                        if (SelectedIndex == 0)
                        {
                            Directory.CreateDirectory(Path.Combine(MainPathRetailCustomer, CustomerNumber, "Müşteri Kimlik"));
                            img.Save(Path.Combine(MainPathRetailCustomer, CustomerNumber, "Müşteri Kimlik", FileName + ".jpeg"), ImageFormat.Jpeg);
                        }
                        if (SelectedIndex == 1)
                        {
                            Directory.CreateDirectory(Path.Combine(MainPathRetailCustomer, CustomerNumber, "Kefil Kimlik"));
                            img.Save(Path.Combine(MainPathRetailCustomer, CustomerNumber, "Kefil Kimlik", FileName + ".jpeg"), ImageFormat.Jpeg);
                        }
                        if (SelectedIndex == 2)
                        {
                            Directory.CreateDirectory(Path.Combine(MainPathRetailCustomer, CustomerNumber, "Diğer Belgeler"));
                            img.Save(Path.Combine(MainPathRetailCustomer, CustomerNumber, "Diğer Belgeler", FileName + ".jpeg"), ImageFormat.Jpeg);
                        }

                        //img.Save(Path.Combine(MainPathRetailCustomer, CustomerNumber,FileName+".jpeg"), ImageFormat.Jpeg);
                    }
                    else if (Directory.Exists(Path.Combine(MainPathRetailCustomer, CustomerNumber)))
                    {
                        if (SelectedIndex == 0)
                        {
                            if (!Directory.Exists(Path.Combine(MainPathRetailCustomer, CustomerNumber, "Müşteri Kimlik")))
                            {
                                Directory.CreateDirectory(Path.Combine(MainPathRetailCustomer, CustomerNumber, "Müşteri Kimlik"));
                                img.Save(Path.Combine(MainPathRetailCustomer, CustomerNumber, "Müşteri Kimlik", FileName + ".jpeg"), ImageFormat.Jpeg);
                            }
                            else if (Directory.Exists(Path.Combine(MainPathRetailCustomer, CustomerNumber, "Müşteri Kimlik")))
                            {
                                img.Save(Path.Combine(MainPathRetailCustomer, CustomerNumber, "Müşteri Kimlik", FileName + ".jpeg"), ImageFormat.Jpeg);
                            }
                        }
                        if (SelectedIndex == 1)
                        {
                            if (!Directory.Exists(Path.Combine(MainPathRetailCustomer, CustomerNumber, "Kefil")))
                            {
                                Directory.CreateDirectory(Path.Combine(MainPathRetailCustomer, CustomerNumber, "Kefil"));
                                img.Save(Path.Combine(MainPathRetailCustomer, CustomerNumber, "Kefil", FileName + ".jpeg"), ImageFormat.Jpeg);
                            }
                            else if (Directory.Exists(Path.Combine(MainPathRetailCustomer, CustomerNumber, "Kefil")))
                            {
                                img.Save(Path.Combine(MainPathRetailCustomer, CustomerNumber, "Kefil", FileName + ".jpeg"), ImageFormat.Jpeg);
                            }
                        }
                        if (SelectedIndex == 2)
                        {
                            if (!Directory.Exists(Path.Combine(MainPathRetailCustomer, CustomerNumber, "Diğer Belgeler")))
                            {
                                Directory.CreateDirectory(Path.Combine(MainPathRetailCustomer, CustomerNumber, "Diğer Belgeler"));
                                img.Save(Path.Combine(MainPathRetailCustomer, CustomerNumber, "Diğer Belgeler", FileName + ".jpeg"), ImageFormat.Jpeg);
                            }
                            else if (Directory.Exists(Path.Combine(MainPathRetailCustomer, CustomerNumber, "Diğer Belgeler")))
                            {
                                img.Save(Path.Combine(MainPathRetailCustomer, CustomerNumber, "Diğer Belgeler", FileName + ".jpeg"), ImageFormat.Jpeg);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata");
            } 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string CustomerName = GetCustomer(textBox1.Text);
            if (CustomerName != null)
            {

                checkBox1.Checked = true;
                //CustomerName = GetCustomer(textBox1.Text);
                textBox2.Text = CustomerName;

            }
            else MessageBox.Show("Müşteri Bulunamadı");
        }

       

        private void textBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string CustomerName = GetCustomer(textBox1.Text);
            if (CustomerName != null)
            {

                checkBox1.Checked = true;
                //CustomerName = GetCustomer(textBox1.Text);
                textBox2.Text = CustomerName;

            }
            else MessageBox.Show("Müşteri Bulunamadı");
        }

        private List<Image> Scan()
        {
            List<Image> ret = new List<Image>();



            return ret;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tabControl1.TabPages.Clear();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try {
                string directory = null;
                string folderDestination = null;
                try
                {
                    if (textBox1.Text != "" && textBox2.Text != "")
                    {
                        directory = Path.Combine(MainPath, "RetailCustomer", textBox1.Text);
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                            Directory.CreateDirectory(Path.Combine(directory, "Müşteri Kimlik"));
                            Directory.CreateDirectory(Path.Combine(directory, "Kefil"));
                            Directory.CreateDirectory(Path.Combine(directory, "Diğer Belgeler"));
                        }
                        if (Directory.Exists(directory))
                        {
                            if (!Directory.Exists(Path.Combine(MainPath, "RetailCustomer", textBox1.Text, "Müşteri Kimlik")))
                            {
                                Directory.CreateDirectory(Path.Combine(MainPath, "RetailCustomer", textBox1.Text, "Müşteri Kimlik"));
                            }
                            if (!Directory.Exists(Path.Combine(MainPath, "RetailCustomer", textBox1.Text, "Kefil")))
                            {
                                Directory.CreateDirectory(Path.Combine(MainPath, "RetailCustomer", textBox1.Text, "Kefil"));
                            }
                            if (!Directory.Exists(Path.Combine(MainPath, "RetailCustomer", textBox1.Text, "Diğer Belgeler")))
                            {
                                Directory.CreateDirectory(Path.Combine(MainPath, "RetailCustomer", textBox1.Text, "Diğer Belgeler"));
                            }

                        }


                    }
                    else MessageBox.Show("Müşteri Numarası Giriniz!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                folderBrowserDialog1.SelectedPath = Path.Combine(directory);
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    folderDestination = folderBrowserDialog1.SelectedPath;
                }
                if (folderDestination != null)
                {
                    OpenFileDialog fileDialog = new OpenFileDialog();
                    fileDialog.Multiselect = true;
                    fileDialog.Title = "Dosya Seçiniz!";
                    if (fileDialog.ShowDialog() == DialogResult.OK)
                    {
                        foreach (string fileName in fileDialog.FileNames)
                        {
                            System.IO.File.Copy(fileName, folderDestination + @"\" + System.IO.Path.GetFileName(fileName));
                        }
                    }
                }


            }
            catch(Exception ex) { MessageBox.Show(ex.Message); }
            }
        

       
    }
}
