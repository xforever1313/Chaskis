Welcome Bot
==============

About
======
This bot will say if a user joins or leaves a channel.

Configuration
=====

There is no configuration.

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