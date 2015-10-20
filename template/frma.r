#predefine_start

outputdir<-"E:/shengquanhu/projects/20150427_chenxi_GEO_AE_breastcancer/datasets/GSE2109"
inputfile<-"E:/shengquanhu/projects/20150427_chenxi_GEO_AE_breastcancer/datasets/GSE2109/celfiles.tsv"

#predefine_end

library(frma)
library(affy)

setwd(outputdir)

celfiles<-read.table(inputfile, stringsAsFactors = F)

frma_cel<-function(celfile){
  cat(celfile, "\n")
  target<-paste0(celfile, ".tsv")
  cat(target, "\n")
  if(!file.exists(target)){
    data<-read.affybatch(filenames=celfile)
    #normalized<-frma(data, summarize="random_effect")
    normalized<-frma(data)
    ndata<-exprs(normalized)
    finaldata<-data.frame(probe=rownames(ndata), value=ndata[,1])
    write.table(finaldata,file=target, sep="\t", row.names=F, quote=F)
  }
}

apply(celfiles, 1, function(x){
  frma_cel(x[1])
})
