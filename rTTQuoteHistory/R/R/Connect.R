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
tthConnect <- function(address = "tp.st.soft-fx.eu",login = "5",password = "123qwe!", port = 5020, name = "client") {
  tthInit()
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'Connect', name, address, port,login,password)
}


#' Disconnect from a TT server
#'
#' @export
tthDisconnect <- function() {
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'Disconnect')
}
