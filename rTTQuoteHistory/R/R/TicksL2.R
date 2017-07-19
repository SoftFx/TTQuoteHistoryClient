#' Gets the ticks level 2 as requested
#'
#' @param symbol Symbol looked
#' @param timestamp timestamp
#' @param count Count of ticks
#' @param depth Depth of ticks
#' @export
tthTickL2Request <- function(symbol = "", timestamp= "", count= "",depth = 0) {
  hResult = rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'TickRequest', timestamp,count,symbol,TRUE)
  if(hResult == 0){tGetTickL2DataFrame(depth)}
}

#' Gets the ticks level 2 as requested
#'
#' @param symbol Symbol looked
#' @param startTime Start of the time intervals
#' @param endTime Count of ticks
#' @param depth Depth of ticks
#' @export
tthFileTickL2Request <- function(symbol = "", startTime= "", endTime= "",depth = 0) {
  hResult = rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'FileTickRequest', startTime,endTime,symbol,TRUE)
  if(hResult == 0){tGetTickL2DataFrame(depth)}
}

#' Get level 2 tick table
tGetTickL2DataFrame<-function(depth)
{
  DateTime = tGetTickL2DateTime(depth)
  VolumeBid = tGetTickL2VolumeBid(depth)
  VolumeAsk = tGetTickL2VolumeAsk(depth)
  PriceBid = tGetTickL2PriceBid(depth)
  PriceAsk = tGetTickL2PriceAsk(depth)
  Level = tGetTickL2Level(depth)
  tthClear()
  data.table(DateTime,VolumeBid,VolumeAsk,PriceBid,PriceAsk,Level)
}
#' Get level 2 tick field
tGetTickL2VolumeBid<-function(depth){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'GetTickL2VolumeBid',depth)
}
#' Get level 2 tick field
tGetTickL2VolumeAsk<-function(depth){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'GetTickL2VolumeAsk',depth)
}
#' Get level 2 tick field
tGetTickL2PriceBid<-function(depth){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'GetTickL2PriceBid',depth)
}

#' Get level 2 tick field
tGetTickL2PriceAsk<-function(depth){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'GetTickL2PriceAsk',depth)
}

#' Get level 2 tick field
tGetTickL2Level<-function(depth){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'GetTickL2Level',depth)
}

#' Get level 2 tick field
tGetTickL2DateTime<-function(depth){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'GetTickL2DateTime',depth)
}