# Teknik Kararlar

Bu belge projede alınan mimari ve tasarım kararlarını, gerekçeleriyle
birlikte kaydeder. Format: karar, gerekçe, değerlendirilen alternatif.

---

## 1. Katmanlı proje yapısı

**Karar:** Backend dört ayrı projeye bölündü: `Domain`, `Infrastructure`,
`Api`, `Tests`.

**Gerekçe:** Katman sınırlarının derleyici tarafından zorlanması.
`Domain` projesi `Infrastructure`'a referans vermediği için iş mantığının
içine veri erişim kodu sızdırmak derleme hatası üretir. Tek projede bu
disiplin yalnızca geliştiricinin dikkatiyle korunur.

**Alternatif:** Tek proje içinde klasör bazlı ayrım. Daha az kurulum
gerektirir, küçük projelerde yeterlidir. Tercih edilmeme sebebi: bu projede
amaç bağımlılık yönü kavramını somut olarak uygulamak.

---

## 2. Swagger Petstore'un `Category` kaynağı `Species` olarak modellendi

**Karar:** Referans API'deki `Category` yerine `Species` (tür) ve `Breed`
(cins) olmak üzere iki seviyeli bir yapı kuruldu.

**Gerekçe:** Cins ölçütleri (enerji, ses, alan ihtiyacı) tür seviyesinde 
anlamlı değil. "kedi" için tek bir enerji seviyesi tanımlanamaz. AI önerisinin
dayanacağı veriler cinse ait olduğundan iki seviyeli yapı zorunlu hale geldi.

**Alternatif:** Referans API'deki tek seviyeli Category. Daha az tablo, daha 
basit sorgular. Tercih edilmeme sebebi: cins ölçütlerini barındıracak yer bırakmıyor. 

---

## 3. Cins özellikleri yapılandırılmış olarak saklanıyor

**Karar:** `Breed` tablosunda yedi ölçüt 1–5 aralığında sayısal alan
olarak tutuluyor: `EnergyLevel`, `NoiseLevel`, `SpaceRequirement`,
`AppetiteLevel`, `GoodWithChildren`, `GoodWithOtherPets`, `GroomingNeed`.

**Gerekçe:** AI servisinin `recommend-pet` endpoint'i gerçek veriye
dayanmalı. Serbest metin açıklamalar modele bağlam olarak verilebilir
ancak filtrelenemez ve model yorum yapmak zorunda kalır. Sayısal ölçekler
hem SQL seviyesinde filtrelemeye hem de modele karşılaştırılabilir
girdi sunmaya elverişli.

**Alternatif:** Serbest metin açıklama alanı. Yazması esnek, nüansı
korur, ancak sorgulanamaz ve tutarsız girilir.

---

## 4. Yaş yerine doğum tarihi saklanıyor

**Karar:** `Pet` tablosunda `Age` yerine `BirthDate` tutuluyor.

**Gerekçe:** Yaş zamana bağlı olarak değişen bir değer; kaydedildiği
anda doğru olsa bile bir yıl sonra güncellenmediği sürece yanlış olur. 
BirthDate ise değişmeyen bir olgudur, yaş ondan hesaplanır. 
Genel ilke: türetilebilen veri saklanmaz, kaynağı saklanır.

---

## 5. `Pet.Status` bir enum

**Karar:** Dört değer: `Available`, `Pending`, `Sick`, `Sold`.

**Gerekçe:** `Sick` durumu referans API'de yok, eklendi: tedavi altındaki
bir hayvan satışa kapalıdır ancak `Sold` değildir, ileride tekrar
`Available` olacaktır. Bir hayvanın aynı anda tek durumda olduğu kabul
edildi; hastalık ayrı bir alan olarak değil durum değeri olarak modellendi.

**Alternatif:** `Status` + ayrı bir `IsHealthy` alanı. İki alanın
kombinasyonu geçersiz durumlar üretebileceği için (`Sold` + `IsHealthy:
false` ne anlama gelir?) tercih edilmedi.

---

## 6. Satılan kayıtlar silinmiyor

**Karar:** Pet kayıtları fiziksel olarak silinmez. Satış gerçekleştiğinde
Status değeri Sold olur; kaydın listelerden kaldırılması gerektiğinde ayrı
bir IsDeleted bayrağı işaretlenir (bkz. madde 18).

**Gerekçe:** Soft delete yapılması genel anlamda hem admin tarafından
tüm hayvanlar için görülebilir ve ulaşılabilir olur. Kayıtları sürekli
tutulur. Sadece Statusları sold yapılır ve ekstra databaseden bir silinme
uğraşında bulunulmaz. Kayıt silinemez çünkü OrderItem ona referans veriyor
Silinse sipariş geçmişi kırılır. Bu teknik zorunluluk, tercih değil.

---

## 7. Fotoğraflar ayrı tabloda

**Karar:** `Pet` üzerinde tek bir `PhotoUrl` alanı yerine `PetPhoto`
tablosu kuruldu (`IsPrimary`, `SortOrder` alanlarıyla).

**Gerekçe:** Gerçek bir PetStore'da hayvanların birden fazla fotoğrafları
bulunur. Bu diğer aynı cinsteki hayvanları ayırt etmekte kilit rol oynar.

---

## 8. `OrderItem` fiyatı sipariş anında dondurulur

**Karar:** `OrderItem.Price` alanı, sipariş oluşturulduğu andaki
`Pet.Price` değerinin kopyasıdır.

**Gerekçe:** Pet fiyatı zamanla değişebilir. OrderItem sipariş anındaki
fiyatı kopyalamazsa, geçmiş bir siparişin toplam tutarı fiyat güncellendiğinde
geriye dönük olarak değişmiş görünür. Sipariş kaydı işlem anının fotoğrafıdır,
sonradan değişmemelidir.

---

## 9. `Favorite` tablosunda bileşik birincil anahtar

**Karar:** Ayrı bir `Id` yerine `(UserId, PetId)` birincil anahtar olarak
tanımlandı.

**Gerekçe:** Bir kullanıcının aynı pet'i birden fazla favorileyememesi
kısıtı veritabanı seviyesinde garanti altına alınır. Uygulama katmanında
kontrol etmek yarış koşullarına açıktır.

---

## 10. Kimlik doğrulama kapsamı sınırlı tutuldu

**Karar:** `User` tablosu ve ilişkileri (`Favorite.UserId`,
`Order.UserId`) baştan kuruldu, ancak JWT üretimi ve yetkilendirme
sonraki bir aşamaya bırakıldı.

**Gerekçe:** Şemanın sonradan değiştirilmesi migration ve veri taşıma
maliyeti getirir; ilişkiler baştan doğru kurulduğunda auth eklemek
yalnızca `Program.cs` ve controller seviyesinde değişiklik gerektirir.
Kimlik doğrulamanın derinlemesine uygulaması önceki bir projede
(Login System) ele alındığından, bu projede odak AI servisi ve
OpenAPI-first çalışma disiplinidir.

**Güncelleme:** Bu erteleme sona erdi, JWT 40–52 arası maddelerde
uygulandı. Tahmin doğru çıktı: şema hiç değişmedi, `RefreshTokens`
dışında yeni tablo gerekmedi.

---

## 11. Ürün katalogu kapsam dışı

**Karar:** Mama, kum, oyuncak gibi ürünler v1 kapsamına alınmadı.

**Gerekçe:** Tamamen farklı bir sisteme dayandığı için genel olarak v1 kapsamına
alınmadı ileride v2, v3 versiyonlarında düşünülebilir.

---

## 12. Her kaynak için repository + service katmanı

**Karar:** Her veri kaynağı (`Species`, `Breed`, `User`, `Pet`) için üç parça
oluşturuluyor: `Domain`'de repository arayüzü, `Infrastructure`'da implementasyonu,
`Domain`'de servis sınıfı. Controller yalnızca servis arayüzünü tanır.

