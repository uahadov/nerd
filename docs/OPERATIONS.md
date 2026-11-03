# İşin davamı, təhlükəsizlik və xərc

## Status və insan təsdiqi

[STATE](STATE.json) əsas maşın statusudur; [PROGRESS](../PROGRESS.md) onun qısa insan görünüşüdür.
`schema_version` formatı versiyalaşdırır; onu dəyişəndə validatoru da yenilə.

İcra axını: PLANNED → IN_PROGRESS → SELF_TEST_FAILED / SELF_TEST_PASSED
→ AWAITING_HUMAN_APPROVAL → HUMAN_APPROVED.
İnsan düzəliş istəsə REWORK_REQUIRED → IN_PROGRESS, eyni part daxilində qalır.
Cari status AWAITING_HUMAN_APPROVAL olduqda agent özünü təsdiqləyə bilməz.
Master Planın WAITING_FOR_HUMAN_REVIEW adı üçün yalnız bu alias istifadə olunur.

`authorization` başlanmağa icazənin kimdən və hansı açıq sözlə gəldiyini saxlayır.
`human_approvals` yalnız faktiki qəbul edilmiş partları göstərir; təsdiqlənməmiş cari part daxil edilmir.
Cari partın qəbulu gəlsə onun qeydi və uyğun boolean/status yenilənir; bu, növbəti partı açmır.
Hazırkı validator yalnız P02.3-ün təsdiq gözləyən snapshot-unu yoxlayır; gələcək insan
qəbulu/status dəyişikliyi zamanı müvafiq yoxlama sərhədi də açıq icazəyə uyğun yenilənməlidir.
`current_part_human_approved` və `next_part_authorized` ayrı bool sahələrdir.
Qəbul və növbəti iş icazəsi hər ikisi insanın açıq sözünə söykənməlidir.
Növbəti icazə gələndə əvvəl cari qəbulu tarix və dəqiq sitatla əlavə et,
sonra active_part və authorization-u yeni parta keçir; tarixçəni silmə.
Status faylını əvvəl müvəqqəti fayla yazıb JSON kimi yoxla, sonra atomik əvəz et.
Söhbət identifikatoru və ya xüsusi agent yaddaşı məcburi deyil.

## Ötürmə və yarımçıq iş

1. AGENTS, STATE, PROGRESS, cari hesabat, mənbələr və aidiyyəti sənədləri oxu.
2. Git status və son commitləri yoxla; dəyişiklik varsa kimə aid olduğunu müəyyən et.
3. `open_items` hər məsələ üçün ID, status, təsir və növbəti əməl saxlayır.
4. `resume_instruction` agentə icazəli davam nöqtəsini deyir. İcazə yoxdursa dayan.
5. İcra zamanı qalan işləri hesabatda yaz; yalnız faylları görüb test edilmiş sayma.

## Git və uzaq nüsxə

Mövcud repo varsa init/reset/force əməliyyatı ilə əvəz etmə. Lokal müəllif konfiqurasiyası istifadə olunur.
Yeni uzaq ünvan istifadəçi tərəfindən verilməlidir. Fetch əvvəl, history yoxlaması sonra;
remote-də tarixçə varsa əvvəl uzlaşdır, üzərinə yazma. **Hazırkı origin-ə push qadağandır.**
`repository.push_authorized` false qalır; private görünüş avtomatik icazə yaratmır.
Gələcək push üçün ayrıca açıq insan icazəsi, private görünüş yoxlaması
və məzmun baxışı lazımdır. İcazəli push yalnız görünüş private kimi yoxlanandan və məzmun secret/cache baxımından yoxlanandan sonra, force olmadan edilir.
Push-un nəticəsini real commit SHA ilə yoxla. Push uğuru insan qəbulu deyil.
Qəbul tag-ı insan təsdiqindən sonra; P00.2, P00.3, P01.1 və P01.2 və P01.3 qəbul edilib; P01.4 qəbul edilib; P02.1 qəbul edilib; P02.2 qəbul edilib; P02.3 hələ qəbul edilməyib.
İlk commitdə müəllif e-poçtu Git metadata-sına daxil olur; token heç vaxt daxil olmur.

## Təhlükəsizlik

.env, signing key, token, mağaza receipt-i, şəxsi cihaz ID-si, tam istifadəçi logu Git-də saxlanmır.
.gitignore izlənən secret-i avtomatik silmir: hər staging və commitdən əvvəl yoxla.
Validator tanınan secret nümunələri, ölçü və fayl yolları üzrə məhdud statik yoxlamadır;
naməlum secret-lərin hamısını aşkarladığına zəmanət vermir. Real həssas məlumatı test nümunəsi kimi də işlətmə.
Test hesabatlarında yalnız sanitizasiya olunmuş lokal nəticələr saxla.
Serverə giriş, DB, firewall, port, production, mağaza və ödəniş ayrıca açıq icazə tələb edir.

## Xərc

P02.3: əlavə xidmət alınmayıb, lokal pulsuz alətlər istifadə olunur; əlavə ödəniş 0 AZN; lokal Git/Python istifadə edilir.
Mövcud AI planının qiyməti bu rəqəmə daxil deyil. Yeni CI/hosting və pullu API əlavə edilmir.
Gələcək mağaza və iOS xərcləri aidiyyəti partda rəsmi mənbədən yenidən yoxlanacaq;
bu sənəddə yeni qiymət proqnozu verilmir. Məhsula açıq mənbə lisenziyası əlavə edilmir.

## Asset qeydiyyatı

[ASSET_MANIFEST.csv](ASSET_MANIFEST.csv) sütunları:
asset_id, path, source, creator, license, license_evidence, modifications, approved_part, status.
P02.1-də procedural müvəqqəti taxta qrafikası öz mənbə koduna istinadla qeyd edilir; xarici art/font faylı gətirilməyib.
Hər yeni asset üçün unikal ID, mövcud nisbi fayl yolu və real hüquq sübutu lazımdır.
Əvvəl agent öz yarada bildiyi yolu yoxlayır; mümkün deyilsə Master Plan IX formatında
bir konkret asset üçün hazır prompt verir. Bilinməyən hüququ təsdiqlənmiş yazma.
