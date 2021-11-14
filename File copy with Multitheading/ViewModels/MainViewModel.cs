using File_copy_with_Multitheading.Commands;
using File_copy_with_Multitheading.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace File_copy_with_Multitheading.ViewModels
{

    public class MainViewModel : BaseViewModel
    {
        public MainWindow MainWindows { get; set; }

        public RelayCommand LoadedCommand { get; set; }
        public RelayCommand FromFileCommand { get; set; }
        public RelayCommand ToFileCommand { get; set; }
        public RelayCommand EncryptDecryptCommand { get; set; }
        public RelayCommand CopyStartCommand { get; set; }
        public RelayCommand CopyPausePlayCommand { get; set; }
        public RelayCommand CopyCancelCommand { get; set; }
        public RelayCommand ProgressBarValueChangedCommand { get; set; }


        public ObservableCollection<Texts> Texts { get; set; }

        private Texts _Text;
        public Texts Text { get { return _Text; } set { _Text = value; OnPropertyChanged(); } }

        public List<string> ComboBoxList = new List<string>();

        public Stopwatch watch = Stopwatch.StartNew();

        string text = string.Empty;

        string f = string.Empty;
        string t = string.Empty;
        int c = -1;
        double p = 0.0;
        double pMax = 0.0;
        double pMin = 0.0;
        long length1 = 0;
        long length2 = 0;
        int pauseclick=0;
        bool running = false;
        int count = 100;
        int currentcount = 0;

        int count2 = 50;

        private double _pv;
        public double Pv { get { return _pv; } set { _pv = value; OnPropertyChanged(); } }

        int threadcount = 1001;
        public MainViewModel()
        {

            DispatcherTimer timer = new DispatcherTimer();

            timer.Tick += Timer_Tick;




            Thread[] thread = new Thread[threadcount];

            for (int i = 0; i < threadcount; i++)
            {
                thread[i] = new Thread(() =>
                {


                    CopyDecryption(text, f, t, c, watch);


                });
            }






            LoadedCommand = new RelayCommand((sender) =>
            {
                timer.Start();

                ComboBoxList.Add("Encryption text");
                ComboBoxList.Add("Non Protected text");

                MainWindows.EncryptDecryptComboBox.ItemsSource = ComboBoxList;

                //MessageBox.Show($"{thread.IsAlive}");


            });


            FromFileCommand = new RelayCommand((sender) =>
            {

                OpenFileDialog openFile = new OpenFileDialog();

                //openFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                openFile.InitialDirectory = Path.GetFullPath(Environment.CurrentDirectory + @"../../../../");
                openFile.Filter = "Text files (*.txt)|*.txt";

                if (openFile.ShowDialog() == true)
                {
                    MainWindows.FromPathTextBox.Text = openFile.FileName;
                }



            });


                OpenFileDialog saveFile = new OpenFileDialog();
            ToFileCommand = new RelayCommand((sender) =>
            {

                saveFile.InitialDirectory = Path.GetFullPath(Environment.CurrentDirectory + @"../../../../");
                saveFile.Filter = "Text files (*.txt)|*.txt";

                if (saveFile.ShowDialog() == true)
                {
                    MainWindows.ToPathTextBox.Text = saveFile.FileName;

                }




            });


            CopyStartCommand = new RelayCommand((sender) =>
            {
                Text = new Texts();

                timer.Start();

                threadcount--;

                //MessageBox.Show($"{threadcount}");


                if (MainWindows.FromPathTextBox.Text == MainWindows.ToPathTextBox.Text)
                {
                    MessageBox.Show($"The paths are the same.");
                }
                else
                {
                    try
                    {

                        thread.ElementAt(threadcount).Start();

                    }
                    catch (Exception)
                    {


                    }

                    timer.Stop();

                    Text = new Texts();
                }


            });


            CopyPausePlayCommand = new RelayCommand((sender) =>
            {
          
                Text = new Texts();
                pauseclick++;


                timer.Start();
                try
                {
                  
                    if (!string.IsNullOrEmpty(MainWindows.FromPathTextBox.Text) || !string.IsNullOrEmpty(MainWindows.ToPathTextBox.Text) || MainWindows.ToPathTextBox.Text != "Path" || MainWindows.FromPathTextBox.Text != "Path")
                    {
                        if (pauseclick % 2 != 0)
                        {
                            MainWindows.CopyPausePlayButton.Content = "Copy Play";
                            thread.ElementAt(threadcount).Suspend();
                        }

                        else
                        {
                            MainWindows.CopyPausePlayButton.Content = "Copy Pause";
                            thread.ElementAt(threadcount).Resume();
                        }
                    }

                    else
                    {
                        MessageBox.Show("Select Path.");
                    }

                }
                catch (Exception)
                {


                }

                timer.Stop();

                Text = new Texts();

            });


            CopyCancelCommand = new RelayCommand((sender) =>
            {
                Text = new Texts();


                timer.Start();
                try
                {



                    if (!string.IsNullOrEmpty(MainWindows.FromPathTextBox.Text) || !string.IsNullOrEmpty(MainWindows.ToPathTextBox.Text) || MainWindows.ToPathTextBox.Text != "Path" || MainWindows.FromPathTextBox.Text != "Path")
                    {

                        for (int j = currentcount; j >= 0; j--)
                        {
                            Thread.Sleep(50);
                            Pv = j;
                        }


                        thread.ElementAt(threadcount).Abort();
                    }

                    else
                    {
                        MessageBox.Show("Select Path.");
                    }

                }
                catch (Exception)
                {

                }

                timer.Stop();

                Text = new Texts();

            });



        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            MainWindows.CopyStartButton.Dispatcher.BeginInvoke(new Action(() =>
            {//this refer to form in WPF application 


                f = MainWindows.FromPathTextBox.Text;
                t = MainWindows.ToPathTextBox.Text;
                c = MainWindows.EncryptDecryptComboBox.SelectedIndex;
                p = MainWindows.ProcessControlProgressBar.Value;
                pMax = MainWindows.ProcessControlProgressBar.Maximum;
                pMin = MainWindows.ProcessControlProgressBar.Minimum;

            }));
        }


        static void FileStreamWrite(string text, string f)
        {
            try
            {
                using (FileStream fs = new FileStream($" {f}", FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {

                    byte[] bytes = Encoding.UTF8.GetBytes(text);
                    fs.Write(bytes, 0, bytes.Length);

                }

            }
            catch (Exception)
            {


            }

        }

        static void FileStreamRead(string text, string f)
        {
            try
            {
                using (FileStream fs = new FileStream($"{f}", FileMode.OpenOrCreate, FileAccess.Read, FileShare.None))
                {
                    byte[] bytes = new byte[(int)fs.Length];
                    fs.Read(bytes, 0, bytes.Length);
                    text = Encoding.UTF8.GetString(bytes);

                }

            }
            catch
            {

            }

        }

        string  Encrypt(string text)
        {
            byte[] entropy = Encoding.UTF8.GetBytes(Assembly.GetExecutingAssembly().FullName);
            byte[] data = UTF8Encoding.UTF8.GetBytes(text);
            string protectedData = Convert.ToBase64String(ProtectedData.Protect(data, entropy, DataProtectionScope.CurrentUser));
            
            return protectedData;

        }

         string Decrpyt(string text)
        {
            byte[] protectedData = Convert.FromBase64String(text);
            byte[] entropy = Encoding.UTF8.GetBytes(Assembly.GetExecutingAssembly().FullName);
            string data = Encoding.UTF8.GetString(ProtectedData.Unprotect(protectedData, entropy, DataProtectionScope.CurrentUser));
            return data;
        }

        void CopyDecryption(string text, string f, string t, int c, Stopwatch watch)
        {
            
                 length1 = new System.IO.FileInfo(f).Length;
                 length2 = new System.IO.FileInfo(t).Length;


            if (c == 1)
            {

        

                for (int i = 0; i < count; i++)
                {
                    Thread.Sleep(count2);

                    if (i == count - 1)
                    {

                        for (int j = 0; j < 101; j++)
                        {
                            Thread.Sleep(50);
                            Pv = j;

                            if (length1 >= 0.005 && length2 >= 0.005)
                            {
                                MessageBox.Show("File is not empty");

                                break;
                            }

                            else
                            {

                                if (Pv == 100)
                                {
                                    try
                                    {
                                        text = Text.words();

                                        FileStreamWrite(text, f);
                                        FileStreamRead(text, f);

                                        File.Copy(f, t, true);

                                        Thread.Sleep(500);

                                        Pv = 0;

                                    }
                                    catch (Exception)
                                    {

                                    }
                                }

                                if (Pv != 100)
                                {
                                    try
                                    {

                                        text = Text.words().Substring(0, Convert.ToInt32(100 * (j++)));

                                        FileStreamWrite(text, f);
                                        FileStreamRead(text, f);

                                        File.Copy(f, t, true);

                                        Thread.Sleep(500);

                                       // Pv = 0;
                                    }
                                    catch (Exception)
                                    {


                                    }

                                }

                                currentcount = int.Parse(Pv.ToString());

                            }
                        }
                    }
                }

        


                watch.Stop();

               
            }

            if (c == 0)
            {



                for (int i = 0; i < count; i++)
                {
                    Thread.Sleep(count2);

                    if (i == count - 1)
                    {

                        for (int j = 0; j < 101; j++)
                        {
                            Thread.Sleep(50);
                            Pv = j;

                            if (length1 >= 0.005 && length2 >= 0.005)
                            {
                                MessageBox.Show("File is not empty");

                                break;
                            }

                            else
                            {

                                if (Pv == 100)
                                {
                                    try
                                    {

                                        text = Encrypt(Text.words());

                                        FileStreamWrite(text, f);
                                        FileStreamRead(text, f);

                                        File.Copy(f, t, true);

                                        Thread.Sleep(500);

                                        Pv = 0;


                                    }
                                    catch (Exception)
                                    {

                                    }
                                }

                                if (Pv != 100)
                                {
                                    try
                                    {

                                        text = Encrypt(Text.words().Substring(0, Convert.ToInt32(100 * (j++))));

                                        FileStreamWrite(text, f);
                                        FileStreamRead(text, f);

                                        File.Copy(f, t, true);

                                        Thread.Sleep(500);

                                        
                                        // Pv = 0;
                                    }
                                    catch (Exception)
                                    {


                                    }

                                }

                                currentcount = int.Parse(Pv.ToString());

                            }
                        }
                    }
                }




                watch.Stop();


            }



        }

    }
}
