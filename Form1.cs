using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using System.Windows.Forms.DataVisualization.Charting;


namespace ProxyAnalyser
{
    public partial class Form1 : Form
    {
        private System.Diagnostics.FileVersionInfo myFileVersionInfo;
        private string strVersion;
        private string[] _Directions = new string[300]; //Directions
        private string[] _DirectionsDiscription = new string[300]; //Discription of the Directions
        private double[] _DirectionsByte = new double[300]; //Downloaded by every of the Directions of the whole time
        private double[] _DirectionsTime = new double[300]; //Elapsed time by every of the Directions of the whole time

        private double[] _substringMonthAndBytes = new double[12]; //Downloaded by every Period of the whole time
        private double[,] _BytesEveryYearMonthByDirection = new double[300, 12]; //every Direction and every Period with summarize Data
        private string[,] _BytesEveryYearMonthByURL = new string[300, 36]; //TOP100 URL /Bytes by every Period with summarize Data
        private string[,] _BytesTopUrlTotal = new string[300, 4]; //TOP100 URL/Direction/Bytes/Time in Total

        //0-99 - bytes at directions, by 24 times 
        private int[] _dm = { 299, 299, 299, 299, 299 }; //индексы направлений с максиммальным объемом скачиваного

        private double[] _DirectionsMonthByte = new double[300]; //Downloaded by every of the Directions of the whole time
        private double[] _DirectionsMonthTime = new double[300]; //Elapsed time by every of the Directions of the whole time

        private string[] substringDirectionIni = new string[300]; //temporary array for directions was taken from the substringDirectionIniFull[]
        private string[] substringDirectionIniFull = new string[300]; // array for directions was taken from the ini file's

        private HashSet<string> hsClearingIni = new HashSet<string>();
        private HashSet<string> hsSimplifyingIni = new HashSet<string>();
        private HashSet<string> hsSimplifying2Ini = new HashSet<string>();
        private HashSet<string> hsReplacingIni = new HashSet<string>();
        private string[] arrayReplacingIni = new string[1];
        private string[] arraySimplifying2Ini = new string[1];
        private string[] arraySimplifyingIni = new string[1];
        private string[] arrayClearingIni = new string[1];

        private List<string> listProxyCheckerIni = new List<string>();

        private string[] substringMonthSummary1 = new string[6000]; //1st collumn - URL
        private double[] substringMonthSummary2 = new double[6000]; //2nd collumn - Bytes
        private double[] substringMonthSummary3 = new double[6000]; //3rd collumn - Ellapsed Time
        private string[] substringMonthSummary4 = new string[6000]; //4th collumn - Direction
        private string[] substringMonthSummary5 = new string[6000]; //5th collumn - Month
        private int[] substringMonthSummary6 = new int[6500]; //6th collumn - Year
        private string direction; //Direction
        public string _uUser; //login of the Selected User at analysing
        public string _toolTipLabel2 = "";

        public iTextSharp.text.Image LogoPNG;
        //   private string UserName;  // for SB - UserName = "sb"
        //    private string UserPassword;  // for SB - UserPassword = "Sicherheit"
        //   private string myPrLogKey = @"SOFTWARE\RYIK\ProxyAnalyser2";

        Form2 f2;

        public Form1()  //for transfer any data between Form1 and Form12
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) //Read ProxyAnalyser.ini into arrays at memory in the start of the Form1
        {
            Icon = Properties.Resources.iconRYIK;                   //my icon
            notifyIcon.Icon = Properties.Resources.iconRYIK;
            Bitmap bmplogo = new Bitmap(Properties.Resources.LogoRYIK);
            var converter = new ImageConverter();
            LogoPNG = iTextSharp.text.Image.GetInstance((byte[])converter.ConvertTo(bmplogo, typeof(byte[])));

            myFileVersionInfo = myFileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
            strVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            StatusLabel1.Text = myFileVersionInfo.Comments + " ver." + myFileVersionInfo.FileVersion + " b." + strVersion + " " + myFileVersionInfo.LegalCopyright;
            _MakeIni makeIni = new _MakeIni();
            try
            {
                if (!File.Exists("ProxyAnalyser.ini"))
                { makeIni.CreateIni(); }
            }
            catch { MessageBox.Show("Отсутствует файл ProxyAnalyser.ini\nПри создании его возникли проблемы!"); }
            finally { makeIni = null; }

            preparedDataToolStripMenuItem.Enabled = false;
            exportToExcelSummarizeToolStripMenuItem.Enabled = false;
            StatusLabel2.Visible = false;
            printReportFullToolStripMenuItem.Enabled = false;
            printDialogToolStripMenuItem.Enabled = false;
            tabControl1.Enabled = false;
            printReportFullToolStripMenuItem.Enabled = true;
            printDialogToolStripMenuItem.Enabled = true;
            exportPDFTestToolStripMenuItem.Enabled = false;
            StatusLabel1.Visible = true;
            exportDataToolStripMenuItem.Enabled = false;

            labelDescription.Text = "Описание категорий";
            labelTOPSitesTotal.Text = "";
            labelTOP100.Text = "";
            StatusLabel3.Text = "";
            MainToolStripMenuItem.ToolTipText = "Получение данных, Выход из программы";
            ReadIniTolist();
            comboDirection.Items.Clear();
            _initoArrayDirection();

            //     _printIniDB();
            _CheckTemporaryHTML();
        }

        private void _LoadAndSelectData_Click(object sender, EventArgs e) //Open Form2  "Select Data"
        {
            //The Start of The Block. for transfer any data between Form1 and Form2
            this.Hide();
            comboMonth.Items.Clear();
            f2 = new Form2(this);

            f2.ShowDialog();
            this.Show();
            f2.Close();
            f2.Dispose();
            _CheckTemporaryHTML();
        }

        private void _MakeResault_Click(object sender, EventArgs e) //analyzing of the loaded data 
        { _MakeDone(); }

        private void cleanFolder_Click(object sender, EventArgs e) //Use CleanTempFolder()
        { CleanTempFolder(); }

