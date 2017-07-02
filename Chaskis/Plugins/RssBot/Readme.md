RSS Bot
==============

About
======
The RSS Bot plugin pulls from an RSS or ATOM feed at an interval, and then posts it to the IRC channel.

Configuration
=====

```XML
<?xml version="1.0" encoding="utf-8" ?>
<!--

This is the configuration for RSS Bot.

This bot queries the given RSS Feed in the URL tag every X minutes in the refreshinterval tag.

URL - The URL to pull the feed from
refreshinterval - Minutes before it pulls from it again.

Note that this will only post updates to the feed starting AFTER the bot starts up.
Any existing posts on the feed will NOT be posted.

-->
<rssbotconfig xmlns="https://files.shendrick.net/projects/chaskis/schemas/rssbotconfig/2017/rssbotconfig.xsd">
   <feed>
        <url>https://www.shendrick.net/atom.xml</url>
        <refreshinterval>60</refreshinterval>
    </feed>
    <feed>
        <url>http://thenaterhood.com/feed.xml</url>
        <refreshinterval>30</refreshinterval>
    </feed>
</rssbotconfig>

```

Installing
======

RssBot comes with Chaskis by default.  To enable, open PluginConfig.xml, and add the following line inside of &lt;pluginconfig&gt;

*Windows:*
```XML
<assembly path="C:\Program Files\Chaskis\Plugins\RssBot\RssBot.dll" />
```

*Linux:*
```XML
<assembly path="/usr/lib/Chaskis/Plugins/RssBot/RssBot.dll" />
```
