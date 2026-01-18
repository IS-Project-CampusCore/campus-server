@echo off
echo Starting CampusCore Server...
echo Please make sure Docker Desktop is running!
docker compose -f docker-compose.dist.yml up
docker compose -f docker-compose.dist.yml logs -f http users chat email excel notification announcements grades schedule campus
echo.
echo Server is running! You can close this window.
pause