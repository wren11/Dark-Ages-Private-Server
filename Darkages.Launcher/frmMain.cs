using Binarysharp.MemoryManagement;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static ClientLauncher.frmMain.Configuration;

namespace ClientLauncher
{
    public partial class frmMain : Form
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
            config = Config.LoadConfig();


            if (config.Servers == null)
                config.Servers = new List<Server>();

            UpdateConfig(config);
        }

        public void UpdateConfig(Configuration config)
        {
            comboBox1.DataSource = null;
            comboBox1.DataSource = config.Servers.Select(i => i.ServerName).ToList();
        }

        public class Config
        {
            public static string ConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            static Config()
            {
                ConfigPath = Path.Combine(ConfigPath, "Darkages_Client");

                var path = Path.Combine(ConfigPath, "client_config.xml");

                if (!Directory.Exists(ConfigPath))
                    Directory.CreateDirectory(ConfigPath);

                if (!File.Exists(path))
                    Config.Save(new Configuration());
            }

            public static Configuration Configs = new Configuration();

            public static Configuration LoadConfig()
            {
                var path = Path.Combine(ConfigPath, "client_config.xml");
                using (var s = File.OpenRead(path))
                using (var f = new StreamReader(s))
                    return JsonConvert.DeserializeObject<Configuration>(f.ReadToEnd());
            }

            public static void Save(Configuration config)
            {
                var path = Path.Combine(ConfigPath, "client_config.xml");
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
            public string ClientDirectory { get; set; }

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
                public int SplashPtr { get; set; }
            }
        }

        private void GetOps(Configuration config)
        {
            new ConfigOps(config).ShowDialog();
            Config.Save(config);
            UpdateConfig(config);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!File.Exists(config.ClientDirectory))
                GetOps(config);

            var server = config.Servers.Find(i => i.ServerName == comboBox1.Text);
            if (server == null)
            {
                MessageBox.Show("Error, Check config.");
                return;
            }

            STARTUPINFO si = new STARTUPINFO();
            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
            bool success = NativeMethods.CreateProcess(config.ClientDirectory, null,
                IntPtr.Zero, IntPtr.Zero, false,
                ProcessCreationFlags.CREATE_SUSPENDED,
                IntPtr.Zero, null, ref si, out pi);

            MemorySharp memory;
            try
            {
                memory = new MemorySharp((int)pi.dwProcessId);
            }
            catch
            {
                MessageBox.Show("This application needs to run as admin.");
                return;
            }
            var payload = new byte[7];
            var segments = server.IPAddress.Split('.');


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

                //kill
                memory.Write((IntPtr)(0x400000 + server.SplashPtr), 0x87, false);
            }

            IntPtr ThreadHandle = pi.hThread;
            NativeMethods.ResumeThread(ThreadHandle);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            GetOps(config);
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
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
        }
    }
}
