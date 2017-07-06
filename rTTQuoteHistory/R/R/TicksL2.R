#' Gets the ticks level 2 as requested
#'
#' @param symbol Symbol looked
#' @param timestamp timestamp
#' @param count Count of ticks
#' @export
tthTickL2Request <- function(symbol = "", timestamp= "", count= "") {
  hResult = rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'TickRequest', timestamp,count,symbol,TRUE)
  if(hResult == 0){tGetTickL2DataFrame()}
}

#' Gets the ticks as requested
#'
#' @param symbol Symbol looked
#' @param startTime Start of the time intervals
#' @param endTime Count of ticks
#' @export
tthFileTickL2Request <- function(symbol = "", startTime= "", endTime= "") {
  hResult = rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'FileTickRequest', startTime,endTime,symbol,TRUE)
  if(hResult == 0){tGetTickL2DataFrame()}
}

#' Get tick table
tGetTickL2DataFrame<-function()
{
  DateTime = tGetTickL2DateTime()
  VolumeBid = tGetTickL2VolumeBid()
  VolumeAsk = tGetTickL2VolumeAsk()
  PriceBid = tGetTickL2PriceBid()
  PriceAsk = tGetTickL2PriceAsk()
  Level = tGetTickL2Level()
  tthClear()
  data.table(DateTime,VolumeBid,VolumeAsk,PriceBid,PriceAsk,Level)
}
#' Get tick field
tGetTickL2VolumeBid<-function(){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'GetTickL2VolumeBid')
}
#' Get tick field
tGetTickL2VolumeAsk<-function(){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'GetTickL2VolumeAsk')
}
#' Get tick field
tGetTickL2PriceBid<-function(){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'GetTickL2PriceBid')
}

#' Get tick field
tGetTickL2PriceAsk<-function(){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'GetTickL2PriceAsk')
}

#' Get tick field
tGetTickL2Level<-function(){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'GetTickL2Level')
}

tGetTickL2DateTime<-function(){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'GetTickL2DateTime')
}