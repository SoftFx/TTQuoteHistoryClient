#' Initialize the CLR runtime and loads QuoteHistory host assembly
#'
tthInit <- function() {
  require(rClr)
  if(!require(stringi))
  {
    install.packages("stringi")
    require(stringi)
  }
  if(!require(data.table))
  {
    install.packages("data.table")
    require(data.table)
  }
  fileName <-system.file("data", "rTTQuoteHistory.dll", package="rTTQuoteHistory")
  clrLoadAssembly(fileName)
}
#' Connects to a TT server
#'
#' @param address Url of the server
#' @param port port
#' @export
tthConnect <- function(address = "tp.st.soft-fx.eu", port = 5020) {
  tthInit()
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'Connect', address, port)
}

#' Disconnect from a TT server
#'
#' @export
tthDisconnect <- function() {
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'Disconnect')
}