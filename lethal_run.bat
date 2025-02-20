@echo off

for /F "usebackq tokens=* delims=" %%x in ("Lethal Path.txt") do set "lethal=%%x"
powershell "Start-Process ""%lethal%/Lethal Company.exe"""