**Gerekçe:** Controller'ın `DbContext`'e doğrudan erişmemesi, veri erişim kodunun HTTP
katmanına sızmasını engelliyor. İş kuralları servis katmanında tek noktada toplanıyor —
örneğin `Pet` oluşturulurken `Status`, `CreatedAt` ve `UpdatedAt` alanlarının dışarıdan
alınmayıp servis tarafından doldurulması. Servisler yalnızca arayüzlere bağımlı olduğu
için birim testlerinde sahte (mock) repository ile çalıştırılabiliyor.

**Alternatif:** Basit kaynaklarda (`Species` gibi iş kuralı taşımayanlarda) controller'ın
doğrudan `DbContext` kullanması. Daha az kod üretirdi, ancak aynı projede iki farklı
desen bulunması tutarsızlık yaratacağı için tercih edilmedi.

---

## 13. Repository sorgularında `FirstOrDefaultAsync` kullanımı

**Karar:** Birincil anahtarla tekil kayıt getiren repository metotlarında `FindAsync`
yerine `FirstOrDefaultAsync` kullanılıyor.

**Gerekçe:** `FindAsync` önce EF Core'un değişiklik takip listesine bakar ve kayıt
bellekteyse veritabanına hiç gitmez; bu yönüyle daha verimlidir. Ancak `Include`
zincirini desteklemez. `PetResponse` içindeki `BreedName` ve `SpeciesName` alanlarının
doldurulabilmesi için ilişkili verinin yüklenmesi zorunlu olduğundan, bu verimlilik
avantajından vazgeçildi.

**Alternatif:** `FindAsync` kullanıp ilişkili veriyi ayrı sorgularla çekmek. Daha fazla
veritabanı turu anlamına geldiği için tercih edilmedi.

---

## 14. API cevaplarında entity yerine response DTO'ları

**Karar:** Servis katmanı entity değil DTO döndürüyor (`PetResponse`, `SpeciesResponse`
vb.). DTO'lar `Domain/DTOs/` altında tutuluyor.

**Gerekçe:** Entity'lerin doğrudan serileştirilmesi üç somut soruna yol açtı. Birincisi,
navigation property'ler döngü oluşturdu (`Pet` → `Breed` → `Pets` → `Pet`) ve
serileştirme hatası verdi. İkincisi, dışarıya açılmaması gereken alanlar cevaba sızdı —
`User.PasswordHash` bunun en net örneği. Üçüncüsü ve en önemlisi, veritabanı şeması ile
API sözleşmesi aynı sınıfa bağlı kaldığı sürece şemadaki her değişiklik sözleşmeyi de
habersizce değiştiriyor. DTO bu bağı koparıyor: entity değişse bile DTO sabit kaldığı
sürece API tüketicileri etkilenmiyor.

DTO'lar ayrıca veriyi tüketiciye uygun biçime getiriyor: enum'lar sayı yerine metin
olarak sunuluyor, ilişkili nesneler iç içe gömülmek yerine düzleştiriliyor
(`breed.name` yerine `breedName`), ve saklanmayan `Age` alanı `BirthDate`'ten
hesaplanarak ekleniyor.

**Alternatif:** `ReferenceHandler.IgnoreCycles` ayarıyla döngüyü kırmak. Yalnızca
serileştirme hatasını önlüyor, cevapta `null` değerler bırakıyor ve diğer iki sorunu
hiç çözmüyor. Geçici çözüm olarak kullanıldı, DTO'lara geçişle birlikte kaldırıldı.

---

## 15. Enum'lar veritabanında metin olarak saklanıyor

**Karar:** `PetStatus`, `Gender`, `OrderStatus` ve `UserRole` alanları
`HasConversion<string>()` ile metin sütunlara eşleniyor. C# tarafında enum olarak
kullanılmaya devam ediyor.

**Gerekçe:** Veritabanına doğrudan bakıldığında `Status` sütununda `0` yerine
`Available` görünüyor; sorgu yazarken ve veri incelerken enum sıralamasını koda bakıp
hatırlamak gerekmiyor. Ayrıca enum tanımının ortasına yeni bir değer eklendiğinde
sonraki değerlerin sayısal karşılığı kayar ve mevcut kayıtların anlamı sessizce
değişir — metin saklandığında bu risk ortadan kalkıyor.

**Alternatif:** Varsayılan davranış olan tamsayı saklama. Daha az yer kaplıyor ve
karşılaştırmada marjinal olarak daha hızlı, ancak bu ölçekte fark edilir bir kazanç
sağlamıyor.

---

## 16. Tip değiştiren migration'lar mevcut veriyi dönüştürmez

**Karar:** Enum sütunlarını `integer`'dan `text`'e çeviren migration uygulanmadan önce
mevcut test verisi silindi.

**Gerekçe:** EF Core'un ürettiği `AlterColumn` çağrısı yalnızca sütun tipini değiştiriyor,
içindeki değerleri yeni anlamlarına çevirmiyor. PostgreSQL `0` tamsayısını `text`'e
dönüştürdüğünde ortaya `"Available"` değil `"0"` metni çıkıyor ve uygulama bu değeri
enum'a geri çeviremiyor. Bu projede veriler test amaçlı olduğu için silmek en pratik
yoldu.

**Not:** Gerçek veriyle çalışan bir sistemde bu yol izlenemez. Migration'a `AlterColumn`
öncesinde elle `UPDATE` ifadeleri eklenerek mevcut değerlerin karşılıkları yazılmalıdır.
Tip değiştiren her migration'ın mevcut veriyi nasıl etkilediği uygulamadan önce
incelenmelidir.

---

## 17. Seçici soft delete

**Karar:** Silme davranışı entity'nin rolüne göre farklılaşıyor. `Pet` ve `User`
kayıtları `IsDeleted` bayrağıyla işaretleniyor, fiziksel olarak silinmiyor.
`Species` ve `Breed` gerçek anlamda siliniyor, ancak bağlı kayıt varsa işlem
engelleniyor. `Favorite` ve `PetPhoto` için gerçek silme uygun; `Order` ve
`OrderItem` hiçbir koşulda silinmiyor.

**Gerekçe:** Soft delete uyguladığım tablolar başka tablolar tarafından referans
veriliyor. Pet kaydını silersem OrderItem ve Favorite üzerinden kurulan bağlar
kırılır, satış geçmişi anlamsızlaşır. User için de aynısı geçerli — bir kullanıcı
hesabını kapatsa bile verdiği siparişlerin sahibi belli kalmalı. Species ve Breed
ise referans kataloğu; bunlar zaten nadiren siliniyor ve bağlı kayıt varken 
silinmeleri engellendiği için geçmişi bozma riski yok.

**Alternatif:** Tüm tablolara soft delete uygulamak. Tutarlı görünür ancak her
entity'nin rolü farklı — bir favoriyi kaldırmak gerçekten kaldırmak demektir,
geçmişte saklanacak bir bilgi taşımaz. Tek kuralı her yere uygulamak yapay bir
tutarlılık üretirdi.

---

## 18. `Pet.Status` ile `IsDeleted` ayrı alanlar

**Karar:** Soft delete için `PetStatus` enum'una yeni bir değer eklenmedi; ayrı
bir `IsDeleted` bayrağı tanımlandı.

**Gerekçe:** Bunlar iki ayrı bilgi. Status hayvanın gerçek durumunu anlatıyor
satışa hazır mı, rezerve mi, tedavide mi, satıldı mı. IsDeleted ise kaydın
listelerde görünüp görünmeyeceğini belirliyor. İkisi birbirinden bağımsız:
satılmış bir kaydı da kaldırabilmeliyim, satışa hazır bir kaydı da.
Enum'a Removed eklesem, bir hayvanın hem satılmış hem kaldırılmış olduğunu ifade edemezdim.

