# NƏRD. — bütün AI agentləri üçün iş müqaviləsi

Bu qaydalar Codex, Google Antigravity və digər agentlərə eyni tətbiq olunur.
Layihəni davam etdirmək üçün söhbət tarixçəsinə, modelə, MCP-yə və Skill-ə ehtiyac olmamalıdır.

## İcra sərhədi və mənbələr

1. Əvvəl bu faylı, [README](README.md), [statusu](docs/STATE.json),
   [irəliləyişi](PROGRESS.md), [qərarları](docs/DECISIONS.md), hər iki tam mənbəni
   [GDD](NARD_Final_GDD_v1.0.md) və [Master Planı](NARD_Master_Icra_Plani_v2.md),
   [qəbul](docs/APPROVED.md) və [rədd](docs/REJECTED.md) inventarını oxu.
   Sonra statusdakı cari hesabatı və part üçün müvafiq sənədləri oxu.
2. Məhsul qərarları GDD-dən, inkişaf ardıcıllığı Master Plandan gəlir.
   İstifadəçinin son açıq göstərişi üstünlük təşkil edir; fərqi DECISIONS-da qeyd et.
   İki mənbənin orijinal adları saxlanılır; GDD.md və MASTER_PLAN.md adlı surətlər yaratma.
3. Yalnız STATE-də icazəli bir part üzərində işlə. Fayldakı icazə insanın
   qeyd olunmuş açıq mesajına söykənməlidir; agent özü icazə yarada bilməz.
   İcazə və status ziddirsə yeni işə başlama, əvvəl ziddiyyəti aydınlaşdır.
4. Hazırda yalnız P02.5 icazəlidir: iki responsive matç yerləşimi, safe area və çevrilmədə state-in qorunması.
   D033 insanın P02.4 funksional qəbulunu və P02.5 icazəsini saxlayır. D032 üzrə board-focus HUD qurulur;
   premium referans material üslubudur, bütöv hazır səhnə deyil. D034 cari iki-orientasiya/HUD düzəlişidir; portret ayrıca dik tray-lərdən qurulur. Final polish və P02.6/save yoxdur.
   Rules/Match/Interaction, zər mənbəyi və əvvəlki C# testlərini qoru. Undo/avtomatik commit qaydasını dəyişmə.
   Bot, online, başqa ekran və final art yoxdur. Fiziki Android və yerli oyunçu NOT_RUN qalır.
   Altı PROVISIONAL_RULE saxlanır. Native Editor qəzası həll sübutu olmadan açıq riskdir.
   İş bitəndə AWAITING_HUMAN_APPROVAL ilə dayan; P02.6-ya icazə yoxdur.
5. A01–A87 uzunmüddətli backlog-dur; hamısı ilk buraxılış deyil.
   R01–R20-ni yeni açıq məhsul qərarı olmadan həyata keçirmə.
6. Gələcək final taxtanın əsas insan-seçilmiş [vizual referansını](docs/DECISIONS.md) D024-də oxu.
   Üfüqi taxta və şaquli matç dəstəyi birlikdə saxlanır. İlkin D024 qeydi gələcək iş idi; D031 son insan icazəsi ilə konkret taxta/daş görünüşünü indi açır.

## Alət və xərc qaydaları

- Heç bir Skill istifadə etmə: quraşdırılmış, avtomatik təklif olunan, dizayn,
  UI/UX, Unity, kod, test, arxitektura və əlavə yüklənən Skill-lər daxil.
  SKILL.md açma, Skill işə salma, yükləmə və quraşdırma.
- Antigravity MCP, Codex Gravity MCP və Antigravity ilə əlaqəli MCP
  serverləri/alətləri qadağandır; adını dəyişmiş vasitə ilə də dolayı istifadə etmə.
- Adi terminal, Git, birbaşa fayl redaktəsi və standart proqramlaşdırma alətləri icazəlidir.
  Unity CLI adi əmr kimi müvafiq icazəli partda istifadə oluna bilər;
  bu, onun Skill-lərini işlətməyə və ya paket quraşdırmağa ümumi icazə deyil.
- Heç bir başqa MCP və ya agentə xas plugin layihənin davamı üçün məcburi asılılıq olmasın.
- Əvvəl pulsuz/lisenziyalı və lokal yol. Ödəniş, serverə giriş/dəyişiklik,
  kritik sistem parametrləri, deployment və mağaza yayımı ayrıca açıq icazə tələb edir.
- Başqa layihəyə toxunma. Git tarixini sıfırlama, force-push etmə,
  mövcud məlumatları silmə. Uzaq repo tarixçəsi varsa əvvəl oxu və uzlaşdır.
- Mövcud GitHub origin-ə push qadağandır; onun private olması bu qadağanı avtomatik qaldırmır.
  Gələcək uzaq nüsxə yalnız private repository və ayrıca açıq push icazəsi ilə mümkündür.
- Token/parol/şəxsi cihaz ID-si saxlama və göstərmə. Yalnız bu repo üçün
  Git müəllifini qur; qlobal konfiqurasiyanı dəyişmə.

## Kiçik partın iş dövrü

Plan → minimal icra → real test → düzəliş → təkrar yoxlama → hesabat → təhlükəsiz commit → DAYAN.

- [Arxitektura](docs/ARCHITECTURE.md) sərhədlərinə əməl et; gələcək modulun kodunu qabaqcadan yazma.
- Qayda və UI-ni ayır; online zər, gediş, qalibiyyət və mükafat serverdə təsdiqlənir.
- Testi icra etmədən PASS yazma. NOT_RUN/BLOCKED səbəbini və sübutunu qeyd et.
  Kod oxumaq vizual test deyil. Build fiziki cihaz testi deyil.
- Cari yoxlama: `python3 scripts/validation/validate_project.py`.
  Yoxlayıcının sınaqları: `python3 -m unittest discover -s scripts/validation -p 'test_*.py' -v`.
- Status, progress, qərar, hesabat, test sübutu və CHANGELOG uyğun olsun.
  [Hesabat şablonundan](docs/reports/TEMPLATE.md) istifadə et.
- Cari iş bitəndə status **AWAITING_HUMAN_APPROVAL** qalır, `current_part_human_approved` false olur,
  `next_part_authorized` false olur. Avtomatik testin uğuru insan təsdiqi deyil.
- Cari partın qəbulu növbəti partın başlanmasına icazə demək deyil.
  İnsan həm qəbulu, həm növbəti işə icazəni açıq bildirməlidir.
- Qəbul tag-ı yalnız insan təsdiqindən sonra yaradıla bilər. Commit və push qəbul deyil.

## Agent dəyişəndə

`git status --short --branch` və `git log -5 --oneline` oxu; yarımçıq işi silmə.
STATE → PROGRESS → cari hesabat → mənbələr üzrə konteksti bərpa et.
`open_items` və `resume_instruction` nə etmək lazım olduğunu göstərir.
Gözləmə statusunda insan rəyini gözlə. İnsan eyni part üçün audit/düzəliş istəyərsə
yalnız həmin işi icra et; bu, yeni part icazəsi deyil. Son göstəriş statusun
AWAITING_HUMAN_APPROVAL qalmasını tələb edirsə audit zamanı da onu dəyişmə.
İnsan düzəliş istəyibsə yalnız həmin cari partı düzəlt.
