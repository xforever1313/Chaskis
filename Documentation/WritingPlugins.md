
Writing Chaskis Plugins
========

1. **Install Chaskis**

    To write plugins, the easiest way is to clone this repo.  Then, clone the submodule SethCS,  Once cloned, go into the install directory.  There is a Python script called "BuildInstall.py"  Go ahead and run it.  This will install Chakis and the plugins to your Application Data if on Windows, or ~/.config on Linux.  
    Next, make a new Visual Studio or MonoDevelop project.  Select "Class Library" as the type.  Chaskis is currently set for .NET 4.5, so you should pick the same.  Then, add a reference to ChaskisCore.dll from its installed location in AppData/Chaskis/bin/.  Now you are ready to write your plugin!

2. **Implement the IPlugin interface.**

    You must implement the IPlugin interface for one (and only one) class in your plugin.  The IPluin interface has a few things you need to implement.

    * SourceCodeLocation
      The location of your source code (e.g. GitHub).  When someone asks the bot !botname source yourplugin, it will return this string.

    * Version
      A string representation of the version of your plugin.  When someone asks the bot !botname version yourplugin, it will return this string.

    * About
      A brief description of your plugin.  When someone asks the bot !botname about yourplugin, it will return this string.

    * Init()
      Init initializes the plugin.  The Chaskis plugin loader will pass in two arguments:  The first being the absolute path to the .dll it loaded (including the filename of the dll).  This can be useful if you have config files you need to read-in that live in the save directory as the assembly.  The second argument is a read-only IrcConfig object.  This includes information about the IRC connection, such as the server connected to, the channel listening on, the nickname of the bot, etc.  Some plugins may want this information.  If something bad happens during Init(), such as a config file missing or an external library missing, throw an exception.  If Chaskis is running as a service, it will terminate.  If running as a command-line program and the user has failOnBadPlugin set to yes, the program will terminate gracefully.

    * HandleHelp
      When a users asks the bot !botname help yourplugin arg1 arg2,
      this function is called.  You'll get a way to write to the IRC channel via the passed in IIrcWriter, and get information about the IRC message the triggered the bot.  Arguments are also passed in.  !botname help yourplugin are NOT passed into the args array, but everything else after (separated by whitespace) is.

    * GetHandlers 
      Returns all the event handlers your plugin has.  This gets returned as an IList.  Your Init function needs to populate a list of event handlers that this function should return.  See below for more information about handlers.

    * Add the ChaskisPlugin Attribute.
      The last thing you'll need to do is add the ChaskisPlugin attribute to your class.  Any class in your assembly with this attribute will be loaded as a plugin when your assembly is loaded.  The single argument to pass in is your plugin name.  This is what will show up in the IRC channel.  The name is stripped of whitespace and lowercased.

      ```C#
      [ChaskisPlugin( "myplugin" )]
      public class MyPlugin : IPlugin
      {
          // Your implementation.
      }
      ```

