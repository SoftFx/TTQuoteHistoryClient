#' Gets the list of supported symbols
#' @export
tthGetSupportedSymbols <- function() {
  Symbols = rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'GetSupportesSymbols')
  Symbols
}

#' Gets the list of supported periodicities
#' @export
tthGetSupportedPeriodicities <- function() {
  Periodicities = rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'GetSupportedPeriodicities')
  Periodicities
}