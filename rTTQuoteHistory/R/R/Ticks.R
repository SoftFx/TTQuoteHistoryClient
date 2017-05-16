#' Gets the ticks as requested
#'
#' @param symbol Symbol looked
#' @param endTime Start of the time intervals
#' @param count Count of ticks
#' @export
tthTickRequest <- function(symbol = "", endTime= "", count= "") {
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'TickRequest', endTime,count,symbol,FALSE)
  tGetTickDataFrame()
}
#' Get tick table
tGetTickDataFrame<-function()
{
  DateTime = tGetTickDateTime()
  Bid = tGetTickBid()
  Ask = tGetTickAsk()
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