**Alternatif:** `PetStatus` enum'una `Removed` değeri eklemek. Tek alanla
yönetilirdi, ancak hayvanın gerçek durumu ile kaydın görünürlüğü farklı iki
bilgi — satılmış bir kayıt da listeden kaldırılabilir, satışa hazır bir kayıt da.
Tek alana sıkıştırmak bu ayrımı kaybediyordu.

---

## 19. Soft delete filtresi `DbContext` seviyesinde tanımlı

**Karar:** `Pet` ve `User` için `HasQueryFilter` ile global sorgu filtresi
tanımlandı. Repository metotlarında ayrıca `Where(x => !x.IsDeleted)` koşulu
yazılmıyor.

**Gerekçe:** Filtre tek noktada tanımlandığı için yeni bir sorgu yazarken
unutulma riski ortadan kalkıyor. Silinmiş kayıtlara kasten erişmek gerektiğinde
(raporlama, yönetim ekranı) `IgnoreQueryFilters()` ile filtre devre dışı
bırakılabiliyor — yani varsayılan güvenli, istisna açık şekilde belirtiliyor.

**Not:** Filtrenin etkisi `GetByIdAsync` için de geçerli olduğundan, silinmiş bir
kayıt üzerinde güncelleme veya tekrar silme denemesi `404` döndürüyor. Bu
davranış ayrıca kodlanmadı, filtrenin doğal sonucu.

---

## 20. İlişkisel kısıtlar için `409 Conflict`

**Karar:** Bağlı kaydı olan bir `Species` veya `Breed` silinmeye çalışıldığında
`ConflictException` fırlatılıyor ve istemciye `409 Conflict` dönüyor. Kontrol
servis katmanında, silme işleminden önce yapılıyor.

**Gerekçe:** Kontrolü kendim yaptığım için hangi durumda ne fırlatacağımı önceden
biliyorum ve istemciye anlamlı bir mesaj verebiliyorum. Veritabanının foreign key
hatasına bıraksaydım, o hata GlobalExceptionHandler'da tanınmayan bir tip olarak 500'e
düşerdi ve kullanıcı "An unexpected error occurred." görürdü — oysa sorun sunucuda değil,
silmeye çalıştığı kaydın bağlı kayıtları olmasında.

**Alternatif:** Veritabanının foreign key hatasını (`DbUpdateException`) yakalayıp
çevirmek. Fazladan sorgu gerektirmez, ancak hata mesajı veritabanı kısıt adından
türetilmek zorunda kalır ve istemciye anlamlı bir açıklama üretmek zorlaşır.

**Neden `400` değil:** İstek gövdesi geçerli, istemcinin düzeltebileceği bir
girdi hatası yok. Engelleyen şey kaynağın o anki durumu — `409` tam olarak bu
durumu ifade ediyor.

---

## 21. Doğrulama iki katmanda yapılıyor

**Karar:** Biçim ve aralık kuralları DTO'larda Data Annotations ile tanımlanıyor
(`Required`, `StringLength`, `Range`, `EmailAddress`). Bağlam gerektiren kurallar
servis katmanında kontrol ediliyor — var olmayan `BreedId`, gelecek bir
`BirthDate`, bağlı kaydı olan bir türün silinmesi.

**Gerekçe:** Gerekçe: DTO'daki attribute'lar yalnızca derleme zamanı sabitleri
alabiliyor; [Range(1, 5)] yazabiliyorum ama,
"bu BreedId veritabanında var mı" ya da "bu tarih bugünden sonra mı" diye soramıyorum.
Bu yüzden ayrım şuraya düşüyor: veri tek başına bakılarak doğrulanabiliyorsa DTO'da,
dış bir şeye (veritabanı, bugünün tarihi) bakmak gerekiyorsa serviste. Pratik faydası da var.
Attribute'lar controller'a hiç ulaşmadan çalışıyor, geçersiz bir istek veritabanına
dokunmadan reddediliyor.

**Sonuç:** İki mekanizma iki farklı cevap biçimi üretiyor. Data Annotations
ihlalleri `ValidationProblemDetails` döndürüyor ve hatalar alan bazında
`errors` nesnesinde gruplanıyor; servis katmanı hataları `GlobalExceptionHandler`
üzerinden geçip `detail` alanında tek bir mesaj taşıyor. İkisi de `400` ancak
şekilleri farklı — istemci her iki biçimi de ele almak durumunda.

---

## 22. Positional record'larda doğrulama attribute'ları

**Karar:** DTO'lardaki doğrulama attribute'ları `[property:]` öneki olmadan,
doğrudan constructor parametresine yazılıyor.

**Gerekçe:** ASP.NET Core model doğrulaması attribute'ları constructor
parametresinde arıyor. `[property:]` ile property'ye yönlendirildiğinde
doğrulama çalışmıyor ve ASP.NET Core bu durumu çalışma zamanında
`InvalidOperationException` ile bildiriyor — istek `500` dönüyor.

**Not:** Hata derleme aşamasında yakalanmıyor; yalnızca o endpoint'e istek
geldiğinde ortaya çıkıyor. Attribute eklenen her DTO için en az bir çağrı
yapılarak doğrulanması gerekiyor.

---

## 23. Kendi exception tiplerinin isim benzerliği

**Not:** Projede tanımlı `NotFoundException` ve `ValidationException` tipleri,
.NET'in kendi `KeyNotFoundException` ve
`System.ComponentModel.DataAnnotations.ValidationException` tipleriyle
karışabiliyor. Yanlış tip kullanıldığında derleme hatası oluşmuyor;
`GlobalExceptionHandler` o tipi tanımadığı için istek `500` dönüyor ve hata
mesajı istemciye ulaşmıyor.

Yeni bir exception fırlatırken `using PetStore.Domain.Exceptions;` satırının
mevcut olduğundan ve IDE'nin otomatik tamamlamasının doğru tipi seçtiğinden emin
olunmalı.

---

## 24. Sipariş işlemleri tek bir `SaveChangesAsync` ile kaydediliyor

**Karar:** Sipariş oluşturulurken `Order` kaydı, `OrderItem` kayıtları ve `Pet`
güncellemeleri tek bir `SaveChangesAsync` çağrısıyla yazılıyor. Açık transaction
kullanılmadı.

**Gerekçe:** EF Core `SaveChangesAsync`'i zaten transaction içinde çalıştırıyor, yani
o çağrıya kadar biriken her şey ya hep birlikte yazılıyor ya hiç yazılmıyor. Sipariş
akışında dört işlem birbirine bağlı; biri yazılıp diğeri yazılmazsa hayvanlar rezerve
görünürken sipariş toplamı eksik kalabilir.

`OrderItem`'lar `order.OrderItems` koleksiyonuna eklendiği için EF Core sıralamayı
kendisi çözüyor — önce `Order`'ı yazıp `Id`'yi alıyor, sonra kalemlere işliyor. `Pet`
güncellemeleri ise değişiklik takibi üzerinden yakalanıyor; nesneler aynı `DbContext`
üzerinden okunduğu için alan ataması yapmak yeterli.

**Alternatif:** `BeginTransactionAsync` ile açık transaction. Birden fazla
`SaveChangesAsync` gerektiğinde zorunlu ama burada tek çağrı yettiği için gereksiz
karmaşıklık olurdu.

---

## 25. Doğrulamalar veri değişikliğinden önce yapılıyor

**Karar:** Servis metotlarında önce bütün kontroller yapılıyor, sonra değişikliklere
geçiliyor.

**Gerekçe:** Kontrol ve değişiklik iç içe olursa, listenin ortasında hata çıktığında
önceki elemanlarda yapılan değişiklikler yarım kalıyor. Bu akışta `SaveChangesAsync`'e
ulaşılmadığı için veritabanına yansımıyor ama araya başka bir kayıt girerse sorun
olurdu. Ayırmak hem güvenli hem metodu okumayı kolaylaştırıyor.

