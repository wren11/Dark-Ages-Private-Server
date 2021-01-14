using Lorule.Client.Base.Dat;
using Lorule.Content.Editor.Dat;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Forms;

namespace Hades.ArchiveTools
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using var serviceProvider = new ServiceCollection()
                .AddSingleton<Main>()
                .AddSingleton<IArchive, Archive>()
                .AddSingleton<IPaletteCollection, PaletteCollection>()
                .BuildServiceProvider();

            var frm = serviceProvider.GetService<Main>();
            if (frm != null) Application.Run(frm);
        }
    }
}
