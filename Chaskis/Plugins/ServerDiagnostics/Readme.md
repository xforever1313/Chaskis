Server Diagnostics Bot
==============

About
======
This bot prints out diagnostics about the server the bot is on.

Diagnostics include:

    - How long the the bot has been up.
    - The OS version.
    - The number of processors.
    - The time of the server.

Configuration
=====
By default, the commands to trigger the above diagnostics are:

| Command: | What gets printed: |
| ------- | :----: |
| @botName: utime | I have been running for 0 Day(s), 1 Hour(s), 2 Minute(s), and 3 Second(s). |
| @botName: osversion | My system is Microsoft Windows NT 5.1.2600.0. |
| @botName: processorcount | My system has 4 processors. |
| @botName: time | My time is 12:03:02 UTC. |

All commands have a 60 second cooldown.

Modify Config/SampleServerDiagnosticsConfig.xml to change these default commands.  You can also leave commands blank in the config for the bot to ignore it.

Sample Output:
======
```
[11:07.56] <xforever1313> @SethTestBot utime
[11:07.56] <SethTestBot> I have been running for 0 Day(s), 0 Hour(s), 3 Minute(s), and 28 Second(s).
[11:08.07] <xforever1313> @SethTestBot processorcount
[11:08.08] <SethTestBot> My system has 8 processors.
[11:08.15] <xforever1313> @SethTestBot osversion
[11:08.15] <SethTestBot> My system is Microsoft Windows NT 6.2.9200.0.
[11:08.27] <xforever1313> @SethTestBot time
[11:08.27] <SethTestBot> My time is 2016-05-29 03:08:27 UTC.
```