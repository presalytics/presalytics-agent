#! /bin/bash
set -e

FLDR=$HOME/.presalytics

echo "Creating folder to house agent files: $FLDR"

rm -rf $FLDR
mkdir $FLDR
cd $FLDR

echo "Downloading latest release of the Preaslytics CLI..."
echo ""
wget https://github.com/presalytics/presalytics-agent/releases/download/v0.1.1-linux/presalytics-agent.tar.gz
echo ""
echo "Extracting tar ball..."
echo ""
tar -xzvf presalytics-agent.tar.gz
echo ""
echo "Adding executable to path and creating aliases..."

touch ~/.bashrc

PATH_COMMAND='PATH=$PATH:'${FLDR}'/bin/Release/net5.0/linux-x64/publish'
grep -qxF "$PATH_COMMAND" "$HOME/.bashrc" || echo "$PATH_COMMAND" >> "$HOME/.bashrc"

ALIAS_COMMAND='alias presalytics='$FLDR'/bin/Release/net5.0/linux-x64/publish/presalytics-cli'
grep -qxF "$ALIAS_COMMAND" "$HOME/.bashrc" || echo "$ALIAS_COMMAND" >> "$HOME/.bashrc"

echo "Removing temporary files..."
rm -rf presalytics-agent.tar.gz
echo ""
echo 'Presalytics CLI installed.  Open a new terminal and use command "presalytics --help" for more information'