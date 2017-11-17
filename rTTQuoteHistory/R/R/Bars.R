#' Gets the bars as requested
#'
#' @param symbol Symbol looked
#' @param timestamp timestamp
#' @param count Count of bars
#' @param periodicity periodicity
#' @param priceType priceType
#' @export
tthBarRequest <- function(symbol = "", timestamp= "", count= "", periodicity="M1",priceType = "Bid") {
  hResult = rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'BarRequest', timestamp, count, symbol, periodicity, priceType)
  if(hResult == 0){tGetBarDataFrame()}
}
#' Checking end of bar stream
#' 
#' @export
tthIsEndOfBarStream<-function(){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'IsEndOfBarStream')
}

#' Checking for new params
#' 
#' @export
CheckNewBarParams<-function(symbol = "", startTime= "", endTime= "", periodicity="M1",priceType = "Bid"){
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'CheckNewBarParams', startTime, endTime, symbol, periodicity, priceType)
}

#' Gets the bars as requested
#'
#' @param symbol Symbol looked
#' @param startTime Start of the time intervals
#' @param endTime End of the time interval
#' @param periodicity periodicity
#' @param priceType priceType
#' @export
tthStreamBarRequest <- function(symbol = "", startTime= "", endTime= "", periodicity="M1",priceType = "Bid") {
  if(tthIsEndOfBarStream() == TRUE || CheckNewBarParams(symbol, startTime, endTime, periodicity, priceType) == TRUE) rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'FillBarRange', startTime, endTime, symbol, periodicity, priceType)
  hResult = rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost','StreamBarRequest')
  if(hResult == 0){tGetBarDataFrame()}
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
  tthClear()
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
