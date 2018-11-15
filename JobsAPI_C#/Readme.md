# PrintOS PrintBeat Jobs C# sample

## General Information

Code was developed in Visual Studio 2015

Code is implemented with three executable files:
- RetrieveJobs.cs - standard call to collect jobs from your account
- RetrieveHistoricJobs.cs - will collect historic jog data from your account and keep it updated on a daily interval
- RetrieveParentJobs.cs - each time a new print run is found it will find the parent DFE job for that print run 
- HistoricJobsDateOfInterest - will collect historic job data from your account and parse the response set for jobs completed 3 days ago.

## How To Run / Program Information

- Import the solution into Visual Studio
- Configure the Key/Secret generated from your PrintOS account in the executable file you wish to run
- You can select which file you wish to run in Visual Studio by opening the Job Properties window in the Project menu and setting the Startup Object to object you want.
