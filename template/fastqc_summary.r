#predefine_start

outputdir<-"H:/shengquanhu/projects/Jennifer/20150406_bojana_dnaseq_selectedgenes/fastqc/result"
inputfile<-"summary.tsv"
outputfile<-"summary.png"

#predefine_end

setwd(outputdir)

library(ggplot2)

fp<-read.table(inputfile, header=T, sep="\t", stringsAsFactor=F)
fp$QCResult<-factor(fp$QCResult, levels=c("PASS","WARN","FAIL"))
fp$File<-factor(fp$File, levels=unique(fp$File))
fp$Category<-factor(fp$Category, levels=unique(fp$Category))

width=max(2000, 50 * length(unique(fp$File)))
png(file=outputfile, height=1300, width=width, res=300)
g<-ggplot(fp, aes(File, Category))+  
  geom_tile(data=fp, aes(fill=QCResult), color="white") +
  scale_fill_manual(values=c("light green", "skyblue", "red")) +
  theme(axis.text.x = element_text(angle=90, vjust=1, size=11, hjust=1, face="bold"),
        axis.text.y = element_text(size=11, face="bold")) + 
  coord_equal()
print(g)
dev.off()
