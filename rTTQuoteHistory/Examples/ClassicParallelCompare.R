library(rFdk)
library(rTTQuoteHistory)
library(foreach)
library(doParallel)
library(uuid)
library(lubridate)
library(ggplot2)

Init<-function(){
    tthConnect("@host", login = "5",password = "123qwe!", name=UUIDgenerate())
}
Deinit<-function(){tthDisconnect()}
Payload<-function(symbols, barsCount){
    sapply(symbols, function(symbol) {
        
        d<-tthBarRequest(symbol, now("UTC"), barsCount)
        if( is.null(d))
            stop("Bar Request returned error.")
        if( nrow(d) != barsCount )
            stop(paste(symbol, ": wrong number of bars. Expected ", barsCount, ", but received ", nrow(d)))
    })
}


RunTest <-function(vectorVariable1, seqVariable1, clusterCount, fInit, fDeinit, fPayload)
{
    cl <- makeCluster(clusterCount)
    registerDoParallel(cl)
    
    totalElapsedTime<-system.time({
        
        foreach( clientId = 1:clusterCount, .packages = c("rTTQuoteHistory", "uuid", "lubridate"),
                 .export = c("seqVariable1","vectorVariable1", "fInit", "fDeinit", "fPayload")) %dopar%
        {
            fInit()
            fPayload(vectorVariable1, seqVariable1)
            fDeinit()
        }
    })
    
    stopCluster(cl)
    totalElapsedTime[[3]]
}

ttConnect()
symbols<-ttConf.Symbol()

exp<-as.data.table(expand.grid(symbolsCount=2^(2), barsCount=2^(c(16:18)), clusterCount=2^(0:1)))

result<-mapply(RunTest, lapply(exp$symbolsCount, function(count) symbols[1:count, name]), 
               exp$barsCount, exp$clusterCount, MoreArgs=list(fInit=Init, fDeinit=Deinit, fPayload=Payload))
exp[,elapsed:=result]
exp[,totalBars:=barsCount*symbolsCount]

ggplot(exp, aes(x=totalBars, y=totalBars/elapsed, group=clusterCount, colour=clusterCount)) + geom_point() + geom_line()