# Variables
$AppName = "netspotify-downloader"
$ServerUser = "servernitaxx"
$ServerHost = "192.168.1.207"
$ServerPath = "/home/servernitaxx/docker-projects/netspotify-downloader/"

Write-Host "🚀 Construyendo imagen Docker..."
docker build -t $AppName:latest -f NetSpotifyDownloaderApi/Dockerfile .

Write-Host "📦 Exportando imagen a TAR..."
$tarFile = "$AppName.tar"
docker save "${AppName}:latest" -o $tarFile

Write-Host "📤 Subiendo al servidor..."
scp $tarFile "${ServerUser}@${ServerHost}:${ServerPath}/"

Write-Host "🔄 Desplegando en el servidor..."
ssh $ServerUser@$ServerHost @"
    cd $ServerPath
    docker load -i $tarFile
    docker compose down
    docker compose up -d
    rm $tarFile
"@

Write-Host "✅ Despliegue completado con éxito en $ServerHost"
