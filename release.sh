#!/bin/bash
VERSION=`cat CommonAssemblyInfo.cs|grep AssemblyVersion | sed -nr 's/.*"(.*)".*/\1/p'`
cd Releases
cp Setup.exe Setup-$VERSION.exe

rsync -av * presetmagician.com@miau.mgmt.drachenkatze.org:/home/presetmagician.com/PresetMagicianSite/public/downloads/alpha/
cd ..
echo "CHANGEVERSION" >> CommonAssemblyInfo.cs