---

## 26. Sipariş durumları tek yönlü

**Karar:** `Created` tek giriş durumu, oradan `Paid` veya `Cancelled`'a geçiliyor. Bu
ikisi son durum. Her iki geçiş de sadece `Created` durumundaki siparişlerde çalışıyor,
değilse `409 Conflict` dönüyor.

**Gerekçe:** Kontrolü sadece "zaten iptal edilmiş mi" diye yazdığımda ödenmiş siparişi
de iptal edebiliyordum, o zaman satılmış hayvanlar tekrar `Available` oluyordu. Geçişi
`Created` ile sınırlayınca bu kapandı.

**Kapsam dışı:** İade akışı v1'e alınmadı. Eklenirse `Cancelled`'dan ayrı bir
`Refunded` durumu olmalı — iptal ile iade farklı şeyler.

---

## 27. Hayvan durumu siparişin durumunu takip ediyor

**Karar:** Sipariş `Created` olduğunda hayvanlar `Pending`, `Paid` olduğunda `Sold`,
`Cancelled` olduğunda `Available` oluyor.

**Gerekçe:** Sipariş oluştuğu anda hayvan rezerve edilmezse iki kullanıcı aynı hayvanı
aynı anda sipariş edebilir. Sipariş oluştururken yapılan `Status != Available` kontrolü
bunu engelliyor.

---

## 28. Rezervasyon süresi yapılandırmadan okunuyor

**Karar:** `Pet` tablosuna `ReservedUntil` alanı eklendi. Sipariş oluşturulduğunda
`UtcNow + ReservationMinutes` değeriyle doluyor, hayvan `Sold` veya `Available` olunca
`null`'a çekiliyor. Süre `appsettings.json`'dan okunuyor, servise `IOptions<OrderSettings>`
ile taşınıyor.

**Gerekçe:** Süreyi koda gömsem geliştirme ve üretimde farklı değer kullanamazdım.
Yapılandırmadan okuyunca `appsettings.Development.json`'da 2 dakika, üretimde 15 dakika
yazabiliyorum — kod değiştirmeden.

**Eksik:** Süresi dolan rezervasyonları serbest bırakan kod henüz yok. `ReservedUntil`
şu an sadece yazılıyor, okunmuyor. Bunun için düzenli çalışan bir arka plan servisi
gerekiyor, ayrı bir adım olarak ele alınacak.

---

## 29. `Favorite` kullanıcının alt kaynağı

**Karar:** Rotalar `/api/User/{userId}/favorites` altında toplandı, kod ayrı bir
`FavoriteController`'da.

**Gerekçe:** Favori bağımsız bir kaynak değil, hep bir kullanıcıya ait. Rota bunu
yansıtınca silme de sadeleşiyor: `DELETE /api/User/2/favorites/3` yeterli, gövdeye
gerek kalmıyor. Düz `/api/Favorite` olsaydı iki id'yi gövdede ya da sorgu dizesinde
taşımak gerekirdi.

**Not:** `PUT` yok, çünkü favori ya var ya yok — güncellenecek bir alanı yok.

---

## 30. Global sorgu filtresi `Include` ile gelen varlıklara uygulanmıyor

**Not:** `Pet` üzerindeki `HasQueryFilter(p => !p.IsDeleted)` sadece sorgunun kök
varlığı `Pet` olduğunda çalışıyor. `Favorite` üzerinden `Include(f => f.Pet)` ile
yüklendiğinde devreye girmiyor, silinmiş hayvan favori listesinde görünebiliyor.

Bu yüzden `FavoriteRepository`'de koşul elle yazıldı:

```csharp
.Where(f => f.UserId == userId && !f.Pet.IsDeleted)
```

Soft delete uygulanan bir varlığa `Include` ile erişen her sorguda bu kontrolü elle
eklemek gerekiyor.

---

## 31. Kapak fotoğrafı kuralları servis katmanında

**Karar:** Bir hayvanın en fazla bir kapak fotoğrafı olabiliyor. Üç yerde uygulanıyor:
yeni fotoğraf `IsPrimary: true` ile eklenince eski kapak temizleniyor; ilk fotoğraf
`IsPrimary: false` gelse bile kapak yapılıyor; kapak silinince `SortOrder`'ı en küçük
olan otomatik kapak oluyor.

**Gerekçe:** Tek fotoğrafı olan bir hayvanın kapaksız kalması anlamsız, kapak silinince
de hayvanın kapaksız kalmaması lazım. Bu kuralları veritabanında koşullu indeksle
yazmak mümkündü ama migration'a elle SQL eklemek gerekirdi ve hata mesajı istemciye
anlamlı gitmezdi.

---

## 32. `SortOrder` istemciden alınmıyor

**Karar:** Yeni fotoğrafın `SortOrder` değeri, o hayvanın mevcut en büyük değerinin bir
fazlası olarak serviste hesaplanıyor. `AddPetPhotoRequest`'te bu alan yok.

**Gerekçe:** İstemciye bıraksam aynı sıraya birden fazla fotoğraf düşebilir ya da
boşluk oluşabilirdi. Aynı mantık `Pet` oluştururken `Status` ve `CreatedAt` alanlarını
DTO'ya koymamakla aynı — sistemin belirlemesi gereken değerler dışarıdan alınmıyor.

---

## 33. Yüklenen dosyalar diskte, uygulamanın yanında tutuluyor

**Karar:** Fotoğraflar `wwwroot/uploads/pets` altına yazılıyor, statik dosya olarak
sunuluyor. Nesne depolama (S3, Azure Blob) kullanılmadı.

**Gerekçe:** Ek bir servis kurmadan çalışıyor, bu projenin amacı dosya yükleme akışını
öğrenmek. Nesne depolama gerçek üretim çözümü ve ölçekleniyor ama SDK öğrenmek ve ayrı
bir servis yönetmek konuyu dağıtırdı.

**Not:** Dosyalar konteyner içinde durduğu için volume bağlanmazsa konteyner yeniden
kurulunca gidiyorlar. Birden fazla sunucu çalıştırılırsa da dosyalar tek makinede kalır.
İkisi de v1 için kabul edilebilir, ama Dockerfile yazılırken volume tanımlanması
gerekiyor.

**Güncelleme:** Volume tanımlandı (bkz. madde 55). İkinci sorun — birden fazla sunucuda
dosyaların tek makinede kalması — hâlâ geçerli.

**Alternatif:** Veritabanında `bytea` olarak saklamak. Yedeklemesi kolay ama veritabanını
şişiriyor ve sorguları yavaşlatıyor, pratikte kaçınılıyor.

---

## 34. Dosya tipi hem uzantıdan hem içerikten doğrulanıyor

**Karar:** Yüklenen dosyanın uzantısı izin verilen listede mi diye bakılıyor, ayrıca ilk
dört baytı okunup gerçekten resim mi diye kontrol ediliyor.

**Gerekçe:** Uzantı kontrolü tek başına kandırılabilir — bir `.exe` dosyasını `kedi.jpg`
diye adlandırmak mümkün. Dosya tiplerinin başında karakteristik baytlar var (JPEG
`FF D8 FF`, PNG `89 50 4E 47`, WebP `RIFF`), bunlara bakmak gerçek içeriği doğruluyor.
İkisini birden yapmak ucuz: uzantı kontrolü anında, bayt kontrolü dört bayt okumak.

**Not:** Magic number okunduktan sonra `stream.Position = 0` ile akışı başa sarmak
gerekiyor, yoksa dosyanın ilk dört baytı kaybolur.

---

## 35. Dosya adları GUID ile üretiliyor

**Karar:** Kullanıcının gönderdiği dosya adı kullanılmıyor; `Guid.NewGuid()` ile yeni bir
ad üretilip sadece uzantı korunuyor.

