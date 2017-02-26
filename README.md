# SteamBotLite
Needs a name-change, it does bot stuff for TF2Maps. Easily configurable. 

#USE AT YOUR OWN RISK

#General code structure

A general idea of the code is: 

There are "interfaces" to external services (Steam and Discord Currently)
There are "Chathandlers" which will respond to messages through events. (VBot being the major one)

Interfaces can be linked to chathandlers. Interfaces will translate the data into an abstract form for the chathandlers to do work on, similarly chathandlers will fire events in response TO the interfaces, in a similarly abstract form.
A cool thing is a single chathandler can be linked to many interfaces. 

#Configuring:

In bin/example there are several files. 

config.json is used by the VBot Userhandler, i've provided an example configuration for it. If there's a module loaded in VBot that isn't in the config it'll throw an error upon initialisation.

The files in applicationconfigs are used for "Interfaces" to login, as well as restrict their chat. They are named (and case-sensitive) to the classes that load them. For example "SteamAccountVBot" class will load SteamAccountVBot.json
