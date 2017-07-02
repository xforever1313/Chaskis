XML Bot
==============

About
======
This bot takes in an XML file of commands and their corresponding response.  You can generate responses without needing any coding experience!

Configuration
=====

```XML
<?xml version="1.0" encoding="utf-8" ?>

<!-- 
Xml Bot Config.

Use this file to have your bot generate responses based on the commands users give.

command - 
    The command regex to trigger the response.
    Accepts usual Chaskis replacement for channel, user, nick:
    {%channel%} is replaced with the channel name the bot is listening on.
    {%nick%} is replaced with the bot's nickname
    {%user%} is replaced with the user who sent the command's user name.
    
    For example:
        !{%nick%} {%channel%} {%user%}
        will make the command the bot will watch for:
        !botNick #myChannel someUser.
        
    Commands can also do regex groups as a find/replace mechanism.
    A regex group's value can be sent in the response by having {%groupName%}
    in the response.
    
    For example, having the command:
        My name is (?<name>\w+) 
        
    and a response:
        Hello {%name%}!
        
    will produce this if in chat:
        <user> My name is user
        <bot> Hello user!


    Tip: If you are doing a command such as !someCommand, put '^' in the front of the command regex.
         That way, "!command" will be triggered only if "!command" is at the start of the message, not if something
         like "lol !command" comes through the chat.

response -
    What the bot responds with when the command is seen in the channel.
    
    Any regex groups in the command whose value should be outputted in the response should be tagged
    like this: {%groupName%}.
    
cooldown -
    How long in SECONDS before the bot responds to the command again.  Defaulted to no cooldown.
    
respondto -
    Does the command only respond to messages in the channel, PMs only, or both.
    
    ChannelAndPms - Responds to both messages in the channel and PMs.
    PmsOnly - Only respond to Private Messages from users.
    ChannelOnly - Only respond to messages from a channel.
-->
<xmlbotconfig xmlns="https://files.shendrick.net/projects/chaskis/schemas/xmlbotconfig/2017/xmlbotconfig.xsd">
    <message>
        <command>[Hh]ello {%nick%}</command>
        <response>Hello {%user%}!</response>
        <cooldown>0</cooldown>
        <respondto>ChannelAndPms</respondto>
    </message>
    <message>
        <command><![CDATA[[Mm]y\s+name\s+is\s+(?<name>\w+)]]></command>
        <response>Hello {%name%}!</response>
        <cooldown>1</cooldown>
        <respondto>ChannelOnly</respondto>
    </message>
    <message>
        <command>^!{%nick%} {%channel%} {%user%}</command>
        <response>Hello {%user%}, I am {%nick%} on channel {%channel%}!</response>
        <cooldown>0</cooldown>
        <respondto>ChannelAndPms</respondto>
    </message>
    <message>
        <command><![CDATA[[Ww]hat is a(?<an>n)? (?<thing>\w+)]]></command>
        <response>A{%an%} {%thing%} is a thing!</response>
        <cooldown>0</cooldown>
        <respondto>ChannelAndPms</respondto>
    </message>
</xmlbotconfig>

```

Installing
======

XmlBot comes with Chaskis by default.  To enable, open PluginConfig.xml, and add the following line inside of &lt;pluginconfig&gt;

*Windows:*
```XML
<assembly path="C:\Program Files\Chaskis\Plugins\XmlBot\XmlBot.dll" />
```

*Linux:*
```XML
<assembly path="/usr/lib/Chaskis/Plugins/XmlBot/XmlBot.dll" />
```

Sample Output:
======

This output is from the sample configuration.
```
[09:52.20] <xforever1313> !SethTestBot #TestSeth xforever1313
[09:52.21] <SethTestBot> Hello xforever1313, I am SethTestBot on channel #TestSeth!
[09:52.47] <xforever1313> Hello SethTestBot
[09:52.47] <SethTestBot> Hello xforever1313!
[09:53.21] <xforever1313> What is a mouse?
[09:53.21] <SethTestBot> A mouse is a thing!
[09:53.25] <xforever1313> What is an acorn?
[09:53.25] <SethTestBot> An acorn is a thing!
[09:53.36] <xforever1313> My name is xforever1313
[09:53.36] <SethTestBot> Hello xforever1313!
```
