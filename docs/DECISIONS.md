# Qərarlar və fərziyyələr

Məhsul qərarları üçün [GDD](../NARD_Final_GDD_v1.0.md), ardıcıllıq üçün
[Master Plan](../NARD_Master_Icra_Plani_v2.md) əsasdır. Bunların məzmunu dəyişdirilməyib.
Hər yeni qeyd: ID, tarix, növ, əsas, nəticə, təsir və təsdiq vəziyyəti daşımalıdır.

| ID / tarix | Növ və əsas | Qərar / nəticə |
|---|---|---|
| D001 / 2026-11-03 | İstifadəçinin P00.2 göstərişi | Agentdən/modeldən asılı olmayan repo. GDD-də Antigravity sahiblik/prioritet qeydləri məcburi alət tələbi yaratmır. Məhsul qərarları dəyişmir. |
| D002 / 2026-11-03 | İstifadəçinin açıq qadağası | Bütün Skill-lər, Antigravity MCP, Codex Gravity MCP və Antigravity ilə əlaqəli MCP-lər qadağandır. Adi Git/terminal/redaktə icazəlidir. |
| D003 / 2026-11-03 | İstifadəçinin status tələbi | Master Plandakı WAITING_FOR_HUMAN_REVIEW bu repoda AWAITING_HUMAN_APPROVAL kimi saxlanır; mənası eyni gözləmə qapısıdır. SELF_TEST_PASSED insan təsdiqi deyil. |
| D004 / 2026-11-03 | P00.2 daxilində sənəd quruluşu | Orijinal iki mənbənin adı və baytları qorunur. GDD.md / MASTER_PLAN.md surətləri yaradılmır; README dəqiq yol xəritəsidir. |
| D005 / 2026-11-03 | İstifadəçinin bu çatdakı açıq mesajı | P00.1 insan tərəfindən qəbul edilib; yalnız P00.2 icazəlidir. P00.3-ə icazə yoxdur. |
| D006 / 2026-11-03 | İstifadəçinin cavabı | Commit müəllifi Ulvi; təqdim edilmiş e-poçt lokal Git config-də və commit müəllif metadatasındadır. Uzaq ünvan https://github.com/uahadov/nerd.git. Görünüş və push nəticəsi STATE və hesabatda faktiki qeyd olunur. |
| D007 / 2026-11-03 | Agentin P00.2 struktur seçimi; insan baxışına təqdim edilir | ARCHITECTURE modul sərhədlərini sənədləşdirir. Nə yeni xidmət, nə oyun kodu, nə əvvəlcədən Unity/SDK quraşdırması əlavə edilir. |

## Təsdiqlənməmiş / gələcəkdə ölçüləcək

- GDD-də 30/90 saniyə, beta/KPI və 60 FPS rəqəmləri hədəf/fərziyyədir.
- Uzun nərdin yerli variantları P01.1 və beta qapısında doğrulanacaq; RuleSetVersion tələb olunur.
- P00.1-də tapılan Unity versiyası bu partda layihə versiyası kimi sabitlənmir.
- Android alətləri çatışmır; bu partda quraşdırılmır. iOS build/device sınağı aparılmayıb.
- 1 GB VPS uyğunluğu ölçülməyib. İstehsal CockroachDB seçimi saxlanır;
  lokal PostgreSQL yolu yalnız gələcək rəsmi uyğunluq yoxlaması ilə development üçündür.

## D008 — 2026-11-03, uzaq repository

Git ls-remote oxuması uğurludur və 0 ref göstərir. GitHub açıq metadata cavabı
private=false, visibility=public göstərdi. İstifadəçi “davam et sonrada private edərəm”
bildirdi. Bu, lokal P00.2-nin davamı kimi tətbiq edildi; public remote-ə push edilmədi.
Private uzaq nüsxə açıq məsələdir və P00.2-nin tam qəbulundan əvvəl ayrıca həll edilməlidir.

## D009 — 2026-11-03, yalnız P00.2 yekun auditi

