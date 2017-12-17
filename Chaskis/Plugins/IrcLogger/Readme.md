IrcLogger
=========

IrcLogger is a plugin that will log each and every message the bot sees over IRC to a specified directory.  The user is able to set how many messages they wish to save to the file before a new file is created.

Instructions
-------

There are no commands the people in the channel need to do.  All messages are logged regardless of user.

Configuration
--------
The plugin settings are located in the default Chaskis plugin folder (AppData/Chaskis/Plugins/IrcLogger).  There are three settings you can set.  The default is below.

```XML
<?xml version="1.0" encoding="utf-8" ?>
<!--
    This is the configuration file for the irc logger service
    for Chaskis.

    For all the settings below, a missing or empty tag results
    in the default settings.

    logfilelocation - Where to store the log files.  Defaulted
                      to a Logs folder inside of
                      the plugin directory (AppData/Chaskis/Plugins/IrcLogger/Logs).

    maxnumbermessagesperlog - The number of messages from the channel to save in a single log file.
                              After this number of messages, the old log file will be closed, and a new
                              one will be opened.  Defaulted to 1000 messages.  Set to 0 for no limit.

    logname - What to name the logs.  Defaulted to "irclog".  The log name will look like:
              logName.yyyy-MM-dd_HH-mm-ss-ff.log
              Note: The time stamp is the date of file create in UTC.
              
    ignores - If an incomming IRC message matches any of these regexes, it is not logged.
-->
<ircloggerconfig>
    <logfilelocation></logfilelocation>
    <maxnumbermessagesperlog></maxnumbermessagesperlog>
    <logname></logname>
    <ignores>
        <!-- Ignore pongs -->
        <ignore><![CDATA[^:\S+\s+PONG\s+\S+\s+:.+$]]></ignore>
    </ignores>
</ircloggerconfig>
```

Installation
--------
IrcLogger is included as a default Chaskis plugin.  To enable, open PluginConfig.xml and add the following line:

### Windows: ###

```XML
<assembly path="C:\Program Files\Chaskis\Plugins\IrcLogger\IrcLogger.dll" />
```

### Linux: ###

```XML
<assembly path="/usr/lib/Chaskis/Plugins/IrcLogger/IrcLogger.dll" />
```

Sample Output
--------

Your log files are named logname.timestamp.log.  The first log that is generated when the bot starts looks like:

```
############################################################
New instance of Chaskis launched.
############################################################
2016-07-09T21:18:28.2406880Z  :hitchcock.freenode.net NOTICE * :*** Looking up your hostname...
2016-07-09T21:18:28.3664462Z  :hitchcock.freenode.net NOTICE * :*** Checking Ident
2016-07-09T21:18:28.5432996Z  :hitchcock.freenode.net NOTICE * :*** Couldn't look up your hostname
2016-07-09T21:18:34.6600238Z  :hitchcock.freenode.net NOTICE * :*** No Ident response
2016-07-09T21:18:34.6615245Z  :hitchcock.freenode.net 001 SethTestBot :Welcome to the freenode Internet Relay Chat Network SethTestBot
2016-07-09T21:18:34.6620276Z  :hitchcock.freenode.net 002 SethTestBot :Your host is hitchcock.freenode.net[130.185.232.126/6667], running version ircd-seven-1.1.3
2016-07-09T21:18:34.6620276Z  :hitchcock.freenode.net 003 SethTestBot :This server was created Tue Jul 21 2015 at 18:44:23 UTC
...
############################################################
Maximum Size reached.  Continuing in irclog.2016-07-09_21-20-43-3181.log
############################################################
```

In the newly created log:
```
############################################################
Continuation of logs from irclog.2016-07-09_21-18-28-2406.log
############################################################
2016-07-09T21:20:44.1995065Z  :xforever1313!~Seth@10.15.3.1 PRIVMSG #TestSeth :test
2016-07-09T21:21:00.7172717Z  :xforever1313!~Seth@10.15.3.1 PRIVMSG #TestSeth :There it is!
2016-07-09T21:21:04.8322256Z  :xforever1313!~Seth@10.15.3.1 PRIVMSG #TestSeth :yay!
2016-07-09T21:21:09.2695812Z  :xforever1313!~Seth@10.15.3.1 PRIVMSG #TestSeth :.
```
