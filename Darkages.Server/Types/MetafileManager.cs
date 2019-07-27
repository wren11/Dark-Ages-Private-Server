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

using System;
using System.Collections.ObjectModel;
using System.IO;
using Darkages.Compression;

namespace Darkages.Types
{
    public class MetafileManager
    {
        private static readonly MetafileCollection metafiles;

        static MetafileManager()
        {
            var files = Directory.GetFiles(Path.Combine(ServerContext.StoragePath, "metafile"));
            metafiles = new MetafileCollection(files.Length);

            foreach (var file in files)
            {
                metafiles.Add(
                    CompressableObject.Load<Metafile>(file, true));
            }
        }

        public static Metafile GetMetafile(string name)
        {
            return metafiles.Find(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public static MetafileCollection GetMetafiles()
        {
            return metafiles;
        }
    }
}