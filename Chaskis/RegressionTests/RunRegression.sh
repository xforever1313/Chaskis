java -jar fitnesse-standalone.jar -p 10013 -e 1 -c ChaskisTests.AllTests?suite -b TestResults.html
sed -i -e 's/\/files\//https:\/\/files.shendrick.net\//g' ./TestResults.html