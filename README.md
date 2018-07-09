# Crawler

This crawler was made as an exercice. It could certainly be better, and I'll work on it if I have the time.

Objectives contains :
--Make it a service so it can clean stop

--Make it able to remember where it stopped, so that we don't have to try again for each file/folder 

--Make it faster (original attempt was made on 35k files for a total of more than 300G, and after running for 3 days it had only downloaded a third...)


A few remarks if you plan to try this :

You should probably tweak the range in the DownloadFile method. When i started with all the 36k files at once, I had a dataloss resulting in file corruption (all pdf were created but none was readable ... ), hence the range. With a range of 10, it works. I'll try with a range of 100 soon.

Also, if you use another site than the one I've tried (a source of RPG books), you might want to check the structure of the document loaded by the Html Agility Pack : in my case, the links were all in a table so I add to use "/td//a" as a template, but you might have another one.

Finally, the CleanName method could be better as an extension of String, and all the code could be more configurable. But hey, this was an exercice for myself, not production code ;-)
