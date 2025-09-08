# Item Notes

Bu repoda, PDF'de verilen **Infopoints** programı gereksinimlerine uygun olarak geliştirilen `item‑notes` masaüstü uygulamasının tam kaynak kodu ve proje yapısı bulunmaktadır. Uygulama, .NET 8 üzerinde **Avalonia UI** kullanılarak çapraz platform masaüstü uygulaması olarak geliştirilmiştir ve **Entity Framework Core** ile **MSSQL** veritabanına bağlanmaktadır. Uygulamanın amacı, birden çok sayfa içerebilen, yuvarlak şekilli ve üç farklı renkte notlar oluşturup düzenleyebilmektir. Her not, yaratıcısının adı ve oluşturulma tarihi ile birlikte saklanır. Kullanıcılar notların içerisine farklı yazı tipleri, yazı biçimlendirme (kalın, italik, altı çizili, üstü çizili), madde işaretleri, resim yükleme, tablo ekleme, sembol ekleme ve bağlantı ekleme gibi özellikler uygulayabilirler.

## Proje Yapısı

```
item-notes/
├── README.md                 # Bu dosya
├── ItemNotes.sln             # Çözüm dosyası (isteğe bağlı, paket yöneticileri için)
└── src/
    ├── ItemNotes.Domain/     # Alan katmanı (entity ve enum tanımları)
    │   ├── ItemNotes.Domain.csproj
    │   ├── Entities/
    │   │   ├── Note.cs
    │   │   └── NotePage.cs
    │   └── Enums/
    │       └── NoteColor.cs
    ├── ItemNotes.Infrastructure/ # Altyapı katmanı (EF Core, veritabanı erişimi)
    │   ├── ItemNotes.Infrastructure.csproj
    │   ├── Data/
    │   │   └── ItemNotesDbContext.cs
    │   ├── Repositories/
    │   │   ├── INoteRepository.cs
    │   │   └── NoteRepository.cs
    │   └── appsettings.json
    ├── ItemNotes.Application/    # İş katmanı (servisler ve arayüzler)
    │   ├── ItemNotes.Application.csproj
    │   ├── Interfaces/
    │   │   └── INoteService.cs
    │   └── Services/
    │       └── NoteService.cs
    └── ItemNotes.UI/             # Kullanıcı arayüzü katmanı (Avalonia)
        ├── ItemNotes.UI.csproj
        ├── App.xaml
        ├── App.xaml.cs
        ├── Program.cs
        ├── Views/
        │   ├── MainWindow.xaml
        │   ├── MainWindow.xaml.cs
        │   ├── CreateNoteWindow.xaml
        │   ├── CreateNoteWindow.xaml.cs
        │   ├── NoteWindow.xaml
        │   └── NoteWindow.xaml.cs
        ├── ViewModels/
        │   ├── MainWindowViewModel.cs
        │   ├── CreateNoteWindowViewModel.cs
        │   └── NoteWindowViewModel.cs
        ├── Models/
        │   └── NoteModel.cs
        └── Converters/
            ├── NullToBoolConverter.cs
            └── NoteColorToBrushConverter.cs
```

> **Not:** Bu repoda derleme için `dotnet` komutlarının çalışması zorunlu değildir. Kod dosyaları, projeyi JetBrains Rider ya da WebStorm'da açtığınızda derlenebilecek şekilde yapılandırılmıştır. Migration dosyaları boş bırakılmıştır; `dotnet ef` komutları ile kendiniz üretebilirsiniz.

## Kurulum ve Çalıştırma

1. **Gerekli araçları yükleyin.** Projenin derlenebilmesi için bilgisayarınızda .NET 8 SDK ve **Avalonia UI** şablonları kurulu olmalıdır. Komut satırından:

   ```bash
   dotnet new install Avalonia.Templates
   ```

