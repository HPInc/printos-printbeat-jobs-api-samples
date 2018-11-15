<?php
/**
 * Created by IntelliJ IDEA.
 * User: Muhannad
 * Date: 10/8/2018
 * Time: 11:34 AM
 */

/// In this sample we are showing the basics of establishing a local view of historic jobs in an account and updating them every hour or 24 hours depending on the data retrieved.
/// The example shows the pulling of the historical jobs and sets a timer until the next time to pull
/// Once you have this view of your jobs, you can analyze the data as you see fit.
/// In this example only the current startMarker and total amount of jobs in-memory are outputed. You may want to persist your view in a formal database.
///
namespace JobsAPI;
include "RetrieveHistoricJobs.php";
$historicData = new RetrieveHistoricJobs();
//Based on the status of the last call, determine what interval the timer will need to be to next trigger the retrieval of the latest jobs
//An error will usually be due to hitting the limit of permitted calls each hour, so we will set the interval to being an hour (in milliseconds).
//
//If it was successful we can assume that all the data has been retrieved and the data set is up to date, so the interval will be set to 24 hours
//because the data in the historic context is only updated once a day.
while (true)
{
  //Add new data to the existing set.
  $historicData->GetLatestJobs();
  //Wait 24 hours before the next update.
  sleep(86400);
}
