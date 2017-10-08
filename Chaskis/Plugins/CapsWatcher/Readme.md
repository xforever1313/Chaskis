Caps Watcher Bot
==============

About
======
This bot yells at users if they decide to use all caps.

The messages are 

Configuration
=====
The only configuration file is what message you wish to send to an offending user.  To include the offending user's name in the message, add \{%user%\} somewhere in the message.

The configuration file lives in /home/chaskis/.config/Chaskis/Plugins/CapsWatcher/CapsWatcherConfig.xml

```XML
<?xml version="1.0" encoding="utf-8" ?>

<!--
    The config for caps watcher is very simple.  Each tag is something
    you want the bot to randomly say when someone says something that is in all caps.
    {%user%} is replaced with the offending user's nickname.

    If no config is specified, the plugin will not validate and abort, and not work.
    Ditto if there's an empty string (<message></message>).

    Remember to use <![CDATA[Your message]]> inside of the <message> tags if your message
    contains XML stuff.

    Ignores are a list of strings that are allowed to be capitalized without triggering the bot.
    For example, acronyms are a popular one.
    The ones included by default are common acronyms.

    Rules for triggering the bot:
        - Message must contain at least two capital letters in a row.
        - All letters must be in all caps.  If not, the bot is not triggered.
        - Ignores are not included during the caps test.
-->
<capswatcherconfig xmlns="https://files.shendrick.net/projects/chaskis/schemas/capswatcherconfig/2017/capswatcherconfig.xsd">
    <message>LOUD NOISES!</message>
    <message>@{%user%}: shhhhhhhhhh!</message>
    <message>Contrary to popular belief, caps lock is not cruise control for cool :/</message>
    <ignores>      
        <ignore>AMD</ignore>    <!-- Advanced Micro Devices -->
        <ignore>BLS</ignore>    <!-- Basic Life Support -->
        <ignore>CIA</ignore>    <!-- Central Intelligence Agency -->
        <ignore>CPU</ignore>    <!-- Central Processing Unit -->
        <ignore>DC</ignore>     <!-- Washingon DC, or Direct Current-->
        <ignore>FBI</ignore>    <!-- Federal Bureau of Investigation -->
        <ignore>GPU</ignore>    <!-- Graphical Processing Unit -->
        <ignore>IRC</ignore>    <!-- Internet Relay Chat -->
        <ignore>IIRC</ignore>   <!-- If I Recall Correctly -->
        <ignore>NYC</ignore>    <!-- New York City -->
        <ignore>RSS</ignore>    <!-- Rich Site Summary -->
        <ignore>TIL</ignore>    <!-- Today I Learned -->
        <ignore>TV</ignore>     <!-- Television -->
        <ignore>US</ignore>     <!-- United States -->
        <ignore>USA</ignore>    <!-- United States of America -->
        
        <!-- US States -->
        <ignore>AL</ignore>     <!-- Alabama        -->
        <ignore>AK</ignore>     <!-- Alaska         -->
        <ignore>AZ</ignore>     <!-- Arizona        -->
        <ignore>AR</ignore>     <!-- Arkansas       -->
        <ignore>CA</ignore>     <!-- California     -->
        <ignore>CO</ignore>     <!-- Colorado       -->
        <ignore>CT</ignore>     <!-- Connecticut    -->
        <ignore>DE</ignore>     <!-- Delaware       -->
        <ignore>FL</ignore>     <!-- Florida        -->
        <ignore>GA</ignore>     <!-- Georgia        -->
        <ignore>HI</ignore>     <!-- Hawaii         -->
        <ignore>ID</ignore>     <!-- Idaho          -->
        <ignore>IL</ignore>     <!-- Illinois       -->
        <ignore>IN</ignore>     <!-- Indiana        -->
        <ignore>IA</ignore>     <!-- Iowa           -->
        <ignore>KS</ignore>     <!-- Kansas         -->
        <ignore>KY</ignore>     <!-- Kentucky       -->
        <ignore>LA</ignore>     <!-- Louisiana      -->
        <ignore>ME</ignore>     <!-- Maine          -->
        <ignore>MD</ignore>     <!-- Maryland       -->
        <ignore>MA</ignore>     <!-- Massachusetts  -->
        <ignore>MI</ignore>     <!-- Michigan       -->
        <ignore>MN</ignore>     <!-- Minnesota      -->
        <ignore>MS</ignore>     <!-- Mississippi    -->
        <ignore>MO</ignore>     <!-- Missouri       -->
        <ignore>MT</ignore>     <!-- Montana        -->
        <ignore>NE</ignore>     <!-- Nebraska       -->
        <ignore>NV</ignore>     <!-- Nevada         -->
        <ignore>NH</ignore>     <!-- New Hampshire  -->
        <ignore>NJ</ignore>     <!-- New Jersey     -->
        <ignore>NM</ignore>     <!-- New Mexico     -->
        <ignore>NY</ignore>     <!-- New York       -->
        <ignore>NC</ignore>     <!-- North Carolina -->
        <ignore>ND</ignore>     <!-- North Dakota   -->
        <ignore>OH</ignore>     <!-- Ohio           -->
        <ignore>OK</ignore>     <!-- Oklahoma       -->
        <ignore>OR</ignore>     <!-- Oregon         -->
        <ignore>PA</ignore>     <!-- Pennsylvania   -->
        <ignore>PR</ignore>     <!-- Puerto Rico    -->
        <ignore>RI</ignore>     <!-- Rhode Island   -->
        <ignore>SC</ignore>     <!-- South Carolina -->
        <ignore>SD</ignore>     <!-- South Dakota   -->
        <ignore>TN</ignore>     <!-- Tennessee      -->
        <ignore>TX</ignore>     <!-- Texas          -->
        <ignore>UT</ignore>     <!-- Utah           -->
        <ignore>VT</ignore>     <!-- Vermont        -->
        <ignore>VA</ignore>     <!-- Virginia       -->
        <ignore>WA</ignore>     <!-- Washington     -->
        <ignore>WV</ignore>     <!-- West Virginia  -->
        <ignore>WI</ignore>     <!-- Wisconsin      -->
        <ignore>WY</ignore>     <!-- Wyoming        -->

    </ignores>
</capswatcherconfig>
```

Installing
======
CapsWatcher comes with Chaskis by default.  To enable, edit your PluginConfig.xml, and add the following line inside of &lt;pluginconfig&gt;

### Windows: ###

```XML
<assembly path="C:\Program Files\Chaskis\Plugins\CapsWatcher\CapsWatcher.dll" />
```

### Linux: ###

```XML
<assembly path="/usr/lib/Chaskis/Plugins/CapsWatcher/CapsWatcher.dll" />
```

Sample Output:
======
```
[09:47.49] <xforever1313> OKAY
[09:47.49] <SethTestBot> @xforever1313: shhhhhhhhhh!
[09:47.58] <xforever1313> fine :(
[09:48.02] <xforever1313> LOUD NOISES!
[09:48.02] <SethTestBot> Contrary to popular belief, caps lock is not cruise control for cool :/
[09:49.32] <xforever1313> GRUMBLE
[09:49.32] <SethTestBot> LOUD NOISES!
```
