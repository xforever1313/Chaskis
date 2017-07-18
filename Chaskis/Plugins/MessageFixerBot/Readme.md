Message Fixer Bot
==============

About
======
This bot allows a user to "Edit" their last IRC message by using the Unix SED syntax.  The bot will then type out their edited message.

This bot supports Regexes.

To trigger the bot, simply type ```s/FindRegex/Replace```.
Wherever 'FindRegex' matches in the previous message the user sent, it gets replaced with 'Replace'.
Use "\/" in the FindRegex to escape '/' characters.

Configuration
=====

There is no configuration.

Installing
======

Message Fixer Bot comes with Chaskis by default. To enable, open PluginConfig.xml, and add the following line inside of &lt;pluginconfig&gt;

### Windows: ###

```XML
<assembly path="C:\Program Files\Chaskis\Plugins\MessageFixerBot\MessageFixerBot.dll" />
```

### Linux: ###

```XML
<assembly path="/usr/lib/Chaskis/Plugins/MessageFixerBot/MessageFixerBot.dll" />
```

Sample Output:
======

```
[10:12.59] <xforever1313> Hello world!
[10:13.12] <xforever1313> s/World/Earth!
[10:13.12] <SethTestBot> @xforever1313's updated message: 'Hello Earth!!'
[10:14.34] <xforever1313> s/[h]/j
[10:14.34] <SethTestBot> @xforever1313's updated message: 'jello world!'
[10:14.39] <xforever1313> s/[]/j
[10:14.39] <SethTestBot> @xforever1313: error when trying to fix your message: 'parsing "[]" - Unterminated [] set.'
[10:14.52] <xforever1313> s/jkjlk/j
[10:14.52] <SethTestBot> @xforever1313: error when trying to fix your message: 'Your find regex doesn't match anything in your previous message, can not edit.'
[10:15.46] <xforever1313> s/[]/j
[10:15.47] <SethTestBot> @xforever1313: error when trying to fix your message: 'parsing "[]" - Unterminated [] set.'
[10:15.58] <xforever1313> s/(()/j
[10:15.59] <SethTestBot> @xforever1313: error when trying to fix your message: 'parsing "(()" - Not enough )'s.'
[10:16.33] <xforever1313> Hello World!
[10:16.40] <xforever1313> s/Earth/World
[10:16.40] <SethTestBot> @xforever1313: error when trying to fix your message: 'Your find regex doesn't match anything in your previous message, can not edit.'
[10:16.47] <xforever1313> s/World/Earth
[10:16.48] <SethTestBot> @xforever1313's updated message: 'Hello Earth!'
[10:16.59] <xforever1313> s/[h]/J
[10:17.00] <SethTestBot> @xforever1313's updated message: 'Jello World!'
[10:17.06] <xforever1313> s/[hw]/J
[10:17.06] <SethTestBot> @xforever1313's updated message: 'Jello Jorld!'
[10:17.19] <xforever1313> s/.+/Good Bye World!
[10:17.20] <SethTestBot> @xforever1313's updated message: 'Good Bye World!'
```
