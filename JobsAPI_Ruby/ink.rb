module Jobs
class Ink
 def initialize
   @counts = Array.new
 end
 attr_accessor :counts

  def add(inkCount)
    @counts << inkCount
  end
end
  class InkCount
   def initialize (name,amountUsed)
     @name = name
     @amountUsed = amountUsed
   end
    attr_accessor :name
   :amountUsed

  end
end
