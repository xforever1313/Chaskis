ConsoleOut Plugin
=================

This is a very simple plugin that is really only used for debug purposes.  It takes whatever comes from IRC, and prints it to Console.Out.

That's it.

No really, that's it.

This is really only meant to be used with the Console Application (Chaskis.exe) instead of ChaskisService.exe for debug purposes.

Installation
--------
ConsoleOut is included as a default Chaskis plugin.  To enable, open PluginConfig.xml and add the following line:

### Windows: ###
```XML
<assembly path="C:\Program Files\Chaskis\Plugins\ConsoleOut\ConsoleOut.dll" />
```

### Linux: ###
```XML
<assembly path="/usr/lib/Chaskis/Plugins/ConsoleOut/ConsoleOut.dll" />
```

Its recommended that this line is the first line in the XML so its the first handler that gets fired.  This way you can see what you get from IRC before the other plugins react to it.