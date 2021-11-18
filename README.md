# Presalytics Agent

## Packaging for Release

### Linux

From the `cli` directory, run commands

````bash
dotnet publish --configuration Release -r linux-x64
tar -czvf presalytics-agent.tar.gz bin/Release/net5.0/linux-x64/publish
````


