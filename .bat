cd "Letsgobiking\PROXY\bin\Debug"

powershell Start-Process -FilePath "TPREST.exe" -Verb RunAs

cd "..\..\..\RoutingServer\bin\Debug"

powershell Start-Process -FilePath "RoutingServer.exe" -Verb RunAs

activemq start