library(rFdk)
library(rTTQuoteHistory)
library(foreach)
library(doParallel)
library(uuid)
library(lubridate)
library(ggplot2)

Init<-function(){
  tthConnect("tp.st.soft-fx.eu", login = "5",password = "123qwe!", name=UUIDgenerate())
}


Deinit<-function(){tthDisconnect()}
Payload<-function(symbols, period){
  sapply(symbols, function(symbol) {
    startTime = Sys.Date() - years(2)
    period = toString(period)
    endTime = Sys.Date()
    if(period == "Day") {
      endTime = startTime + days(1)
    }
    if(period == "Week") {
      endTime = startTime + weeks(1)
    }
    if(period == "HalfMonth") {
      endTime = startTime + weeks(2)
    }
    if(period == "Month") {
      endTime = startTime + months(1)
    }
    if(period == "QuarterYear") {
      endTime = startTime + months(3)
    }
    if(period == "HalfYear") {
      endTime = startTime + months(6)
    }
    if(period == "Year") {
      endTime = startTime + years(1)
    }
    if(period == "TwoYears") {
      endTime = startTime + years(2)
    }
    d<-tthStreamBarRequest(symbol, startTime, endTime)
    count = nrow(d)
    return(count)
    if( is.null(d)) 
      stop("Bar Request returned error.") 
  })
}

RunTest <-function(vectorVariable1, seqVariable1, clusterCount, fInit, fDeinit, fPayload)
{
  
  cl <- makeCluster(clusterCount)
  registerDoParallel(cl)
  totalElapsedTime<-system.time({
    r<-foreach( clientId = 1:clusterCount, .packages = c("rTTQuoteHistory", "uuid", "lubridate"),
                .export = c("seqVariable1","vectorVariable1", "fInit", "fDeinit", "fPayload"), .combine = c) %dopar%
                {
                  fInit()
                  results <<- fPayload(vectorVariable1, seqVariable1)
                  fDeinit()
                  results
                }
    
  })
  stopCluster(cl)
  r = sum(r[1:length(vectorVariable1)])
  return(c(r,totalElapsedTime[[3]]))
} 
ttConnect()
symbols<-ttConf.Symbol()
Periods = c("Day","Week", "HalfMonth","Month","QuarterYear", "HalfYear", "Year", "TwoYears")
exp<-as.data.table(expand.grid(symbolsCount=2^(1), barsCount=Periods[1:length(Periods)], clusterCount=2^(0:4)))

result<-mapply(RunTest, lapply(exp$symbolsCount, function(count) symbols[1:count, name]), 
               exp$barsCount, exp$clusterCount, MoreArgs=list(fInit=Init, fDeinit=Deinit, fPayload=Payload))
exp[, elapsed:=result[2,]]
exp[,totalBars:= result[1,]]
ggplot(exp, aes(x=totalBars, y=totalBars/elapsed, group=clusterCount, colour=clusterCount)) + geom_point() + geom_line()
