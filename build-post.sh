#!/usr/bin/bash

game_name="Death Must Die"
name="MoreQOD"
source="/home/marc/RiderProjects/MoreQOD/bin"
asset="/home/marc/dev/MoreQODAssets/Assets/AssetBundles/moreqodassets"
mods="/home/marc/Death Must Die/Mods"
gameid="2334730" 

moddir="${mods}/${name}"
mkdir -p "${moddir}"
dll="${name}.dll"
usage() {
    echo "Options: "
    echo "Debug"
    echo "Release"
    echo "ReleaseZip"
}
install() {
    if [ -n "${asset}" ]; then 
        echo "rm ${moddir}/${name}Assets"
        rm "${moddir}/${name}Assets" 
        cp "${asset}" "${moddir}/${name}Assets" 
        echo "${asset} -> ${moddir}/${name}Assets"
    fi 
    
    echo "rm ${mods}/${dll}"
    rm "${mods}/${dll}"
    cp "${source}" "${mods}/${dll}"
    echo "${source} -> ${mods}/${dll}"
}

if [ -z "$1" ]; then
    usage
    exit 1
fi

if [ "$1" = "Debug" ]; then
    source="${source}/Debug/${dll}"
else
    if [ "$1" = "Release" ]; then
        source="${source}/Release/${dll}"
    else
        if [ "$1" = "ReleaseZip" ]; then
            source="${source}/Release/${dll}"
            install
            targetzip="${name}.zip"
            cd "${mods}" || exit
            rm "${targetzip}"
            zip -r "${targetzip}" "${name}.dll" "${name}"
            exit 1
        else
            echo "$1"
            usage
            exit 1
        fi
    fi
fi

pkill -HUP "${game_name}" 
install
steam steam://rungameid/${gameid}