3.  **Write handlers**

    For each IRC message the bot hears, it could handle that message in multiple ways as defined by you, the programmer!  You don't need to worry about implementing handler classes, those already exist. You simply need to create them and add them to your plugin's internal list of them.

    * Join Handlers
      Fired whenever a user (not counting your bot) joins the channel the bot is in.  To create a join handler, create a void function that takes in an IIrcWriter (so you can write to the IRC channel) and an IrcResponse object (so you can get the user who joined, and the channel they joined to).  See below for an example .

      ```c#
      /// <summary>
      /// Ran when someone joins the channel.  Prints the user has   joined the channel.
      /// </summary>
      /// <param name="writer">The means to write to an IRC   channel.</param>
      /// <param name="response">A response from the server.</param>
      private static void JoinMessage( IIrcWriter writer, IrcResponse   response )
      {
          writer.SendCommand( response.RemoteUser + " has joined " +   response.Channel );
      }
  
      // Some other function somewhere else
      JoinHandler handler = new JoinHandler( JoinMessage );
  
      // at some point, add handler to your internal handler list   that is returned when GetHandlers() is called.
      ```

    * Part Handlers
      Fired whenever a user (not counting your bot) parts the channel the bot is in.  Creating a part handler is exactly like the join handler.

      ```c#
      /// <summary>
      /// Ran when someone parts the channel.  Tells the channel that    they left.
      /// </summary>
      /// <param name="writer">The means to write to an IRC    channel.</param>
      /// <param name="response">A response from the server.</param>
      private static void PartMessage( IIrcWriter writer, IrcResponse    response )
      {
          writer.SendCommand(
              response.RemoteUser + " has left " + response.Channel
          );
      }
  
      // Some other function somewhere else
      PartHandler handler = new PartHandler( PartMessage );
  
      // at some point, add handler to your internal handler list   that is returned when GetHandlers() is called.
      ```

      A good example of Join and Part handlers being used in a plugin is the WelcomeBotPlugin, located in Chaskis/Plugins/WelcomeBotPlugin/WelcomeBot.cs.

    * Message Handler.
      This fires whenever someone sends a PRIVMSG to either your bot directly, or the channel the bot is in.  This one is a tad more complicated to set up than the join or part handlers, but is still straight forward.

      When constructing a message handler, you will need to pass in a few arguments.  The first is a regex that your bot will look for that will fire the passed-in action.  For example, if you want your bot to respond with "Hello, World" when a user types "!hello", pass in "!hello" to this function.  The next argument is the action that will fire if the regex matches.  See below for the action that will send "Hello, World" to the channel.

      ```c#
      /// <summary>
      /// Ran when someone types !hello in the channel.
      /// The bot only sends "Hello, World!" to the channel.
      /// </summary>
      /// <param name="writer">The means to write to an IRC   channel.</param>
      /// <param name="response">A response from the server.</param>
      private static void HelloMessage( IIrcWriter writer,   IrcResponse response )
      {
          writer.SendCommand(
              "Hello, World!"
          );
      }
      ```

      The next argument is a cooldown.  This is defaulted to zero.  This is the number of seconds the bot will wait before firing the action since it received a message that previously fired the action.  For example, if this is set to 5, if user A sends "!hello" the bot will respond with "Hello, World!".  However, 2 seconds later, user A sends it again, the cool down hasn't passed yet, so the bot ignores the line.

      The next option is ResponseOptions.  Set this to RespondOnlyToPMs if you want the bot to only respond to Private Messages, not messages that appear in channels.  RespondOnlyToChannel should be turned on if you want the bot to only respond to messages that appear in the channel it is in.  RespondToBoth responds to both private messages and messages in the channel.

      The last option is whether or not the bot will respond to itself.  This is typically set to false.  If set to true, you run the risk of the bot ending up in an endless loop since it keeps responding to itself.  Set to true carefully.

      _Note, for all handlers you can also pass in a delegate instead of a function pointer._

    * All Handler
      Fired for ALL received IRC messages, regardless of structure.  Useful when looking for raw IRC commands.

4.  **Tell Chaskis to load the plugin**

    Inside of your users Application data (/home/userName/.config/Chaskis on Linux or c:\Users\userName\AppData\Roaming\Chaskis on Windows), open the PluginConfig.xml.  Add a new assembly tag, and the path is the absolute path to your plugin dll.  Typically, plugins go in AppData/Chaskis/Plugins/PluginName.

5.  **Test**

    Chaskis can be run as a service, but a service is annoying to get Console.out or Console.Error.  Therefore, we include a handy command-line version of Chaskis located in Chaskis/Chaskis for debugging purposes.  If you do not wish to load the default IRC or plugin config in AppData, you can change those over the command line.  You can also set if you want to abort the process if your plugin fails to load, or try to continue.

    ```
    Chaskis IRC Bot Help:
    --help, -h, /?    --------  Prints this message and exits.
    --version         --------  Prints the version and exits.
    --configPath=xxx  --------  The IRC config xml file to use.
                                Default is in AppData.
    --pluginConfigPath=xxx ---  The plugin config xml file to use.
                                Default is in AppData.
    --failOnBadPlugin=yes|no -  Whether or not to fail if a plugin load fails.
                                Defaulted to no.
    ```