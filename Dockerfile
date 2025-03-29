# Specifies the base image for the first build stage 
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set the working directory in the container
WORKDIR /src

COPY ["MemberOnly.Api/MemberOnly.Api.csproj", "MemberOnly.Api/"]
RUN dotnet restore "MemberOnly.Api/MemberOnly.Api.csproj"

COPY . .
WORKDIR "/src/MemberOnly.Api"
RUN dotnet build "MemberOnly.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MemberOnly.Api.csproj" -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /publish .
EXPOSE 8080
EXPOSE 443

# # loads the appsetting.Docker.json file
# ENV ASPNETCORE_ENVIRONMENT=Docker
#-----
# docker run -e ASPNETCORE_ENVIRONMENT=Development myapp
# if not set, default to Production
ENV ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-Production}
# run the application 
ENTRYPOINT ["dotnet", "MemberOnly.Api.dll"]