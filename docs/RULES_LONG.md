# Uzun nərd — P01.1 qayda müqaviləsi

Əsas [GDD §4.1–4.2, §4.4–4.5](../NARD_Final_GDD_v1.0.md),
[ortaq koordinatlar və variantlar](RULESET_VARIANTS.md). Versiya: **long-az-gdd1-provisional.1**.
Bu sənəd uzun nərdi qısa nərddən ayrı təsvir edir; P01.2 motoru hələ yazılmır.

## Başlanğıc və yol — L-START, C-DIRECTION

P0: points[0]=15; P1: points[12]=−15. Digər 22 xana boş, bar/off sıfırdır.
P0 0→…→23→off, P1 12→…→23→0→…→11→off gedir; istiqamətləri eynidir.
Şəxsi q=18..23 evdir. İkinci dövrəyə qayıtmaq olmaz.
Nümunələr: L_START, L_P1_WRAP və L_NO_SECOND_LAP.

## Başlanğıc zəri — L-OPEN

**PROVISIONAL_RULE:** ilkin bir zəri böyük atan başlayır, sonra təzədən iki zər atır.
Bərabərlikdə hər iki ilkin zər təkrar atılır. Bu, qısa nərddəki ilkin iki qiymətlə
oynama seçimi ilə əvəz edilmir. L_OPEN_HIGH/TIE fərqi göstərir.

## Tutulmuş xana və baş — L-HEAD, L-HEAD-FIRST

Rəqibin bir daşı belə xananı bağlayır; daş vurmaq və bar yoxdur.
Öz daşının olduğu xananı yenə tutmaq olar. Hər bir die-hop-un hədəfi yoxlanır;
hop daxilindəki ara xanalardan tullanmaq olar.
Adi növbədə başdan yalnız bir daş çıxa bilər. Başdan bir dəfə çıxmış daşın
növbəti hərəkəti yeni baş çıxışı sayılmır; başqa daş başdan götürülsə sayılır.

**PROVISIONAL_RULE:** hər oyunçunun ilk şəxsi növbəsində 3/3, 4/4 və 6/6
iki daş istisnasıdır; yalnız oyuna birinci başlayana aid deyil.
L_FIRST_33 və L_FIRST_44-də iki daş dörd tokenlə 6 və 8-ə gətirilir.
L_FIRST_66-da iki daş 6-ya gəlir, qalan iki 6 oynana bilmir: qarşı baş 12 bağlıdır.
1/1 və 5/5 dörd hərəkəti bir daşla edir, ikinci baş daşı çıxmır.
İlk şəxsi növbə bitəndən sonra xüsusi icazə qalmır; L_LATER_66_SECOND_HEAD rədd nümunəsidir.
İlk növbə flag-i növbə yekunlaşanda dəyişəcək; yerli addım zamanı vaxtından əvvəl silinmir.

## Altılıq blok — L-BLOCK

**PROVISIONAL_RULE:** rəqibin bütün daşlarını altılığın arxasında saxlayan blok qadağandır.
Dəqiq draft predikatı RULESET_VARIANTS-dədir; hazırkı zərlərlə hərəkətsizliklə qarışdırılmır.
L_BLOCK_ALL_BEHIND və L_BLOCK_ONE_AHEAD yalnız bir rəqib daşının mövqeyi ilə fərqlənir.
L_BLOCK_GLOBAL_CORNER global döngəni göstərir. Beşli blok bu altılıq qadağası ilə rədd edilmir.
Bu variantın insan testi açıqdır; avtomatik sənəd yoxlaması onun icma təsdiqi deyil.

## Zər məcburiyyəti — C-DICE, C-MAX, L-HIGH

Hər qiymət ayrı addımdır; qoşa dörd eyni qiymət verir. Maksimum mümkün token sayı işlənir.
Zər sırası əvvəlcədən məcburi deyil: L_ORDER_BOTH-də kiçik sonra böyük hər ikisini oynadır.
Heç nə oynanmırsa boş turn; oynamaq mümkündürsə könüllü pass yoxdur.
**PROVISIONAL_RULE L-HIGH:** maksimum yalnız bir token olanda iki alternativdən yüksək seçilir.
L_HIGH_ACCEPT/REJECT son bir daşla bunun nə demək olduğunu göstərir.

## Evə yığma və çıxarma — L-BEAR

**PROVISIONAL_RULE:** bütün qalan daşlar evə gəlmədən çıxarma yoxdur.
Evdə daşın məsafəsi 24−q-dur. Dəqiq məsafə ilə çıxır; artıq qiymət yalnız daha uzaq
öz daşı qalmadıqda ən uzaq mövcud daşa tətbiq edilir. Dəqiq xana boş, uzaqda daş varsa
öncə evdaxili qanuni hərəkət lazımdır. Ev daxilində gediş alternativi ola bilər;
son turn yenə maksimum istifadə şərtinə tabedir.
L_BEAR_EXACT, L_NOT_ALL_HOME, L_OVERSHOOT_* və L_NO_SECOND_LAP bunu nümunələşdirir.
Son, 15-ci daş çıxdıqda partiya tamamlanır; ondan sonra token işlədilmir.

## Nəticə — L-MARS

Adi qələbə 1, mars 2 xal; uzun nərddə koks **yoxdur** — GDD-nin dəyişməyən seçimidir.
**PROVISIONAL_RULE** yalnız mars kriteriyasına aiddir: məğlub off=0 olduqda mars,
off>0 olduqda adi nəticə. L_NORMAL/L_MARS nümunələridir.
Best of 3/5 xal cəminə deyil partiya qələbəsinə görədir; ortaq C-SERIES-ə bax.
Undo yalnız təsdiqlənməmiş öz növbəsindədir; baş sayacı və tokenlər də snapshot-a qayıdır.

İnsan yoxlaması: [beş əsas mövqe qrupu və tam matris](RULE_MATRIX.md).
