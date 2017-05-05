
if(!require(roxygen2)){
	install.packages("roxygen2", repos="http://cran.us.r-project.org")
  library(roxygen2)
}

if(!require(devtools)){
  install.packages("devtools", repos="http://cran.us.r-project.org")
  library(devtools)
}

if(!require(data.table)){
  install.packages("data.table", repos="http://cran.us.r-project.org")
  library(data.table)
}

getwd()
if(!require(rClr)){
  install.packages("Lib/RClr/rClr_0.7-4.zip", repos = NULL, type = "win.binary")
  library(rClr)
}
setwd("R")

devtools::document(roclets=c('rd', 'collate', 'namespace'))
packPath <- devtools::build(binary = TRUE, args = c('--preclean'), path="..//dist")
