#!/bin/bash
git checkout beta
VERSION=`cat CommonAssemblyInfo.cs|grep AssemblyVersion | sed -nr 's/.*"(.*)".*/\1/p'`
cd Releases
cp Setup.exe PresetMagicianSetup-$VERSION.exe
rm PresetMagicianSetup.exe
mv Setup.exe PresetMagicianSetup.exe

rsync -av * presetmagician.com@miau.mgmt.drachenkatze.org:/home/presetmagician.com/PresetMagicianSite/public/downloads/beta/
cd ..
echo "CHANGEVERSION" >> CommonAssemblyInfo.cs
