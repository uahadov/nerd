# P01.1 — qayda nümunələrinin oxunaqlı matrisi

Əsas məlumat [fixtures.json](rules/fixtures.json), format [JSON Schema](rules/fixtures.schema.json).
Bu cədvəl həmin məlumatın oxunaqlı indeksidir; müstəqil yeni qərar mənbəyi deyil.
Validator ID/title/dice/move/result/assurance sətirlərinin JSON-la tam uyğunluğunu yoxlayır.
ACCEPT/REJECT P01.1-də yazılmış **gözlənilən** cavabdır. P01.2 uzun mühərrikdə
26 aid nümunə icra edilir; qısa/seriya/reytinq/undo üçün motor testi hələ yoxdur.
`move` tək addımın yerli şərtini, `turn` tam təklifin maksimum zər şərtini göstərir;
heç biri server commit-i deyil. `slot` zərin unikal istifadə tokenidir.
Tam 24 xana, bar/off və əvvəl/sonra vəziyyətlər JSON-dadır; [koordinatlar və variantlar](RULESET_VARIANTS.md).

| ID | Nümunə | Zərlər | Təklif | Gözlənilən | Status |
|---|---|---|---|---|---|
| L_START | Başlanğıc: 24 xana, hər tərəfdə 15 daş | [] | start | ACCEPT: OK | GDD_ALIGNED |
| L_OPEN_HIGH | İlkin 6/2: qalib və ilk oyun zərləri | [] | opening | ACCEPT: OK | PROVISIONAL_RULE |
| L_OPEN_TIE | İlkin 4/4: heç kim başlamır, təkrar atılır | [] | opening | ACCEPT: OK | PROVISIONAL_RULE |
| S_START | Başlanğıc: 24 xana, hər tərəfdə 15 daş | [] | start | ACCEPT: OK | GDD_ALIGNED |
| S_OPEN_HIGH | İlkin 6/2: qalib və ilk oyun zərləri | [] | opening | ACCEPT: OK | GDD_ALIGNED |
| S_OPEN_TIE | İlkin 4/4: heç kim başlamır, təkrar atılır | [] | opening | ACCEPT: OK | GDD_ALIGNED |
| L_STEP_OVER_OCCUPIED | Bir zərin daxilində aralıq xana tutulsa da boş hədəfə tullanmaq olar | [2, 1] | 0→2(slot 0) | ACCEPT: OK | GDD_ALIGNED |
| L_OPPONENT_SINGLE_BLOCKS | Rəqibin tək daşı vurulmur, həmin xana bağlıdır | [2, 1] | 5→7(slot 0) | REJECT: DESTINATION_OCCUPIED | GDD_ALIGNED |
| L_SECOND_HEAD_REJECT | Adi növbədə ikinci daş başdan çıxmır | [3, 2] | 0→2(slot 1) | REJECT: HEAD_LIMIT | GDD_ALIGNED |
| L_FIRST_33 | İlk şəxsi növbə 3/3: iki baş daşı | [3, 3] | 0→3(slot 0), 3→6(slot 1), 0→3(slot 2), 3→6(slot 3) | ACCEPT: OK; max=4 | PROVISIONAL_RULE |
| L_FIRST_44 | İlk şəxsi növbə 4/4: iki baş daşı | [4, 4] | 0→4(slot 0), 4→8(slot 1), 0→4(slot 2), 4→8(slot 3) | ACCEPT: OK; max=4 | PROVISIONAL_RULE |
| L_FIRST_66 | İlk şəxsi növbə 6/6: iki baş daşı | [6, 6] | 0→6(slot 0), 0→6(slot 1) | ACCEPT: OK; max=2 | PROVISIONAL_RULE |
| L_FIRST_11 | İlk 1/1: yalnız bir baş daşı, dörd ayrı hərəkət | [1, 1] | 0→1(slot 0), 1→2(slot 1), 2→3(slot 2), 3→4(slot 3) | ACCEPT: OK; max=4 | PROVISIONAL_RULE |
| L_FIRST_55 | İlk 5/5: yalnız bir baş daşı, dörd ayrı hərəkət | [5, 5] | 0→5(slot 0), 5→10(slot 1), 10→15(slot 2), 15→20(slot 3) | ACCEPT: OK; max=4 | PROVISIONAL_RULE |
| L_LATER_66_SECOND_HEAD | Sonrakı 6/6 növbəsində iki baş daşı istisnası yoxdur | [6, 6] | 0→6(slot 0), 0→6(slot 1) | REJECT: HEAD_LIMIT; max=1 | PROVISIONAL_RULE |
| L_P1_WRAP | P1 23→1 gedir: öz yolunda irəliləyir, P0 ilə eyni dövrə istiqaməti | [2, 1] | 23→1(slot 0) | ACCEPT: OK | GDD_ALIGNED |
| L_ORDER_BOTH | 1 sonra 2: hər iki zər oynanır; 2 əvvəl bağlıdır | [2, 1] | 0→1(slot 1), 1→3(slot 0) | ACCEPT: OK; max=2 | GDD_ALIGNED |
| L_BLOCK_ALL_BEHIND | 18..23 altılığı: bütün rəqib daşları arxadadır | [6, 1] | 17→23(slot 0) | REJECT: SIX_BLOCK_TRAPS_ALL | PROVISIONAL_RULE |
| L_BLOCK_ONE_AHEAD | Eyni altılıq: rəqibin bir daşı blokdan qabaqdadır | [6, 1] | 17→23(slot 0) | ACCEPT: OK | PROVISIONAL_RULE |
| L_BLOCK_GLOBAL_CORNER | 22,23,0,1,2,3: global döngə də ardıcıldır | [3, 1] | 0→3(slot 0) | REJECT: SIX_BLOCK_TRAPS_ALL | PROVISIONAL_RULE |
| L_NOT_ALL_HOME | 17-də daş qaldıqda 18-dən çıxarmaq olmaz | [6, 1] | 18→off(slot 0) | REJECT: NOT_ALL_HOME | PROVISIONAL_RULE |
| L_BEAR_EXACT | 21-dəki son daş məsafə 3 ilə çıxır | [2, 3] | 21→off(slot 1) | ACCEPT: OK | PROVISIONAL_RULE |
| L_OVERSHOOT_NEAR_REJECT | 6 ilə yalnız ən uzaq 20-dəki daş artıq qiymətlə çıxarıla bilər | [6, 1] | 22→off(slot 0) | REJECT: OVERSHOOT_NOT_FARTHEST | PROVISIONAL_RULE |
| L_OVERSHOOT_FARTHEST | 6 ilə yalnız ən uzaq 20-dəki daş artıq qiymətlə çıxarıla bilər | [6, 1] | 20→off(slot 0) | ACCEPT: OK | PROVISIONAL_RULE |
| L_NO_SECOND_LAP | 23-dən 0-a ikinci dövrə yox; evdən çıxış off-dur | [1, 2] | 23→0(slot 0) | REJECT: SECOND_LAP | PROVISIONAL_RULE |
| L_HIGH_REJECT | Tək son daş: maksimum 1 hərəkətdirsə yüksək zər seçilir | [1, 2] | 23→off(slot 0) | REJECT: HIGHER_DIE_REQUIRED; max=1 | PROVISIONAL_RULE |
| L_HIGH_ACCEPT | Tək son daş: maksimum 1 hərəkətdirsə yüksək zər seçilir | [1, 2] | 23→off(slot 1) | ACCEPT: OK; max=1 | PROVISIONAL_RULE |
| S_HIT_SINGLE | 7→5: tək rəqib daşı bar-a gedir | [2, 1] | 7→5(slot 0) | ACCEPT: OK | GDD_ALIGNED |
| S_TWO_BLOCK | İki rəqib daşı hədəfi bağlayır | [2, 1] | 7→5(slot 0) | REJECT: DESTINATION_OCCUPIED | GDD_ALIGNED |
| S_BAR_PRIORITY | Bar boşalmadan 5→3 seçmək olmaz | [1, 2] | 5→3(slot 1) | REJECT: BAR_REQUIRED | GDD_ALIGNED |
| S_BAR_ENTER_P0 | P0 bar-dan 1 ilə 23-ə girir | [1, 2] | bar→23(slot 0) | ACCEPT: OK | GDD_ALIGNED |
| S_BAR_ENTER_P1 | P1 bar-dan 3 ilə 2-yə girir | [3, 2] | bar→2(slot 0) | ACCEPT: OK | GDD_ALIGNED |
| S_BAR_HIT | Bar-dan giriş tək rəqib daşını vura bilər | [1, 2] | bar→23(slot 0) | ACCEPT: OK | GDD_ALIGNED |
| S_BAR_FULL_BLOCK | 1 və 2 girişləri bağlı: sıfır hərəkətli turn qanunidir | [1, 2] | turn | ACCEPT: OK; max=0 | GDD_ALIGNED |
| S_BAR_PARTIAL | İki bar daşından yalnız biri 2 ilə girir, 1 bağlıdır | [1, 2] | bar→22(slot 1) | ACCEPT: OK; max=1 | GDD_ALIGNED |
| S_ORDER_BOTH | 8→7→5: əvvəl kiçik zər lazımdır, hər ikisi istifadə olunur | [2, 1] | 8→7(slot 1), 7→5(slot 0) | ACCEPT: OK; max=2 | GDD_ALIGNED |
| S_MAX_REQUIRED | İki gediş mümkün olduğu halda bir gedişlə turn bitmir | [2, 1] | 8→6(slot 0) | REJECT: MAX_DICE_REQUIRED; max=2 | GDD_ALIGNED |
| S_LOW_ONLY | Yüksək zər bağlıdırsa yeganə kiçik zər işlənir | [1, 2] | 8→7(slot 0) | ACCEPT: OK; max=1 | GDD_ALIGNED |
| S_NO_SUM_JUMP | 2/3 zərlərini 5 sayıb 8→3 sıçramaq olmaz | [2, 3] | 8→3(slot 0) | REJECT: SUMMED_DICE_STEP | GDD_ALIGNED |
| S_INTERMEDIATE_BLOCK | Hər iki aralıq dayanacaq bağlıdır: bütün turn keçilir | [2, 3] | turn | ACCEPT: OK; max=0 | GDD_ALIGNED |
| S_DOUBLE_FOUR | 2/2: dörd ayrı token və dörd ardıcıl hərəkət | [2, 2] | 12→10(slot 0), 10→8(slot 1), 8→6(slot 2), 6→4(slot 3) | ACCEPT: OK; max=4 | GDD_ALIGNED |
| S_REUSED_DIE | İstehlak olunmuş token ikinci dəfə işlənmir | [2, 1] | 8→6(slot 0) | REJECT: DIE_ALREADY_USED | GDD_ALIGNED |
| S_WRONG_DIRECTION | P0 5→6 ilə geri gedə bilməz | [1, 2] | 5→6(slot 0) | REJECT: WRONG_DIRECTION | GDD_ALIGNED |
| S_NOT_ALL_HOME | 7-də daş qaldıqda 5-dən çıxarmaq olmaz | [6, 1] | 5→off(slot 0) | REJECT: NOT_ALL_HOME | GDD_ALIGNED |
| S_BEAR_EXACT | 2-dəki son daş məsafə 3 ilə çıxır | [2, 3] | 2→off(slot 1) | ACCEPT: OK | GDD_ALIGNED |
| S_OVERSHOOT_NEAR_REJECT | 6 ilə yalnız ən uzaq 4-dəki daş artıq qiymətlə çıxarıla bilər | [6, 1] | 3→off(slot 0) | REJECT: OVERSHOOT_NOT_FARTHEST | GDD_ALIGNED |
| S_OVERSHOOT_FARTHEST | 6 ilə yalnız ən uzaq 4-dəki daş artıq qiymətlə çıxarıla bilər | [6, 1] | 4→off(slot 0) | ACCEPT: OK | GDD_ALIGNED |
| S_HIGH_REJECT | 0-da tək son daş: yüksək 2 seçilir | [1, 2] | 0→off(slot 0) | REJECT: HIGHER_DIE_REQUIRED; max=1 | GDD_ALIGNED |
| S_HIGH_ACCEPT | 0-da tək son daş: yüksək 2 seçilir | [1, 2] | 0→off(slot 1) | ACCEPT: OK; max=1 | GDD_ALIGNED |
| L_UNDO_LOCAL | Öz təsdiqlənməmiş növbəsi: daş, bar və zər tokeni birlikdə geri qayıdır | [2, 1] | undo | ACCEPT: OK | GDD_ALIGNED |
| L_UNDO_COMMITTED | Server təsdiqindən sonra undo rədd edilir | [2, 1] | undo | REJECT: COMMITTED_TURN | GDD_ALIGNED |
| S_UNDO_LOCAL | Öz təsdiqlənməmiş növbəsi: daş, bar və zər tokeni birlikdə geri qayıdır | [2, 1] | undo | ACCEPT: OK | GDD_ALIGNED |
| S_UNDO_COMMITTED | Server təsdiqindən sonra undo rədd edilir | [2, 1] | undo | REJECT: COMMITTED_TURN | GDD_ALIGNED |
| L_NORMAL | Nəticə: NORMAL | [] | classify_result | ACCEPT: OK; NORMAL=1 | PROVISIONAL_RULE |
| L_MARS | Nəticə: MARS | [] | classify_result | ACCEPT: OK; MARS=2 | PROVISIONAL_RULE |
| S_NORMAL | Nəticə: NORMAL | [] | classify_result | ACCEPT: OK; NORMAL=1 | GDD_ALIGNED |
| S_MARS | Nəticə: MARS | [] | classify_result | ACCEPT: OK; MARS=2 | GDD_ALIGNED |
| S_KOKS_BAR | Nəticə: KOKS | [] | classify_result | ACCEPT: OK; KOKS=3 | GDD_ALIGNED |
| S_KOKS_HOME | Nəticə: KOKS | [] | classify_result | ACCEPT: OK; KOKS=3 | GDD_ALIGNED |
| S_OFF_ONE_PREVENTS_KOKS | Nəticə: NORMAL | [] | classify_result | ACCEPT: OK; NORMAL=1 | GDD_ALIGNED |
| C_BO3_CONTINUES | Seriyada mars/koks xalı ayrıca partiya qələbəsi deyil | [] | series | ACCEPT: OK | GDD_ALIGNED |
| C_BO3_FINISHES | Seriyada mars/koks xalı ayrıca partiya qələbəsi deyil | [] | series | ACCEPT: OK | GDD_ALIGNED |
| C_BO5_FINISHES | Seriyada mars/koks xalı ayrıca partiya qələbəsi deyil | [] | series | ACCEPT: OK | GDD_ALIGNED |
| C_RATING_BOT_CASUAL | Reytinq yalnız təsdiqli ranked random nəticəsinə aiddir | [] | rating_policy | ACCEPT: OK | GDD_ALIGNED |
| C_RATING_PRIVATE_CASUAL | Reytinq yalnız təsdiqli ranked random nəticəsinə aiddir | [] | rating_policy | ACCEPT: OK | GDD_ALIGNED |
| C_RATING_RANDOM_RANKED | Reytinq yalnız təsdiqli ranked random nəticəsinə aiddir | [] | rating_policy | ACCEPT: OK | GDD_ALIGNED |
| C_RATING_RANDOM_CASUAL | Reytinq yalnız təsdiqli ranked random nəticəsinə aiddir | [] | rating_policy | ACCEPT: OK | GDD_ALIGNED |

