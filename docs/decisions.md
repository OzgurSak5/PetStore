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