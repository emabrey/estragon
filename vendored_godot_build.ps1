#Requires -Version 6.0

git submodule update --init --recursive --depth 1

$PCK_SECRET_FILEPATH = "./PCK_SECRET.key"
if (-not (Test-Path $PCK_SECRET_FILEPATH)) {
    Read-Host "Enter OpenSSL Key" -AsSecureString | ConvertFrom-SecureString | Set-Content $PCK_SECRET_FILEPATH
}

$pckSecret = Get-Content $PCK_SECRET_FILEPATH | ConvertTo-SecureString
$env:SCRIPT_AES256_ENCRYPTION_KEY = ConvertFrom-SecureString -AsPlainText -SecureString $pckSecret

Push-Location ./vendored/godot

#scons target=editor --clean
#scons target=template_release --clean
#scons target=template_debug --clean


	python ./misc/scripts/install_angle.py
	python ./misc/scripts/install_accesskit.py

if ($IsWindows) 
{
	python ./misc/scripts/install_d3d12_sdk_windows.py
	scons target=editor platform=windows target=editor arch=x86_64
	./bin/godot.windows.editor.x86_64.estragon.mono --headless --generate-mono-glue ./modules/mono/glue
}
elseif ($IsLinux)
{
	scons target=editor platform=linux target=editor arch=x86_64
	./bin/godot.linux.editor.x86_64.estragon.mono --headless --generate-mono-glue ./modules/mono/glue
}


scons target=template_release
scons target=template_debug
New-Item -Path "$env:USERPROFILE\.godotnuget" -ItemType Directory
dotnet nuget add source "$env:USERPROFILE\.godotnuget" --name GodotNuget
python ./modules/mono/build_scripts/build_assemblies.py --godot-output-dir ./bin --push-nupkgs-local GodotNuget

Pop-Location
