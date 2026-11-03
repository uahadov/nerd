# P01.1 — ortaq müqavilə, koordinatlar və versiyalı variantlar

Əsas qərarlar [GDD §4, §15–16](../NARD_Final_GDD_v1.0.md), əhatə
[Master Plan P01.1](../NARD_Master_Icra_Plani_v2.md). Mənbələr dəyişdirilmir.
Bu hissə sənəd və gözlənilən test məlumatıdır; qanuni gediş motoru yoxdur.

## Koordinat müqaviləsi — board-global-v1

Xanalar sabit global 0..23 indekslidir; ekran çevrilməsi indeksləri dəyişmir.
Bu, texniki adlandırmadır, yeni nərd variantı deyil. İnsanların 1..24 nömrələri ilə qarışdırma.

```text
yuxarı sıra: 12 13 14 15 16 17 | 18 19 20 21 22 23
aşağı sıra:  11 10  9  8  7  6 |  5  4  3  2  1  0
```

| Rejim | P0 yolu | P1 yolu | P0 evi | P1 evi |
|---|---|---|---|---|
| Uzun | 0→1→…→23→off | 12→…→23→0→…→11→off | 18..23 | 6..11 |
| Qısa | 23→22→…→0→off | 0→1→…→23→off | 0..5 | 18..23 |

Uzun nərddə şəxsi irəliləyiş q=(global−baş+24)%24, baş P0=0/P1=12-dir.
q=0 başlanğıc, q=18..23 ev, q=24 yalnız off-dur; ikinci dövrə yoxdur.
Qısa nərddə evdəki off məsafəsi P0 üçün global+1, P1 üçün 24−global-dır.
Uzunda off məsafəsi 24−q-dur. Bar/off 24 xananın içində nömrələnmir.
`points[24]`: müsbət P0, mənfi P1, sıfır boş; eyni xanada qarışıq sahiblik yoxdur.
`bar[2]` və `off[2]` ayrı sayılır; hər oyunçu üçün taxta+bar+off=15 invariantdır.
Uzun nərddə bar həmişə [0,0]-dır.

## Ortaq qayda matrisi

| ID | Şərt / tələb | Sərhəd |
|---|---|---|
| C-BOARD | 24 xana, 15+15 daş, itmə/çoxalma yoxdur | İlkin və bütün before/after nümunələri |
| C-DIRECTION | Yalnız şəxsi yol üzrə irəli; uzun/qısa yollar yuxarıdadır | Ekran orientation-dan asılı deyil |
| C-DICE | Hər zər ayrı token; qoşa dörd token | İstifadə olunmuş token təkrarlanmır |
| C-MAX | Tam növbə mümkün maksimum sayda token işlətməlidir | Əvvəl bütün qanuni ardıcıllıqlar nəzərə alınır; greedy ilk seçim kifayət deyil |
| C-HIGH | Maksimum 1-dirsə və hər iki qiymət alternativ mümkün olursa yüksək seçilir | Yalnız kiçik mümkündürsə kiçik; uzun konkretliyi L-HIGH qapısındadır |
| C-INTERMEDIATE | Eyni daş iki zəri ardıcıl işlədə bilər; hər zərin son dayanacağı qanuni olmalıdır | Bir zərin addımı içindəki xanalardan tullanmaq olar; iki zər birləşdirilib bağlı dayanacaq atlanmır |
| C-OPEN | Bərabər başlanğıc zərləri yenidən atılır | Uzun/qısa ilk istifadə olunan qiymətlər fərqlidir |
| C-UNDO | Yalnız öz, hələ yekun təsdiqlənməmiş növbəsi geri alınır | Daş, bar, off, token və baş sayacı birlikdə snapshot-dan bərpa olunur |
| C-SERIES | Best of 3 üçün 2, Best of 5 üçün 3 partiya qələbəsi | Mars/koks bir partiyadır; ayrıca nəticə xalı var |
| C-RATING | Reytinqə yalnız təsdiqli ranked random nəticəsi uyğundur | Bot/özəl/casual dəyişmir; eligibility server nəticəsinin etibarlılığını əvəz etmir |

