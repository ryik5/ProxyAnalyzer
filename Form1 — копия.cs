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
        private string[] arrayReplacingIni = new string            [1];
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

        public Form1()  //for transfer any data between Form1 and Form12
        {
            InitializeComponent();
            Icon = Properties.Resources.iconRYIK;                   //my icon
            notifyIcon.Icon = Properties.Resources.iconRYIK;
            Bitmap bmplogo = new Bitmap(Properties.Resources.LogoRYIK);
            var converter = new ImageConverter();
            LogoPNG = iTextSharp.text.Image.GetInstance((byte[])converter.ConvertTo(bmplogo, typeof(byte[])));

            System.Diagnostics.FileVersionInfo myFileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(Environment.CurrentDirectory + "\\ProxyAnalyser.exe");
            StatusLabel1.Text = "Анализ статистики прокси-сервера (SARG). ver." + myFileVersionInfo.FileVersion + "  ©RYIK 2016-2018";
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
            _initoArraysClearerAndSiplifierUrl();
            _CheckTemporaryHTML();
        }

        private void Form1_Load(object sender, EventArgs e) //Read ProxyAnalyser.ini into arrays at memory in the start of the Form1
        {         

        }

        private void _LoadAndSelectData_Click(object sender, EventArgs e) //Open Form2  "Select Data"
        {
            //The Start of The Block. for transfer any data between Form1 and Form2
            this.Hide();
            comboMonth.Items.Clear();
            Form2 f2 = new Form2(this);

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
                        _ParsingHtmlToCSV((k), ii);
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
                    for (int iD = 0; iD < 299; iD++)
                    {
                        try
                        {
                            string o2 = tmpDscrDirctn[0].ToLower().Trim();
                            string o1 = _Directions[iD].ToLower();
                            if (o1.Length > 1 && o1 == o2)
                            {
                                _DirectionsDiscription[iD] = tmpDscrDirctn[1].Trim();
                                iD++;
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        private void _initoArraysClearerAndSiplifierUrl() //Read listProxyCheckerIni and Make HashLists with settings
        {
            MakeListClearURL(";cleaner", ";end cleaner", hsClearingIni, listProxyCheckerIni); //Remove trash from URL
            MakeListClearURL(";simplifier", ";end simplifier", hsSimplifyingIni, listProxyCheckerIni); //Remove trash from URL
            MakeListClearURL(";SimplifyEnd", ";End SimplifyEnd", hsSimplifying2Ini, listProxyCheckerIni); //Remove trash from URL
            MakeListClearURL(";replacer", ";end replacer", hsReplacingIni, listProxyCheckerIni); //Remove trash from URL

            string[] arrayReplacingIni = hsReplacingIni.ToArray();
            string[] arraySimplifying2Ini = hsSimplifying2Ini.ToArray();
            string[] arraySimplifyingIni = hsSimplifyingIni.ToArray();
            string[] arrayClearingIni = hsClearingIni.ToArray();
        }

        private void MakeListClearURL(string startWord, string endWord, HashSet<string> hsClearURL, List<string> fullListIni)
        {
            hsClearURL = new HashSet<string>();
            string s=""; bool bListUrls = false;
            for (int i = 0; i < fullListIni.ToArray().Length; i++)
            {
                s = fullListIni[i];
                if (s.StartsWith(startWord.ToLower())) { bListUrls = true; continue; }  //Start of list
                if (s.StartsWith(endWord.ToLower())) { bListUrls = false; break; }      //End of list
                if (bListUrls) { hsClearURL.Add(s);  }
            }
            s = null;
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
                    { listProxyCheckerIni.Add(s.ToLower().Trim()); }
                }
            }
        }

        private void _ReadArrayAndSimplify() //упрощение каждого URL
        {
            string a = "", b = "", c = "", d="", tmpReplUrl0 = "", sDomainLevelsUserEnd = "";
            string[] tmpReplUrls1 = new string[] { "" };
            string[] domainLevelsUser = new string[] { "" };

            for (int l = 0; l < 5999; l++)
            {
                c = substringMonthSummary1[l].Trim().ToLower();

                if (c.Contains("vimeo"))
                { MessageBox.Show("1.\n" + c); }

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

                    if (c.Contains("vimeo"))
                        MessageBox.Show("2.\n" + substringMonthSummary1[l]);

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

                    if (c.Contains("vimeo"))
                        MessageBox.Show("3.\n" + substringMonthSummary1[l]);

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

                    if (c.Contains("vimeo"))
                    {
                        MessageBox.Show("4.\n" + substringMonthSummary1[l] + "\n" + arraySimplifying2Ini.Length+"\n" +
                           arrayClearingIni.Length+"\n"+
                             arraySimplifyingIni.Length);
                    }

                    //SimplifyEnd
                    foreach (string sReplacement in arraySimplifying2Ini)
                    {
                        MessageBox.Show("4.5.\n" + substringMonthSummary1[l]);
                        a = sReplacement;
                        tmpReplUrl0 = substringMonthSummary1[l];

                        if (tmpReplUrl0.Contains(a))
                        { substringMonthSummary1[l] = a; MessageBox.Show("4.7.\n" +a); }
                    }

                    if (c.Contains("vimeo"))
                        MessageBox.Show("5.\n" + substringMonthSummary1[l]);

                }
            }
            sDomainLevelsUserEnd = ""; domainLevelsUser = new string[] { "" }; tmpReplUrls1 = new string[] { "" };
        }

        private void _ReadArrayAndSetUrlStatus() //Read "substringDirectionIniFull" и выставление категорий для каждого направления
        {
            string b="", s = "", a = "";
            try
            {
                for (int h = 0; h < substringDirectionIniFull.Length; h++)
                {
                    if (substringDirectionIniFull[h].Length > 2)
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
            }
            catch { }
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
            comboMonth.SelectedIndex = 0; uTemporary2 = null;s = null;
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
            string _tmpExist = "0", k=null, s=null;
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

            try  //Parsing Elapsed Time into minutes
            {
                NoAltElements = HD.DocumentNode.SelectNodes("//tfoot/tr/th[@class='header_r'][1]");
                if (NoAltElements != null)      // проверка на наличие найденных узлов
                {
                    foreach (HtmlNode HN in NoAltElements)
                    {
                        string a = HN.InnerText.Replace(".", ",");
                        string a1 = "0";

                        if (a.ToLower().Contains('k'))
                        { a1 = "k"; }

                        if (a.ToLower().Contains('g'))
                        { a1 = "g"; }

                        if (a.ToLower().Contains('m'))
                        { a1 = "m"; }


                        switch (a1)
                        {
                            case ("k"):
                                a = a.ToLower().Replace("k", "").Trim();
                                a = ((Convert.ToDouble(a) / 1024 / 1024).ToString());
                                break;
                            case ("m"):
                                a = a.ToLower().Replace("m", "").Trim();
                                a = ((Convert.ToDouble(a) / 1024).ToString());
                                break;
                            case ("g"):
                                a = a.ToLower().Replace("g", "").Trim();
                                a = (Convert.ToDouble(a)).ToString();
                                break;
                            default:
                                a = "0,000001";
                                break;
                        }

                        _substringMonthAndBytes[(myStartAddrs)] += Math.Round(Convert.ToDouble(a), 2); //Результат в ГБ
                        break;
                    }
                }
            }
            catch { }
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
            System.Diagnostics.FileVersionInfo myFileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(Environment.CurrentDirectory + "\\ProxyAnalyser.exe");
            string strVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            DialogResult result = MessageBox.Show(
                "Программа предназначена\nдля обработки статистики интернет-трафика\nпользователей корпоративного прокси-сервера SARG версии 2.3.9\n" +
                "\nOriginal name: " + myFileVersionInfo.OriginalFilename + "\n" + myFileVersionInfo.LegalCopyright +
                "\n" + "Файл:\n" + Environment.CurrentDirectory + "\\ProxyAnalyser.exe\n" + "Версия: " + myFileVersionInfo.FileVersion + "\nBuild: " +
                strVersion + "\n",
                "Информация о программе",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.DefaultDesktopOnly);
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
                            _myDirection = _Directions[h].ToString(),
                            _myDiscription = _DirectionsDiscription[h].ToString(),
                            _myDirectionBytes = _DirectionsByte[h]
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

            //            _myStatistics = _myStatistics.AsEnumerable().OrderBy(row => row.Field<Double>(stolbec)).CopyToDataTable();

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
            List<_StatisticsDirectionMonth> _items = new List<_StatisticsDirectionMonth>();

            string[] submon = Regex.Split(comboMonth.SelectedItem.ToString(), " ");
            int _selYear = Convert.ToInt32(submon[0]);
            string _selMonth = submon[1].ToString().Trim();

            for (int h = 0; h < 99; h++) //сбор данных за месяц из общих данных по выбранным данным в комбобох в массивы
            {
                try
                {
                    if (_Directions[h].Length > 1)
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
                        _items.Add(new _StatisticsDirectionMonth
                        {
                            _myDirection = _Directions[h].ToString(),
                            _myDiscription = _DirectionsDiscription[h].ToString(),
                            _myDirectionBytes = _DirectionsMonthByte[h]
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
                    string a = _Directions[i].ToString();
                    if (a.Length > 1 && _DirectionsMonthByte[i] > 0)
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



            ////////////////////////////////////// Проверить. Нужно ли удалять эти куски //////////////////////////////////

            List<_StatisticsDirectionMonthFull> _itemsF = new List<_StatisticsDirectionMonthFull>();

            for (int h = 0; h < 5999; h++) // формирование таблицы из массивов
            {
                try
                {
                    if (substringMonthSummary2[h] > 0 &&
                        substringMonthSummary5[h] == _selMonth &&
                        substringMonthSummary6[h] == _selYear &&
                        substringMonthSummary1[h].Length > 1
                        )
                    {
                        _itemsF.Add(new _StatisticsDirectionMonthFull
                        {
                            _myDirection = substringMonthSummary4[h],
                            _myURL = substringMonthSummary1[h],
                            _myDirectionBytes = substringMonthSummary2[h]
                        });
                    }
                }
                catch { }
            }
            ////////////////////////////////////// Проверить. Нужно ли удалять эти куски //////////////////////////////////



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

            ////////////////////////////////////// Проверить. Нужно ли удалять эти куски //////////////////////////////////

            List<_StatisticsURLEveryMonth> _itemsU = new List<_StatisticsURLEveryMonth>(); //TOP100 сайтов every month
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

            for (int h = 0; h < 99; h++) // формирование таблицы из массивов
            {
                try
                {
                    if (_BytesEveryYearMonthByURL[h, U].Length > 2)
                    {
                        _itemsU.Add(new _StatisticsURLEveryMonth
                        {
                            _myURL = _BytesEveryYearMonthByURL[h, U],
                            _myDirectionBytes = Convert.ToDouble(_BytesEveryYearMonthByURL[h, U1 + 1]),
                            _myDirection = _BytesEveryYearMonthByURL[h, U2],
                            _Month = _selMonth,
                            _Year = _selYear
                        });
                    }
                }
                catch { }
            }
            ////////////////////////////////////// Проверить. Нужно ли удалять эти куски //////////////////////////////////


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
            {
                chart4.SaveImage(Application.StartupPath + "\\ProxyAnalyser\\chart4.png", ChartImageFormat.Png);
            }
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


            ////////////////////////////////////// Проверить. Нужно ли удалять эти куски //////////////////////////////////

            List<_StatisticsURLTotal> _itemsUT = new List<_StatisticsURLTotal>(); //TOP100 сайтов Total Period

            for (int h = 0; h < 99; h++) // формирование таблицы из массивов
            {
                try
                {
                    if (_BytesTopUrlTotal[h, 0].Length > 2)
                    {
                        _itemsUT.Add(new _StatisticsURLTotal
                        {
                            _myURL = _BytesTopUrlTotal[h, 0],
                            _myDirection = _BytesTopUrlTotal[h, 1],
                            _myDirectionBytes = Math.Round(Convert.ToDouble(_BytesTopUrlTotal[h, 2]), 0),
                            _myTime = Math.Round(Convert.ToDouble(_BytesTopUrlTotal[h, 3]), 0),
                            _User = _uUser,
                            _iD = h
                        });
                    }
                }
                catch { }
            }
            ////////////////////////////////////// Проверить. Нужно ли удалять эти куски //////////////////////////////////


            try { dataGridView4.Rows.Clear(); } catch { }

            DataTable _myUTStatistics = new DataTable("StatisticsURLEveryMonth");
            DataColumn[] colsUT ={
                                  new DataColumn("URL",typeof(string)),
                                  new DataColumn("Категория",typeof(string)),
                                  new DataColumn("Скачано ГБ",typeof(double)),
                                  new DataColumn("Затрачено часов",typeof(double)),
                              };

            _myUTStatistics.Columns.AddRange(colsUT);
            //            _myUTStatistics.PrimaryKey = new DataColumn[] { _myUTStatistics.Columns["iD"] };

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
                        if (_Directions[i] == substringMonthSummary4[p])
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

    }        //The end of Form1


    // Класс для построения загoловков PDF iTextSharp - footer and header
    class MyHeaderFooterEvent : iTextSharp.text.pdf.PdfPageEventHelper
    {
        private string _subHeaderText;
        private string _timerText;
        private int _pageNo;

        iTextSharp.text.pdf.BaseFont baseFont = iTextSharp.text.pdf.BaseFont.CreateFont(Environment.ExpandEnvironmentVariables(@"%systemroot%\fonts\Calibri.TTF"), "CP1251", iTextSharp.text.pdf.BaseFont.EMBEDDED);
        iTextSharp.text.pdf.BaseFont boldFont = iTextSharp.text.pdf.BaseFont.CreateFont(Environment.ExpandEnvironmentVariables(@"%systemroot%\fonts\Calibri.TTF"), "CP1251", iTextSharp.text.pdf.BaseFont.EMBEDDED);
        //iTextSharp.text.Font FONT = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 6, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);
        //iTextSharp.text.Font FONT = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 6, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);


        public string SubHeaderText
        {
            get { return _subHeaderText; }
            set { _subHeaderText = value; }
        }
        public string TimerText
        {
            get { return _timerText; }
            set { _timerText = value; }
        }
        public int PageNumber
        {
            get { return _pageNo; }
            set { _pageNo = value; }
        }

        public override void OnEndPage(iTextSharp.text.pdf.PdfWriter writer, iTextSharp.text.Document document)
        {
            iTextSharp.text.Rectangle page = document.PageSize;
            iTextSharp.text.pdf.PdfContentByte canvas = writer.DirectContent;
            iTextSharp.text.pdf.ColumnText.ShowTextAligned(canvas, iTextSharp.text.Element.ALIGN_LEFT, new iTextSharp.text.Phrase("ProxyAnalyser", new iTextSharp.text.Font(baseFont, 6)), 20, 20, 0);
            iTextSharp.text.pdf.ColumnText.ShowTextAligned(canvas, iTextSharp.text.Element.ALIGN_LEFT, new iTextSharp.text.Phrase(" ©RYIK 2016-2017", new iTextSharp.text.Font(baseFont, 6)), 510, 20, 0);
            //iTextSharp.text.pdf.ColumnText.ShowTextAligned(canvas, iTextSharp.text.Element.ALIGN_CENTER, new iTextSharp.text.Phrase(SubHeaderText, new iTextSharp.text.Font(baseFont, 8)), (page.Left + page.Right) / 2, page.Height - document.TopMargin - 5, 0);
            iTextSharp.text.pdf.ColumnText.ShowTextAligned(canvas, iTextSharp.text.Element.ALIGN_CENTER, new iTextSharp.text.Phrase(SubHeaderText, new iTextSharp.text.Font(baseFont, 8)), (page.Left + page.Right) / 7, page.Height - document.TopMargin / 2, 0);
            iTextSharp.text.pdf.ColumnText.ShowTextAligned(canvas, iTextSharp.text.Element.ALIGN_CENTER, new iTextSharp.text.Phrase(string.Format("Страница {0}", ++PageNumber), new iTextSharp.text.Font(baseFont, 10)), (page.Right + page.Left) / 2, document.BottomMargin, 0);
            iTextSharp.text.pdf.ColumnText.ShowTextAligned(canvas, iTextSharp.text.Element.ALIGN_RIGHT, new iTextSharp.text.Phrase(TimerText, new iTextSharp.text.Font(baseFont, 6)), page.Right - 10, page.Height - document.TopMargin / 2, 0);
        }
    }
    
    //http://metanit.com/sharp/tutorial/15.2.php
    //Классы для  таблиц сводных, помесячных и др.
    class _StatisticsURLTotal
    {
        public int _iD { get; set; } //Primary Key
        public string _myURL { get; set; } //URL
        public string _myDirection { get; set; } //category
        public double _myDirectionBytes { get; set; } //GB
        public double _myTime { get; set; } //hours
        public string _User { get; set; } //UserLogin
    }

    class _StatisticsURLEveryMonth
    {
        public int _iD { get; set; } //Primary Key
        public string _myDirection { get; set; } //category
        public string _myURL { get; set; } //URL
        public double _myDirectionBytes { get; set; } //GB
        public string _Month { get; set; } //Apr
        public int _Year { get; set; } //Year
        public string _User { get; set; } //UserLogin
    }
        
    class _StatisticsDirection
    {
        public int _iD { get; set; } //Primary Key
        public string _myDirection { get; set; } //category
        public string _myDiscription { get; set; } //discription of category
        public double _myDirectionBytes { get; set; } //GB
        public double _myTime { get; set; } //hours
        public string _User { get; set; } //UserLogin
    }

    class _StatisticsDirectionMonth
    {
        public int _iD { get; set; } //Primary Key
        public string _myDirection { get; set; } //category
        public string _myDiscription { get; set; } //discription of category
        public double _myDirectionBytes { get; set; } //GB
        public double _myTime { get; set; } //hours
        public string _User { get; set; } //UserLogin
    }

    class _StatisticsDirectionMonthFull
    {
        public int _iD { get; set; } //Primary Key
        public string _myURL { get; set; } //URL
        public string _myDirection { get; set; } //category
        public double _myDirectionBytes { get; set; } //GB
        public double _myTime { get; set; } //hours
        public string _User { get; set; } //UserLogin
    }

    class _StatisticsFull
    {
        public int _iD { get; set; } //Primary Key
        public string _Url { get; set; } //URL
        public double _Bytes { get; set; } //MB
        public double _Time { get; set; } //minutes
        public string _Direction { get; set; } //category
        public string _Month { get; set; } //Apr
        public int _Year { get; set; } //Year
        public string _User { get; set; } //UserLogin
    }

    class _MakeIni
    {
        StringBuilder sb = new StringBuilder();
        public void CreateIni()
        {
            sb.AppendLine(@"# ProxyAnalyser.ini");
            sb.AppendLine(@"# Author @RYIK 2016-2018");
            sb.AppendLine(@"# Дата обновления файла:  22.06.2018 23:39:16");
            sb.AppendLine(@"# Start of Configuration");
            sb.AppendLine(@"");
            sb.AppendLine(@"");
            sb.AppendLine(@";Direction.");
            sb.AppendLine(@"# URL по направлениям. Может быть несколько строк с одним направлением. URL Разделять разделять пробелом. ");
            sb.AppendLine(@"# Примеры:");
            sb.AppendLine(@"# xxx = xxx tits");
            sb.AppendLine(@"# xxx = xuy.com");
            sb.AppendLine(@"# microsoft = microsoft.com");
            sb.AppendLine(@"# Direction1 = URL1 URL2");
            sb.AppendLine(@"# Direction1 = URL3");
            sb.AppendLine(@"");
            sb.AppendLine(@"");
            sb.AppendLine(@"Common = microsoft.com windowsupdate.com microsofttranslator.com msn.com bing.com ");
            sb.AppendLine(@"");
            sb.AppendLine(@"# DigitalAds = adfox.ru c8.net.ua dt00.net st00.net .umh.ua adland.ru admaster.net adme.ru admitad.com admixer.net adpro.ua adriver.ru adrock.com.ua adru.net advbroker.ru advert advideo.com.ua alexegarmin.com alpha-alpha.ru avers.ua awalon.com.ua backromy.com banner.kiev.ua banner.ua begun.ru ");
            sb.AppendLine(@"# DigitalAds = bigbordi.com.ua brainberry.ua citysites.com.ua doubleclick.net elephantmedia.com.ua exoclick.com fabika.ru goodadvert.ru google-analytics.com googlesyndication.com holder.com.ua itcg.ua kolitat.com ladycash.ru lux-bn.com.ua luxup.ru marva.ru mediatraffic.com.ua michurin.com.ua mstarproject.com ");
            sb.AppendLine(@"# DigitalAds = openmedia.com.ua outdoor-city.com.ua post.rmbn.ru prime-group.com.ua reklama sellbe.com smi2.net sostav.ua spyboard.net trafmag.com videoclick.ru vidigital.ru vongomedia.ru vy-veska.com.ua yieldmanager.com ");
            sb.AppendLine(@"# DigitalAds = adsoftheworld.com e-promedio.pl images-amazon.com hbr-russia.ru nypl.org adwords.google.com e-stradivarius.net ctfs.ftn.qq.com picdn.net bambus.com.ua egonomik.com direct.yandex.ru nrb-development.com.ua dmonsters.ru goodadvert.ru snbr-stone.com ill.in.ua materials.crasman.fi ");
            sb.AppendLine(@"# DigitalAds = ggpht.com krutilka.net unipdfconverter.com e-ratings.com.ua bongacash.com likondok.com luxup.ru royaladvertising.ua kuruza.ua propellerads.com rontar.com eclipsemc.com dt00.net trafmag.com abcnet-srv1.mpsa.com biturboplus.org blogun.ru uacdn.org mediatraffic.com.ua scene7.com am15.net livesmi.com ");
            sb.AppendLine(@"# DigitalAds = dekoravto.com.ua restyling.in.ua rarenok.biz propellerads.com pix-cdn.org gpm-digital.com spyoutdoor.com gallerymedia.com.ua zassets.com karo.pk mi6.kiev.ua kaltura.com ");
            sb.AppendLine(@"# DigitalAds = syzygy.net skd-druk.com antbeeprint.com ooyala.com mediateas.com brightcove.com marketgid.com recreativ.ru comodoca.com mmr.ua nvjqm.com youshido.com api2.waladon.com adframesrc.com pay-click.ru wambacdn.net goodadvert.ru pay-click.ru .adocean. pix.eu.criteo.net ");
            sb.AppendLine(@"");
            sb.AppendLine(@"# Finders = google. gstatic.com yandex. meta.ua wikimedia. wikimapia.org wikipedia. bing.com rambler. yahoo. aport. .webalta.ru ");
            sb.AppendLine(@"NewsInfoAds = aol.com magnet.kiev.ua mariupol-express.com.ua marsovet.org.ua mgm.com.ua mreporter.ru msn.com yanukovychleaks.org news novaposhta.ua novias.com.ua novostimira. obozrevatel. online.ua otipb.at.ua paper. podrobnosti. polemika.com.ua popmech.ru pravda.com.ua pravmir.ru redtram russianmanitoba.ca segodnya. silauma.ru sinoptik slando.ua slon.ru smartbooka.net smi2.ru sn00.net stakhanov.org.ua supercoolpics.com telegraf.com.ua thawte.com theatlantic.com timedom.com.ua tochka.net tonis.ua translate.ru utg.ua tugraz uaprom.net ubc-corp.com ubr.ua ucdn.com ukrinform.ua unian.net unn.com.ua ustltd.com vesti.ru rbc.ua .ria.ua liga.net 163.com 112.ua pravda.com wunderground.com golos-ameriki.ru p-p.com.ua sdelanounas.in.ua tut.by vesti-ukr.com politeka.net ");
            sb.AppendLine(@"NewsInfoAds = vido.com.ua v-mire.com vremia.in.ua vtbrussia.ru webtrends.com xvatit.com yaskraviy.com zaxid.net zhitomir.info zirki.info znakiua.com korrespondent.net gazeta.ua synoptyc.com.ua bigmir.net censor.net.ua tvi.ua lenta.ru puls.kiev.ua pravo-kiev.com ts.ua fakty.ictv.ua 06239.com.ua kp.ru intv.ua companion.ua forbes.ru 1tv.com.ua kommersant.ua pinchukfund.org .ukr.net mmr.ua news.meta.ua ipress.ua sledstvie-veli.ks.ua lb.ua kompik.if.ua fakty.ua vashmagazin.ua ntv.ru novosti-n.mk.ua gorod.dp.ua 15minut.org aspo.biz vgorode.ua news.mail.ru vesti.ua dumskaya.net odessa-life.od.ua ukrgo.com glavcom.ua zik.ua delo.ua vz.ua 048.ua timer.od.ua m24.ru mr7.ru cinemaciti.kiev.ua ictv.ua politkuhnya.net uainfo.org mk.ru tsn.ua inter.ua uapress.info znaj.ua apostrophe.ua forexpros.com ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Images = riastatic.com makeagif.com fotocdn.net ");
            sb.AppendLine(@"");
            sb.AppendLine(@"SocialNets = 205.188.22.193 62.41.58.141 62.41.58.87 64.12.30.66 64.12.98.203 64.211.168.47 64.211.168.60 80.150.142.69 91.190.216.23 91.190.216.24 91.190.216.25 antimir.com.ua badoo.com blogger clubs.ya.ru disqus.com facebook fbcdn.net fbsbx.com fdating.com funs.djuice.ua gidepark.ru googleusercontent.com icq. lavra.spb.ru linkedin live.com liveinternet.ru livejournal love.mail.ru love.viagra.co.ua mad-ptah.com mamba.ru mamboo.com mirtesen.ru my.mail.ru mylivepage.ru ning.com onona.ua planeta.rambler.ru plusone.google.com privet.ru qip.ru skype. spasivdim.org.ua topface.com tumblr.com twimg.com twitter userapi.com vk.com vkontakte.ru vk.me plus.google.com vkadre.ru ");
            sb.AppendLine(@"SocialNets = .fbcdn.net odnoklassniki.ru moimir.org loveplanet.ru 24open.ru lovetime.com mylove.ru tourister.ru 217.20.153. 217.20.145. 217.20.157. sender.mobi intercom.io instagram.com presenta.xyz ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Mailers = accounts.google.com torba.com un.net.ua zakladki.ukr.net freemail.ukr.net smartresponder.ru ");
            sb.AppendLine(@"");
            sb.AppendLine(@"FileStores = akamaihd.net akamaized.net datalock.ru .grt01.com .s3.ua 176.37.57.50 199.91.154.33 213.199.179. 2file.net 37.220.161.181 46.165.200.111 46.98.66.174 4chandata.org 4put.ru 62.109.141.165 78.108.178.215 78.108.179.233 78.108.183.128 78.140.145.105 78.140.170.212 78.140.170.236 78.140.170.68 78.140.178.86 78.140.184.146 78.140.184.147 78.140.184.148 78.140.184.150 78.140.184.160 78.140.184.162 78.140.184.169 78.140.190.243 78.140.190.251 89.184.66.165 93.74.35.248 94.198.240.163 94.198.240.164 94.198.240.18 94.198.240.193 94.198.240.203 94.198.240.212 94.198.240.37 94.198.240.56 94.198.240.96 addthis.com adsua.com amazonaws.com cloudfront.net crl.entrust.net depositfiles. dotua.org dropbox e.mail.ru edgecastcdn.net edisk etsystatic.com fastcdn.me fastpic.ru file-cdn.com files.mail.ru fileshare.in.ua filestore.com.ua firepic.org forumimage.ru fotohost.kz freeshareloader.com fsimg.ru godaddy.com ");
            sb.AppendLine(@"FileStores = googleapis.com hotfile.com ifolder. leaseweb.net letitbit. loadup.ru mediaget.com onlinefilefactory.net vividlabz.com podvignaroda.mil.ru imageban.ru imageshack.us jkgbr.com keep4u.ru .mycdn.me hotcloud. .dropmefiles. storage .turbobit. kor.ill.in.ua 50.7.161.18 ");
            sb.AppendLine(@"FileStores = userfiles.me mkpages.epaperflip.com ollcdn.net pawidgets.trafficmanager.net piccy.info radikal.ru rapidshare. rghost.ru rusfolder.com savepic.net sdlc-esd.sun.com sendfile.su slickpic.com slil.ru tchkcdn.com tempfile.ru turbobit. uafile.com.ua unibytes.com uploaded.net uploads.ru verisign.com vimeocdn.com yimg.jp zakachali. api2.waladon.com foto.rambler.ru digitalua.com.ua savepic.org isok.ru media.adrcdn.com grt02.com sendimage.me edisk.ukr.net shutterstock.com ferrari-4me.weebo.it img.ria.ua files.namba.net disk.yandex.ru dfiles.ru sendspace.com rapidview.co.uk us.ua photo.torba.com fayloobmennik.net hotdisk.org rackcdn.com tttmoon.com getitbit.net filecdn.to vcdn.biz files.ukr.net jpe.ru toroff.net habrastorage.org join.me .d-cd.net photofile.ru picatom.com leprosorium.com ftp.havaswwkiev.com.ua googledrive.com wetransfer.com yousendit.com auto-media.com.ua 212.90.177.226 studioavtv.com.ua minus.com ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Shops = versialux.com.ua funforkids.ru .es.com.ua fashion4you.com.ua .hm.com jam.ua .mp.spb.ru mvp.com.ua price.ua 1001bilet.com.ua 4club.com.ua airis.spb.ru aklas.com.ua albion-books.com alkupone.ru ararat.kiev.ua art-oboi.com.ua atbmarket.com avecoffee.ua babystyle.com.ua barin.kiev.ua bauhauz.com.ua begemot.com.ua benuar.com.ua berislav.com.ua biglion biserok bodo.ua bonprix.ua bookshop.ua brasletik.com.ua bt-baby.com.ua cabinet.ua cafeboutique.com.ua camellia. cd-market.com.ua cheboturka.com.ua chicco.com.ua chmall.ru chytayka.com.ua clasno.com.ua cnd-shellac.com.ua daru.kiev.ua dekor-carpet.com.ua dividan.com.ua dogdevik.net ebay ed-mebel.ru e-komora.com.ua e-kvytok.com.ua emozzi.com.ua empik.ua firework.kiev.ua fisher-price.com.ua flagman.kiev.ua flashmeb.com food-ltd.com.ua galereya.com.ua gardini.net.ua gitara.in.ua gobelen.kiev.ua golka.com.ua groupon.com.ua hipp.ua hitime.com.ua hozdom i-drink.com.ua ids-service.com.ua ");
            sb.AppendLine(@"Shops = kidstaff kovroff.com.ua kupikupon.com.ua kupiskidku.com kupisuvenir.com.ua kuponi.com.ua ladyu.com.ua lemage.com.ua violity.kiev.ua viyar.kiev.ua vppmebel.com.ua v-ticket.com.ua vueling.com witt-international.com.ua wizzair.com yakaboo.ua yarema.ua kupinatao.com shket.com.ua gepur.com.ua mehovoy.com metro-group.com termocomplekt.ru zoostar.com.ua fabika.ru rakuten.co.jp igel.com.ua instrumentk.com.ua intertop.ua intimo.com.ua italiavogs.com kickstarter.org tu-tu.ru ufsa.com.ua ukanc.com.ua uniq.ua uplight.com.ua veshalka.com.ua vilonna.com.ua izumsky.com.ua pullandbear.net vintagetrends.com alibaba.com makeup-shop.com.ua stylesalon.com.ua vovabrend.com.ua tarelki.com.ua prom.ua smilefood.od.ua a.alicdn.com aliexpress.com gotoshop.net.ua ");
            sb.AppendLine(@"Shops = mamamia.ua market.tut.ua matraso.com.ua maxicard.ua mebelok.com mebelstyle.net meblium.com.ua megaskidki.com.ua mens-bag.com metro.ua miraton.ua modanadom.com moda-z.com modern.com.ua modnakasta.ua modna-shtora.ua moni.in.ua muzmania.com.ua muztorg.ua mvk-vostok.com.ua myline.com.ua my-office.com.ua my-watch.com.ua narbutas.com oringo.com.ua ozone.ru pafos.kiev.ua pamyatniki.net.ua papirus.com.ua parter.ua petrovka.ua plastics.ua podushka.com.ua pokupon poparada.com.ua posuda prikid.ua prizolov.in.ua promdesign.ua qrticket.in.ua reloading.com.ua rmigroup.ru robotun.com.ua roda.ua rollhouse.com.ua rondell.kiev.ua rukzak.ua samex. secunda.com.ua sewing.kiev.ua shoe-care.com.ua silpo.ua skidka.ua skidochnik.com.ua sn-style.com.ua soundmaster. style.aliunicorn.com sumki-dina.com.ua superdeal.com.ua sushi-anime.com.ua svitstyle.com.ua tanuki.ru tickets.ua tik-tak.ua time-casio.ru tivardo tk-textile.com.ua tripsta.com.ua ");
            sb.AppendLine(@"Shops = zakupka.com zdorovalavka.com.ua .parkflyer.ru trade-city.ua .bag24.com.ua vipbag.com.ua stilago.com.ua ergopack.ua tovaryplus.ru ukrpapir.com.ua faberlic-online.info bazilkandusupov.com ukrzoloto.ua tktimport.com veneto.ua shop.topsecret.com.ua hilt.com.ua plato.ua stiliaga.com.ua braggart.ua mir-maek.ho.ua xstyle.com.ua shopnow.com.ua koketka-online.com evora.ua centrofashion.ru elit-alco.com.ua napitokclub.net urbanstyle.com.ua asos-media.com kanapa.ho.ua neimanmarcus.com taobaocdn.com timeshop.com.ua bestwatch.com.ua v7kupon.com groupon-cdn.ru ricci.com.ua katalogkartin.com uagallery.com.ua fashion-online.com.ua basconi.com mizo.com.ua superdeal.com.ua posudaclub.kiev.ua welfare.ua ua.all.biz hello-kitty.kiev.ua euroenergo.biz banggood.com avia-booking.com e-travels.com.ua nanoprotec.ua vendors.com.ua luxlingerie.net.ua self-collection.com.ua yatego.com ujena.com bershka.net ");
            sb.AppendLine(@"Shops = individ.ua armored.com.ua conte-kids.by plazma.com.ua booklya.com.ua voda.com.ua bilethouse.com.ua chastime.com.ua itsell.com.ua gustosa.com.ua zvek.com.ua vcolec.com.ua parfums.ua sm-michel.com vanilla.kiev.ua obruchalka.com.ua orix-gold.com.ua zappos.com swarovski.com winefood.com.ua a-sky.in.ua mystyle.kiev.ua icaravan.com.ua tally-weijl.com 08.od.ua forus.com.ua topmall.ua 105.com.ua albertokavalli.com.ua avangard-time.ru imperio.kiev.ua belgusto.com.ua spreadshirt.com vipkupon.com.ua vashashuba.com.ua supermaiki.com ralphlauren.com goodwine.ua megavision.ua fatline.com.ua creativemama.com.ua fashionwatches.com.ua vramke.com.ua preta.com.ua ua.centrofashion.com mydnk.com setadecor.ua topshoptv.com.ua multivarka.pro derby.ua filter.ua med-magazin.com.ua moglee.com bee-pharmacy.com meblinovi.kiev.ua baldessarini.com futbolki.dp.ua reglan.com.ua stamps.kiev.ua mfest.com.ua elitebrand.com.ua olx.ua ");
            sb.AppendLine(@"ShopDigital = .bt.kiev.ua .mo.ua .y.ua 5ok.com.ua agsat.com.ua allo.ua alloxa.com antenka.com.ua apple.com aukro.ua avgold.ru cezar.ua citrus.ua city.com.ua comteh.com deshevshe.net.ua e-katalog fotomag.com.ua fotos.ua foxtrot.com.ua goods.marketgid.com hotline hotprice.ua i-m.com.ua itbox. klondayk.com.ua kpiservice.com.ua magazyaka.com.ua megabite.ua metamarket.ua mobilluck.com.ua mobiset.ru mobitrade.ua nadavi.com.ua notus.com.ua pcshop.ua protoria.ua repka.ua roks.com.ua rozetka satmaste sokol. sotmarket.ru strobist.ua stylus.com.ua technoportal.ua tehnohata.ua torg.alkar.net ukrshops.com.ua vcene.ua avic.com.ua technopolis.com.ua bosch-home.com.ua slinex.kiev.ua stockmobile.ua intermobil.com.ua fotos.com.ua comfy.ua foxmart.ua shop-gsm.net nofelet.in.ua siemens-home.com.ua ");
            sb.AppendLine(@"ShopBuild = .nl.ua 1giper.com.ua accbud.ua agromat.ua altherm.com.ua aney.com.ua bau. bioplast.ua bitovki.kiev.ua bprice.ua brille.ua dizajio.kiev.ua document.ua dokamin.ru dvernik.com.ua ekodveri.in.ua ekonom-remont.com.ua ibud.ua ideidetsploshad.info instrument.in.ua kamni-market.com keramida.com.ua konkurs.ru krainamaystriv.com lampa.kiev.ua liko-holding.com.ua muratordom.com.ua novalinia.com.ua okna.ua proekty.ua promobud.ua proxima.com.ua rabotnik.kiev.ua spectr.kiev.ua stroimdom.com.ua stroymart.com.ua tehnikaokna.ru truba.ua tvoydom.kiev.ua viknadveri.com zaglushka.ru termocom.ru bul-market.com.ua 3dklad.com beton.kovalska.com knauf.ru xn--80a1agg3a.com.ua keramdev.com.ua kupiplitku.com.ua germes-studio.kiev.ua metall-ks.com.ua 3208.ru hunterdouglas.com san-tehnika.com.ua feeder.kiev.ua ekodom.net.ua akm.kiev.ua kamelotstone.ua infohome.com.ua pufic.com.ua rollstroy.narod.ru autonomenergo.com.ua lesprom.kiev.ua perestroika.com.ua praktiker.ua luminaelit.com.ua gunter-hauer.ua mebli-zakaz.kiev.ua ");
            sb.AppendLine(@"ShopBuild = epicentrik.info santehtop.com.ua ceramica.ks.ua maxus.com.ua infohome.com.ua dveri-pol.com.ua zametkielectrika.ru e-1.com.ua balon.kiev.ua voltweld.com promsvarka.com domsvarki.lg.ua in-green.com.ua ");
            sb.AppendLine(@"ShopRieltor = lipinka.com.ua .est.ua .fn.ua .lun.ua 100realty.ua 3doma.ua address.ua appartament.kiev.ua bfontanov.com.ua blagovist.ua chayka.org.ua comforttown.com.ua concord.in.ua country.ua dobovo.com dom.ria.ua dom2000.com dom9.kiev.ua domik.net domproekt.kiev.ua dom-z.com.ua eastbooking.ua elitgrup.com.ua estater.biz etag.com.ua friendsplace.ru home-poster.net hotel kvartiravkieve.com kvartorg.com mdigroup.com.ua megamakler.com.ua meget.kiev.ua miete.com.ua mirkvartir.ua mistechko.com.ua most-city.com novakvartira.com.ua ozimka.com parklane.ua promap.ua prostodom.ua realt rieltor.ua v-irpen.com vkvartir zhitlo.in.ua kanzas.ua prestigehall.com.ua novbud.com.ua richtown.com.ua perlina-kiev.com.ua evrodim.com zeleniykvartal.com.ua bgm.kiev.ua zirka-dnipra.com.ua fn.ua kadorrgroup.com cheremushki.od.ua lun.ua dbk4.com.ua zhk.org.ua 7sky.od.ua levitana.com.ua novostroy.od.ua prazhsky.com.ua kmb-sale.com capital.ua panovision.com.ua mariinsky.com.ua cottage.ru zolotoybereg.com doba.ua oneday.ua ");
            sb.AppendLine(@"ShopRieltor = lunnovostroyki. vnovostroike.com.ua quote-spy.com l-kvartal.com.ua kulumok.kiev.ua b-l.org.ua club-bl.kiev.ua");
            sb.AppendLine(@"ShopBoutiq = z95.ru antoniobiaggi.com.ua b-1.ua bicotone.com.ua brocard.ua butik.ru carlopazolini.com chanel.com cop-copine.com dioriss.com.ua enna-levoni.com etam.com fashionavenue.com.ua gold.ua incanto.ru kuz.ua lanett.ua leboutique.com lediamant.com.ua multi-butik.com red.ua unona.ua zapatos.com.ua zara.net mango.com hm.com daniel.kiev.ua victoriassecret.com topsecret.ua helen-marlen.com pierrecardin-ukraine.com joma.com.ua wittchen leboutique deezee.pl issaplus.r.worldssl.net flashsale.chia.ua ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Banks = rbc.ru finance.ua vab.ua adspynet.com aval.ua bank finline.com.ua fuib.com kruss.kiev.ua nadra.com.ua portmone privat24.ua pumb.ua unicredit 24nonstop.com.ua rsb.ua usstandart.com.ua minfin.com.ua quote-spy.com ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Profiles = asmap.org.ua 4konverta.com balance.ua buhgalter capitaltimes.com.ua ipk-dszu.kiev.ua kurs.com.ua ligazakon.ua nibu.factor.ua vobu.biz vobu.com.ua ua-auto.com.ua krbizn.com mtsbu.kiev.ua udai.kiev.ua ivc.in.ua otomoto.pl.ua master-d.com.ua ligazakon.net ");
            sb.AppendLine(@"Profiles = smarttender.biz ");
            sb.AppendLine(@"");
            sb.AppendLine(@"HelthSportBeauty = .nba.com 11na11.com 5el.com.ua amrita-ua.pp.ua avon basket.com.ua beintrend.ua championat.com chernomorets dress-code.com.ua dynamo elle.ru fashiontime.ru feelgood.ua football gorodokboxing.com inessa-salon.com.ua jlady.ru kr-zdorovia.com.ua lidiko.com.ua london2012.com lumenis.com.ua makeup.com.ua manutd marykay master-hairstyles.ru master-pletenij.ru median.kiev.ua m-kay.kiev.ua nevrologia.far.ru ngenix.net omorfia.ru oriflame poozico.com rubasket.com sbnation.com shidnycia.com snowboarding. sport synevo.ua terrasport.ua terrikon.com veliki.com.ua veloonline.com velostyle.com.ua ");
            sb.AppendLine(@"HelthSportBeauty = veritas.in.ua wella.com .jv.ru ya-modnaya.ru yves-rocher.ua zefir.ua bet365.com luxoptica.ua ecolab.kiev.ua glossary.ua footboom.com championat.net footclub.com.ua etgdta.com anastasia.net fcdnipro.ua allboxing.ru kiki.sumy.ua gooool.org medsovet.info marathonbet.com extremstyle.ua parimatch.com s5o.ru williamhill.com futbik24.com fc-anji.ru danabol.com.ua cosmopolitan.ru cosmo.ru kosmo.ua ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Ittech = skachatbesplatnyeprogrammy.ru home-soft.com.ua ixbt.com lg.com 1c-bitrix-cdn.ru 3dmir.ru afo.kiev.ua artlebedev.ru china-review.com.ua chip.ua exler.ru gagadget.com htc.com ibm.com iphones.ru iptelecom.net.ua kyivstar.ua lanzone.info lifehacker.ru lingvo.ua mobile-review.com mts.com.ua multitran.ru online.dynamics.com orfo.ru romka.eu samsung.com smartphone.ua softstudio.ru sql.ru tehnoarhiv.ru volia.com esetnod32.ru free-pdf-tools.ru ho.ua jimdo.com macromedia.com mcafee.com adobe mob.ua mozilla opera.com.ua oracle.com paint-net.ru templatemonster.com true.nl download.cdn.mozilla.net autodevel.com bemobile.ua samsung.brawnconsulting.com itc.ua life.ua download.adobe.com qtrax.com wordpress.com wix.com extrimdownloadmanager.com ua.uar.net sipnd.com android-plus.ru android.wildmob.ru android-app.ru svyaznoy.ru mob.ua weebly.com inkfrog.com keddr.com microsoft.com autodesk.com mobile-review.com smartphone.ua itcg.ua samsung.com jimdo.com oodrive.com neulion.com pgp.com ");
            sb.AppendLine(@"Ittech = top-android.org get4mobile.net 4pda.ru photoshop-master.org for-foto.ru eltel.net delfi.ua luxhard.com intertelecom.ua docspal.com 77.88.210.226 logitech-viva.navisite.net speedtest.hatanet.com.ua vsassets.io githubusercontent.com visualstudio.com vo.msecnd.net python.org redhat.com windowsupdate.com update.microsoft.com");
            sb.AppendLine(@"");
            sb.AppendLine(@"LookForAJob = hh.ru hh.ua job rabota trud work");
            sb.AppendLine(@"");
            sb.AppendLine(@"VideoTV = fbvkcdn.com .kinokrad.net .ovva.tv .fs.ua .vtm.be ytimg.com .zerx.ru 109.68.40.68 109.70.232.147 13-e.ru 173.44.34.108 173.44.34.109 194.190.77.133 194.190.77.177 1tv.ru 24tv 3gpfilm.net 78.108.178.203 79.142.100.23 79.142.100.32 91.197.128.34 allserials.tv autopark.tv cdn.ua cochrane.wimp.com dailymotion.com data.intv.ua dmcdn.net flv.bigmir.net fx-film.com.ua good-zona.ru kino kwcdn.kz lilotv.com livetv lovi.tv magnolia-tv.com watch.online.ua megalife.com.ua megastar.in.ua megogo.net mggcdn.net moova.ru movie my-hit. myvi. ntvplus.ru online-24-7.ru openfile.ru play.ukr.net pulta.net rovenkismi.com.ua rutube.ru scifi-tv.ru serialsonline.net smotri.com stopnegoni.ru streamcdn.eu .shtorm.com gidonline youtube.com .twitch.tv bonus-tv.ru ");
            sb.AppendLine(@"VideoTV = media video khabar.kz moonwalk. testlivestream.rfn.ru thespace.org tikilive.com tours-tv.com turner.com tushkan.net tvigle.ru tvzavr.ru ujena.tv ustream.vo.llnwd.net vd-tv.ru videa.hu video vimeo.com vimple.ru vzale.tv webtv.moldtelecom.md whitecdn.org itv.com youtube.com freeetv.com justin.tv aliez.tv media.trkua.tv neulion.net filmix.net media.ntv.ru mover.uz tfilm.tv ovg.cc films-online.su veterok.tv novatv.bg kewego.com multfilmi.at.ua kiniska.com minizal.net spruto.tv liveleak.com clipsonline.org.ua pbh2.com damiti.ru tvbest.net pteachka.ru ustream.tv divan.tv ukrlife.tv bambuser.com portall.tv media.stb.ua megogo.net thesame.tv vidyomani.com planeta-online.tv kintavr.ru kinogo. .kaban.tv Ex-fs.net ivi.ru baskino 37.220.36.40 rutube.ru ");
            sb.AppendLine(@"VideoTV = 91.234.34.154 91.234.34.136 pdbcdn.co hlsvod.rambler.eaglecdn.com 31.28.163.146 .kaltura.com 185.38.12.41 .stb.ua apollostream.xyz lanet.tv 50.7.128.107 ovva.tv 37.220.39.62 185.38.12.50 185.38.12.48 ");
            sb.AppendLine(@"");
            sb.AppendLine(@"GPSCar = 194.247.12.35 200stran.ru aerosvit.ua airarabia.com dnepr-oblast.com.ua gunsel.com.ua istrim.com mapia.ua maps openstreetmap.org visicom navitel.su delivery-auto.com flyuia.com map.meta.ua");
            sb.AppendLine(@"");
            sb.AppendLine(@"AutoClubs = .okyami.net .rst.ua .rul.ua .vodiy.kiev.ua astra-club.org.ua autocentre.ua autoconsulting.com.ua autoportal.ua autoreview.ru avtopoligon.info c3-picasso.ru cars.ru drag402.com drive.ru drive2.ru ducati.kontain.com forum.2108.kiev.ua mad4wheels.com manualedereparatie.info mini.ua mitsubishi-club.org moto oktja.ru privat-auto.info sti-club.su topgearrussia.ru topgir. turbo.ua uavto. vladislav.uа vvm-auto.ru zr.ru kia-club.com.ua offroadclub.ru ukraine-trophy.com auto.mail.ru 3dgarage.ru autolines.org.ua help-on-way.ru forum.autoua.net gazel-club.com.ua e30club.ru a2goos.com bodybeat.ru reviews.drom.ru ");
            sb.AppendLine(@"AutoClubs = forum.vodila.net skoda-club.org.ua autowp.ru autobild.by nissan-club.org j-cars.org j-cars.in.ua kostructor.altervista.org aveoclub.info hexagon.narod.ru gazellnext.ru autoevolution.com indianautosblog.com auto.mail.ru retro-avtomobili.net autoplus.su cfts.org.ua ujena.com ua-auto.com.ua youcar.com.ua auto.ria.com auto.mail.ru automps.ru gaz-club.com.ua oldfordclub.net getcar.ua ");
            sb.AppendLine(@"Cars = .lada. nissan-single.com.ua .ferrari. .infiniti. .lu.com.ua .uaz.ru ais.com.ua ais-avto.com.ua ais-market.com.ua .skoda-auto. alfaromeo-ukraine. americanfleet.com.ua atlant-m.in.ua audi auto-planeta.com.ua avtobazar.ua avtoport-kiev.com.ua avtosojuz.ua awt.com.ua bentleyconfigurator.com bmw. cadillac.com chery.net.ua chevrolet citroen. dodge.com.ua ducati-russia.ru euroavto.in fiat ford.com gaz. geely honda hyundai infinitiusa.com infocar. jaguar. kia. lacetti.com.ua lancer.com.ua landrover. lardi-trans.com lexus. maserati.com.ua mazda. mercedes niko.ua niko-ukraine. nissan.eu nissan.goloseevsky.com nissan-vidi. opelukraine. oskar.odessa.ua autogidas.lt ");
            sb.AppendLine(@"Cars = pickup-center.ru planetavto.com.ua porsche. praga-auto.com.ua renault subaru-vidi.com.ua sy. toyota uavto.kiev.ua uaz4x4 vidi-automarket.com.ua volkswagen. winner.ua winnerauto.ua autoutro.ru abw.by ukravto.ua faw.com.ua rstcars.com citroen-center.com.ua ford.ua byd.ua mg.co.uk greatwall-ukraine.com bogdanauto.com.ua nissan.ua landrover-vidi.com.ua eurocar.com.ua zaz.ua msk.obuhov.ru new.skoda-auto.com baz.ua avtek.ua autoline.com.ua cadillac-ais.com automir.com.ua infiniti-vidi.com.ua nissan-moscow.ru usedauto.com.ua autozaz.kiev.ua kievskoda.com ais-kiev-dnepr.com.ua gazlux.com gazgroup.ru avtobazar-ukraine.com.ua polycar.com.ua carsontheweb.com ");
            sb.AppendLine(@"Cars = omega-auto.biz bulavka.ua autobazar.od.ua inter-auto.com.ua otomoto.pl avtorinok.ru autobum.in.ua jeep.ua avtopoisk.ua rst.ua sauto.cz suchen.mobile.de cars.auto.ru ssangyong.ru sweet-auto.com.ua nextcar.ua sollers-auto.com newcars.ua m1.ua.f6m.fr fordodessa.com ford-vidi.com.ua edem-auto.com.ua orient-uaz.ru daihatsu-dias.com.ua kia-kiev.com.ua paritet.com.ua autocredit.com.ua ssangyong-irbis.ru autopark.od.ua m1.ru.f6m.fr vidi-autocity.com avtosale.ua volkswagen-rivne.com aispolis.com.ua bus.ru infiniti-lab.com.ua gm-avtovaz.ru atlant-m.spb.ru scania.ua ffclub.ru mazdadb.com autosite.com.ua peugeot suzuki. tivoli. subaru.ua autotrade.com.ua vis.iaai.com");
            sb.AppendLine(@"AutoLogistika = estafeta.org avtologistika.com 1move.com auto-partner.net spincar.com ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Parts = vsedlyaavto.kiev.ua avto-diski.com.ua autokovri.com.ua shop.omega-auto.biz .mao.in.ua 3dtuning.ru ais-sp.com.ua autoklad.ua avtoplaneta.com.ua avtozvuk.ua axxa.com.ua baltkam.ru brcgasequipment.ua car-care.com.ua castrol.com chiptuner.ru elcats.ru elit.ua elit-tyres.com.ua vsedoavto.com.ua avtomaler-plus.com.ua b2b.ad.ua ");
            sb.AppendLine(@"Parts = erisin.com exist.ru exist.ua ford-chrom.com.ua gaz-car.ru injapan.ru interlight.biz ip-auto.com.ua japancats led-svet-drl.com losk.ua mannol market.autoua.net masterniva.ru mrcap.com.ua nashashina.com.ua neoriginal.ru polarisind.com radial.com.ua razborki.com shell.com teamparts.ru gazdetal54.ru r-avto.kiev.ua 130.com.ua ");
            sb.AppendLine(@"Parts = tuning-market.od.ua tyretrader.com.ua unit-9.ru vse-o-pokryshkah.ru wheelhunter.com.ua zapadpribor.com zavoli.com.ua sgauto.com.ua catalog.autotechnics.ua avtoparts.com.ua city-auto.com.ua agrosoyuz.com am-servis76.ru 412345.ru uaz-upi.com avtoall.ru auto-sklad.com zapchasti.ria.ua autoprofi73.ru cartuning.in.ua ");
            sb.AppendLine(@"Parts = sherpa-auto.ru point.autoua.net daihatsu.at.ua:rezina.cc auto-light.com.ua avtoradosti.com.ua belcard-grodno.com autodealer.ru konsulavto.ru luxshina.ua shyp-shyna.com.ua bus-comfort.com.ua autoplaz.com.ua pereoborudovanie.com.ua automillenium.com.ua automaidan.com.ua obhivka.com autostyle.zt.ua detal-komplekt.ru ");
            sb.AppendLine(@"Parts = avtobox.com.ua luxsto.com.ua dio.kiev.ua china-shop1.com rdrom.ru carid.com dekoravto.com.ua tuninga.com.ua carmanauto.ru dekoravto.com.ua restyling.in.ua intercars.eu rezina.cc vladislav.ua opletka.net avtika.ru autogas.in.ua soundplanet.com.ua fordfocus.com.ua pixtinauto.ru vazinj.com.ua kolesiko.ua ");
            sb.AppendLine(@"Parts = azvuk.ua kingauto.com.ua premiorri.com aksavto.com.ua emgrand-shop.com rezina13.com.ua kolpak.com.ua autoshini.com shinaplus.com kayaba.com.ua shinservice.ru avtocomfort.com.ua all4cars.com.ua avto.pro market.ria.ua autoscan.com.ua electroshemi.ru ultrastar.ru garazh.com.ua gazok.in.ua razborkabmw-e39.ru ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Teach = fpk.in.ua greenforest.com.ua krok.edu.ua yappi.com.ua lvduvs.edu.ua englisher.com.ua classroom.com.ua languagefree.narod.ru mti.edu.ru window.edu.ru kname.edu.ua gai.ua intuit.ru kneu.edu.ua academic.ru .edu.ua");
            sb.AppendLine(@"");
            sb.AppendLine(@"MusicRadioonline = .galaradio.com .zf.fm 101.ru 109.120.141.174 109.120.141.181 109.234.154.100 109.234.154.119 109.234.154.194 109.234.154.29 109.234.154.30 188.75.223.31 188.93.17.187 188.93.19.234 195.66.153.17 195.95.206.17 195.95.206.214 212.115.229.83 212.26.129.2 212.26.129.222 212.26.146.47 217.171.15.155 217.20.164.163 46.4.98.119 5.79.69.115 62.80.190.246 77.47.134.32 78.159.122.138 79.98.143.194 83.142.232.246 91.201.37.43 91.202.73.76 91.214.237.247 91.214.237.248 91.220.157.3 92.241.191.100 95.81.162.158 akadostream.ru bandcamp.com batzbatz.com clubomba.com europaplus.ua flypage.ru fm.odtrk.km.ua froster.org get-tune.net ");
            sb.AppendLine(@"MusicRadioonline = glob.radiogroup.com.ua globaltranceinvasion.com hitfm. hitru.ru ipfm.net iplayer.fm kissfm.ua lux.fm media.brg.ua media.fregat.com megalyrics. miloman.net molode.com.ua moskva.fm music musvid.net muzebra.com muznarod.net muzofon.com myzuka. ololo.fm optima.fm podfm.ru radio retro.ua rferl.org ringon.ru rorg.zf.fm rpfm.ru setmedia.ru sky.fm snimi.tv soundcloud.com stream.kissfm.ua stream-1.k26.ru tavrmedia.ua thankyou.ru uhradio.com.ua uplink.duplexfx.com zaycev.net sc-atr.1.fm icecastlv.luxnet.ua loungefm.com.ua mixupload.com mixcloud.com muzofond.org zf.fm ");
            sb.AppendLine(@"");
            sb.AppendLine(@"EBooksMagazine = e-reading.org.ua flibusta.net issuu.com lib.ru phoenixcenter.com.ua cbs3vao.narod.ru bookclub.ua");
            sb.AppendLine(@"");
            sb.AppendLine(@"Totalizatos = maxiforex.ru fox-manager.com.ua masterforex-v.org tradernet.ru mavrodi marathonbet.com betcityru.com anyoption.com vo3tok.biz vostok3.com criteo.net superbinary.com ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Funny = .irc.lv .lah.ru .msl.ua .pnz.ru .uku.com.ua .yimg.com 15rokiv.novy.tv 1zoom.ru 2bobra.com.ua 2k.ua 3karasya.com.ua 4tyres.ua 78.com.ua 99px.ru admiralclub.com.ua adsence.kiev.ua aeroboat.ru afisha.mail.ru akamaihd akkord-tour.com.ua alick666.kiev.ua allelitepass.com allwatch.kiev.ua altantour.com anekdotov.net anextour.com antclub.ru antratsit.net aquafanat.com.ua aquafisher.org.ua aquapark.bg artek.ua artleo.com arutoronto.co.uk asianculture.ru autoprazdnik.ru autowalls.ru avan-ti.net avmc.com.ua avsim.su babai-family.com.ua babyplan.ru postnext.com std3.ru bit.ua vimka.ru spaces.ru spac.me ");
            sb.AppendLine(@"Funny = bastet.in.ua batona.net bayka.info bazi-otdiha.com.ua behance.net bemarry.com.ua berloga.net bestin.ua bestpozitiv.ru best-wedding.com.ua betradar.com bianca-lux.ru bigpicture. bilshe.com bingo.ua bitgravity.com blin.com.ua blockbuster. blogspot.com bobx.com booking.com boranmare.com bts.aero bugaga.ru bukovel.com buro247.ru butterfly.vn.ua buzzfed.com byaki.net canarsky-forum.ru cardsgif.ru car-ups.com channelingstudio.ru chevy-rezzo.narod.ru chistueprudi.ru cinemagraphs.com cityclub.kiev.ua cityfrog.com.ua clever-eyes.com.ua cmexota. stereoplaza.com.ua forum. tutkryto.su gorod.cn.ua userfiles.me ");
            sb.AppendLine(@"Funny = dakotapub.com darina.tv dedmoroz.ru deluxesound.com.ua demotivator deti.mail.ru dipserv.com.ua dirty dnepr.com docker.com.ua dofiga do-gazdy.com.ua dogcat.com.ua dominospizza. domskidok.com doodoo.ru dormstormer.com doroga.ua doseng.org dragobrat-go.com dreamtown.ua dsa-travel.com durdom.in.ua dusia.telekritika.ua dwor-rychwald.pl dyachenko.kiev.ua dyvosvit.ua edimdoma.ru effectfree.ru elementdance.ru elstile.ru esenin.kiev.ua esquire.ru eva.ru evilnight.ru fanfabrika.novy.tv fashion-mix.ru favbet.com feerie.com.ua films-iphone.com fionatravel.com.ua fishing fishki flickr.com superfiles.me ");
            sb.AppendLine(@"Funny = fotochumak.com fotofilmi.ru fotomania.in.ua fototelegraf.ru fotozefir.com.ua fresher.ru funik.ru funny garage21.com.ua gardena.com garriphotoman.pp.ua gartourkonkurs.net geometria.ru gifzona giphy.com gloss.ua goodfon.ru gorets-media.ru gorockop.ru gport.com.ua gradient.cx gradiva.com.ua grandmaideas.com graniart.ru graphics.in.ua gravure-idols.com groupon. gut.ru havana-club.com hawaii-kirillovka.com hd.at.ua hero2012.ru histoiredeshalfs.com hobbydelux.com hohota. horo.mail.ru horoscope hottours.in.ua husky.co.ua hwb.com.ua ibigdan.com ibrovary.com ifun.ru il-patio.com.ua menunedeli.ru thisispivbar.ua ");
            sb.AppendLine(@"Funny = jino.ru jongoo.net joyreactor.cc jphip.com kaifolog.ru kalinka-malinka.com.ua kamelek.com kanzas.ua karaoke.ru karavan.com.ua karpela.com katran-club.com.ua katysha.com.ua kirillovka.su klopp.ru klouny.kiev.ua klukva.org kolesogizni.com kolyan.net kontinent-card.com.ua korchma.kiev.ua korefun.net koroli.kiev.ua korsun.ic.ck.ua korzik.net kotomatrix.ru krabov.net kraina-ua.com kuda.com.ua kuda-ugodno.ua kundalini.com.ua kvitochka.kiev.ua kyxarka.ru leopark.ua lider-bk.com.ua lifeglobe.net look.com.ua lookatme.ru lostworld.com.ua lottery.com.ua loviskidki.com.ua luckyfisher.com.ua ochevidets ");
            sb.AppendLine(@"Funny = luxlux.net luxtv.ua lvivske.com maestro-travel.com.ua mafia.ua mainpeople.ua makuha.ru malva-tour.com.ua mamajeva-sloboda.ua matriarchat.ru mcdonalds.ua mediablender.com.ua mediananny.com memorial.kiev.ua menu.ru migalki.net miph.info mir-animasiya.ru mir-idei.com.ua mirprazdnika.kiev.ua mirvkartinkah.ru mkpages.epageview.com mnogo-idei.com modelist-konstruktor.com monk.com.ua moreleto.com.ua muzey-factov.ru myfishka.com nairi.com.ua nash.com.ua nasha-karta.ua nashaplaneta.net nashpilkah.com.ua nastol.com.ua nataliakabliuk.com nethouse.ua netlore.ru nevozmozhnogo.net ochepyatki.ru cameralabs.org ");
            sb.AppendLine(@"Funny = ngoboi.ru nibler.ru nice-places.com nightparty.ru nocookie.net nudistam.com oboffsem.ru oceanplaza.com.ua ochi.com.ua odessaguide.net ohoter.ru olivertwist.com.ua orakul. originaloff.com.ua orion-intour.com osinka.ru otpusk.com outshoot.ru packpacku.net panoramio.com parkkyivrus.com partsukraine.com.ua passion.ru pattayaphotoguide.com pegast.com.ua pepe.com photo. photoe.kiev.ua photovolkov.com.ua pikabu.ru pikch.ru pipec.ru pirojok.net pitchforkmedia.com pivarium.com.ua pizza. pizza33.com.ua pizza-celentano.kiev.ua playcast.ru poetryclub.com.ua porjat porter.com.ua sweetbook.net chocoapp.ru ");
            sb.AppendLine(@"Funny = premierworld.com.sg pricheska-kiev.com.ua prikol princessyachts.com prjadko. prochan.com coraltravel creative.su cruze-club.com.ua crystalhall.com.ua cveti.ucoz.ua d3.ru incz.com.ua io.ua irecommend.ru italia.com.ua ittour.com.ua izum.ua jazz.koktebel.info jetsetter.ua versal-online.com.ua vetton.ru vinbazar.com vip.vn.ua virtual.ua vishivay.ru visualization.com.ua bacchusclass.com baginya.org banisauni.com.ua conviva.com cool-birthday.com copypast.ru collie-merrybrook.com studia.kiev.ua nezabarom.ua fotki imgur.com pozdravlenye.com verdiktor.net lurkmore ochepyatki.ru slivki24.club ltu.org.ua ");
            sb.AppendLine(@"Funny = puzatahata.com.ua raduga-club.org raffaello.net.ua ragu.li raskraska.com re-actor.net redbull.com redigo.ru redtubefiles.com relax restaurant-esenin.ru reston.com.ua restoran-stop.com.ua rodynnefoto.com.ua rolandus.org route66.com.ua roxyclub.kiev.ua rtamada.kiev.ua rulez-t.info rusforum.ca rybalka rybinsk20.narod.ru saeco.de sastattoo.com scalemodels.ru schastie.kiev.ua serebro-rmb.com sezon-rybalki.com.ua shopaholic.kiev.ua shtormovoe.crimea.ua skeletov.net skybar.ua snasti.com.ua spankwire.phncdn.com spletnik. starer.ru starlife.com.ua starlightmedia.ua nevsedoma.com.ua anwap.org shutterstock.com ");
            sb.AppendLine(@"Funny = studio37th.com studio-moderna.com sunny7.ua surfingbird.ru sushi-nadom.com.ua sushiya.ua tarantino.com.ua tarhankut.ucoz.ua tastesgood.ua tattoomakers.com.ua teleblondinka.com teplitsacafe.com teztour theadventuresofteamhiemstra.com themeparkreview.com the-village.ru time2eat.com.ua today.kiev.ua toget.ru tophotels tourpalata.org.ua travel.ru trinixy tripadvisor.ru trostyan-rezort.com.ua tunersandmodels.com turbina.ru turne.com.ua turpravda.com turtess.com tury.ru ua.igru-film.net ucoin.net uoor.com.ua urod.ru uti-puti.com.ua vadim-grinberg.com vasi.net vasilkov.info vashapanda.ru trofey.net ");
            sb.AppendLine(@"Funny = vitalstorage.info viva.ua voboyah.com vodka-bar.com.ua voffka.com vogue.ru vokrug.tv vokrugsveta.com votrube.ru voyage.kiev.ua vsyako-razno.ru wallpapermania.eu wallpapers watch.ru webpark.ru wedlife.ru wetravelin.com wizardcamp.com.ua woman.ru wooms.ru xameleon.club300.com.ua xa-xa.org xkc.com.ua xn--80aqafcrtq.cc ya1.ru yaicom.ru yaki.com.ua yapfiles yaplakal yaremcha.com.ua zagony.ru zapilili.ru zazuzoom.com.ua z-d.com.ua ziza.ru zooclub.com.ua zoo-flo.com zooforum.ru woman.ua 1001mem.ru ruchess.ru gfycat.com fanat.ru yarovikov.ru vengrija.com.ua vbios.com img.com.ua bogolvar.com.ua 1ua.photos fotoaz.com.ua ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Porno = sxgirls.net sweet-girl.su sex3.com putana.cz lurtik.net babestar.ru myero.su erotic-foto.net titya.ru erodomen.ru pr.nsimg.net youngincest.net jjgirls.com familyfuck.org mofos.com 18firstsex.com ybanda.com yobt.com zhestkoeporevo.net porn1xa.net foto-zhenshin.ru kordonivkakino.net 37.72.170.58 overthumbs.com poimel.me ero-pixe.com fap-foto.com ero-pixes.com .xvideos.com .adult poxot.net poxotcdn1.ru ");
            sb.AppendLine(@"Porno = mofos.com .pinkrod.com .rk.com updatetube.com 18kitties.com 18teencore.net 21sextury.com 4tube.com 78.140.136.196 78.140.136.197 78.140.136.198 78.140.181.76 8teenies.com 91.83.237.41 94.242.252.77 absolutesuccess.su analbreakers.com angelsnu.com anilos.com babesandstars.com babesmachine.com babi.su babushky.ru banan.in bananateens.com bravotube.net brazzers.com brbpics.com cocku.net deffki.su deviantclip.com dojki.com dreamfilth.com empflix.com erotikax.ru erovid.org exgirlsss.org exposedwebcams.com fotofaza.com fovoritki.com free-abbywinters.com fuckday.ru galleries.payserve.com ");
            sb.AppendLine(@"Porno = galleryarea.com gallsforpleasure.com girlstop.info glamursgirls.ru hardsextube.com hornygf.net inferalton.com innocentcute.com juicyads.com karups kashtanka.com lustyguide.com massage-bagira.com.ua mature-beauty.com maturegoldenladies.com maw.ru MAXIM minuet.biz modelsnu.com mybabes.com myshyteens.com nagishom.org naked nastyteens.net nubiles.net nude nudist-colony.org NUTS nylonx.net onlyamateursteens.com osiskax.com prorvasex.com pussycash.com xxx tits porno xuy.com paikry7.narod.ru penthouse pinkmature.com playboy podlectube.com pokazuha.ru popka. porn powersex.ru ");
            sb.AppendLine(@"Porno = realitykings.com redtube.com runetki. seks sensualgirls.org sexa. sexy shufuni.com solokittens.com soscka.ru spermian.com spy2wc.org teen-angels.org teenartclub.net teenport.com theteens.org tinysolo.com tnaflix.com trahun.tv tube8.com tusnya.net tygiepopki.com ubka.zadniza.com upskirt video-girl.tv vidz.com v-razvrate.org wetplace.com mybestfetish.com xvideo XXL XXX xyu.tv xyya.net yellowmedia.biz yobt.tv youjizz.com young-n-fetish.com yourlust.com suero.tv erotixkachky.nestkwell.ru ero-x.com brazzers.com chastnoe.net flv.pteranoz.ru lopso.net ruwrz.ru foto-golykh.ru sex.borzna.com ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Games = .war 11x11.ru 188.93.63.90 85.214.84.91 bigler desert-operations. flashhome.ru game inviziads klanz.ru mir-stalkera.ru myazart.com playground.ru romadoria.ru thesaints.info travian uo.net.ua worldoftanks.net wargaming.net xcraft. zgncdn.com zynga.com vk.angrypets.ru tankionline.com ag.ru onlineguru.ru mochiads.com kiwzi.net igrofania.ru warthunder.ru playjournal.ru skillclub.com playstation. bungie.net gaming igromania.ru playtomic.com ru-wotp.wgcdn.co ");
            sb.AppendLine(@"Guns = abrams.com.ua airgun.org.ua allzip.org guns.ru gunshop.com.ua ibis.net.ua maksnipe.kiev.ua militarist. opoccuu.com pmcjournal.com russianguns.ru 3mv.ru topwar.ru gearshout.net guns02.ru wiking.kiev.ua ukrspecexport.com reibert.info voentorg.ua guns.ua ohotniki.ru ohotnik.com stvol.ua knifeclub.com.ua knife.com.ua guns ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Anonimize = translate.google. translate.yandex.ru anonymizer anonimizing hide anonymouse obhodilka.ru omg5.ru prolezayka.ru blaim.ru vkvezde.ru vkontaktir.ru vkhodi.ru dd34.ru cmle.ru 1proxy.de erenta.ru bremdy.ru biglu.ru oknovpope.ru nblu.ru noblockme anonim.pro pingway.ru kalarupa.com 2ip.ru cameleo.ru proxfree proxyweb 3proxy.de daidostup.ru leader.ru hidemy ");
            sb.AppendLine(@"");
            sb.AppendLine(@"Virus = .rackcdn.com ");
            sb.AppendLine(@"");
            sb.AppendLine(@"VideoSurveillance = golden-eye.com.ua");
            sb.AppendLine(@";End Direction");
            sb.AppendLine(@"");
            sb.AppendLine(@"");
            sb.AppendLine(@";Disciption of the Direction.  ");
            sb.AppendLine(@"# Описание Направлений URL");
            sb.AppendLine(@"Anonimize = Анонимайзеры (скрытие посещаемого URL)");
            sb.AppendLine(@"AutoClubs = Автоклубы и автофорумы");
            sb.AppendLine(@"AutoLogistika = Автологистические компании");
            sb.AppendLine(@"Banks = Банки и платежные системы");
            sb.AppendLine(@"Totalizatos = Тотализаторы - игры с ценными бумагами");
            sb.AppendLine(@"Cars = Автопроизводители и автодилеры");
            sb.AppendLine(@"DigitalAds = Интернет реклама");
            sb.AppendLine(@"Ebooksmagazine = Электронные журналы и книги");
            sb.AppendLine(@"FileStores = Файловые хранилища");
            sb.AppendLine(@"Finders = Поисковые сервера");
            sb.AppendLine(@"Funny = Развлечения");
            sb.AppendLine(@"Games = Игровые сервера");
            sb.AppendLine(@"GPSCar = GPS - навигация - карты");
            sb.AppendLine(@"Guns = Оружие и военная тематика");
            sb.AppendLine(@"HelthSportBeauty = Здоровье - красота - спорт");
            sb.AppendLine(@"Ittech = Информационные технологии");
            sb.AppendLine(@"LookforaJob = Поиск работы");
            sb.AppendLine(@"Mailers = E-Mail сервисы");
            sb.AppendLine(@"MusicRadioonline = Музыка и радио онлайн");
            sb.AppendLine(@"NewsInfoAds = Новости - информация - объявления");
            sb.AppendLine(@"Parts = Автозапчасти и СТО");
            sb.AppendLine(@"Porno = Клубничка");
            sb.AppendLine(@"Profiles = Профильные направления (бухгалтерия - кадры - юридические - таможенные)");
            sb.AppendLine(@"Shops = Магазины");
            sb.AppendLine(@"ShopBoutiq = Бутики");
            sb.AppendLine(@"ShopBuild = Строительные сайты ");
            sb.AppendLine(@"ShopDigital = Магазины цифровой техники");
            sb.AppendLine(@"ShopRieltor = Покупка - продажа - аренда недвижимости");
            sb.AppendLine(@"SocialNets = Социальные сети");
            sb.AppendLine(@"Teach = Обучение");
            sb.AppendLine(@"VideoTV = Видео и телевидение онлайн");
            sb.AppendLine(@"Images = Файловые хранилища изображений");
            sb.AppendLine(@"Common = Категория неопределенна");
            sb.AppendLine(@"Virus = Вирусный сайт - ПК ЗАРАЖЕН");
            sb.AppendLine(@"VideoSurveillance = Видеонаблюдение (железо, ПО и услуги)");
            sb.AppendLine(@";End Disciption of the Direction");
            sb.AppendLine(@"");
            sb.AppendLine(@"");
            sb.AppendLine(@";Cleaner.");
            sb.AppendLine(@"# Удаление мусора из ссылок. Только один набор на строку!");
            sb.AppendLine(@"www.");
            sb.AppendLine(@"# :21");
            sb.AppendLine(@"*.");
            sb.AppendLine(@";End Cleaner");
            sb.AppendLine(@"");
            sb.AppendLine(@"");
            sb.AppendLine(@";Replacer.");
            sb.AppendLine(@"# Замена URL содержащей  домен указанный с правой стороны знака  =  на другой указанный перед знаком = ");
            sb.AppendLine(@"# Перебор доменов идет слева направо");
            sb.AppendLine(@"# После нахождения первого совпадения перебор прекращается");
            sb.AppendLine(@"");
            sb.AppendLine(@"kinogo.club = kinogo.club kinogo.co kinogo.cc kinogo.by ");
            sb.AppendLine(@"moonwalk.cc = moonwalk.cc moonwalk.co ");
            sb.AppendLine(@"facebook.net = facebook.net facebook.com ");
            sb.AppendLine(@"soundcloud.com = soundcloud.com cf-hls-media.sndcdn.com ");
            sb.AppendLine(@"google.com = google.com.ua google.com.ru safebrowsing-cache.google.com google.com ");
            sb.AppendLine(@"yandex.ru = yandex.ru yandex.net yandex.ua ");
            sb.AppendLine(@"wargaming.net = wargaming.net wargaming.ua wargaming.ru ");
            sb.AppendLine(@"worldoftanks.net = worldoftanks.net worldoftanks.ua worldoftanks.ru");
            sb.AppendLine(@"4pda.ru = 4pda.ru 4pda.to ");
            sb.AppendLine(@"kinokrad.net = kinokrad.net kinokrad.co ");
            sb.AppendLine(@"criteo.net = criteo.com");
            sb.AppendLine(@";End Replacer");
            sb.AppendLine(@"");
            sb.AppendLine(@"");
            sb.AppendLine(@";Simplifier.");
            sb.AppendLine(@"# Если URL содержит домен указанный ниже URL заменяется на указанный");
            sb.AppendLine(@"101.ru");
            sb.AppendLine(@"24video.adult");
            sb.AppendLine(@"adio.obozrevatel.com");
            sb.AppendLine(@"adme.ru");
            sb.AppendLine(@"akamaihd.net");
            sb.AppendLine(@"alicdn.com");
            sb.AppendLine(@"aliexpress.com");
            sb.AppendLine(@"amazonaws.com");
            sb.AppendLine(@"anonimizing.com");
            sb.AppendLine(@"anwap.org");
            sb.AppendLine(@"apollostream.xyz");
            sb.AppendLine(@"auto.drom.ru");
            sb.AppendLine(@"auto.ria.com");
            sb.AppendLine(@"autogidas.lt");
            sb.AppendLine(@"bonus-tv.ru");
            sb.AppendLine(@"carsontheweb.com");
            sb.AppendLine(@"cdn.riastatic.com");
            sb.AppendLine(@"cdn.yandex.ru");
            sb.AppendLine(@"cdnvideo.ru");
            sb.AppendLine(@"censor.net.ua");
            sb.AppendLine(@"cf5.rackcdn.com");
            sb.AppendLine(@"chocoapp.ru");
            sb.AppendLine(@"citrus.ua");
            sb.AppendLine(@"cosmopolitan.ru");
            sb.AppendLine(@"criteo.net");
            sb.AppendLine(@"d-cd.net");
            sb.AppendLine(@"deezee.pl");
            sb.AppendLine(@"doubleclick.net");
            sb.AppendLine(@"dropmefiles.com");
            sb.AppendLine(@"edisk.ukr.net");
            sb.AppendLine(@"estafeta.org");
            sb.AppendLine(@"facebook.net");
            sb.AppendLine(@"files.attachmail.ru");
            sb.AppendLine(@"fishki.net");
            sb.AppendLine(@"forexpros.com");
            sb.AppendLine(@"fotocdn.net");
            sb.AppendLine(@"githubusercontent.com");
            sb.AppendLine(@"golden-eye.com.ua");
            sb.AppendLine(@"gvt1.com");
            sb.AppendLine(@"hotcloud.org");
            sb.AppendLine(@"imgsmail.ru");
            sb.AppendLine(@"instagram.com");
            sb.AppendLine(@"intercom.io");
            sb.AppendLine(@"kaban.tv");
            sb.AppendLine(@"kamaized.net");
            sb.AppendLine(@"kinogo.club");
            sb.AppendLine(@"kinokrad.net");
            sb.AppendLine(@"lanet.tv");
            sb.AppendLine(@"leboutique.com");
            sb.AppendLine(@"ligazakon.net");
            sb.AppendLine(@"makeagif.com");
            sb.AppendLine(@"maps.yandex.ru");
            sb.AppendLine(@"marketgid.com");
            sb.AppendLine(@"media.online.ua");
            sb.AppendLine(@"mixcloud.com");
            sb.AppendLine(@"mixupload.com");
            sb.AppendLine(@"moonwalk.cc");
            sb.AppendLine(@"my.mail.ru");
            sb.AppendLine(@"muzofond.org");
            sb.AppendLine(@"mycdn.me");
            sb.AppendLine(@"myzuka.me");
            sb.AppendLine(@"mzstatic.com");
            sb.AppendLine(@"nblu.ru");
            sb.AppendLine(@"obozrevatel.ua");
            sb.AppendLine(@"ollcdn.net");
            sb.AppendLine(@"olx.ua");
            sb.AppendLine(@"onlineradiobox.com");
            sb.AppendLine(@"ovva.tv");
            sb.AppendLine(@"pdbcdn.co");
            sb.AppendLine(@"pikabu.ru");
            sb.AppendLine(@"planeta-online.tv");
            sb.AppendLine(@"playstation.com");
            sb.AppendLine(@"playtomic.com");
            sb.AppendLine(@"presenta.xyz");
            sb.AppendLine(@"rackcdn.com");
            sb.AppendLine(@"redhat.com");
            sb.AppendLine(@"ringon.ru");
            sb.AppendLine(@"riastatic.com");
            sb.AppendLine(@"rozetka.ua");
            sb.AppendLine(@"runetki.co");
            sb.AppendLine(@"rutube.ru");
            sb.AppendLine(@"sender.mobi");
            sb.AppendLine(@"shutterstock.com");
            sb.AppendLine(@"spincar.com");
            sb.AppendLine(@"soundcloud.com");
            sb.AppendLine(@"spac.me");
            sb.AppendLine(@"spaces.ru");
            sb.AppendLine(@"storage.yandex.ru");
            sb.AppendLine(@"tavrmedia.ua");
            sb.AppendLine(@"testlivestream.rfn.ru");
            sb.AppendLine(@"thesame.tv");
            sb.AppendLine(@"trofey.net");
            sb.AppendLine(@"ttvnw.net");
            sb.AppendLine(@"turbobit.net");
            sb.AppendLine(@"tvigle.ru");
            sb.AppendLine(@"tvzavr.ru");
            sb.AppendLine(@"twitch.tv");
            sb.AppendLine(@"vcdn.biz");
            sb.AppendLine(@"videoprobki.com.ua");
            sb.AppendLine(@"vidyomani.com");
            sb.AppendLine(@"vimeocdn.com");
            sb.AppendLine(@"# VisualStudio Extension");
            sb.AppendLine(@"#vsassets.io");
            sb.AppendLine(@"#visualstudio.com");
            sb.AppendLine(@"xvideos.com");
            sb.AppendLine(@"yapfiles.ru");
            sb.AppendLine(@"yaplakal.com");
            sb.AppendLine(@"yaporn.sex");
            sb.AppendLine(@"youtube.com");
            sb.AppendLine(@"ytimg.com");
            sb.AppendLine(@"wargaming.net");
            sb.AppendLine(@"# Windows Update");
            sb.AppendLine(@"windowsupdate.com");
            sb.AppendLine(@"update.microsoft.com");
            sb.AppendLine(@"zaycev.net");
            sb.AppendLine(@"zf.fm");
            sb.AppendLine(@";End Simplifier");
            sb.AppendLine(@"");
            sb.AppendLine(@"");
            sb.AppendLine(@";SimplifyEnd");
            sb.AppendLine(@"# Если URL заканчивается на указанный ниже, то отрезается часть URL спереди до указанной нижн маски");
            sb.AppendLine(@"vimeo.akamaized.net");
            sb.AppendLine(@";End SimplifyEnd");
            sb.AppendLine(@"");
            sb.AppendLine(@"");
            sb.AppendLine(@"; End of Configuration");
            File.WriteAllText("ProxyAnalyser.ini", sb.ToString(), System.Text.Encoding.GetEncoding(1251));
            sb = null;
        }
    }
}

