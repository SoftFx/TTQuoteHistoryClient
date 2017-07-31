library(foreach)
library(doParallel)
library(uuid)

Init<-function(){
}
Deinit<-function(){
}
Payload<-function(vectorVariable, variable){
    sapply(vectorVariable, function(iterator) {
        
    })
  1
}

RunTest <-function(vectorVariable, testInputParameter, clusterCount, libraries, fInit, fDeinit, fPayload)
{
    cl <- makeCluster(clusterCount)
    registerDoParallel(cl)
    
    totalElapsedTime<-system.time({
        
        r<-foreach( clientId = 1:clusterCount, .packages = c(libraries, "uuid"),
                 .export = c("vectorVariable","testInputParameter", "fInit", "fDeinit", "fPayload"), .combine = c) %dopar%
        {
            fInit()
            result<-fPayload(vectorVariable, testInputParameter)
            fDeinit()
            result
        }
    })
    
    stopCluster(cl)
	r/totalElapsedTime[[3]]
}
RunTest(c("EURUSD", "EURCHF"), 100, 5, c("lubridate"), Init, Deinit, Payload)
