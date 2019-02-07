#!/bin/bash
cd Releases
rsync -av * presetmagician.com@miau.mgmt.drachenkatze.org:/home/presetmagician.com/PresetMagicianSite/public/downloads/alpha/
cd ..
echo "CHANGEVERSION" >> PresetMagician/Properties/AssemblyInfo.cs
