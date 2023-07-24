FROM mcr.microsoft.com/dotnet/aspnet:6.0

ARG DEBIAN_FRONTEND=noninteractive
RUN apt-get update && apt-get install -y curl libcap2-bin libpoppler102 poppler-utils texlive-fonts-extra texlive-xetex

WORKDIR /opt/cloudsume-services
COPY . ./

RUN useradd -d /opt/cloudsume-services -M -r -s /usr/sbin/nologin aspnet
RUN setcap 'cap_net_bind_service=+ep' /usr/share/dotnet/dotnet

USER aspnet

# override settings
ENV Logging__Console__FormatterName=Simple

ENTRYPOINT ["dotnet", "Cloudsume.Services.dll"]