**Gerekçe:** Üç sorunu birden çözüyor. İki kullanıcı aynı adı gönderirse çakışma olmuyor.
Dosya adında `../` geçerse sunucuda başka bir yere yazma denemesi (path traversal)
engelleniyor. Türkçe karakter, boşluk gibi şeyler URL'de sorun çıkarmıyor.

---

## 36. Fotoğraf silinince diskteki dosya da siliniyor

**Karar:** `DeleteAsync` veritabanı kaydını sildikten sonra `IFileStorageService.Delete`
çağrılıyor. Silme işlemi `try/catch` içinde, hata olursa yutuluyor.

**Gerekçe:** Dosya bırakılırsa zamanla kimsenin referans vermediği yetim dosyalar
birikiyor. Hatanın yutulma sebebi: dosya silinemese bile veritabanı kaydının silinmiş
olması daha önemli, tersi tutarsızlık yaratırdı.

**Not:** URL ile eklenmiş fotoğraflarda (dış adres) bu çağrı bir şey yapmıyor, `File.Exists`
false dönüyor. Zararsız.

---

## 37. Rezervasyon temizliği arka plan servisiyle yapılıyor

**Karar:** `ReservationCleanupService` adında bir `BackgroundService` belli aralıklarla
çalışıp süresi dolmuş siparişleri iptal ediyor, hayvanlarını `Available` yapıyor. Aralık
`OrderSettings:ReservationCheckIntervalSeconds` değerinden okunuyor.

**Gerekçe:** `ReservedUntil` alanı yazılıyordu ama kimse okumuyordu; süre dolsa bile
hayvan `Pending` kalıyordu. Veritabanı kendi kendine alan değiştirmediği için düzenli
çalışan bir şeye ihtiyaç var.

**Alternatif:** Okuma anında hesaplamak — `GET /api/Pet` çağrıldığında "bu hayvan
`Pending` ama süresi dolmuş, demek ki müsait" demek. Arka plan servisi gerektirmiyor ama
veritabanındaki `Status` yanlış kalıyor ve bu hesabı her sorguda tekrarlamak gerekiyor,
unutulan yerde tutarsızlık çıkıyor.

---

## 38. Süresi dolan sipariş de iptal ediliyor

**Karar:** Temizlik sadece hayvanları serbest bırakmıyor, siparişi de `Cancelled` yapıyor.

**Gerekçe:** Sadece hayvanlar serbest bırakılsaydı sipariş `Created` durumunda kalırdı ve
içindeki hayvanlar başkasına satılabilirdi. Kullanıcı ödemeye çalıştığında ne olacağı
belirsiz olurdu. İkisini birlikte iptal etmek tutarlı durum bırakıyor.

---

## 39. Arka plan servisinde scope elle açılıyor

**Not:** `BackgroundService` uygulama ömrü boyunca tek örnek olarak yaşıyor (singleton),
`DbContext` ise `AddScoped` ile kayıtlı. Singleton bir servise scoped bağımlılık
enjekte edilemiyor — ASP.NET Core bunu çalışma zamanında hata olarak bildiriyor.

Bu yüzden servise `IServiceScopeFactory` enjekte ediliyor ve döngünün her turunda elle
scope açılıyor:

```csharp
using var scope = _scopeFactory.CreateScope();
var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
```

Aynı durum `Program.cs`'teki seed çağrısında da var — orada da scope elle açılıyor.

**İki ayrıntı:** Döngü içindeki iş `try/catch` ile sarılı, bir hata olursa servis ölmesin
diye. `Task.Delay` çağrısına `stoppingToken` geçiriliyor ki uygulama kapanırken bekleme
hemen kesilsin, bir sonraki tura kadar beklemesin.
---

## 40. Şifreler BCrypt ile hash'leniyor

**Karar:** `User.PasswordHash` alanına düz metin yerine BCrypt hash'i yazılıyor.
Hashleme `IPasswordHasher` arayüzünün arkasında, implementasyonu
`Infrastructure/Security` altında.

**Gerekçe:** Düz metin şifre veritabanı sızdığında doğrudan kullanılabilir hale
geliyor. BCrypt kasten yavaş çalışıyor (varsayılan work factor 11), yani saldırgan
saniyede milyonlarca deneme yapamıyor. Ayrıca her çağrıda rastgele bir salt üretip
sonucun içine gömüyor — aynı şifreyi kullanan iki kullanıcının hash'i farklı oluyor,
veritabanına bakıp kimlerin aynı şifreyi kullandığı anlaşılamıyor.

Arayüz arkasına almamın sebebi: BCrypt üçüncü parti bir kütüphane, Domain katmanının
ona doğrudan bağımlı olmaması gerekiyor. `IFileStorageService` ile aynı desen.

**Not:** Mevcut test kullanıcıları düz metin şifre taşıdığı için hashleme eklenirken
silindi. Gerçek veriyle çalışan bir sistemde bu yol izlenemez; kullanıcıların bir
sonraki girişinde şifreyi yakalayıp yeniden hash'lemek gerekirdi.

---

## 41. Refresh token'lar hash'lenerek saklanıyor ve tek kullanımlık

**Karar:** `RefreshTokens` tablosunda token'ın kendisi değil SHA-256 özeti tutuluyor.
Token kullanıldığında `RevokedAt` doluyor ve yerine yenisi üretiliyor.

**Gerekçe:** Refresh token şifre gibi davranıyor — elinde olan kişi 7 gün boyunca yeni
access token üretebiliyor. Özet saklandığında veritabanı sızsa bile token geri
üretilemiyor.

Rotasyonun kazancı şu: token çalınırsa saldırgan onu bir kez kullanabiliyor, kullandığı
anda token iptal oluyor. Gerçek kullanıcı bir sonraki yenilemede 401 alıp yeniden giriş
yapmak zorunda kalıyor, yani sızıntı fark edilir hale geliyor. Aynı token 7 gün geçerli
kalsaydı saldırgan sessizce erişmeye devam ederdi.

**Neden BCrypt değil:** BCrypt her çağrıda farklı salt ürettiği için aynı girdi farklı
çıktı veriyor, yani `WHERE "TokenHash" = ...` diye arama yapılamazdı; tüm tabloyu çekip
tek tek doğrulamak gerekirdi. Ayrıca BCrypt'in yavaşlığı kısa ve tahmin edilebilir
şifreler için gerekli — 64 baytlık rastgele bir token zaten kaba kuvvetle bulunamaz.

**Not:** Satır silinmiyor, `RevokedAt` yazılıyor. Hangi token ne zaman üretildi ve ne
zaman iptal edildi görülebiliyor. Soft delete kararının aynı mantığı.

---

## 42. Email normalleştiriliyor ve tekil

**Karar:** Email'ler kaydedilmeden önce `Trim().ToLowerInvariant()` ile
normalleştiriliyor, `Users` tablosunda `Email` üzerinde unique index var.

**Gerekçe:** Normalleştirme olmasa `Ozgur@x.com` ile kayıt olan kişi `ozgur@x.com` ile
giriş yapamazdı, ve unique index ikisini farklı email sayardı. `ToLower()` değil
`ToLowerInvariant()` çünkü `ToLower()` makinenin kültürüne bakıyor ve Türkçe'de `I`
harfi `ı`'ya düşüyor — sunucunun dil ayarı değişince email'ler sessizce başka bir şeye
normalleşirdi.

**Soft delete ile etkileşim:** `User` soft delete ile siliniyor, satır tabloda kalıyor,
yani silinmiş bir kullanıcının email'i bir daha kullanılamıyor. Kısmi index
(`HasFilter`) ile email'i serbest bırakmak mümkündü ama tercih etmedim: aynı email'i
hem eski hem yeni hesabın paylaşması sipariş geçmişinde "bu kimin" sorusunu
karıştırıyor.

