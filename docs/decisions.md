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