        private void CleanTempFolder() //Clean the folder called "ProxyAnalyser"
        {
            ProgressBar1.Value = 0;
            tabControl1.Enabled = false;
            DirectoryInfo dirInfo = new DirectoryInfo(Environment.CurrentDirectory + "\\ProxyAnalyser\\");

            if (dirInfo.Exists)
            {
                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    try
                    {
                        file.Delete();
                        _ProgressWork1();
                    }
                    catch { }
                }
                try
                {
                    dirInfo.Delete(true);
                    StatusLabel2.Text = @"Очистка завершена успешно!";
                    MessageBox.Show("Очистка завершена успешно!");
                }
                catch (Exception expt)
                {
                    MessageBox.Show(expt.Message);
                    StatusLabel2.Text = @"Очистка завершена неудачно!";
                }
            }
            else
            {
                MessageBox.Show("Очистка завершена успешно!");
                StatusLabel2.Text = @"Очистка завершена успешно!";
            }
            ProgressBar1.Value = 100;
            exportDataToolStripMenuItem.Enabled = false;
        }

        public void _MakeDone() //Печать графиков за месяц через бэкграунд
        {
            ProgressBar1.Value = 0;
            StatusLabel2.Text = "Идет обработка данных";
            _ProgressWork2();

            for (int i = 0; i < 5999; i++) // предварительное обнуление массива
            {
                substringMonthSummary1[i] = "";
                substringMonthSummary2[i] = 0;
                substringMonthSummary3[i] = 0;
                substringMonthSummary4[i] = "Common";
                substringMonthSummary5[i] = "";
                substringMonthSummary6[i] = 1900;
            }
            _ProgressWork2();

            for (int i = 0; i < 12; i++) // предварительное обнуление массива
            { _substringMonthAndBytes[i] = 0; }
            _ProgressWork2();

            int ii;
            string k = null, kk = null, t = "ProxyAnalyser\\!htupm";
            for (int i = 1; i < 13; i++)
            {
                k = t + i + ".tmp";
                if (File.Exists(k))
                {
                    ii = (i - 1) * 500;
                    FileInfo file = new FileInfo(k);
                    if ((file.Length / 1024) > 2)
                    {
                        _ParsingHtmlToCSV(k, ii);
                        _UserInfoFromFileIntoArrays(k, ii);
                        _UserInfoFromFileIntoCombobox(k, ii);
                        _ParsingHtmlTotalAmount(k, i - 1);
                    }
                }

                k = t + i + "_.tmp";
                if (File.Exists(kk))
                {
                    ii = (i - 1) * 500 + 250;
                    FileInfo file = new FileInfo(k);
                    if ((file.Length / 1024) > 2)
                    {
                        _ParsingHtmlToCSV((k), ii);
                        _UserInfoFromFileIntoArrays(k, ii);
                        _UserInfoFromFileIntoCombobox(k, ii);
                        _ParsingHtmlTotalAmount(k, i - 1);
                    }
                }
            }
            _ProgressWork10();

            StatusLabel2.Visible = true;

            _ReadArrayAndSimplify(); // Simplify an every URL from the DB
            _ReadArrayAndSetUrlStatus();

            _InfoStaticsFull();
            _ProgressWork10();
            /////////////////// work with loaded DATA ////////////////////////////
            comboMonth.SelectedIndex = 0;
            _InfoStaticsDirection();
            _ProgressWork10();
            _InfoStaticsTotalMonth();
            _ProgressWork10();
            _InfoStaticsUrlByMonth();
            _ProgressWork10();
            _InfoStaticsMonth();
            _ProgressWork10();
            _InfoStaticsDirectionTotalMonth();
            _ProgressWork2();
            _InfoStaticsBytesByDirection();
            _ProgressWork10();
            _URLTOP100Total(); //Make the Table URLTOP100Total
            labelTOPSitesTotal.Font = new Font("Arial", 9, FontStyle.Bold);
            labelTOPSitesTotal.Text = "TOP сайтов";
            preparedDataToolStripMenuItem.Enabled = true;
            exportDataToolStripMenuItem.Enabled = true;
            printReportFullToolStripMenuItem.Enabled = true;
            printDialogToolStripMenuItem.Enabled = true;
            exportPDFTestToolStripMenuItem.Enabled = true;
            exportDataToolStripMenuItem.Enabled = true;
            tabControl1.Enabled = true;

            StatusLabel2.Text = "Обработка данных завершена";

            ProgressBar1.Value = 100;
        }

        private void ReadIniTolist() //Read ProxyAnalyser.ini and write whole settings into the list "listProxyCheckerIni"
        {
            listProxyCheckerIni = new List<string>();
            var Coderwin = Encoding.GetEncoding(1251);//For The Pages at win1251
            string s = null;

            using (StreamReader Readerwin = new StreamReader("ProxyAnalyser.ini", Coderwin))
            {
                while ((s = Readerwin.ReadLine()) != null)
                {
                    if (s.Trim().Length > 0 && !s.Contains('#'))
                    { listProxyCheckerIni.Add(s.Trim()); }
                }
            }
            _initoArraysClearerAndSiplifierUrl();
        }

        private void _initoArraysClearerAndSiplifierUrl() //Read listProxyCheckerIni and Make HashLists with settings
        {
            string s = ""; bool bListUrls = false;

            bListUrls = false;
            for (int i = 0; i < listProxyCheckerIni.ToArray().Length; i++)
            {
                s = listProxyCheckerIni[i].ToLower();
                if (s.Contains(@";cleaner".ToLower())) { bListUrls = true; continue; }  //Start of list
                if (s.Contains(@";end cleaner".ToLower())) { bListUrls = false; break; }      //End of list
                if (bListUrls) { hsClearingIni.Add(s); }
            }

            bListUrls = false;
            for (int i = 0; i < listProxyCheckerIni.ToArray().Length; i++)
            {
                s = listProxyCheckerIni[i].ToLower();
                if (s.Contains(@";simplifier".ToLower())) { bListUrls = true; continue; }  //Start of list
                if (s.Contains(@";end simplifier".ToLower())) { bListUrls = false; break; }      //End of list
                if (bListUrls) { hsSimplifyingIni.Add(s); }
            }

            bListUrls = false;
            for (int i = 0; i < listProxyCheckerIni.ToArray().Length; i++)
            {
                s = listProxyCheckerIni[i].ToLower();
                if (s.Contains(@";SimplifyEnd".ToLower())) { bListUrls = true; continue; }  //Start of list
                if (s.Contains(@";End SimplifyEnd".ToLower())) { bListUrls = false; break; }      //End of list
                if (bListUrls) { hsSimplifying2Ini.Add(s); }
            }

            bListUrls = false;
            for (int i = 0; i < listProxyCheckerIni.ToArray().Length; i++)
            {
                s = listProxyCheckerIni[i].ToLower();
                if (s.Contains(@";replacer".ToLower())) { bListUrls = true; continue; }  //Start of list
                if (s.Contains(@";end replacer".ToLower())) { bListUrls = false; break; }      //End of list
                if (bListUrls) { hsReplacingIni.Add(s); }
            }

            arrayReplacingIni = hsReplacingIni.ToArray();
            arraySimplifying2Ini = hsSimplifying2Ini.ToArray();
            arraySimplifyingIni = hsSimplifyingIni.ToArray();
            arrayClearingIni = hsClearingIni.ToArray();
        }

        private void _initoArrayDirection() //Read Direction from listProxyCheckerIni and it writes into the array "substringDirectionIniFull"
        {
            string s = "";
            for (int i = 0; i < 99; i++) // предварительное обнуление массива
            {
                substringDirectionIniFull[i] = "";
                _DirectionsDiscription[i] = "no";
            }

            s = null; bool bListDiscr = false;
            int k = 0;
            for (int i = 0; i < listProxyCheckerIni.ToArray().Length; i++)
            {
                s = listProxyCheckerIni[i];
                if (s.ToLower().Contains(";Direction".ToLower())) { bListDiscr = true; }
                if (s.ToLower().Contains(";End Direction".ToLower())) { bListDiscr = false; }
                if (!s.Contains(";") && s.Contains("=") && bListDiscr)
                {
                    substringDirectionIniFull[k] = s.ToString().Trim();
                    k++;
                    if (k > 98)
                    { textBox1.AppendText("- Не могу добавить строку для анализа:  " + s.ToString().Trim() + "\n"); k = 98; }
                }
            }

            comboDirection.Sorted = true;
            for (int i = 0; i < 99; i++)    //Write a direction at array "_Directions" only one time
            {
                string a = substringDirectionIniFull[i];
                string[] bArray;
                try { bArray = Regex.Split(a.ToString(), "=| "); } catch { bArray = new string[1]; bArray[0] = ""; }
                if (a.Length > 1 && bArray[0].Trim().Length > 2)
                {
                    if (i == 0)
                    {
                        _Directions[i] = bArray[0].Trim();
                        comboDirection.Items.Add(bArray[0].Trim()); //Prepare Combobox Direction

                    }
                    if (i > 0)
                    {
                        bool DirectionAtArray = false;
                        for (int itr = 0; itr < _Directions.Length; itr++)
                        {
                            if (_Directions[itr] == bArray[0].Trim())
                            { DirectionAtArray = true; }
                        }
                        if (DirectionAtArray == false)
                        {
                            _Directions[i] = bArray[0].Trim();
                            comboDirection.Items.Add(bArray[0].Trim());
                        }
                    }
                }
            }

            s = null; bListDiscr = false;
            for (int i = 0; i < listProxyCheckerIni.ToArray().Length; i++)
            {
                s = listProxyCheckerIni[i];
                if (s.Contains(";Disciption of the Direction".ToLower())) { bListDiscr = true; continue; }
                if (s.Contains(";End Disciption of the Direction".ToLower())) { bListDiscr = false; break; }
                if (!s.Contains(';') && s.Contains("=") && bListDiscr)
                {
                    string[] tmpDscrDirctn = Regex.Split(s, "=");
                    for (int iD = 0; iD < 99; iD++)
                    {
                        string o2 = tmpDscrDirctn[0].ToLower().Trim();
                        string o1 = _Directions[iD].ToLower();
                        if (o1.Length > 1 && o1 == o2)
                        {
                            _DirectionsDiscription[iD] = tmpDscrDirctn[1].Trim();
                            iD++;
                        }
                    }
                }
            }
        }

        private void _ReadArrayAndSimplify() //упрощение каждого URL
        {
            string a = "", b = "", c = "", d = "", tmpReplUrl0 = "", sDomainLevelsUserEnd = "";
            string[] tmpReplUrls1 = new string[] { "" };
            string[] domainLevelsUser = new string[] { "" };

            for (int l = 0; l < 5999; l++)
            {
                c = substringMonthSummary1[l].Trim().ToLower();

                if (c.Length > 2)
                {
                    //Remove end of URLs with ':'
                    if (c.Contains(':')) { d = c.Split(':')[0]; }
                    else { d = c; }

                    //Remove start of URLs with www. and *. in the start of URL
                    domainLevelsUser = d.Split('.');
                    if ((domainLevelsUser[0] == "www" || domainLevelsUser[0].Contains('*')) && domainLevelsUser.Length - 1 > 1)
                    { substringMonthSummary1[l] = string.Join(".", domainLevelsUser, 1, domainLevelsUser.Length - 1); }
                    else
                    { substringMonthSummary1[l] = d; }

                    //Replacer
                    domainLevelsUser = substringMonthSummary1[l].Split('.');

                    for (int iT = 0; iT < arrayReplacingIni.Length; iT++)
                    {
                        a = (arrayReplacingIni[iT]).Trim(); //строка с перечнем доменов 
                        if (domainLevelsUser.Length > 1)
                        {
                            string[] tmpReplUrls = a.Split('='); //масив из 2 частей. 1-я часть - домен, на который будет произведена замена, 2-я - массив доменов с которых будет произведена замена на tmpReplUrl0
                            tmpReplUrl0 = tmpReplUrls[0].Trim(); //домен, на который будет произведена замена
                            tmpReplUrls1 = tmpReplUrls[1].Trim().Split(' ');  //массив доменов с которых будет произведена замена на tmpReplUrl0
                            // Перебор доменов идет слева направо
                            // После нахождения первого совпадения перебор прекращается
                            if (tmpReplUrl0.Length > 1 && tmpReplUrls1.Length > 1)
                            {
                                for (int i = 0; i < tmpReplUrls1.Length; i++) //перебираем домены, которые меняем
                                {
                                    b = tmpReplUrls1[i].Trim();
                                    if (b.Length > 1)
                                    {
                                        if (domainLevelsUser.Length == 2)
                                        {
                                            sDomainLevelsUserEnd = string.Join(".", domainLevelsUser, domainLevelsUser.Length - 2, 2);
                                            if (sDomainLevelsUserEnd == b)
                                            {
                                                substringMonthSummary1[l] = tmpReplUrl0;
                                                i = tmpReplUrls1.Length; iT = arrayReplacingIni.Length;
                                            }
                                        }
                                        if (domainLevelsUser.Length == 3)
                                        {
                                            sDomainLevelsUserEnd = string.Join(".", domainLevelsUser, domainLevelsUser.Length - 2, 2);
                                            if (sDomainLevelsUserEnd == b)
                                            {
                                                substringMonthSummary1[l] = domainLevelsUser[0] + "." + tmpReplUrl0;
                                                i = tmpReplUrls1.Length; iT = arrayReplacingIni.Length;
                                            }
                                            else
                                            {
                                                sDomainLevelsUserEnd = string.Join(".", domainLevelsUser, domainLevelsUser.Length - 3, 3);
                                                if (sDomainLevelsUserEnd == b)
                                                {
                                                    substringMonthSummary1[l] = tmpReplUrl0;
                                                    i = tmpReplUrls1.Length; iT = arrayReplacingIni.Length;
                                                }
                                            }
                                        }
                                        if (domainLevelsUser.Length == 4)
                                        {
                                            sDomainLevelsUserEnd = string.Join(".", domainLevelsUser, domainLevelsUser.Length - 2, 2);
                                            if (sDomainLevelsUserEnd == b)
                                            {
                                                substringMonthSummary1[l] = string.Join(".", domainLevelsUser, 0, 2) + "." + tmpReplUrl0;
                                                i = tmpReplUrls1.Length; iT = arrayReplacingIni.Length;
                                            }
                                            else
                                            {
                                                sDomainLevelsUserEnd = string.Join(".", domainLevelsUser, domainLevelsUser.Length - 3, 3);
                                                if (sDomainLevelsUserEnd == b)
                                                {
                                                    substringMonthSummary1[l] = domainLevelsUser[0] + "." + tmpReplUrl0;
                                                    i = tmpReplUrls1.Length; iT = arrayReplacingIni.Length;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //Simplifier
                    domainLevelsUser = (substringMonthSummary1[l]).Split('.');

                    foreach (string sReplacement in arraySimplifyingIni)
                    {
                        a = sReplacement;
                        b = string.Join(".", domainLevelsUser, domainLevelsUser.Length - 2, 2);
                        if (a == b) { substringMonthSummary1[l] = a; }
                        else if (domainLevelsUser.Length > 2)
                        {
                            b = string.Join(".", domainLevelsUser, domainLevelsUser.Length - 3, 3);
                            if (a == b) { substringMonthSummary1[l] = a; }
                        }
                    }

                    //SimplifyEnd
                    foreach (string sReplacement in arraySimplifying2Ini)
                    {
                        a = sReplacement;
                        tmpReplUrl0 = substringMonthSummary1[l];

                        if (tmpReplUrl0.Contains(a))
                        { substringMonthSummary1[l] = a; }
                    }
                }
            }
            sDomainLevelsUserEnd = ""; domainLevelsUser = new string[] { "" }; tmpReplUrls1 = new string[] { "" };
        }

        private void _ReadArrayAndSetUrlStatus() //Read "substringDirectionIniFull" и выставление категорий для каждого направления
        {
            string b = "", s = "", a = "";
            for (int h = 0; h < substringDirectionIniFull.Length; h++)
            {
                if (substringDirectionIniFull[h] != null && substringDirectionIniFull[h].Length > 2)
                {
                    s = substringDirectionIniFull[h].ToString().Trim();
                    substringDirectionIni = Regex.Split(s, "=| ");
                    direction = substringDirectionIni[0].Trim();
                    for (int j = 3; j < substringDirectionIni.Length; j++)
                    {
                        b = substringDirectionIni[j];
                        for (int l = 0; l < 5999; l++)
                        {
                            a = substringMonthSummary1[l];
                            if (a.Contains(b))
                            { substringMonthSummary4[l] = direction; }
                        }
                    }
                }
            }
            b = ""; s = ""; a = "";
        }

        private void _UserInfoFromFileIntoCombobox(string myTempFile, int myAddrs) //Read Month and year from Temporary Files (!htupmxxx.tmp) and write it into the Combobox
        {
            var Coder = Encoding.GetEncoding(1251);
            string s = null; string[] uTemporary2;
            string uMonth = null, uYear = null;
            string mystatus1 = "0", mystatus2 = "0";
            using (StreamReader Reader = new StreamReader(myTempFile, Coder))
            {
                while ((s = Reader.ReadLine()) != null)
                {
                    if (s.Contains("<!Selected_User:"))
                    {
                        mystatus1 = "1";
                        _uUser = Regex.Split(s.Replace("  ", " "), " |>")[1];
                    }

                    if (s.Contains("<!Selected_Period:"))
                    {
                        mystatus2 = "1";
                        uTemporary2 = Regex.Split(s.Replace("  ", " "), " |>");
                        uMonth = uTemporary2[2];
                        uYear = uTemporary2[1];
                    }

                    if (mystatus1 == "1" && mystatus2 == "1")
                    {
                        comboMonth.Items.Add(Convert.ToInt32(uYear) + " " + uMonth); //Prepare Combobox Analyse
                        mystatus1 = "0";
                        mystatus2 = "0";
                    }
                }
            }
            comboMonth.SelectedIndex = 0; uTemporary2 = null; s = null;
        }

        private void _UserInfoFromFileIntoArrays(string myTempFile, int myAddrs) //Read user's info from Temporary Files (!htupmxxx.tmp) and write it into the arrays "substringMonthSummary 5-7"
        {
            var Coder = Encoding.GetEncoding(1251);
            string s = null;
            string uMonth = null, uYear = null, wo = ""; string[] uTemporary2;
            string mystatus1 = "0", mystatus2 = "0";
            using (StreamReader Reader = new StreamReader(myTempFile, Coder))
            {
                while ((s = Reader.ReadLine()) != null)
                {
                    if (s.Contains("<!Selected_User:"))
                    {
                        mystatus1 = "1";
                        _uUser = Regex.Split(s.Replace("  ", " "), " |>")[1];
                    }

                    if (s.Contains("<!Selected_Period:"))
                    {
                        mystatus2 = "1";
                        uTemporary2 = Regex.Split(s.Replace("  ", " "), " |>");
                        uMonth = uTemporary2[2];
                        uYear = uTemporary2[1];
                    }
                    if (mystatus1 == "1" && mystatus2 == "1")
                    {
                        for (int w = myAddrs; w < (myAddrs + 250); w++)
                        {
                            wo = substringMonthSummary1[w].ToString();
                            if (wo.Length > 2)
                            {
                                substringMonthSummary5[w] = uMonth;
                                substringMonthSummary6[w] = Convert.ToInt32(uYear);
                            }
                        }
                        _toolTipLabel2 += uMonth + " " + uYear;
                        mystatus1 = "0";
                        mystatus2 = "0";
                    }
                }
            }
            StatusLabel2.Visible = true;
            StatusLabel2.Text = _toolTipLabel2 + "  по логину - " + _uUser;
        }

        private void _CheckTemporaryHTML() //Check files exist of HTML Temporary Files
        {
            string _tmpExist = "0", k = null, s = null;
            string t = "ProxyAnalyser\\!htupm";
            for (int i = 1; i < 13; i++)
            {
                k = t + i + ".tmp";
                if (File.Exists(k))
                {
                    _tmpExist = "1";
                    var Coder = Encoding.GetEncoding(65001);
                    s = null;
                    try
                    {
                        using (StreamReader Reader = new StreamReader(k, Coder))
                        {
                            while ((s = Reader.ReadLine()) != null)
                            {
                                if (s.Contains("<!Selected_User:"))
                                {
                                    _uUser = Regex.Split(s.Replace("  ", " "), " |>")[1];
                                    break;
                                }
                            }
                        }
                    }
                    catch { }
                }
                k = t + i + "_.tmp";
                if (File.Exists(k))
                {
                    _tmpExist = "1";
                    var Coder = Encoding.GetEncoding(65001);
                    s = null;
                    try
                    {
                        using (StreamReader Reader = new StreamReader(k, Coder))
                        {
                            while ((s = Reader.ReadLine()) != null)
                            {
                                if (s.Contains("<!Selected_User:"))
                                {
                                    _uUser = Regex.Split(s.Replace("  ", " "), " |>")[1];
                                    break;
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            if (_tmpExist == "1")
            {
                preparedDataToolStripMenuItem.Enabled = true;
                //http://metanit.com/sharp/windowsforms/4.19.php
                if (!bLoadURI)
                {
                    DialogResult result = MessageBox.Show
                    ("Обнаружены закачанные ранее временные файлы по пользователю  \"" + _uUser + "\" \nДля их обработки сейчас нажмите кнопку \"ДА\". Или нажав кнопку \"Нет\" можете обработать их позже, после загрузки данной программы, выбрав в меню пункт: " +
                    "Функции\\Обработать данные.",
                    "Внимание!",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button2
                    );
                    if (result == DialogResult.Yes)
                    { _MakeDone(); }
                }
                else
                { _MakeDone(); }
                StatusLabel3.Enabled = true;
                StatusLabel3.Visible = true;
                StatusLabel3.Text = "Данные по " + _uUser + "  | ";
            }
            if (_tmpExist == "0")
            {
                MessageBox.Show
                    ("Предварительно необходимо загрузить данные по собранной статистике прокси по выбранному пользователю. Для их загрузки в программу, выберите в меню пункт: " +
                    "Функции\\Загрузить статистику.",
                    "Внимание!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1
                    );
            }
        }

        private void _ParsingHtmlToCSV(string myInFile, long myStartAddrs) //Parsing the downloaded information from a file
        {
            HtmlDocument HD = new HtmlDocument();
            HtmlNodeCollection NoAltElements;
            HD.LoadHtml(File.ReadAllText(myInFile).ToString());
            string _success = " успешно!";
            int i = 0;
            try  //Parsing URLs
            {
                NoAltElements = HD.DocumentNode.SelectNodes("//td[@class='data2'][1]");
                if (NoAltElements != null)      // проверка на наличие найденных узлов
                {
                    i = 0;
                    foreach (HtmlNode HN in NoAltElements)
                    {
                        substringMonthSummary1[(i + myStartAddrs)] = HN.InnerText.Trim().ToLower();
                        if (i + myStartAddrs < 249 + myStartAddrs)
                        { i++; }
                    }
                }
                NoAltElements = null;
            }
            catch { _success = " с ошибками!"; }

            try  //Parsing amount  Downloaded Bytes into MB
            {
                NoAltElements = HD.DocumentNode.SelectNodes("//td[@class='data'][2]");
                if (NoAltElements != null)      // проверка на наличие найденных узлов
                {
                    i = 0;
                    foreach (HtmlNode HN in NoAltElements)
                    {
                        string a = HN.InnerText.Replace(".", ",").Trim().ToLower();
                        string a1 = "0";

                        if (a.Contains('k'))
                        { a1 = "k"; }

                        if (a.Contains('g'))
                        { a1 = "g"; }

                        if (a.Contains('m'))
                        { a1 = "m"; }

                        switch (a1)
                        {
                            case ("k"):
                                a = a.Replace("k", "").Trim();
                                break;
                            case ("m"):
                                a = a.Replace("m", "").Trim();
                                a = (1024 * Convert.ToDouble(a)).ToString();
                                break;
                            case ("g"):
                                a = a.Replace("g", "").Trim();
                                a = (1024 * 1024 * Convert.ToDouble(a)).ToString();
                                break;
                            default:
                                a = "0,001";
                                break;
                        }

                        substringMonthSummary2[(i + myStartAddrs)] = Math.Round((Convert.ToDouble(a) / 1024), 2);//MB, округление результата до 3-х знаков после запятой
                        if (i + myStartAddrs < 249 + myStartAddrs)
                        { i++; }
                    }
                }
                NoAltElements = null;
            }
            catch { _success = " с ошибками!"; }

            try  //Parsing Elapsed Time into minutes
            {
                NoAltElements = HD.DocumentNode.SelectNodes("//td[@class='data'][6]");
                if (NoAltElements != null)      // проверка на наличие найденных узлов
                {
                    i = 0;
                    foreach (HtmlNode HN in NoAltElements)
                    {
                        string[] myTime = Regex.Split(HN.InnerText, ":");
                        double b = Math.Round((60 * 60 * Convert.ToDouble(myTime[0]) + 60 * Convert.ToDouble(myTime[1]) + Convert.ToDouble(myTime[2])) / 60, 3);

                        substringMonthSummary3[(i + myStartAddrs)] = b;
                        if (i + myStartAddrs < 249 + myStartAddrs)
                        { i++; }
                    }
                }
            }
            catch { _success = " с ошибками!"; }
            HD = null;
            NoAltElements = null;
            StatusLabel2.Text = myInFile + " " + _success;
        }

        private void _ParsingHtmlTotalAmount(string myInFile, int myStartAddrs) //Parsing the Amount downloaded from a temporary file in the array "_substringMonthAndBytes"
        {
            HtmlDocument HD = new HtmlDocument();
            HD.LoadHtml(File.ReadAllText(myInFile).ToString());
            HtmlNodeCollection NoAltElements;
            string a, a1;
            double result;
            try  //Parsing Elapsed Time into minutes
            {
                NoAltElements = HD.DocumentNode.SelectNodes("//tfoot/tr/th[@class='header_r'][1]");
                if (NoAltElements != null)      // проверка на наличие найденных узлов
                {
                    foreach (HtmlNode HN in NoAltElements)
                    {
                        a = HN.InnerText.Replace(".", ",").Trim();
                        if (a.ToLower().Contains('k'))
                        { a1 = "k"; }
                        else if (a.ToLower().Contains('g'))
                        { a1 = "g"; }
                        else if (a.ToLower().Contains('m'))
                        { a1 = "m"; }
                        else
                        { a1 = ""; }

                        switch (a1)
                        {
                            case "k":
                                result = Convert.ToDouble(a.ToLower().Remove('k')) / 1024 / 1024;
                                break;
                            case "m":
                                result = Convert.ToDouble(a.ToLower().Remove('m')) / 1024;
                                break;
                            case "g":
                                result = Convert.ToDouble(a.ToLower().Remove('g'));
                                break;
                            default:
                                result = 0.0001;
                                break;
                        }

                        MessageBox.Show(a + "\n" + result);

                        _substringMonthAndBytes[myStartAddrs] += Math.Round(result, 2); //Результат в ГБ
                        break;
                    }
                }
            }
            catch (Exception expt)
            {
                MessageBox.Show(expt.ToString());
            }
            NoAltElements = null;
            HD = null;
        }

        private void _buttonExit_Click(object sender, EventArgs e) //Меню Exit
        { System.Windows.Forms.Application.Exit(); }

        private void reloadIniToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProgressBar1.Value = 0;
            _ProgressWork10();
            ReadIniTolist();
            comboDirection.Items.Clear();
            _initoArrayDirection();
            _initoArraysClearerAndSiplifierUrl();
            ProgressBar1.Value = 100;
            _MakeDone();
        }

        private void curentFolderToolStripMenuItem_Click(object sender, EventArgs e) //Меню "Current Folder"
        { System.Diagnostics.Process.Start("explorer", Environment.CurrentDirectory); }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)//Меню "О программе"
        {
            DialogResult result = MessageBox.Show(
                myFileVersionInfo.Comments + "\n\nВерсия: " + myFileVersionInfo.FileVersion + "\nBuild: " +
                strVersion + "\n" + myFileVersionInfo.LegalCopyright + "\n\nПрограмма предназначена\nдля обработки статистики интернет-трафика\nпользователей корпоративного прокси-сервера SARG версии 2.3.9\n" +
                "\nOriginal file: " + myFileVersionInfo.OriginalFilename + "\nFull path: " + Application.ExecutablePath,
                "Информация о программе",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            myFileVersionInfo = null; strVersion = null;
        }

        private void comboDirection_SelectedIndexChanged(object sender, EventArgs e) //Комбобокс Направлений сбор данных за месяц из общих данных по выбранным данным в комбобох в массивы
        {
            string s = comboDirection.SelectedItem.ToString();

            for (int h = 0; h < 99; h++)
            {
                try
                {
                    if (_Directions[h] == s && _Directions[h].Length > 0)
                    { labelDescription.Text = _DirectionsDiscription[h]; }
                }
                catch { }
            }
        }

        private void comboMonth_SelectedIndexChanged(object sender, EventArgs e) //Комбобокс месяцев
        {
            _InfoStaticsMonth();
            tabControl1.SelectTab(tabPage5);
        }

        private void _InfoStaticsFull() //Build a Table статистики за весь период
        {
            List<_StatisticsFull> _items = new List<_StatisticsFull>();

            for (int h = 0; h < 5999; h++)
            {
                try
                {
                    string b = substringMonthSummary1[h];
                    if (b.Length > 2 && substringMonthSummary2[h] > 0)
                    {
                        _items.Add(new _StatisticsFull
                        {
                            _Url = substringMonthSummary1[h],
                            _Bytes = substringMonthSummary2[h],
                            _Time = substringMonthSummary3[h],
                            _Direction = substringMonthSummary4[h],
                            _Month = substringMonthSummary5[h],
                            _Year = substringMonthSummary6[h],
                            _User = _uUser
                        });
                    }
                }
                catch { }
            }

            DataTable _myFullStatistics = new DataTable("FullStatistics");
            DataColumn[] cols ={
                                  new DataColumn("iD",typeof(Int32)),
                                  new DataColumn("URL",typeof(String)),
                                  new DataColumn("Скачано ГБ",typeof(double)),
                                  new DataColumn("Затрачено часов",typeof(double)),
                                  new DataColumn("Категория",typeof(string)),
                                  new DataColumn("Месяц",typeof(string)),
                                  new DataColumn("Год",typeof(Int32)),
                                  new DataColumn("Логин",typeof(string))
                              };

            _myFullStatistics.Columns.AddRange(cols);
            _myFullStatistics.PrimaryKey = new DataColumn[] { _myFullStatistics.Columns["iD"] };

            try { dataGridView1.Rows.Clear(); } catch { }

            for (int i = 0; i < 5999; i++)
            {
                try
                {
                    string a = substringMonthSummary1[i].ToString();
                    if (a.Length > 2 && substringMonthSummary2[i] > 0)
                    {
                        DataRow row = _myFullStatistics.NewRow();
                        row["iD"] = i;
                        row["URL"] = substringMonthSummary1[i];
                        row["Скачано ГБ"] = Math.Round(substringMonthSummary2[i] / 1024, 2);
                        row["Затрачено часов"] = Math.Round(substringMonthSummary3[i] / 60, 2);
                        row["Категория"] = substringMonthSummary4[i];
                        row["Месяц"] = substringMonthSummary5[i];
                        row["Год"] = substringMonthSummary6[i];
                        row["Логин"] = _uUser;
                        _myFullStatistics.Rows.Add(row);
                    }
                }
                catch { }
            }
            dataGridView1.DataSource = _myFullStatistics;
            dataGridView1.AutoResizeColumns();
            dataGridView1.Columns[2].SortMode = DataGridViewColumnSortMode.Programmatic;
            dataGridView1.Sort(dataGridView1.Columns[2], ListSortDirection.Descending);

            //Select 1 - Direction contains "video"
            var _myDataURL = _items.Where(u => u._Direction.Contains("video"));

            //Select 2    - Downloaded GB by an every direction
            textBox1.AppendText("\n\n");
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.AppendLine("Отчет по " + _uUser + " за выбранные месяцы:");

            for (int i = 0; i < _Directions.Length; i++)
            {
                double _by3 = 0, _bu3 = 0;
                try
                {
                    if (_Directions[i].ToString().Length > 1)
                    {
                        var _myDataURL1 = _items.Where(u => u._Direction.Contains(_Directions[i].ToString()));
                        double _by2 = 0, _bu2 = 0;
                        foreach (var n in _myDataURL1)
                        {
                            _by2 = _by2 + n._Bytes;
                            _bu2 = _bu2 + n._Time;
                        };
                        _by3 = Math.Round(_by2 / 1024, 2);
                        _bu3 = Math.Round(_bu2 / 60, 2);


                        sb.AppendLine((_Directions[i].ToString()) + " " + _by3 + "  GB " + "\n" + "\n");
                    }
                    _DirectionsByte[i] = _by3;
                    _DirectionsTime[i] = _bu3;
                }
                catch { }
            }

            textBox1.AppendText(sb.ToString());
            textBox1.AppendText("\n\n");
        }

        private void _InfoStaticsDirection() //Build a Table and a Chart by summarize data  full time
        {
            List<_StatisticsDirection> _items = new List<_StatisticsDirection>();
            for (int h = 0; h < 99; h++)
            {
                try
                {
                    if (_DirectionsByte[h] > 0.1)
                    {
                        _items.Add(new _StatisticsDirection
                        {
                            _Direction = _Directions[h].ToString(),
                            _Discription = _DirectionsDiscription[h].ToString(),
                            _DirectionBytes = _DirectionsByte[h]
                        });
                    }
                }
                catch { }
            }

            DataTable _myStatistics = new DataTable("StatisticsDirection");
            DataColumn[] cols ={
                                  new DataColumn("Категория",typeof(string)),
                                  new DataColumn("Описание",typeof(string)),
                                  new DataColumn("Скачано ГБ",typeof(double))
                              };

            _myStatistics.Columns.AddRange(cols);
            _myStatistics.PrimaryKey = new DataColumn[] { _myStatistics.Columns["iD"] };

            for (int i = 0; i < 99; i++)
            {
                try
                {
                    string a = _Directions[i].ToString();
                    if (a.Length > 1 && _DirectionsByte[i] > 0)
                    {
                        DataRow row = _myStatistics.NewRow();
                        row["Категория"] = _Directions[i];
                        row["Описание"] = _DirectionsDiscription[i];
                        row["Скачано ГБ"] = Math.Round(_DirectionsByte[i], 1);
                        _myStatistics.Rows.Add(row);
                    }
                }
                catch { }
            }

            try { dataGridView2.Rows.Clear(); } catch { }
            try { chart1.Series[0].Points.Clear(); } catch { }

            dataGridView2.DataSource = _myStatistics;
            dataGridView2.AutoResizeColumns();
            dataGridView2.Columns[2].SortMode = DataGridViewColumnSortMode.Programmatic;
            dataGridView2.Sort(dataGridView2.Columns[2], ListSortDirection.Descending); //Сортировка в таблице по возрастанию 

            chart1.DataSource = dataGridView2;
            for (int d = 0; d < 99; d++)
            {
                try
                {
                    if (_Directions[d].ToString().Length > 1 && (_DirectionsByte[d] > 1 || _DirectionsTime[d] > 1))
                    { chart1.Series["Скачано ГБ"].Points.AddXY(_Directions[d], _DirectionsByte[d]); }
                }
                catch { }
            }
            chart1.Series[0].ChartArea = "ChartArea1";
            chart1.Series[0].ToolTip = _uUser + " | Категория = #VALX, Скачано = #VALY ГБ ";

            chart1.Titles[0].Text = "По категориям за весь период";
            chart1.Titles[0].Font = new Font("Arial", 9, FontStyle.Bold);
            chart1.DataManipulator.Sort(PointSortOrder.Descending, "Скачано ГБ");  //Сортировка в графике по возрастанию 
            chart1.DataBind();
            try
            {
                dataGridView2.ReadOnly = true;
                chart1.SaveImage(Application.StartupPath + "\\ProxyAnalyser\\chart1.png", ChartImageFormat.Png);
            }
            catch { }
            dataGridView2.ReadOnly = false;
            chart1.Visible = true;

            exportToExcelSummarizeToolStripMenuItem.Enabled = true;
        }

        private void _InfoStaticsMonth() //Build a Table and a Chart by the selected month
        {
            List<_StatisticsDirection> _items = new List<_StatisticsDirection>();

            string[] submon = Regex.Split(comboMonth.SelectedItem.ToString(), " ");
            int _selYear = Convert.ToInt32(submon[0]);
            string _selMonth = submon[1].ToString().Trim();

            for (int h = 0; h < 99; h++) //сбор данных за месяц из общих данных по выбранным данным в комбобох в массивы
            {
                try
                {
                    if (_Directions[h]?.Length > 1)
                    {
                        double l1 = 0, l2 = 0;
                        for (int k = 0; k < 5999; k++)
                        {
                            try
                            {
                                if (
                                    _Directions[h] == substringMonthSummary4[k] &&
                                    substringMonthSummary6[k] == _selYear &&
                                    substringMonthSummary5[k] == _selMonth &&
                                    substringMonthSummary2[k] > 0
                                    )
                                {
                                    l1 = l1 + substringMonthSummary2[k];
                                    l2 = l2 + substringMonthSummary3[k];
                                }

                            }
                            catch { }
                        }
                        _DirectionsMonthByte[h] = Math.Round(l1, 2);
                        _DirectionsMonthTime[h] = Math.Round(l2 / 60, 0);
                    }
                }
                catch { }
            }

            for (int h = 0; h < 99; h++) // формирование общей таблицы из массивов
            {
                try
                {
                    if (_DirectionsMonthByte[h] > 0.1)
                    {
                        _items.Add(new _StatisticsDirection
                        {
                            _Direction = _Directions[h].ToString(),
                            _Discription = _DirectionsDiscription[h].ToString(),
                            _DirectionBytes = _DirectionsMonthByte[h]
                        });
                    }
                }
                catch { }
            }

            DataTable _myStatistics = new DataTable("StatisticsDirectionMonth");
            DataColumn[] cols ={
                                  new DataColumn("Категория",typeof(string)),
                                  new DataColumn("Описание",typeof(string)),
                                  new DataColumn("Скачано МБ",typeof(double))
                              };

            _myStatistics.Columns.AddRange(cols);

            DataView view = new DataView(_myStatistics);
            view.Sort = "Скачано МБ DESC";

            for (int i = 0; i < 99; i++) //сбор данных в таблицу
            {
                try
                {
                    string a = _Directions[i];
                    if (a?.Length > 1 && _DirectionsMonthByte[i] > 0)
                    {
                        DataRow row = _myStatistics.NewRow();
                        row["Категория"] = _Directions[i];
                        row["Описание"] = _DirectionsDiscription[i];
                        row["Скачано МБ"] = Math.Round(_DirectionsMonthByte[i], 1);
                        _myStatistics.Rows.Add(row);
                    }
                }
                catch { }
            }

            dataGridView3.ReadOnly = false;
            try { dataGridView3.Rows.Clear(); } catch { }
            try { chart3.Series[0].Points.Clear(); } catch { }

            dataGridView3.DataSource = _myStatistics;
            dataGridView3.AutoResizeColumns();
            dataGridView3.Columns[2].SortMode = DataGridViewColumnSortMode.Programmatic;
            dataGridView3.Sort(dataGridView3.Columns[2], ListSortDirection.Descending);


            chart3.DataSource = dataGridView3;
            for (int d = 0; d < 99; d++)
            {
                try
                {
                    if (_Directions[d].ToString().Length > 1 && (_DirectionsMonthByte[d] > 50 || _DirectionsMonthTime[d] > 10))
                    {
                        chart3.Series[0].Points.AddXY(_Directions[d], _DirectionsMonthByte[d]);
                    }
                }
                catch { }
            }

            chart3.Series[0].ChartArea = "ChartArea1";
            chart3.Series[0].ToolTip = _uUser + " | Категория = #VALX, Скачано = #VALY МБ ";
            chart3.DataManipulator.Sort(PointSortOrder.Descending, "Скачано МБ");
            chart3.Titles[0].Font = new Font("Arial", 9, FontStyle.Bold);
            chart3.DataBind();
            try
            {
                dataGridView3.ReadOnly = true;
                chart3.SaveImage(Application.StartupPath + "\\ProxyAnalyser\\chart3.png", ChartImageFormat.Png);
            }
            catch { }
            dataGridView3.ReadOnly = false;
            chart3.Visible = true;

            DataTable _myFullStatistics = new DataTable("StatisticsDirectionMonthFull");
            DataColumn[] colsFull ={
                                  new DataColumn("URL",typeof(string)),
                                  new DataColumn("Категория",typeof(string)),
                                  new DataColumn("Скачано МБ",typeof(double))
                              };

            _myFullStatistics.Columns.AddRange(colsFull);
            _myFullStatistics.PrimaryKey = new DataColumn[] { _myFullStatistics.Columns["iD"] };

            view = new DataView(_myStatistics);
            view.Sort = "Скачано МБ DESC";

            for (int h = 0; h < 5999; h++) // формирование таблицы из массивов
            {
                try
                {
                    if (substringMonthSummary1[h].Length > 1 &&
                        substringMonthSummary2[h] > 1 &&
                        substringMonthSummary5[h] == _selMonth &&
                        substringMonthSummary6[h] == _selYear
                        )
                    {
                        DataRow row = _myFullStatistics.NewRow();
                        row["URL"] = substringMonthSummary1[h];
                        row["Категория"] = substringMonthSummary4[h];
                        row["Скачано МБ"] = Math.Round(substringMonthSummary2[h], 1);
                        _myFullStatistics.Rows.Add(row);
                    }
                }
                catch { }
            }

            int U = 0, U2 = 24;
            for (int k = 499; k < 5999; k++)
            {
                try
                {
                    if (
                        substringMonthSummary6[k] == _selYear &&
                        substringMonthSummary5[k] == _selMonth &&
                        k < 6000
                        )
                    {
                        U = k / 250;
                        U2 = 24 + (k / 500);
                        k = 6000;
                    }
                }
                catch { }
            }

            int U1 = U + 1;

            DataTable _myUStatistics = new DataTable("StatisticsURLEveryMonth");
            DataColumn[] colsU ={
                                  new DataColumn("URL",typeof(string)),
                                  new DataColumn("Скачано МБ",typeof(double)),
                                  new DataColumn("Категория",typeof(string)),
                                  new DataColumn("Месяц",typeof(string)),
                                  new DataColumn("Год",typeof(Int32)),
                              };

            _myUStatistics.Columns.AddRange(colsU);
            _myUStatistics.PrimaryKey = new DataColumn[] { _myUStatistics.Columns["iD"] };

            for (int h = 0; h < 99; h++) // формирование таблицы из массивов
            {
                try
                {
                    if (_BytesEveryYearMonthByURL[h, U].Trim().Length > 2 && Math.Round(Convert.ToDouble(_BytesEveryYearMonthByURL[h, U1]), 1) > 0)
                    {
                        DataRow row = _myUStatistics.NewRow();
                        row["URL"] = _BytesEveryYearMonthByURL[h, U];
                        row["Скачано МБ"] = Math.Round(Convert.ToDouble(_BytesEveryYearMonthByURL[h, U1]), 1);
                        row["Категория"] = _BytesEveryYearMonthByURL[h, U2];
                        row["Месяц"] = _selMonth;
                        row["Год"] = _selYear;
                        _myUStatistics.Rows.Add(row);
                    }
                }
                catch { }
            }
            try { dataGridView6.Rows.Clear(); } catch { }

            dataGridView6.DataSource = _myUStatistics;
            dataGridView6.AutoResizeColumns();
            dataGridView6.Columns[1].SortMode = DataGridViewColumnSortMode.Programmatic;
            dataGridView6.Sort(dataGridView6.Columns[1], ListSortDirection.Descending);
            labelTOP100.Font = new Font("Arial", 9, FontStyle.Bold);
            labelTOP100.Text = "TOP сайтов";
        }

        private void _InfoStaticsTotalMonth() //Amount Downloaded. Build the Chart of the changing amount downloaded from a month to another month
        {
            try { chart4.Series[0].Points.Clear(); } catch { }
            try { chart4.Series[1].Points.Clear(); } catch { }

            for (int d = 11; d > -1; d--)
            {
                string _data = null;
                int f = 0;
                f = d * 500;
                _data = substringMonthSummary6[f].ToString() + " " + substringMonthSummary5[f];

                try
                {
                    if (_substringMonthAndBytes[d] > 0)
                    {
                        chart4.Series[0].Points.AddXY(_data, _substringMonthAndBytes[d]);
                        chart4.Series[1].Points.AddXY(_data, _substringMonthAndBytes[d] - _substringMonthAndBytes[d + 1]);
                    }
                }
                catch { }
            }
            chart4.Series[0].ChartArea = "ChartArea1";
            chart4.Series[0].Color = Color.Green;
            chart4.Series[1].ChartArea = "ChartArea2";
            chart4.Series[1].Color = Color.DarkOliveGreen;
            chart4.Series[0].ToolTip = "Период = #VALX, скачано = #VALY ГБ ";


            //           chart4.Titles[1].Text = "Изменение объема интернет-трафика";
            //           chart4.Titles[1].Font = new Font("Arial", 9, FontStyle.Bold);

            //            chart4.Series[0].Label = "Объем интернет-трафика помесячно";
            //            chart4.Series[1].Label = "Изменение объема интернет-трафика";
            //chart4.Legends["Legend2"].CellColumns.Add(new LegendCellColumn("Name", LegendCellColumnType.Text, "#LEGENDTEXT"));
            // chart4.Legends["Legend2"].CellColumns.Add(new LegendCellColumn("Sym", LegendCellColumnType.SeriesSymbol, ""));
            //  chart4.Legends["Legend2"].CellColumns.Add(new LegendCellColumn("Avg", LegendCellColumnType.Text, "#AVG{N2}"));

            chart4.Titles[0].Font = new Font("Arial", 9, FontStyle.Bold);

            //            _labelVolumeChange.Text = "Изменение объема интернет-трафика";
            //            _labelVolumeChange.Font = new Font("Arial", 9, FontStyle.Bold);
            //            _labelVolumeTotal.Text = "Объем интернет-трафика помесячно";
            //            chart4.Series[0].Label = "#PERCENT";
            //            chart4.Series[0].LegendText = "#VALX";
            /*System.XML.XmlTextWriter myWriter = new System.XML.XmlTextWriter("c:\\MyPersistedData.xml", System.Text.Encoding.ASCII);
            Chart1.Serializer.Save(myWriter);

            // We initialize the XML reader with data from a file.
            System.XML.XmlTextReader myXMLReader = new System.XML.XmlTextReader("c:\\MyPersistedData.xml");
            Chart1.Serializer.Load(myXMLReader);*/

            chart4.DataBind();
            chart4.Visible = false;
            try
            { chart4.SaveImage(Application.StartupPath + "\\ProxyAnalyser\\chart4.png", ChartImageFormat.Png); }
            catch { }
            chart4.Visible = true;
        }

        private void _URLTOP100Total() //prepare array and make the Table URL/Direction/Bytes/Time "_BytesTopUrlTotal[i, j]"
        {
            for (int i = 0; i < 4; i++) //clear the table
            {
                for (int j = 0; j < 100; j++)
                { _BytesTopUrlTotal[j, i] = "0"; }
            }

            for (int k = 0; k < 5999; k++)
            {
                for (int j = 0; j < 99; j++)
                {
                    try
                    {
                        if (substringMonthSummary1[k].Length > 2)
                        {
                            if (_BytesTopUrlTotal[0, 0].Length < 3 && substringMonthSummary1[k].Length > 2)
                            {
                                _BytesTopUrlTotal[0, 0] = substringMonthSummary1[k];
                                _BytesTopUrlTotal[0, 1] = substringMonthSummary4[k];
                                _BytesTopUrlTotal[0, 2] = substringMonthSummary2[k].ToString();
                                _BytesTopUrlTotal[0, 3] = substringMonthSummary3[k].ToString();
                                j = 100;
                            }

                            if ((j < 100) && (_BytesTopUrlTotal[j, 0] == substringMonthSummary1[k]) && substringMonthSummary1[k].Length > 2)
                            {
                                double o1 = Convert.ToDouble(_BytesTopUrlTotal[j, 2]);
                                _BytesTopUrlTotal[j, 2] = (o1 + substringMonthSummary2[k]).ToString();

                                double o2 = Convert.ToDouble(_BytesTopUrlTotal[j, 3]);
                                _BytesTopUrlTotal[j, 3] = (o2 + substringMonthSummary3[k]).ToString();
                                j = 100;
                            }

                            if ((j < 100) && (_BytesTopUrlTotal[j, 0].Length < 3) && substringMonthSummary1[k].Length > 2)
                            {
                                _BytesTopUrlTotal[j, 0] = substringMonthSummary1[k];
                                _BytesTopUrlTotal[j, 1] = substringMonthSummary4[k];
                                _BytesTopUrlTotal[j, 2] = substringMonthSummary2[k].ToString();
                                _BytesTopUrlTotal[j, 3] = substringMonthSummary3[k].ToString();
                                j = 100;
                            }
                        }
                    }
                    catch { }
                }
            }

            try { dataGridView4.Rows.Clear(); } catch { }

            DataTable _myUTStatistics = new DataTable("StatisticsURLEveryMonth");
            DataColumn[] colsUT ={
                                  new DataColumn("URL",typeof(string)),
                                  new DataColumn("Категория",typeof(string)),
                                  new DataColumn("Скачано ГБ",typeof(double)),
                                  new DataColumn("Затрачено часов",typeof(double)),
                              };

            _myUTStatistics.Columns.AddRange(colsUT);

            for (int h = 0; h < 99; h++) // формирование таблицы из массивов
            {
                try
                {
                    if (_BytesTopUrlTotal[h, 0].Length > 2 && Math.Round(Convert.ToDouble(_BytesTopUrlTotal[h, 2]) / 256, 0) > 0)
                    {
                        DataRow row = _myUTStatistics.NewRow();
                        row["URL"] = _BytesTopUrlTotal[h, 0];
                        row["Категория"] = _BytesTopUrlTotal[h, 1];
                        row["Скачано ГБ"] = Math.Round(Convert.ToDouble(_BytesTopUrlTotal[h, 2]) / 1024, 1);
                        row["Затрачено часов"] = Math.Round(Convert.ToDouble(_BytesTopUrlTotal[h, 3]) / 60, 0);
                        _myUTStatistics.Rows.Add(row);
                    }
                }
                catch { }
            }
            dataGridView4.DataSource = _myUTStatistics;
            dataGridView4.AutoResizeColumns();

            dataGridView4.Columns[2].SortMode = DataGridViewColumnSortMode.Programmatic;
            dataGridView4.Sort(dataGridView4.Columns[2], ListSortDirection.Descending);
        }

        //http://forum.sources.ru/index.php?showtopic=214813
        //http://blog.kislenko.net/show.php?id=1103&s=0
        //https://msdn.microsoft.com/ru-ru/library/dd456628.aspx

        private void _InfoStaticsBytesByDirection() //prepare array Bytes-Directions-months "_BytesEveryYearMonthByDirection[i, j]"
        {
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 12; j++)
                { _BytesEveryYearMonthByDirection[i, j] = 0; }
            }
            for (int j = 0; j < 12; j++)
            {
                int l = j * 500;

                for (int k = 0; k < 499; k++)
                {
                    int p = l + k;
                    for (int i = 0; i < 99; i++)
                    {
                        if (_Directions[i] != null && substringMonthSummary4[p] != null && _Directions[i].Equals(substringMonthSummary4[p]))
                        {
                            _BytesEveryYearMonthByDirection[i, j] += substringMonthSummary2[p];
                        }
                    }
                }
            }
        }

        private void _SummDirectionEveryMonth() //prepare array Bytes-Directions-months "_BytesEveryYearMonthByDirection[i, j]"
        {
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 12; j++)
                { _BytesEveryYearMonthByDirection[i, j] = 0; }
            }
            for (int j = 0; j < 12; j++)
            {
                int l = j * 500;

                for (int k = 0; k < 499; k++)
                {
                    int p = l + k;
                    for (int i = 0; i < 99; i++)
                    {
                        if (_Directions[i] == substringMonthSummary4[p])
                        {
                            _BytesEveryYearMonthByDirection[i, j] += substringMonthSummary2[p];
                        }
                    }
                }
            }
        }

        private void _InfoStaticsDirectionTotalMonth() //Amount Downloaded by every month every direction. Build a Table and a Chart by the every month
        {
            Color[] cSeries = { Color.OrangeRed, Color.DeepSkyBlue, Color.Orange, Color.ForestGreen, Color.MediumAquamarine };
            chart5.Visible = false;
            chart5.Enabled = true;
            for (int i = 0; i < 5; i++)
            {
                try { chart5.Series[i].Points.Clear(); } catch { }
                try { chart5.Series[i].Name = ""; } catch { }
                try { chart5.Series[i].LegendText = ""; } catch { }
                try { chart5.Series[i].ToolTip = ""; } catch { }
                try { chart5.Series[i].ChartArea = ""; } catch { }
                try { chart5.Series[i].ChartType = SeriesChartType.StackedColumn; } catch { }

                //                try { chart5.Series[i].IsVisibleInLegend = false; } catch { }
                try { chart5.Series[i].Color = cSeries[i]; } catch { }
            }
            _arrFindMax5(_DirectionsByte);
            _SummDirectionEveryMonth();
            string _data = null;
            int h = 0;
            for (int d = 11; d > -1; d--)
            {
                _data = null;
                int f = 0;
                f = d * 500;
                _data = substringMonthSummary6[f].ToString() + " " + substringMonthSummary5[f];
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        h = _dm[i];
                        double s = Math.Round(_BytesEveryYearMonthByDirection[h, d] / 1024, 1);
                        if (_substringMonthAndBytes[d] > 0 && !substringMonthSummary6[f].ToString().ToLower().Contains("common"))
                        { chart5.Series[i].Points.AddXY(_data, s); }
                    }
                    catch (Exception exp) { MessageBox.Show(exp.Message); }
                }
            }
            //            chart5.Series[0].ChartType = SeriesChartType.Column;
            h = 0;
            bool bExistChart5Data = false;
            for (int i = 0; i < 5; i++)
            {
                h = _dm[i];
                if (_Directions[h] != null && !_Directions[h].ToLower().Contains("common") && _Directions[h].Trim().Length > 2)
                {
                    try { chart5.Series[i].Name = _Directions[h]; } catch { }
                    try { chart5.Series[i].LegendText = _Directions[h]; } catch { }
                    try { chart5.Series[i].ToolTip = "#SERIESNAME : #VALX, скачано #VALY ГБ "; } catch { chart5.Series[i].ToolTip = ""; }
                    // try { chart5.Series[i].LegendText = "#SERIESNAME "; } catch { }
                    try { chart5.Series[i].ChartArea = "ChartArea1"; } catch { }
                    chart5.Series[i].IsVisibleInLegend = true;
                    bExistChart5Data = true;
                }
                else
                {
                    chart5.Series[i].IsVisibleInLegend = false;
                    chart5.Series[i].Name = i.ToString();
                    chart5.Series[i].Color = Color.Transparent;
                }
            }
            //https://msdn.microsoft.com/ru-ru/library/dd456687.aspx
            chart5.Titles[0].Text = "Распределение объема интернет-трафика с учетом категорий";
            chart5.Titles[0].Font = new Font("Arial", 9, FontStyle.Bold);
            chart5.Series[0].Color = Color.Crimson;
            try { chart5.DataBind(); } catch (Exception exp) { MessageBox.Show(exp.ToString()); }

            if (bExistChart5Data)
            {
                chart5.Visible = true;
                try
                {
                    chart5.SaveImage(Application.StartupPath + "\\ProxyAnalyser\\chart5.png", ChartImageFormat.Png);
                }
                catch (Exception exp) { MessageBox.Show(exp.ToString()); }
            }

        }

        private void _arrFindMax5(double[] ArrayBytes) //поиск индексов 5-ти максимальных значений в направлении _dm[j]
        {
            double[] _arrtemp = new double[100];
            for (int i = 0; i < 100; i++)
            { _arrtemp[i] = ArrayBytes[i]; }  //заполняем временный масив значениями с рабочего масива
            Array.Sort(_arrtemp);

            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (ArrayBytes[i] == _arrtemp[_arrtemp.Length - 1 - j])
                    {
                        _dm[j] = i; //Запись в массив индексов максимальных значений
                    }
                }
            }
        }

        private void _InfoStaticsUrlByMonth() //Amount Downloaded by every month TOP100 URL. Build a Table and a Chart by the every month into _BytesEveryYearMonthByURL
        {
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 24; j++)
                { _BytesEveryYearMonthByURL[i, j] = "0"; }
            }

            for (int d = 0; d < 24; d += 2)
            {
                string _data = null;
                int f = 0;
                f = d * 250;
                _data = substringMonthSummary6[f].ToString() + " " + substringMonthSummary5[f];
                for (int i = 0; i < 499; i++)
                {
                    int d1 = d + 1;
                    int l = i + f;
                    try
                    {
                        if (substringMonthSummary1[l].Length > 2)
                        {
                            for (int j = 0; j < 99; j++)
                            {
                                try
                                {
                                    if (i == 0 && j == 0)
                                    {
                                        _BytesEveryYearMonthByURL[0, d] = substringMonthSummary1[l];
                                        _BytesEveryYearMonthByURL[0, d1] = substringMonthSummary2[l].ToString();
                                    }
                                    if (i > 0 && j < 100 && (_BytesEveryYearMonthByURL[j, d] == substringMonthSummary1[l]))
                                    {
                                        double st = Convert.ToDouble(_BytesEveryYearMonthByURL[j, d1]);
                                        _BytesEveryYearMonthByURL[j, d1] = (st + substringMonthSummary2[l]).ToString();
                                        j = 100;
                                    }
                                    if (i > 0 && j < 100 && (_BytesEveryYearMonthByURL[j, d].Length < 3))
                                    {
                                        _BytesEveryYearMonthByURL[j, d] = substringMonthSummary1[l];
                                        _BytesEveryYearMonthByURL[j, d1] = substringMonthSummary2[l].ToString();
                                        j = 100;
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }
                }
            }

            int d2 = 24;
            for (int d = 0; d < 24; d += 2)
            {
                for (int j = 0; j < 99; j++)
                {
                    for (int i = 0; i < 5999; i++)
                    {
                        try
                        {
                            if (_BytesEveryYearMonthByURL[j, d].Length > 2 && (_BytesEveryYearMonthByURL[j, d] == substringMonthSummary1[i]))
                            {
                                _BytesEveryYearMonthByURL[j, d2] = substringMonthSummary4[i];
                                i = 6000;
                            }
                        }
                        catch { }
                    }
                }
                d2++;
            }
        }

        /// ////////////////////////////////////////////////////////////////////




        //PRINT AND EXPORT. Start of the Block Buttons and Function
        private void printReportFullToolStripMenuItem_Click(object sender, EventArgs e)//Печать сводных графиков
        {
            chart1.Printing.PrintPreview();
            chart3.Printing.PrintPreview();
            chart4.Printing.PrintPreview();
            chart5.Printing.PrintPreview();
        }

        private void printDialogToolStripMenuItem_Click_1(object sender, EventArgs e) // Печать сводной таблицы
        {
            System.Drawing.Printing.PrintDocument Document = new System.Drawing.Printing.PrintDocument();
            //            Document.DefaultPageSettings.Landscape = true;                //для альбомной печати

            Document.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(_printDocument_PrintPage2);
            PrintPreviewDialog dlg = new PrintPreviewDialog();
            dlg.Document = Document;
            dlg.ShowDialog();

            Document = new System.Drawing.Printing.PrintDocument();
            Document.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(_printDocument_PrintPage3);
            dlg.Document = Document;
            dlg.ShowDialog();

            Document = new System.Drawing.Printing.PrintDocument();
            Document.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(_printDocument_PrintPage4);
            dlg.Document = Document;
            dlg.ShowDialog();

            Document = new System.Drawing.Printing.PrintDocument();
            Document.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(_printDocument_PrintPage6);
            dlg.Document = Document;
            dlg.ShowDialog();
        }

        private void _printDocument_PrintPage2(object sender, System.Drawing.Printing.PrintPageEventArgs e) // Печать таблицы 2. сводной таблицы
        {
            {
                Graphics g = e.Graphics;
                int x = 20;
                int y = 20;
                int cell_height = 0;

                int colCount = dataGridView2.ColumnCount;
                int rowCount = dataGridView2.RowCount - 1;

                Font font = new Font("Tahoma", 9, FontStyle.Bold, GraphicsUnit.Point);

                int[] widthC = new int[colCount];

                int current_col = 0;
                int current_row = 0;

                while (current_col < colCount)
                {
                    if (g.MeasureString(dataGridView2.Columns[current_col].HeaderText.ToString(), font).Width > widthC[current_col])
                    {
                        widthC[current_col] = (int)g.MeasureString(dataGridView2.Columns[current_col].HeaderText.ToString(), font).Width;
                    }
                    current_col++;
                }

                while (current_row < rowCount)
                {
                    while (current_col < colCount)
                    {
                        if (g.MeasureString(dataGridView2[current_col, current_row].Value.ToString(), font).Width > widthC[current_col])
                        {
                            widthC[current_col] = (int)g.MeasureString(dataGridView2[current_col, current_row].Value.ToString(), font).Width;
                        }
                        current_col++;
                    }
                    current_col = 0;
                    current_row++;
                }

                current_col = 0;
                current_row = 0;

                string value = "";

                int width = widthC[current_col];
                int height = dataGridView2[current_col, current_row].Size.Height;

                Rectangle cell_border;
                SolidBrush brush = new SolidBrush(Color.Black);

                while (current_col < colCount)
                {
                    width = widthC[current_col];
                    cell_height = dataGridView2[current_col, current_row].Size.Height;
                    cell_border = new Rectangle(x, y, width, height);
                    value = dataGridView2.Columns[current_col].HeaderText.ToString();
                    g.DrawRectangle(new Pen(Color.Black), cell_border);
                    g.DrawString(value, font, brush, x, y);
                    x += widthC[current_col];
                    current_col++;
                }
                while (current_row < rowCount)
                {
                    while (current_col < colCount)
                    {
                        width = widthC[current_col];
                        cell_height = dataGridView2[current_col, current_row].Size.Height;
                        cell_border = new Rectangle(x, y, width, height);
                        value = dataGridView2[current_col, current_row].Value.ToString();
                        g.DrawRectangle(new Pen(Color.Black), cell_border);
                        g.DrawString(value, font, brush, x, y);
                        x += widthC[current_col];
                        current_col++;
                    }
                    current_col = 0;
                    current_row++;
                    x = 20;
                    y += cell_height;
                }
            }
        }

        private void _printDocument_PrintPage3(object sender, System.Drawing.Printing.PrintPageEventArgs e) // Печать таблицы 3. Only month
        {
            Graphics g = e.Graphics;
            int x = 20;
            int y = 20;
            int cell_height = 0;

            int colCount = dataGridView3.ColumnCount;
            int rowCount = dataGridView3.RowCount - 1;

            Font font = new Font("Tahoma", 9, FontStyle.Bold, GraphicsUnit.Point);

            int[] widthC = new int[colCount];

            int current_col = 0;
            int current_row = 0;

            while (current_col < colCount)
            {
                if (g.MeasureString(dataGridView3.Columns[current_col].HeaderText.ToString(), font).Width > widthC[current_col])
                {
                    widthC[current_col] = (int)g.MeasureString(dataGridView3.Columns[current_col].HeaderText.ToString(), font).Width;
                }
                current_col++;
            }

            while (current_row < rowCount)
            {
                while (current_col < colCount)
                {
                    if (g.MeasureString(dataGridView3[current_col, current_row].Value.ToString(), font).Width > widthC[current_col])
                    {
                        widthC[current_col] = (int)g.MeasureString(dataGridView3[current_col, current_row].Value.ToString(), font).Width;
                    }
                    current_col++;
                }
                current_col = 0;
                current_row++;
            }

            current_col = 0;
            current_row = 0;

            string value = "";

            int width = widthC[current_col];
            int height = dataGridView3[current_col, current_row].Size.Height;

            Rectangle cell_border;
            SolidBrush brush = new SolidBrush(Color.Black);

            while (current_col < colCount)
            {
                width = widthC[current_col];
                cell_height = dataGridView3[current_col, current_row].Size.Height;
                cell_border = new Rectangle(x, y, width, height);
                value = dataGridView3.Columns[current_col].HeaderText.ToString();
                g.DrawRectangle(new Pen(Color.Black), cell_border);
                g.DrawString(value, font, brush, x, y);
                x += widthC[current_col];
                current_col++;
            }
            while (current_row < rowCount)
            {
                while (current_col < colCount)
                {
                    width = widthC[current_col];
                    cell_height = dataGridView3[current_col, current_row].Size.Height;
                    cell_border = new Rectangle(x, y, width, height);
                    value = dataGridView3[current_col, current_row].Value.ToString();
                    g.DrawRectangle(new Pen(Color.Black), cell_border);
                    g.DrawString(value, font, brush, x, y);
                    x += widthC[current_col];
                    current_col++;
                }
                current_col = 0;
                current_row++;
                x = 20;
                y += cell_height;
            }
        }

        private void _printDocument_PrintPage4(object sender, System.Drawing.Printing.PrintPageEventArgs e) // Печать таблицы 4. Only month
        {
            Graphics g = e.Graphics;
            int x = 20;
            int y = 20;
            int cell_height = 0;

            int colCount = dataGridView4.ColumnCount;
            int rowCount = dataGridView4.RowCount - 1;

            Font font = new Font("Tahoma", 9, FontStyle.Bold, GraphicsUnit.Point);

            int[] widthC = new int[colCount];

            int current_col = 0;
            int current_row = 0;

            while (current_col < colCount)
            {
                if (g.MeasureString(dataGridView4.Columns[current_col].HeaderText.ToString(), font).Width > widthC[current_col])
                {
                    widthC[current_col] = (int)g.MeasureString(dataGridView4.Columns[current_col].HeaderText.ToString(), font).Width;
                }
                current_col++;
            }

            while (current_row < rowCount)
            {
                while (current_col < colCount)
                {
                    if (g.MeasureString(dataGridView4[current_col, current_row].Value.ToString(), font).Width > widthC[current_col])
                    {
                        widthC[current_col] = (int)g.MeasureString(dataGridView4[current_col, current_row].Value.ToString(), font).Width;
                    }
                    current_col++;
                }
                current_col = 0;
                current_row++;
            }

            current_col = 0;
            current_row = 0;

            string value = "";

            int width = widthC[current_col];
            int height = dataGridView4[current_col, current_row].Size.Height;

            Rectangle cell_border;
            SolidBrush brush = new SolidBrush(Color.Black);

            while (current_col < colCount)
            {
                width = widthC[current_col];
                cell_height = dataGridView4[current_col, current_row].Size.Height;
                cell_border = new Rectangle(x, y, width, height);
                value = dataGridView4.Columns[current_col].HeaderText.ToString();
                g.DrawRectangle(new Pen(Color.Black), cell_border);
                g.DrawString(value, font, brush, x, y);
                x += widthC[current_col];
                current_col++;
            }
            while (current_row < rowCount)
            {
                while (current_col < colCount)
                {
                    width = widthC[current_col];
                    cell_height = dataGridView4[current_col, current_row].Size.Height;
                    cell_border = new Rectangle(x, y, width, height);
                    value = dataGridView4[current_col, current_row].Value.ToString();
                    g.DrawRectangle(new Pen(Color.Black), cell_border);
                    g.DrawString(value, font, brush, x, y);
                    x += widthC[current_col];
                    current_col++;
                }
                current_col = 0;
                current_row++;
                x = 20;
                y += cell_height;
            }
        }

        private void _printDocument_PrintPage6(object sender, System.Drawing.Printing.PrintPageEventArgs e) // Печать таблицы 6. Only month
        {
            Graphics g = e.Graphics;
            int x = 20;
            int y = 20;
            int cell_height = 0;

            int colCount = dataGridView6.ColumnCount;
            int rowCount = dataGridView6.RowCount - 1;

            Font font = new Font("Tahoma", 9, FontStyle.Bold, GraphicsUnit.Point);

            int[] widthC = new int[colCount];

            int current_col = 0;
            int current_row = 0;

            while (current_col < colCount)
            {
                if (g.MeasureString(dataGridView6.Columns[current_col].HeaderText.ToString(), font).Width > widthC[current_col])
                {
                    widthC[current_col] = (int)g.MeasureString(dataGridView6.Columns[current_col].HeaderText.ToString(), font).Width;
                }
                current_col++;
            }

            while (current_row < rowCount)
            {
                while (current_col < colCount)
                {
                    if (g.MeasureString(dataGridView6[current_col, current_row].Value.ToString(), font).Width > widthC[current_col])
                    {
                        widthC[current_col] = (int)g.MeasureString(dataGridView6[current_col, current_row].Value.ToString(), font).Width;
                    }
                    current_col++;
                }
                current_col = 0;
                current_row++;
            }

            current_col = 0;
            current_row = 0;

            string value = "";

            int width = widthC[current_col];
            int height = dataGridView6[current_col, current_row].Size.Height;

            Rectangle cell_border;
            SolidBrush brush = new SolidBrush(Color.Black);

            while (current_col < colCount)
            {
                width = widthC[current_col];
                cell_height = dataGridView6[current_col, current_row].Size.Height;
                cell_border = new Rectangle(x, y, width, height);
                value = dataGridView6.Columns[current_col].HeaderText.ToString();
                g.DrawRectangle(new Pen(Color.Black), cell_border);
                g.DrawString(value, font, brush, x, y);
                x += widthC[current_col];
                current_col++;
            }
            while (current_row < rowCount)
            {
                while (current_col < colCount)
                {
                    width = widthC[current_col];
                    cell_height = dataGridView6[current_col, current_row].Size.Height;
                    cell_border = new Rectangle(x, y, width, height);
                    value = dataGridView6[current_col, current_row].Value.ToString();
                    g.DrawRectangle(new Pen(Color.Black), cell_border);
                    g.DrawString(value, font, brush, x, y);
                    x += widthC[current_col];
                    current_col++;
                }
                current_col = 0;
                current_row++;
                x = 20;
                y += cell_height;
            }
        }

        public void _ProgressWork1()
        {
            if (ProgressBar1.Value > 99)
            { ProgressBar1.Value = 0; }
            ProgressBar1.Maximum = 100;
            ProgressBar1.Value += 2;
        }

        public void _ProgressWork2()
        {
            if (ProgressBar1.Value > 98)
            { ProgressBar1.Value = 0; }
            ProgressBar1.Maximum = 100;
            ProgressBar1.Value += 2;
        }

        public void _ProgressWork10()
        {
            if (ProgressBar1.Value > 90)
            { ProgressBar1.Value = 0; }
            ProgressBar1.Maximum = 100;
            ProgressBar1.Value += 10;
        }

        private void _exportTablesMenuItem_Click(object sender, EventArgs e) //Export all Tables to Excel
        {
            Thread th_3 = new Thread(_exportTables);
            th_3.Priority = ThreadPriority.Lowest;
            th_3.Start();
        }

        public void _exportTables(object data) //Печать графиков за месяц через бэкграунд
        {
            tabControl1.Enabled = false;
            ProgressBar1.Value = 0;
            StatusLabel2.Text = "Идет экспорт таблиц в Excel";
            string s = Environment.CurrentDirectory;
            dataGridView1.ReadOnly = true;
            _ExporDatagridToExcel(
                dataGridView1,
                s + "\\ProxyAnalyser\\" + _uUser + "_" + "summarize.xls", "URL", "Скачано ГБ", "Затрачено часов", "Категория", "Месяц");
            dataGridView1.ReadOnly = false;

            _ProgressWork10();
            dataGridView2.ReadOnly = true;
            _ExporDatagridToExcel(
                dataGridView2,
                s + "\\ProxyAnalyser\\" + _uUser + "_" + "fulldata.xls", "Категория", "Описание", "Скачано ГБ", "Затрачено часов");
            dataGridView2.ReadOnly = false;

            _ProgressWork10();
            dataGridView3.ReadOnly = true;
            _ExporDatagridToExcel(
                dataGridView3,
                s + "\\ProxyAnalyser\\" + _uUser + "_" + "summarize" + "_" + comboMonth.SelectedItem.ToString() + ".xls", "Категория", "Описание", "Скачано МБ", "Затрачено минут");
            dataGridView3.ReadOnly = false;

            _ProgressWork10();
            dataGridView6.ReadOnly = true;
            _ExporDatagridToExcel(
                dataGridView6,
                s + "\\ProxyAnalyser\\" + _uUser + "_" + "fulldata" + "_" + comboMonth.SelectedItem.ToString() + ".xls", "URL", "Категория", "Скачано МБ", "Затрачено минут");
            dataGridView6.ReadOnly = false;
            _ProgressWork10();

            _Excel_make();
            _ProgressWork10();

            MessageBox.Show("Excel files сохранены в " + s);
            StatusLabel2.Text = "Экспорт в Excel завершен";
            ProgressBar1.Value = 100;

            if (File.Exists(s + "\\ProxyAnalyser\\" + _uUser + "_" + "fulldata" + "_" + comboMonth.SelectedItem.ToString() + ".xls"))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer.exe", " /select, " + s + "\\ProxyAnalyser\\" + _uUser + "_" + "fulldata" + "_" + comboMonth.SelectedItem.ToString() + ".xls"));
            }
            tabControl1.Enabled = true;
        }

        private void _ExporDatagridToExcel(DataGridView dgv, string files, string _B1, string _C1 = "", string _D1 = "", string _E1 = "", string _F1 = "") //export Datagrid2 to Excel
        {
            Microsoft.Office.Interop.Excel.Application xlApp;
            Microsoft.Office.Interop.Excel.Workbook xlWorkBook;
            Microsoft.Office.Interop.Excel.Worksheet xlWorkSheet;
            object misValue = System.Reflection.Missing.Value;

            xlApp = new Microsoft.Office.Interop.Excel.Application();
            xlWorkBook = xlApp.Workbooks.Add(misValue);
            xlWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
            //add data 
            xlWorkSheet.Cells[1, 1] = _B1;
            xlWorkSheet.Cells[1, 2] = _C1;
            xlWorkSheet.Cells[1, 3] = _D1;
            xlWorkSheet.Cells[1, 4] = _E1;
            xlWorkSheet.Cells[1, 5] = _F1;

            int i = 0;
            int j = 0;
            for (i = 0; i <= dgv.RowCount - 1; i++)
            {
                for (j = 0; j <= dgv.ColumnCount - 1; j++)
                {
                    DataGridViewCell cell = dgv[j, i];
                    if (dgv[j, 1].Value != null)
                    {
                        xlWorkSheet.Cells[i + 2, j + 1] = cell.Value;
                    }
                }
            }

            Microsoft.Office.Interop.Excel.Range chartRange;

            Microsoft.Office.Interop.Excel.ChartObjects xlCharts = (Microsoft.Office.Interop.Excel.ChartObjects)xlWorkSheet.ChartObjects(Type.Missing);
            Microsoft.Office.Interop.Excel.ChartObject myChart = (Microsoft.Office.Interop.Excel.ChartObject)xlCharts.Add(10, 80, 300, 250);
            Microsoft.Office.Interop.Excel.Chart chartPage = myChart.Chart;
            string tcell = "F" + i + 2;
            chartRange = xlWorkSheet.get_Range("A1", tcell);
            chartPage.SetSourceData(chartRange, misValue);
            chartPage.ChartType = Microsoft.Office.Interop.Excel.XlChartType.xlColumnClustered;

            xlWorkBook.SaveAs(files, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
            xlWorkBook.Close(true, misValue, misValue);
            xlApp.Quit();

            releaseObject(xlWorkSheet);
            releaseObject(xlWorkBook);
            releaseObject(xlApp);
        }

        private void releaseObject(object obj) //for Export to excel
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                MessageBox.Show("Exception Occured while releasing object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }

        private void _Excel_make() //for button "Make Excel Chart"
        {
            string s = Environment.CurrentDirectory;

            //http://csharp.net-informations.com/excel/csharp-excel-chart.htm
            Microsoft.Office.Interop.Excel.Application xlApp;
            Microsoft.Office.Interop.Excel.Workbook xlWorkBook;
            Microsoft.Office.Interop.Excel.Worksheet xlWorkSheet;
            object misValue = System.Reflection.Missing.Value;

            xlApp = new Microsoft.Office.Interop.Excel.Application();
            xlWorkBook = xlApp.Workbooks.Add(misValue);
            xlWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

            //add data 
            xlWorkSheet.Cells[1, 1] = "Категории";
            xlWorkSheet.Cells[1, 2] = "ГБ";
            //            xlWorkSheet.Cells[1, 3] = "Затраченное время";
            int t = 1;
            for (int d = 0; d < 99; d++)
            {
                try
                {
                    if (_Directions[d].Length > 2 && _DirectionsByte[d] > 1)
                    {
                        xlWorkSheet.Cells[1, t] = _Directions[d].ToString();
                        xlWorkSheet.Cells[2, t] = _DirectionsByte[d];
                        t++;
                        //                        xlWorkSheet.Cells[t+2, 2] = _DirectionsTime[d];
                    }
                }
                catch { }
            }
            Microsoft.Office.Interop.Excel.Range chartRange;

            Microsoft.Office.Interop.Excel.ChartObjects xlCharts = (Microsoft.Office.Interop.Excel.ChartObjects)xlWorkSheet.ChartObjects(Type.Missing);
            Microsoft.Office.Interop.Excel.ChartObject myChart = (Microsoft.Office.Interop.Excel.ChartObject)xlCharts.Add(10, 80, 300, 250);
            Microsoft.Office.Interop.Excel.Chart chartPage = myChart.Chart;
            string tcell = "p" + 2;
            chartRange = xlWorkSheet.get_Range("A1", tcell);
            chartPage.SetSourceData(chartRange, misValue);
            chartPage.ChartType = Microsoft.Office.Interop.Excel.XlChartType.xlColumnClustered;

            xlWorkBook.SaveAs(s + "\\ProxyAnalyser\\" + "myfile1.xls", Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
            xlWorkBook.Close(true, misValue, misValue);
            xlApp.Quit();

            releaseObject(xlWorkSheet);
            releaseObject(xlWorkBook);
            releaseObject(xlApp);
        }

        ////////////// Export to PDF /////////////////////////////////////////////////////////////////////////////

        private void _exportTablesToPDF_Click(object sender, EventArgs e) //Export all Tables to Excel
        {
            Thread th_1 = new Thread(_exportTablesToPDF);
            th_1.Priority = ThreadPriority.Normal;
            th_1.Start();
            //      _exportTablesToPDF();
        }

        public void _exportTablesToPDF() //Экспорт графиков и таблиц в PDF через бэкграунд
        {
            tabControl1.Enabled = false;

            ProgressBar1.Value = 0;
            StatusLabel2.Text = "Идет экспорт таблиц в PDF";
            string s = Environment.CurrentDirectory;


            //////////////////////------PDF1------////////////////
            //Заголовок 1
            MyHeaderFooterEvent mypdfFooter = new MyHeaderFooterEvent();
            iTextSharp.text.pdf.PdfPCell cell1 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("Table",
            new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 16,
            iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(Color.Orange))));
            cell1.BackgroundColor = new iTextSharp.text.BaseColor(Color.Wheat);
            _ProgressWork10();

            // iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 50f, 50f, 40f, 40f);
            // iTextSharp.text.pdf.PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(pdfDoc, new FileStream(Application.StartupPath + "\\ProxyAnalyser\\" + _uUser + "_" + "fulldata.pdf", FileMode.Create));

            iTextSharp.text.Document PDFdocument = new iTextSharp.text.Document();
            iTextSharp.text.pdf.BaseFont baseFont = iTextSharp.text.pdf.BaseFont.CreateFont(Environment.ExpandEnvironmentVariables(@"%systemroot%\fonts\Calibri.TTF"), "CP1251", iTextSharp.text.pdf.BaseFont.EMBEDDED);
            iTextSharp.text.pdf.BaseFont boldFont = iTextSharp.text.pdf.BaseFont.CreateFont(Environment.ExpandEnvironmentVariables(@"%systemroot%\fonts\Calibri.TTF"), "CP1251", iTextSharp.text.pdf.BaseFont.EMBEDDED);

            try
            {
                iTextSharp.text.pdf.PdfWriter wri = iTextSharp.text.pdf.PdfWriter.GetInstance(PDFdocument, new FileStream(Application.StartupPath + "\\ProxyAnalyser\\" + _uUser + "_" + "fulldata.pdf", FileMode.Create));

                //Служебная информация для PDF файла
                PDFdocument.AddAuthor("©RYIK 2016-2018");
                PDFdocument.AddProducer();
                PDFdocument.AddCreationDate();
                PDFdocument.AddCreator("ProxyAnalyser.exe");
                PDFdocument.AddTitle("Анализ статистики прокси  " + _uUser);
                PDFdocument.AddSubject("Результаты анализа статистики by ProxyAnalyser ©RYIK 2016-2018");
                PDFdocument.AddHeader("content-disposition", "attachment;filename=Locations.pdf");
                PDFdocument.AddKeywords("ProxyAnalyser, SARG, RYIK, прокси, статистика");

                //задаем фон и размеры для главной страницы 
                iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(iTextSharp.text.PageSize.A4);
                // iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(iTextSharp.text.PageSize.A4.Rotate());
                //  rec.BackgroundColor = new iTextSharp.text.BaseColor(Color.WhiteSmoke);
                PDFdocument.SetPageSize(rec);
                PDFdocument.SetMargins(30, 20, 30, 30);
                PDFdocument.Open();

                //Переменные для форматирования PDF
                iTextSharp.text.Paragraph ph;
                iTextSharp.text.Image jpg;
                iTextSharp.text.pdf.PdfPTable table;
                string fileCreationDatetime = DateTime.Now.ToShortDateString();

                // Add myLOGO  RYIK
                jpg = iTextSharp.text.Image.GetInstance(LogoPNG);
                jpg.ScaleToFit(32f, 32f);
                jpg.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                PDFdocument.Add(jpg);

                ph = new iTextSharp.text.Paragraph("\n\n\n\n\nАнализ\n", new iTextSharp.text.Font(baseFont, 32, 1, new iTextSharp.text.BaseColor(Color.SteelBlue)));
                ph.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                PDFdocument.Add(ph);
                ph = new iTextSharp.text.Paragraph("статистики прокси (SARG)\n", new iTextSharp.text.Font(baseFont, 20, 1, new iTextSharp.text.BaseColor(Color.SteelBlue)));
                ph.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                PDFdocument.Add(ph);
                ph = new iTextSharp.text.Paragraph("по логину \"" + _uUser + "\"", new iTextSharp.text.Font(baseFont, 20, 3, new iTextSharp.text.BaseColor(Color.SteelBlue)));
                ph.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                PDFdocument.Add(ph);
                ph = new iTextSharp.text.Paragraph("и генерация отчета\n\n", new iTextSharp.text.Font(baseFont, 20, 3, new iTextSharp.text.BaseColor(Color.SteelBlue)));
                ph.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                PDFdocument.Add(ph);
                ph = new iTextSharp.text.Paragraph("выполнены\nby ProxyAnalyser\n" + fileCreationDatetime, new iTextSharp.text.Font(baseFont, 12, 1, new iTextSharp.text.BaseColor(Color.SlateGray)));
                ph.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                PDFdocument.Add(ph);
                _ProgressWork10();

                //новая страница
                PDFdocument.NewPage();

                //Отступ сверху
                string sHeader = "Анализ статистики прокси по " + _uUser;
                mypdfFooter.SubHeaderText = sHeader;
                mypdfFooter.PageNumber = wri.CurrentPageNumber - 1;
                mypdfFooter.TimerText = fileCreationDatetime;

                wri.PageEvent = mypdfFooter;

                // Add JPG
                jpg = iTextSharp.text.Image.GetInstance(Application.StartupPath + @"/ProxyAnalyser/chart1.png");
                jpg.ScaleToFit(500f, 450f);
                jpg.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                PDFdocument.Add(jpg);
                _ProgressWork10();

                ph = new iTextSharp.text.Paragraph("\n", new iTextSharp.text.Font(baseFont, 16, 1));
                ph.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                PDFdocument.Add(ph);

                //Таблица
                table = new iTextSharp.text.pdf.PdfPTable(dataGridView2.ColumnCount);
                table.KeepTogether = true;

                //Создадим заголовок 1
                iTextSharp.text.pdf.PdfPCell cell2 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Paragraph("Суммарно, за выбранный период", new iTextSharp.text.Font(baseFont, 16)));
                cell2.Colspan = dataGridView2.ColumnCount;
                cell2.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                table.AddCell(cell2);

                cell1.Padding = 5;
                cell1.VerticalAlignment = iTextSharp.text.pdf.PdfPCell.ALIGN_MIDDLE;
                cell1.HorizontalAlignment = iTextSharp.text.pdf.PdfPCell.ALIGN_CENTER;

                string value1 = "Категория";
                cell1 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Paragraph(value1, new iTextSharp.text.Font(baseFont, 14)));
                cell1.HorizontalAlignment = 1;
                table.AddCell(cell1);

                value1 = "Описание";
                cell1 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Paragraph(value1, new iTextSharp.text.Font(baseFont, 14)));
                cell1.HorizontalAlignment = 1;
                table.AddCell(cell1);

                value1 = "Скачано, ГБ";
                cell1 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Paragraph(value1, new iTextSharp.text.Font(baseFont, 14)));
                cell1.HorizontalAlignment = 1;
                table.AddCell(cell1);
                _ProgressWork2();

                //Ограничим 15 строками 
                for (int i = 0; i < 15; i++)
                {
                    for (int z = 0; z < dataGridView2.ColumnCount; z++)
                    {
                        try
                        {
                            string value = dataGridView2.Rows[i].Cells[z].Value.ToString();
                            iTextSharp.text.pdf.PdfPCell cell = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Paragraph(value, new iTextSharp.text.Font(baseFont, 9)));
                            table.AddCell(cell);
                        }
                        catch { }
                    }
                }

                PDFdocument.Add(table);
                _ProgressWork10();


                //////////////////////------PDF2------////////////////
                //новая страница
                PDFdocument.NewPage();
                //            добавляем параграф
                ph = new iTextSharp.text.Paragraph("Статистика по месяцам", new iTextSharp.text.Font(baseFont, 16));
                ph.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                PDFdocument.Add(ph);

                // Add JPG
                jpg = iTextSharp.text.Image.GetInstance(Application.StartupPath + @"/ProxyAnalyser/chart4.png"); jpg.ScaleToFit(400f, 450f);
                jpg.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                PDFdocument.Add(jpg);

                //новая страница
                PDFdocument.NewPage();
                //            добавляем параграф
                ph = new iTextSharp.text.Paragraph("Статистика по месяцам", new iTextSharp.text.Font(baseFont, 16));
                ph.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                PDFdocument.Add(ph);

                // Add JPG
                jpg = iTextSharp.text.Image.GetInstance(Application.StartupPath + @"/ProxyAnalyser/chart5.png"); jpg.ScaleToFit(400f, 450f);
                jpg.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                PDFdocument.Add(jpg);
                _ProgressWork2();

                //////////////////////------PDF3------////////////////
                //новая страница
                PDFdocument.NewPage();
                //            добавляем параграф
                ph = new iTextSharp.text.Paragraph("\n", new iTextSharp.text.Font(baseFont, 16, 1));
                ph.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                PDFdocument.Add(ph);

                //Таблица
                table = new iTextSharp.text.pdf.PdfPTable(dataGridView4.ColumnCount);
                table.KeepTogether = true;

                //Создадим заголовок 1
                cell2 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Paragraph("ТОР сайтов, за выбранный период", new iTextSharp.text.Font(baseFont, 16)));
                cell2.Colspan = dataGridView4.ColumnCount;
                cell2.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                table.AddCell(cell2);

                cell1.Padding = 5;
                cell1.VerticalAlignment = iTextSharp.text.pdf.PdfPCell.ALIGN_MIDDLE;
                cell1.HorizontalAlignment = iTextSharp.text.pdf.PdfPCell.ALIGN_CENTER;

                value1 = "URL";
                cell1 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Paragraph(value1, new iTextSharp.text.Font(baseFont, 14)));
                cell1.HorizontalAlignment = 1;
                table.AddCell(cell1);

                value1 = "Категория";
                cell1 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Paragraph(value1, new iTextSharp.text.Font(baseFont, 14)));
                cell1.HorizontalAlignment = 1;
                table.AddCell(cell1);

                value1 = "Скачано, ГБ";
                cell1 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Paragraph(value1, new iTextSharp.text.Font(baseFont, 14)));
                cell1.HorizontalAlignment = 1;
                table.AddCell(cell1);

                value1 = "Затрачено часов";
                cell1 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Paragraph(value1, new iTextSharp.text.Font(baseFont, 14)));
                cell1.HorizontalAlignment = 1;
                table.AddCell(cell1);
                _ProgressWork2();

                string value2;
                //Ограничим 25 строками 
                for (int i = 0; i < 25; i++)
                {
                    for (int z = 0; z < dataGridView4.ColumnCount; z++)
                    {
                        try
                        {
                            value2 = dataGridView4.Rows[i].Cells[z].Value.ToString();
                            iTextSharp.text.pdf.PdfPCell cell4 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Paragraph(value2, new iTextSharp.text.Font(baseFont, 9)));
                            table.AddCell(cell4);
                        }
                        catch { }
                    }
                }

                PDFdocument.Add(table);
                _ProgressWork10();


                //////////////////////------PDF4------////////////////
                // новая страница
                PDFdocument.NewPage();
                // добавляем параграф
                ph = new iTextSharp.text.Paragraph("Распределение по категориям за " + comboMonth.SelectedItem.ToString(), new iTextSharp.text.Font(baseFont, 16));
                ph.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                PDFdocument.Add(ph);

                // Add JPG
                jpg = iTextSharp.text.Image.GetInstance(Application.StartupPath + @"/ProxyAnalyser/chart3.png"); jpg.ScaleToFit(450f, 450f);
                jpg.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                PDFdocument.Add(jpg);
                _ProgressWork2();

                PDFdocument.Add(new iTextSharp.text.Paragraph("\n", new iTextSharp.text.Font(baseFont, 16)));

                //Таблица
                table = new iTextSharp.text.pdf.PdfPTable(dataGridView3.ColumnCount);
                table.KeepTogether = true;

                //Создадим заголовок 1
                cell2 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Paragraph("Распределение по ТОП категориям за  " + comboMonth.SelectedItem.ToString(), new iTextSharp.text.Font(baseFont, 16)));
                cell2.Colspan = dataGridView3.ColumnCount;
                cell2.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                table.AddCell(cell2);

                cell1.Padding = 5;
                cell1.VerticalAlignment = iTextSharp.text.pdf.PdfPCell.ALIGN_MIDDLE;
                cell1.HorizontalAlignment = iTextSharp.text.pdf.PdfPCell.ALIGN_CENTER;

                value1 = "Категория";
                cell1 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Paragraph(value1, new iTextSharp.text.Font(baseFont, 14)));
                cell1.HorizontalAlignment = 1;
                table.AddCell(cell1);

                value1 = "Описание";
                cell1 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Paragraph(value1, new iTextSharp.text.Font(baseFont, 14)));
                cell1.HorizontalAlignment = 1;
                table.AddCell(cell1);

                value1 = "Скачано, МБ";
                cell1 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Paragraph(value1, new iTextSharp.text.Font(baseFont, 14)));
                cell1.HorizontalAlignment = 1;
                table.AddCell(cell1);
                _ProgressWork2();

                //Ограничим 25 строками 
                for (int i = 0; i < 25; i++)
                {
                    for (int z = 0; z < dataGridView3.ColumnCount; z++)
                    {
                        try
                        {
                            string value3 = dataGridView3.Rows[i].Cells[z].Value.ToString();
                            iTextSharp.text.pdf.PdfPCell cell3 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Paragraph(value3, new iTextSharp.text.Font(baseFont, 9)));
                            table.AddCell(cell3);
                        }
                        catch { }
                    }
                }
                PDFdocument.Add(table);
                _ProgressWork10();


                //////////////////////------PDF5------////////////////
                //новая страница
                PDFdocument.NewPage();
                //  добавляем параграф
                ph = new iTextSharp.text.Paragraph("TOP сайтов за " + comboMonth.SelectedItem.ToString() + "\n", new iTextSharp.text.Font(baseFont, 16));
                ph.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                PDFdocument.Add(ph);
                ph = new iTextSharp.text.Paragraph("\n", new iTextSharp.text.Font(baseFont, 12));
                ph.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                PDFdocument.Add(ph);

                //Таблица
                table = new iTextSharp.text.pdf.PdfPTable(dataGridView6.ColumnCount);

                //Создадим заголовок 1
                cell2 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Paragraph("Распределение по категориям за " + comboMonth.SelectedItem.ToString(), new iTextSharp.text.Font(baseFont, 16)));
                cell2.Colspan = dataGridView6.ColumnCount;
                cell2.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
                table.AddCell(cell2);

                cell1.Padding = 5;
                cell1.VerticalAlignment = iTextSharp.text.pdf.PdfPCell.ALIGN_MIDDLE;
                cell1.HorizontalAlignment = iTextSharp.text.pdf.PdfPCell.ALIGN_CENTER;

                value1 = "URL";
                cell1 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Paragraph(value1, new iTextSharp.text.Font(baseFont, 14)));
                cell1.HorizontalAlignment = 1;
                table.AddCell(cell1);

                value1 = "Скачано, МБ";
                cell1 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Paragraph(value1, new iTextSharp.text.Font(baseFont, 14)));
                cell1.HorizontalAlignment = 1;
                table.AddCell(cell1);

                value1 = "Категория";
                cell1 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Paragraph(value1, new iTextSharp.text.Font(baseFont, 14)));
                cell1.HorizontalAlignment = 1;
                table.AddCell(cell1);

                value1 = "Месяц";
                cell1 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Paragraph(value1, new iTextSharp.text.Font(baseFont, 14)));
                cell1.HorizontalAlignment = 1;
                table.AddCell(cell1);

                value1 = "Год";
                cell1 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Paragraph(value1, new iTextSharp.text.Font(baseFont, 14)));
                cell1.HorizontalAlignment = 1;
                table.AddCell(cell1);
                _ProgressWork10();

                //Ограничим 30 строками 
                for (int i = 0; i < 30; i++)
                {
                    for (int z = 0; z < dataGridView6.ColumnCount; z++)
                    {
                        try
                        {
                            string value = dataGridView6.Rows[i].Cells[z].Value.ToString();
                            iTextSharp.text.pdf.PdfPCell cell = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Paragraph(value, new iTextSharp.text.Font(baseFont, 9)));
                            table.AddCell(cell);
                        }
                        catch { }
                    }
                }
                _ProgressWork10();
                PDFdocument.Add(table);

                PDFdocument.Close();
                StatusLabel2.Text = "Экспорт результатов анализа в PDF завершен";

                if (File.Exists(Application.StartupPath + "\\ProxyAnalyser\\" + _uUser + "_fulldata.pdf"))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer.exe", " /select, " + Application.StartupPath + "\\ProxyAnalyser\\" + _uUser + "_" + "fulldata.pdf"));
                }
            }
            catch { MessageBox.Show("Ошибка доступа к файлу" + Application.StartupPath + "\\ProxyAnalyser\\" + _uUser + "_fulldata.pdf"); }
            ProgressBar1.Value = 100;
            tabControl1.Enabled = true;
        }

        private bool bLoadURI = false;

        private void loadURIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //The Start of The Block. for transfer any data between Form1 and Form2
            this.Hide();
            comboMonth.Items.Clear();
            Form2 f2 = new Form2(this);
            f2.bLoadURI = true;
            bLoadURI = true;
            f2.ShowDialog();
            this.Show();
            _CheckTemporaryHTML();
            f2.bLoadURI = false;
            f2.Close();
            f2.Dispose();
            bLoadURI = false;
        }
        ////////////// Export to PDF /////////////////////////////////////////////////////////////////////////////
        //PRINT AND EXPORT. End of The Block Buttons and Function

    }
    //The end of Form1

}

