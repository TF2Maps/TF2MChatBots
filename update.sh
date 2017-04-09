#!bin/bash

git pull https://github.com/TF2Maps/SteamBotLite.git

xbuild ../../SteamBotLite.sln

cd bin/Debug
sudo pkill mono
./SteamBotLite.exe
