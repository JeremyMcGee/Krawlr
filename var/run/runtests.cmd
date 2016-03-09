pushd
cd ..\..\src\Krawlr.Console\bin\Debug
krawlr -u=http://www.hiscoxcollection.com/default.aspx 
taskkill /f /im firefox.exe
popd
