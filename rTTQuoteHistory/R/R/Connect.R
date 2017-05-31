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
#' @param login account you login
#' @param password password of the account
#' @param name name of client
#' @export
tthConnect <- function(address = "",login = "",password = "", port = 5020, name = paste0("clinet_",Sys.getpid())) {
  tthInit()
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'Connect', name, address, port,login,password)
  print(paste0(name," was created"))
}


#' Disconnect from a TT server
#'
#' @export
tthDisconnect <- function() {
  rClr::clrCallStatic('rTTQuoteHistory.TTQuoteHistoryHost', 'Disconnect')
}
