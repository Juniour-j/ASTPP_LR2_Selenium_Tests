@echo off
chcp 65001 >nul
cd /d D:\lab2
set BANK_HEADLESS=true
echo ===START=== > testrun-headless.txt
echo --- BUILD --- >> testrun-headless.txt
dotnet build >> testrun-headless.txt 2>&1
echo --- TEST (headless) --- >> testrun-headless.txt
dotnet test --no-build >> testrun-headless.txt 2>&1
echo ===DONE=== >> testrun-headless.txt
