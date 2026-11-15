МОНСТР (ОБГОРЕВШИЙ ПАРЕНЬ ИЗ ПАЛАТЫ 313) — ИНСТРУКЦИЯ ДЛЯ UNITY
=================================================================

Это монстр-версия парня из 313. Скелет ТОТ ЖЕ, что у обычного парня (Mixamo),
поэтому всё, что ты делал с парнем, работает и тут.
Масштаб: 1 юнит = 1 метр, рост ~1.65 м. Модель в Т-позе.


1. ЧТО В ПАПКЕ
--------------
Monster.fbx            — сам монстр: скелет + все детали (текстуры вшиты внутрь)
Monster_Skin.png       — кожа с ожогами
Monster_Eye_L.png      — левый глаз
Monster_Eye_R.png      — правый глаз
Gown_Paint.png         — роба
Animations\            — анимации (БЕЗ модели, только движения):

  Monster Injured Idle.fbx            — стоит на месте, покачивается (ОСНОВНОЕ стояние)
  Monster Idle.fbx                    — запасной вариант стояния
  Monster Injured Walk.fbx            — хромает прямо (In Place)
  Monster Injured Walk Left Turn.fbx  — хромает с поворотом налево
  Monster Injured Run.fbx             — хромающий бег, погоня (In Place)
  Monster Yelling Out.fbx             — шаг вперёд и крик: "заметил игрока"
  Monster Zombie Neck Bite.fbx        — кусает в шею: "поймал игрока"

"In Place" = персонаж бежит на месте, двигать его по уровню должен скрипт/NavMesh.


2. ЧТО ЕСТЬ ЧТО ВНУТРИ Monster.fbx
----------------------------------
Armature            — скелет (mixamorig:...)
Human               — тело: голова, руки, ноги, бёдра
Human.001           — роба (зелёная)
Human.002           — глаза
Monster_Teeth       — зубы, дёсны и тёмная глубина рта (внутри рта)
Monster_HairWisps   — редкие волосинки на голове
Monster_Bone        — кость, торчащая из голени (левая нога)
Monster_ArmBone     — открытый перелом левого предплечья (2 обломка)
Monster_UnderBody   — тёмный торс ПОД робой (чтобы под робой не было пустоты).
                      Снаружи его видно не должно. НЕ удалять.

Всё привязано к костям — при анимации двигается само.


3. ИМПОРТ (по шагам)
--------------------
1) Перетащи всю папку Monster_for_Unity в Assets.

2) Кликни Monster.fbx -> Inspector:
   Model:
     Scale Factor 1
     Convert Units — галка
   Rig:
     Animation Type = Humanoid
     Avatar Definition = Create From This Model
     -> Apply.  (Если кнопка Configure показывает зелёного человечка — всё ок.)
   Animation:
     Import Animation — выключить (в самой модели анимаций нет)
   Materials:
     Location = Use External Materials (Legacy) -> Apply
     (или Extract Materials в папку Materials)

3) Каждую анимацию в Animations\ :
   Rig:
     Animation Type = Humanoid
     Avatar Definition = Copy From Other Avatar
     Source = MonsterAvatar (аватар из Monster.fbx)
     -> Apply
   Animation:
     Loop Time — ГАЛКА для: Injured Idle, Idle, Injured Walk, Walk Left Turn, Injured Run
     Loop Time — БЕЗ галки для: Yelling Out, Zombie Neck Bite (играют один раз)
     Root Transform Rotation: Bake Into Pose — галка, Based Upon = Original
     Root Transform Position (Y): Bake Into Pose — галка, Based Upon = Feet
     Root Transform Position (XZ): Bake Into Pose — галка (у ходьбы/бега, т.к. In Place)
     -> Apply
   Клип внутри файла называется "mixamo.com" — можно переименовать для удобства.

4) Перетащи Monster.fbx на сцену. Проверь: в Т-позе, смотрит вперёд, рост ~1.65.


4. МАТЕРИАЛЫ (URP Lit)
----------------------
Если после импорта что-то серое/белое, выстави вручную:

  Human.body        Base Map = Monster_Skin.png
  Monster_Eye_L     Base Map = Monster_Eye_L.png
  Monster_Eye_R     Base Map = Monster_Eye_R.png
  Gown_Fabric       Base Map = Gown_Paint.png
                    !!! Render Face = Both (у робы одна сторона, иначе изнутри
                    и снизу её не видно и кажется, что монстр голый)
  Monster_Teeth     цвет #D8CDA8, Smoothness 0.5
  Monster_Gums      цвет #8A3A3F
  Monster_MouthDark цвет #1A0A0B, Smoothness 0
  Monster_Wisp      цвет #030303 (волосинки), Render Face = Both
  Monster_Bone      цвет #CFC3A9 (кости)
  Monster_UnderBody цвет #95614F, Smoothness 0

  Для PS2-стиля: у всех PNG поставь Filter Mode = Point (no filter),
  Max Size 512, Compression None.


