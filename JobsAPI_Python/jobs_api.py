# © Copyright 2016 HP Development Company, L.P.
# SPDX-License-Identifier: MIT

#!/usr/bin/python

__author__ = 'dfe'

import requests, json, hmac, hashlib, datetime, base64, string, time

#access credentials
baseUrl = "https://printos.api.hp.com/jobs-service"		#use for production server account
#baseUrl = "https://stage.printos.api.hp.com/jobs-service"	#use for staging server account

key = ''  #The API Key generated from your PrintOS account for the Jobs API
secret = ''		#The API Secret generated from your PrintOS account for the Jobs API
context = 'historic'	#context that Jobs API queries will be made in (e.g. 'job', 'dfe', 'press', 'printrun' or 'historic')

marker = None   #last marker value from previous query_jobs
jobsList = []	#List of jobs (list of dictionaries)

#--------------------------------------------------------------#


'''
Queries Property Specification

Params:
  context - The context you want to get property specifications for (e.g. 'press', 'printrun', 'historic')
 
 Returns:
   Response data from API query (list of properties)
  
'''
def propertyspecs(context):
	print("Querying Property Specs for context " + context)
	path = '/jobs-sdk/propertyspecs'
	timestamp = datetime.datetime.utcnow().strftime('%Y-%m-%dT%H:%M:%SZ')
	headers = create_headers("GET", path, timestamp)
	url = baseUrl + path + '?context=' + context
	print("URL: " + url)
	return requests.get(url, headers=headers)

'''
Queries Jobs

Params:
  context - The context you want to get property specifications for (e.g. 'press', 'printrun', 'historic')
  startMarker - marker value to start the query from
  limit - the number of entries to be returned for this query (1000 - 10000)
  
 Returns:
  response - the respone data from the API query
'''
def query_jobs(context, startMarker, limit):
	print("Querying Jobs with context " + context)
	path = '/jobs-sdk/jobs/' + context
	timestamp = datetime.datetime.utcnow().strftime('%Y-%m-%dT%H:%M:%SZ')
	headers = create_headers("GET", path, timestamp)
	url = baseUrl + path
	if startMarker is not None:
		url = url + "?startMarker=" + str(startMarker)
	if limit is not None:
		if (url.find('?') != -1):
			url = url + "&limit=" + str(limit)
		else:
			url = url + "?limit=" + str(limit)
	print("URL: " + url)
	response = requests.get(url, headers=headers)
	return response

'''
Get Last Job Marker

Params:
  response - The response data from the latest query

Returns:
  last_marker - the marker value of the last entry in the response
  
'''
	
def get_last_marker(response):
	last_marker = None
	response_json = json.loads(response.text)
	if (len(response_json) > 0):
		last_marker = response_json[len(response_json)-1]['marker']
		print ("Last marker = " + str(last_marker) )
	return last_marker

'''
Creates the header using the key/secret which
allows you to make the API calls

Params:
  method - type of method (POST, GET, PUT, etc)
  path - api path (excluding the base url)
  timestamp - current time in specified format
'''
def create_headers(method, path, timestamp):
	string_to_sign = method + ' ' + path + timestamp
	local_secret = secret.encode('utf-8')
	string_to_sign = string_to_sign.encode('utf-8')
	signature = hmac.new(local_secret, string_to_sign, hashlib.sha256).hexdigest()
	auth = key + ':' + signature
	return {'content-type': 'application/json',
		'x-hp-hmac-authentication': auth,
		'x-hp-hmac-date': timestamp,
		'x-hp-hmac-algorithm' : 'SHA256'
	}

'''
Prints the data into a cleaner JSON format

Params:
  data - data that needs to be printed into JSON format
'''
def print_json(data):
	print(json.dumps(data.json(), indent=4, sort_keys=True))


'''
GET call

Params:
  path - api path (excluding the base url)
'''
def request_get(path):
	print("In request_get() function")
	timestamp = datetime.datetime.utcnow().strftime('%Y-%m-%dT%H:%M:%SZ')
	print(" Timestamp: ", timestamp)
	url = baseUrl + path
	headers = create_headers("GET", path, timestamp)
	result = requests.get(url, headers=headers)
	return result


'''
POST call

Params:
  path - api path (excluding the base url)
  data - data to be posted
'''
def request_post(path, data):
	print("In request_post() function")
	timestamp = datetime.datetime.utcnow().strftime('%Y-%m-%dT%H:%M:%SZ')
	print(" Timestamp: ", timestamp)
	url = baseUrl + path
	headers = create_headers("POST", path, timestamp)
	result = requests.post(url, data, headers=headers)
	return result

#--------------------------------------------------------------#

#Call the list of property specfications for the current context
#print_json(propertyspecs(context))

#Make an initial set of queries to capture all current jobs +
while True:
	response = query_jobs(context, marker, 10000)
	print_json(response)
	print (str(len(response.json())) + " entries were returned.")
	if (len(response.json()) > 0):
		jobsList = jobsList + response.json()
		marker = get_last_marker(response)
	if len(response.json()) < 10000:
		print ("All current jobs have been captured")
		break
	else :
		print ("Waiting 1 minute for next query")
		time.sleep(60)
		
#Once the initial set of data has been collected query at a given interval to get any new job data
while True:
	print ("Waiting 5 minutes for next query")
	time.sleep(300)
	response = query_jobs(context, marker, None)
	print_json(response)
	print (str(len(response.json())) + " entries were returned.")
	if (len(response.json()) > 0):
		jobsList = jobsList + response.json()
		marker = get_last_marker(response)
