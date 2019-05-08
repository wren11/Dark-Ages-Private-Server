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
using Darkages.Common;
using Darkages.Storage;
using Darkages.Types;
using System.Collections.Generic;
using System.Linq;

namespace ServerFactory
{
    partial class Program
    {
        #region Validation
        private static void ValidateTemplates()
        {
            //This Provides a Id to All templates, if they don't have one already.
            ICollection<Template> PendingTemplateUpdates = new List<Template>();

            foreach (var template in Instance.GlobalWorldMapTemplateCache.Select(i => i.Value))
            {
                lock (Generator.Random)
                {
                    template.Id = Generator.GenerateNumber();
                    PendingTemplateUpdates.Add(template);
                }
            }

            foreach (var template in Instance.GlobalItemTemplateCache.Select(i => i.Value))
            {
                lock (Generator.Random)
                {
                    template.Id = Generator.GenerateNumber();
                    PendingTemplateUpdates.Add(template);
                }
            }

            foreach (var template in Instance.GlobalMundaneTemplateCache.Select(i => i.Value))
            {
                lock (Generator.Random)
                {
                    template.Id = Generator.GenerateNumber();
                    PendingTemplateUpdates.Add(template);
                }
            }

            foreach (var template in Instance.GlobalSkillTemplateCache.Select(i => i.Value))
            {
                lock (Generator.Random)
                {
                    template.Id = Generator.GenerateNumber();
                    PendingTemplateUpdates.Add(template);
                }
            }

            foreach (var template in Instance.GlobalSpellTemplateCache.Select(i => i.Value))
            {
                lock (Generator.Random)
                {
                    template.Id = Generator.GenerateNumber();
                    PendingTemplateUpdates.Add(template);
                }
            }

            foreach (var template in Instance.GlobalWorldMapTemplateCache.Select(i => i.Value))
            {
                lock (Generator.Random)
                {
                    template.Id = Generator.GenerateNumber();
                    PendingTemplateUpdates.Add(template);
                }
            }

            foreach (var template in Instance.GlobalReactorCache.Select(i => i.Value))
            {
                lock (Generator.Random)
                {
                    template.Id = Generator.GenerateNumber();
                    PendingTemplateUpdates.Add(template);
                }
            }

            foreach (var template in Instance.GlobalMonsterTemplateCache)
            {
                lock (Generator.Random)
                {
                    template.Id = Generator.GenerateNumber();
                    PendingTemplateUpdates.Add(template);
                }
            }

            //Save all Templates.
            foreach (var template in PendingTemplateUpdates)
            {
                if (template.Id == 0)
                {
                    { if (template is ItemTemplate _tmpl) { StorageManager.ItemBucket.SaveOrReplace(_tmpl); } }
                    { if (template is MonsterTemplate _tmpl) { StorageManager.MonsterBucket.SaveOrReplace(_tmpl); } }
                    { if (template is MundaneTemplate _tmpl) { StorageManager.MundaneBucket.SaveOrReplace(_tmpl); } }
                    { if (template is SkillTemplate _tmpl) { StorageManager.SkillBucket.SaveOrReplace(_tmpl); } }
                    { if (template is SpellTemplate _tmpl) { StorageManager.SpellBucket.SaveOrReplace(_tmpl); } }
                    { if (template is WorldMapTemplate _tmpl) { StorageManager.WorldMapBucket.SaveOrReplace(_tmpl); } }
                    { if (template is Reactor _tmpl) { StorageManager.ReactorBucket.SaveOrReplace(_tmpl); } }
                    { if (template is WarpTemplate _tmpl) { StorageManager.WarpBucket.Save(_tmpl); } }
                }
            }
        }

        private static void StartMainServers(Instance instance)
        {
            instance.Start();
        }
        #endregion
    }
}
