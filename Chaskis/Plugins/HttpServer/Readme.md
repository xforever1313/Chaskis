HTTP Server Plugin
==============

About
======
This plugin's purpose is to control sending commands to the bot via HTTP.  It can be a form of interprocess-communication.  Want to send a message to IRC when a CI build completes?  Send an HTTP Post request to this plugin and the bot will send a message.

Usage
=====
To send commands, you must ALWAYS do a POST request.  Any other request type shall be ignored.

The URL to POST to is http://server:PORT/command, and the query string will contain arguments for each command.  Unless specified otherwise, all arguments are required.

The HTTP response will be in XML format.  A sample response is:

```XML
<chaskis_http_response>
    <status>Ok</status>
    <error_message>None</error_message>
    <message>A full message</message>
</chaskis_http_response>
```

status can be:
 * **Ok** - The message has been queued up.  This will return an HTTP status code of 200 (ok).
 * **ClientError** - The request is invalid.  Check your URL or query string.  This will return an HTTP status code of 400 (Bad Request)
 * **ServerError** - Something went wrong server-side.  This will return an HTTP status code of 500 (Internal Server Error).

error_message is a static error code.

message is a full message explain what happened.

At the moment, the plugin allows one to send the following IRC commands:

 * PRIVMSG
 * PART
 * KICK
 * BCAST

PRIVMSG
---
Post to http://server:PORT/privmsg to send a message to the channel as the bot.  The query string must contain the following information:

 * **channel** - The channel to send the message to.  The bot must have joined the channel in order for the message to be sent
 * **message** - The message to send to the channel.

The query string should look something like:
```
channel=#somechannel&message=Hello%20World
```

PART
---
Post to http://server:PORT/part for the bot to leave a channel.  The query string must contain the following information:

 * **channel** - The channel to leave.  The bot must have joined the channel in order for it to leave it.
 * **message** - Optional.  The message to send while leaving the channel.  If not specified, a default message is sent instead.

The query string should look something like:
```
channel=#somechannel&message=Bye%20Channel
```

KICK
---
Post to http://server:PORT/kick for the bot to kick as user from a channel.  The query string must contain the following information:

 * **channel** - The channel the user is to be kicked from.
 * **user** - The user that will be kicked from the channel
 * **message** - Optional. The reason why the user was kicked.  If left blank, the server will use its default reason for kicking.

The query string should look something like:
```
channel=#somechannel&user=baduser&message=Go%20Away
```
BCAST
---
Post to either http://server:PORT/bcast or http://server:PORT/broadcast to send a message to ALL channels the bot is currently joined in.  The query string must contain the following information:

 * **message** - The message to send to all channels the bot joined.

The query string should look something like:
```
message=Hello%20All%20Channels
```

Warnings
=====
This is NOT designed to be exposed to the public-facing internet.  DO NOT set the port to listen on on a port that is public facing.  Otherwise, anyone can control your IRC bot, and you probably don't want that.

Configuration
=====
The plugin settings are located in the default Chaskis plugin folder (Chaskis/Plugins/HttpServer/HttpServerConfig.xml).  The default configuration is below.

```XML
<?xml version="1.0" encoding="utf-8" ?> 
<!-- 
This plugin allows a user to send an HTTP request to the bot in order to perform
actions on the channel.

 - Port: Which port to open to accept HTTP requests from.  It is discouraged to put a port
         that is open outside of your PC, unless you trust everyone on your network.
         DO NOT put this on a port that is exposed to the internet, unless you want strangers
         controlling your bot.
-->
<httpserverconfig xmlns="https://files.shendrick.net/projects/chaskis/schemas/httpserverconfigschema/2018/HttpServerConfigSchema.xsd">
    <port>10080</port>
</httpserverconfig>
```

Installation
======
HttpServer is included as a default Chaskis plugin.  To enable, open PluginConfig.xml and add the following line:

### Windows: ###

```XML
<assembly path="C:\Program Files\Chaskis\Plugins\HttpServer\HttpServer.dll" />
```

### Linux: ###

```XML
<assembly path="/usr/lib/Chaskis/Plugins/HttpServer/HttpServer.dll" />
```

