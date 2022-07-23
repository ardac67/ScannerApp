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
using ZXing;

namespace Scannerapplication
{
    public partial class MultiplePage : Form
    {

        string MainPath = ConfigurationManager.AppSettings["MainPath"].ToString();
        string MainPathRetailCustomer = null;
        //string Result;
        
        public MultiplePage()
        {
            InitializeComponent();
        }
        //button click event
        
        private void btn_scan_Click(object sender, EventArgs e)
        {

            
            textBox1.Text = MainPath;
            try
            {
               
                //get images from scanner
                List<Image> images = WIAScanner.Scan((string)lbDevices.SelectedItem);
                int pagenum = 0;
                int errorCount = 0;

                foreach (Image image in images)
                {
                    try
                    {
                        
                        TabPage tabPage = new TabPage();
                        PictureBox picBox = new PictureBox();
                        picBox.Dock = DockStyle.Fill;
                        string  Result;
                        string CustomerNumber;
                        //string CustomerFolder;
                        picBox.Image = image;
                        tabPage.Controls.Add(picBox);
                        picBox.Show();
                        picBox.SizeMode = PictureBoxSizeMode.StretchImage;
                        //tabPage.Text = pagenum.ToString();
                        

                        tabControl1.Controls.Add(tabPage);
                        if (BarcodeScan(image) == null)
                        {
                            tabPage.Text = "Başarısız";
                            errorCount++;
                            //throw new NotImplementedException("Okunamadı");

                        }
                       
                        else
                        {
                            Result = BarcodeScan(image).ToString(); textBox2.Text = Result;

                            if (Result != null)
                            {
                                CustomerNumber = GetCustomer(Result);
                                textBox3.Text = CustomerNumber;
                                FileSave(CustomerNumber, Result, image); //(string customerNumber, string BarcodeResult, Image img)
                                tabPage.Text = "Başarılı";
                            }
                            else MessageBox.Show("Müşteri bulunamadı/Barkod okunamadı", "Hata");
                        }

                        //image.Save(@"D:\" + DateTime.Now.ToString("yyyy-MM-dd HHmmss") + ".jpeg", ImageFormat.Jpeg);
                        
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Hata");
                    }
                    pagenum++;
                }
                if (errorCount > 0)
                {
                    MessageBox.Show("" + errorCount + " sayfadaki barkod okunamadı!!");
                }


            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
           
        }


        private void Home_SizeChanged(object sender, EventArgs e)
        {
           // int pheight = this.Size.Height - 153;
           // picBox.Size = new Size(pheight - 150, pheight);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //get list of devices available
            List<string> devices = WIAScanner.GetDevices();

            foreach (string device in devices)
            {
                lbDevices.Items.Add(device);
            }


            //check if device is not available
            if (lbDevices.Items.Count == 0)
            {
                MessageBox.Show("Tarayıcı veya tarayıcılar bulunamadı port bağlantılarını kontrol edin!");
                //this.Close();
            }
            else
            {
                lbDevices.SelectedIndex = 0;
            }

            CreateOrCheckDirectory();

            CheckConnection();
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

        private ZXing.Result BarcodeScan(Image _Image)
        {
            BarcodeReader reader = new BarcodeReader();

            var barcodeBitmap = (Bitmap)_Image;

            var result = reader.Decode(barcodeBitmap);
            try
            {
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Warning");
                return result;
            }



        }

        private string GetCustomer(string result)
        {
            try
            {

                SqlConnection cnn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conn"].ToString());

                string procName = ConfigurationManager.AppSettings["ProcedureName"].ToString();

                cnn.Open();

                SqlCommand cmd = new SqlCommand(procName, cnn);

                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.Add("@DocumentNum", System.Data.SqlDbType.VarChar, 50).Value = result;

                SqlDataReader reader = cmd.ExecuteReader();

                string customerID;


                while (reader.Read())
                {
                    customerID = reader["CustomerID"].ToString();

                    return customerID;
                }

                cnn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return ex.Message;
            }

            return null;
        }

        private void FileSave(string customerNumber, string BarcodeResult, Image img)
        {
            string FolderType = BarcodeResult;
            string CustomerFolder = Path.Combine(MainPathRetailCustomer, customerNumber);
            if (!Directory.Exists(CustomerFolder))
            {
                try
                {
                    Directory.CreateDirectory(CustomerFolder);
                    if (BarcodeResult != null)
                    {


                        if (FolderType.IndexOf("DV") != -1)
                        {
                            int index1 = FolderType.IndexOf("DV");
                            string checkString = FolderType.Substring(index1, 2);

                            if (checkString == "DV")
                            {
                                textBox4.Text = "Senet";
                                string senet = "Senetler";
                                string pathForSave = Path.Combine(CustomerFolder, senet);
                                pathForSave = Path.Combine(pathForSave, FolderType);
                                if (!Directory.Exists(pathForSave))
                                {
                                    Directory.CreateDirectory(pathForSave);

                                    string FileName = DateTime.Now.ToString("yyyy-MM-dd HHmmss");
                                    textBox5.Text = FileName;
                                    img.Save(Path.Combine(pathForSave, FileName + ".jpeg"), ImageFormat.Jpeg);

                                }
                            }
                        }
                        else if (FolderType.IndexOf("RI") != -1)
                        {
                            int index1 = FolderType.IndexOf("RI");
                            string checkString = FolderType.Substring(index1, 2);
                            if (checkString == "RI")
                            {
                                textBox4.Text = "Sozlesme";
                                string sozlesme = "Sozlesmeler";
                                string pathForSaveContract = Path.Combine(CustomerFolder, sozlesme);
                                pathForSaveContract = Path.Combine(pathForSaveContract, FolderType);
                                if (!Directory.Exists(pathForSaveContract))
                                {
                                    Directory.CreateDirectory(pathForSaveContract);

                                    string FileName = DateTime.Now.ToString("yyyy-MM-dd HHmmss");
                                    textBox5.Text = FileName;
                                    img.Save(Path.Combine(pathForSaveContract, FileName + ".jpeg"), ImageFormat.Jpeg);
                                }
                            }

                        }

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
            else if (Directory.Exists(CustomerFolder))
            {
                try
                {
                    if (BarcodeResult != null)
                    {
                        string UniqueFolder = BarcodeResult;

                        if (UniqueFolder.IndexOf("DV") != -1)
                        {
                            int index1 = UniqueFolder.IndexOf("DV");
                            string checkString = UniqueFolder.Substring(index1, 2);

                            if (checkString == "DV")
                            {
                                textBox4.Text = "Senet";
                                string senet = "Senetler";
                                string parhForSave = Path.Combine(CustomerFolder, senet);
                                parhForSave = Path.Combine(parhForSave, UniqueFolder);
                                if (!Directory.Exists(parhForSave))
                                {
                                    Directory.CreateDirectory(parhForSave);


                                    string FileName = DateTime.Now.ToString("yyyy-MM-dd HHmmss");
                                    textBox5.Text = FileName;
                                    img.Save(Path.Combine(parhForSave, FileName + ".jpeg"), ImageFormat.Jpeg);

                                }
                                else
                                {
                                    string FileName = DateTime.Now.ToString("yyyy-MM-dd HHmmss");
                                    textBox5.Text = FileName;
                                    img.Save(Path.Combine(parhForSave, FileName + ".jpeg"), ImageFormat.Jpeg);
                                }

                            }
                        }
                        else if (UniqueFolder.IndexOf("RI") != -1)
                        {
                            int index1 = UniqueFolder.IndexOf("RI");
                            string checkString = UniqueFolder.Substring(index1, 2);
                            if (checkString == "RI")
                            {
                                textBox4.Text = "Sozlesme";
                                string sozlesme = "Sozlesmeler";
                                string parhForSave = Path.Combine(CustomerFolder, sozlesme);
                                parhForSave = Path.Combine(parhForSave, UniqueFolder);
                                if (!Directory.Exists(parhForSave))
                                {
                                    Directory.CreateDirectory(parhForSave);

                                    string FileName = DateTime.Now.ToString("yyyy-MM-dd HHmmss");
                                    textBox5.Text = FileName;
                                    img.Save(Path.Combine(parhForSave, FileName + ".jpeg"), ImageFormat.Jpeg);

                                }
                                else
                                {
                                    string FileName = DateTime.Now.ToString("yyyy-MM-dd HHmmss");
                                    textBox5.Text = FileName;
                                    img.Save(Path.Combine(parhForSave, FileName + ".jpeg"), ImageFormat.Jpeg);
                                }
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tabControl1.TabPages.Clear();
        }
    }
}
