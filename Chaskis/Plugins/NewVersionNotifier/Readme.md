New Version Notifier Bot
==============

About
======
This plugin will send to all of the channels the bot joins if chaskis is updated to a new version.
It will only send the message the first time the bot joins a channel when it was upgraded.

Configuration
=====

Edit the following in YourChakisRoot/Plugins/NewVersionNotifier/NewVersionNotifierConfig.xml

```XML
<?xml version="1.0" encoding="utf-8" ?>
<!--
This plugin sends a message to all the channels the bot is a member
of when it first joins the channel if the bot's version changed.

- Message: The message to send.  Must be at least 1 character and not all whitespace.  {%version%} is replaced with the Chaskis version.
-->
<newversionnotifierconfig xmlns="https://files.shendrick.net/projects/chaskis/schemas/newversionnotifierconfigschema/2018/NewVersionNotifierConfigSchema.xsd">
    <message>I have been updated to version {%version%}.  Release Notes: https://github.com/xforever1313/Chaskis/releases/tag/{%version%}</message>
</newversionnotifierconfig>
```

You may see a ".lastversion.txt" file in this directory.  Do not touch this file.  Its used to determine if the bot was upgraded or not.
If this file is missing, the bot will assume it was upgraded.

Installing
======

New Version Notifier Bot comes with Chaskis by default. To enable, open PluginConfig.xml, and add the following line inside of &lt;pluginconfig&gt;

### Windows: ###

```XML
<assembly path="C:\Program Files\Chaskis\Plugins\NewVersionNotifier\NewVersionNotifier.dll" />
```

### Linux: ###

```XML
<assembly path="/usr/lib/Chaskis/Plugins/NewVersionNotifier/NewVersionNotifier.dll" />
```

Sample Output:
======

```
[05:40.52] * SethTestBot (~SethTestB@8.41.64.82) has joined channel #TestSeth
[05:40.53] <SethTestBot> I have been updated to version 0.6.1. 
Release Notes: https://github.com/xforever1313/Chaskis/releases/tag/0.6.1
```
