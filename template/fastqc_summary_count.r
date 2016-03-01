#predefine_start

outputdir<-"H:/shengquanhu/projects/Jennifer/20150406_bojana_dnaseq_selectedgenes/fastqc/result"
inputfile<-"summary.tsv"
outputfile<-"summary.png"

#predefine_end

setwd(outputdir)

library(ggplot2)

fp<-read.table(inputfile, header=T, sep="\t")

width=max(2000, 50 * nrow(fp))
png(file=outputfile, height=1300, width=width, res=300)
g<-ggplot(fp, aes(x=Sample, y=Reads))+ geom_bar(stat="identity", width=.5)+
  theme(axis.text.x = element_text(angle=90, vjust=0.5, size=11, hjust=0, face="bold"),
        axis.text.y = element_text(size=11, face="bold"))
print(g)
dev.off()