İki zər atılıbsa token slotları 0,1-dir; qoşada slotlar 0,1,2,3 eyni qiymətlidir.
`used` artıq işlənmiş slotları saxlayır. Heç bir token qanuni deyilsə max=0 və boş turn təklifidir.
Bir və ya bir neçə token mümkün olduqda boş turn/pass rədd edilir.
Oyun bitənədək maksimum istifadə axtarılır; 15-ci daş çıxandan sonra əlavə hərəkət yoxdur.
Zər RNG-si və online son təsdiq bu hissədə yazılmır; GDD-yə görə serverin səlahiyyətidir.

## RuleSetVersion və PROVISIONAL_RULE qeydiyyatı

| Versiya | Məna | İnsan yoxlaması |
|---|---|---|
| long-az-gdd1-provisional.1 | GDD seçimləri və aşağıdakı açıq yerli variant qapıları | PENDING |
| short-gdd1-draft.1 | GDD standart qısa nərd, doubling cube olmadan | PENDING |

| ID | Seçilmiş nümunə qaydası / dəqiqləşdirmə | Status |
|---|---|---|
| L-OPEN | Böyük ilkin tək zər atan yenidən iki zər atır; ilkin iki qiymətlə başlamır | PROVISIONAL_RULE — GDD seçimi, yerli yoxlama |
| L-HEAD-FIRST | Hər oyunçunun ilk şəxsi növbəsində 3/3, 4/4, 6/6 üçün iki baş daşı; sonrakı növbədə yox | PROVISIONAL_RULE — GDD seçimi, yerli yoxlama |
| L-BLOCK | Altı ardıcıl tutulan xana üçün ən azı bir rəqib daşı blokdan irəlidə olmalıdır | PROVISIONAL_RULE — GDD ümumi məhdudiyyətinin dəqiq layihə draftı |
| L-BEAR | Hamısı evdə olmalıdır; dəqiq qiymət və ən uzaq daş üçün overshoot | PROVISIONAL_RULE — nümunələşdirilmiş yerli yoxlama |
| L-HIGH | Maksimum bir hərəkətdə hər iki qiymət alternativ mümkündürsə yüksək zər | PROVISIONAL_RULE — variant konkretliyi yerli yoxlama |
| L-MARS | Məğlub heç daş çıxarmayıbsa mars, əks halda adi | PROVISIONAL_RULE — kriteriya yerli yoxlama; 1/2 xal və koksun olmaması GDD-də sabitdir |

L-BLOCK draftında rəqibin şəxsi yolu q=0..23 üzrə altı ardıcıl xana q=a..a+5
(a=0..18) götürülür. Ən azı bir rəqib daşı q>a+5 olmalıdır. Global 23→0 döngəsi
rəqibin şəxsi yolunun daxilindədirsə sayılır; şəxsi finişdən başa ikinci dövrə sayılmır.
“Hamısını bağlama”nı yalnız cari zərlərdə heç gediş tapılmaması kimi yoxlamaq yetərli deyil.
Bu dəqiqləşdirmə insan tərəfindən hələ kilidlənməyib; istehsal qaydası kimi təqdim edilmir.
Baş, ilk atış, altılıq, evdən çıxış və mars ən azı bir neçə yerli təcrübəli oyunçu ilə
beta öncəsi yoxlanmalıdır. Fərq varsa yeni versiya/qərar qeydi lazımdır, səssiz dəyişiklik yox.

