#!bin/bash

pwd 

git pull https://github.com/TF2Maps/SteamBotLite.git
echo ls
echo pwd 

xbuild ../../SteamBotLite.sln

cd bin/Debug

echo "Loading SteamBot"

./SteamBotLite.exe