**Not:** Çakışma kontrolü `IgnoreQueryFilters()` ile yapılıyor, çünkü normal sorgu
silinmiş kullanıcıyı görmüyor. Görmeseydi kontrol "boş" derdi, veritabanı unique index
ihlali atardı ve istek 409 yerine 500 dönerdi.

---

## 43. Rol sunucu tarafında atanıyor

**Karar:** `RegisterRequest` DTO'sunda rol alanı yok; `AuthService.RegisterAsync` her
yeni kullanıcıya `UserRole.Customer` atıyor. Admin hesapları `POST /api/User` üzerinden
açılıyor ve o endpoint admin'e kapalı.

**Gerekçe:** Rol istekten gelseydi herkes kendini admin yapabilirdi. Alanı silmek
"kontrol etmeyi unutma" riskini de ortadan kaldırıyor — kontrol edilecek bir şey
gelmiyor.

**Not:** Bu bir tavuk-yumurta sorunu yaratıyor: ilk admin hesabını açmak için admin
token'ı gerekiyor. Seeder geliştirme ortamında bir admin kuruyor, ama `Species` tablosu
doluysa atlıyor. Üretimde ilk admin elle veritabanından açılmak zorunda. Kabul
edilebilir, çünkü alternatifi "ilk kaydolan admin olsun" gibi istismara açık bir kural.

---

## 44. Token'daki claim adları kısa tutuluyor

**Karar:** Access token'a `sub`, `email`, `jti` ve `role` claim'leri yazılıyor.
`JwtBearer` tarafında `MapInboundClaims = false`, `RoleClaimType = "role"`,
`NameClaimType = "sub"` ayarlandı.

**Gerekçe:** .NET varsayılan olarak claim adlarını eski WS-Federation URI'lerine
çeviriyor — `sub` yerine `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/
nameidentifier` gibi. Bu hem token'ı şişiriyor hem de okuma tarafında "hangi isimle
arayacağım" belirsizliği yaratıyor. Login projesinde `Me()` metodunda iki ayrı isimle
aramak zorunda kalmıştım, sebebi buydu.

`RoleClaimType = "role"` özellikle kritik: bu satır olmadan `[Authorize(Roles =
"Admin")]` hiçbir zaman eşleşmiyor ve hata da vermiyor, sadece sessizce 403 dönüyor.

**Not:** Token'ın içeriği şifreli değil, sadece Base64 ile kodlanmış — jwt.io'ya
yapıştıran herkes claim'leri okuyabiliyor. İmza gizliliği değil bütünlüğü sağlıyor.
Bu yüzden token'a hiçbir zaman hassas bilgi konmuyor.

---

## 45. Token doğrulama parametreleri açıkça yazılıyor

**Karar:** `TokenValidationParameters` içinde issuer, audience, ömür ve imza
doğrulamasının dördü de açık. `ClockSkew` 30 saniyeye çekildi.

**Gerekçe:** `ValidateIssuerSigningKey` dördünün en kritiği — kapalı olsa biri kendi
anahtarıyla imzaladığı, `role: Admin` yazan bir token üretip sisteme girerdi.
`ValidateLifetime` kapalı olsa 15 dakikalık ömrün hiçbir anlamı kalmazdı.

`ClockSkew`'in varsayılanı 5 dakika; yani varsayılan ayarla 15 dakikalık token aslında
20 dakika geçerli oluyor. Bu tolerans sunucu saatlerinin sapması için var, tek makinede
çalışırken gereksiz cömert.

**Not:** İmzalama anahtarı `appsettings.json`'da boş duruyor, gerçek değer
user-secrets'ta. `Program.cs` başlangıçta anahtarın var olduğunu ve en az 32 bayt
olduğunu kontrol edip yoksa uygulamayı hiç açmıyor. Eksik anahtar fark edilmeden
uygulamanın açılması, hatanın ilk giriş denemesinde anlaşılmaz bir mesajla çıkması
demek olurdu.

---

## 46. Rate limiting iki katmanlı

**Karar:** Email bazlı limit (5 dakikada 5 başarısız deneme) `AuthService` içinde,
kendi yazdığım `LoginAttemptLimiter` ile. IP bazlı limit (5 dakikada 20 istek)
middleware seviyesinde, ASP.NET Core'un `AddRateLimiter`'ı ile. İkisi de kayan pencere.

**Gerekçe:** İkisinin işi farklı. Email limiti **bir hesabı** çok denemeden koruyor,
IP limiti **bir saldırganı** çok hesap denemekten alıkoyuyor. Sadece email limiti
olsaydı saldırgan bin farklı email deneyip her birinde 5 hak kullanırdı.

Kayan pencere tercih edildi çünkü sabit pencerede sınırın hemen öncesi ve sonrası
birleştirilerek iki katı deneme sıkıştırılabiliyor.

**Not:** Email limiti sadece **başarısız** denemeleri sayıyor, IP limiti endpoint'e
gelen **her isteği**. Başarılı giriş hak harcamamalı; IP limiti ise isteğin sonucunu
bilmeden önce karar veriyor.

**Bilinen eksikler:**
- Sayaçlar bellekte, uygulama yeniden başlayınca sıfırlanıyor. Birden fazla sunucu
  çalıştırılırsa her biri kendi sayacını tutar. Gerçek çözüm Redis gibi paylaşılan bir
  depo, bu ölçekte gereksiz.
- `RemoteIpAddress` bir reverse proxy arkasında proxy'nin adresini gösterir, bütün
  kullanıcılar tek kovaya düşer. `X-Forwarded-For` okumak gerekir ama o başlık
  güvenilir bir proxy set etmiyorsa istemci uydurabilir.
- Kullanıcı bulunamadığında `Verify` çalışmadığı için cevap hızlı dönüyor, varsa BCrypt
  yüz milisaniye harcıyor. Yanıt sürelerini ölçen biri bu farktan email'in kayıtlı olup
  olmadığını çıkarabilir (zamanlama saldırısı). Çözümü sahte bir hash'e karşı doğrulama
  yapıp süreyi eşitlemek; rate limiter deneme sayısını beşe indirdiği için şimdilik
  kabul edilebilir.

---

## 47. Giriş hatalarında tek mesaj

**Karar:** "Kullanıcı yok" ve "şifre yanlış" durumları aynı `401` ve aynı mesajla
cevaplanıyor: "Invalid email or password."

**Gerekçe:** Ayırsaydım saldırgana hangi email'lerin sistemde kayıtlı olduğunu
söylemiş olurdum. Kayıtlı email listesi tek başına değerli bir bilgi, ayrıca saldırganın
hangi hesaba yoğunlaşacağını belirler. Buna kullanıcı sayımı (user enumeration)
deniyor.

Aynı sebeple `LoginRequest`'te şifre için uzunluk kuralı yok — girişte "şifre en az 8
karakter olmalı" demek sistemdeki kuralı açık etmek olurdu. Kurallar kayıt anında
uygulanıyor.

---

## 48. Yetkilendirme varsayılan kapalı

**Karar:** Controller sınıflarına en kısıtlayıcı kural yazılıyor, herkese açık
action'lara `[AllowAnonymous]` ekleniyor. Katalog okuma (`Species`, `Breed`, `Pet`,
fotoğraf listesi) herkese açık; yazma işlemleri ve `UserController`'ın tamamı admin'e;
sipariş ve favoriler girişli kullanıcıya.

**Gerekçe:** Tersini yapsaydım — sınıf açık, kısıtlı olanlara attribute — ileride yeni
bir action eklediğimde attribute koymayı unutursam o action korumasız kalırdı ve bunu
fark etmezdim. Bu yönde unutmanın sonucu "çalışmıyor" oluyor, "korumasız" değil.

