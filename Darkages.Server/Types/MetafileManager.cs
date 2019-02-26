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
using Darkages.Compression;
using System;
using System.IO;

namespace Darkages.Types
{
    public class MetafileManager
    {
        private static readonly MetafileCollection metafiles;

        //TODO : finish implementation.
        //Note to self: remove *.defalted, uncomment the save and use this to handle the meta information.
        static MetafileManager()
        {
            var path = new FileInfo(Path.Combine(ServerContext.StoragePath, "metafile")).FullName;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);


            var files = Directory.GetFiles(path, "*.deflated", SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                metafiles = new MetafileCollection(files.Length);
                {
                    foreach (var file in files)
                    {
                        var mf = CompressableObject.Load<Metafile>(file, true);
                        mf.Name = mf.Name.Replace(".deflated", string.Empty).Trim();

                        if (mf.Name.StartsWith("NationDesc"))
                        {
                            mf.Nodes = new System.Collections.ObjectModel.Collection<MetafileNode>
                            {
                                new MetafileNode("nation_1", "Lorule"),
                                new MetafileNode("nation_2", "Lividia"),
                                new MetafileNode("nation_3", "Exiles")
                            };
                        }

                        if (mf.Name.StartsWith("SClass"))
                        {
                            mf.Nodes = new System.Collections.ObjectModel.Collection<MetafileNode>
                            {
                                new MetafileNode("Skill")
                            };

                            foreach (var skill in ServerContext.GlobalSkillTemplateCache)
                            {
                                if (skill.Value.Prerequisites != null)
                                {
                                    var nmf = new MetafileNode(skill.Key, skill.Value.Prerequisites.MetaData);
                                    mf.Nodes.Add(nmf);
                                }
                            }
                            mf.Nodes.Add(new MetafileNode("Skill_End"));
                            mf.Nodes.Add(new MetafileNode(""));
                        }


                        //CompressableObject.Save(file, mf);

                        metafiles.Add(mf);

                    }
                }
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
