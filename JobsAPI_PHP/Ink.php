<?php
/**
 * Created by IntelliJ IDEA.
 * User: Muhannad
 * Date: 10/7/2018
 * Time: 2:28 PM
 */

namespace JobsAPI;

use JsonSerializable;

class Ink implements JsonSerializable
{
  public $counts = array();
  public function jsonSerialize() {
    return [
      'InkCount' => [
        'name' => $this->name,
        'amountUsed' => $this->amountUsed,
      ]
    ];
  }

}

class InkCount implements JsonSerializable
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
  public function jsonSerialize() {
    return [
      'InkCount' => [
      'name' => $this->name,
      'amountUsed' => $this->amountUsed,
    ]
    ];
  }
}
