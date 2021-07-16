@ECHO OFF

ECHO win-x64 section

dotnet publish "Memenim.csproj" -c "Release" -p:PublishProfile="netcore3.1 win-x64 standalone.pubxml"
dotnet publish "Memenim.csproj" -c "Release" -p:PublishProfile="netcore3.1 win-x64.pubxml"

ECHO win-x64 no-single-file section

dotnet publish "Memenim.csproj" -c "Release" -p:PublishProfile="netcore3.1 win-x64 standalone no-single-file.pubxml"
dotnet publish "Memenim.csproj" -c "Release" -p:PublishProfile="netcore3.1 win-x64 no-single-file.pubxml"

ECHO win-x86 section

dotnet publish "Memenim.csproj" -c "Release" -p:PublishProfile="netcore3.1 win-x86 standalone.pubxml"
dotnet publish "Memenim.csproj" -c "Release" -p:PublishProfile="netcore3.1 win-x86.pubxml"

ECHO win-x86 no-single-file section

dotnet publish "Memenim.csproj" -c "Release" -p:PublishProfile="netcore3.1 win-x86 standalone no-single-file.pubxml"
dotnet publish "Memenim.csproj" -c "Release" -p:PublishProfile="netcore3.1 win-x86 no-single-file.pubxml"
