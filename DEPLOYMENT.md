# Deployment & Production Guidelines

Detailed deployment setup guidelines.

## Hosting Stack
- **OS**: Linux (Ubuntu 22.04 LTS recommended)
- **Web Server**: Kestrel reverse-proxied behind **Nginx**
- **Process Manager**: Systemd service to maintain Kestrel process running state
- **Database**: Dedicated PostgreSQL cloud instance with periodic backups

## Systemd Service Configuration
Create a service file `/etc/systemd/system/hajj-erp.service`:
```ini
[Unit]
Description=Hajj & Umrah ERP Web Application
After=network.target

[Service]
WorkingDirectory=/var/www/hajj-erp
ExecStart=/usr/bin/dotnet /var/www/hajj-erp/BusinessManagement.Web.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=hajj-erp-app
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```