**Not:** Action seviyesindeki `[Authorize]` sınıf seviyesindekinin yerine geçmiyor,
üstüne ekleniyor. `OrderController`'da sınıfta `[Authorize]`, listeleme action'ında
`[Authorize(Roles = "Admin")]` var — o action için iki koşul birden geçerli.

---

## 49. Sahiplik token'dan belirleniyor

**Karar:** `CreateOrderRequest`'ten `UserId` alanı kaldırıldı. Sipariş kimin adına
veriliyorsa o bilgi token'dan okunuyor. Controller'lar `User.ToCurrentUser()` ile
kimliği ve admin olup olmadığını servise taşıyor.

**Gerekçe:** `[Authorize]` sadece "giriş yapmış mı" diye soruyor, "bu veri senin mi"
diye sormuyor. Alan gövdede dururken giriş yapmış herhangi bir müşteri başkasının
id'sini yazıp onun adına sipariş verebiliyordu. Bu açığa "broken object level
authorization" deniyor ve gerçek dünyada en sık rastlanan API açığı.

Alanı silmek, kontrol etmekten daha iyi: kontrol edilecek bir şey hiç gelmezse unutma
ihtimali de olmuyor.

**Not:** Sipariş sahipliği veritabanındaki kayıttan okunuyor (`order.UserId`), o yüzden
kontrol servis katmanında. Favori sahipliği ise doğrudan rotada duruyor
(`/api/User/{userId}/favorites`), veritabanına gitmeye gerek yok, o yüzden kontrol
controller'da. Farklı yerlerde olmaları tutarsızlık değil, iki farklı kontrol türü.

---

## 50. Başkasının siparişinde `404`, başkasının favorisinde `403`

**Karar:** Kullanıcı kendisine ait olmayan bir siparişi istediğinde `404` dönüyor.
Kendisine ait olmayan bir favori koleksiyonunu istediğinde `403` dönüyor.

**Gerekçe:** Siparişte `403` dönmek "bu sipariş var ama senin değil" demek olurdu —
saldırgan id'leri 1'den başlayıp tarayarak sistemde kaç sipariş olduğunu, hangi
aralıkta id kullanıldığını öğrenebilirdi. `404` hiçbir şey sızdırmıyor: var olmayan
sipariş ile başkasının siparişi dışarıdan ayırt edilemiyor.

Favoride böyle bir sızıntı yok, çünkü kontrol veritabanına gitmeden önce çalışıyor —
`userId` ister var olsun ister olmasın cevap aynı `403`. Sızıntı olmadığına göre dürüst
cevabı vermek daha iyi.

---

## 51. Şifre değişimi ve kullanıcı silme token'ları iptal ediyor

**Karar:** `ChangePasswordAsync` ve `UserService.DeleteAsync` sonunda o kullanıcının
bütün aktif refresh token'ları iptal ediliyor.

**Gerekçe:** Şifre değiştirmek genellikle "hesabım tehlikede olabilir" demek. Eski
refresh token'lar aktif kalsaydı saldırgan 7 gün daha erişmeye devam ederdi, yani şifre
değiştirmenin bütün anlamı kaybolurdu. Silinen kullanıcı için de aynısı.

**Not:** Access token'lar imzalı oldukları için iptal edilemiyor; süresi dolana kadar
(15 dakika) geçerli kalıyorlar. Bu JWT'nin doğasında olan bir ödünç — token kendi
kendini doğruluyor, karşılığında merkezi olarak geri çağrılamıyor. 15 dakikalık pencere
kabul edilebilir bulundu.

---

## 52. Hata cevaplarının şekli

**Sorun:** Aynı `404` iki farklı şekilde dönebiliyordu. Controller `NotFound()`
döndürdüğünde gövde boş kalıyor, `UseStatusCodePages` dolduruyor ve cevapta `type` ile
`traceId` oluyordu, `detail` olmuyordu. Servis `NotFoundException` fırlattığında
`GlobalExceptionHandler` üretiyor ve cevapta `detail` ile `instance` oluyordu, `type`
olmuyordu. İkisi de geçerli Problem Details ama aynı API'de iki farklı şekil bulunması
istemci tarafında can sıkıcı.

**Çözüm:** `GlobalExceptionHandler` artık `ProblemDetails` nesnesini elle kurup
yazmıyor, `IProblemDetailsService.TryWriteAsync` çağırıyor. Böylece ASP.NET Core'un
varsayılan doldurma mekanizması devreye giriyor ve `type` ile `traceId` her iki yolda
da aynı şekilde dolduruluyor. `Instance` alanı elle set ediliyor, çünkü varsayılan
doldurma onu yazmıyor.

**Kalan fark:** `UseStatusCodePages` yolu `application/json`, handler yolu
`application/problem+json` content-type'ı ile cevap veriyor. Doğrusu ikincisi, ama
ikisi de ayrıştırılabildiği için peşine düşülmedi.

**Not:** `UseStatusCodePages` eklenmeden önce 401 ve 403 cevaplarının gövdesi tamamen
boştu — yetkilendirme middleware'i isteği `GlobalExceptionHandler`'a hiç uğratmadan
kesiyor.

---

## 53. API konteynerleştirildi, iki aşamalı build kullanıldı

**Karar:** `backend/Dockerfile` iki aşamalı. Birinci aşama SDK imajında derleyip
yayınlıyor, ikinci aşama sadece çalışma zamanı imajını kullanıp yayınlanmış çıktıyı
kopyalıyor. `docker-compose.yml`'a `api` servisi eklendi, `depends_on` ile veritabanının
healthcheck'i geçene kadar bekliyor.

**Gerekçe:** Tek aşamalı yazsaydım son imajda derleyici, kaynak kod ve NuGet önbelleği
de bulunurdu — SDK imajı bir gigabayt civarı, runtime imajı iki yüz megabayt civarı.
Ayrıca kaynak kodu dağıtılan imajın içinde taşımak gereksiz.

`.csproj` dosyalarının kaynak koddan **önce** kopyalanması Docker'ın katman önbelleği
için. Her `COPY` ve `RUN` bir katman üretiyor ve girdisi değişmeyen katman yeniden
çalıştırılmıyor; bu sırayla `dotnet restore` yalnızca bağımlılıklar değiştiğinde
tekrarlanıyor, her kod değişikliğinde değil.

**Yapılandırma:** Konteynerde user-secrets yok, o yüzden bağlantı dizesi ve JWT anahtarı
ortam değişkeniyle geçiliyor: `ConnectionStrings__DefaultConnection` ve `Jwt__Key`. .NET
çift alt çizgiyi iki nokta üst üsteye çeviriyor, çünkü çoğu kabuk ortam değişkeni adında
iki nokta kabul etmiyor. Değerler `.env` dosyasından okunuyor, o da `.gitignore`'da.

**Not:** Bağlantı dizesinde `Host=postgres` yazıyor, `localhost` değil. Konteyner
içinden `localhost` kendisi demek; compose her servise kendi adıyla bir ağ adı veriyor.

**Not:** Yerelde `dotnet run` 5087 portunda, konteyner 8080'de çalışıyor. İkincisi .NET
konteyner imajlarının varsayılanı. `FileStorageSettings__BaseUrl` de konteynerde
eziliyor, yoksa fotoğraf URL'leri 5087'yi gösterirdi.

---

## 54. Migration'lar uygulama açılışında uygulanıyor

**Karar:** `Program.cs` açılışta `context.Database.MigrateAsync()` çağırıyor. Seed ise
yalnızca geliştirme ortamında çalışmaya devam ediyor.

**Gerekçe:** Konteyner içinde `dotnet ef database update` çalıştırılamıyor — SDK ve
`dotnet-ef` aracı runtime imajında yok. Şemanın güncel olmasını sağlamanın pratik yolu
uygulamanın kendi kendine uygulaması. Böylece `docker compose up` tek başına çalışan bir
sistem getiriyor.

