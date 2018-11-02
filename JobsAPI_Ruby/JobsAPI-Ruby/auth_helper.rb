module Jobs
  require 'openssl'
  class HMACAuthHelper


    def initialize(key, secret)
      @key = key
      @secret = secret
    end

    def createHmacAuth(method, path, time)
      combined = method + ' ' + path + time
      digest = OpenSSL::Digest.new('sha1')
      hmac = OpenSSL::HMAC.hexdigest(digest, @secret, combined)
      return @key + ':' + hmac
    end

    def CreateHmacHeaders(method, path)
      timestamp = Time.now.getutc.iso8601
      signed = createHmacAuth(method, path, timestamp)
      headers = {:content_type => :json, 'x-hp-hmac-authentication' => signed, 'x-hp-hmac-date' => timestamp, 'x-hp-hmac-algorithm' => 'SHA1'}
      return headers
    end
  end
end

