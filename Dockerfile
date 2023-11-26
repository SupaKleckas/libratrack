# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR ./

# copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore -r /p:PublishReadyToRun=true

# copy everything else and build app
COPY . ./
RUN dotnet publish -c release -o /app -r --self-contained true --no-restore /p:PublishReadyToRun=true /p:PublishSingleFile=true

# final stage/image
FROM mcr.microsoft.com/dotnet/runtime-deps:7.0-alpine-amd64
WORKDIR ./
COPY --from=build ./ .
ENTRYPOINT ["./LibraTrack"]

ENV \
	DOTNET_SYSTEM_GLOBILAZATION_INVARIANT=false \
	LC_ALL=en_US.UTF8 \
	LANG=en_US.UTF8
RUN apk add --no-cache \
	icu-data-full \
	icu-libs