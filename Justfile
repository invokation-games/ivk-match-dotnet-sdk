default: build

build:
    dotnet build src/Invokation.Match.Sdk/Invokation.Match.Sdk.csproj --configuration Release

build-all:
    dotnet build --configuration Release

test:
    dotnet test --configuration Release

test-net6:
    dotnet build tests/Invokation.Match.Sdk.Tests.Net6 --configuration Release
    mise x dotnet@6 -- dotnet test tests/Invokation.Match.Sdk.Tests.Net6 --configuration Release --no-build

test-net8:
    dotnet build tests/Invokation.Match.Sdk.Tests.Net8 --configuration Release
    mise x dotnet@8 -- dotnet test tests/Invokation.Match.Sdk.Tests.Net8 --configuration Release --no-build

test-net10:
    dotnet test tests/Invokation.Match.Sdk.Tests.Net10 --configuration Release

pack:
    dotnet pack src/Invokation.Match.Sdk/Invokation.Match.Sdk.csproj --configuration Release -o ./nupkg

format:
    dotnet format

clean:
    dotnet clean
    rm -rf ./nupkg

restore:
    dotnet restore

run-example:
    dotnet run --project src/Invokation.Match.Sdk.Example

ci: restore build-all test

publish: pack
    dotnet nuget push ./nupkg/*.nupkg --api-key $NUGET_API_KEY --source https://api.nuget.org/v3/index.json --skip-duplicate

buf-lint:
    buf lint protos

buf-breaking:
    buf breaking protos --against ".git#branch=main,subdir=protos"