Xarici müqayisə yalnız L-BLOCK/baş/çıxış nümunələrini yoxlamağa kömək edir;
layihənin əsas mənbəyini və yerli qapını əvəz etmir. [Nardy Union qaydaları](https://nardyunion.com/en/learn/long-nardy-rules).
Doubling cube, avtomatik doubling, mərc və xarici turnir opsiyaları layihəyə daşınmır.

## Nümunə formatının sərhədi

[JSON Schema](rules/fixtures.schema.json), [tam nümunələr](rules/fixtures.json),
[oxunaqlı matris](RULE_MATRIX.md), [uzun](RULES_LONG.md), [qısa](RULES_SHORT.md).
`before`/`expected.after` konkret 24-xanalı vəziyyətdir; saylar sintetik nümunələrdir,
başlanğıcdan həmin mövqeyə həqiqətən çatıldığı iddiası yoxdur.
`phase`/`committed` fixture kontekstidir; gələcək runtime sinif/API adlarını təyin etmir.
`move` yerli tək-addım şərtini, `turn` bütün təklifin maksimum zər istifadəsini yoxlamağı təsvir edir.
Yerli addımın ACCEPT olması onun tam qanuni növbədə mütləq saxlanıla biləcəyi demək deyil:
gələcək motor tam ardıcıllıq üçün C-MAX/C-HIGH yoxlamasını da tətbiq etməlidir.
ACCEPT/REJECT **gözlənilən** cavabdır, mövcud motordan alınmış nəticə deyil.
Schema validatoru sahə/tip, say invariantı, istinad və müəllifin göstərdiyi keçidlərin
arifmetikasını yoxlayır; qanuni gedişləri yaratmır, strateji seçim etmir və RNG işləmir.
`rating_policy` yalnız eligibility şərtidir; həqiqi reytinq yenilənməsi deyil.
`series` siyahısı yekun təsdiqlənmiş partiya nəticələrini fərz edir.

Provisional ID-lər də C-/L-/S- prefiksli tələb inventarına daxildir: cəmi 23 ID.
Altılıq, baş, maksimum zər, istiqamət və çıxarma qanuniliyinin tam hesablanması
bu checker-in işi deyil; P01.2/P01.3 motor testlərində icra ediləcək.

## Fixture sahələrinin dəqiq mənası

| Sahə | Müqavilə |
|---|---|
| points, bar, off | Yuxarıdakı işarəli global xana və oyunçu sıralı iki say; dəyişməz 15+15 invariantı |
| roll | Cari növbənin iki qiyməti; başlanğıc zəri atılmamış və nəticə nümunələrində [] |
| used | Cari roll-un artıq işlənmiş unikal slotları; boş roll-da boşdur |
| to_move | P0=0/P1=1; yalnız opening mərhələsində −1 |
| first_turn | Hər oyunçunun ilk şəxsi növbəsi hələ bitməyibsə true; növbə commit-ində false olur |
| head_departures | Cari növbədə hər oyunçu üçün başdan götürülmüş daş sayı; növbə başlananda sıfırlanır; qısada [0,0] |
| phase | opening → await_roll/moving → committed; son daş çıxarsa finished. Bu part state machine implementasiyası deyil |
| committed | Yalnız phase=committed üçün true; bu nöqtədə undo qadağandır |
| expected.max_used | Before vəziyyətindən qalan tokenlərlə mümkün maksimum əlavə hop sayı; used-dəki keçmiş tokenlər daxil deyil |
| expected.after | Təklif qəbul edilsə göstərilən vəziyyət; rədd edilsə before ilə tam eynidir |
| expected.reason | Sabit nümunə səbəb etiketi; UI mətni və gələcək API xəta modeli deyil |
| assurance | GDD_ALIGNED mənbəyə uyğun draft nümunəsi, insan qəbulu deyil; PROVISIONAL_RULE ayrıca yerli qapıdır |

`start` hazır ilkin mövqenin read-only nümunəsidir. `opening` ilkin tək zərləri qəbul edir.
`move`/`turn` göstərilmiş tokenləri işlədib before→after nümunəsini verir; to_move dəyişmir,
turn commit-i avtomatik baş vermir. Son daş çıxanda phase=finished olur.
`undo.snapshot_fixture` eyni rejimdə əvvəlki qəbul nümunəsinə istinaddır: onun after-ından
before-ına qayıdır; real oyunda tarixçe/snapshot saxlanması sonrakı motorun məsuliyyətidir.
`classify_result` hazır finished mövqenin qalib/növ/xal gözləntisidir, taxtanı dəyişmir.
`series` və `rating_policy` taxtanı dəyişməyən nəticə siyasəti nümunələridir;
onların before taxtası yalnız ortaq format konteyneridir, seriya tarixçəsinin son taxtası deyil.
Sıralı winners/win_kinds elementləri eyni partiyaya aiddir; seriya bitəndən sonra element olmaz.

Bu partda taymerin bitməsi, disconnect, təslimolma və server nəticəsinin etibarlılığı
üçün yeni qayda icad edilmir; bunlar uyğun sonrakı partın siyasətinə tabedir.
