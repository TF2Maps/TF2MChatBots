# VBOT (A.k.a SteamBotLite)
VBot is a program that is utilised by the TF2Maps community, accessible via the steam group chat and discord channel. 
Its primary purpose is to improve the capability of the online chatroom as well as improve the development process for members. 

VBot allows users to actively add their Maps into the chat-bot’s list, and all maps in the list can be recited anytime through a command. Furthermore VBot will actively connect to TF2Map’s Game Servers and remove maps from the list when they are ran, and notify the user through a private message (as well as post in the chatroom on map-change to encourage users to join). 

VBot’s module-based design allows for the easy integration and isolated development of numerous functions w/o causing disturbance to the primary features, the Bot has a MOTD module that broadcasts a message hourly to the chatroom, a Saved Replies Module (e.g: /maps replies http://vbot.site/) and an online search module for the Team Fortress 2 development reference site as well as TF2maps.net itself.

The design of the bot allows for a single “UserHandler” to handle and process requests on multiple platforms simultaneously (and has been integrated to also work with Discord) as instead ‘InterfacePlatforms’ normalize the data across platforms and serve as a bridge. VBot also hosts a website at: http://vbot.site/ that allows users to view the maps currently in the queue. 

# Maintananers
This program is primarily written, maintained, deployed and updated by Ben "Fantasmos" Krajancic, all questions should be sent to him.

Maxine Dupis https://github.com/maxdup was responsible for developing the initial modules and command system, however due to issues with github the initial commit was made under fantasmos' account at: https://github.com/TF2Maps/SteamBotLite/commit/fe204c423372e8038c1a77ded378a22630d8522a 

# General code structure

A general idea of the code is: 

There are "interfaces" to external services (Steam and Discord Currently)
There are "Chathandlers" which will respond to messages through events. (VBot being the major one)

Interfaces can be linked to chathandlers. Interfaces will translate the data into an abstract form for the chathandlers to do work on, similarly chathandlers will fire events in response TO the interfaces, in a similarly abstract form.
A cool thing is a single chathandler can be linked to many interfaces. 

# Configuring:

In bin/example there are several files. 

config.json is used by the VBot Userhandler, i've provided an example configuration for it. If there's a module loaded in VBot that isn't in the config it'll throw an error upon initialisation.

The files in applicationconfigs are used for "Interfaces" to login, as well as restrict their chat. They are named (and case-sensitive) to the classes that load them. For example "SteamAccountVBot" class will load SteamAccountVBot.json
