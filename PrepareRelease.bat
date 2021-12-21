@ECHO OFF

set /p releaseVersion="Enter release version (example format - '1.1.1'): "
set releaseVersionHash=%releaseVersion:.=_%

set publishPath=bin\Release\net5.0-windows\publish
set releasePath=_release\v%releaseVersion%
set preparedPath=_release\v%releaseVersion%\prepared



:release-creation-started
ECHO Creation of the release (for version %releaseVersion%) has been started



if exist %releasePath% (
	rd /s /q %releasePath%
)

md %releasePath%
md %preparedPath%



:win-x64-FDD
ECHO win-x64 section



call :create-release-part "win-x64-FDD"



:win-x64-SCD
ECHO win-x64 standalone section



call :create-release-part "win-x64-SCD"



:win-x64-FDD-nosingle
ECHO win-x64 no-single-file section



call :create-release-part "win-x64-FDD-nosingle"



:win-x64-SCD-nosingle
ECHO win-x64 standalone no-single-file section



call :create-release-part "win-x64-SCD-nosingle"



:win-x86-FDD
ECHO win-x86 section



call :create-release-part "win-x86-FDD"



:win-x86-SCD
ECHO win-x86 standalone section



call :create-release-part "win-x86-SCD"



:win-x86-FDD-nosingle
ECHO win-x86 no-single-file section



call :create-release-part "win-x86-FDD-nosingle"



:win-x86-SCD-nosingle
ECHO win-x86 standalone no-single-file section



call :create-release-part "win-x86-SCD-nosingle"



:release-creation-ended
ECHO Creation of the release (for version %releaseVersion%) has been ended



exit



:create-release-part

if not exist "%publishPath%\%1" (
	exit /b
)

md "%preparedPath%\%1"
md "%preparedPath%\%1\hash"
md "%preparedPath%\%1\hash\sha512"

xcopy "%publishPath%\%1\*.exe" "%preparedPath%\%1\"
xcopy "%publishPath%\%1\*.pdb" "%preparedPath%\%1\"

xcopy "%publishPath%\%1\hash\sha512\*%releaseVersionHash%*.sha512" "%preparedPath%\%1\hash\sha512\"

exit /b
