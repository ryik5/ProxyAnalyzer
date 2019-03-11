using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using Microsoft.Win32;  // для работы с реестром


namespace ProxyAnalyser
{
    public partial class Form2 : Form
    {
        public string _allUsers="";
        public string sLoadURI="";
        public bool bLoadURI = false;
        private string _file1 = "ProxyAnalyser\\!tmpfile1.tmp"; //Periods
        private string _file2 = "ProxyAnalyser\\!tmpfile2.tmp"; //
        private string _file3 = "ProxyAnalyser\\!tmpfile3.tmp"; //
        private string _file4 = "ProxyAnalyser\\!tmpfile4.tmp";
        private string UserName;
        private string UserPassword;
        private string myPrLogKey = @"SOFTWARE\RYIK\ProxyAnalyser2";
        //        private string _myHTTPstring = "http://sarg.kiev.ais/PLOD/index.html";
        private TextBox textboxInputURI;
        private Label labelInputURI;

        public Form2(Form1 f1) //for transfer any data between Form1 and Form12
        { InitializeComponent(); }

        private void Form2_Load(object sender, EventArgs e) //Set Status of buttons and Delete previous temporary files
        { Form2Load(); }

        private void Form2Load() //Set Status of buttons and Delete previous temporary files
        {
            CheckRegistrySavedData();
            textBoxLogin.Text = UserName;
            textBoxPassword.Text = UserPassword;

            //my icon
            Icon = Properties.Resources.iconRYIK;
            try { DirectoryInfo di = Directory.CreateDirectory("ProxyAnalyser"); } catch { }

            toolTip1.SetToolTip(textBoxUserLogin, "Введите логин пользователя, у которого планируется анализировать статистику прокси");
            toolTip1.SetToolTip(textBoxLogin, "Введите свой логин, для доступа к серверу SARG");
            toolTip1.SetToolTip(textBoxPassword, "Введите свой пароль, для доступа к серверу SARG");
            toolTip1.SetToolTip(buttonOK, "Нажмите эту кнопку для авторизации на сервере SARG");
            toolTip1.SetToolTip(buttonLoadSelectedData, "Нажмите эту кнопку для загрузки выбранного периода статистики прокси по пользователю");
            toolStripStatusLabel1.Text = "Формат данных - pupkin_av";
            toolStripStatusLabel2.Text = " ©RYIK  2016-2017";
            if (textBoxLogin.TextLength > 1 && textBoxPassword.TextLength > 1)
            { buttonOK.Enabled = true; }
            else { buttonOK.Enabled = false; }

            buttonCheck.Enabled = false;
            textBoxUserLogin.Enabled = false;
            buttonLoadSelectedData.Visible = false;
            buttonLastListUsers.Enabled = false;
            foreach (CheckBox checkBox in Controls.OfType<CheckBox>())      //Перебираем все чекбоксы на форме
            { checkBox.Visible = false; checkBox.Enabled = false; }
            _DeleteEmptyTmpFile(); //Delete All tmp Files if they have sticked
            if (bLoadURI)
            {
                textboxInputURI = new TextBox
                {
                    Text = "Введите URL страницы",
                    Location = new System.Drawing.Point(84, 70),
                    Size = new System.Drawing.Size(310, 20),
                    BorderStyle = BorderStyle.FixedSingle,
                    Enabled = false,
                    Parent = this
                };
                textboxInputURI.Click += new System.EventHandler(TextboxInputURI_Click);
                textboxInputURI.TextChanged += new System.EventHandler(TextboxInputURI_TextChanged);

                toolTip1.SetToolTip(textboxInputURI, @"Введите URL страницы со статистикой прокси. http://www.name.com/address.html");

                labelInputURI = new Label
                {
                    Text = "URL",
                    BackColor = System.Drawing.Color.LightSteelBlue,
                    Location = new System.Drawing.Point(10, 70),
                    Size = new System.Drawing.Size(65, 20),
                    BorderStyle = BorderStyle.None,
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                    Parent = this
                };
                buttonLoadSelectedData.Visible = true;
                /*
                AutoCompleteStringCollection source = new AutoCompleteStringCollection()
                { "Кузнецов", "Иванов", "Петров", "Кустов" };
                _textBoxUserLogin.AutoCompleteCustomSource = source;
                _textBoxUserLogin.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                _textBoxUserLogin.AutoCompleteSource = AutoCompleteSource.CustomSource;
                */
            }
        }

