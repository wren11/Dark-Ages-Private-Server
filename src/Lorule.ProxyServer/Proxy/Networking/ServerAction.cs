// Decompiled with JetBrains decompiler
// Type: Bot2008.ServerAction
// Assembly: Bot2008, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DAC6BB13-1725-4402-8B48-36C7A32E897C
// Assembly location: C:\Users\Dean\Desktop\Bot\Bot\bot08\Bot2008.exe

namespace Proxy.Networking
{
  public enum ServerAction
  {
    Redirect = 0x3,
    Location = 0x4,
    Serial = 0x5,
    AddSprites = 0x7,
    StatsUpdated = 0x8,
    Bar = 0xA, // 0x0000000A
    ClientWalk = 0xB, // 0x0000000B
    EntityWalked = 0xC, // 0x0000000C
    Chat = 0xD, // 0x0000000D
    RemoveSprite = 0xE, // 0x0000000E
    ItemSlotInfo = 0xF, // 0x0000000F
    RemoveItem = 0x10, // 0x00000010
    EntityTurn = 0x11, // 0x00000011
    DisplayHpbar = 0x13, // 0x00000013
    MapInfo = 0x15, // 0x00000015
    SpellSlotInfo = 0x17, // 0x00000017
    RemoveSpell = 0x18, // 0x00000018
    SoundPlay = 0x19, // 0x00000019
    BodyAnimation = 0x1A, // 0x0000001A
    Animation = 0x29, // 0x00000029
    SkillSlotInfo = 0x2C, // 0x0000002C
    RemoveSkill = 0x2D, // 0x0000002D
    PopUpResponse = 0x30, // 0x00000030
    Wall = 0x32, // 0x00000032
    AddPlayer = 0x33, // 0x00000033
    LegendInfo = 0x34, // 0x00000034
    CountryList = 0x36, // 0x00000036
    Appendage = 0x37, // 0x00000037
    RemoveAppendage = 0x38, // 0x00000038
    SpellBar = 0x3A, // 0x0000003A
    Ping = 0x3B, // 0x0000003B
    Cooldown = 0x3F, // 0x0000003F
    GroupRequest = 0x63, // 0x00000063
    Tick = 0x68, // 0x00000068
  }
}
