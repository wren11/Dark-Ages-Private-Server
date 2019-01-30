![#000000](https://raw.githubusercontent.com/wren11/DarkAges-Lorule-Server/master/GitStuff/pictures/darkages.gif)

# Lorule-Dark-Ages-Server
## Official Discord- https://discord.gg/PwbFH3T
## Official Website: http://darkages.creatorlink.net/

out-of-the-box Sand box Private Dark Ages Server For the Dark Ages Client 7.18
The goal of this project was to provide a platform for anyone to use, build and play on there own Dark Ages Worlds.
I'm happy to announce that this server is near the completion stages and content for this platform is being developed.
All content, Server and Client source will always remain open source and free to use.

![Alt text](https://raw.githubusercontent.com/wren11/DarkAges-Lorule-Server/master/GitStuff/pictures/lorule2.png "In Game")


## About This Project
A Darkages Server Project - For client **7.18** Only. 
The server features custom assets and Graphic Resources for the Official Lorule Client.

## Project Design Overview.
- Persistant JSON Storage and templates
- Fully sandbox, everything is configurable from a single .config file.
- Efficient and Fast Object Manager Service
- Scriptable Content using .NET Scripting
- Component driven design pattern.

## Server Development Status:
- Server is currently about 100% percent completed and only content scripted is remaining.
- Expect Glithes! Help by testing and reporting them!


## What is missing From this Server?
- [ ] ![#f03c15](https://placehold.it/15/f03c15/000000?text=+) Skill/Spell Formulas (Thye work but damage may be incorrect/unbalanced)
- [ ] ![#f03c15](https://placehold.it/15/f03c15/000000?text=+) Meta Database (Will be Last to Do.)

## Building & Compiling Source Code
- This was developed using Visual Studio 2017 Community (https://www.visualstudio.com/thank-you-downloading-visual-studio/?sku=Community&rel=15#)
    - Using Microsoft's Framework **4.6.1**
    - By default, It will compile to ..\Staging\bin\Debug\

So the Staging Folder is the place to seek when making changes to templates, other settings ect.
as everything will by default point to that directory.


## Using this Server / Server Hosting

- Download and Compile the Lorule Source code, It should compile out-of-the-box
- Download and Install Dark Ages Client 7.18
        - https://drive.google.com/file/d/1EbIf7AzQLJaUrR9Kd3wmZDQWM7qT0-hR/view?usp=sharing    
- Download our modified Darkages.exe executable and replace the previous one in the 7.18 Installation Directory.
        - https://drive.google.com/file/d/18fCo2kyL1pF6QPJ9TgO1PIGmfKn5Fm_D/view?usp=sharing
    
That is all you need to run the server on your local system.
To host your server online, you must use the Client Launcher for your users to connect to your IP Address.
Your IPAddress also needs to reflect the **server.tbl** file located in your Server's running directory.

Lorule Client Launcher Download:    
        - https://drive.google.com/file/d/16Uff3R23Qdg3qRiaMbCG2q1szgfCj4LG/view?usp=sharing 
        - (optional) - Copy the Darkages_Client folder and the config to your documents directory.
    
            -- When adding a server to the launcher config file, the settings you will be looking for client 7.18 are:
                    -- PatchTable  : 212257
                    -- HookTable   : 213435
            -- The rest of the information can be set yourself, IP, port, Ect.

One more thing, run launcher as administrator if it fails, darkages will also need to run as **administrator**.
        
        
        
        


