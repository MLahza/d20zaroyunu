# CHANGELOG

## 2026-11-09 — Gerçek Kart Görünümü ve Yelpaze Dizilimi

### Eklendi
- `CardView.cs`: Kart yüzü (isim bandı, türe göre renkli görsel alanı + tür etiketi, açıklama kutusu, güç çubuğu) ve kart arkası. Pozitif kartlar altın, negatif kartlar kırmızı çerçeve.

### Güncellendi
- `AutoSceneBuilder.cs`: Kartlar portre oranında (~0.7) ve çok parçalı yapıda üretilir (`BuildCardFace`). El ve rakip kartları yelpaze gibi hafif eğimli (±10°) ve kavisli dizilir, birbirine hafif biner.
- `CardDragDrop.cs`: Metin/renk yerine `CardView` kullanır; ev rotasyonunu saklar. Sürüklerken düzleşir, geri dönünce eğimini geri alır.
- `OpponentCardUI.cs`: `CardView` kullanır.

### Not
- **Tools > Setup Full Game Scene** menüsü yeniden çalıştırılmalı.

## 2026-11-09 — Kompakt Düzen, Rakip Kartları, Tut-Çek-Bırak D20

### Eklendi
- `OpponentCardUI.cs`: Rakip kartı (kapalı "?" / açık yüz).
- `D20Sprite.cs`: Çalışma anında üretilen D20 görseli (altıgen siluet + iç üçgen yüzey çizgileri).

### Güncellendi
- `AutoSceneBuilder.cs`: Slotlar ve kartlar küçültüldü, 5 sütun ortak hizada (rakip kartları üstte, slotlar ortada, oyuncu eli altta). Zar paneli büyütüldü.
- `DraftUIController.cs`: Sıra kimdeyse onun kartları açık, diğer tarafın kartları kapalı görünür. Rakip sırasında AI kartları rastgele slotlara dizer ve zarı kendisi atar. Sıra rakipteyken oyuncu zarı kilitli, uyarı "Rakip oynuyor...".
- `ThrowableDice2D.cs`: Sapan yerine fare/dokunmatikle zara basılı tutup sürükle-bırak; bırakma anındaki hız ile fırlar (çok yavaşsa atış sayılmaz). D20 görseli kullanılır. Rastgele `AddTorque`, duvarlardan sekme, durunca 1-20 sonuç ve slot tetikleme korunur. `AutoThrow()` yalnızca AI için.
- `MatchTurnManager.cs`: Başarısız atışta top karşı tarafa geçer ve tekrar geri döner (toggle); gol sonrası da sıra değişir.
- `SlotUI.cs`: AI kartını gösteren `aiText` / `SetAICard()`.
- `CardDragDrop.cs`: `ShowBack()`; snap yatay payı sabit 12 px.

### Not
- **Tools > Setup Full Game Scene** menüsü yeniden çalıştırılmalı.

## 2026-11-09 — "ZAR AT" Butonu Kaldırıldı

### Kaldırıldı
- `AutoSceneBuilder.cs` / `DraftUIController.cs`: "ZAR AT" butonu ve `rollButton` referansları. Zar yalnızca fare/dokunmatik ile geri çekip bırakarak (Drag & Throw) atılır.

### Güncellendi
- `AutoSceneBuilder.cs`: Zar alanı butonun boşalttığı yere genişletildi (DiceArea y: 0.22–0.84); `WarningText` panelin altına taşındı.
- Mevcut (önceki adımlardan): duvarlı `BoxCollider2D` zar alanı, `Rigidbody2D` + sekme özellikli `PhysicsMaterial2D`, `AddTorque`, durunca 1-20 sonuç + slot tetikleme (`ThrowableDice2D.cs`), 5 slot dolmadan zar kilidi (`SetLocked`) korunmuştur.

### Not
- **Tools > Setup Full Game Scene** menüsü yeniden çalıştırılmalı.

## 2026-11-08 — UI Düzeni ve Tur Hazırlık Kuralı

### Güncellendi
- `AutoSceneBuilder.cs`: Yerleşim yeniden düzenlendi (ortak sol kenar boşluğu, 5 eşit sütun + sabit boşluk; slotlar/kartlar/sonuç metni/zar paneli üst üste binmez). Zar paneline `WarningText` ve devre dışı renkli "ZAR AT" butonu eklendi.
- `CardDragDrop.cs`: Slota bırakınca kart 0.15 sn'lik SmoothStep animasyonuyla slot merkezine oturur ve ölçeklenir (üst etiket bandı simetrik boşlukla). Zar atılırken kart sürükleme kilitlenir (`DragLocked`).
- `SlotUI.cs`: `labelInset` yerine `labelFraction` / `InsetPixels` (çözünürlükten bağımsız).
- `MatchTurnManager.cs`: `AllSlotsFilled` özelliği eklendi.
- `ThrowableDice2D.cs`: `Locked` / `IsRolling` / `SetLocked()`; kilitliyken zar sürüklenemez/fırlatılamaz ve gri görünür.
- `DraftUIController.cs`: Her karede 5 slotun doluluğu denetlenir; boşsa zar + "ZAR AT" kilitli ve "Tüm slotlara kart yerleştirin!" uyarısı görünür, dolunca zar aktifleşir ve uyarı gizlenir.

