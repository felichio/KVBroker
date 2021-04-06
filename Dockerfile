FROM mcr.microsoft.com/dotnet/sdk:3.1

WORKDIR /tripleAppSetup

RUN git clone https://felichio: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@github.com/felichio/DataGenerator.git

RUN git clone https://felichio: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@github.com/felichio/KVBroker.git

RUN git clone https://felichio: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@github.com/felichio/KVServer.git

RUN cd DataGenerator && dotnet build -c Release -o app

RUN cd KVBroker && dotnet build -c Release -o app

RUN cd KVServer && dotnet build -c Release -o app

RUN apt-get update

RUN apt-get install -y vim

WORKDIR /tripleAppSetup/playGround

RUN ln -s /tripleAppSetup/DataGenerator/app/createData /usr/local/sbin/createData

RUN ln -s /tripleAppSetup/KVBroker/app/kvBroker /usr/local/sbin/kvBroker

RUN ln -s /tripleAppSetup/KVServer/app/kvServer /usr/local/sbin/kvServer

RUN cp ../KVServer/initservers.sh .

RUN cp ../DataGenerator/keyFile.txt .

RUN cp ../KVBroker/serverFile.txt .

# ENV PATH="${PATH}:/tripleAppSetup/DataGenerator/app:/tripleAppSetup/KVBroker/app:/tripleAppSetup/KVServer/app"
