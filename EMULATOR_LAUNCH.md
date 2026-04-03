# Android Emulator Launch Instructions

## Launch the emulator

```powershell
$env:ANDROID_SDK_ROOT = "${env:ProgramFiles(x86)}\Android\android-sdk"
& "${env:ProgramFiles(x86)}\Android\android-sdk\emulator\emulator.exe" -avd EuchreTablet -gpu auto
```

## Build and install the MAUI app

```powershell
cd C:\Users\Matthew\source\repos\Euchre\MAUIEuchre
dotnet build -t:Install -f net9.0-android
```

## Install the APK manually (if needed)

```powershell
$apk = Get-ChildItem -Path bin\Debug\net9.0-android -Filter "*.apk" -Recurse | Select-Object -First 1
& "${env:ProgramFiles(x86)}\Android\android-sdk\platform-tools\adb.exe" install -r $apk.FullName
```

## Launch the app on the emulator

```powershell
& "${env:ProgramFiles(x86)}\Android\android-sdk\platform-tools\adb.exe" shell am start -n com.wheresthatcat.euchre/crc64b4495fe61bb05dfb.MainActivity
```

## Check connected devices

```powershell
& "${env:ProgramFiles(x86)}\Android\android-sdk\platform-tools\adb.exe" devices
```

## Notes

- JDK 17 is at: `C:\Program Files\Microsoft\jdk-17.0.18.8-hotspot`
- AVD name: `EuchreTablet` (Pixel Tablet profile, API 35, x86_64)
- App ID: `com.wheresthatcat.euchre`
- The activity class hash may change if you rebuild — use `adb shell cmd package resolve-activity --brief com.wheresthatcat.euchre` to find it