### Not
- Yeni yerleşim için **Tools > Setup Full Game Scene** menüsü yeniden çalıştırılmalı.

## 2026-11-06 — Git Kurulumu

### Eklendi
- `.gitignore`: Unity standardı (Library/, Temp/, Logs/, UserSettings/, Obj/, Build/ vb. hariç).
- İlk commit: "d20zaroyunu" (69 dosya).

## 2026-11-06 — Hata Düzeltme

### Düzeltildi
- `DraftUIController.Start()` içindeki `NullReferenceException` (dice/slot/kart referansları atanmamış, eski sahne). `ResolveReferences()` eklendi: eksik referansları sahnede isimle/tipiyle otomatik bulur; bulunamazsa net hata mesajı verip kendini devre dışı bırakır (Tools > Setup Full Game Scene tekrar çalıştırılmalı).

## 2026-11-06 — Drag & Drop, Fizikli D20 Zar

### Eklendi
- `Assets/Scripts/CardDragDrop.cs`: Kart sürükle-bırak (IBeginDragHandler, IDragHandler, IEndDragHandler). Slota bırakılınca merkeze oturur (snap), geçersiz bırakmada ele döner. Slottaki kart tekrar sürüklenip geri alınabilir.
- `Assets/Scripts/ThrowableDice2D.cs`: Rigidbody2D + CircleCollider2D + sekme özellikli PhysicsMaterial2D'li zar. Zar paneli çevresine çalışma anında 4 adet BoxCollider2D duvar kurulur. Geri çek-bırak (sapan) ile fırlatma, rastgele `AddTorque`, hız < 0.1 olunca 1-20 sonuç üretip `MatchTurnManager.RollDiceAndExecute(int)` çağrılır.
- `CHANGELOG.md`: Bu dosya. Sonraki her adımda güncellenecek.

### Güncellendi
- `Assets/Scripts/SlotUI.cs`: Yeni oluşturuldu (IDropHandler). Boş slot kontrolü, `MatchTurnManager.PlaceCard` ile veri bağlama, vurgu rengi (yeşil/kırmızı).
- `Assets/Scripts/MatchTurnManager.cs`: `RollDiceAndExecute(int roll)` overload'u eklendi (parametresiz sürüm rastgele zarla bunu çağırır). `RemoveCardFromSlot(int)` eklendi.
- `Assets/Scripts/DraftUIController.cs`: Otomatik yerleştirme kaldırıldı; kartlar elle yerleştirilir. SlotUI/CardDragDrop/ThrowableDice2D entegre edildi. "ZAR AT" butonu rastgele fırlatma yapar.
- `Assets/Scripts/AutoSceneBuilder.cs`: Slot ve kartlara SlotUI/CardDragDrop/CanvasGroup eklendi; zar paneli "DiceArea" + fizikli zar objesi olarak yeniden düzenlendi.

### Mimari Notlar
- Fizik dünyası UI'dan ayrıdır: `DiceWorld2D` objesi (500,500) ofsetinde, 1 birim = 100 piksel ölçeğiyle simüle edilir; UI zarı her karede fizik gövdesini takip eder (Overlay Canvas piksel koordinatlarında Box2D hız limitlerine takılmamak için).
- Girdi, EventSystem pointer arayüzleri üzerinden işlenir (fare + dokunmatik, Input System uyumlu).

## 2026-11-05 — İlk Kurulum

### Eklendi
- `Assets/Scripts/CardEnums.cs`: AttackPhase, CardType, Possession enum'ları.
- `Assets/Scripts/CardData.cs`: ScriptableObject kart verisi.
- `Assets/Scripts/CardSlot.cs`: Slot verisi, `CardPlacementValidation()`, `ExecuteEffect()`.
- `Assets/Scripts/MatchTurnManager.cs`: Faz takibi, 5 slot, d20 mantığı, faz ilerletme/top değişimi.
- `Assets/Scripts/AutoSceneBuilder.cs`: `Tools/Setup Full Game Scene` menüsü ile otomatik Canvas/UI taslağı.
- `Assets/Scripts/DraftUIController.cs`: Test destesi, otomatik yerleştirme, slot vurgulama.