**Ödünç:** Gerçek üretimde migration'ı ayrı bir adımda çalıştırmak tercih ediliyor.
Birden fazla konteyner aynı anda açılırsa ikisi de migration uygulamaya kalkışır; EF Core
bunun için kilit aldığı için veri bozulmuyor ama açılış yavaşlıyor. Daha önemlisi, hatalı
bir migration'ın üretime kendiliğinden gitmesi istenmeyen bir şey. Bu ölçekte kabul
edilebilir.

---

## 55. Yüklenen dosyalar named volume'de

**Karar:** `docker-compose.yml`'da `uploads_data` adında bir volume tanımlandı ve
konteynerdeki `/app/wwwroot/uploads` yoluna bağlandı.

**Gerekçe:** Volume olmadan dosyalar konteynerin yazılabilir katmanında duruyor ve
konteyner silindiğinde gidiyorlar. `docker compose down` sonrası `up` yapınca yüklenen
bütün fotoğraflar kaybolurdu. Named volume konteynerin ömründen bağımsız yaşıyor;
`down -v` denmedikçe silinmiyor.

**Not:** `.dockerignore`'a `**/wwwroot/uploads/` eklendi — yerel makinedeki fotoğrafların
imaja kopyalanmasının anlamı yok, zaten volume devralıyor.

---

## 56. Bir EF Core uyarısı kasten susturuldu

**Karar:** `AddDbContext` içinde `ConfigureWarnings` ile
`PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning` yok sayılıyor.

**Gerekçe:** Uyarı her açılışta üç kez çıkıyordu: `Favorite → Pet`, `Order → User`,
`OrderItem → Pet`. Anlattığı durum madde 30'daki durum — sorgu filtresi olan bir
varlığa zorunlu navigation ile bağlanan bir varlık, `Include` ile yüklendiğinde `null`
alabiliyor, oysa tip "null olamaz" diyor.

Uyarı haklı ama her ilişki için ele alınmış: `FavoriteRepository`'de `!f.Pet.IsDeleted`
koşulu elle yazılı, `AuthService.RefreshAsync`'te `stored.User is null` kontrolü var.

**Gerekçe (susturma):** Sürekli görünen ve "normal" diye geçilen uyarılar zamanla gerçek
uyarıları da görünmez kılıyor. Tek bir olay kimliği susturuldu, diğer bütün uyarılar
açık kaldı.

**Uyarı:** Bu maddeyi okumadan susturulan uyarıya güvenme. Soft delete uygulanan bir
varlığa `Include` ile erişen **yeni** bir sorgu yazarsan EF Core artık seni uyarmayacak;
koşulu elle eklemen gerektiğini hatırlaman lazım.

---

## 57. Log seviyesi SQL ifadelerini gizleyecek şekilde ayarlandı

**Karar:** `appsettings.json`'da `Microsoft.EntityFrameworkCore.Database.Command`
kategorisi `Warning` seviyesine çekildi.

**Gerekçe:** EF Core geliştirme ortamında çalıştırdığı her SQL'i `Information` seviyesinde
yazıyor. `ReservationCleanupService` her 60 saniyede bir sorgu çalıştırdığı için ekran
sürekli doluyordu ve gerçek log satırları kayboluyordu. `Warning` seviyesinde sorgular
görünmüyor, hatalar görünmeye devam ediyor.

**Not:** Bir sorguyu incelemek gerektiğinde bu satırı geçici olarak `Information`'a
çekmek yeterli.

---

## 58. API sözleşmesi koddan üretiliyor

**Karar:** `docs/openapi.yaml` elle yazılmıyor; çalışan uygulamanın
`/swagger/v1/swagger.yaml` çıktısı dosyaya alınıp repoya konuyor.

**Gerekçe:** Ödev "OpenAPI-first" diyor, yani önce sözleşmeyi yazıp kodu ona uydurmak.
Ben tersini yaptım: kodu yazdım, sözleşmeyi koddan ürettim. İkisi arasındaki fark gerçek
— contract-first'te sözleşme kaynak, kod ona uymak zorunda ve istemci ekibi backend
bitmeden çalışmaya başlayabiliyor. Code-first'te sözleşme kodun bir çıktısı, yani kod
değişince sözleşme de habersizce değişiyor.

Bu projede tek başıma çalıştığım ve API'nin şeklini yazarken keşfettiğim için code-first
pratikti. Ama seçim bilinçli olsun diye buraya yazıyorum.

**Tuzak:** Üretilmiş dosya repoya girdiği an eskimeye başlıyor. Yeni bir endpoint
eklenip dosya yenilenmezse sözleşme yalan söylemeye başlıyor. README'ye yenileme komutu
yazıldı. Daha sağlamı CI'da dosyayı yeniden üretip repodakiyle karşılaştırmak, farklıysa
build'i kırmak — test ve CI adımında ele alınacak.

**Not:** Dosyada `servers` bloğu iki adres taşıyor, konteyner için 8080 ve yerel çalışma
için 5087. İstemci üreten araçlar taban adresini oradan okuyor.

---

## 59. Doğrulama kuralları şemaya bir filtre ile taşınıyor

**Sorun:** Doğrulama attribute'ları positional record'ların constructor
parametrelerinde duruyor (bkz. madde 22), çünkü ASP.NET Core model doğrulaması orada
arıyor. Swashbuckle ise şemayı property'lerden üretiyor ve attribute'ları hiç görmüyor.
Sonuç: doğrulama çalışıyor ama sözleşmede görünmüyordu — `RegisterRequest` için
`required` listesi, `format: email` ve `minLength: 8` yoktu.

Bu önemli bir eksik, çünkü sözleşmeden istemci üreten biri `password` alanının zorunlu
ve en az 8 karakter olduğunu bilemiyor.

**Denenen ve vazgeçilen yol:** Attribute'ları `[property:]` hedefiyle bir kez daha
yazmak. Derleniyor ama çalışmıyor — ASP.NET Core, constructor parametresiyle eşleşen bir
property'de doğrulama attribute'u görünce `InvalidOperationException` atıyor ve o
endpoint 500 dönüyor. Madde 22'de yaşanan tuzağın aynısı.

**Karar:** `RecordValidationSchemaFilter` adında bir `ISchemaFilter` yazıldı. Filtre her
şema için tipin birincil constructor'ını bulup parametrelerindeki `ValidationAttribute`
örneklerini okuyor ve karşılık gelen şema property'sine uyguluyor: `Required` zorunlu
alan listesine, `EmailAddress` `format: email`'e, `MinLength`/`MaxLength`/`StringLength`
uzunluk sınırlarına, `Range` sayısal sınırlara.

**Gerekçe:** DTO'lara hiç dokunulmuyor, doğrulama davranışı değişmiyor, sadece doküman
zenginleşiyor. Sorunu kaynağında değil, sorunun çıktığı yerde çözüyor.

**İki ayrıntı:**

Dizilerde `MinLength` `minItems`'a çevriliyor, `minLength`'e değil — JSON Schema'da
`minLength` string içindir, dizide yok sayılır. `CreateOrderRequest.petIds` için önemli.

Enum alanları paylaşılan şemalara `$ref` ile bağlandığı için filtre onların içini
değiştiremiyor; `[Required]` yine üst şemanın zorunlu alan listesine giriyor, yeterli.

**Kalan eksik:** Değer tipleri (`int`, `decimal`, `DateOnly`) `[Required]` taşımadığı
için `required` listesine girmiyorlar. `CreatePetRequest` gönderirken `breedId`
yazmazsan sözleşme buna izin veriyor, C# tarafında alan `0` oluyor ve servis katmanı
"böyle bir cins yok" diye 400 dönüyor. Zararsız ama sözleşme yanlış söylüyor. Doğru
çözüm bu alanları nullable yapmak, v1'de yapılmadı.