
FROM microsoft/aspnetcore:1
LABEL Name=darkages-lorule-server Version=0.0.1
ARG source=.
WORKDIR /app
EXPOSE 3000
COPY $source .
ENTRYPOINT dotnet darkages-lorule-server.dll
