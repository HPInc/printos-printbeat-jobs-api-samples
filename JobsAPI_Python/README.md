# Python

## General Information

Code was written in Python 3.7.3
Uses the "requests" module so that will need to be installed in order to run the code ``` > pip install requests```

It also uses json, hmac, hashlib, datetime, base64, string, and time modules as well. If they aren't found, you may need to install these as well.

## What this program does

This application will query the jobs API to collect all current data, then it will send a similar query every 5 minutes to collect updated jobs information

There is also a call to collect jobs properties for an individual context

## How To Run / Program Information

Before you can run the code, you need to provide the Key/Secret. There are two baseUrls provided. Uncomment the one that your Key/Secret was created/provided in (this will be the production URL for most users)

Run on the command line using ```python jobs_api.py```