        private void CheckRegistrySavedData()
        {
            try
            {
                using (RegistryKey EvUserKey = Registry.CurrentUser.OpenSubKey(myPrLogKey, RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey))
                {
                    UserName = EvUserKey.GetValue("UserLogin").ToString();
                    UserPassword = EvUserKey.GetValue("UserPassword").ToString();
                }                   
            } catch { }

            if (UserName!=null && UserPassword != null && UserName.Length > 0 && UserPassword.Length > 0)
            {
                textBoxLogin.Text = UserName;
                textBoxPassword.Text = UserPassword;
            }
        }

        private void TextboxInputURI_Click(object sender, EventArgs e)
        { textboxInputURI.Clear(); }

        private void TextboxInputURI_TextChanged(object sender, EventArgs e)
        {
            if (textboxInputURI.TextLength > 5 && textboxInputURI.Text.Contains(@"://"))
            { buttonLoadSelectedData.Enabled = true; }
            else
            { buttonLoadSelectedData.Enabled = false; }
        }

        private void _buttonOK_Click(object sender, EventArgs e) //Check Login and Password at The SARG
        {
            try
            {
                if (!bLoadURI)
                {
                    _ReadURLAndSaveToHtml("http://sarg.kiev.ais/PLOD/index.html", _file1);
                    textBoxUserLogin.Enabled = true; //Enable Input searching Data with the user
                    buttonLastListUsers.Enabled = true;
                }
                else
                { textboxInputURI.Enabled = true; }
                textBoxLogin.ReadOnly = true;
                textBoxPassword.ReadOnly = true;
            }
            catch (Exception Expt)
            {
                string exception = Expt.ToString();
                if (exception.ToLower().Contains("невозможно разрешить удаленное"))
                { MessageBox.Show("Ошибка доступа к сайту со статистикой SARG"); }
                else
                { MessageBox.Show(Expt.ToString()); }
            }
        }

        private void _buttonCheck_Click(object sender, EventArgs e) //Check existing statistics the inputed user
        { _ParsingPeriodHtmlToFile(_file1, _file2); }

        private void _ProgressWork()
        {
            try
            {
                if (InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate
                    {
                        if (ProgressBar1.Value > 99)
                        { ProgressBar1.Value = 0; }
                        ProgressBar1.Maximum = 100;
                        ProgressBar1.Value += 5;
                    }));
                else
                {
                    if (ProgressBar1.Value > 99)
                    { ProgressBar1.Value = 0; }
                    ProgressBar1.Maximum = 100;
                    ProgressBar1.Value += 5;
                }
            }
            catch { }
        }