Əsas: istifadəçinin açıq mesajı — “İndi yalnız P00.2-nin yekun auditini apar.”
Status AWAITING_HUMAN_APPROVAL qalır; P00.2 qəbulu və P00.3 icazəsi verilməyib.
İstifadəçi mövcud GitHub repository-sinə push etməyi qadağan edib; gələcəkdə yalnız
private repository istifadə ediləcək. Əvvəlki “sonra private edərəm” qeydi avtomatik
push icazəsi deyil. Yekun audit dəqiqləşdirmələri məhsul qərarlarını dəyişmir.

## D010 — 2026-11-03, P00.2 qəbulu və yalnız P00.3 icazəsi

İstifadəçi: “P00.2 mərhələsini təsdiqləyirəm. İndi yalnız P00.3 mərhələsinə keç.”
Unity 6000.6.7f1 istifadə olunur; rəsmi pulsuz Android alətləri quraşdırılması
və real minimal layihə/build/test icazəlidir. Oyun funksiyaları və UI/UX ekranları yoxdur.
P00.2 qəbulu açıqdır; private uzaq nüsxə açıq məsələ olaraq qalır. D008-dəki qəbuldan
əvvəl private backup gözləntisi son insan qərarı ilə təxirə salınıb; push icazəsi verilmir.
Status sonda AWAITING_HUMAN_APPROVAL qalır. Master Planda P00.3-dən sonra P01.1 gəlir;
P00.4 yoxdur və heç bir sonrakı part icazəli deyil. GitHub push qadağası qüvvədədir.

## D011 — 2026-11-03, P00.3 texniki konfiqurasiya

Unity 6000.6.7f1, Test Framework 1.8.0 və Input System 1.20.1 sabitlənir.
Rəsmi Editor-un createProject yolu ilə lokal layihə yaradılır; Unity Cloud/GitHub əlaqəsi qurulmur.
Minimal built-in 2D ortoqrafik kamera və krem fon texniki smoke səhnəsidir; art/UI məhsul qərarı deyil.
Android development APK üçün ARM64/IL2CPP, min API26 (Editor default minimum), target API36 seçilir;
bunlar ilkin sınaq konfiqurasiyasıdır, final cihaz matrisi deyil. Android alətləri rəsmi
Unity CLI-nin bu Editor versiyası üçün seçdiyi modullardır; qlobal sistem JDK/SDK parametrləri dəyişdirilmir.

## D012 — 2026-11-03, Editor vizual yoxlaması və lokal ehtiyatlar

İlk GUI preview köməkçisi PlayMode domain reload zamanı callback-i itirirdi;
SessionState vasitəsilə yalnız açıq preview sorğusunun davamı saxlanıldı.
Təkrar açılış əvvəlki proses bağlanmadan başladığı üçün duplicate-project dialoqu yarandı.
Yalnız agentin bu layihə üçün başlatdığı proseslər bağlandı, köhnə yaradılmış Temp lock
build/P00.3 daxilinə qorunaraq köçürüldü. Heç bir mənbə və Git tarixçəsi silinmədi.
Unity recovery dialoqu üçün istifadəçiyə Keep Backups tövsiyə edildi; _Recovery ehtiyatları
lokal saxlanır və Git-ə daxil edilmir. GUI ilk açılışında Unity SearchDatabase daxilində
ArgumentOutOfRangeException qeydə alındı; yekun retry nəticəsi part hesabatında ayrılır.

## D013 — 2026-11-03, GUI retry nəticəsi

Rəsmi UnityCsReference SearchDatabase.SetupDefaultIndexFirstUse mənbəsində PlayMode-a
keçid vaxtı ilkin index qurulmasının buraxıldığı görüldü. Eyni anda UserSettings/Search.index
yox idi və erkən avtomatik Play tələbi verilirdi. Preview başlanğıcı Editor ilkin
yeniləmələri bitəndən sonraya gecikdirildi; normal Editor API ilə bağlanıb təmiz açıldı.
Son açılışda 25 kadrdan sonra yeni screenshot alındı, C# error və exception sayı 0 oldu.
Bu, müşahidə ilə yoxlanmış lokal həll yoludur; Garuda/Intel üçün ümumi rəsmi dəstək iddiası deyil.
Mənbə: https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/QuickSearch/Editor/Indexing/SearchDatabase.cs
