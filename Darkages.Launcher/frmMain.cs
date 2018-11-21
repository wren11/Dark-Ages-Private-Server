///************************************************************************
//Project Lorule: A Dark Ages Server (http://darkages.creatorlink.net/index/)
//Copyright(C) 2018 TrippyInc Pty Ltd
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
//*************************************************************************/
using Binarysharp.MemoryManagement;
using DevExpress.XtraEditors;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ClientLauncher.frmMain.Configuration;

namespace ClientLauncher
{
    public partial class frmMain : XtraForm
    {
        public struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        public static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern bool CreateProcess(string lpApplicationName,
                   string lpCommandLine, IntPtr lpProcessAttributes,
                   IntPtr lpThreadAttributes,
                   bool bInheritHandles, ProcessCreationFlags dwCreationFlags,
                   IntPtr lpEnvironment, string lpCurrentDirectory,
                   ref STARTUPINFO lpStartupInfo,
                   out PROCESS_INFORMATION lpProcessInformation);

            [DllImport("kernel32.dll")]
            public static extern uint ResumeThread(IntPtr hThread);

            [DllImport("kernel32.dll")]
            public static extern uint SuspendThread(IntPtr hThread);
        }

        [Flags]
        public enum ProcessCreationFlags : uint
        {
            ZERO_FLAG = 0x00000000,
            CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_NO_WINDOW = 0x08000000,
            CREATE_PROTECTED_PROCESS = 0x00040000,
            CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
            CREATE_SEPARATE_WOW_VDM = 0x00001000,
            CREATE_SHARED_WOW_VDM = 0x00001000,
            CREATE_SUSPENDED = 0x00000004,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            DEBUG_ONLY_THIS_PROCESS = 0x00000002,
            DEBUG_PROCESS = 0x00000001,
            DETACHED_PROCESS = 0x00000008,
            EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
            INHERIT_PARENT_AFFINITY = 0x00010000
        }

        public Configuration config;

        public frmMain()
        {
            InitializeComponent();
            DoubleBuffered = true;


            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException1;

            config = Config.LoadConfig();


            if (config.Servers == null)
                config.Servers = new List<Server>();

            UpdateConfig(config);
        }

        private void CurrentDomain_UnhandledException1(object sender, UnhandledExceptionEventArgs e)
        {

        }

        public void UpdateConfig(Configuration config)
        {
            comboBox1.DataSource = null;
            comboBox1.DataSource = config.Servers.Select(i => i.ServerName).ToList();
        }

        public class Config
        {
            public static string ConfigPath = Environment.CurrentDirectory;

            static Config()
            {
                var path = Path.Combine(ConfigPath, "lorule_config.xml");

                if (!File.Exists(path))
                {
                    using (var client = new WebClient())
                    {
                        try
                        {
                            var str = client.DownloadString("http://lorule-da.com/lorule_config.xml");
                            var config = JsonConvert.DeserializeObject<Configuration>(str);
                            Config.Save(config);
                        }
                        catch
                        {
                            Config.Save(new Configuration());
                        }
                        finally
                        {
                        }
                    }
                }
            }

            public static Configuration Configs = new Configuration();

            public static Configuration LoadConfig()
            {
                var path = Path.Combine(ConfigPath, "lorule_config.xml");

                if (File.Exists(path))
                {
                    using (var s = File.OpenRead(path))
                    using (var f = new StreamReader(s))
                        return JsonConvert.DeserializeObject<Configuration>(f.ReadToEnd());
                }
                return null;
            }

            public static void Save(Configuration config)
            {
                var path = Path.Combine(ConfigPath, "lorule_config.xml");
                var objString = JsonConvert.SerializeObject(config);
                File.WriteAllText(path, objString);
            }

        }

        public class ServerCollection : CollectionBase
        {
            public void Add(Server emp)
            {
                List.Add(emp);
            }
            public void Remove(Server emp)
            {
                List.Remove(emp);
            }
        }

        public class ServerCollectionEditor : CollectionEditor
        {
            public ServerCollectionEditor(Type type)
            : base(type)
            {

            }

            protected override string GetDisplayText(object value)
            {
                var item = new Server();
                item = (Server)value;

                return base.GetDisplayText(string.Format("{0}", item.ServerName));
            }
        }


        public class Configuration
        {
            public int SelectedIndex { get; set; }

            [Editor(typeof(ServerCollectionEditor),
            typeof(System.Drawing.Design.UITypeEditor))]
            [Category("Add/Remove Server Configurations")]
            [DisplayName("Servers")]
            [Description("what we got.")]
            public List<Server> Servers { get; set; }

            public Configuration()
            {
            }

            public class Server
            {
                public string IPAddress { get; set; }
                public int Port { get; set; }
                public string ServerName { get; set; }
                public int PatchTable { get; set; }
                public int HookTable { get; set; }
                public int ClientVersion { get; set; }
            }
        }

        private void GetOps(Configuration config)
        {
            new ConfigOps(config).ShowDialog();
            Config.Save(config);
            UpdateConfig(config);
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            CheckAdmin();

            try
            {
                if (comboBox1.SelectedIndex < 0)
                    return;

                comboBox1.SelectedIndex = config.SelectedIndex;
            }
            catch
            {
                config.SelectedIndex = 0;
                Config.Save(config);
            }

            GetRemoteConfig();
        }

