---
description: also Formally known as project "Lorule"
---

# Hades - A Dark Ages Server & Client Project

## Getting Started End User Guide

Depending on what you want to do, develop for Hades or simply run and host a game using Hades as an end user, Before you can do either, You will need to install some stuff first.  
  
so before you start, see the table below and see what actions require what, and be sure you have everything you need before going any further. if you are not sure what to install from the table below then i suggest you install and download everything that way you won't run into any problems later.

### Client & Server Prerequisites

| Action | Prerequisite | References / URL |
| :--- | :--- | :--- |
| Build and Compile Hades | Visual Studio 2019 | [Download](https://www.visualstudio.com/downloads/) |
| Build and Compile Hades | .NET 5.0.1 SDK | [Download](https://dotnet.microsoft.com/download/dotnet/5.0) |
| Running the Server | .NET 5.0.1 Runtime | [Download](https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-desktop-5.0.1-windows-x64-install) |
| Hades Client | .NET Framework 4.7.2 Developer pack | [Download](https://dotnet.microsoft.com/download/dotnet-framework/thank-you/net472-developer-pack-offline-installer) |
| Hades World Editor | .NET Framework 4.7.2 Developer pack | [Download](https://dotnet.microsoft.com/download/dotnet-framework/thank-you/net472-developer-pack-offline-installer) |

### Content Creation Prerequisites

| Action | Prerequisite | Reference / URL |
| :--- | :--- | :--- |
| Writing Server Scripts | VS Code | [Download](https://code.visualstudio.com/) |
| Writing Game Content | Text Editor | [Download](https://notepad-plus-plus.org/downloads/v7.8.) |

### Client Files

You will also need to download a client. [Download the game client](https://drive.google.com/file/d/1VtnTcGivQ1P89bocQTO1LkwCDB4hvx8A/view?usp=sharing)

## Setting up the Game Server 

* Download and Install Hades  [Download](https://github.com/wren11/Dark-Ages-Private-Server/releases/tag/e121eac) \(Release\) 
* Download both [database.zip](https://github.com/wren11/Dark-Ages-Private-Server/releases/download/e121eac/database.zip) And [Lorule.GameServer.zip](https://github.com/wren11/Dark-Ages-Private-Server/releases/download/e121eac/Lorule.GameServer.zip) from the project's **GitHub** Page [here.](https://github.com/wren11/Dark-Ages-Private-Server/releases/tag/e121eac) 
* Install all Prerequisites in the table above, for "**Running the Server**"  
* Extract all contents of database.zip and Lorule.GameServer.zip somewhere on your computer that will be the host computer \(most likely this will be the computer you are using now\) 
* navigate to where you extracted the Lorule.GameServer.zip archive and browse the folder and find the configuration file. it is called `Lorule.Config.json` 
* Open the configuration file in some type of text editor, i recommend Notepad++ [Download](https://notepad-plus-plus.org/downloads/v7.8.8/) if you have not got a handy text editor yet. Notepad shipped with windows will do, but we all know it's garbage! 
* Modify the `Lorule.Config.json` file with the path's inside the **`locales`** section.  
  below is a part of the configuration file that you must edit correctly. and when editing this file.  
  **you must use double slashes** for path **`\\`** separators.   
  do not use any single **`\`** backslash in this configuration file for any paths. 

  ```text
  // Locales
  "Content": {
      "Location": "C:\\Users\\Dean\\Documents\\GitHub\\DarkAges-Lorule-Server\\database\\server",
      "ServerIP": "127.0.0.1"
  },
  "Editor": {
      "Location": "C:\\Users\\Dean\\Documents\\GitHub\\DarkAges-Lorule-Server\\database",
      "GameLocation": "C:\\Users\\Dean\\Documents\\GitHub\\DarkAges-Lorule-Server\\game"
  }
  ```

* Modify the configuration file inside the Locales section, you will see two properties and i will explain each in the next section below.

### Server Configuration

```text
"Content": {
    "Location": "C:\\Users\\Dean\\Documents\\GitHub\\DarkAges-Lorule-Server\\database\\server",
    "ServerIP": "127.0.0.1"
},
"Editor": {
    "Location": "C:\\Users\\Dean\\Documents\\GitHub\\DarkAges-Lorule-Server\\database",
    "GameLocation": "C:\\Users\\Dean\\Documents\\GitHub\\DarkAges-Lorule-Server\\game"
}
```

{% tabs %}
{% tab title="Content" %}
L**ocation**  
This should be the location pointing to the **`database\\server`**  
  
_It needs to point where all the server content lives, and the location should point to a directory that looks like this below._

![](.gitbook/assets/image%20%287%29.png)

**Server IP**  
This should be your public facing IP Address, or you can leave it as `127.0.0.1` if you don't plan to host the server online yet.
{% endtab %}

{% tab title="Editor" %}
```text
Location
```

  
This should be the location of where **database** folder lives.  
note, it's not the same as the location inside of the Content section.  
  
for example, it you had both database folder and the Lorule.GameServer folder inside a folder called "Hades" on your Desktop, then you would set the Location for the Editor's Section to  
**This PC\\Desktop\\Hades**

\*\*\*\*

```text
GameLocation
```

This should be the path, again with double back slashes **`\\`** to the **`DarkAges`**   
Game Client directory.   
  
Supported clients are the Official Dark Ages Client Version 7.18  
or the Hades Client \(still In Development\)  
  
if you don't have a client yet, you can [download it here](https://drive.google.com/file/d/1EbIf7AzQLJaUrR9Kd3wmZDQWM7qT0-hR/view?usp=sharing).   
{% endtab %}
{% endtabs %}

### Running the Game Server

* Navigate to the folder ![](.gitbook/assets/image%20%2812%29.png)
* Double click on the file ![](.gitbook/assets/image%20%284%29.png)
* The game server should now be started. if you are prompted by windows firewall, click allow.
* You should now see a running console and it should look like this \(as shown below\)

![](.gitbook/assets/image%20%2811%29.png)

## Setting up the Game Client

So you should be up and running now with the game server and you now you probably want to connect to your running server, to achieve this, you should need to use a client. so let's start with what you will need.

### Game Client

You will need a supported Client to connect to the Game Server.

#### Download Dark Ages Client 7.18

Below you can download the client is 6 parts, Alternatively if you rather, you can download the entire thing  
that is [available here](https://drive.google.com/file/d/1VtnTcGivQ1P89bocQTO1LkwCDB4hvx8A/view?usp=sharing) \(google drive\)

### Installing the Game Client

once the client is installed, and extracted to your computer. You should be able to see the following contents inside \(as shown below\)

![](.gitbook/assets/image%20%2810%29.png)

### **Client Not Connecting?**

The **Hades.exe** client has been **hard-coded** and it will allow you to   
connect to `localhost` also known as `127.0.0.1`  
  
if when you setup the IP Address in the **LoruleConfig.json** file, if you have modified it to not use **`127.0.0.1`** and instead used your **`public IP Address`**, then it should work with both, but if it does not connect, then i assume you have **NOT** or incorrect setup **NAT forwarding.**  
  
if you have not setup NAT forwarding, then it's best to set the IPAddress in the config file to **127.0.0.1**  
until you have done so**.**

## Router Configuration

* First, open the windows cmd terminal and type /ipconfig
* look for the active eth adapter in use. an example of what mine looks like below is show below

![](.gitbook/assets/image%20%283%29.png)

* Find the correct IPv4 Address, this will be important, as you can see above. Mine is 192.168.1.110 so whatever yours is, is going to be your INTERNAL IP when setting up NAT forwarding. 
* open a browser and connect to your default gateway Address, as above mine is 192.168.1.1 \(yours will likely be different\)
* Look for a section called **NAT Forwarding**, or **Port Fowarding** or **Virtual Servers**, the terminology will be different from device to device, so you will have to work this out yourself from here, but somewhere in there will be a place with a similar looking table like the one shown below

![](.gitbook/assets/image%20%286%29.png)

* Add in your IPv4 Address you found using **`/ipconfig`**
* Set External and Internal Ports to **2610** - **2615**
* Set the Protocol to **TCP** or both **TCP** or **UPD** if that's the only option in there.
* type into google "what is my ip" and update the LoruleConfig.json and set the IPAddress to your IP Address that google tells you. \(Your Public IP Address\)
* you should not have the router setup to allow you to connect to yourself, from your Public IP, others to connect to you, and vise versa.
* if you have trouble setting this up, Feel free to seek help on the[ project discord ](https://discord.gg/QayQFJY)











  




