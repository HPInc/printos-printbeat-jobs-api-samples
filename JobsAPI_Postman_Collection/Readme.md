# Postman Collection
## General Information
This is an exported collection for the Postman application (exported as a v2 Collection). It allows you to import a working set of API calls to test the functionality of the current APIs.

The Postman application can be downloaded here: https://www.getpostman.com/

## How To Run / Program Information
1. Import the "Jobs API.postman_collection.json" file in the main Import dialog of Postman.
2. Import the "Jobs API Environment.postman_environment.json" file through the Manage Environments dialog in the upper right settings menu
3. In the Manage Environments dialog edit the environment you just imported and replace the "key" and "secret" values with the key and secret from the PrintOS account you are using. 
4. Make sure the SmsHost enviroment variable is checked for the stack you are using (Production vs Staging)
5. Click "Update" to save changes.
6. Select the Environment you just imported in the environment drop-down menu.
7. In the Jobs API collection in the left pane select the API call you wish to make then click the "Send" button to send the API call.

## About the Postman collection
- A Pre-request Script inside the collection uses the CryptoJS library to dynamically generate the authentication HMAC.
- This Pre-request Script may also set some necessary environment variables which are used in the HTTP Headers section for each call.
- The response field can be used to capture response JSON messages from the Jobs API calls.
**NOTE:** Some of the API calls are dependant on certain environment variables being set to valid values in your environment.  Make sure they are defined correctly if errors occur.