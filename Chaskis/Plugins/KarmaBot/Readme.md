KarmaBot
=========

Karmabot keeps track of Karma users (or other things) have.  Karma is points earned when a user or thing does something positive, and taken away when a user or thing does something bad.

Instructions
-------

By default, to give karma to something or someone, type

```
++userName or userName++ 
```

in the IRC chat.  To take away karma, type
```
--userName or userName--
```
in the IRC chat.

You can also specify a reason for increasing or decreasing karma:
```
++csharp for being better than Java!
```

To query how much Karma a user has, type
```
!karma userName
```

Configuration
--------
The plugin settings are located in the default Chaskis plugin folder (Chaskis/Plugins/KarmaBot).  There are three settings you can set.  The default is below.

```XML
<!--
Settings for the karma bot.

    increasecmd - The command to increase karma of the given name.
                  This MUST contain the regex group "name", or this isn't going to work.
                  Default is ++name or name++
                  
     decreasecmd - The command to decrease karma of the given name.
                   This MUST contain the regex group "name", or this isn't going to work.
                   Default is --name or name--
                 
     querycmd - The command to query how much karma something has.
                This MUST contain the regex group "name", or this isn't going to work.
                Default is !karma name.
-->
<karmabotconfig>
    <increasecmd><![CDATA[^((\+\+(?<name>\S+))|((?<name>\S+)\+\+))]]></increasecmd>
    <decreasecmd><![CDATA[^((--(?<name>\S+))|((?<name>\S+)--))]]></decreasecmd>
    <querycmd><![CDATA[^!karma\s+(?<name>\S+)]]></querycmd>
</karmabotconfig>

```

Installation
--------
KarmaBot is included as a default Chaskis plugin.  To enable, open PluginConfig.xml and add the following line:

### Windows: ###

```XML
<assembly path="C:\Program Files\Chaskis\Plugins\KarmaBot\KarmaBot.dll" />
```

### Linux: ###

```XML
<assembly path="/usr/lib/Chaskis/Plugins/KarmaBot/KarmaBot.dll" />
```

Sample Output
--------

```
[03:32.31] <xforever1313> ++chaskis
[03:32.32] <@SethTestBot> chaskis has had their karma increased to 2
[03:32.45] <xforever1313> !karma seth
[03:32.45] <@SethTestBot> seth has 1 karma.
[03:33.11] <xforever1313> --java for being terrible language
[03:33.11] <@SethTestBot> java has had their karma decreased to -1
```

Registered Chaskis Events
--------
 * Query Karma - Allows another plugin to query a user's Karma.
    * **Command:** QUERY NAME=userName CHANNEL=#myChannel
    * **Response:** QUERY NAME=userName CHANNEL=#myChannel KARMA=X
    * NAME: The thing whose karma to query.
    * CHANNEL: This simply gets echoed back in the response as is.  Useful in case a plugin is on a per-channel context.
    * KARMA: Number of karma that userName has as an int.

    * Example:
      ```
      :xforever1313!~xforever1313@W.X.Y.Z JOIN #testseth
      CHASKIS PLUGIN WELCOMEBOT KARMABOT QUERY NAME=xforever1313 CHANNEL=#testseth
      CHASKIS PLUGIN KARMABOT WELCOMEBOT QUERY NAME=xforever1313 CHANNEL=#testseth KARMA=1
      ```

Credits
--------

 * ### Entity Framework ###
    * **License:** https://www.microsoft.com/web/webpi/eula/net_library_eula_enu.htm
    * **Website:** https://msdn.microsoft.com/en-us/library/aa937723(v=vs.113).aspx

 * ### SQLite.Net-PCL
    * **License:** https://raw.githubusercontent.com/oysteinkrog/SQLite.Net-PCL/master/LICENSE.txt
    * **Website:** https://github.com/oysteinkrog/SQLite.Net-PCL