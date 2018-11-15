<?php
/**
 * Created by IntelliJ IDEA.
 * User: Muhannad
 * Date: 10/7/2018
 * Time: 2:49 PM
 */

namespace JobsAPI;


class JobsAPI
{
  private $baseUrl;
  private $key;
  private $secret;

  public function __construct($baseUrl, $key, $secret)
  {
    $this->baseUrl = $baseUrl;
    $this->key = $key;
    $this->secret = $secret;
  }

  function createHmacAuth($method, $path, $timestamp)
  {

    $str = $method . ' ' . $path . $timestamp;
    $hash = hash_hmac('sha1', $str, $this->secret);
    return $this->key . ':' . $hash;
  }

  public function RetrieveJobs($context, $limit, $properties, $devices, $startMarker, $direction)
  {
    $path = "/jobs-sdk/jobs/" . $context;

    $querySring = array(
      'properties' => $properties,
      'limit' => $limit,
      'devices' => $devices,
      'startMarker' => $startMarker,
      'direction' => $direction
    );

    return $this->getRequest($path, http_build_query($querySring));

  }

  function getRequest($path, $queryString)
  {


    $t = microtime(true);
    $micro = sprintf("%03d", ($t - floor($t)) * 1000);
    $time = gmdate('Y-m-d\TH:i:s.', $t) . $micro . 'Z';
    $auth = $this->createHmacAuth('GET', $path, $time);
    $options = array(
      'http' => array(
        'header' => "Content-Type: application/json\r\n" .
          "x-hp-hmac-date: " . $time . "\r\n" .
          "x-hp-hmac-authentication: " . $auth . "\r\n",
        'method' => 'GET',
		//'ignore_errors' => true,   //ignoring errors will let you capture the response JSON for invalid calls
        //'proxy' => 'web-proxy:8080', // If you are operating behind a proxy, uncomment proxy and request_fulluri lines and specify the proxy address and port.
        //'request_fulluri' => true,
      ),

    );
    $context = stream_context_create($options);
     $fullPath =$this->baseUrl . $path;
     if($queryString!=null)
       $fullPath = $fullPath . '?' . $queryString;
    print("Sending GET to URL: " . $fullPath . "\n");
    $response = file_get_contents($fullPath, false, $context);
    $data = gzdecode($response);
    print_r($this->parseHeaders($http_response_header));
    return $data;
  }

  function parseHeaders($headers)
  {
    $head = array();
    foreach ($headers as $k => $v) {
      $t = explode(':', $v, 2);
      if (isset($t[1]))
        $head[trim($t[0])] = trim($t[1]);
      else {
        $head[] = $v;
        if (preg_match("#HTTP/[0-9\.]+\s+([0-9]+)#", $v, $out))
          $head['reponse_code'] = intval($out[1]);
      }
    }
    return $head;
  }

  function RetrievePropertySpecs($context)
  {
    $path = "/jobs-sdk/propertyspecs";
    $contextParam = ($context != null) ? "?context=" . $context : "";

    $fullPath = $this->baseUrl . $path . $contextParam;

    return $this->getRequest($fullPath, null);
  }
}
