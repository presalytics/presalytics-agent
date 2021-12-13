# Presalytics Agent

## Installation

### Linux

````bash
wget -O - https://raw.githubusercontent.com/presalytics/presalytics-agent/master/scripts/linux-installer.sh | bash
````

## Packaging for Release

### Linux

From the `cli` directory, run commands

````bash
dotnet publish --configuration Release -r linux-x64
tar -czvf presalytics-agent.tar.gz bin/Release/net5.0/linux-x64/publish
````


