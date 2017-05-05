#' Gets the bars as requested
#'
#' @param symbol Symbol looked
#' @param startTime Start of the time intervals
#' @param count Count of ticks
#' @export
tthBarRequest <- function(symbol = "", endTime= "", count= "", periodicity="M1",priceType = "Bid") {
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'BarRequest', endTime, count, symbol, periodicity, priceType)
  tGetBarDataFrame()
}
#' Get Bar table
tGetBarDataFrame<-function()
{
  DateTime = tGetBarDateTime()
  Open = tGetBarOpen()
  High = tGetBarHigh()
  Low = tGetBarLow()
  Close = tGetBarClose()
  Volume = tGetBarVolume()
  data.table(DateTime,Open,High,Low,Close,Volume)
}
#' Get Bar field
tGetBarDateTime<-function(){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'GetBarTime')
}
#' Get Bar field
tGetBarOpen<-function(){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'GetBarOpen')
}
#' Get Bar field
tGetBarHigh<-function(){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'GetBarHigh')
}
#' Get Bar field
tGetBarLow<-function(){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'GetBarLow')
}
#' Get Bar field
tGetBarClose<-function(){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'GetBarClose')
}
#' Get Bar field
tGetBarVolume<-function(){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'GetBarVolume')
}
