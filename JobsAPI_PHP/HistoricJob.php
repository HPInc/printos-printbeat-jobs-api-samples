<?php
/**
 * Created by IntelliJ IDEA.
 * User: Muhannad
 * Date: 10/7/2018
 * Time: 2:10 PM
 */

namespace JobsAPI;
include "class.Ink.php";
include "class.Substrate.php";

class HistoricJob
{
  public $deviceId;
  public $impressions;
  public $impressions1Color;
  public $impressions2Colors;
  public $impressionsNColors;
  public $impressionsType;
  public $inks;
  public $inkUnits;
  public $jobCompleteTime;
  public $jobName;
  public $jobSubmitTime;
  public $marker;
  public $substrates;
  public $substrateUnits;

  public function __construct()
  {
    $this->inks = new Ink();
    $this->substrates = new Substrate();

  }

  public function __get($property)
  {
    if (property_exists($this, $property)) {
      return $this->$property;
    }
  }

  public function __set($property, $value)
  {
    if (property_exists($this, $property)) {
      $this->$property = $value;
    }

    return $this;
  }
}
