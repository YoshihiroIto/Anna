dotnet clean -c Debug
dotnet clean -c Release

dotnet test -c Debug
dotnet test -c Release

pause

