# steam_archive_tools

 I wanted something decoupled from steam to backup and restore my games, so I wrote a small executable to handle this.

 Main features:  

 - Faster (multithreaded) backup & restore.   
 - Ability to backup & restore many games at the same time.   
 - Low priority, so you can actually do something else with your computer (even play another game).   
 - Ability to backup & restore from / to another computer with network shared library.   
 - Easy to use and to deploy everywhere.   

## Requirements

You NEED 7-zip for this to work. Download it here: https://www.7-zip.org/download.html

## Usage 

- Download the latest release of the tool: https://github.com/dbkblk/steam_archive/releases
- Extract it in the directory where you want your games to be backup in.
- Modify steamapps.txt to point where your Steam libraries are (point the steamapps folder). 

### To archive a game

- Execute "steam_archive.exe". 
- It will look for games in the paths you've specified in steamapps.txt. 
- Select the game you want, then select if you want to keep the game installed after the archive is done.
- The backup will be kept in the "backups/" directory.
- Wait for tha backups to process, then relaunch Steam.

### To restore a game

- Execute "steam_restore.exe".
- It will look for games in the "backups/" directory.
- Select the one you want to restore.
- Select the path where to install it (specified in steamapps.txt)
- Wait for extraction, then relaunch Steam.
- Enjoy :)
