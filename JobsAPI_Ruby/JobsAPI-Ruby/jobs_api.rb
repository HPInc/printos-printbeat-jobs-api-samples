module Jobs
  require 'json'
  require 'net/http'
  require 'uri'
  require './auth_helper'
  require 'time'

  class JobsApi
    def initialize(baseUrl, key, secret)
      @baseUrl = baseUrl
      @hmacAuth = HMACAuthHelper.new(key, secret)
    end
    def hash_to_query(hash)
      return hash.map{|k,v| "#{k}=#{v}"}.join("&")
    end
    def retrieveJobs(context, limit, properties, devices, startMarker, direction)
      path = '/jobs-sdk/jobs/' + context
      timestamp = Time.now.iso8601
      auth = @hmacAuth.createHmacAuth('GET', path, timestamp)
      queryHash = Hash.new
      if limit != nil
        queryHash['limit'] = limit
      end
      if properties != nil
        queryHash['properties'] = properties
      end
      if startMarker != nil
        queryHash['startmarker'] = startMarker
      end
      if direction != nil
        queryHash['direction'] = direction
      end
      if devices != nil
        queryHash['devices'] = devices
      end
      fullPath = @baseUrl + path
      uri = (URI).parse(fullPath)
      uri.query = hash_to_query(queryHash)

      request = Net::HTTP::Get.new(uri)
      request.add_field("x-hp-hmac-authentication", auth)
      request.add_field("x-hp-hmac-date", timestamp)

      response = Net::HTTP.start(uri.host, uri.port,
                                 :use_ssl => uri.scheme == 'https',
                                 :verify_mode => OpenSSL::SSL::VERIFY_NONE) do |http|
        http.request(request)
        # If operating behind a proxy, replace with:
        # response = Net::HTTP::Proxy(proxy_addr, proxy_port).start(uri.host....
      end
      return response

    end

    def retrievePropertySpecs (context)
      path = '/jobs-sdk/propertyspecs'
      contextParam = '?' + context
      fullpath = @baseUrl + path + contextParam
      timestamp = Time.now.utc.iso8601
      auth = @hmacAuth.createHmacAuth('GET', path, timestamp)

      uri = URI(fullpath)

      request = Net::HTTP::Get.new(uri)
      request.add_field("x-hp-hmac-authentication", auth)
      request.add_field("x-hp-hmac-date", timestamp)

      response = Net::HTTP.start(uri.host, uri.port,
                                 :use_ssl => uri.scheme == 'https',
                                 :verify_mode => OpenSSL::SSL::VERIFY_NONE) do |http|
        http.request(request)
      end
      return response
    end

  end
end

