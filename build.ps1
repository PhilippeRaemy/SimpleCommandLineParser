('Debug', 'Release') | %{ $config=$_; dir *.sln | %{ dotnet build -c $config $_ }}
dir .\bin\Release\*.nupkg | %{nuget push $_.FullName -source nuget}