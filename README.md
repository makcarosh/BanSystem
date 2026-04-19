Configuration
```
<?xml version="1.0" encoding="utf-8"?>
<Configuration xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <webhookBan>https://discord.com/api/webhooks/...</webhookBan>
  <webhookUnBan>https://discord.com/api/webhooks/...</webhookUnBan>
  <webhookBanPlayerConnected>https://discord.com/api/webhooks/...</webhookBanPlayerConnected>
  <serverName>Server Name</serverName>
</Configuration>
```
Translate
```
<?xml version="1.0" encoding="utf-8"?>
<Translations xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Translation Id="banplayer" Value="**Игрок [{0}]({1}) был забанен!&#xA; Время бана: {2}&#xA; Кто забанил: [{3}]({4})&#xA;Сервер: {5}**" />
  <Translation Id="banplayer_connected" Value="**Забаненный игрок [{0}]({1}) пытался зайти &#xA;Сервер: {2}**" />
  <Translation Id="unbanplayer" Value="**[{0}]({1}) был разбанен!&#xA; Кто разбанил: [{2}]({3})&#xA;Сервер: {4}**" />
</Translations>
```
