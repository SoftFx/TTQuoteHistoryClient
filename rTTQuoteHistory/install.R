install.packages("install.load")
library(install.load)
install_load("data.table", "stringi")

installRPackage <- function(url){
	package = basename(url)
    download.file(url, destfile = package)
	install.packages(package, repos = NULL, type = "source")
	file.remove(package)
}
if(!require(rClr) )
    installRPackage("https://github.com/SoftFx/rFdk-/raw/master/Lib/RClr/rClr_0.7-4.zip");
require(devtools)
install_github("SoftFx/TTQuoteHistoryClient",subdir = "rTTQuoteHistory/R")