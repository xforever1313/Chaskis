Quote Bot
==============

About
======
This bot saves memorable quotes from the IRC channel into a [LiteDB](http://www.litedb.org/) database.  Users can then query a random quote from the bot, or query a specific quote based on its unique ID.

Configuration
=====

The configuration file lives in /home/chaskis/.config/Chaskis/Plugins/QuoteBot/QuoteBotConfig.xml

```XML
<?xml version="1.0" encoding="utf-8" ?>
<!--
Quote bot configuration:

addcommand - The command to add a quote.  
             Must be a regex that contains groups "user" and "quote".
             
deletecommand - The command to delete a quote from the database based on its ID. 
                Must be a regex that contains an "id" group.
                
randomcommand - The command to tell the bot to post a random quote from the database.

getcommand - The command used to get a quote with a specific ID from the database, and post it to the channel.
             Must be a regex that contains an "id" group.
             
Note: Notice for all the commands, there is a '^' in front.  This is important, otherwise a user can do
      "lololol !quote get 12" and the bot will parse that.  However, if you want that behavior, remove the '^'
      from the front.
-->
<quotebotconfig>
    <addcommand><![CDATA[^!quote\s+add\s+\<(?<user>\S+)\>\s+(?<quote>.+)]]></addcommand>
    <deletecommand><![CDATA[^!quote\s+delete\s+(?<id>\d+)]]></deletecommand>
    <randomcommand><![CDATA[^!quote\s+random]]></randomcommand>
    <getcommand><![CDATA[^!quote\s+(get)?\s*(?<id>\d+)]]></getcommand>
</quotebotconfig>
```

Installing
======

QuoteBot comes with Chaskis by default.  To enable, open PluginConfig.xml, and add the following line inside of &lt;pluginconfig&gt;

### Windows: ###

```XML
<assembly path="C:\Program Files\Chaskis\Plugins\QuoteBot\QuoteBot.dll" />
```

### Linux: ###

```XML
<assembly path="/usr/lib/Chaskis/Plugins/QuoteBot/QuoteBot.dll" />
```

Sample Output:
======
```
[09:31.07] <xforever1313> !quote add <thenaterhood> Hello there, how are you?
[09:31.08] <SethTestBot> Quote said by thenaterhood added by xforever1313.  Its ID is 1.
[09:38.24] <xforever1313> !quote get 3
[09:38.25] <SethTestBot> 'Hi there!' -someone. Added by me on 4/25/2017.
[09:35.18] <xforever1313> !quote delete 2
[09:35.19] <SethTestBot> Quote 2 deleted successfully.
```

LiteDB database gets saved as Chaskis/Plugins/QuoteBot/quotes.ldb in your Chaskis Config Root.

Credits
--------

 * ### LiteDB ###
    * **License:** MIT: https://github.com/mbdavid/LiteDB/blob/master/LICENSE
    * **Website:** http://www.litedb.org/
