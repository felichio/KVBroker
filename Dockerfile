FROM mcr.microsoft.com/dotnet/sdk:3.1

WORKDIR /app

COPY . .

ENV PATH="${PATH}:/app/app"

ENV random="felix"

RUN dotnet build -c Release -o app

WORKDIR /app/app