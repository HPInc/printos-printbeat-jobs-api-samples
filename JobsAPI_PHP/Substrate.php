<?php
/**
 * Created by IntelliJ IDEA.
 * User: Muhannad
 * Date: 10/7/2018
 * Time: 2:17 PM
 */

namespace JobsAPI;


class SubstrateCount
{
  public $name;
  public $amountUsed;

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

class Substrate
{
  public $counts = array();
}
