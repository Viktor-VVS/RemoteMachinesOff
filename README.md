# RemoteMachinesOff
клиент-серверное приложение позволяющее выключать/перезагружать удаленные компьютеры.
(ServerforVirtual)Сервер висит в системе , открывает порт и ждет входящих коннектов 
и ждет одной из 2-х возможных команд от клиента(ClientforVirtual) , это перезагрузка или shutdown.
На компьютере с которого будет выполнятся управление нужно запустить ClientforVirtual
рядом с ним должен находится файл settings.ini  в нем настройки такого плана 
#servers=Nata-192.168.5.4:19800,Misha-192.168.5.5:19801,Viktor-192.168.24.128:4455,Pavel-192.168.24.128:4456;
где указываются адреса и порты  которые будет проверять наш клиент и куда будет отсылать команды на выкл/перезагрузку.
Чтобы проверить состояние удаленных машин нажмем CheckActiveMachines в списке появятся все машины которые в данный момент активны и ждут команду.
Далее правой кнопкой мыши на нужном сервере и в контекстном меню выбираем ShutDown или Reboot систему.
   На удаленных машинах запускаем ServerforVirtual и нажимаем Listen. Все машина ждет команд. 


