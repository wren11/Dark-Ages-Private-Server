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
namespace Darkages.Types
{
    /// <summary>
    /// CREDITS: Enum Borrowed from... i forgot where... the creator i guess.)
    /// </summary>
    public enum MapFlags : uint
    {
        ArenaTeam = 8192,
        CanLocate = 512,
        CanSummon = 256,
        CanTeleport = 1024,
        CanUseSkill = 2048,
        CanUseSpell = 4096,
        Darkness = Snow | Rain,
        Default = CanSummon | CanLocate | CanTeleport | CanUseSkill | CanUseSpell | SendToHell | ShouldComa,
        HasDayNight = 131072,
        NoMap = 64,
        PlayerKill = 16384,
        Rain = 2,
        SendToHell = 32768,
        ShouldComa = 65536,
        Snow = 1,
        Winter = 128
    }
}
