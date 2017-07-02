Weather Bot
========

Weather bot is a plugin that allows a user to query NOAA's [National Digital Forecast Database](http://graphical.weather.gov/xml/) and retrieve information of weather from a US zip code.  That information is then printed to the IRC channel.

Commands
------

| Command | What it does |
| ------- | :----------: |
| !weather 12345 (or any 5-digit US zip code) | Prints "Weather for 12345 - Partly Cloudy, 77.0F (feels like 77.0F). High: 83.0F. Low: 62.0F. Chance of Precipitation: 19.0%."|
| !weather about | Prints information about the plugin |
| !weather help | Prints valid commands |
| !weather sourcecode | Prints a link to the plugin's source code.

Caveots
------
  * Only US zip codes are accepted
  * NOAA updates their database every hour.  They ask on their site to only query the database no more than once per hour per location.  We therefore do cacheing of each location that is queried in the channel.  After an hour, if a location is queried we grab the latest information from NOAA.
  * Each command has a 15 second cooldown.  Therefore, any commands that happen within 15 seconds of the previous command type are ignored.

Installing
------

WeatherBot comes with Chaskis by default. To enable, open PluginConfig.xml, and add the following line inside of &lt;pluginconfig&gt;

### Windows: ###
```XML
<assembly path="C:\Program Files\Chaskis\Plugins\WeatherBot\WeatherBot.dll" />
```

### Linux: ###
```XML
<assembly path="/usr/lib/Chaskis/Plugins/WeatherBot/WeatherBot.dll" />
```