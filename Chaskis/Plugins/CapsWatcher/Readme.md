Caps Watcher Bot
==============

About
======
This bot yells at users if they decide to use all caps.

The messages are 

Configuration
=====
The only configuration file is what message you wish to send to an offending user.  To include the offending user's name in the message, add \{%user%\} somewhere in the message.

The configuration file lives in /home/chaskis/.config/Chaskis/Plugins/CapsWatcher/CapsWatcherConfig.xml

```XML
<?xml version="1.0" encoding="utf-8" ?>

<!--
    The config for caps watcher is very simple.  Each tag is something
    you want the bot to randomly say when someone says something that is in all caps.
    {%user%} is replaced with the offending user's nickname.
    
    If no config is specified, the plugin will not validate and abort, and not work.
    Ditto if there's an empty string (<message></message>).
    
    Remember to use <![CDATA[Your message]]> inside of the <message> tags if your message
    contains XML stuff.
    
    Rules for triggering the bot:
        - Message must contain at least one letter
        - All letters must be in all caps.  If not, the bot is not triggered.
        - The message must be at least 3 characters long.
-->    
<capswatcherconfig>
    <message>LOUD NOISES!</message>
    <message>@{%user%}: shhhhhhhhhh!</message>
    <message>Contrary to popular belief, caps lock is not cruise control for cool :/</message>
</capswatcherconfig>
```

Installing
======
CapsWatcher comes with Chaskis by default.  To enable, edit your PluginConfig.xml, and add the following line inside of &lt;pluginconfig&gt;

*Windows:*
```XML
<assembly path="C:\Program Files\Chaskis\Plugins\CapsWatcher\CapsWatcher.dll" />
```

*Linux:*
```XML
<assembly path="/usr/lib/Chaskis/Plugins/CapsWatcher/CapsWatcher.dll" />
```

Sample Output:
======
```
[09:47.49] <xforever1313> OKAY
[09:47.49] <SethTestBot> @xforever1313: shhhhhhhhhh!
[09:47.58] <xforever1313> fine :(
[09:48.02] <xforever1313> LOUD NOISES!
[09:48.02] <SethTestBot> Contrary to popular belief, caps lock is not cruise control for cool :/
[09:49.32] <xforever1313> GRUMBLE
[09:49.32] <SethTestBot> LOUD NOISES!
```
