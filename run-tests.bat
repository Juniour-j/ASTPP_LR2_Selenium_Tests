@echo off
chcp 65001 >nul
cd /d D:\lab2
echo ===START=== > testrun.txt
echo --- dotnet version --- >> testrun.txt
dotnet --version >> testrun.txt 2>&1
echo --- BUILD --- >> testrun.txt
dotnet build >> testrun.txt 2>&1
echo --- TEST --- >> testrun.txt
dotnet test --no-build >> testrun.txt 2>&1
echo ===DONE=== >> testrun.txt
