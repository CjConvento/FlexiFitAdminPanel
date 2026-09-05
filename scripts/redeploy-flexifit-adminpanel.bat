@echo off
:: redeploy-flexifit-adminpanel.bat
:: Double-click this file to redeploy the Admin Panel to IIS.
:: This must be run as Administrator (right-click > Run as administrator).

powershell -NoExit -ExecutionPolicy Bypass -File "%~dp0redeploy-flexifit-adminpanel.ps1"