        public static bool Checked = false;

        public static Stack<string> Updates = new Stack<string>();

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(string.Format("Error {0}. Please try again.", e.ExceptionObject));
            Application.Exit();
        }

        private void GetRemoteConfig()
        {

            var daPath = @"..\7.18\Darkages.exe";

            if (config != null)
            {
                var darkages = File.ReadAllBytes(daPath);
                var length = darkages.Length;

                using (var client = new WebClient())
                {
                    client.DownloadDataCompleted += Client_DownloadDataCompleted;
                    client.DownloadProgressChanged += Client_DownloadProgressChanged;
                    client.DownloadDataAsync(new Uri("http://lorule-da.com/CLIENT_DATA/Darkages.exe"), "Darkages");
                }

                using (var client = new WebClient())
                {
                    client.DownloadDataCompleted   += Client_DownloadDataCompleted;
                    client.DownloadProgressChanged += Client_DownloadProgressChanged;
                    client.DownloadDataAsync(new Uri("http://lorule-da.com/CLIENT_DATA/plugins/EtDA.dll"), "lor");
                }

                Task.Run(() => CheckForUpdates(daPath)).ContinueWith((ct) =>
                {

                    if (Updates.Count > 0)
                    {

                        var activeupdates = 0;
                        var total = ((int)(object)Updates.Count);

                        KillClients();


                        while (Updates.Count > 0)
                        {
                            var update = Updates.Pop();


                            using (var client = new WebClient())
                            {

                                var daRelativePath = Path.GetDirectoryName(Path.GetFullPath(daPath));
                                var datPath = Path.Combine(daRelativePath, update);

                                client.DownloadDataCompleted += Client_DownloadDataCompleted;
                                client.DownloadProgressChanged += Client_DownloadProgressChanged;

                                Invoke((MethodInvoker)delegate ()
                                {
                                    progressBarControl1.EditValue = (int)(activeupdates * 100 / total);
                                    labelControl2.Text = string.Format("Downloading Updated Data: {0} Please Wait. {1}/{2}", update, activeupdates, total);
                                    marqueeProgressBarControl1.Visible = true;
                                    progressBarControl1.Visible = false;
                                });


                                var data = client.DownloadData(new Uri("http://lorule-da.com/CLIENT_DATA/data/" + update));

                                try
                                {
                                    KillClients();
                                    File.WriteAllBytes(datPath, data);
                                }
                                catch
                                {
                                    MessageBox.Show("Could not Update. Service may be unavailable.\nPlease check the Discord for server status.");
                                }



                            }

                            Invoke((MethodInvoker)delegate ()
                            {
                                marqueeProgressBarControl1.Visible = false;
                                progressBarControl1.Visible = true;

                                progressBarControl1.EditValue = (int)(activeupdates * 100 / total);
                                labelControl2.Text = string.Format("Downloading Updated Data: {0} Please Wait. {1}/{2}", update, activeupdates, total);
                            });


                            activeupdates++;
                        }
                    }

                    Checked = true;
                });


 



            }
        }

        private static void CheckForUpdates(string daPath)
        {
            using (var client = new WebClient())
            {
                var datInfo = client.DownloadString("http://lorule-da.com/datidx.txt");
                var datInfoLines = datInfo.Split(new char[] { '\n' });


                foreach (var line in datInfoLines)
                {
                    var path = line.TrimEnd('\r').Split(new char[] { '%' }, StringSplitOptions.RemoveEmptyEntries);
                    var daRelativePath = Path.GetDirectoryName(Path.GetFullPath(daPath));
                    var dat = Path.Combine(daRelativePath, path[0]);
                    var remotecrc = path[1];

                    var datbin = File.Exists(dat) ? File.ReadAllBytes(dat) : new byte[2] { 0x7F, 0x91 };
                    var crc = MD5.Create();

                    crc.ComputeHash(datbin);

                    var localcrc = Convert.ToBase64String(crc.Hash);


                    if (localcrc != remotecrc)
                    {
                        Updates.Push(path[0]);
                    }
                }




            }
        }

        private static void CheckAdmin()
        {
            bool isElevated;
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            if (!isElevated)
            {
                MessageBox.Show("Please restart and set to run-as Administrator.");
                Environment.Exit(2);
            }
        }

        public static string LOR_DLL = string.Empty;

        private void Client_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {

            try
            {
                if (((string)e.UserState == "Darkages"))
                {
                    Invoke((MethodInvoker)delegate ()
                    {
                        var daPath = @"..\7.18\Darkages.exe";
                        var darkages = File.ReadAllBytes(daPath);
                        var length = darkages.Length;

                        if (e.Result.Length == darkages.Length)
                        {

                            var m = MD5.Create();
                            var a = m.ComputeHash(e.Result);
                            var b = m.ComputeHash(darkages);

                            if (!a.SequenceEqual(b))
                            {
                                MessageBox.Show("Error: Client modification detected. Please try again.");
                                Environment.Exit(0);
                            }
                        }
                    });
                }
                else if ((string)e.UserState == "lor")
                {
                    LOR_DLL = Path.Combine(Environment.CurrentDirectory, "lor.dll");
                    if (File.Exists(LOR_DLL))
                    {

                        try
                        {
                            var crc = MD5.Create();

                            var a = crc.ComputeHash(e.Result);
                            var b = crc.ComputeHash(File.ReadAllBytes(LOR_DLL));

                            if (!a.SequenceEqual(b))
                            {
                                KillClients();

                                File.WriteAllBytes(LOR_DLL, e.Result);
                            }
                        }
                        catch 
                        {
                            MessageBox.Show("Could not Update. Service may be unavailable.\nPlease check the Discord for server status.");
                        }
                    }
                }

            }
            catch 
            {
                MessageBox.Show("Error 12. Please try again.");
                Application.Exit();
            }

        }

        private static void KillClients()
        {
            var clients = Process.GetProcessesByName("Darkages");
            foreach (var client in clients)
            {
                client.Kill();
            }
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Invoke((MethodInvoker)delegate ()
            {
                progressBarControl1.EditValue = e.ProgressPercentage;
                labelControl2.Text = string.Format("Downloading Data... Please Wait. {0}/{1}  ({2}%)", e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage);
            });
        }

        private void pictureEdit1_EditValueChanged(object sender, EventArgs e)
        {

        }

        private void pictureEdit1_Click(object sender, EventArgs e)
        {
            var daPath = @"..\7.18\Darkages.exe";
            var server = config.Servers.Find(i => i.ServerName == comboBox1.Text);
            if (server == null)
            {
                MessageBox.Show("Error, Check config.");
                return;
            }

            STARTUPINFO si = new STARTUPINFO();
            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
            bool success = NativeMethods.CreateProcess(daPath, null,
                IntPtr.Zero, IntPtr.Zero, false,
                ProcessCreationFlags.CREATE_SUSPENDED,
                IntPtr.Zero, null, ref si, out pi);

            MemorySharp memory;
            try
            {
                memory = new MemorySharp((int)pi.dwProcessId);
            }
            catch (Exception error)
            {
                MessageBox.Show(string.Format("Fatal Launch Error: {0}", error.Message + "\n" + error.StackTrace));
                File.WriteAllText("lorule_lerror.txt", string.Format("Fatal Launch Error: {0}", error.Message + "\n" + error.StackTrace));
                return;
            }
            var payload = new byte[7];
            var segments = server.IPAddress.Split('.');


            if (server.IPAddress.Contains(".com") || server.IPAddress.Contains("www.") || segments.Length != 4)
            {
                var ip = Dns.GetHostAddresses(server.IPAddress)[0];
                var ipString = ip.ToString();

                payload[0] = Convert.ToByte(ipString[3]);
                payload[1] = 0x6A;
                payload[2] = Convert.ToByte(ipString[2]);
                payload[3] = 0x6A;
                payload[4] = Convert.ToByte(ipString[1]);
                payload[5] = 0x6A;
                payload[6] = Convert.ToByte(ipString[0]);

                memory.Write((IntPtr)(0x400000 + server.HookTable), payload, false);
                memory.Write((IntPtr)(0x400000 + server.PatchTable), payload, false);

            }
            else
            {
                if (server.ClientVersion == 718 && segments.Length == 4)
                {
                    payload[0] = Convert.ToByte(segments[3]);
                    payload[1] = 0x6A;
                    payload[2] = Convert.ToByte(segments[2]);
                    payload[3] = 0x6A;
                    payload[4] = Convert.ToByte(segments[1]);
                    payload[5] = 0x6A;
                    payload[6] = Convert.ToByte(segments[0]);

                    memory.Write((IntPtr)(0x400000 + server.HookTable), payload, false);
                    memory.Write((IntPtr)(0x400000 + server.PatchTable), payload, false);
                }
            }

            IntPtr ThreadHandle = pi.hThread;
            NativeMethods.ResumeThread(ThreadHandle);

            CheckAdmin();

            try
            {
                var Memory = new MemorySharp((int)pi.dwProcessId);
                {

                    var injection = Memory.Modules.Inject(LOR_DLL);

                    if (injection.IsValid)
                    {
                        Console.Beep();
                    }

                }
            }
            catch
            {

            }
        }

        private void pictureEdit2_EditValueChanged(object sender, EventArgs e)
        {

        }

        private void pictureEdit2_Click(object sender, EventArgs e)
        {
            GetOps(config);
        }

        private void hyperlinkLabelControl1_Click(object sender, EventArgs e)
        {
            Process.Start("http://lorule-da.com");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Checked)
            {
                Checked = false;
                Console.Beep();

                Invoke((MethodInvoker)delegate ()
                {
                    if (!pictureEdit1.Enabled)
                    {
                        pictureEdit1.Enabled = true;
                        labelControl2.Text = "Ready 2 Play!";
                        progressBarControl1.EditValue = 100;
                        comboBox1.Enabled = true;
                        this.pictureEdit1.Focus();
                        Application.DoEvents();
                    }
                });
            }
        }
    }
}
