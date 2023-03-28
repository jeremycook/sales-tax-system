# Cohub.WebApp Readme

## Linux

```bash
sudo apt update

# Install libgdiplus
sudo apt install libgdiplus
```

## Deploying

1. Publish the appropriate profile like "linux-x64".
2. `rsync` the changes via bash.
3. Restart the service

```bash
# On local
cd Cohub.WebApp
dotnet publish --configuration Release
rsync -azvhP bin/Release/net5.0/publish/ app-anywhereusa@anywhereusa1.cohub.us:~/websites/anywhereusa.cohub.us/wwwroot

# On remote
ssh root@anywhereusa1.cohub.us
systemctl restart app-anywhereusa.cohub.us.service && date
```