        private void _ProgressBar1Value100() //Set progressBar Value into 100 from other threads
        {
            try
            {
                if (InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate
                    { ProgressBar1.Value = 100; }));
                else
                { ProgressBar1.Value = 100; }
            }
            catch { }
        }

        private void _ProgressBar1Value0() //Set progressBar Value into 0 from other threads
        {
            try
            {
                if (InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate
                    { ProgressBar1.Value = 0; }));
                else
                { ProgressBar1.Value = 0; }
            }
            catch { }
        }

        private void _ProgressBar1ToolTipText(string s) //Set progressBar Value into 0 from other threads
        {
            try
            {
                if (InvokeRequired)
                    BeginInvoke(new MethodInvoker(delegate
                    { ProgressBar1.ToolTipText = s; }));
                else
                { ProgressBar1.ToolTipText = s; }
            }
            catch { }
        }

        private void _ReadURLAndSaveToHtml(string myURL, string myFile) //Read URL and Save into the local File
        {
            try
            {
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(myURL);
                myHttpWebRequest.PreAuthenticate = true;
                NetworkCredential networkCredential = new NetworkCredential(textBoxLogin.Text.Trim(), textBoxPassword.Text.Trim());
                myHttpWebRequest.Credentials = networkCredential;
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                using (StreamReader myStreamReader = new StreamReader(myHttpWebResponse.GetResponseStream(), Encoding.GetEncoding(20866))) //KOI8
                {
                        var Coder = Encoding.GetEncoding(65001); //Save The Pages to UTF8.  //Save The Pages to win1251 - "Coder = Encoding.GetEncoding(1251)"
                        File.AppendAllText(myFile, myStreamReader.ReadToEnd(), Coder);
                }
                myHttpWebResponse.Close();
                myHttpWebRequest.Abort();
            }
            catch { }
        }

        private void _ParsingPeriodHtmlToFile(string myInFile, string myOutFile) //Parsing Previous read information about the periods
        {
            string[] substringURL = new string[15];
            string[] substringFullURL = new string[15];
            string[] substringYear = new string[15];
            string[] substringMonth = new string[15];
            string[] substrings;
            string s;
            int i = 0;
            StringBuilder sb = new StringBuilder();
            var Coder = Encoding.GetEncoding(65001); //For Saving of The Pages to UTF8
            HtmlDocument HD = new HtmlDocument();

            try //Parsing URL, Month and Year and write their into the arrays
            {
                HD.LoadHtml(File.ReadAllText(myInFile).ToString());
                HtmlNodeCollection NoAltElements = HD.DocumentNode.SelectNodes("//td[@class='data2'][1]");

                if (NoAltElements != null)      // проверка на наличие найденных узлов
                {
                    foreach (HtmlNode HN in NoAltElements)
                    {
                        substringURL[i] = HN.InnerText;
                        substrings = Regex.Split(HN.InnerText, "-| ");
                        substringMonth[i] = substrings[1].Trim(new Char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' }); //Take only month
                        s = substrings[1];
                        substringYear[i] = s.Remove(4);
                        sb.AppendLine(HN.InnerText);
                        if (i < 13)
                        { i ++; }
                    }
                }
                NoAltElements.Clear();
                toolStripStatusLabel1.Text = "Обработка завершена!";
            }
            catch (FileNotFoundException Expt)
            { MessageBox.Show(Expt.Message + "\nНет такого файла", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); }
            catch (Exception Expt)
            { MessageBox.Show(Expt.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); }

            try
            { File.AppendAllText(myOutFile, sb.ToString(), Coder); }
            catch (Exception Expt)
            { MessageBox.Show(Expt.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); }

            try
            {
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://sarg.kiev.ais/PLOD/index.html");
                myHttpWebRequest.PreAuthenticate = true;
                NetworkCredential networkCredential = new NetworkCredential(textBoxLogin.Text.Trim(), textBoxPassword.Text.Trim());
                myHttpWebRequest.Credentials = networkCredential;
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                using (StreamReader myStreamReader = new StreamReader(myHttpWebResponse.GetResponseStream(), Encoding.GetEncoding(20866)))
                {    //KOI8
                    HD.LoadHtml(myStreamReader.ReadToEnd());
                    HtmlNodeCollection NoAltElements = HD.DocumentNode.SelectNodes("//a");
                    if (NoAltElements != null)      // проверка на наличие найденных узлов
                    {
                        i = 0;
                        foreach (HtmlNode HN in NoAltElements)
                        {
                            if (HN.Attributes["href"] != null && !HN.Attributes["href"].Value.ToLower().Contains("sourceforge"))
                            {
                                string u = "http://sarg.kiev.ais/PLOD/" + HN.Attributes["href"].Value;
                                if (u.Contains("201")) //Проверка статистики только с 2010 по 2019 года
                                { substringFullURL[i] = u; }
                                if (i < 13)
                                { i++; }
                            }
                        }
                    }
                }
                myHttpWebResponse.Close();
                myHttpWebRequest.Abort();
            }
            catch (Exception Expt)
            { MessageBox.Show(Expt.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); }
            toolStripStatusLabel1.Text = "Обработка завершена!";

            //Write Setting of the State into checkBoxes
            foreach (CheckBox checkBox in this.Controls.OfType<CheckBox>())      //Перебираем все чекбоксы на форме
            {
                int k = Convert.ToInt32(checkBox.TabIndex);
                if (substringYear[k - 1] != null && substringYear[k - 1].Length == 4)
                {
                    checkBox.Text = substringYear[k - 1] + " " + substringMonth[k - 1];
                    checkBox.Tag = substringURL[k - 1];
                    checkBox.AccessibleDescription = substringFullURL[k - 1];
                    checkBox.Visible = true;
                    checkBox.Enabled = true;
                }
            }
            sb = null;
            HD = null;
            buttonLoadSelectedData.Visible = true;
            buttonLoadSelectedData.Enabled = false;
        }

        private void _LastListUsers_Click(object sender, EventArgs e) //Prepare the list of logins from SARG
        {
            _ChecklistUsers();
            _ListTop20Users(_allUsers);
            buttonLastListUsers.Enabled = false;
        }

        private void _ChecklistUsers() //check exists the list of logins and try to parse the first of the pages of statistics
        {
            try
            {
                var Coder = Encoding.GetEncoding(65001); //For Saving of The Pages to UTF8
                HtmlDocument HD = new HtmlDocument();
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://sarg.kiev.ais/PLOD/index.html");
                myHttpWebRequest.PreAuthenticate = true;
                NetworkCredential networkCredential = new NetworkCredential(textBoxLogin.Text.Trim(), textBoxPassword.Text.Trim());
                myHttpWebRequest.Credentials = networkCredential;
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                using (StreamReader myStreamReader = new StreamReader(myHttpWebResponse.GetResponseStream(), Encoding.GetEncoding(20866))) //KOI8
                {
                    HD.LoadHtml(myStreamReader.ReadToEnd());
                    HtmlNodeCollection NoAltElements = HD.DocumentNode.SelectNodes("//a");
                    if (NoAltElements != null)      // проверка на наличие найденных узлов
                    {
                        int i = 0;
                        foreach (HtmlNode HN in NoAltElements)
                        {
                            if (HN.Attributes["href"] != null)
                            {
                                string u = "http://sarg.kiev.ais/PLOD/" + HN.Attributes["href"].Value;

                                if (u.Contains("201")) //Проверка статистики только с 2010 по 2019 года
                                {
                                    _allUsers = u;
                                    break;
                                }
                                if (i < 13)
                                { i++; }
                            }
                        }
                    }
                }
                myHttpWebResponse.Close();
                myHttpWebRequest.Abort();
                HD = null;
            }
            catch (Exception Expt)
            { MessageBox.Show(Expt.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); }
        }

        private void _ListTop20Users(string _URLUserList) //Parsing TOP list of logins
        {
            textBox2.Clear();
            //            textBox2.AppendText(: \n");
            string[] _tmpUsers20 = new string[20];
            string[] _tmpBytes20 = new string[20];
            comboBox1.Sorted = true;
            HtmlDocument HD = new HtmlDocument();
            HtmlNodeCollection nodes;
            HttpWebRequest myHttpWebRequest;
            HttpWebResponse myHttpWebResponse;
            try
            {
                myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(_URLUserList);
                myHttpWebRequest.PreAuthenticate = true;
                NetworkCredential networkCredential = new NetworkCredential(textBoxLogin.Text.Trim(), textBoxPassword.Text.Trim());
                myHttpWebRequest.Credentials = networkCredential;
                myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                using (StreamReader myStreamReader = new StreamReader(myHttpWebResponse.GetResponseStream(), Encoding.GetEncoding(20866))) //KOI8
                {
                    HD.LoadHtml(myStreamReader.ReadToEnd());
                    nodes = HD.DocumentNode.SelectNodes("//td[@class='data2'][2]");
                    if (nodes != null)      // проверка на наличие найденных узлов
                    {
                        int i = 0;
                        foreach (HtmlNode HN in nodes)
                        {
                            if (HN.InnerText != null && HN.InnerText.ToString().Contains("nbsp") == false)
                            {
                                try
                                {
                                    string mycombo = HN.InnerText.ToString().Trim();
                                    string mycombo1 = mycombo.Replace(@"corp.ais\\", "");
                                    string mycombo2 = mycombo1.Replace(@"corp\\", "");
                                    if (i < 20)
                                    { _tmpUsers20[i] = mycombo2; }   //TOP пользователей
                                    i++;
                                    comboBox1.Items.Add(mycombo2.Trim());
                                }
                                catch { }
                            }
                        }
                    }

                    nodes = HD.DocumentNode.SelectNodes("//td[@class='data'][3]");
                    if (nodes != null)      // проверка на наличие найденных узлов
                    {
                        int i = 0;
                        foreach (HtmlNode HN in nodes)
                        {
                            if (HN.InnerText != null && HN.InnerText.ToString().Contains("nbsp") == false)
                            {
                                try
                                {
                                    string mycombo = HN.InnerText.ToString().Trim();
                                    if (i < 20)
                                    { _tmpBytes20[i] = mycombo; }
                                    i++;
                                }
                                catch { }
                            }
                        }
                    }
                }
                myHttpWebResponse.Close();
                myHttpWebRequest.Abort();
            }
            catch (Exception Expt)
            { MessageBox.Show(Expt.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); }
            for (int i = 0; i < 20; i++)
            {
                try
                { textBox2.AppendText(i + ".  " + _tmpUsers20[i] + "  -  " + _tmpBytes20[i] + "\n"); } catch { }
            }
            _tmpUsers20 = null; _tmpBytes20 = null; HD = null; nodes = null;
            try { comboBox1.SelectedIndex = 0; } catch { }
            textBox2.Focus();
            textBox2.Select(0, 0);
            textBox2.ScrollToCaret();
            textBox2.SelectionStart = 0;
            textBox2.SelectionLength = 0;
            textBox2.ReadOnly = true;
            HD = null;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) // Select login for downloading
        {
            textBoxUserLogin.Text = comboBox1.SelectedItem.ToString();
            buttonCheck.Enabled = true;

            foreach (CheckBox checkBox in this.Controls.OfType<CheckBox>())      //Перебираем все чекбоксы на форме
            {
                checkBox.Text = "";
                checkBox.Tag = "";
                checkBox.AccessibleDescription = "";
                checkBox.Checked = false;
                checkBox.Visible = false;
            }
            buttonLoadSelectedData.Enabled = false;
        }

        private void _textBoxLoginANDPassword_TextChanged(object sender, EventArgs e)  //Enable The button "OK"
        {
            if (textBoxLogin.TextLength > 1 && textBoxPassword.TextLength > 1)
            {buttonOK.Enabled=true; }
            else { buttonOK.Enabled = false; }

        }

        private void _textBoxUserLogin_TextChanged(object sender, EventArgs e)   //Enable The button "Check"
        {
            if (textBoxUserLogin.TextLength>1)
            { buttonCheck.Enabled = true; }
        }

        private void _WriteMonthInFileAndDeleteEmpty(CheckBox chk, string MyFile) //function. write Data into CheckBoxes and delete empty files
        {
            File.WriteAllText(MyFile + ".tmp", "<!Selected_User:  " + textBoxUserLogin.Text.Trim() + ">\n" + "<!Selected_Period:  " + chk.Text.Trim() + ">\n" + "<!Selected_End:  >\n\n\n");
            _ReadURLAndSaveToHtml("http://sarg.kiev.ais/PLOD/" + chk.Tag.ToString() + "/" + (textBoxUserLogin.Text.Trim()).Replace(@".", "_") + "/" + (textBoxUserLogin.Text.Trim()).Replace(@".", "_") + ".html", MyFile+ ".tmp");
            try
            {
                File.WriteAllText(MyFile+"_.tmp", "<!Selected_User:  " + textBoxUserLogin.Text.Trim() + ">\n" + "<!Selected_Period:  " + chk.Text.Trim() + ">\n" + "<!Selected_End:  >\n\n\n");
                _ReadURLAndSaveToHtml("http://sarg.kiev.ais/PLOD/" + chk.Tag.ToString() + "/corp_" + textBoxUserLogin.Text.Trim() + "/corp_" + textBoxUserLogin.Text.Trim() + ".html", MyFile+"_.tmp");
            }
            catch { }
            FileInfo file = new FileInfo(MyFile + ".tmp");
            if ((file.Length / 1024) < 1)
            {
                try { file.Delete(); } catch { }
            }
            file = new FileInfo(MyFile + "_.tmp");
            if ((file.Length / 1024) < 1)
            {
                try { file.Delete(); } catch { }
            }
        }

        private void _buttonLoadmySelectedData_Click(object sender, EventArgs e)  //Load pages which took by checkboxes into the local temporary files 
        {
            UserName = textBoxLogin.Text;
            UserPassword = textBoxPassword.Text;

            _loadData();
        }

        private void _loadData() //Печать графиков за месяц через бэкграунд
        {
            if (bLoadURI)
            {
                /*
                 <!Selected_User:  ry>
                 <!Selected_Period:  2017 Aug>
                 <!Selected_End:  >
                 */
                HtmlDocument HD = new HtmlDocument();
                sLoadURI = textboxInputURI.Text.Trim();
                //http://sarg.kiev.ais/deriy/2017Sep07-2017Sep07/deriy_aa/deriy_aa.html
                string sUsername = "";
                string sPeriod = "";
                string sTmpNode = "";
                string[] aTmp;
                try
                {
                    HttpWebRequest myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(textboxInputURI.Text.Trim());
                    NetworkCredential networkCredential = new NetworkCredential(textBoxLogin.Text.Trim(), textBoxPassword.Text.Trim());
                    myHttpWebRequest.PreAuthenticate = true;
                    myHttpWebRequest.Credentials = networkCredential;
                    HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                    using (StreamReader myStreamReader = new StreamReader(myHttpWebResponse.GetResponseStream(), Encoding.GetEncoding(20866))) //KOI8
                    {
                        HD.LoadHtml(myStreamReader.ReadToEnd());
                        HtmlNodeCollection nodes = HD.DocumentNode.SelectNodes("//td[@class='header_c'][1]"); //Select only the name of the table 
                        if (nodes != null)      // проверка на наличие найденных узлов
                        {
                            foreach (HtmlNode HN in nodes)
                            {
                                if (HN.InnerText != null)
                                {
                                    sTmpNode = HN.InnerText.Replace("&nbsp", " ").Replace("&mdash", @"-").Replace(";", "").Replace(":", "").Trim();

                                    if (sTmpNode.ToLower().Contains("user"))
                                    { sUsername = sTmpNode.ToLower().Replace("user", "").Replace(" ", "").Trim(); }

                                    else if (sTmpNode.ToLower().Contains("period"))
                                    {
                                        string stmp = "";
                                        string stmp1 = sTmpNode.ToLower().Replace("period", "");
                                        string stmp2 = stmp1.Replace(@"   ", " ");
                                        stmp2 = stmp2.Replace(@"  ", " ");

                                        if (sTmpNode.Contains(@"-"))
                                        { stmp = Regex.Split(stmp2, "-")[0] + "-" + Regex.Split(stmp2, "-")[1].Remove(0, 4); }
                                        else
                                        { stmp = stmp2; }

                                        aTmp = Regex.Split(stmp, " ");
                                        if (aTmp.Length > 1)
                                        {
                                            for (int k = 0; k < aTmp.Length; k++)
                                            {
                                                if (k < 2)
                                                {
                                                    sPeriod += aTmp[k].Trim();
                                                    sPeriod += " ";
                                                }
                                                else
                                                    sPeriod += aTmp[k].Trim();
                                            }
                                        }
                                        else { sPeriod = "Unknown Unknown"; }
                                    }
                                }
                            }
                        }
                        nodes = null;
                    }
                    myHttpWebResponse.Close();
                    myHttpWebRequest.Abort();

                    File.WriteAllText("ProxyAnalyser\\!htupm1.tmp", "<!Selected_User:  " + sUsername + ">\n" + "<!Selected_Period:  " + sPeriod.Trim() + ">\n" +
                        "<!Selected_End:  >\n\n\n");
                    _ReadURLAndSaveToHtml(sLoadURI, "ProxyAnalyser\\!htupm1.tmp");
                }
                catch { }
                HD = null;
                sUsername = null;
                sPeriod = null;
                sTmpNode = null;
                aTmp = null;
            }
            else
            {
                _ProgressBar1Value0();
                _ProgressBar1ToolTipText("Идет загрузка и сохранение данных в файлы");
                int i;
                foreach (CheckBox checkBox in this.Controls.OfType<CheckBox>())      //Перебираем все чекбоксы на форме
                {
                    _ProgressWork();
                    i = Convert.ToInt32(checkBox.TabIndex);
                    if (checkBox.Checked == true)
                    { try { _WriteMonthInFileAndDeleteEmpty(checkBox, "ProxyAnalyser\\!htupm" + i); } catch { } }
                }
                i = 0;
                _ProgressWork();

                try { File.Delete(_file1); } catch { }
                try { File.Delete(_file2); } catch { }
                try { File.Delete(_file3); } catch { }
                try { File.Delete(_file4); } catch { }
                _ProgressBar1Value100();
            }
            try
            {
                using (var PrUserKey = Registry.CurrentUser.CreateSubKey(myPrLogKey))
                {
                    PrUserKey.SetValue("UserLogin", UserName, RegistryValueKind.String);
                    PrUserKey.SetValue("UserPassword", UserPassword, RegistryValueKind.String);
                }
            }
            catch { }
            Form2.ActiveForm.Close();
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e) // Show the button "Load Selected Data"
        {
            buttonLoadSelectedData.Visible = true;
            buttonLoadSelectedData.Enabled = false;
            foreach (CheckBox checkBox in this.Controls.OfType<CheckBox>())      //Перебираем все чекбоксы на форме
            {
                if (checkBox.Checked)
                {
                    buttonLoadSelectedData.Enabled = true;
                    break;
                }
            }
        }

        private void _DeleteEmptyTmpFile() // Try to Delete my all temporary Files 
        {
            try { File.Delete(_file1); } catch { }
            try { File.Delete(_file2); } catch { }
            try { File.Delete(_file3); } catch { }
            try { File.Delete(_file4); } catch { }
            for (int i = 1; i < 19; i++)
            {
                try { File.Delete("ProxyAnalyser\\!htupm" + i + ".tmp"); } catch { }
                try { File.Delete("ProxyAnalyser\\!htupm" + i + "_.tmp"); } catch { }
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (textboxInputURI != null) textboxInputURI.Dispose();
                if (labelInputURI != null) labelInputURI.Dispose();

                if (UserName.Length > 0 && UserPassword.Length > 0)
                    using (var PrUserKey = Registry.CurrentUser.CreateSubKey(myPrLogKey))
                    {
                        PrUserKey.SetValue("UserLogin", UserName, RegistryValueKind.String);
                        PrUserKey.SetValue("UserPassword", UserPassword, RegistryValueKind.String);
                    }
            }
            catch { }
        }
    }//The end of Form2
}
