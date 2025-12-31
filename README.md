# 💬 Socket Chat

TCP/IP socket programlama ile geliştirilmiş, çok kullanıcılı bir sohbet uygulamasıdır.

## 📋 Özellikler

### Sunucu (ChatServer)
- Çoklu istemci bağlantı yönetimi
- Asenkron mesaj işleme
- Otomatik kullanıcı listesi yayını
- Broadcast mesaj sistemi

### İstemci (ChatClient.WinForms)
- Modern Windows Forms arayüzü
- Gerçek zamanlı mesaj alımı
- Kullanıcı listesi görüntüleme
- Bağlantı durumu takibi

### Komutlar
| Komut | Açıklama |
|-------|----------|
| `/help` | Yardım menüsünü gösterir |
| `/nick YeniAd` | Takma ad değiştirir |
| `/w Kullanıcı Mesaj` | Özel mesaj gönderir (whisper) |
| `/list` | Çevrimiçi kullanıcıları listeler |

## 🏗️ Mimari

```
┌──────────────┐         TCP/IP         ┌──────────────┐
│   İstemci 1  │◄──────────────────────►│              │
├──────────────┤                        │    Sunucu    │
│   İstemci 2  │◄──────────────────────►│   (Server)   │
├──────────────┤                        │              │
│   İstemci N  │◄──────────────────────►│              │
└──────────────┘                        └──────────────┘
```

## 🛠️ Teknolojiler

| Teknoloji | Kullanım |
|-----------|----------|
| TcpListener/TcpClient | Socket bağlantısı |
| ConcurrentDictionary | Thread-safe istemci yönetimi |
| async/await | Asenkron I/O işlemleri |
| StreamReader/Writer | TCP stream okuma/yazma |

## 🚀 Kurulum ve Çalıştırma

### 1. Sunucuyu Başlatın
```bash
cd ChatServer
dotnet run
```
Sunucu varsayılan olarak `127.0.0.1:5000` portunda dinler.

### 2. İstemciyi Çalıştırın
```bash
cd ChatClient.WinForms
dotnet run
```

### 3. Bağlanma
- Sunucu IP: `127.0.0.1`
- Port: `5000`
- Takma ad girin ve "Bağlan" butonuna tıklayın

## 📁 Proje Yapısı

```
SocketChat/
├── ChatServer/
│   ├── Server.cs         # Ana sunucu sınıfı, bağlantı yönetimi
│   ├── Program.cs        # Sunucu başlatma
│   └── ClientConnection  # İç içe sınıf, istemci bağlantısı
│
├── ChatClient.WinForms/
│   ├── MainForm.cs       # Windows Forms arayüzü
│   ├── NetChatClient.cs  # TCP istemci sınıfı
│   └── Program.cs        # Uygulamayı başlatma
│
└── SocketChat.sln        # Solution dosyası
```

## ⚙️ Protokol

- Mesajlar satır bazlı (newline ile ayrılır)
- Özel komutlar `/` ile başlar
- `#USERS` protokol mesajı ile kullanıcı listesi güncellenir

## 👨‍💻 Geliştirici

Yazılım Mühendisliği 2. Sınıf - Haftalık Proje
