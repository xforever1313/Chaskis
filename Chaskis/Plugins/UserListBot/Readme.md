User List Bot
==============

About
======
This bot prints all the users in the channel in a single message.

Configuration
=====
There are two things to configure: command and cooldown.

```XML
<!--
This is the config for the user list bot.

    comamnd - The command that when a user triggers it in IRC  {%nick%} gets replaced with the bot's name.
    
    cooldown - Time in seconds before the bot responds to the command again.  If the time since the last command
               is less than this value, the command is ignored.
               Can be 0.  Can not be negative.
-->
<userlistconfig>
    <command>!userlist</command>
    <cooldown>60</cooldown>
</userlistconfig>
```

User the following for a default configuration:
```XML
<userlistconfig/>
```

Installing
======

UserListBot comes with Chaskis by default. To enable, open PluginConfig.xml, and add the following line inside of &lt;pluginconfig&gt;

*Windows:*
```XML
<assembly path="C:\Program Files\Chaskis\Plugins\UserListBot\UserListBot.dll" />
```

*Linux:*
```XML
<assembly path="/usr/lib/Chaskis/Plugins/UserListBot/UserListBot.dll" />
```

Sample Output:
======
```
[10:30.43] <xforever1313> !userlist
[10:30.43] <SethTestBot> Users in #TestSeth: SethTestBot @xforever1313
```
