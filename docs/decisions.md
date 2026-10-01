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

**Karar:** Pet kayıtları fiziksel olarak silinmez, `Status` değeri
değiştirilir.

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