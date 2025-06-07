@echo off
  setlocal

  :: Change to the donut directory
  cd /d "%~dp0..\external\donut"
  if %ERRORLEVEL% NEQ 0 (
      echo ERROR: Could not find donut directory at %~dp0..\external\donut
      exit /b 1
  )

  :: Try to find Visual Studio installation
  if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\VC\Auxiliary\Build\vcvarsall.bat" (
      call "%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\VC\Auxiliary\Build\vcvarsall.bat" amd64
      goto :build
  )

  if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Professional\VC\Auxiliary\Build\vcvarsall.bat" (
      call "%ProgramFiles%\Microsoft Visual Studio\2022\Professional\VC\Auxiliary\Build\vcvarsall.bat" amd64
      goto :build
  )

  if exist "%ProgramFiles%\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvarsall.bat" (
      call "%ProgramFiles%\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvarsall.bat" amd64
      goto :build
  )

  echo ERROR: Could not find Visual Studio 2022 installation
  exit /b 1

  :build
  echo Building Donut...
  nmake -f Makefile.msvc
  if %ERRORLEVEL% NEQ 0 (
      echo ERROR: nmake failed with exit code %ERRORLEVEL%
      exit /b %ERRORLEVEL%
  )

  echo Donut build completed successfully