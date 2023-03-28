# Setup Ubuntu 20.04 Server on Digital Ocean

If on Windows I recommend using Ubuntu from the Microsoft Store.

## Create SSH Key

https://help.github.com/en/articles/generating-a-new-ssh-key-and-adding-it-to-the-ssh-agent

```bash
ssh-keygen -t rsa -b 4096 -C "your_email@example.com"
```

## SSH Into It

Create the droplet with your public key configured, and then ssh into it.

```bash
ssh root@[ip|domain]
```

## Initial Server Setup with Ubuntu 18.04

https://www.digitalocean.com/community/tutorials/initial-server-setup-with-ubuntu-18-04

Logged in as root:

```bash
apt update
apt upgrade
reboot

timedatectl set-timezone America/Denver

adduser jeremy # and enter password
usermod -aG sudo jeremy
# May need to `apt install rsync`
rsync --archive --chown=jeremy:jeremy ~/.ssh /home/jeremy

exit
```

Now you can connect as the user you just created (i.e. "jeremy")

```bash
ssh jeremy@[ip|domain]
```

Install dotnet Snap

See [instructions](https://github.com/dotnet/core/blob/master/release-notes/5.0/preview/5.0.0-rc.2-install-instructions.md)

```bash
# As root
snap install dotnet-sdk --channel=5.0/beta --classic
snap alias dotnet-sdk.dotnet dotnet
```

Create the user that will run the website. I like to prefix with "app-" so I know what is one of my app or service accounts, and use the subdomain as the name of the app. If it will use your main domain (example in example.com) then I will call it app-www and redirect www.example.com to example.com. Some example names:

* app-www via www.example.com, example.com
* app-myapp via myapp.example.com
* app-test-myapp via test-myapp.example.com

```bash
# As root
adduser --disabled-password app-subdomain
rsync --archive --chown=app-subdomain:app-subdomain ~/.ssh /home/app-subdomain
```

```bash
sudo su - app-subdomain
mkdir -p apps/subdomain/wwwroot apps/subdomain/wwwroot
cd  apps/subdomain
```

## NGINX

https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-5.0

## Snapd

```
apt update
apt install snapd
```

## Certbot

Certbot https://certbot.eff.org/lets-encrypt/ubuntufocal-nginx

```
snap install core; snap refresh core
# remove old certbot packages
snap install --classic certbot
ln -s /snap/bin/certbot /usr/bin/certbot
certbot --nginx
certbot renew --dry-run
```

## Seq

```
# Test without persistent storage
docker run \
  --name seq \
  -d \
  --restart unless-stopped \
  -e ACCEPT_EULA=Y \
  -p 15340:80 \
  -p 15341:5341 \
  datalust/seq:latest

# Production with persistent storage
docker run \
  --name seq \
  -d \
  --restart unless-stopped \
  -e ACCEPT_EULA=Y \
  -v /opt/seq/data:/data \
  -p 15340:80 \
  -p 15341:5341 \
  datalust/seq:latest
```

Nginx configuration

```
server {
    server_name   seq1.cohub.us;
    client_max_body_size 100m;
    large_client_header_buffers 4 16k;
    proxy_connect_timeout  600;
    proxy_send_timeout     600;
    proxy_read_timeout     600;
    send_timeout           600;
    location / {
        proxy_pass         http://localhost:5340;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```
