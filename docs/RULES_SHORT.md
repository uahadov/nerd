# Qısa nərd — P01.1 qayda müqaviləsi

Əsas [GDD §4.1, §4.3–4.4, §15–16](../NARD_Final_GDD_v1.0.md),
[ortaq koordinatlar](RULESET_VARIANTS.md). Versiya: **short-gdd1-draft.1**.
Sənəd və nümunələrdir; P01.3 qayda motoru yazılmayıb.

## Başlanğıc və istiqamət — S-START, C-DIRECTION

| Oyunçu | 2 daş | 5 daş | 3 daş | 5 daş | Yol / ev |
|---|---|---|---|---|---|
| P0 | 23 | 12 | 7 | 5 | Azalan indeks; ev 0..5 |
| P1 | 0 | 11 | 16 | 18 | Artan indeks; ev 18..23 |

Bu, hər oyunçunun öz nömrələməsində 24/13/8/6 düzülüşünün 0-based global qarşılığıdır.
Hər tərəfdə cəmi 15 daş, bar/off sıfırdır. S_START tam 24 xananı saxlayır.
P0 və P1 yolları əksdir; ekranın portret/landşaft olması bunu dəyişmir.

## Başlanğıc, zərlər və məcburi istifadə — C-OPEN, C-DICE, C-MAX, C-HIGH

Hər kəs bir zər atır; böyük atan **həmin iki ilkin qiymətlə** başlayır, təzədən atmır.
Bərabərlikdə təkrar atılır; ilkin ilk gedişdə qoşa buna görə yaranmır.
Adi qoşa dörd ayrı token verir. Bir daş müxtəlif tokenləri ardıcıl istifadə edə bilər;
hər dayanacaq qanuni olmalıdır. Bir zərin hop-u arasındakı fiziki xanalardan tullanmaq olar.
Tam turn mümkün maksimum token sayını işlətməlidir. Maksimum birdirsə və hər iki
qiymət ayrı-ayrılıqda mümkündürsə yüksək seçilir; yalnız kiçik mümkündürsə kiçik oynanır.
Heç biri mümkün deyilsə boş turn qəbul olunur. Məcburi sıra “həmişə böyük əvvəl” deyil.
S_ORDER_BOTH, S_MAX_REQUIRED, S_LOW_ONLY və S_HIGH_* nümunələrinə bax.

## Vurma və bar — S-HIT, S-BAR

Rəqibin tək daşı vurulur, bar-a keçir. İki və çox daş hədəfi bağlayır.
Öz daşlarını eyni xanada yığmaq olar. Bar-da öz daşı varsa əvvəl onu daxil etmək məcburidir.
P0 die d üçün global 24−d, P1 üçün d−1 xanasından girir.
Giriş tək rəqib daşını vura bilər; bağlı girişdən keçmək olmaz.
Bar boşalmadan başqa daş hərəkət etmir. Bar tam boşalarsa qalan tokenlər işlənməlidir;
yalnız bir hissəsi daxil ola bilirsə o hissə oynanır və qalan hüquq bloklanır.
S_BAR_FULL_BLOCK, S_BAR_PARTIAL, S_BAR_ENTER_P0/P1 və S_BAR_HIT nümunələridir.

## Evdən çıxarma — S-BEAR

Bütün aktiv daşlar evdə, bar boş olmalıdır. Vurulub evdən kənara qayıdan daş olduqda
çıxarma dayanır və həmin daş evə gətirilməlidir.
P0 məsafəsi global+1, P1 məsafəsi 24−global-dır. Dəqiq qiymət uyğun daşı çıxarır.
Artıq qiymətlə çıxarma yalnız daha uzaq öz daşı yoxdursa mümkündür; o halda ən uzaq
mövcud daş çıxır. Evdaxili alternativ gediş varsa onu seçmək olar, amma tam turn
C-MAX/C-HIGH şərtlərini ödəməlidir. S_BEAR_EXACT/S_OVERSHOOT_*/S_NOT_ALL_HOME bunu göstərir.
Bu zər/çıxış detallarının yoxlama mənbəyi [USBGF qaydalarıdır](https://usbgf.org/backgammon-basics-how-to-play/);
layihənin mərc/cube qadağası həmin mənbənin digər bölmələrindən üstündür.

## Adi, mars, koks — S-SCORE

Qalib ilk 15 daşı çıxarandır. Məğlub ən azı bir daş çıxarıbsa adi nəticə: 1 xal.
Məğlub off=0, bar=0 və qalibin evində daşı yoxdursa mars: 2 xal.
Məğlub off=0 və bar-da **və ya** qalibin evində ən az bir daşı varsa koks: 3 xal.
Off>0 şərti hətta bar/evdə daş qalanda da nəticəni adiyə qaytarır.
S_MARS, S_KOKS_BAR, S_KOKS_HOME və S_OFF_ONE_PREVENTS_KOKS sərhədləridir.

Doubling cube, automatic doubles, Jacoby/Crawford kimi əlavə turnir variantları və mərc
bu layihəyə əlavə edilmir. Best of 3/5-də mars/koks bir partiya qələbəsi olaraq qalır.
Server təsdiqindən sonra undo yoxdur; bar-a vurulan daş da lokal undo snapshot-u ilə bərpa edilir.

İnsan yoxlaması: [tam nümunə matrisi](RULE_MATRIX.md). Bu, JSON gözləntilərinin insan baxışıdır;
automatik checker-dən PASS almaq real motorun gedişləri düzgün oynatdığı demək deyil.