## İnsan üçün beş əsas baxış

1. **Baş istisnaları:** L_FIRST_33, L_FIRST_44 və L_FIRST_66. 6/6-da yalnız iki token işlənir;
   qarşı baş 12-ni bağlayır. Sonrakı 6/6-da iki baş daşı olmaz (L_LATER_66_SECOND_HEAD).
2. **Altılıq:** L_BLOCK_ALL_BEHIND rədd, L_BLOCK_ONE_AHEAD qəbul gözləyir;
   L_BLOCK_GLOBAL_CORNER-də 23→0 döngəsi də sayılır. Bunlar yerli oyunçu qapısıdır.
3. **Qısa nərd/bar:** S_HIT_SINGLE, S_BAR_PRIORITY və P0/P1 girişləri.
   P0 üçün bar+1→23, P1 üçün bar+3→2; istiqamətlər tərsdir.
4. **Evə çıxarma və xal:** EXACT/OVERSHOOT nümunələri, S_KOKS_BAR/HOME,
   S_OFF_ONE_PREVENTS_KOKS və L_MARS. Uzun nərddə koks yoxdur.
5. **Məcburi zər və seriya:** S_ORDER_BOTH, S_HIGH_ACCEPT/REJECT və BO3/BO5.
   Mars/koks xalı iki/üç partiya qələbəsi deyil. UNDO_LOCAL snapshot-u bərpa edir,
   UNDO_COMMITTED isə rədd olunur.

Rəydə yalnız nümunə ID-sini və qəbul etdiyiniz/fərqli oynadığınız nəticəni bildirmək kifayətdir.
Yerli qayda fərqi səssiz dəyişdirilməyəcək; yeni RuleSetVersion qərarı lazımdır.
