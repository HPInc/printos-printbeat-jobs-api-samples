<?php
/**
 * Created by IntelliJ IDEA.
 * User: Muhannad
 * Date: 10/7/2018
 * Time: 2:03 PM
 */

namespace JobsAPI;
include "JobsAPI.php";

class RetrieveHistoricJobs
{
//Access Credentials
  private $baseUrl = "https://printos.api.hp.com/jobs-service";        //use for production account
  //private $baseUrl = ""https://stage.printos.api.hp.com/jobs-service"; 		//Use for staging account


  //#Access Credentials
  ////###########################################################################################
  //#
   private $key = null;     //insert your account PrintOS Jobs key here
   private $secret = null;  //insert your account PrintOS Jobs secret here
  //#
  //###########################################################################################



//private static HistoricJobComparator comparator = new HistoricJobComparator();
  private $jobs = array();
  private $jobsApi;


  //Set of properties to ask for, if there are any additional properties needed make sure to add them to HistoricJob.cs
  //Also be sure to review what the available set of properties are available in each context
  private $propertiesList = "deviceId,impressions,impressions1Color,impressions2Colors,impressionsNColors,impressionsType,inks,inkUnits,jobCompleteTime,jobName,jobSubmitTime,marker,substrates,substrateUnits";
  /*************************************************************************/
  /*Initially the startMarker is set ot the value of 0 so that we can collect all the data from six weeks back till now.*/
  /*After each call the startMarker value will be advanced so that only new jobs are returned.*/
  private $startMarker = 0;
  /*************************************************************************/
  private $limit = 10000;

  public function __construct()
  {
    $this->jobsApi = new JobsAPI($this->baseUrl, $this->key, $this->secret);
  }

  function GetJobSet($response)
  {
    return json_decode($response);
  }

  function GetLatestJobs()
  {
    //Retrieving the first set of 10000 jobs since the last known marker value
    $response = $this->jobsApi->retrieveJobs('historic', $this->limit, $this->propertiesList, null, $this->startMarker, "forward");
    $jobSet = $this->GetJobSet($response);
    if($jobSet == null)
      return;
    if(Count($jobSet) == 0)
      return;
    $this->jobs[] = $jobSet;
    $continueSync = (Count($jobSet) == $this->limit);
    // Update startMarker
    $this->startMarker = end($jobSet)->marker;
    while ($continueSync) {
      if (Count($jobSet) > 0) {
        $this->startMarker = end($jobSet)->marker; //Only update the marker if the last response returned any jobs
      }
      if (Count($jobSet) < $this->limit) {
        $continueSync = false; //No longer need to sync up jobs as the result set has returned less than 10000 jobs (either due to an error or success)
      } else {
        $response = $this->jobsApi->retrieveJobs('historic', $this->limit, $this->propertiesList, null, $this->startMarker, "forward");
        $jobSet = $this->GetJobSet($response);
        $this->jobs[] = $jobSet;
      }
    }
  }
}
