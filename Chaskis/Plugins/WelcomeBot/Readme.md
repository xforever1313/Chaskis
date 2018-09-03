Welcome Bot
==============

About
======
This bot will say if a user joins or leaves a channel.

Configuration
=====

Configuration lives in your Chaskis Root in Plugins/WelcomeBot/WelcomeBotConfig.xml
```XML
<?xml version="1.0" encoding="utf-8" ?>

<!--
This file is the configuration for Welcome Bot, so users
can specify what WelcomeBot responds to.

All values should be set to either 'true' to enable the setting
or 'false' to disable the settings.

joinmessages - If set to 'true', the bot will send a message
               to the channel that a user has joined the channel.
               
partmessages - If set to 'true', the bot will send a message
               to the channel that a user has left (parted) the channel.
               
kickmessages - If set to 'true', the bot will send a message to the
               channel that a user was kicked from the channel.
               
karmabotintegration - If set to 'true', the bot will report a user's Karma
                      when they join the channel.  joinmessages must be set to 'true'.
                      The KarmaBot plugin must also be loaded, or nothing will happen.
                      

Other Notes:
    - quitmessages is not implemented yet.  The reason why is the QUIT message does not
      specify a channel like parting does, and it probably isn't desirable for the bot
      to message ALL channels when someone quits in one channel, but is not in most
      channels.
-->    

<welcomebotconfig xmlns="https://files.shendrick.net/projects/chaskis/schemas/welcomebotconfig/2018/welcomebotconfigschema.xsd">
    <joinmessages>true</joinmessages>
    <partmessages>true</partmessages>
    <kickmessages>true</kickmessages>
    <karmabotintegration>false</karmabotintegration>
</welcomebotconfig>
```

Installing
======

Welcome Bot comes with Chaskis by default. To enable, open PluginConfig.xml, and add the following line inside of &lt;pluginconfig&gt;

### Windows: ###

```XML
<assembly path="C:\Program Files\Chaskis\Plugins\WelcomeBot\WelcomeBot.dll" />
```

### Linux: ###

```XML
<assembly path="/usr/lib/Chaskis/Plugins/WelcomeBot/WelcomeBot.dll" />
```

Sample Output:
======

```
[03:50.04] * You have joined #testseth
[03:50.04] Channel modes for #testseth are :+ns
[03:50.04] Channel Created on: 7:44 PM 6/25/2017
[03:50.05] <SethTestBot> xforever1313 has joined #testseth
```

Optional Dependencies:
======

 * KarmaBot
    * If the [KarmaBot](https://github.com/xforever1313/Chaskis/tree/master/Chaskis/Plugins/KarmaBot) plugin is enabled, each time a user joins, their karma will be printed out.
    * Example:
    ```
    [04:48.03] * You have joined #testseth
    [04:48.05] <SethTestBot> xforever1313 has joined #testseth
    [04:48.06] <SethTestBot> User xforever1313 has 1 karma
    ```