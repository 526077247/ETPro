echo please enter index/n:
set /p index=
::echo %index%
mkdir %cd%\Unity%index%
mklink/J %cd%\Unity%index%\Assets %cd%\Unity\Assets
mklink/J %cd%\Unity%index%\Library %cd%\Unity\Library
mklink/J %cd%\Unity%index%\Packages %cd%\Unity\Packages
mklink/J %cd%\Unity%index%\PackageSources %cd%\Unity\PackageSources
mklink/J %cd%\Unity%index%\ProjectSettings %cd%\Unity\ProjectSettings

pause
