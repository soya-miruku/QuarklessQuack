@echo off
cd ..
FOR /f "tokens=*" %%i IN ('docker ps -q') DO docker stop %%i
FOR /f "tokens=*" %%i IN ('docker ps -q') DO docker rm %%i
docker volume prune -f
docker network rm localnet
timeout 15