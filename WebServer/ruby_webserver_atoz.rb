# -*- coding: utf-8 -*-
require 'webrick'

$document_root = './'

class Ranking
  attr_accessor :time
  attr_accessor :name
  def initialize()
    @time = 60.0
    @name = "NoName"
  end
end
$ranking = [Ranking.new, Ranking.new, Ranking.new, Ranking.new]
p $ranking
$mutex = Mutex.new

class MyServer < WEBrick::HTTPServlet::AbstractServlet

  def do_GET(req, res)

    p "do_GET start"
    $mutex.synchronize{
      data=Array.new
      # postでは、配列として３番目を用意しているが、Rankingは３位までしか表示しない
      # ので、３つ分だけ送る
      data.push($ranking[0].time)
      data.push($ranking[0].name)
      data.push($ranking[1].time)
      data.push($ranking[1].name)
      data.push($ranking[2].time)
      data.push($ranking[2].name)
      format = "fA16fA16fA16"
      res.body = data.pack(format)
    }
  end

  def do_POST(req, res)

    p "do_POST start"
    # わざと、最後部に挿入して、timeメンバでソートさせる
    $mutex.synchronize{
      $ranking[3].time = req.query["time"].unpack("f")[0].to_f
      $ranking[3].name = req.query["name"]
      $ranking = $ranking.sort{|a,b|
        p a
        p b
        a.time <=> b.time
      }      
    }
    p $ranking
  end
end

server = WEBrick::HTTPServer.new({
  :DocumentRoot => $document_root,
  :BindAddress => '0.0.0.0',
  :Port => 10080
})

server.mount('/', MyServer)

['INT', 'TERM'].each {|signal|
  Signal.trap(signal){ server.shutdown }
}

server.start
