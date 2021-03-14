using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Modes_ps
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum Device { Повторитель, Концентратор, Сетевой_мост, Коммутатор, Маршрутизатор, Конечная_станция };

        public enum buildings { I, II, III, IV, V }
        MySqlConnection conn;
        List<ARP> arpTable = new List<ARP> { };
        List<ARP_ans_all> ans_Alls = new List<ARP_ans_all> { };
        List<ARP_short> shorts = new List<ARP_short> { };
        public class ARP
        {
            public string IP { get; set; }
            public string MAC { get; set; }
            public string Type { get; set; }
            public string Vendor { get; set; }
            public Device device { get; set; }
            public int Num_port { get; set; }
            public string Model { get; set; }
            public string OS { get; set; }
            public string Vlan { get; set; }
            public buildings buildings { get; set; }
            public int Hall { get; set; }
            public string Inventory { get; set; }
            public int Link { get; set; }

        }
        public class ARP_ans_all
        {
            public string IP { get; set; }
            public string MAC { get; set; }
            public string Vendor { get; set; }
            public string device { get; set; }
            public int Num_port { get; set; }
            public string Model { get; set; }
            public string OS { get; set; }
            public string Vlan { get; set; }
            public string buildings { get; set; }
            public int Hall { get; set; }
            public string Inventory { get; set; }
            public string Link { get; set; }
            public string date { get; set; }
            public string comment { get; set; }
        }
        public class ARP_short
        {
            public string vendor { get; set; }
            public string device { get; set; }
            public string address { get; set; }
            public int hall { get; set; }
            public string inventory { get; set; }

        }
        public MainWindow()
        {
            InitializeComponent();
        }

        public class InputBox
        {

            Window Box = new Window();//window for the inputbox
            FontFamily font = new FontFamily("Tahoma");//font for the whole inputbox
            int FontSize = 30;//fontsize for the input
            StackPanel sp1 = new StackPanel();// items container
            string title = "InputDialog";//title as heading
            string boxcontent;//title
            string defaulttext = "Введите текст...";//default textbox content
            string errormessage = "Некорректные данные";//error messagebox content
            string errortitle = "Ошибка";//error messagebox heading title
            string okbuttontext = "OK";//Ok button content
            Brush BoxBackgroundColor = Brushes.Gray;// Window Background
            Brush InputBackgroundColor = Brushes.Ivory;// Textbox Background
            bool clicked = false;
            TextBox input = new TextBox();
            Button ok = new Button();
            bool inputreset = false;

            public InputBox(string content)
            {
                try
                {
                    boxcontent = content;
                }
                catch { boxcontent = "Ошибка!"; }
                windowdef();
            }

            public InputBox(string content, string Htitle, string DefaultText)
            {
                try
                {
                    boxcontent = content;
                }
                catch { boxcontent = "Ошибка!"; }
                try
                {
                    title = Htitle;
                }
                catch
                {
                    title = "Ошибка!";
                }
                try
                {
                    defaulttext = DefaultText;
                }
                catch
                {
                    DefaultText = "Ошибка!";
                }
                windowdef();
            }

            public InputBox(string content, string Htitle, string Font, int Fontsize)
            {
                try
                {
                    boxcontent = content;
                }
                catch { boxcontent = "Ошибка!"; }
                try
                {
                    font = new FontFamily(Font);
                }
                catch { font = new FontFamily("Tahoma"); }
                try
                {
                    title = Htitle;
                }
                catch
                {
                    title = "Ошибка!";
                }
                if (Fontsize >= 1)
                    FontSize = Fontsize;
                windowdef();
            }

            private void windowdef()// window building - check only for window size
            {
                Box.Height = 200;// Box Height
                Box.Width = 300;// Box Width
                Box.Background = BoxBackgroundColor;
                Box.Title = title;
                Box.Content = sp1;
                Box.Closing += Box_Closing;
                TextBlock content = new TextBlock();
                content.TextWrapping = TextWrapping.Wrap;
                content.Background = null;
                content.HorizontalAlignment = HorizontalAlignment.Center;
                content.Text = boxcontent;
                content.FontFamily = font;
                content.FontSize = FontSize;
                sp1.Children.Add(content);

                input.Background = InputBackgroundColor;
                input.FontFamily = font;
                input.FontSize = FontSize;
                input.HorizontalAlignment = HorizontalAlignment.Center;
                input.Text = defaulttext;
                input.MinWidth = 200;
                input.MouseEnter += input_MouseDown;
                sp1.Children.Add(input);
                ok.Width = 70;
                ok.Height = 30;
                ok.Click += ok_Click;
                ok.Content = okbuttontext;
                ok.HorizontalAlignment = HorizontalAlignment.Center;
                sp1.Children.Add(ok);

            }

            void Box_Closing(object sender, System.ComponentModel.CancelEventArgs e)
            {
                if (!clicked)
                    e.Cancel = true;
            }

            private void input_MouseDown(object sender, MouseEventArgs e)
            {
                if ((sender as TextBox).Text == defaulttext && inputreset == false)
                {
                    (sender as TextBox).Text = null;
                    inputreset = true;
                }
            }

            void ok_Click(object sender, RoutedEventArgs e)
            {
                clicked = true;
                if (input.Text == defaulttext || input.Text == "")
                    MessageBox.Show(errormessage, errortitle);
                else
                {
                    Box.Close();
                }
                clicked = false;
            }

            public string ShowDialog()
            {
                Box.ShowDialog();
                return input.Text;
            }
        }

        public void ConnectToMysql()
        {
            string serverName = "localhost"; // Адрес сервера 
            string userName = "root"; // Имя пользователя
            string dbName = "model_ps"; //Имя базы данных
            string port = "3306"; // Порт для подключения
            string password = ""; // Пароль для подключения
            string connStr = "server=" + serverName +
                ";user=" + userName +
                ";database=" + dbName +
                ";port=" + port +
                ";password=" + password + ";";
            conn = new MySqlConnection(connStr);
            conn.Open();
        }
        public string MySqlRequest(string sql)
        {
            //ConnectToMysql();
            MySqlCommand sqlCom = new MySqlCommand(sql, conn);
            sqlCom.ExecuteNonQuery();
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);
            DataTable dt = new DataTable();
            dataAdapter.Fill(dt);
            string ans = null;// контейнер для формирования данных из запроса
            var myData = dt.Select();
            for (int i = 0; i < myData.Length; i++)
            {
                for (int j = 0; j < myData[i].ItemArray.Length; j++)
                {
                    ans = Convert.ToString(myData[i][j]);//формирования контейнера
                }
            }
            return ans;//возврат контейнера
        }
        public bool CheckVendor(string vendor)
        {
            bool code = false;
            try
            {
                ConnectToMysql();
            }
            catch (Exception) { code = false; }
            string ans;
            string sql = "SELECT `ID` FROM `vendors` WHERE `vendors`.`Vendor`='" + vendor + "'";
            try
            {
                ans = MySqlRequest(sql);
            }
            catch (Exception) { ans = null; }
            if (ans == null)
            {
                code = false;
            }
            else code = true;
            return code;
        }
        public static StreamReader ExecuteCommandLine(String file, String arguments = "")
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();

            //Задаем значение, указывающее необходимость запускать
            //процесс в новом окне.
            startInfo.CreateNoWindow = true;

            //Устанавливаем скрытый стиль окна. Окно может быть видимым или скрытым.
            //Система отображает скрытое окно, не прорисовывая его.
            //Если окно скрыто, оно эффективно отключено.
            //Скрытое окно может обрабатывать сообщения от системы или
            //от других окон, но не может обрабатывать ввод от пользователя
            //или отображать вывод. Часто, приложение может держать новое окно
            //скрытым, пока приложение определит внешний вид окна, а затем
            //сделать стиль окна Normal.
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            //Задаем значение, указывающее, что не нужно использовать
            //оболочку операционной системы для запуска процесса.
            startInfo.UseShellExecute = false;

            //Задаем значение, указывающее необходимость записывать выходные
            //данные приложения в поток System.Diagnostics.Process.StandardOutput.
            startInfo.RedirectStandardOutput = true;

            //Задаем приложение для запуска.
            startInfo.FileName = file;

            //Задаем набор аргументов командной строки, используемых при
            //запуске приложения.
            startInfo.Arguments = arguments;

            //Задаем предпочтительную кодировку для стандартного вывода.
            startInfo.StandardOutputEncoding = Encoding.GetEncoding(866);

            //Запускам ресурс процесса, с указанными выше параметрами и связываем
            //ресурс с новым компонентом System.Diagnostics.Process.
            Process process = Process.Start(startInfo);

            //Возвращаем System.IO.StreamReader, который может использоваться
            //для чтения потока стандартного вывода приложения.
            return process.StandardOutput;
        }
        public async Task<List<ARP>> ButtonAction(List<ARP> x)
        {
            //List <ARP> arpTable = new List<ARP> { };
            var arpStream = ExecuteCommandLine("arp", "-a");

            //Удаляем первые три строки, т.к они содержат
            //пустую строку
            //имя интерфейса
            //заголовки столбцов
            for (int i = 0; i < 3; i++)
            {
                arpStream.ReadLine();
            }

            //Циклически проходим по входному потоку
            //Пока функция EndOfStream не вернет значение true
            //указывающая, что текущая позиция потока
            //находится в конце потока
            while (!arpStream.EndOfStream)
            {
                //Получаем одну строку из текущего потока
                var line = arpStream.ReadLine().Trim();

                //Так как между столбцами есть несколько пробелов
                //их необходимо сократить до одного
                while (line.Contains("  "))
                {
                    line = line.Replace("  ", " ");
                }

                //Чтобы распределить полученные данные по столбцам таблицы их
                // необходимо разделить с помощью метода Split
                // который возвращает массив, элементы которого содержат
                //подстроки данного экземпляра, разделенные одним или более
                //знаками указанных в его значении.
                var parts = line.Split(' ');

                //Если значение первого столбца пустое, значит
                //данную строку необходимо пропустить

                if (parts[0].Trim() != string.Empty)
                {
                    if (parts[2].Trim() == "динамический")
                    {
                        Thread.Sleep(1000);
                        string n_mac = parts[1].Trim().Replace('-', ':');//замена символовок в строке мак адреса
                        string l_vendor = await ConnectAsync(n_mac);
                        try
                        {
                            if (!CheckVendor(l_vendor))
                            {
                                string sql = "INSERT IGNORE INTO `vendors`(`Vendor`) VALUES('" + l_vendor + "')";
                                string ans = MySqlRequest(sql);

                            }
                        }
                        catch (Exception) {}
                        
                            x.Add(new ARP { IP= parts[0].Trim(),MAC= n_mac,Type = parts[2].Trim(), Vendor = l_vendor, device = new Device(), Num_port = 0, Model = "non", OS = "non", Vlan = "non", buildings = new buildings(), Hall = 0, Inventory = "non", Link = 1 });//заполнение полей класса АРП

                        }
                    }

                }
                return x;//возвращаем массив класса АРП


            }
        int rebuilding(buildings b)
        {
            string str = Convert.ToString(b);
            switch (str)
            {
                case "I": {
                        return 1;
                       
                    }
                case "II":
                    {
                        return 2;
                        
                    }
                case "III":
                    {
                        return 3;
                        
                    }
                case "IV":
                    {
                        return 4;
                        
                    }
                case "V":
                    {
                        return 5;
                        
                    }
            }return 0;
        }
        public void btn_db_action()
        {
            try
            {
                ConnectToMysql();
                string l_ip = null;
                string l_mac = null;
                string l_vendor = null;
                Device l_device;
                int l_hn_port;
                string l_model = null;
                string l_os = null;
                string l_vlan = null;
                buildings l_buildings;
                int l_hall;
                string l_invent = null;
                int l_link;
                string l_mark = null;
                int ind;
                l_mark = new InputBox("Введите короткий комментарий").ShowDialog();
                
                for (int i = 0; i < arpTable.Count; i++)
                {

                    var local = arpTable.ElementAt(i);
                    l_ip = local.IP;
                    l_mac = local.MAC;
                    l_vendor = local.Vendor;
                    l_device = local.device;
                    string l_d = Convert.ToString(l_device);
                    string sql = "SELECT `ID` FROM `devices` WHERE `Device`='" + l_d + "'";
                    int loc_d = Convert.ToInt32(MySqlRequest(sql));
                    l_hn_port = local.Num_port;
                    l_model = local.Model;
                    l_os = local.OS;
                    l_vlan = local.Vlan;
                    l_buildings = local.buildings;
                    int l_b=rebuilding(l_buildings);
                    
                    l_hall = local.Hall;
                    l_invent = local.Inventory;
                    l_link = local.Link;
                   
                    
                    
                    sql = "SELECT `ID` FROM `vendors` WHERE `vendors`.`Vendor`='" + l_vendor + "'";
                    ind = Convert.ToInt32(MySqlRequest(sql));
                    string main_sql = "INSERT IGNORE INTO `scan_tab`(`IP`, `MAC`, `Vendor`, `Device`, `Num_ports`, `Model`, `OS`, `Vlan`, `Building`, `Hall`, `Inventory`, `Link`, `Date`, `Comment`) VALUES ('"+l_ip+"','"+l_mac+"',"+ind+","+loc_d+","+l_hn_port+",'"+l_model+"','"+l_os+"','"+l_vlan+"',"+l_b+","+l_hall+",'"+l_invent+"',1,CURRENT_DATE,'"+l_mark+"')";
                    string ans = MySqlRequest(main_sql);
                   // MessageBox.Show(ans);
                }
                MessageBox.Show("Данные внесены");
            }
            catch (Exception) { MessageBox.Show("Подключение к серверу отсутсвует!\nНевозможно внести данные в БД!"); }

        }

        public static async Task<string> ConnectAsync(string mac)// работа АПИ для определения вендора
        {
            WebRequest request = WebRequest.Create("https://api.macvendors.com/" + mac);
            request.Method = "GET";
            WebResponse response = await request.GetResponseAsync();
            string answer = string.Empty;
            using (Stream s = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    answer = await reader.ReadToEndAsync();
                    response.Close();
                    return answer;
                }
            }
        }
        public void MySqlRequest_void_g(string sql, List<ARP_ans_all> ans)
        {
            ConnectToMysql();
            MySqlCommand sqlCom = new MySqlCommand(sql, conn);
            
            sqlCom.ExecuteNonQuery();
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);
            DataTable dt = new DataTable();
            dataAdapter.Fill(dt);
            // контейнер для формирования данных из запроса
            var myData = dt.Select();
            for (int i = 0; i < myData.Length; i++)
            {

                ans.Add(new ARP_ans_all { IP = Convert.ToString(myData[i][0]), MAC = Convert.ToString(myData[i][1]), Vendor = Convert.ToString(myData[i][2]), device = Convert.ToString(myData[i][3]), Num_port = Convert.ToInt32(myData[i][4]),Model=Convert.ToString(myData[i][5]), OS= Convert.ToString(myData[i][6]),Vlan= Convert.ToString(myData[i][7]),buildings= Convert.ToString(myData[i][8]),Hall=Convert.ToInt32(myData[i][9]),Inventory=Convert.ToString(myData[i][10]),Link=Convert.ToString(myData[i][11]), date = Convert.ToString(myData[i][12]), comment = Convert.ToString(myData[i][13]) });




            }
        }
        public void MySqlRequest_void_short(string sql, List<ARP_short> ans)
        {
            ConnectToMysql();
            MySqlCommand sqlCom = new MySqlCommand(sql, conn);

            sqlCom.ExecuteNonQuery();
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sqlCom);
            DataTable dt = new DataTable();
            dataAdapter.Fill(dt);
            // контейнер для формирования данных из запроса
            var myData = dt.Select();
            for (int i = 0; i < myData.Length; i++)
            {

                ans.Add(new ARP_short {vendor = Convert.ToString(myData[i][0]), device = Convert.ToString(myData[i][1]), address = Convert.ToString(myData[i][2]), hall = Convert.ToInt32(myData[i][3]), inventory = Convert.ToString(myData[i][4]) });




            }
        }

        private async void  btn_scan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                arpTable = await ButtonAction(arpTable);
               
            }
            catch (Exception) { MessageBox.Show("Ошибка!"); }
            if (arpTable.Count != 0)
            {
                ArpPanel.ItemsSource = arpTable;
            }
            else MessageBox.Show("Найти устройства не удалось!\nВозможно у вас отсутсвует подключение к сети или в Вашей сети нет активных устройств");
        }

        private void Btn_clear_Click(object sender, RoutedEventArgs e)
        {
            ArpPanel.ItemsSource = "";
            arpTable.Clear();
        }

        private void Btn_db_Click(object sender, RoutedEventArgs e)
        {
            if (arpTable.Count == 0)
            {
                MessageBox.Show("Нет данных для записи!\nВыполните сканирование!");
            }
            else
            {

                btn_db_action();

            }
        }

        private void Cmb_first_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            scan_dg.DataContext = "";
            ans_Alls.Clear();
            shorts.Clear();
            switch (cmb_first.SelectedIndex)
            {
                case 0:
                    {
                        string sql = "SELECT `scan_tab`.`IP`, `scan_tab`.`MAC`, `vendors`.`Vendor`, `devices`.`Device`, `scan_tab`.`Num_ports`, `scan_tab`.`Model`,`scan_tab`.`OS`, `scan_tab`.`Vlan`, `buildings`.`Number`,`scan_tab`.`Hall`, `scan_tab`.`Inventory`, `scan_tab`.`Link`, `scan_tab`.`Date`, `scan_tab`.`Comment` FROM `scan_tab` JOIN `vendors` JOIN `devices` JOIN`buildings` ON `scan_tab`.`Vendor`=`vendors`.`ID` AND `scan_tab`.`Device`=`devices`.`ID` AND `scan_tab`.`Building`=`buildings`.`ID`";
                        MySqlRequest_void_g(sql, ans_Alls);
                        scan_dg.DataContext = ans_Alls;

                        break;
                    }
                case 1:
                    {
                        string sql = "SELECT `scan_tab`.`IP`, `scan_tab`.`MAC`, `vendors`.`Vendor`, `devices`.`Device`, `scan_tab`.`Num_ports`, `scan_tab`.`Model`,`scan_tab`.`OS`, `scan_tab`.`Vlan`, `buildings`.`Number`,`scan_tab`.`Hall`, `scan_tab`.`Inventory`, `scan_tab`.`Link`, `scan_tab`.`Date`, `scan_tab`.`Comment` FROM `scan_tab` JOIN `vendors` JOIN `devices` JOIN`buildings` ON `scan_tab`.`Vendor`=`vendors`.`ID` AND `scan_tab`.`Device`=`devices`.`ID` AND `scan_tab`.`Building`=`buildings`.`ID` AND NOT(`devices`.`Device`='Конечная_станция')";
                        MySqlRequest_void_g(sql, ans_Alls);
                        scan_dg.DataContext = ans_Alls;
                        break;
                    }
                case 2:
                    {
                        string sql = "SELECT `vendors`.`Vendor`, `devices`.`Device`,`buildings`.`Address`,`scan_tab`.`Hall`,`scan_tab`.`Inventory` from `scan_tab` JOIN `vendors` join `devices` join `buildings` on `scan_tab`.`Vendor`=`vendors`.`ID` and `scan_tab`.`Device`=`devices`.`ID` and `scan_tab`.`Building`=`buildings`.`ID`";
                        MySqlRequest_void_short(sql, shorts);
                        scan_dg.DataContext = shorts;
                        break;
                    }
                case 3:
                    {
                        string sql = "SELECT `scan_tab`.`IP`, `scan_tab`.`MAC`, `vendors`.`Vendor`, `devices`.`Device`, `scan_tab`.`Num_ports`, `scan_tab`.`Model`,`scan_tab`.`OS`, `scan_tab`.`Vlan`, `buildings`.`Number`,`scan_tab`.`Hall`, `scan_tab`.`Inventory`, `scan_tab`.`Link`, `scan_tab`.`Date`, `scan_tab`.`Comment` FROM `scan_tab` JOIN `vendors` JOIN `devices` JOIN`buildings` ON `scan_tab`.`Vendor`=`vendors`.`ID` AND `scan_tab`.`Device`=`devices`.`ID` AND `scan_tab`.`Building`=`buildings`.`ID` AND `devices`.`Device`= 'Конечная_станция'";
                        MySqlRequest_void_g(sql, ans_Alls);
                        scan_dg.DataContext = ans_Alls;
                        
                        break;
                    }
                case 4:
                    {//поиск по аудиториям
                        break;
                    }
                case 5:
                    {// поиск по корпусам (номеру корпуса)
                        break;

                    }
                case 6:
                    {//поиск по типу устройства
                        break;
                    }
                default: { break; }
            }
        }

        private void Btn_export_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Office.Interop.Excel._Application app = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel._Workbook workbook = app.Workbooks.Add(Type.Missing);
            Microsoft.Office.Interop.Excel._Worksheet worksheet = null;

            if (scan_dg.Columns.Count != 0)
            {
                app.Visible = true;
                worksheet = workbook.Sheets["Лист1"];
                worksheet = workbook.ActiveSheet;
                for (int i = 1; i < scan_dg.Columns.Count + 1; i++)
                {
                    worksheet.Cells[1, i] = scan_dg.Columns[i - 1].Header;
                }

                var t = scan_dg.Items;
                //в зависимости от выбраного пункта в комбобоксе портировать нужный набор данных!


               
            }
            else MessageBox.Show("Нет данных для экспорта. Произведите выборку!");

        }
    }
}
