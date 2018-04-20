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
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;
using System;
using System.Collections.Generic;

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("buff")]
    public class buffgiver : MundaneScript
    {
        NormalPopup dialogs = null;

        public buffgiver(GameServer server, Mundane mundane) : base(server, mundane)
        {
            var Steps = new List<Step>();
            Steps.Add(new Step() { Body = "Hello.", ScriptId = 0x0000, Serial = (uint)Mundane.Serial, StepId = 0x0000, Title = this.Mundane.Template.Name, HasBack = false, HasNext = true, Image = (ushort)Mundane.Template.Image });
            Steps.Add(new Step() { Body = "There becomes a point in everybodys life. Where one must say to himself, Shit.", ScriptId = 0x0001, Serial = (uint)Mundane.Serial, StepId = 0x0001, Title = this.Mundane.Template.Name, HasBack = false, HasNext = true, Image = (ushort)Mundane.Template.Image });
            Steps.Add(new Step() { Body = "Shit becomes the shadow to that of a dark and fearful happening.", ScriptId = 0x0002, Serial = (uint)Mundane.Serial, StepId = 0x0002, Title = this.Mundane.Template.Name, HasBack = false, HasNext = true, Image = (ushort)Mundane.Template.Image });
            Steps.Add(new Step() { Body = "We call this the fruits of the happening", ScriptId = 0x0003, Serial = (uint)Mundane.Serial, StepId = 0x0003, Title = this.Mundane.Template.Name, HasBack = false, HasNext = true, Image = (ushort)Mundane.Template.Image });
            Steps.Add(new Step() { Body = "Don't let this happen to you!", ScriptId = 0x0004, Serial = (uint)Mundane.Serial, StepId = 0x0004, Title = this.Mundane.Template.Name, HasBack = false, HasNext = true, Image = (ushort)Mundane.Template.Image });
            Steps.Add(new Step() { Body = "Let me aid your fagile frame. I will cast apon the magic of great magnitude. ", ScriptId = 0x0005, Serial = (uint)Mundane.Serial, StepId = 0x0005, Title = this.Mundane.Template.Name, HasBack = false, HasNext = false, Image = (ushort)0x8066 });


            dialogs = new NormalPopup()
            {
                Steps = Steps,
                CurrentStep = 0,
                TotalSteps = Steps.Count,
            };

        }

        public override void OnClick(GameServer server, GameClient client)
        {
            if (dialogs != null)
            {
                dialogs.CurrentStep = 0;

                var step = dialogs.Steps[0];
                if (step != null)
                    client.Send(new ServerFormat30(step));
            }
        }

        public override void OnResponse(GameServer server, GameClient client, short responseID, string args)
        {
            if (dialogs != null)
            {
                if (dialogs.CurrentStep + 1 < dialogs.TotalSteps)
                {
                    dialogs.CurrentStep = dialogs.CurrentStep + 1;
                    var step = dialogs.Steps[dialogs.CurrentStep];
                    if (step != null)
                        client.Send(new ServerFormat30(step));
                }
            }
        }
    }
}
