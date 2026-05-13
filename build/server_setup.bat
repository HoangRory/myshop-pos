@echo off
setlocal
cls
title Trinh tai file tu GitHub - Lucifer-VN

echo =====================================================
echo    Dang tai file tu GitHub (Repository: myshop-pos)
echo =====================================================

:: Link Raw truc tiep tu GitHub cua ban
:: Luu y: 'tree' duoc thay bang 'raw' de curl tai dung file binary
set "URL_EXE=https://github.com/HoangRory/myshop-pos/raw/Backend-Handler/build/Server/1.0.3/server_setup.exe"
set "URL_MSI=https://github.com/HoangRory/myshop-pos/raw/Backend-Handler/build/Server/1.0.3/server_setup.msi"

echo [+] Dang tai server_setup.exe (20MB)...
curl -L -o "server_setup.exe" "%URL_EXE%"

if %ERRORLEVEL% NEQ 0 (
    echo [LOI] Khong the tai server_setup.exe. Kiem tra lai ket noi.
    pause
    exit /b
)

echo [+] Dang tai server_setup.msi...
curl -L -o "server_setup.msi" "%URL_MSI%"

if %ERRORLEVEL% NEQ 0 (
    echo [LOI] Khong the tai server_setup.msi.
    pause
    exit /b
)

echo =====================================================
echo    Tai xong! Dang thuc thi setup.exe...
echo =====================================================

:: Kiem tra file ton tai truoc khi chay
if exist "server_setup.exe" (
    start "" "server_setup.exe"
)

echo.
echo Hoan tat! Moi file da duoc tai ve thu muc hien tai.
pause