5. ANIMATOR — КАК СОБРАТЬ
-------------------------
Создай Animator Controller "MonsterAnimator", повесь на монстра (компонент Animator,
Avatar = MonsterAvatar, Apply Root Motion — ВЫКЛ).

Параметры:
  Speed   (float)   — 0 стоит, ~1 идёт, ~3 бежит
  Alert   (trigger) — заметил игрока
  Catch   (trigger) — поймал игрока

Состояния:
  Locomotion  — Blend Tree по Speed:
                 0   -> Monster Injured Idle
                 1   -> Monster Injured Walk
                 3   -> Monster Injured Run
  Yell        — Monster Yelling Out
  Bite        — Monster Zombie Neck Bite

Переходы:
  Locomotion -> Yell   : по Alert, Has Exit Time выкл
  Yell -> Locomotion   : Has Exit Time вкл (после крика сразу в бег — ставь Speed=3)
  Any State -> Bite    : по Catch
  Bite -> Locomotion   : Has Exit Time вкл

Поворот налево (Walk Left Turn) можно не использовать — NavMesh сам
поворачивает модель. Или повесить его на поворот, если хочется красивее.


6. ДВИЖЕНИЕ И ПОВЕДЕНИЕ (под сюжет, главы 4–5)
----------------------------------------------
Монстр появляется на 3 этаже. Нужно:

  - NavMeshAgent на монстре (Speed ходьба ~1, бег ~3.2, Angular Speed 200).
    Запеки NavMesh по больнице (Window -> AI -> Navigation).
  - Каждый кадр: animator.SetFloat("Speed", agent.velocity.magnitude);
  - Состояния скрипта:
      Бродит  — ходит между точками на 3 этаже (Speed ~1).
      Заметил — если игрок в зоне видимости И включён фонарик (монстр идёт на свет!)
                -> SetTrigger("Alert"), потом погоня (agent.SetDestination(player)).
      Погоня  — бежит к игроку. Если игрок забежал в палату и закрыл дверь —
                монстр НЕ заходит, возвращается бродить.
      Поймал  — расстояние < 1 м -> SetTrigger("Catch"), камеру на монстра,
                через ~1.5 с белая вспышка -> игрок телепортируется к лифту
                3 этажа (F3_LiftSpawn), собранные вещи остаются.
  - Сцена у двери 313: монстр стоит в конце коридора (Injured Idle, Speed 0),
    экран глючит, дверь 313 мигает (по очереди включать дверь и заглушку
    "заглушка для комнаты 313"). Игрок подходит вплотную -> монстр выключается
    (SetActive(false)), дверь остаётся.
  - Финал (палата 313): монстр стоит в дверях (Injured Idle).


7. ЧТО ПРОВЕРИТЬ ПОСЛЕ ИМПОРТА
------------------------------
  [ ] Монстр стоит в Т-позе, нормального роста, смотрит вперёд
  [ ] Все текстуры на месте (кожа с ожогами, глаза, роба)
  [ ] Зубы видны во рту, не торчат сквозь щёки
  [ ] Кости на голени и на руке на месте
  [ ] Каждая анимация на превью (вкладка Animation, внизу окошко Preview,
      перетащи туда Monster.fbx) — роба не рвётся, ноги не торчат сквозь неё
  [ ] Ходьба/бег на месте (не уезжает вперёд — значит In Place работает)


8. ЕСЛИ ЧТО-ТО НЕ ТАК
--------------------
  - Монстр лежит на боку / огромный -> проверь Scale Factor 1 и что Rig = Humanoid.
  - Анимация "ломает" тело / руки выкручены -> у анимации Avatar должен быть
    Copy From Other Avatar = MonsterAvatar.
  - Уезжает вперёд при ходьбе -> Root Transform Position (XZ) Bake Into Pose.
  - Проваливается в пол / висит -> Root Transform Position (Y) Based Upon = Feet.
  - Роба изнутри прозрачная -> Gown_Fabric: Render Face = Both.
  - Колени иногда чуть протыкают робу — это нормально для длинной робы
    без симуляции ткани. Если сильно мешает: на робу можно повесить компонент
    Cloth (только на нижнюю часть подола), но это сложнее. Приседающие анимации
    НЕ брать — в них ноги вылезают сквозь робу.

Удачи, жги :)