2. **Proje oluşturma.** Rider kullanıyorsanız Yeni Çözüm sihirbazında "Avalonia .NET MVVM App" şablonunu seçin. Komut satırı kullanıyorsanız aşağıdaki komut yeni bir MVVM proje dizini oluşturur:

   ```bash
   dotnet new avalonia.mvvm -o item-notes -n ItemNotes
   ```

   Bu repoda gerekli proje dosyaları zaten bulunduğundan yalnızca NuGet bağımlılıklarını geri yüklemek yeterlidir.

3. **Bağımlılıkları yükleyin.** Çözüm dizininde NuGet paketlerini geri yüklemek için:

   ```bash
   dotnet restore
   ```

4. **Veritabanını yapılandırın.** `ItemNotes.Infrastructure/appsettings.json` dosyasında `DefaultConnection` anahtarının değerini güncelleyin. PDF'de verilen bağlantı dizesi örneği şu şekildedir:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "server=localhost;uid=sa;password=reallyStrongPwd123;database=item_notes_db;TrustServerCertificate=true"
     }
   }
   ```

5. **Migration oluşturun.** EF Core Migration oluşturmak için önce `Microsoft.EntityFrameworkCore.Tools` paketini kurduğunuzdan emin olun. Ardından:

   ```bash
   dotnet ef migrations add InitialCreate --project src/ItemNotes.Infrastructure --startup-project src/ItemNotes.UI
   dotnet ef database update --project src/ItemNotes.Infrastructure --startup-project src/ItemNotes.UI
   ```

   Bu komutlar veritabanında `Notes` ve `NotePages` tablolarını oluşturacaktır.

6. **Uygulamayı çalıştırın.** Rider içinde `ItemNotes.UI` projesini başlangıç projesi olarak ayarlayın ve çalıştırın. Komut satırından ise şu komut ile uygulamayı başlatabilirsiniz:

   ```bash
   dotnet run --project src/ItemNotes.UI
   ```

## PDF'den Gelen Gereksinimler

PDF dokümanına göre uygulama, masaüstünde "sticky notes" benzeri fakat veritabanı destekli bir kelime işleme programıdır. Temel gereksinimler şunlardır:

- **Yuvarlak form ve boyut ayarı:** Not pencereleri yuvarlak şekilde tasarlanmış olup istenildiğinde büyütülüp küçültülebilir.
- **Renk seçenekleri:** Kırmızı, yeşil ve sarı olmak üzere üç farklı renk seçeneği bulunur.
- **Çizgili arkaplan:** Notların arka planı çizgili bir kağıt görünümüne sahiptir.
- **Çoklu sayfa:** Bir not içinde birden fazla sayfa (sekme) açılabilir.
- **Yazma koruması:** Notlar istenildiğinde salt okunur yapılabilir.
- **Metin biçimlendirme:** Farklı yazı tipleri, yazı büyüklükleri ve biçimlendirme (kalın, italik, altı çizili, üstü çizili) desteği vardır.
- **Liste ve madde işaretleri:** Farklı madde işaretleri veya numaralı listeler eklenebilir.
- **Resim ekleme:** Kullanıcılar notlara resim yükleyebilir veya clipboard üzerinden yapıştırabilir.
- **Tablo ekleme:** Hücre sayısı belirlenen tablolar eklenebilir.
- **Sembol ve bağlantı ekleme:** Özel semboller ve URL bağlantıları eklenebilir.
- **Alt bilgi:** Notu oluşturan kullanıcının adı ve oluşturulma tarihi otomatik olarak görüntülenir.
- **Başlık:** Her notun başlığı vardır ve liste ekranında başlık ile tarih birlikte gösterilir.

Yukarıda verilen kodlar, bu özelliklerin çoğu için bir temel sağlar. `NoteWindowViewModel` içerisinde biçimlendirme komutlarının uygulanması örnek olarak verilmiştir; Avalonia'da `RichTextBox` API'si WPF ile büyük ölçüde benzer olduğundan bu kodlar uyarlanabilir. Geliştiriciler gerekirse komutların detaylarını genişleterek tam işlevsellik sağlayabilirler.
