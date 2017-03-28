Cowsay Bot
==============

About
======

Cowsay Bot is a plugin for Chaskis.  An IRC user is able to call this bot and the bot responds with the message in a bubble of a cow.  For example:

```
[06:07.27] <xforever1313> !cowsay hello
[06:07.27] <CowSayBot>  _______ 
[06:07.28] <CowSayBot> < hello >
[06:07.28] <CowSayBot> '-------'
[06:07.28] <CowSayBot>         \   ^__^
[06:07.28] <CowSayBot>          \  (oo)\_______
[06:07.28] <CowSayBot>             (__)\       )\/\
[06:07.28] <CowSayBot>                 ||----w |
[06:07.28] <CowSayBot>                 ||     ||
```

Installing
======

CowsayBot comes with Chaskis by default.  It lives in /home/chaskis/.config/Chaskis/Plugins/CowSayBot.  To enable, open /home/chaskis/.config/Chaskis/PluginConfig.xml, and add the following line inside of &lt;pluginconfig&gt;

&lt;assembly path="/home/cowsaybot/.config/Chaskis/Plugins/CowSayBot/CowSayBot.dll" /&gt;

You must have cowsay installed.  Install it using your favorite package manager:
* Arch: pacman -S cowsay
* Debian: apt-get install cowsay

Configuring
=====
There is a cowsay configuration file located in /home/chaskis/.config/Chaskis/Plugins/CowSayBot/CowSayBotConfig.xml.  It looks like this:

```
<cowsaybotconfig>
    <command><![CDATA[!{%saycmd%} (?<msg>.+)]]></command>
    <path>/usr/bin/cowsay</path>
    <cowsaycooldown>5</cowsaycooldown>
    <cowfiles>
        <cowfile command="cowsay" name="DEFAULT" />
        <cowfile command="vadersay" name="vader" />
        <cowfile command="tuxsay" name="tux" />
        <cowfile command="moosesay" name="moose" />
        <cowfile command="lionsay" name="moofasa" />
    </cowfiles>
</cowsaybotconfig>
```

**Command:**

Command is the regex the bot looks for.  This MUST contain the group '(?<msg>.+)'.  Any characters captured by this group is what gets echoed in the cow's bubble to the IRC Channel.  {%saycmd%} will be replaced by all the valid commands.  For example if your valid cowfile commands are "cowsay" and "tuxsay", it gets replaced with (cowsay)|(tuxsay) so either will trigger the bot.  The default message it looks for is !cowsay message.  Command will replace {%nick%} with your bot's nickname, and {%channel%} with whatever channel your bot is listening on.

Other examples:
* (@{%nick%}:?\s*)?!{%saycmd%} (?<msg>.+)  allows "!cowsay something" or "@CowSayBot !cowsay something" to trigger the bot

**Path:**

The Path to where the cowsay executable lives.

**cowsaycooldown:**

How long in seconds before the bot responds to a new command after sending one.  This can be used to prevent flooding of the channel.  For example, if set to 5, the bot will ignore all !cowsay commands until 5 seconds has passed.  Set to 0 for no limit.

**cowfiles:**

Cowsay has what are called "Cow Files.  You can find which cow files your versoin of cowsay supports by sending the command "cowsay -l".  Each cowfile has two attributes: name and command.  The command is what the bot will listen for in the IRC channel, name is the name of the cowfile to use.  For example, in the above config, !vadersay message will call cowsay with the cowfile vader specified:

```
[05:29.06] <xforever1313> !vadersay your lack of faith disturbs me
[05:29.06] <CowSayBot>  ________________________________ 
[05:29.06] <CowSayBot> < your lack of faith disturbs me >
[05:29.07] <CowSayBot> '--------------------------------'
[05:29.07] <CowSayBot>         \    ,-^-.
[05:29.07] <CowSayBot>          \   !oYo!
[05:29.07] <CowSayBot>           \ /./=\.\______
[05:29.07] <CowSayBot>                ##        )\/\
[05:29.07] <CowSayBot>                 ||-----w||
[05:29.08] <CowSayBot>                 ||      ||
[05:29.08] <CowSayBot>                Cowth Vader
```
