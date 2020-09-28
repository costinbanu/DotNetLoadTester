# DotNetLoadTester
Website load and stress testing tool

## How it works
This program will create as many parallel threads as instructed and will use them to send HTTP GET requests to the website intended for testing. 
Each thread will start with fetching the homepage ("/"), then it will extract all local links found on that particular page. 
Additional filtering is available through the app settings.
It will then pick a random link from those that have been extracted and will set it as the URI for the next request.
Finally, it will wait for one or two seconds (every other thread waits for two seconds).
A thread will exit if it has ran for a longer time than the session timeout.

## Configuration
The base url, parallelism degree and session timeout are easily configurable through the appsettings.json file.
Additional filtering options are also available in the same configuration file.

The visit time is not configurable through the app settings, but hey, it is open source after all, you're free to set it in the code anyway.

## Error handling
A thread will stop if an error occurs (which will also be reported), but it won't interfere with other threads.

## Terms of use
This software is open-source and made available through the [MIT](https://opensource.org/licenses/MIT) license.
It is provided as-is without any warranty. This software is intended solely for testing purposes. 
The owner(s) and/or technical team(s) of the website subject to tests must be aware of and must have consented to the testing activity.

By using this software you agree to these terms of use.
