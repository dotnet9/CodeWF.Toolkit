@echo off
setlocal

set "ROOT=%~dp0"
set "SOLUTION=%ROOT%CodeWF.Toolkit.slnx"
set "CONFIGURATION=%~1"
if "%CONFIGURATION%"=="" set "CONFIGURATION=Release"

set "PACKAGE_DIR=%ROOT%artifacts\packages"

set "PROJECTS=%ROOT%src\Lang\CodeWF.Toolkit.Lang\CodeWF.Toolkit.Lang.csproj;%ROOT%src\Core\CodeWF.Toolkit.Core\CodeWF.Toolkit.Core.csproj;%ROOT%src\Files\CodeWF.Toolkit.Files\CodeWF.Toolkit.Files.csproj;%ROOT%src\Image\CodeWF.Toolkit.Image\CodeWF.Toolkit.Image.csproj;%ROOT%src\Logging\CodeWF.Toolkit.Logging\CodeWF.Toolkit.Logging.csproj;%ROOT%src\Logging\CodeWF.Toolkit.Logging.Microsoft\CodeWF.Toolkit.Logging.Microsoft.csproj;%ROOT%src\EventBus\CodeWF.Toolkit.EventBus\CodeWF.Toolkit.EventBus.csproj;%ROOT%src\EventBus\CodeWF.Toolkit.EventBus.Prism\CodeWF.Toolkit.EventBus.Prism.csproj;%ROOT%src\EventBus\CodeWF.Toolkit.EventBus.AspNetCore\CodeWF.Toolkit.EventBus.AspNetCore.csproj;%ROOT%src\EventBus\CodeWF.Toolkit.EventBus.Socket\CodeWF.Toolkit.EventBus.Socket.csproj;%ROOT%src\Net\CodeWF.Toolkit.NetWeaver\CodeWF.Toolkit.NetWeaver.csproj;%ROOT%src\Net\CodeWF.Toolkit.NetWrapper\CodeWF.Toolkit.NetWrapper.csproj"

echo [CodeWF.Toolkit] Restore solution...
dotnet restore "%SOLUTION%"
if errorlevel 1 goto :failed

if not exist "%PACKAGE_DIR%" mkdir "%PACKAGE_DIR%"
del /q "%PACKAGE_DIR%\*.nupkg" 2>nul
del /q "%PACKAGE_DIR%\*.snupkg" 2>nul

echo [CodeWF.Toolkit] Build %CONFIGURATION% solution...
dotnet build "%SOLUTION%" -c "%CONFIGURATION%" --no-restore -nologo
if errorlevel 1 goto :failed

echo [CodeWF.Toolkit] Pack NuGet packages...
for %%P in ("%PROJECTS:;=" "%") do (
    dotnet pack %%P -c "%CONFIGURATION%" --no-build --no-restore -nologo -o "%PACKAGE_DIR%"
    if errorlevel 1 goto :failed
)

for /r "%PACKAGE_DIR%" %%F in (*.pdb) do del /q "%%F" 2>nul

if not exist "%PACKAGE_DIR%\*.nupkg" goto :failed

echo.
echo [CodeWF.Toolkit] Packages:
dir /b "%PACKAGE_DIR%\*.nupkg"
echo.
echo [CodeWF.Toolkit] Done. Output: %PACKAGE_DIR%
exit /b 0

:failed
echo.
echo [CodeWF.Toolkit] Pack failed.
exit /b 1
