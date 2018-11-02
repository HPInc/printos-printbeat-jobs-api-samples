module Jobs
  class Substrate
    def initialize
      @counts = Array.new
    end

    attr_accessor :counts

    def add(substrateCount)
      @counts << substrateCount
    end
  end
  class SubstrateCount
    def initialize (name, amountUsed)
      @name = name
      @amountUsed = amountUsed
    end

    attr_accessor :name
    :amountUsed

  end
end


