# REPUBLIC MABAR - ANTI BERGETAR (Recoil Helper)

Selamat datang di repositori **REPUBLIC MABAR - ANTI BERGETAR**. Aplikasi ini adalah perangkat lunak utilitas (Recoil Helper) yang dibangun menggunakan WPF dan .NET 8.0 untuk membantu mengurangi efek getaran (recoil) saat bermain game, memberikan kontrol yang lebih stabil, dan meningkatkan kenyamanan bermain.

## 🚀 Fitur Utama

- **Recoil Engine:** Sistem inti yang mengatur dan meminimalisir getaran/recoil.
- **Dukungan Multi-bahasa:** Antarmuka aplikasi dapat digunakan dalam bahasa Indonesia, Inggris, dan Arab.
- **Discord Rich Presence (RPC):** Terintegrasi secara otomatis dengan profil Discord Anda untuk menunjukkan status aktivitas Anda.
- **Manajemen Hotkey:** Kontrol cepat fitur-fitur aplikasi saat berada di dalam game tanpa harus membuka jendela aplikasi (HotkeyManager).
- **Notifikasi Suara:** Umpan balik audio untuk setiap interaksi dengan menggunakan *SoundManager*.
- **Antarmuka Modern & Responsif:** Desain UI/UX yang elegan didukung oleh *ModernWpfUI*.
- **Fitur Always on Top:** Opsional untuk selalu menampilkan overlay atau UI di atas jendela lain.

## 🛠️ Teknologi yang Digunakan

- **Framework:** .NET 8.0 (WPF - Windows Presentation Foundation)
- **Library Tambahan:** 
  - `ModernWpfUI` (v0.9.6) untuk tampilan antarmuka modern.
  - `DiscordRichPresence` (v1.6.1.70) untuk integrasi status aplikasi ke Discord.
  - `Newtonsoft.Json` (v13.0.3) untuk manajemen konfigurasi/JSON.

## 📋 Prasyarat

Sebelum mencoba atau mengkompilasi aplikasi ini, pastikan Anda telah memasang:

- [SDK .NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) atau versi yang lebih baru.
- Sistem Operasi Windows (Proyek ini secara spesifik menargetkan `net8.0-windows`).
- Visual Studio 2022 (Disarankan) atau IDE pendukung C# lainnya.

## ⚙️ Instalasi & Kompilasi

1. **Kloning Repositori**
   Mulai dengan mengkloning repositori ini ke komputer lokal Anda:
   ```bash
   git clone https://github.com/akbartriagi/republic-mabar-anti-bergetar.git
   cd "REPUBLIC MABAR - ANTI BERGETAR"
   ```

2. **Restore Dependency**
   Jalankan perintah berikut untuk mengunduh semua NuGet package yang dibutuhkan proyek:
   ```bash
   dotnet restore
   ```

3. **Build Aplikasi**
   Untuk melakukan build, jalankan:
   ```bash
   dotnet build -c Release
   ```

4. **Jalankan Aplikasi**
   Anda bisa menjalankannya lewat Visual Studio atau menggunakan CLI:
   ```bash
   dotnet run
   ```

## 🎮 Cara Penggunaan

1. Buka aplikasi `RecoilHelper` yang telah dibuild.
2. Saat pertama kali diluncurkan, Anda bisa mengatur bahasa melalui halaman **Settings(Pengaturan)**.
3. Anda dapat memantau status AKTIF atau TIDAK AKTIF pelacak recoil langsung dari menu **Dashboard**.
4. Di bagian *Settings*, atur hotkey dan suara sesuai preferensi Anda.
5. Biarkan aplikasi berjalan di latar belakang (atau jadikan *Always on Top*) saat Anda mulai memainkan game. Profile Discord Anda secara otomatis akan ter-update.

## 🤝 Kontribusi

Kami sangat berterima kasih apabila Anda ingin berkontribusi pada pengembangan **REPUBLIC MABAR - ANTI BERGETAR**.

1. Fork repositori ini.
2. Buat branch fitur baru (`git checkout -b fitur-baru-keren`).
3. Lakukan Commit untuk perubahan Anda (`git commit -m 'Menambahkan fitur baru yang keren'`).
4. Push ke branch (`git push origin fitur-baru-keren`).
5. Buka **Pull Request**.

Segala bentuk kontribusi—baik itu perbaikan bug, penambahan fitur, terjemahan, atau perbaikan dokumentasi—sangat diapresiasi!

## 📜 Lisensi

Proyek ini dibuat untuk keperluan utilitas permainan dan dibagikan dalam komunitas Republic Mabar. Harap gunakan dengan bijak dan sesuai dengan aturan komunitas atau game yang dimainkan (Terms of Service). dan pstinya selalu gunakan prinsip DWYOR (Do With Your Own Risk).
