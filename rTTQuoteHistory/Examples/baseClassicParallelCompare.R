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

RunTest <-function(testInputVector, testInputParameter, clusterCount, libraries, fInit, fDeinit, fPayload)
{
    cl <- makeCluster(clusterCount)
    registerDoParallel(cl)
    
    totalElapsedTime<-system.time({
        
        r<-foreach( clientId = 1:clusterCount, .packages = c(libraries, "uuid"),
                 .export = c("testInputVector","testInputParameter", "fInit", "fDeinit", "fPayload"), .combine = c) %dopar%
        {
            fInit()
            result<-fPayload(testInputVector, testInputParameter)
            fDeinit()
            result
        }
    })
    
    stopCluster(cl)
	r/totalElapsedTime[[3]]
}
RunTest(c("EURUSD", "EURCHF"), 100, 5, c("lubridate"), Init, Deinit, Payload)
