#!/usr/bin/bash

name="MoreQOD"
source="/home/marc/RiderProjects/MoreQOD/bin"
asset="/home/marc/dev/MoreQODAssets/Assets/AssetBundles/moreqodassets"
mods="/home/marc/Death\ Must\ Die/Mods"

moddir="${mods}/${name}"
mkdir -p "${moddir}"
dll="${name}.dll"
usage() {
    echo "Options: "
    echo "Debug"
    echo "Release"
    echo "z  (Zip)"
}
if [ -z "$1" ]; then
    usage
    exit 1
fi

if [ $1 = "Debug" ]; then
    source="${source}/Debug/${dll}"
else
    if [ $1 = "Release" ]; then
        source="${source}/Release/${dll}"
    else
        if [ $1 = "z" ]; then
            rm "${name}.zip"
            zip -r "${name}.zip" "${mods}/${name}.dll" "${moddir}"
            exit 1
        else
            echo $1
            usage
            exit 1
        fi
    fi
fi

# ps aux | grep -i SteamChildMonit | grep -v grep
pkill -HUP "Death Must Die"
# ps aux | grep -i "Death Must Die" | awk '{print $2}' | xargs kill -9
# echo "${moddir}/${name}Assets"
cp -f "${asset}" "${moddir}/${name}Assets"
cp -f "${source}" "${mods}"
steam steam://rungameid/2334730

# cp -rf ~/MoreQualityOfDeath/Assets/AssetBundles/morequalityofdeath MoreQualityOfDeath/MoreQualityOfDeathAssets
# cp -rf ~/RiderProjects/MoreQualityOfDeath/MoreQualityOfDeath/bin/Debug/MoreQualityOfDeath.dll .
# steam steam://rungameid/2334730
# cp -rf ~/MoreQualityOfDeath/Assets/AssetBundles/morequalityofdeath ~/Death\ Must\ Die/Mods/MoreQualityOfDeath/MoreQualityOfDeathAssets && cp -rf ~/RiderProjects/MoreQualityOfDeath/MoreQualityOfDeath/bin/Release/MoreQualityOfDeath.dll /home/marc/Death\ Must\ Die/Mods && steam steam://rungameid/2334730
