#Requires -Version 6.0

git submodule update --init --recursive --depth 1

$PCK_SECRET_FILEPATH = "./godot.gdkey"
if (-not (Test-Path $PCK_SECRET_FILEPATH)) {
    Read-Host "Enter OpenSSL Key" -AsSecureString | ConvertFrom-SecureString | Set-Content $PCK_SECRET_FILEPATH
}

$pckSecret = Get-Content $PCK_SECRET_FILEPATH | ConvertTo-SecureString
$env:SCRIPT_AES256_ENCRYPTION_KEY = ConvertFrom-SecureString -AsPlainText -SecureString $pckSecret

$arch = if ([System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture -eq [System.Runtime.InteropServices.Architecture]::Arm64) { "arm64" } else { "x86_64" }

Push-Location ./vendored/godot -ErrorAction SilentlyContinue

# Uncomment to always run a clean rebuild; uncommented because during development we want
# the builds to be warm and fast instead of clean and reproducible like we would want in CI

#scons target=editor --clean
#scons target=template_release --clean
#scons target=template_debug --clean

# We removed accesskit and angle scripts calls here because Godot 4.6 no longer requires that

if ($IsWindows)
{
	# If windows we need to install the D3D script, but only if not already installed by prior run
	
    $d3dDepsFolder = "$env:LOCALAPPDATA\Godot\build_deps"
    $stampFile = "$d3dDepsFolder\installed_versions.txt"

    $scriptContent = Get-Content ./misc/scripts/install_d3d12_sdk_windows.py -Raw
    $mesaVersion    = [regex]::Match($scriptContent, 'mesa_version\s*=\s*"([^"]+)"').Groups[1].Value
    $pixVersion     = [regex]::Match($scriptContent, 'pix_version\s*=\s*"([^"]+)"').Groups[1].Value
    $agilityVersion = [regex]::Match($scriptContent, 'agility_sdk_version\s*=\s*"([^"]+)"').Groups[1].Value
    $expectedStamp  = "mesa=$mesaVersion pix=$pixVersion agility=$agilityVersion"

    $currentStamp = if (Test-Path $stampFile) { Get-Content $stampFile } else { "" }
    if ($currentStamp -ne $expectedStamp) {
        python ./misc/scripts/install_d3d12_sdk_windows.py
        Set-Content $stampFile $expectedStamp
    }

    $platform = "windows"
    $editorBin = "./bin/godot.windows.editor.$arch.estragon.mono.exe"
}
elseif ($IsLinux)
{
    $platform = "linux"
    $editorBin = "./bin/godot.linux.editor.$arch.estragon.mono"
}
elseif ($IsMacOS)
{
    $platform = "macos"
    $editorBin = "./bin/Godot.app/Contents/MacOS/Godot"
}

scons target=editor platform=$platform arch=$arch
& $editorBin --headless --generate-mono-glue ./modules/mono/glue

scons target=template_release platform=$platform arch=$arch
scons target=template_debug platform=$platform arch=$arch

New-Item -Path "$env:USERPROFILE\.godotnuget" -ItemType Directory -ErrorAction SilentlyContinue
dotnet nuget add source "$env:USERPROFILE\.godotnuget" --name GodotNuget -ErrorAction SilentlyContinue
python ./modules/mono/build_scripts/build_assemblies.py --godot-output-dir ./bin --push-nupkgs-local GodotNuget

Pop-Location -ErrorAction SilentlyContinue
