# 빌드 단계
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 전체 복사
COPY . .

# API 프로젝트 복원 + 게시
RUN dotnet restore "Kairos/Kairos.Api.csproj"
RUN dotnet publish "Kairos/Kairos.Api.csproj" -c Release -o /app/publish

# 실행 단계
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Kairos.Api.dll"]
