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

#' Gets the ticks as requested
#'
#' @param symbol Symbol looked
#' @param startTime Start of the time intervals
#' @param endTime Count of ticks
#' @export
tthFileTickRequest <- function(symbol = "", startTime= "", endTime= "") {
  hResult = rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'FileTickRequest', startTime,endTime,symbol,FALSE)
  if(hResult == 0){tGetTickDataFrame()}
}

#' Gets next ticks after limit reaching
#' @export
tthNextFileTickRequest <- function() {
  hResult = rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'NextFileTickRequest')
  if(hResult == 0){tGetTickDataFrame()}
  if(hResult == -1) {print("You haven't requested ticks already")}
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
