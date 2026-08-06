# expected build context: root of project, same dir like appsettings.json

# this image ships the necessary dotnet sdk so i can build the dotnet project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build 

# create dir inside container and switch to it
WORKDIR /App/

# restore dependencies before copying project files so rebuilds with same deps are faster
COPY *.csproj /App/
RUN dotnet restore

COPY . /App/

# build binary from source, skip restore since we already did that
RUN dotnet publish --no-restore -o out/

# restart from a fresh image since we don't need the sdk anymore, just the binary.
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /Prod/

# copy from the build stage image into current dir
COPY --from=build /App/out/ .

ENTRYPOINT ["dotnet", "JwtAuthImplementation.dll"]
