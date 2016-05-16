
Writing Chaskis Plugins
========

1. Forking the repo

To write plugins, the easiest way is to fork this repo.  Then, clone the submodule SethCS,  Once cloned, go ahead and open Chaskis/Chaskis.sln in Visual Studio, Xamarin Studio, or MonoDevelop.  Create a new project and set the project type to "Class Library".  This will compile to a .dll that Chaskis can load at run time.  Your project must then add a reference to the GenericIrcBot project included in Chaskis so the thing will compile.

For those who do not wish to fork this repo, you will need at a minimum a reference to GenericIrcBot.dll and the SethCSMono shared project.  All other projects are not dependencies when compiling.

2. Implement the IPlugin interface.

You must implement the IPlugin interface for one (and only one) class in your plugin.  The IPluin interface only has two functions to implement: Init() and GetHandlers().

Init initializes the plugin.  The Chaskis plugin loader will pass in two arguments:  The first being the absolute path to the .dll it loaded (including the filename of the dll).  This can be useful if you have config files you need to read-in that live in the save directory as the assembly.  The second argument is a read-only IrcConfig object.  This includes information about the IRC connection, such as the server connected to, the channel listening on, the nickname of the bot, etc.  Some plugins may want this information.  If something bad happens during Init(), such as a config file missing or an external library missing, throw an exception.  If Chaskis is running as a service, it will terminate.  If running as a command-line program and the user has failOnBadPlugin set to yes, the program will terminate gracefully.

GetHandlers returns all the event handlers your plugin has.  This gets returned as an IList.  Your Init function needs to populate a list of event handlers that this function should return.  See below for more information about handlers.

3.  Write handlers

For each IRC message the bot hears, it could handle that message in multiple ways as defined by you, the programmer!  You don't need to worry about implementing handler classes, those already exist. You simply need to create them and add them to your plugin's internal list of them.

At the moment, there are three kinds of event handlers: JoinHandler, PartHandler, and MessageHandler.

Join Handlers are fired whenever a user (not counting your bot) joins the channel the bot is in.  To create a join handler, create a void function that takes in an IIrcWriter (so you can write to the IRC channel) and an IrcResponse object (so you can get the user who joined, and the channel they joined to).  See below for an example .

```
/// <summary>
/// Ran when someone joins the channel.  Prints the user has joined the channel.
/// </summary>
/// <param name="writer">The means to write to an IRC channel.</param>
/// <param name="response">A response from the server.</param>
private static void JoinMessage( IIrcWriter writer, IrcResponse response )
{
    writer.SendCommand( response.RemoteUser + " has joined " + response.Channel );
}

// Some other function somewhere else
JoinHandler handler = new JoinHandler( JoinMessage );

// at some point, add handler to your internal handler list that is returned when GetHandlers() is called.
```

Part Handlers are fired whenever a user (not counting your bot) parts the channel the bot is in.  Creating a part handler is exactly like the join handler.

```
/// <summary>
/// Ran when someone parts the channel.  Tells the channel that they left.
/// </summary>
/// <param name="writer">The means to write to an IRC channel.</param>
/// <param name="response">A response from the server.</param>
private static void PartMessage( IIrcWriter writer, IrcResponse response )
{
    writer.SendCommand(
        response.RemoteUser + " has left " + response.Channel
    );
}

// Some other function somewhere else
PartHandler handler = new PartHandler( PartMessage );

// at some point, add handler to your internal handler list that is returned when GetHandlers() is called.
```

A good example of Join and Part handlers being used in a plugin is the WelcomeBotPlugin, located in Chaskis/Plugins/WelcomeBotPlugin/WelcomeBot.cs.

The last handler will probably be the most common:  The Message Handler.  This fires whenever someone sends a PRIVMSG to either your bot directly, or the channel the bot is in.  This one is a tad more complicated to set up than the join or part handlers, but is still straight forward.

When constructing a message handler, you will need to pass in a few arguments.  The first is a regex that your bot will look for that will fire the passed-in action.  For example, if you want your bot to respond with "Hello, World" when a user types "!hello", pass in "!hello" to this function.  The next argument is the action that will fire if the regex matches.  See below for the action that will send "Hello, World" to the channel.

```
/// <summary>
/// Ran when someone types !hello in the channel.
/// The bot only sends "Hello, World!" to the channel.
/// </summary>
/// <param name="writer">The means to write to an IRC channel.</param>
/// <param name="response">A response from the server.</param>
private static void HelloMessage( IIrcWriter writer, IrcResponse response )
{
    writer.SendCommand(
        "Hello, World!"
    );
}
```

The next argument is a cooldown.  This is defaulted to zero.  This is the number of seconds the bot will wait before firing the action since it received a message that previously fired the action.  For example, if this is set to 5, if user A sends "!hello" the bot will respond with "Hello, World!".  However, 2 seconds later, user A sends it again, the cool down hasn't passed yet, so the bot ignores the line.

The next option is ResponseOptions.  Set this to RespondOnlyToPMs if you want the bot to only respond to Private Messages, not messages that appear in channels.  RespondOnlyToChannel should be turned on if you want the bot to only respond to messages that appear in the channel it is in.  RespondToBoth responds to both private messages and messages in the channel.

The last option is whether or not the bot will respond to itself.  This is typically set to false.  If set to true, you run the risk of the bot ending up in an endless loop since it keeps responding to itself.  Set to true carefully.

4.  Tell Chaskis to load the plugin

5.  Test