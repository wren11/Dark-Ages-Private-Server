using Capricorn.Drawing;
using Hades.Imaging;
using Hades.Imaging.SPF;
using Lorule.Content.Editor.Dat;
using Lorule.Editor;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using ServiceStack.Text;
using static System.Int32;

namespace Hades.Utils.ImageTools
{
    class Program
    {
        public static List<AssetControl> ControiAssetControls = new List<AssetControl>();

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("LoruleConfig.json");

            var config = builder.Build();
            var editorSettings = config.GetSection("Editor").Get<EditorOptions>();
            var assetLocation = Path.Combine(editorSettings.Location, "Assets", "Bitmaps", "legend");

            if (!Directory.Exists(assetLocation))
                Directory.CreateDirectory(assetLocation);


            Dictionary<string, Palette> pals = new Dictionary<string, Palette>();

            foreach (var palfile in Directory.GetFiles(editorSettings.Location + @"\Extractions\setoa", "*.pal"))
            {
                var pallete = Palette.FromFile(palfile);

                pals[Path.GetFileNameWithoutExtension(palfile)] = pallete;

            }

            Palette palette = null;

            foreach (var file in Directory.GetFiles(editorSettings.Location + @"\Extractions\setoa", "*.*").OrderBy(i => i.Length).ToArray())
            {

                if (Path.GetExtension(file) == ".epf")
                {
                    var epf = EPFImage.FromFile(file);

                    var palleteName = File.Exists(Path.GetFileNameWithoutExtension(file) + ".pal")
                        ? Path.GetFileNameWithoutExtension(file)
                        : "legend";

                    palette = pals[palleteName];

                    foreach (var frame in epf.Frames)
                    {

                        if (frame.RawData.Length > 0 && palette != null)
                        {
                            var bitmap = frame.Render(frame.Width, frame.Height, frame.RawData, palette,
                                EPFFrame.ImageType.EPF);
                            bitmap.Save(
                                Path.Combine(assetLocation,
                                    Path.GetFileNameWithoutExtension(file) + ".bmp"),
                                ImageFormat.Bmp);
                        }
                    }
                }

                if (Path.GetExtension(file) == ".spf")
                {
                    var spf = SpfFile.FromFile(file);

                    foreach (var frame in spf.Frames)
                    {
                        frame.FrameBitmap?.Save(
                            Path.Combine(assetLocation,
                                Path.GetFileNameWithoutExtension(spf.FileName) + $"{spf.FrameCount}.bmp"),
                            ImageFormat.Bmp);
                    }
                }

                if (Path.GetExtension(file) == ".txt")
                {
                    var data = await File.ReadAllLinesAsync(file);

                    if (data != null && data.Length > 0)
                    {
                        AssetControl control = new AssetControl();
                        for (var index = 0; index < data.Length; index++)
                        {
                            var line = data[index];

                            if (line == "<CONTROL>")
                            {
                                control.Name = data[index + 1].Replace("<NAME>", string.Empty).Replace("\"", "").Trim();
                                control.Type = data[index + 2].Replace("<TYPE>", string.Empty).Replace("\"", "").Trim();

                                var rect = data[index + 3].Replace("<RECT>", string.Empty).Replace("\"", "").Trim();
                                var rectparts = rect.Split(" ");

                                if (rectparts.Length == 4)
                                {
                                    TryParse(rectparts[0], out var x);
                                    TryParse(rectparts[1], out var y);
                                    TryParse(rectparts[2], out var w);
                                    TryParse(rectparts[3], out var h);

                                    control.Rect = new Rectangle(x, y, w, h);
                                }

                                if (data[index + 4] == "\t<IMAGE>")
                                {
                                    var imageparts = data[index + 5].Replace("\t\t", string.Empty).Replace("\"", "")
                                        .Trim().Split(" ");

                                    if (imageparts.Length == 2)
                                    {
                                        control.Image = imageparts[0];
                                        control.FrameCount = imageparts[1];
                                    }
                                    else
                                    {
                                        control.Image = imageparts[0];
                                    }
                                }
                            }

                            if (line == "<ENDCONTROL>")
                            {
                                ControiAssetControls.Add(control);
                                control = new AssetControl();
                            }
                        }
                    }
                }
            }


            List<AssetControl> saveableControls = new List<AssetControl>();


            foreach (var control in ControiAssetControls)
            {
                if (!string.IsNullOrEmpty(control.Image) && Path.GetExtension(control.Image) == ".spf")
                {
                    var path = Path.GetRelativePath(Path.Combine(assetLocation, editorSettings.Location), assetLocation);

                    if (File.Exists(path))
                    {
                        control.Image = path;
                        saveableControls.Add(control);
                    }
                }
            }

            var jsonFile = JsonConvert.SerializeObject(saveableControls);

            File.WriteAllText(assetLocation + "\\metafile.json", jsonFile);
        }
    }
}
