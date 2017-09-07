#' Gets the ticks as requested
#'
#' @param symbol Symbol looked
#' @param timestamp timestamp
#' @param count Count of ticks
#' @export
tthTickRequest <- function(symbol = "", timestamp= "", count= "") {
  hResult = rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'TickRequest', timestamp,count,symbol,FALSE)
  if(hResult == 0){tGetTickDataFrame()}
}

#' Checking end of tick stream
#' 
#' @export
tthIsEndOfTickStream<-function(){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'IsEndOfTickStream')
}

#' Checking for new params
#' 
#' @export
CheckNewTickParams<-function(symbol = "", startTime= "", endTime= "",level2 = FALSE){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'CheckNewTickParams', startTime,endTime,symbol,level2)
}

#' Gets the ticks as requested
#'
#' @param symbol Symbol looked
#' @param startTime Start of the time intervals
#' @param endTime End of the time interval
#' @export
tthStreamTickRequest <- function(symbol = "", startTime= "", endTime= "") {
  if(tthIsEndOfTickStream() == TRUE || CheckNewTickParams(symbol,startTime,endTime,FALSE) == TRUE) rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'FillTickRange', startTime,endTime,symbol,FALSE)
  hResult = rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost','StreamTickRequest',FALSE)
  if(hResult == 0){tGetTickDataFrame()}
}

#' Get tick table
tGetTickDataFrame<-function()
{
  DateTime = tGetTickDateTime()
  Bid = tGetTickBid()
  Ask = tGetTickAsk()
  tthClear()
  data.table(DateTime,Bid,Ask)
}
#' Get tick field
tGetTickDateTime<-function(){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'GetTickDate')
}
#' Get tick field
tGetTickBid<-function(){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'GetTickBid')
}
#' Get tick field
tGetTickAsk<-function(){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'GetTickAsk')
}
