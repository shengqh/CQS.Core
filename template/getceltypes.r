#predefine_start

outputdir<-"H:/shengquanhu/projects/chenxi/20141223_chenxi_GEO_AE_breastcancer/datasets/E-MTAB-365"
inputfile<-"H:/shengquanhu/projects/chenxi/20141223_chenxi_GEO_AE_breastcancer/datasets/E-MTAB-365/celfiles.tsv"
outputfile<-"H:/shengquanhu/projects/chenxi/20141223_chenxi_GEO_AE_breastcancer/datasets/E-MTAB-365/celtypes.tsv"

#predefine_end

library(affy)

setwd(outputdir)
celfiles<-read.table(inputfile)

cdfnames<-apply(celfiles,1,function(x){
  cat(x, "\n")
  whatcdf(x)
})
df<-data.frame(file=celfiles, cdf=cdfnames)
colnames(df)<-c("file","cdf")
write.table(df, outputfile, sep="\t", row.names=F, quote = F)
