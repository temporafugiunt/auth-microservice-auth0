FROM microsoft/dotnet:2.2-sdk AS build
RUN mkdir -p /usr/src/app
RUN mkdir /usr/src/build
RUN mkdir /usr/src/publish

WORKDIR /usr/src/app

COPY auth-microservice-auth0.csproj /usr/src/app
RUN dotnet restore

COPY . /usr/src/app
RUN dotnet build  -c Release -o /usr/src/build

RUN dotnet publish -c Release -o /usr/src/publish

FROM microsoft/dotnet:2.2-aspnetcore-runtime
RUN mkdir -p /app
WORKDIR /app
COPY --from=build /usr/src/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "auth-microservice-auth0.dll"]