install.packages("install.load")
library(install.load)
install_load("data.table", "stringi")

installRPackage <- function(url){
	package = basename(url)
    download.file(url, destfile = package)
	install.packages(package, repos = NULL, type = "source")
	file.remove(package)
}

installRPackage("https://github.com/SoftFx/rFdk-/raw/master/Lib/RClr/rClr_0.7-4.zip");
installRPackage("https://github.com/SoftFx/TTQuoteHistoryClient/raw/master/rTTQuoteHistory/dist/rTTQuoteHistory_1.0.0.zip");