using Darkages.Types;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Content_Maker
{
    public partial class NewItemWizard : Form
    {
        public NewItemWizard()
        {
            InitializeComponent();
        }


        public enum SearchType
        {
            Armors  = 0,
            Weapons = 1,
            Helmets = 2,
        }
    

        public abstract class VorlofSearchInfo<T>
        {
            [JsonProperty]
            public abstract string Name { get; set; }

            [JsonProperty]
            public abstract SearchType Type { get; }

            [JsonProperty]
            public abstract Gender Gender { get; set; }

            [JsonProperty]
            public abstract Class Path { get; set; }

            public abstract void Search(string lpUrl);

            [JsonProperty]
            internal List<T> Results { get; set; }

            [JsonProperty]
            public abstract string ImageUrl { get; set; }
        }

        public class SearchArmors : VorlofSearchInfo<SearchArmors>
        {
            public ushort DisplayID { get; set; }

            public string Image { get; set; }

            public override string ImageUrl { get; set; }

            [JsonIgnore]
            public ushort SpriteID => (ushort)(0x8000 + ushort.Parse(Image));

            public override SearchType Type => SearchType.Armors;

            public override Gender Gender { get; set; }

            public override Class Path { get; set; }

            public override string Name { get; set; }

            [JsonProperty]
            public string FileLocation { get; set; }

            public override void Search(string lpUrl)
            {
                Results = new List<SearchArmors>();
    
                var web                = new HtmlWeb();
                var htmlDoc            = web.Load(lpUrl);
                var displayids         = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"dataTables-example\"]/tbody/tr[*]/td[1]");
                var displaytext_female = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"dataTables-example\"]/tbody/tr[*]/td[2]/text()");
                var displaytext_male   = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"dataTables-example\"]/tbody/tr[*]/td[3]/text()"); ;


                foreach (var node in displaytext_female.Zip(displayids, (n, d) => new SearchArmors {
                    Name      = n.InnerText,
                    DisplayID = ushort.Parse(d.InnerText),
                    Image     = GetImageNumber(n.InnerText),
                    Gender    = n.InnerHtml != string.Empty ? Gender.Female : Gender.Both,
                    Path      = GetClass(n.InnerHtml),                   
                    ImageUrl  = string.Format("http://www.vorlof.com/images/items2/{0}.png", GetImageNumber(n.InnerText)),
                }))
                {
                    if (node.DisplayID > 0)
                    {
                        DownloadImage(node);
                        Results.Add(node);
                    }
                }

                foreach (var node in displaytext_male.Zip(displayids, (n, d) => new SearchArmors
                {
                    Name      = n.InnerText,
                    DisplayID = ushort.Parse(d.InnerText),
                    Image     = GetImageNumber(n.InnerText),
                    Gender    = n.InnerHtml != string.Empty ? Gender.Male : Gender.Both,
                    Path      = GetClass(n.InnerHtml),
                    ImageUrl  = string.Format("http://www.vorlof.com/images/items2/{0}.png", GetImageNumber(n.InnerText)),
                }))
                {
                    if (node.DisplayID > 0)
                    {
                        DownloadImage(node);
                        Results.Add(node);
                    }
                }

            }

            public void DownloadImage(SearchArmors node)
            {
                var location        = $"content_assets\\display_images\\{node.Gender.ToString()}\\armors\\";
                var file_location   = System.IO.Path.Combine(location, node.DisplayID.ToString() + "_" + node.SpriteID.ToString() + ".png");

                if (!Directory.Exists(location))
                {
                    Directory.CreateDirectory(location);
                }

                using (var client = new WebClient())
                {
                    if (!File.Exists(file_location))
                    {
                        client.DownloadFileAsync(new Uri(node.ImageUrl), file_location);
                    }
                }

                node.FileLocation = file_location;
            }
        }

        public static string GetImageNumber(string lptext)
        {
            var m = @"^.*?\([^\d]*(\d+)[^\d]*\).*$";
            var res = Regex.Match(lptext, m);

            if (res.Success && res.Groups.Count >= 1)
            {
                return res.Groups[1].Value;
            }

            return "0";
        }

        public static Class GetClass(string lptext)
        {
            if (lptext.ToLower().Contains("priest"))
            {
                return Class.Priest;
            }
            else if (lptext.ToLower().Contains("warrior"))
            {
                return Class.Warrior;
            }
            else if (lptext.ToLower().Contains("monk"))
            {
                return Class.Monk;
            }
            else if (lptext.ToLower().Contains("rogue"))
            {
                return Class.Rogue;
            }
            else if (lptext.ToLower().Contains("wizard"))
            {
                return Class.Wizard;
            }
            else
            {
                return Class.Peasant;
            }
        }


        static string armorPATH = Environment.CurrentDirectory + "\\searchable_armors.json";
        static JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Auto
        };

        public List<SearchArmors> Armors = new List<SearchArmors>();

        private async void NewItemWizard_Load(object sender, EventArgs e)
        {

            new FrmBrowser().Show();

            WindowState = FormWindowState.Normal;
            propertyGrid2.SelectedObject = new ArmorTemplate();
            await LoadArmors();
        }

        private async Task LoadArmors()
        {
            await Task.Run(() =>
            {
                if (!File.Exists(armorPATH))
                {

                    SearchArmors _armors = new SearchArmors();
                    _armors.Search("http://www.vorlof.com/general/searcharmors.html");

                    var json = JsonConvert.SerializeObject(_armors, settings);
                    File.WriteAllText(armorPATH, json);
                }
                else
                {
                    var jsonData = File.ReadAllText(armorPATH);
                    var json     = JsonConvert.DeserializeObject<SearchArmors>(jsonData, settings);

                    if (json != null)
                    {
                        if (Armors == null || Armors.Count == 0)
                        {
                            Armors = new List<SearchArmors>(json.Results);
                        }

                        GetArmors();
                    }
                }
            });
        }

        bool Male = true;

        private void GetArmors()
        {
            Invoke((MethodInvoker)delegate ()
            {
                BuildUIListView();
            });
        }

        private void BuildUIListView()
        {
            ImageList imagelist = new ImageList();

            listView1.LargeImageList = imagelist;
            listView1.SmallImageList = imagelist;

            int idx = 0;

            listView1.Items.Clear();


            var subject = Armors.Where(i => i.Path != Class.Peasant && i.Gender == (Male ? Gender.Male : Gender.Female));

            if (checkBox1.Checked)
            {
                subject = Armors.Where(i => i.Gender == (Male ? Gender.Male : Gender.Female));
            }

            foreach (var node in subject)
            {
                var image = Image.FromFile(node.FileLocation);
                imagelist.Images.Add(node.SpriteID.ToString(), image);

                listView1.Items.Add(new ListViewItem(new string[] { node.DisplayID.ToString(), string.Format("0x{0:X2}", node.SpriteID), node.Name }, idx, new ListViewGroup("Male", "Male Armors")));

                idx++;
            }
        }



        private void propertyGrid1_Click(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count == 0)
                return;

            var selected = listView1.SelectedIndices[0];
            var selected_obj = Armors[selected];

            if (selected_obj != null)
            {
                propertyGrid1.SelectedObject = null;
                propertyGrid1.SelectedObject = selected_obj;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text != "Show Male")
            {
                button1.Text = "Show Male";
                Male = false;
            }
            else
            {
                button1.Text = "Show Female";
                Male = true;
            }

            GetArmors();

        }

        private void compileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var obj = (ArmorTemplate)propertyGrid2.SelectedObject;

            if (obj != null)
            {
                var template = obj.Compile();
            }
        }
    }
}
