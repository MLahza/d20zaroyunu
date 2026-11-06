# CHANGELOG

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
