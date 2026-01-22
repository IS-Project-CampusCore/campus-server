@echo off
set "DOCKER_USER=bocadoru"
set "COMPOSE_FILE=docker-compose.dist.yml"

echo ========================================================
echo   CAMPUS CORE - AUTOMATED DEPLOYMENT SCRIPT
echo ========================================================
echo.

:: --- STEP 1: STOPPING SERVER ---
echo [1/5] Stopping running containers...
docker compose -f %COMPOSE_FILE% down

:: --- STEP 2: CLEANUP ---
echo.
echo [2/5] Removing old application images...
docker rmi %DOCKER_USER%/campus-users:latest ^
           %DOCKER_USER%/campus-chat:latest ^
           %DOCKER_USER%/campus-http:latest ^
           %DOCKER_USER%/campus-email:latest ^
           %DOCKER_USER%/campus-excel:latest ^
           %DOCKER_USER%/campus-notification:latest ^
           %DOCKER_USER%/campus-announcements:latest ^
           %DOCKER_USER%/campus-grades:latest ^
           %DOCKER_USER%/campus-schedule:latest ^
           %DOCKER_USER%/campus-campus:latest 2>NUL

:: --- STEP 3: BUILDING ---
echo.
echo [3/5] Building all services from scratch...
echo.

echo    - Building Users...
docker build -t %DOCKER_USER%/campus-users:latest -f ./users/Dockerfile .

echo    - Building Chat...
docker build -t %DOCKER_USER%/campus-chat:latest -f ./chat/Dockerfile .

echo    - Building HTTP Gateway...
docker build -t %DOCKER_USER%/campus-http:latest -f ./http/Dockerfile .

echo    - Building Email...
docker build -t %DOCKER_USER%/campus-email:latest -f ./email/Dockerfile .

echo    - Building Excel...
docker build -t %DOCKER_USER%/campus-excel:latest -f ./excel/Dockerfile .

echo    - Building Notification...
docker build -t %DOCKER_USER%/campus-notification:latest -f ./notification/Dockerfile .

echo    - Building Announcements...
docker build -t %DOCKER_USER%/campus-announcements:latest -f ./announcements/Dockerfile .

echo    - Building Grades...
docker build -t %DOCKER_USER%/campus-grades:latest -f ./grades/Dockerfile .

echo    - Building Schedule...
docker build -t %DOCKER_USER%/campus-schedule:latest -f ./schedule/Dockerfile .

echo    - Building Campus...
docker build -t %DOCKER_USER%/campus-campus:latest -f ./campus/Dockerfile .

:: --- STEP 4: PUSH TO CLOUD (OPTIONAL) ---
echo.
echo ========================================================
set /p PUSH_CHOICE="Do you want to PUSH these images to Docker Hub now? (y/n): "
if /i "%PUSH_CHOICE%"=="y" (
    echo.
    echo [4/5] Pushing images to Docker Hub...
    docker push %DOCKER_USER%/campus-users:latest
    docker push %DOCKER_USER%/campus-chat:latest
    docker push %DOCKER_USER%/campus-http:latest
    docker push %DOCKER_USER%/campus-email:latest
    docker push %DOCKER_USER%/campus-excel:latest
    docker push %DOCKER_USER%/campus-notification:latest
    docker push %DOCKER_USER%/campus-announcements:latest
    docker push %DOCKER_USER%/campus-grades:latest
    docker push %DOCKER_USER%/campus-schedule:latest
    docker push %DOCKER_USER%/campus-campus:latest
    echo Push complete!
) else (
    echo Skipping push. Running locally only.
)

:: --- STEP 5: RESTART SERVER ---
echo.
echo [5/5] Starting Server...
docker compose -f %COMPOSE_FILE% up
docker compose -f %COMPOSE_FILE% logs -f http users chat email excel notification announcements grades schedule campus

echo.
echo ========================================================
echo   DEPLOYMENT FINISHED SUCCESSFULLY!
echo ========================================================
echo Server is running.
echo.
pause