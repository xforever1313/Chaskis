Math Bot
==========

About
-------

Math Bot allows IRC users to calculate Math expressions right in the IRC Chat!

Instructions
--------------

The command to invoke Math Bot is !calculate or !calc, followed by your expression.  For example:

```
!calculate 2 + 1
!calculate True and False
!calc (2 * 1) + (3 + 3)
!calc ((1 + 4) > 3) AND ( 10 - 3 > 3 )
```

Installation
--------
Math Bot is included as a default Chaskis plugin.  To enable, open PluginConfig.xml and add the following line:

```XML
<assembly path="Path/To/Chaskis/Install/Chaskis/Plugins/MathBot/MathBot.dll" classname="Chaskis.Plugins.MathBot.MathBot" />
```

Sample Output
--------
```
[04:31.21] <xforever1313> !calc 2 + 3
[04:31.21] <ChaskisBot> '2 + 3' calculates to '5'
[04:31.28] <xforever1313> !calc 2 / 3
[04:31.28] <ChaskisBot> '2 / 3' calculates to '0.666666666666667'
[04:31.32] <xforever1313> !calc 2 / 0
[04:31.33] <ChaskisBot> '2 / 0' calculates to '∞'
[04:31.53] <xforever1313> !calc 2 / 2 + 2
[04:31.54] <ChaskisBot> '2 / 2 + 2' calculates to '3'
[04:31.59] <xforever1313> !calc 2 / ( 2 + 2 )
[04:31.59] <ChaskisBot> '2 / ( 2 + 2 )' calculates to '0.5'
[04:32.08] <xforever1313> !calc true and false
[04:32.09] <ChaskisBot> 'true and false' calculates to 'False'
[04:32.30] <xforever1313> !calc 1 != 2
[04:32.30] <ChaskisBot> '1 != 2' calculates to 'True'
[04:32.37] <xforever1313> !calc 1 == 2
[04:32.37] <ChaskisBot> '1 == 2' calculates to 'False'
[04:32.44] <xforever1313> !calc 1 <> 2
[04:32.45] <ChaskisBot> '1 <> 2' calculates to 'True'
[04:32.56] <xforever1313> !calc 1 === 2
[04:32.56] <ChaskisBot> '1 === 2' is not something I can calculate :(
[04:35.43] <xforever1313> !calculate 10000000000000000 + 10000000000000000000000000000
[04:35.43] <ChaskisBot> '10000000000000000 + 10000000000000000000000000000' calculates to
'1.000000000001E+28'
[04:36.02] <xforever1313> !calculate 1 / 0 + 1
[04:36.02] <ChaskisBot> '1 / 0 + 1' calculates to '∞'
[04:37.18] <xforever1313> !calc 2 + 3
[04:37.18] <ChaskisBot> '2 + 3' calculates to '5'
[04:37.27] <xforever1313> !calc true and true or false and true
[04:37.28] <ChaskisBot> 'true and true or false and true' calculates to 'True'
[04:37.40] <xforever1313> !calc 1 < 3
[04:37.40] <ChaskisBot> '1 < 3' calculates to 'True'
```
