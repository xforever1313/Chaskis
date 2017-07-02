Cross Channel Bot
==============

About
======
This bot allows a user to send a message to a different IRC channel the bot is in from a different channel.  It also allows a user to send a message to ALL channels the bot is in.

Configuration
=====

There is no configuration.

Installing
======

Cross Channel Bot comes with Chaskis by default. To enable, open PluginConfig.xml, and add the following line inside of &lt;pluginconfig&gt;

*Windows:*
```XML
<assembly path="C:\Program Files\Chaskis\Plugins\CrossChannel\CrossChannel.dll" />
```

*Linux:*
```XML
<assembly path="/usr/lib/Chaskis/Plugins/CrossChannel/CrossChannel.dll" />
```

Sample Output:
======

Channel #TestSeth:
```
[07:58.35] <xforever1313> !cc <#testseth2> hello
[07:59.00] <xforever1313> !broadcast I am broadcasting!
[07:59.00] <SethTestBot> <xforever1313@#testseth> I am broadcasting!
```

Channel #TestSeth2:
```
[07:58.35] <SethTestBot> <xforever1313@#testseth> hello
[07:59.01] <SethTestBot> <xforever1313@#testseth> I am broadcasting!
```