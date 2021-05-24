('Debug', 'Release') | %{ $config=$_; dir *.sln | %{ dotnet build -c $config $_ }}
dir SimpleCommandLineParser\bin\Release\*.nupkg | %{nuget push $_.FullName -source nuget.org}