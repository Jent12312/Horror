

# ТЕХНИЧЕСКИЙ ДОКУМЕНТ
## «Exorcism» — 3D Multiplayer Horror

---

# ОГЛАВЛЕНИЕ

1. Общее описание проекта
2. Технологический стек
3. Архитектура проекта
4. Этап 1 — Фундамент: Мультиплеер, Управление, Интерактивность
5. Этап 2 — Карта, Освещение, Темнота
6. Этап 3 — Дьявол: Управление, Способности, ИИ-фаза подготовки
7. Этап 4 — Персонажи людей: Уникальные способности
8. Этап 5 — Игровые системы: Книги, Алтарь, Сердце, Выход
9. Этап 6 — Ловушки, Скримеры, Эффекты
10. Этап 7 — Игровой цикл, UI, Лобби
11. Этап 8 — Полировка, Баланс, Тестирование
12. Приложения

---

# 1. ОБЩЕЕ ОПИСАНИЕ ПРОЕКТА

## 1.1 Концепция

Асимметричный мультиплеерный хоррор на 5 игроков. 4 человека против 1 дьявола. Действие происходит в полной темноте. Люди ограничены в видимости и используют светящиеся камни и фонари. Дьявол видит лучше, расставляет ловушки и охотится на людей.

## 1.2 Условия победы

**Люди побеждают одним из трёх способов:**
- Изгнание: собрать 3 книги → доставить к алтарю → прочитать заклинание (только Перс-3)
- Убийство: найти логово дьявола (только Перс-4 открывает) → найти зачарованный нож → нанести урон сердцу дьявола
- Побег: найти скрытый выход с карты

**Дьявол побеждает:**
- Убить всех 4 людей

## 1.3 Предигровая фаза

Рандом распределяет роли. Дьявол получает 2–3 минуты на:
- Размещение 3 книг по карте
- Определение комнаты алтаря (выбор из доступных)
- Размещение своего логова с сердцем
- Размещение ловушек-скримеров (4 шт.)
- Размещение выхода (скрытая дверь/портал)

После этого 4 человека спавнятся в случайных комнатах (из 8). Каждый — в отдельной.

## 1.4 Карта

8 комнат, соединённых коридорами. Полная темнота. Комнаты различаются по размеру и наполнению. Одна из комнат назначается комнатой алтаря. Одна комната (или скрытая зона) становится логовом дьявола.

---

# 2. ТЕХНОЛОГИЧЕСКИЙ СТЕК

## 2.1 Движок и версия
| Компонент | Технология |
|---|---|
| Движок | Unity 6 |
| Render Pipeline | Universal Render Pipeline (URP) |
| .NET | .NET Standard 2.1 |

## 2.2 Мультиплеер
| Компонент | Технология |
|---|---|
| Сетевой фреймворк | Netcode for GameObjects (NGO) 1.x |
| Транспортный слой | Unity Transport (UTP) |
| Подключение | LAN через Radmin VPN (виртуальная локальная сеть) |
| Топология | Host-Client (один игрок является хостом и сервером одновременно) |

## 2.3 Управление и камера
| Компонент | Технология |
|---|---|
| Система ввода | New Input System (Input Actions) |
| Физика персонажа | Rigidbody (не CharacterController) |
| Камера | Cinemachine 3 |

## 2.4 Дополнительные пакеты
| Пакет | Назначение |
|---|---|
| TextMeshPro | UI-текст |
| DOTween или LeanTween | Анимация UI и эффектов |
| Unity Post Processing (URP Volume) | Визуальные эффекты (виньетка, блум, ужас) |
| Cinemachine Impulse | Тряска камеры при скримерах |
| Addressables (опционально) | Загрузка ассетов |
| Odin inscpector | удобство |

## 2.5 Инструменты разработки
| Инструмент | Назначение |
|---|---|
| Git + GitHub/GitLab | Контроль версий |
| Trello / Notion | Управление задачами |
| Unity Profiler | Оптимизация |
| ParrelSync | Тестирование мультиплеера в одном проекте (клоны редактора) |
| Radmin VPN | Создание виртуальной LAN для игры по сети |

---

# 3. АРХИТЕКТУРА ПРОЕКТА

## 3.1 Общая архитектурная парадигма

Проект использует **сервис-ориентированную архитектуру** с элементами **событийной модели**. Бизнес-логика отделена от представления. Сетевая логика централизована на хосте (авторитарный хост).

## 3.2 Структура папок проекта

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Core/                    — Ядро: менеджеры, сервисы
│   │   │   ├── GameManager
│   │   │   ├── NetworkManager
│   │   │   ├── LobbyManager
│   │   │   ├── RoleAssignmentService
│   │   │   ├── GameStateService
│   │   │   └── EventBus
│   │   ├── Player/                  — Всё о персонаже
│   │   │   ├── PlayerController
│   │   │   ├── PlayerHealth
│   │   │   ├── PlayerInteraction
│   │   │   ├── PlayerCamera
│   │   │   ├── PlayerNetwork
│   │   │   └── Abilities/          — Способности каждого перса
│   │   ├── Devil/                   — Всё о дьяволе
│   │   │   ├── DevilController
│   │   │   ├── DevilAbilities
│   │   │   ├── DevilSetupPhase
│   │   │   └── DevilNetwork
│   │   ├── Interactables/           — Интерактивные объекты
│   │   │   ├── InteractableBase
│   │   │   ├── Book
│   │   │   ├── Knife
│   │   │   ├── Altar
│   │   │   ├── Heart
│   │   │   ├── ExitDoor
│   │   │   └── Candle
│   │   ├── Traps/                   — Ловушки
│   │   │   ├── TrapBase
│   │   │   ├── ScreamerTrap
│   │   │   ├── BearTrap
│   │   │   └── Tripwire
│   │   ├── Lighting/                — Система освещения
│   │   │   ├── SinusoidalStone
│   │   │   ├── LinearStone
│   │   │   ├── Flashlight
│   │   │   └── CandleLight
│   │   ├── UI/                      — Интерфейс
│   │   │   ├── HUDController
│   │   │   ├── LobbyUI
│   │   │   ├── RoleScreenUI
│   │   │   └── GameOverUI
│   │   ├── Map/                     — Карта и комнаты
│   │   │   ├── RoomManager
│   │   │   ├── SpawnPointManager
│   │   │   └── DoorSystem
│   │   └── Utils/                   — Утилиты
│   │       ├── Singleton
│   │       ├── Timer
│   │       └── Extensions
│   ├── Prefabs/
│   │   ├── Player/
│   │   ├── Devil/
│   │   ├── Interactables/
│   │   ├── Traps/
│   │   ├── Lighting/
│   │   └── UI/
│   ├── ScriptableObjects/
│   │   ├── CharacterData/           — SO для каждого перса (хп, скорость)
│   │   ├── AbilityData/             — SO для способностей (кд, урон)
│   │   └── GameSettings/            — SO общих настроек
│   ├── InputActions/
│   │   └── PlayerInputActions.inputactions
│   ├── Scenes/
│   │   ├── BootScene
│   │   ├── LobbyScene
│   │   ├── GameScene
│   │   └── TestScene
│   ├── Materials/
│   ├── Textures/
│   ├── Models/
│   ├── Audio/
│   ├── Animations/
│   └── VFX/
```

## 3.3 Сетевая архитектура

```
┌─────────────────────────────────────────────┐
│               HOST (Server + Client)        │
│                                             │
│  ┌─────────────┐  ┌──────────────────────┐  │
│  │ GameManager  │  │ NetworkManager (NGO) │  │
│  │ (ServerOnly) │  │                      │  │
│  └──────┬───────┘  └──────────┬───────────┘  │
│         │                     │              │
│  ┌──────▼───────────────────▼──────────┐    │
│  │     Авторитарная логика на хосте     │    │
│  │  - Распределение ролей              │    │
│  │  - Здоровье всех игроков            │    │
│  │  - Состояние предметов              │    │
│  │  - Проверка условий победы          │    │
│  │  - Валидация действий               │    │
│  └─────────────────────────────────────┘    │
│                                             │
└──────────────────┬──────────────────────────┘
                   │ Unity Transport (UDP)
        ┌──────────┼──────────┐
        │          │          │
   ┌────▼───┐ ┌───▼────┐ ┌───▼────┐
   │Client 2│ │Client 3│ │Client 4│
   │        │ │        │ │        │
   │ Input  │ │ Input  │ │ Input  │
   │ Render │ │ Render │ │ Render │
   │ Predict│ │ Predict│ │ Predict│
   └────────┘ └────────┘ └────────┘
```

**Принципы:**
- **Server-Authoritative**: вся игровая логика (нанесение урона, подбор предметов, смена состояний) валидируется на хосте
- **Client-Owned Movement**: перемещение управляется клиентом через NetworkTransform с owner-authority для отзывчивости
- **RPCs**: клиент отправляет ServerRpc для запроса действия, хост валидирует и рассылает ClientRpc для подтверждения
- **NetworkVariables**: состояние здоровья, состояние предметов, фаза игры — синхронизируются через NetworkVariable

## 3.4 Авторитарная модель данных

```
Что хранится на хосте (NetworkVariables / серверные структуры):
├── GameState (Lobby / Setup / Playing / GameOver)
├── PlayerData[] для каждого игрока:
│   ├── ClientId
│   ├── Role (Human1 / Human2 / Human3 / Human4 / Devil)
│   ├── Health (NetworkVariable<int>)
│   ├── IsAlive (NetworkVariable<bool>)
│   └── CarriedItemId (NetworkVariable<int>) — id переносимого предмета или -1
├── InteractableData[] для каждого предмета:
│   ├── ItemId
│   ├── ItemType
│   ├── Position (NetworkVariable<Vector3>)
│   ├── State (OnGround / Carried / Used / Destroyed)
│   └── CarriedByClientId
├── GameProgressData:
│   ├── BooksDeliveredToAltar (0..3)
│   ├── HeartHealth (NetworkVariable<int>)
│   ├── ExitFound (bool)
│   └── AlivePlayers (int)
├── RoomData[8]:
│   ├── RoomId
│   ├── IsAltarRoom
│   ├── IsDevilLair
│   └── SpawnPoints[]
└── TrapData[]:
    ├── TrapId
    ├── Position
    ├── IsActive
    └── Type
```

## 3.5 Паттерны проектирования

| Паттерн | Где используется |
|---|---|
| **Singleton** | GameManager, NetworkManager, AudioManager |
| **Observer / EventBus** | Оповещение о событиях (подбор предмета, смерть, скример) |
| **State Machine** | Состояния игры, состояния персонажа, состояния дьявола |
| **Strategy** | Способности персонажей (каждая способность — отдельная стратегия) |
| **Command** | Действия игрока (для отправки по сети) |
| **ScriptableObject Data** | Конфигурация персонажей, способностей, предметов |
| **Factory** | Создание предметов, ловушек, спавн игроков |
| **Service Locator** | Доступ к сервисам без жёстких зависимостей |

## 3.6 Схема состояний игры (Game State Machine)

```
[Boot] → [Lobby] → [RoleAssignment] → [DevilSetup] → [Playing] → [GameOver]
                                            │                         │
                                     Дьявол расставляет              │
                                     предметы 2-3 мин         ┌─────┴─────┐
                                                               │           │
                                                          HumansWin   DevilWins
```

## 3.7 Схема состояний персонажа (Player State Machine)

```
[Idle] ←→ [Walking] ←→ [Running]
  │            │
  ▼            ▼
[Interacting] [CarryingItem]
  │                │
  ▼                ▼
[UsingAbility] [Dropping]
  │
  ▼
[Stunned] (после скримера — 5 сек)
  │
  ▼
[Dead] (конечное состояние)
```

## 3.8 Схема состояний дьявола (Devil State Machine)

```
[SetupPhase] → [Hunting]
                   │
          ┌────────┼────────┐
          ▼        ▼        ▼
     [Attacking] [Placing] [Aggression]
          │        │        │
          └────────┼────────┘
                   ▼
              [Cooldown]
```

---

# 4. ЭТАП 1 — ФУНДАМЕНТ: МУЛЬТИПЛЕЕР, УПРАВЛЕНИЕ, ИНТЕРАКТИВНОСТЬ

> **Цель этапа**: два игрока могут подключиться по LAN (Radmin VPN), видеть друг друга на тестовой сцене, ходить по ней, подбирать и бросать предметы. Один общий персонаж для обоих. Полностью рабочий фундамент.

---

## 4.1 ШАГ 1: Настройка проекта

### 4.1.1 Создание проекта Unity
- Создать проект Unity (3D URP Template)
- Версия: Unity 2022.3 LTS или Unity 6
- Название проекта: `ExorcismGame`
- Настроить URP: выбрать URP Asset, назначить в Graphics Settings и Quality Settings

### 4.1.2 Установка необходимых пакетов
Через Package Manager установить:
- **Netcode for GameObjects** (com.unity.netcode.gameobjects) — версия 1.8+ или последняя стабильная
- **Unity Transport** (com.unity.transport) — транспортный слой для NGO
- **Input System** (com.unity.inputsystem) — новая система ввода
- **Cinemachine** (com.unity.cinemachine) — камера
- **TextMeshPro** — UI

### 4.1.3 Настройка Input System
- В Project Settings → Player → Active Input Handling → выбрать **"Both"** (на этапе разработки) или **"Input System Package (New)"**
- Перезапустить редактор после смены

### 4.1.4 Настройка структуры папок
- Создать полную структуру папок согласно разделу 3.2
- Создать начальные сцены: `BootScene`, `LobbyScene`, `TestScene`
- Настроить Build Settings: добавить все сцены в список, `BootScene` первой

### 4.1.5 Установка дополнительных инструментов
- Установить **ParrelSync** (через Git URL в Package Manager или ручной импорт) — для запуска второго экземпляра редактора при тестировании мультиплеера
- Убедиться что **Radmin VPN** установлен на всех тестовых машинах, создать сеть

### 4.1.6 Настройка Git
- Инициализировать репозиторий
- Создать `.gitignore` для Unity (стандартный шаблон)
- Настроить Git LFS для больших файлов (текстуры, модели, аудио)
- Первый коммит: «Initial project setup»

---

## 4.2 ШАГ 2: Настройка сетевой инфраструктуры (Netcode for GameObjects)

### 4.2.1 Создание NetworkManager на сцене

**Что делаем:**
На `BootScene` (или `TestScene` для начала) создаём пустой GameObject `NetworkManager`.

**Компоненты на нём:**
- `NetworkManager` (из NGO)
- `UnityTransport` (транспортный компонент)

**Настройки UnityTransport:**
- Connection Type: **UDP**
- Address: `0.0.0.0` (для хоста) / IP хоста в Radmin VPN (для клиентов)
- Port: `7777` (по умолчанию)
- Max Connect Attempts: 5
- Connect Timeout: 5000ms

### 4.2.2 Создание менеджера подключения (ConnectionManager)

**Описание:** Сервис, управляющий подключением. Предоставляет методы для старта хоста, подключения клиента, отключения. Обрабатывает события подключения/отключения.

**Обязанности:**
- Хранить IP-адрес и порт
- Предоставлять метод `StartHost()` — запуск как Host (сервер + клиент)
- Предоставлять метод `StartClient(string ipAddress)` — подключение к хосту
- Предоставлять метод `Disconnect()` — отключение
- Подписаться на `NetworkManager.Singleton.OnClientConnectedCallback` — отслеживать подключения
- Подписаться на `NetworkManager.Singleton.OnClientDisconnectCallback` — отслеживать отключения
- Логировать все события подключения

**Псевдокод:**
```
class ConnectionManager : MonoBehaviour

    [SerializeField] string defaultPort = "7777"
    
    method StartHost():
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>()
        transport.SetConnectionData("0.0.0.0", parsePort(defaultPort))
        NetworkManager.Singleton.StartHost()
        Log("Host started")
    
    method StartClient(ipAddress: string):
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>()
        transport.SetConnectionData(ipAddress, parsePort(defaultPort))
        NetworkManager.Singleton.StartClient()
        Log("Connecting to " + ipAddress)
    
    method OnEnable():
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected
    
    method OnClientConnected(clientId):
        Log("Client connected: " + clientId)
        // Вызвать событие через EventBus
    
    method OnClientDisconnected(clientId):
        Log("Client disconnected: " + clientId)
```

### 4.2.3 Настройка для работы через Radmin VPN

**Инструкция для игроков:**
1. Все игроки устанавливают Radmin VPN
2. Один создаёт сеть (задаёт имя и пароль)
3. Остальные подключаются к этой сети
4. Хост видит свой IP в интерфейсе Radmin (например `26.45.123.78`)
5. Хост запускает игру и нажимает "Host"
6. Клиенты вводят IP хоста из Radmin и нажимают "Connect"

**Техническая заметка:** Unity Transport по умолчанию использует UDP. Radmin VPN создаёт виртуальный Ethernet-адаптер, поэтому UDP пакеты проходят через него прозрачно. Никакой специальной интеграции с Radmin VPN в коде не требуется — достаточно использовать IP из Radmin.

### 4.2.4 Создание простого UI лобби (временный, для тестирования)

**Элементы UI на TestScene:**
- Поле ввода `InputField` — для IP адреса
- Кнопка `Host` — вызывает `ConnectionManager.StartHost()`
- Кнопка `Connect` — вызывает `ConnectionManager.StartClient(ip)`
- Текстовое поле `Status` — отображает статус подключения
- Текст `Connected Players: N` — количество подключённых

**Структура Canvas:**
```
Canvas (Screen Space - Overlay)
├── Panel_ConnectionUI
│   ├── TMP_InputField_IP (placeholder: "Enter Host IP")
│   ├── Button_Host (text: "HOST GAME")
│   ├── Button_Connect (text: "CONNECT")
│   ├── TMP_Text_Status (text: "Disconnected")
│   └── TMP_Text_PlayerCount (text: "Players: 0")
```

**Логика UI:**
```
class LobbyUI : MonoBehaviour

    references: inputFieldIP, buttonHost, buttonConnect, textStatus, textPlayerCount
    reference: connectionManager

    method Start():
        buttonHost.onClick += OnHostClicked
        buttonConnect.onClick += OnConnectClicked
    
    method OnHostClicked():
        connectionManager.StartHost()
        textStatus.text = "Hosting..."
        HideConnectionButtons()
    
    method OnConnectClicked():
        ip = inputFieldIP.text
        if ip is not empty:
            connectionManager.StartClient(ip)
            textStatus.text = "Connecting to " + ip
            HideConnectionButtons()
    
    method Update():
        if NetworkManager.Singleton.IsServer:
            textPlayerCount.text = "Players: " + NetworkManager.Singleton.ConnectedClientsList.Count
```

### 4.2.5 Тестирование подключения

**Процедура тестирования с ParrelSync:**
1. Открыть ParrelSync → Clones Manager → Create Clone
2. Открыть основной редактор — запустить PlayMode — нажать "Host"
3. Открыть клон редактора — запустить PlayMode — ввести `127.0.0.1` — нажать "Connect"
4. Убедиться что в логах обоих редакторов видны сообщения о подключении
5. Убедиться что счётчик показывает 2 игрока

**Тестирование через Radmin VPN:**
1. Два компьютера в одной сети Radmin
2. На первом — Host, на втором — Client с IP из Radmin
3. Убедиться в подключении

**Критерии успеха шага:**
- [ ] Два клиента подключаются друг к другу
- [ ] В логах видны события подключения/отключения
- [ ] Счётчик игроков корректен
- [ ] Работает через localhost и через Radmin VPN

---

## 4.3 ШАГ 3: Создание тестовой сцены

### 4.3.1 Тестовая комната

**Создать на TestScene:**
- Плоскость пола (Plane, масштаб 5x5 = 50x50 юнитов)
- 4 стены (Box Colliders, высота 4 юнита)
- Простой потолок
- Несколько примитивных объектов внутри (кубы как «столы», цилиндры как «колонны»)
- Все объекты со стандартными URP-материалами

**Освещение тестовой сцены (временное):**
- Одна Directional Light (слабая, для видимости на этапе тестирования)
- Ambient Light — минимальный

### 4.3.2 Точки спавна

- Создать 2 пустых GameObject'а `SpawnPoint_1` и `SpawnPoint_2` в разных углах комнаты
- Каждый содержит только Transform (позиция + поворот)
- Пометить тегом `SpawnPoint`

### 4.3.3 Тестовые интерактивные объекты

- Создать 3 примитива (например кубы с разными цветами) расставленных по комнате
- Это будут тестовые предметы для подбора
- Добавить Collider и Rigidbody (kinematic пока что) на каждый
- Названия: `TestItem_Red`, `TestItem_Blue`, `TestItem_Green`

---

## 4.4 ШАГ 4: Настройка Input System (New Input System)

### 4.4.1 Создание Input Actions Asset

**Создать файл:** `Assets/_Project/InputActions/PlayerInputActions.inputactions`

**Action Maps:**

**ActionMap: "Player"**
| Action | Type | Binding | Описание |
|---|---|---|---|
| Move | Value (Vector2) | WASD / Left Stick | Перемещение |
| Look | Value (Vector2) | Mouse Delta / Right Stick | Обзор (мышь) |
| Interact | Button | LMB (Left Mouse Button) | Подбор / бросание предмета |
| Sprint | Button | Left Shift | Бег (на будущее) |

**ActionMap: "UI"** (стандартный, для меню)
| Action | Type | Binding |
|---|---|---|
| Navigate | Value (Vector2) | Arrow Keys |
| Submit | Button | Enter |
| Cancel | Button | Escape |

### 4.4.2 Настройка Input Actions Asset
- Открыть `.inputactions` файл
- Создать все Action Maps и Actions как описано выше
- Для `Move`:
  - Type: Value, Control Type: Vector2
  - Binding: 2D Vector Composite (WASD) — Up: W, Down: S, Left: A, Right: D
- Для `Look`:
  - Type: Value, Control Type: Vector2
  - Binding: Mouse → Delta
- Для `Interact`:
  - Type: Button
  - Binding: Mouse → Left Button
- Нажать "Generate C# Class" → сохранить как `PlayerInputActions.cs` в ту же папку

### 4.4.3 Проверка
- Убедиться что сгенерированный C# класс `PlayerInputActions` компилируется без ошибок
- Этот класс будет использован в PlayerController

---

## 4.5 ШАГ 5: Создание префаба игрока (Player Prefab)

### 4.5.1 Структура GameObject'а игрока

```
Player (Prefab Root)
├── Components:
│   ├── NetworkObject
│   ├── NetworkTransform (ClientNetworkTransform)
│   ├── Rigidbody
│   ├── CapsuleCollider
│   ├── PlayerController (наш скрипт)
│   ├── PlayerInteraction (наш скрипт)
│   ├── PlayerNetwork (наш скрипт)
│   └── PlayerInput (компонент из Input System)
│
├── PlayerModel (child)
│   └── [Placeholder капсула или модель]
│
├── CameraHolder (child, empty)
│   └── Transform: position (0, 1.6, 0) — уровень глаз
│
├── InteractionPoint (child, empty)
│   └── Transform: position (0, 1.0, 0.8) — перед персонажем, куда «привязывается» переносимый предмет
│
└── GroundCheck (child, empty)
    └── Transform: position (0, 0.05, 0) — для проверки земли
```

### 4.5.2 Настройка Rigidbody
| Параметр | Значение |
|---|---|
| Mass | 70 |
| Drag | 5 |
| Angular Drag | 0.05 |
| Use Gravity | true |
| Is Kinematic | false |
| Collision Detection | Continuous |
| **Constraints** | **Freeze Rotation X, Y, Z** (чтобы не падал) |

**Почему Rigidbody, а не CharacterController:**
- Более реалистичная физика (взаимодействие с объектами)
- Лучше работает с сетевой синхронизацией (позиция обновляется через физику)
- Возможность толкать объекты и получать физические воздействия

### 4.5.3 Настройка CapsuleCollider
| Параметр | Значение |
|---|---|
| Center | (0, 1, 0) |
| Radius | 0.35 |
| Height | 2.0 |
| Direction | Y-Axis |

### 4.5.4 Настройка NetworkObject и NetworkTransform

**NetworkObject:**
- Добавить компонент `NetworkObject` — это делает объект сетевым

**ClientNetworkTransform (вместо обычного NetworkTransform):**
- Использовать `ClientNetworkTransform` из пакета NGO samples / community contributions
- Или написать свой, наследуясь от `NetworkTransform` и переопределив `OnIsServerAuthoritative()` → return false
- Это даёт **owner-authoritative movement** — владелец объекта управляет позицией, остальные получают синхронизацию

**Настройки NetworkTransform:**
| Параметр | Значение |
|---|---|
| Sync Position X,Y,Z | true |
| Sync Rotation Y | true (только Y — горизонтальный поворот) |
| Sync Rotation X,Z | false |
| Sync Scale | false |
| Interpolate | true |
| Threshold Position | 0.01 |
| Threshold Rotation | 0.1 |

### 4.5.5 Настройка PlayerInput (компонент)
- Behavior: **Invoke Unity Events** (или Invoke C Sharp Events — на выбор)
- Default Map: "Player"
- Actions: ссылка на `PlayerInputActions.inputactions`

> **Важно:** PlayerInput будет активен ТОЛЬКО на объекте, которым владеет локальный игрок. На чужих — отключён.

### 4.5.6 Регистрация префаба в NetworkManager
- В компоненте `NetworkManager` → Player Prefab → перетащить созданный префаб
- Также добавить в Network Prefabs List
- Настроить: Default Player Prefab = true

---

## 4.6 ШАГ 6: Скрипт управления персонажем (PlayerController)

### 4.6.1 Описание

`PlayerController` — основной скрипт управления движением персонажа. Считывает ввод из New Input System, применяет силы к Rigidbody для перемещения, управляет поворотом персонажа по оси Y (горизонтально).

### 4.6.2 ScriptableObject для параметров персонажа

**Создать:** `CharacterDataSO`

```
[CreateAssetMenu] ScriptableObject CharacterDataSO:
    string characterName
    int maxHealth = 100
    float moveSpeed = 5.0
    float sprintSpeed = 8.0
    float rotationSensitivity = 2.0
    float groundCheckRadius = 0.3
    LayerMask groundLayer
```

**Создать ассет** `DefaultCharacterData.asset` с параметрами по умолчанию для тестового персонажа.

### 4.6.3 Логика PlayerController

**Поля:**
```
class PlayerController : NetworkBehaviour

    [SerializeField] CharacterDataSO characterData
    [SerializeField] Transform groundCheck
    
    // Private
    Rigidbody rb
    PlayerInputActions inputActions
    Vector2 moveInput
    Vector2 lookInput
    float verticalLookRotation = 0  // для вертикального обзора камеры
    bool isGrounded
    bool isSprinting
```

**Инициализация:**
```
method Awake():
    rb = GetComponent<Rigidbody>()
    inputActions = new PlayerInputActions()

method OnEnable():
    inputActions.Player.Enable()

method OnDisable():
    inputActions.Player.Disable()

method Start():
    if NOT IsOwner:
        // Отключить управление для не-владельцев
        inputActions.Player.Disable()
        // Отключить компонент PlayerInput если есть
        GetComponent<PlayerInput>().enabled = false
        return
    
    // Заблокировать курсор для FPS управления
    Cursor.lockState = CursorLocked
    Cursor.visible = false
```

**Чтение ввода:**
```
method Update():
    if NOT IsOwner: return
    
    moveInput = inputActions.Player.Move.ReadValue<Vector2>()
    lookInput = inputActions.Player.Look.ReadValue<Vector2>()
    isSprinting = inputActions.Player.Sprint.IsPressed()
    
    HandleRotation()

method HandleRotation():
    // Горизонтальный поворот — вращаем весь объект
    float mouseX = lookInput.x * characterData.rotationSensitivity
    transform.Rotate(Vector3.up * mouseX)
    
    // Вертикальный поворот — только камера (через Cinemachine или CameraHolder)
    float mouseY = lookInput.y * characterData.rotationSensitivity
    verticalLookRotation -= mouseY
    verticalLookRotation = Clamp(verticalLookRotation, -80, 80)
    cameraHolder.localRotation = Quaternion.Euler(verticalLookRotation, 0, 0)
```

**Физическое движение (в FixedUpdate):**
```
method FixedUpdate():
    if NOT IsOwner: return
    
    CheckGround()
    Move()

method CheckGround():
    isGrounded = Physics.CheckSphere(groundCheck.position, characterData.groundCheckRadius, characterData.groundLayer)

method Move():
    float currentSpeed = isSprinting ? characterData.sprintSpeed : characterData.moveSpeed
    
    // Направление относительно персонажа
    Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x
    moveDirection = moveDirection.normalized
    
    // Целевая скорость
    Vector3 targetVelocity = moveDirection * currentSpeed
    
    // Сохраняем вертикальную скорость (гравитация)
    targetVelocity.y = rb.velocity.y
    
    // Применяем скорость
    rb.velocity = targetVelocity
```

### 4.6.4 Проблемы и решения

**Проблема: Скольжение по склонам**
- Решение: Использовать `rb.drag = 5` для естественного торможения
- Дополнительно: при отсутствии ввода, горизонтальную скорость гасить принудительно

**Проблема: Дрожание при контакте со стеной**
- Решение: `Collision Detection = Continuous`
- PhysicMaterial на игроке и стенах с `Dynamic Friction = 0`, `Static Friction = 0`, `Bounciness = 0`, `Friction Combine = Minimum`

**Проблема: Персонаж двигается в воздухе**
- Решение: Проверять `isGrounded` перед применением движения (или уменьшать управляемость в воздухе)

---

## 4.7 ШАГ 7: Настройка камеры (Cinemachine)

### 4.7.1 Тип камеры

Используем **вид от первого лица (FPS)**. Cinemachine Virtual Camera привязана к `CameraHolder` — дочернему объекту игрока.

### 4.7.2 Настройка на сцене

**На сцене (не в префабе!):**
- Main Camera → добавить компонент `CinemachineBrain`
- Создать `CinemachineVirtualCamera` (или `CinemachineCamera` в Cinemachine 3.x)

**В префабе игрока:**
- `CameraHolder` (child) → на позиции (0, 1.6, 0) — уровень глаз
- Virtual Camera будет программно привязываться к CameraHolder владельца

### 4.7.3 Программная привязка камеры

```
class PlayerCamera : NetworkBehaviour

    [SerializeField] Transform cameraHolder

    method OnNetworkSpawn():
        if IsOwner:
            // Найти или создать виртуальную камеру
            var vcam = FindOrCreateVirtualCamera()
            vcam.Follow = cameraHolder
            vcam.LookAt = null  // не нужно, мы управляем ротацией вручную
            
            // Настройки vcam
            vcam.m_Lens.FieldOfView = 75
            vcam.m_Lens.NearClipPlane = 0.1
            vcam.m_Lens.FarClipPlane = 100
            
            // Body: Transposer → Binding Mode = Lock To Target
            // Aim: Do Nothing (управляем ротацией вручную через PlayerController)
```

**Настройки Virtual Camera:**
| Параметр | Значение |
|---|---|
| Body | Transposer (или Hard Lock To Target) |
| Aim | Do Nothing |
| Follow | CameraHolder (Owner) |
| FOV | 75 |
| Near Clip | 0.1 |
| Damping | 0 (мгновенное следование для FPS) |

### 4.7.4 Важно: только одна камера для владельца
- На каждом клиенте виртуальная камера привязывается только к своему персонажу
- Чужие персонажи не имеют активных камер

### 4.7.5 Дополнительно: Cinemachine Impulse (подготовка)
- Добавить `CinemachineImpulseListener` на Virtual Camera — для будущих скримеров (тряска камеры)
- Пока не использовать, но подготовить

---

## 4.8 ШАГ 8: Спавн игроков по сети

### 4.8.1 Стратегия спавна

NGO по умолчанию спавнит Player Prefab при подключении клиента. Нам нужно контролировать позицию спавна.

### 4.8.2 SpawnManager

```
class SpawnManager : MonoBehaviour

    [SerializeField] Transform[] spawnPoints  // ссылки на SpawnPoint_1, SpawnPoint_2
    int nextSpawnIndex = 0

    method GetNextSpawnPoint() -> Transform:
        point = spawnPoints[nextSpawnIndex]
        nextSpawnIndex = (nextSpawnIndex + 1) % spawnPoints.Length
        return point
```

### 4.8.3 Переопределение спавна в NetworkManager

В `ConnectionManager` (или отдельном скрипте) подписаться на `NetworkManager.ConnectionApprovalCallback`:

```
method SetupSpawning():
    NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCallback

method ApprovalCallback(request, response):
    response.Approved = true
    response.CreatePlayerObject = true
    response.Position = spawnManager.GetNextSpawnPoint().position
    response.Rotation = spawnManager.GetNextSpawnPoint().rotation
```

**Альтернативный подход (проще для начала):**
- Не использовать ConnectionApproval
- В методе `OnNetworkSpawn()` на PlayerController — если IsOwner и IsServer, телепортировать к спавнпоинту
- Для клиентов — хост назначает позицию через ServerRpc при спавне

### 4.8.4 Отключение управления на чужих объектах

В каждом скрипте, реагирующем на ввод:
```
method OnNetworkSpawn():
    if NOT IsOwner:
        // Отключить компоненты ввода
        this.enabled = false  // или конкретные компоненты
```

### 4.8.5 Проверка

**Тест с ParrelSync:**
1. Хост запускается → спавнится в SpawnPoint_1 → может ходить и оглядываться
2. Клиент подключается → спавнится в SpawnPoint_2 → может ходить
3. Оба видят друг друга
4. Движения синхронизированы
5. Камера каждого привязана к своему персонажу
6. Один не может управлять другим

**Критерии успеха:**
- [ ] Оба игрока видят друг друга на сцене
- [ ] Движение плавное и синхронизированное
- [ ] Камера работает от первого лица
- [ ] Нет конфликтов ввода между игроками
- [ ] Позиции спавна разные

---

## 4.9 ШАГ 9: Система интерактивных предметов

### 4.9.1 Концепция

Предметы в игре **не складываются в инвентарь**. Игрок **несёт** предмет в руках (визуально перед собой). В один момент времени можно нести **только один предмет**. Взаимодействие: нажал ЛКМ — подобрал, нажал ещё раз — бросил.

### 4.9.2 InteractableBase — базовый класс предметов

```
class InteractableBase : NetworkBehaviour

    [SerializeField] string itemName
    [SerializeField] float interactionDistance = 2.0
    
    // Сетевые переменные
    NetworkVariable<bool> isCarried = new(false)
    NetworkVariable<ulong> carriedByClientId = new(0)
    
    Rigidbody itemRigidbody
    Collider itemCollider

    method Awake():
        itemRigidbody = GetComponent<Rigidbody>()
        itemCollider = GetComponent<Collider>()
    
    method OnNetworkSpawn():
        isCarried.OnValueChanged += OnCarriedStateChanged
    
    // Вызывается на сервере
    method PickUp(playerNetworkObjectId: ulong, holdPoint: Transform):
        if NOT IsServer: return
        if isCarried.Value: return  // уже несёт кто-то
        
        isCarried.Value = true
        carriedByClientId.Value = playerNetworkObjectId
        
        // Отключить физику предмета
        SetPhysicsClientRpc(false)
    
    method Drop(dropPosition: Vector3, dropForce: Vector3):
        if NOT IsServer: return
        
        isCarried.Value = false
        carriedByClientId.Value = 0
        
        // Вернуть физику
        SetPhysicsClientRpc(true)
        SetPositionClientRpc(dropPosition)
        // Можно добавить небольшую силу броска
    
    [ClientRpc]
    method SetPhysicsClientRpc(enabled: bool):
        itemRigidbody.isKinematic = NOT enabled
        itemCollider.enabled = enabled  // отключить коллайдер когда несём

    [ClientRpc]
    method SetPositionClientRpc(position: Vector3):
        transform.position = position
    
    method OnCarriedStateChanged(oldValue, newValue):
        // Визуальное обновление: если несут — скрыть/прикрепить к игроку
        // Если бросили — показать на земле
```

### 4.9.3 Сетевой предмет — NetworkObject

**Компоненты на префабе предмета:**
```
TestItem (Prefab)
├── NetworkObject
├── NetworkTransform (Server-Authoritative для предметов)
├── Rigidbody (Mass: 1, UseGravity: true)
├── BoxCollider
├── InteractableBase (наш скрипт)
└── MeshRenderer + MeshFilter (визуал)
```

**Важно:** NetworkTransform предметов — **server-authoritative** (в отличие от игроков). Хост управляет позицией предметов.

### 4.9.4 PlayerInteraction — скрипт взаимодействия игрока

```
class PlayerInteraction : NetworkBehaviour

    [SerializeField] Transform holdPoint        // InteractionPoint из префаба игрока
    [SerializeField] float interactDistance = 2.5
    [SerializeField] LayerMask interactableLayer
    
    InteractableBase currentlyCarriedItem = null
    bool isCarrying = false
    
    // Ссылка на Input
    PlayerInputActions inputActions

    method OnNetworkSpawn():
        if NOT IsOwner: return
        inputActions = new PlayerInputActions()
        inputActions.Player.Enable()
        inputActions.Player.Interact.performed += OnInteractPressed
    
    method OnInteractPressed(context):
        if NOT IsOwner: return
        
        if isCarrying:
            // Бросить предмет
            RequestDropServerRpc()
        else:
            // Попробовать подобрать
            TryPickUp()
    
    method TryPickUp():
        // Рейкаст из камеры (или из центра экрана) вперёд
        Ray ray = new Ray(cameraHolder.position, cameraHolder.forward)
        if Physics.Raycast(ray, out hit, interactDistance, interactableLayer):
            InteractableBase item = hit.collider.GetComponent<InteractableBase>()
            if item != null AND NOT item.isCarried.Value:
                RequestPickUpServerRpc(item.NetworkObjectId)
    
    // --- ServerRpc: запрос на подбор ---
    [ServerRpc]
    method RequestPickUpServerRpc(itemNetworkObjectId: ulong):
        // Валидация на сервере
        NetworkObject netObj = GetNetworkObject(itemNetworkObjectId)
        if netObj == null: return
        
        InteractableBase item = netObj.GetComponent<InteractableBase>()
        if item == null: return
        if item.isCarried.Value: return  // уже кто-то несёт
        
        // Проверка расстояния (анти-чит)
        float distance = Vector3.Distance(transform.position, item.transform.position)
        if distance > interactDistance + 1.0: return  // с запасом
        
        // Всё ок — подобрать
        item.PickUp(NetworkObjectId, holdPoint)
        
        // Уведомить клиента-владельца что он несёт предмет
        ConfirmPickUpClientRpc(itemNetworkObjectId, new ClientRpcParams { Send = { TargetClientIds = { OwnerClientId } } })
    
    [ClientRpc]
    method ConfirmPickUpClientRpc(itemNetworkObjectId: ulong):
        // На стороне клиента-владельца
        NetworkObject netObj = GetNetworkObject(itemNetworkObjectId)
        currentlyCarriedItem = netObj.GetComponent<InteractableBase>()
        isCarrying = true
    
    // --- ServerRpc: запрос на бросание ---
    [ServerRpc]
    method RequestDropServerRpc():
        if currentlyCarriedItem == null: return  // серверная проверка тоже нужна
        
        // Позиция броска: перед игроком, чуть ниже
        Vector3 dropPos = transform.position + transform.forward * 1.5 + Vector3.down * 0.5
        currentlyCarriedItem.Drop(dropPos, transform.forward * 2)
        
        ConfirmDropClientRpc(new ClientRpcParams { Send = { TargetClientIds = { OwnerClientId } } })
    
    [ClientRpc]
    method ConfirmDropClientRpc():
        currentlyCarriedItem = null
        isCarrying = false
    
    // --- Обновление позиции переносимого предмета ---
    method Update():
        if NOT IsOwner: return
        if NOT isCarrying: return
        if currentlyCarriedItem == null: return
        
        // Предмет следует за holdPoint (на стороне владельца)
        // Синхронизация позиции через NetworkTransform предмета
        UpdateCarriedItemPositionServerRpc(holdPoint.position, holdPoint.rotation)
    
    [ServerRpc]
    method UpdateCarriedItemPositionServerRpc(position: Vector3, rotation: Quaternion):
        if currentlyCarriedItem != null:
            currentlyCarriedItem.transform.position = position
            currentlyCarriedItem.transform.rotation = rotation
```

### 4.9.5 Оптимизация переноса предмета

**Проблема:** Отправка позиции каждый кадр — дорого.

**Решение:** 
- NetworkTransform предмета обрабатывает синхронизацию автоматически с интерполяцией
- Вместо отправки ServerRpc каждый кадр — на сервере привязать transform предмета к holdPoint игрока-носителя (parent/child или прямое обновление позиции в серверном Update)

**Улучшенный подход:**
```
// На сервере (в серверном Update или в FixedUpdate NetworkBehaviour)
method ServerUpdate():  // вызывается только на хосте
    if isCarried.Value:
        // Найти игрока-владельца
        NetworkObject ownerPlayer = GetNetworkObject(carriedByClientId.Value)
        if ownerPlayer != null:
            Transform playerHoldPoint = ownerPlayer.GetComponent<PlayerInteraction>().holdPoint
            transform.position = playerHoldPoint.position
            transform.rotation = playerHoldPoint.rotation
```

Это убирает необходимость в ServerRpc каждый кадр. Позиция обновляется серверно и синхронизируется через NetworkTransform предмета.

### 4.9.6 Визуальная обратная связь

**При подборе:**
- Предмет визуально появляется перед персонажем (в holdPoint)
- Можно добавить лёгкое покачивание при ходьбе (через Cinemachine Noise или DOTween)

**При наведении на предмет (до подбора):**
- Подсветка контура предмета (Outline shader)
- Или появление текста «[LMB] Pick Up» на UI
- Реализуется через рейкаст каждый кадр в Update, проверяя объект перед игроком

**Система обнаружения "что перед игроком":**
```
method Update():
    if NOT IsOwner: return
    
    // Рейкаст для обнаружения интерактивных объектов
    Ray ray = new Ray(cameraHolder.position, cameraHolder.forward)
    if Physics.Raycast(ray, out hit, interactDistance, interactableLayer):
        InteractableBase item = hit.collider.GetComponent<InteractableBase>()
        if item != null AND NOT item.isCarried.Value:
            ShowInteractionHint(item.itemName)  // UI подсказка
            // Подсветка объекта
        else:
            HideInteractionHint()
    else:
        HideInteractionHint()
```

### 4.9.7 Регистрация предметов в NetworkManager

- Все префабы интерактивных предметов добавить в **Network Prefabs List** в NetworkManager
- Предметы на сцене: если они размещены в редакторе (статично), на них должен быть `NetworkObject`, и они автоматически синхронизируются
- Если предметы спавнятся динамически (дьяволом) — используется `NetworkObject.Spawn()` на сервере

---

## 4.10 ШАГ 10: Сборка и финальное тестирование этапа 1

### 4.10.1 Чеклист функциональности

**Мультиплеер:**
- [ ] Хост создаёт сессию
- [ ] Клиент подключается по IP (localhost / LAN / Radmin VPN)
- [ ] Оба игрока видят друг друга
- [ ] Отключение обрабатывается корректно (другой игрок исчезает)
- [ ] Нет ошибок в логах при подключении/отключении

**Управление персонажем:**
- [ ] WASD перемещение работает
- [ ] Мышь — вертикальный и горизонтальный обзор
- [ ] Движение плавное (нет дёрганий)
- [ ] Персонаж не проваливается сквозь пол
- [ ] Персонаж не застревает в стенах
- [ ] Физика работает (gravity, collision)
- [ ] FreezeRotation работает (персонаж не падает набок)

**Камера:**
- [ ] Вид от первого лица
- [ ] Камера следует за персонажем мгновенно
- [ ] Вертикальный обзор ограничен (-80°, +80°)
- [ ] Каждый игрок видит из своей камеры

**Интерактивность:**
- [ ] Рейкаст обнаруживает предметы перед игроком
- [ ] ЛКМ подбирает предмет
- [ ] Предмет визуально появляется перед персонажем
- [ ] Повторный ЛКМ бросает предмет
- [ ] Предмет падает на пол с физикой
- [ ] Один предмет одновременно может нести только один игрок
- [ ] Второй игрок не может подобрать уже несомый предмет
- [ ] Всё синхронизировано между клиентами
- [ ] Подсказка при наведении (опционально)

**Сеть:**
- [ ] Нет рассинхронизации позиций
- [ ] Предметы корректно синхронизируются
- [ ] Нет критических задержек (на LAN/Radmin VPN)

### 4.10.2 Процедура тестирования

**Тест 1: Локальный (ParrelSync)**
1. Основной редактор → Play → Host
2. Клон редактора → Play → Connect (127.0.0.1)
3. Проверить все пункты чеклиста

**Тест 2: Два компьютера через Radmin VPN**
1. Собрать билд (File → Build Settings → Build)
2. Скопировать билд на второй компьютер
3. Оба компьютера в одной сети Radmin VPN
4. Компьютер 1: запустить билд → Host
5. Компьютер 2: запустить билд → ввести IP из Radmin → Connect
6. Проверить все пункты чеклиста

### 4.10.3 Известные проблемы и их решения

| Проблема | Возможная причина | Решение |
|---|---|---|
| Клиент не подключается | Firewall блокирует порт 7777 | Добавить правило в Windows Firewall для UDP 7777 |
| Клиент не подключается через Radmin | Неправильный IP | Использовать IP из интерфейса Radmin VPN, а не основной сетевой |
| Рассинхронизация позиции | Конфликт между Rigidbody и NetworkTransform | Убедиться что на клиенте Rigidbody kinematic для не-владельцев |
| Предмет «прыгает» при переносе | Конфликт NetworkTransform и ручного обновления позиции | Отключить NetworkTransform у предмета когда он несётся, обновлять вручную |
| Курсор не блокируется | Порядок вызовов | Вызвать Cursor.lockState в OnNetworkSpawn, не в Start |

### 4.10.4 Настройка билда

**Player Settings:**
- Product Name: Exorcism
- Resolution: 1920x1080 (Windowed для тестирования)
- Run In Background: **true** (важно для хоста!)
- API Compatibility Level: .NET Standard 2.1

**Оптимизация:**
- IL2CPP (для финального билда)
- Mono (для тестов, быстрее компилируется)

---

# 5. ЭТАП 2 — КАРТА, ОСВЕЩЕНИЕ, ТЕМНОТА

## 5.1 Описание этапа
Создание полноценной карты из 8 комнат с коридорами. Реализация системы освещения: полная темнота, синусоидальные камни, линейный камень.

## 5.2 Шаги

### Шаг 1: Проектирование карты
- Нарисовать план 8 комнат и коридоров на бумаге/в редакторе
- Определить размеры: комнаты 10x10 — 20x20 юнитов, коридоры 3x8
- Определить визуальные стили комнат (библиотека, подвал, часовня, склад, и т.д.)
- Продумать навигацию: игрок не должен легко заблудиться, но и карта не должна быть тривиальной
- Расставить точки для возможного размещения предметов (spawn points для книг, ножа, выхода)

### Шаг 2: Построение геометрии карты
- Создать модели комнат (ProBuilder или внешний 3D-редактор)
- Установить коллайдеры на все стены, пол, потолок
- Двери между комнатами (статичные проёмы, без анимации дверей на этом этапе)
- Occlusion Culling для оптимизации (камера не рендерит то, что за стенами)
- Навигационные метки и ориентиры (чтобы отличать комнаты)

### Шаг 3: Настройка полной темноты
- **Убрать все источники света** с карты
- Ambient Light в Lighting Settings → **Color: чёрный, Intensity: 0**
- Environment Lighting → Source: Color → Intensity: 0
- Skybox: чёрный или None
- Fog: отключён (или настроить для атмосферы)
- Результат: абсолютная темнота, ничего не видно без источника света

### Шаг 4: Реализация светящихся камней (синусоидальный свет)

**SinusoidalStone — скрипт:**
```
class SinusoidalStone : NetworkBehaviour

    [SerializeField] Light pointLight
    [SerializeField] float maxIntensity = 1.5
    [SerializeField] float minIntensity = 0.0
    [SerializeField] float cycleDuration = 15.0  // полный цикл 15 сек
    [SerializeField] float maxRange = 5.0  // радиус 0.8x (базовый множитель)
    
    method Update():
        // Синусоидальная интенсивность
        float t = (sin(2 * PI * Time.time / cycleDuration) + 1) / 2  // 0..1
        pointLight.intensity = Lerp(minIntensity, maxIntensity, t)
        pointLight.range = maxRange * t
```

- Каждый игрок (кроме Перса-2) имеет этот камень
- Камень является дочерним объектом персонажа (не предмет, не подбирается)
- Point Light на камне: цвет — тёплый голубой или белый

### Шаг 5: Реализация линейного камня (Перс-2)

**LinearStone — скрипт:**
```
class LinearStone : NetworkBehaviour

    [SerializeField] Light pointLight
    [SerializeField] float intensity = 2.0
    [SerializeField] float range = 6.0
    [SerializeField] float activeDuration = 10.0
    [SerializeField] float cooldown = 20.0
    
    bool isActive = false
    float timer = 0
    
    // Включается/выключается по кд
    // Когда активен — светит ровно (линейно, без пульсации)
    // Когда неактивен — не светит (или светит минимально)
```

### Шаг 6: Настройка дальности видимости

**Система множителей:**
- Базовая единица 1x = определённый радиус (например, 8 юнитов)
- Дьявол: видимость 1x = 8 юнитов → Point Light вокруг дьявола (видимый только дьяволу)
- Фонарь (Перс-4): 1.5x = 12 юнитов → Spot Light
- Камень: 0.8x = 6.4 юнитов → Point Light

**Реализация разной видимости:**
- Дьявол видит через пост-обработку: **Night Vision эффект** (зелёноватый URP Volume с Lift/Gamma/Gain) только на камере дьявола
- Или: невидимый свет вокруг дьявола (слой, который видит только камера дьявола через Culling Mask)

### Шаг 7: Система комнат (RoomManager)
```
class RoomManager : MonoBehaviour

    [SerializeField] Room[] rooms  // 8 комнат
    
    // Каждая комната знает свои SpawnPoints, возможные позиции для предметов
    // Для фазы подготовки дьявол выбирает: какая комната — алтарная, какая — логово
    
class Room : MonoBehaviour
    string roomId
    Transform[] itemSpawnPoints
    Transform[] playerSpawnPoints
    bool isAltarRoom
    bool isDevilLair
    Bounds roomBounds
```

### Шаг 8: Тестирование
- [ ] Карта полностью тёмная без источников света
- [ ] Камни светятся по синусоиде, цикл 15 сек
- [ ] Игроки видят только то, что освещено
- [ ] Все 8 комнат доступны и соединены
- [ ] Нет «дыр» в геометрии (провалы, просвечивание)
- [ ] Occlusion Culling работает корректно
- [ ] FPS стабилен (>60 на целевом железе)

---

# 6. ЭТАП 3 — ДЬЯВОЛ: УПРАВЛЕНИЕ, СПОСОБНОСТИ, ФАЗА ПОДГОТОВКИ

## 6.1 Описание этапа
Реализация отдельного персонажа-дьявола с уникальным управлением, видимостью, способностями и фазой подготовки.

## 6.2 Шаги

### Шаг 1: Создание префаба дьявола

**Структура:**
```
Devil (Prefab)
├── NetworkObject
├── ClientNetworkTransform
├── Rigidbody (аналогично игроку)
├── CapsuleCollider
├── DevilController (скрипт)
├── DevilAbilities (скрипт)
├── DevilNetwork (скрипт)
├── DevilHealth (скрипт)
│
├── DevilModel (child) — визуал
├── CameraHolder (child) — (0, 1.8, 0)
├── NightVisionLight (child) — Point Light, видимый только дьяволу
└── AttackPoint (child) — точка перед дьяволом для определения хитбокса атаки
```

### Шаг 2: Настройка видимости дьявола
- Создать Layer: `DevilVisionOnly`
- `NightVisionLight` → на этом слое
- Камера дьявола: Culling Mask включает `DevilVisionOnly`
- Камеры людей: Culling Mask НЕ включает `DevilVisionOnly`
- Дополнительно: пост-обработка на камере дьявола (лёгкий зеленоватый оттенок, как ноктовизор)

### Шаг 3: DevilController — управление

Аналогично PlayerController, но:
- Скорость выше (или настраиваемая через SO)
- Может проходить через определённые области (опционально)
- Не имеет камня освещения

**ScriptableObject:** `DevilDataSO`
```
DevilDataSO:
    int maxHealth = 150
    float moveSpeed = 6.0
    float aggressionSpeed = 9.0  // скорость в режиме агрессии
    float visionRange = 8.0      // 1x
    int basicAttackDamage = 15
    float basicAttackCooldown = 1.25  // среднее между 1 и 1.5
    int heavyAttackDamage = 30
    float heavyAttackCooldown = 37.5  // среднее между 30 и 45
    int heavyAttackMaxUses = 3
    int screamerTrapCount = 4
    float screamerBlindDuration = 5.0
    float aggressionDuration = 15.0
    float aggressionCooldown = 120.0  // 2 мин
```

### Шаг 4: Система здоровья дьявола (DevilHealth)

```
class DevilHealth : NetworkBehaviour

    NetworkVariable<int> currentHealth
    [SerializeField] DevilDataSO data
    
    method OnNetworkSpawn():
        if IsServer:
            currentHealth.Value = data.maxHealth  // 150
    
    [ServerRpc(RequireOwnership = false)]  
    method TakeDamageServerRpc(int damage, ServerRpcParams rpcParams):
        // Валидация: только зачарованный нож может нанести урон сердцу
        // Прямой урон дьяволу — пока не предусмотрен (урон по сердцу — в Этапе 5)
        currentHealth.Value -= damage
        if currentHealth.Value <= 0:
            Die()
    
    method Die():
        // Дьявол мёртв → люди победили
        GameManager.Instance.EndGame(GameResult.HumansWin_KilledDevil)
```

### Шаг 5: Способности дьявола (DevilAbilities)

**5.1 Обычная атака:**
```
method BasicAttack():
    if NOT CanBasicAttack(): return  // проверка кд
    
    // Сфера-каст перед дьяволом
    hits = Physics.OverlapSphere(attackPoint.position, attackRadius, playerLayer)
    for each hit in hits:
        playerHealth = hit.GetComponent<PlayerHealth>()
        if playerHealth != null:
            playerHealth.TakeDamageServerRpc(data.basicAttackDamage)
    
    StartCooldown(basicAttackCooldown)
```

**5.2 Усиленная атака:**
```
method HeavyAttack():
    if NOT CanHeavyAttack(): return  // кд + остались использования
    if heavyAttackUsesRemaining <= 0: return
    
    // Аналогично, но больше урона
    hits = Physics.OverlapSphere(attackPoint.position, attackRadius * 1.3, playerLayer)
    for each hit in hits:
        playerHealth = hit.GetComponent<PlayerHealth>()
        if playerHealth != null:
            playerHealth.TakeDamageServerRpc(data.heavyAttackDamage)  // 30
    
    heavyAttackUsesRemaining--
    StartCooldown(heavyAttackCooldown)
```

**5.3 Ловушки-скримеры:**
```
method PlaceScreamerTrap():
    if screamerTrapsRemaining <= 0: return
    
    // Спавнить ловушку на сервере
    SpawnScreamerTrapServerRpc(transform.position + transform.forward * 2)
    screamerTrapsRemaining--
```

**5.4 Режим агрессии:**
```
method ActivateAggression():
    if NOT CanActivateAggression(): return  // кд 2мин
    
    ActivateAggressionServerRpc()

[ServerRpc]
method ActivateAggressionServerRpc():
    // Увеличить скорость на 15 сек
    SetAggressionClientRpc(true)
    StartCoroutine(DeactivateAfterDelay(data.aggressionDuration))

method DeactivateAfterDelay(duration):
    wait duration seconds
    SetAggressionClientRpc(false)
    StartCooldown(aggressionCooldown)
```

### Шаг 6: Фаза подготовки дьявола (DevilSetupPhase)

**Состояние игры:** `GameState.DevilSetup`

**Что происходит:**
1. Дьявол спавнится на карте один (люди ещё не заспавнены)
2. Таймер: 2–3 минуты
3. Дьявол свободно перемещается по карте и:
   - Выбирает позиции для 3 книг (из предопределённых слотов в комнатах) → размещает
   - Выбирает комнату алтаря (подходит к специальной точке и активирует)
   - Выбирает место для логова (размещает объект «логово» в выбранной комнате)
   - Расставляет 4 скримера-ловушки
   - Определяет позицию выхода (скрытая дверь)
4. По окончании таймера → GameState.Playing, спавнятся люди

**UI для дьявола в фазе подготовки:**
```
DevilSetup HUD:
├── Timer: "1:45"
├── Checklist:
│   ├── [✓] Books placed: 2/3
│   ├── [ ] Altar room selected
│   ├── [ ] Lair placed
│   ├── [✓] Screamer traps: 3/4
│   └── [ ] Exit placed
├── Instructions: "Place remaining items before time runs out!"
```

**Если дьявол не успел разместить всё:**
- Нераскиданные предметы размещаются автоматически в случайных точках

### Шаг 7: Привязка ввода дьявола

**Input Actions для дьявола (дополнительный ActionMap или тот же Player):**
| Action | Binding | Описание |
|---|---|---|
| Move | WASD | Перемещение |
| Look | Mouse | Обзор |
| BasicAttack | LMB | Обычная атака |
| HeavyAttack | RMB | Усиленная атака |
| PlaceTrap | Q | Разместить ловушку |
| Aggression | E | Режим агрессии |
| Interact | F | Размещение объектов (фаза подготовки) |

### Шаг 8: Тестирование
- [ ] Дьявол управляется отдельно от людей
- [ ] Дьявол видит в темноте (ночное зрение)
- [ ] Базовая атака работает, наносит 15хп, кд 1-1.5 сек
- [ ] Усиленная атака: 30хп, кд 30-45 сек, максимум 3 раза
- [ ] Ловушки-скримеры размещаются (4 штуки)
- [ ] Режим агрессии: скорость увеличивается на 15 сек, кд 2 мин
- [ ] Фаза подготовки: дьявол может расставить всё за 2-3 минуты
- [ ] Переход из фазы подготовки в фазу игры

---

# 7. ЭТАП 4 — ПЕРСОНАЖИ ЛЮДЕЙ: УНИКАЛЬНЫЕ СПОСОБНОСТИ

## 7.1 Описание этапа
Реализация 4 уникальных персонажей со своими характеристиками и способностями. Система выбора/назначения ролей.

## 7.2 Шаги

### Шаг 1: Система здоровья игроков (PlayerHealth)

```
class PlayerHealth : NetworkBehaviour

    NetworkVariable<int> currentHealth
    NetworkVariable<bool> isAlive = new(true)
    [SerializeField] CharacterDataSO characterData
    
    method OnNetworkSpawn():
        if IsServer:
            currentHealth.Value = characterData.maxHealth
    
    [ServerRpc(RequireOwnership = false)]
    method TakeDamageServerRpc(int damage):
        if NOT isAlive.Value: return
        currentHealth.Value -= damage
        if currentHealth.Value <= 0:
            currentHealth.Value = 0
            isAlive.Value = false
            DieClientRpc()
            GameManager.Instance.CheckPlayersAlive()
    
    [ServerRpc(RequireOwnership = false)]
    method HealServerRpc(int amount):
        if NOT isAlive.Value: return
        currentHealth.Value = Min(currentHealth.Value + amount, characterData.maxHealth)
    
    [ClientRpc]
    method DieClientRpc():
        // Отключить управление, показать экран смерти
        // Можно перейти в режим наблюдателя
```

### Шаг 2: Перс-1 — Ловушки и Стены (Trapper)

**Данные (`CharacterDataSO`):**
- HP: 110
- Скорость: стандартная

**Способности:**

**Капканы:**
```
Trapper:
    int maxBearTraps = 3
    
    method PlaceBearTrap():
        if bearTrapsRemaining <= 0: return
        SpawnBearTrapServerRpc(position перед персонажем)
        bearTrapsRemaining--

BearTrap (NetworkBehaviour):
    // При попадании дьявола — замедление на X секунд
    method OnTriggerEnter(other):
        if IsServer AND other is Devil:
            devil.ApplySlow(slowDuration)
            DestroyTrap()
```

**Стены:**
```
    int maxWalls = 2
    
    method PlaceWall():
        if wallsRemaining <= 0: return
        SpawnWallServerRpc(position перед персонажем, rotation)
        wallsRemaining--

DestructibleWall (NetworkBehaviour):
    NetworkVariable<int> wallHealth = new(3)  // 3-4 удара
    
    method TakeDamage():
        wallHealth.Value--
        if wallHealth.Value <= 0:
            DestroyWall()
```

### Шаг 3: Перс-2 — Целитель (Healer)

**Данные:**
- HP: 80
- Реген: 25 хп/мин (≈ 0.42 хп/сек)
- Собственный осветительный камень (линейный, см. Этап 2)

**Способности:**

**Лечение:**
```
Healer:
    float healAmount = 12.5  // среднее 10-15
    float healCooldown = 10.0
    float healRange = 3.0
    
    method HealTarget():
        if onCooldown: return
        
        // Рейкаст или OverlapSphere для поиска союзника
        target = FindNearestAlly(healRange)
        if target != null:
            target.HealServerRpc(healAmount)
            StartCooldown(healCooldown)
```

**Пассивная регенерация:**
```
    method FixedUpdate():  // на сервере
        if IsServer AND isAlive:
            regenAccumulator += 25.0 / 60.0 * Time.fixedDeltaTime
            if regenAccumulator >= 1.0:
                currentHealth.Value = Min(currentHealth.Value + 1, maxHealth)
                regenAccumulator -= 1.0
```

### Шаг 4: Перс-3 — Экзорцист (Exorcist)

**Данные:**
- HP: 90
- Единственный кто может читать заклинания у алтаря

**Способности:**

**Телепортационный прибор:**
```
Exorcist:
    bool devicePlaced = false
    bool deviceDestroyed = false
    NetworkObject deviceObject
    
    method PlaceDevice():
        if devicePlaced OR deviceDestroyed: return
        SpawnDeviceServerRpc(transform.position)
        devicePlaced = true
    
    method Teleport():
        if NOT devicePlaced OR deviceDestroyed: return
        TeleportServerRpc()
    
    [ServerRpc]
    method TeleportServerRpc():
        // Телепортировать игрока к позиции прибора
        transform.position = deviceObject.transform.position
        // Синхронизация через NetworkTransform
    
TeleportDevice (NetworkBehaviour):
    method TakeDamage():  // дьявол может сломать
        DestroyDeviceServerRpc()
    
    [ServerRpc]
    method DestroyDeviceServerRpc():
        exorcist.deviceDestroyed = true
        NetworkObject.Despawn()
```

**Чтение заклинания:**
```
    method ReadIncantation():
        // Можно только рядом с алтарём и когда все 3 книги доставлены
        if NOT IsNearAltar(): return
        if GameManager.BooksDelivered < 3: return
        
        StartReadingServerRpc()
    
    [ServerRpc]
    method StartReadingServerRpc():
        // Запуск процесса чтения (5-10 секунд)
        // Во время чтения — уязвим (не может двигаться)
        // Если прерван (получил урон) — начинает заново
        StartCoroutine(ReadingProcess())
    
    Coroutine ReadingProcess():
        isReading = true
        DisableMovement()
        for t = 0 to readingDuration:
            if wasInterrupted:
                isReading = false
                EnableMovement()
                return
            wait 1 second
        // Успешно прочитано!
        GameManager.Instance.EndGame(GameResult.HumansWin_Exorcism)
```

### Шаг 5: Перс-4 — Следопыт (Scout)

**Данные:**
- HP: 100
- Единственный кто может открыть логово дьявола

**Способности:**

**Фонарь:**
```
Scout:
    [SerializeField] Light flashlight  // Spot Light
    bool flashlightOn = true
    // Дальность: 1.5x = 12 юнитов
    // Spot Angle: 45°
    
    method ToggleFlashlight():
        flashlightOn = NOT flashlightOn
        ToggleFlashlightServerRpc(flashlightOn)
```

**Свечи (3 штуки):**
```
    int candlesRemaining = 3
    
    method PlaceCandle():
        if candlesRemaining <= 0: return
        SpawnCandleServerRpc(transform.position)
        candlesRemaining--
    
Candle (NetworkBehaviour):
    float burnDuration = 60.0  // 1 минута
    Light candleLight
    
    method OnNetworkSpawn():
        if IsServer:
            StartCoroutine(BurnDown())
    
    Coroutine BurnDown():
        wait burnDuration
        NetworkObject.Despawn()
```

**Перетяжки (2 штуки):**
```
    int tripwiresRemaining = 2
    
    method PlaceTripwire():
        if tripwiresRemaining <= 0: return
        SpawnTripwireServerRpc(transform.position, transform.forward)
        tripwiresRemaining--
    
Tripwire (NetworkBehaviour):
    method OnTriggerEnter(other):
        if other is Devil AND IsServer:
            // Показать позицию дьявола на карте Скаута на 3 сек
            scout.RevealDevilPositionClientRpc(devil.transform.position, 3.0)
            DestroyTripwire()
```

**Открытие логова:**
```
    method OpenLair():
        // Рейкаст на дверь логова
        if hit.GetComponent<DevilLairDoor>() != null:
            OpenLairServerRpc(hit.NetworkObjectId)
    
    [ServerRpc]
    method OpenLairServerRpc(doorId):
        door = GetNetworkObject(doorId).GetComponent<DevilLairDoor>()
        door.Open()  // открывает дверь, все могут войти
```

### Шаг 6: Привязка ввода для каждого персонажа

**Дополнительные Input Actions (добавить в существующий ActionMap Player):**
| Action | Binding | Описание |
|---|---|---|
| Ability1 | Q | Основная способность |
| Ability2 | E | Вторая способность |
| Ability3 | R | Третья способность (если есть) |
| Ability4 | F | Четвёртая способность (если есть) |

Каждый персонаж маппит свои способности к этим действиям по-разному через Strategy pattern.

### Шаг 7: Система назначения ролей (RoleAssignmentService)

```
class RoleAssignmentService : NetworkBehaviour

    // Вызывается на сервере в начале игры
    method AssignRoles():
        clients = NetworkManager.Singleton.ConnectedClientsList  // 5 штук
        
        // Случайный выбор дьявола
        devilIndex = Random(0, clients.Count)
        
        // Оставшиеся 4 — люди, случайно раздать 4 роли
        humanRoles = Shuffle([Human1_Trapper, Human2_Healer, Human3_Exorcist, Human4_Scout])
        
        roleIndex = 0
        for i = 0 to clients.Count:
            if i == devilIndex:
                AssignRole(clients[i].ClientId, Role.Devil)
            else:
                AssignRole(clients[i].ClientId, humanRoles[roleIndex])
                roleIndex++
    
    method AssignRole(clientId, role):
        // Спавнить правильный префаб для этого клиента
        // Деспавнить стандартный Player Prefab
        // Спавнить роль-специфичный префаб с owner = clientId
        AssignRoleClientRpc(clientId, role)
```

### Шаг 8: Тестирование
- [ ] Все 4 человеческих персонажа работают
- [ ] HP корректны для каждого (110, 80, 90, 100)
- [ ] Способности каждого перса работают
- [ ] Роли назначаются случайно
- [ ] Правильные префабы спавнятся для каждой роли
- [ ] Дьявол не может использовать способности людей и наоборот
- [ ] Телепортация Экзорциста работает по сети
- [ ] Лечение Целителя синхронизировано
- [ ] Ловушки Траппера работают на дьявола
- [ ] Фонарь и свечи Скаута освещают для всех

---

# 8. ЭТАП 5 — ИГРОВЫЕ СИСТЕМЫ: КНИГИ, АЛТАРЬ, СЕРДЦЕ, ВЫХОД

## 8.1 Описание этапа
Реализация трёх путей победы для людей: изгнание, убийство сердца, побег.

## 8.2 Шаги

### Шаг 1: Система книг

**Book (InteractableBase):**
```
class Book : InteractableBase
    // Наследуется от InteractableBase
    // Подбирается и переносится как обычный предмет
    // Можно бросить рядом с алтарём для «доставки»
    
    // Книга светится слабым светом (чтобы было видно в темноте)
    [SerializeField] Light bookGlow  // слабый Point Light, range 2
```

- 3 книги размещаются дьяволом в фазе подготовки
- Каждый игрок может нести одну книгу
- Книга — физический объект (Rigidbody)
- При бросании рядом с алтарём — проверка на доставку

### Шаг 2: Алтарь

**Altar (NetworkBehaviour):**
```
class Altar : NetworkBehaviour
    NetworkVariable<int> booksDelivered = new(0)
    [SerializeField] float deliveryRadius = 3.0
    [SerializeField] Transform[] bookSlots  // 3 позиции для визуального размещения книг
    
    // Триггер-зона вокруг алтаря
    method OnTriggerEnter(other):
        if IsServer:
            Book book = other.GetComponent<Book>()
            if book != null AND NOT book.isCarried.Value:
                // Книга доставлена!
                DeliverBook(book)
    
    method DeliverBook(book):
        booksDelivered.Value++
        // Разместить книгу визуально на алтаре
        book.transform.position = bookSlots[booksDelivered.Value - 1].position
        book.GetComponent<Rigidbody>().isKinematic = true
        book.GetComponent<NetworkObject>().Despawn()  // или оставить как декорацию
        
        if booksDelivered.Value >= 3:
            // Уведомить всех: можно читать заклинание
            NotifyReadyForIncantationClientRpc()
```

### Шаг 3: Сердце дьявола

**DevilHeart (NetworkBehaviour):**
```
class DevilHeart : NetworkBehaviour
    NetworkVariable<int> heartHealth = new(100)
    
    // Может получить урон ТОЛЬКО от зачарованного ножа
    [ServerRpc(RequireOwnership = false)]
    method TakeDamageServerRpc(int damage, ulong attackerClientId):
        // Проверить: у атакующего в руках зачарованный нож?
        PlayerInteraction attacker = GetPlayerByClientId(attackerClientId)
        if attacker.currentlyCarriedItem is EnchantedKnife:
            heartHealth.Value -= damage
            if heartHealth.Value <= 0:
                GameManager.Instance.EndGame(GameResult.HumansWin_KilledDevil)
```

**Логово дьявола:**
```
class DevilLair : NetworkBehaviour
    NetworkVariable<bool> isOpen = new(false)
    [SerializeField] GameObject door
    // Внутри логова: сердце
    
    method Open():  // только Перс-4 может вызвать
        isOpen.Value = true
        door.SetActive(false)  // открыть дверь
        OpenLairClientRpc()
```

### Шаг 4: Зачарованный нож

**EnchantedKnife (InteractableBase):**
```
class EnchantedKnife : InteractableBase
    int knifeDamage = 25
    float attackCooldown = 2.0
    
    // Подбирается как обычный предмет
    // Когда в руках у игрока — при ЛКМ рядом с сердцем наносит урон
    // Или: отдельная кнопка атаки когда нож в руках
    
    method AttackHeart():
        // Рейкаст / OverlapSphere для поиска сердца
        if heartInRange:
            heart.TakeDamageServerRpc(knifeDamage, OwnerClientId)
```

### Шаг 5: Скрытый выход

**ExitDoor (NetworkBehaviour):**
```
class ExitDoor : NetworkBehaviour
    NetworkVariable<bool> isDiscovered = new(false)
    
    // Скрытая дверь: визуально выглядит как стена
    // При приближении (или при взаимодействии) — открывается
    
    method OnTriggerEnter(other):
        if IsServer AND other is Player:
            DiscoverExitClientRpc()
    
    method UseExit(playerClientId):
        // Игрок «убегает»
        // Проверить: все ли живые игроки у выхода? Или достаточно одного?
        // Вариант: все живые должны добраться
        // Или: каждый может убежать по одиночке
        GameManager.Instance.PlayerEscaped(playerClientId)
```

### Шаг 6: GameProgressTracker

```
class GameProgressTracker : NetworkBehaviour
    // Отслеживает общий прогресс
    
    NetworkVariable<int> booksDelivered
    NetworkVariable<int> heartHealth
    NetworkVariable<int> playersAlive
    NetworkVariable<int> playersEscaped
    NetworkVariable<bool> incantationComplete
    
    method CheckWinConditions():
        // Вызывается после каждого значимого события
        
        if incantationComplete.Value:
            EndGame(HumansWin_Exorcism)
        
        if heartHealth.Value <= 0:
            EndGame(HumansWin_KilledDevil)
        
        if playersEscaped.Value >= playersAlive.Value AND playersAlive.Value > 0:
            EndGame(HumansWin_Escaped)
        
        if playersAlive.Value <= 0:
            EndGame(DevilWins)
```

### Шаг 7: Тестирование
- [ ] Книги можно подобрать и донести до алтаря
- [ ] Счётчик книг на алтаре работает (0→1→2→3)
- [ ] После 3 книг Экзорцист может читать заклинание
- [ ] Чтение заклинания занимает время и может быть прервано
- [ ] Логово дьявола закрыто; только Скаут может открыть
- [ ] Сердце внутри логова; урон только зачарованным ножом
- [ ] Нож можно найти и подобрать
- [ ] Выход скрыт; при нахождении — можно сбежать
- [ ] Все условия победы работают корректно
- [ ] Конец игры отображается для всех

---

# 9. ЭТАП 6 — ЛОВУШКИ, СКРИМЕРЫ, ЭФФЕКТЫ

## 9.1 Описание этапа
Реализация системы ловушек, скример-эффектов, визуальных и звуковых эффектов ужаса.

## 9.2 Шаги

### Шаг 1: Ловушки-скримеры дьявола

**ScreamerTrap (NetworkBehaviour):**
```
class ScreamerTrap : NetworkBehaviour
    [SerializeField] float triggerRadius = 2.0
    [SerializeField] float blindDuration = 5.0
    
    // Невидима для людей (или почти невидима — лёгкое мерцание)
    // При приближении человека — срабатывает
    
    method OnTriggerEnter(other):
        if IsServer AND other.GetComponent<PlayerHealth>() != null:
            TriggerScreamer(other.GetComponent<NetworkObject>().OwnerClientId)
    
    method TriggerScreamer(targetClientId):
        // Скример-эффект на целевом клиенте
        ScreamerEffectClientRpc(new ClientRpcParams { TargetClientIds = { targetClientId } })
        // Уничтожить ловушку
        NetworkObject.Despawn()
    
    [ClientRpc]
    method ScreamerEffectClientRpc():
        // На клиенте жертвы:
        // 1. Воспроизвести громкий звук скримера
        // 2. Показать пугающее изображение (на долю секунды)
        // 3. Тряска камеры (Cinemachine Impulse)
        // 4. Ослепление: белый/чёрный экран на blindDuration (5 сек)
        // 5. Через 5 сек — вернуть нормальный вид
        
        AudioManager.PlayScreamer()
        CameraShake.Shake(intensity: 5, duration: 0.5)
        StartCoroutine(BlindEffect(blindDuration))
    
    Coroutine BlindEffect(duration):
        blindOverlay.SetActive(true)  // полностью перекрывает экран
        playerController.DisableInput()
        wait duration
        blindOverlay.SetActive(false)
        playerController.EnableInput()
```

### Шаг 2: Капканы Траппера (для дьявола)

```
class BearTrap : NetworkBehaviour
    float slowDuration = 3.0
    float slowMultiplier = 0.5  // 50% скорости
    
    method OnTriggerEnter(other):
        if IsServer AND other.GetComponent<DevilController>() != null:
            ApplySlowServerRpc(other.OwnerClientId)
            // Визуал: капкан закрывается
            SnapTrapClientRpc()
    
    // Через N секунд капкан разрушается
```

### Шаг 3: Визуальные эффекты (URP Post Processing)

**Volume Profile для людей:**
- Vignette: высокая интенсивность (ужас, ограничение обзора)
- Film Grain: слабый (атмосфера)
- Color Adjustments: слегка приглушённые цвета, холодные тона
- При получении урона: кратковременно красная виньетка + Chromatic Aberration

**Volume Profile для дьявола:**
- Lift/Gamma/Gain: зеленоватые тона (ноктовизор)
- Bloom: лёгкий
- Отсутствие виньетки (дьявол видит лучше)

**Динамические эффекты:**
```
class PostProcessingEffects : MonoBehaviour
    Volume damageVolume
    Volume fearVolume
    
    method ShowDamageEffect():
        // Плавно показать и скрыть красную виньетку
        DOTween damageVolume.weight from 0 to 1 over 0.1s
        then from 1 to 0 over 0.5s
    
    method ShowFearEffect(duration):
        // Усилить виньетку, добавить chromatic aberration
        DOTween fearVolume.weight from 0 to 1 over 0.2s
        wait duration
        DOTween fearVolume.weight from 1 to 0 over 1.0s
```

### Шаг 4: Звуковой дизайн (структура)

**Категории звуков:**
| Категория | Примеры |
|---|---|
| Ambient | Гул, капающая вода, скрипы |
| Footsteps | Шаги игроков (разные поверхности) |
| Horror | Скримеры, внезапные звуки |
| UI | Клики, уведомления |
| Abilities | Звуки способностей |
| Interaction | Подбор предмета, бросание |

**AudioManager (Singleton):**
```
class AudioManager : Singleton<AudioManager>
    [SerializeField] AudioSource ambientSource
    [SerializeField] AudioSource sfxSource
    [SerializeField] AudioSource screamerSource
    
    method PlaySFX(clip, volume, position)
    method PlayScreamer()
    method PlayAmbient(clip)
    method StopAmbient()
```

**Сетевая синхронизация звуков:**
- Шаги — воспроизводятся локально для каждого (через AudioSource на персонаже, 3D spatial)
- Скримеры — воспроизводятся только у жертвы (ClientRpc целевой)
- Эмбиент — локальный, не синхронизируется

### Шаг 5: Тряска камеры (Cinemachine Impulse)

```
Настройка:
1. CinemachineImpulseSource — добавить на источники тряски
2. CinemachineImpulseListener — на Virtual Camera каждого игрока

Использование:
    impulseSource.GenerateImpulse(force)  // при скримере, получении урона
```

### Шаг 6: Тестирование
- [ ] Скримеры: звук + изображение + тряска + ослепление 5 сек
- [ ] Скримеры срабатывают только на людей
- [ ] Капканы замедляют дьявола
- [ ] Пост-обработка корректна для людей и дьявола
- [ ] Эффект урона (красная виньетка)
- [ ] Звуки воспроизводятся корректно
- [ ] 3D-звук шагов (слышно откуда идёт)

---

# 10. ЭТАП 7 — ИГРОВОЙ ЦИКЛ, UI, ЛОББИ

## 10.1 Описание этапа
Полный игровой цикл от входа в лобби до конца игры. Полноценный UI. Система лобби.

## 10.2 Шаги

### Шаг 1: Сцены и переходы

```
Порядок сцен:
BootScene → MainMenuScene → LobbyScene → GameScene → ResultScene
                                ↑                        │
                                └────────────────────────┘
```

**BootScene:**
- Инициализация сервисов (GameManager, AudioManager, NetworkManager)
- Автоматический переход на MainMenu

**MainMenuScene:**
- Главное меню: Play, Settings, Quit
- Play → LobbyScene

### Шаг 2: Лобби (LobbyScene)

**UI лобби:**
```
LobbyCanvas:
├── Panel_HostOrJoin
│   ├── Button_Host ("Create Room")
│   ├── InputField_IP ("Enter IP")
│   ├── Button_Join ("Join Room")
│   └── Button_Back
│
├── Panel_Lobby (показывается после создания/присоединения)
│   ├── Text_RoomInfo ("Room IP: 26.45.x.x")
│   ├── PlayerList:
│   │   ├── PlayerSlot_1 (Name, Ready status)
│   │   ├── PlayerSlot_2
│   │   ├── PlayerSlot_3
│   │   ├── PlayerSlot_4
│   │   └── PlayerSlot_5
│   ├── Button_Ready ("Ready")
│   └── Button_Start ("Start Game") — только у хоста, активен когда все Ready
│
└── Panel_Settings
    ├── Slider_Sensitivity
    ├── Slider_Volume
    └── Dropdown_Resolution
```

**Логика лобби:**
```
class LobbyManager : NetworkBehaviour
    NetworkList<PlayerLobbyData> players
    
    struct PlayerLobbyData : INetworkSerializable
        ulong clientId
        FixedString32Bytes playerName
        bool isReady
    
    method OnClientConnected(clientId):
        if IsServer:
            players.Add(new PlayerLobbyData { clientId, name, isReady = false })
            UpdateLobbyUIClientRpc()
    
    method ToggleReady(clientId):
        // Переключить ready статус
    
    method StartGame():
        if NOT IsServer: return
        if NOT AllPlayersReady(): return
        if players.Count != 5: return  // или разрешить меньше для тестов
        
        // Назначить роли
        RoleAssignmentService.AssignRoles()
        
        // Загрузить игровую сцену
        NetworkManager.SceneManager.LoadScene("GameScene", LoadSceneMode.Single)
```

### Шаг 3: HUD во время игры

**HUD для людей:**
```
GameHUD_Human:
├── Panel_Health
│   ├── HealthBar (полоска HP)
│   └── Text_HP ("90/90")
│
├── Panel_Abilities
│   ├── AbilitySlot_Q (иконка + кд)
│   ├── AbilitySlot_E (иконка + кд)
│   └── AbilitySlot_R (иконка + кд)
│
├── Panel_CarriedItem
│   └── Icon + Name (если несёт предмет)
│
├── Panel_InteractionHint
│   └── Text "[LMB] Pick Up" (появляется при наведении)
│
├── Panel_Objective
│   └── Text "Find the books" / "Deliver to altar" / etc.
│
├── Panel_Teammates (мини-статусы)
│   ├── Teammate_1 (имя + HP бар)
│   ├── Teammate_2
│   └── Teammate_3
│
└── Panel_MiniMap (опционально, для Скаута при перетяжке)
```

**HUD для дьявола:**
```
GameHUD_Devil:
├── Panel_Health (150 HP)
├── Panel_Abilities
│   ├── BasicAttack (кд)
│   ├── HeavyAttack (кд + счётчик 3/3)
│   ├── Screamer (счётчик 4/4)
│   └── Aggression (кд)
├── Panel_Setup (только в фазе подготовки)
│   └── Checklist
└── Panel_KillFeed
    └── "[PlayerName] killed!" (при убийстве)
```

### Шаг 4: Экран смерти и наблюдение

```
Когда игрок умирает:
1. Экран затемняется
2. Текст "YOU DIED"
3. Переход в режим наблюдателя (свободная камера или камера выжившего союзника)

SpectatorMode:
    method EnterSpectator():
        // Отключить PlayerController
        // Переключить камеру на свободную или на союзника
        // Возможность переключаться между союзниками (LMB/RMB)
```

### Шаг 5: Экран результатов

```
ResultScreen:
├── Text_Result ("HUMANS WIN!" / "DEVIL WINS!")
├── Text_Method ("Exorcism" / "Heart Destroyed" / "Escaped" / "All Humans Killed")
├── Panel_Stats
│   ├── PlayerStat_1 (role, survived, damage dealt, etc.)
│   ├── ...
│   └── PlayerStat_5
├── Button_BackToLobby
└── Button_Quit
```

### Шаг 6: Пауза и настройки в игре

```
PauseMenu (ESC):
├── Button_Resume
├── Slider_Sensitivity
├── Slider_Volume
├── Button_Disconnect ("Leave Game")
└── Text_Warning ("Leaving will disconnect you from the game")
```

**Важно:** Пауза НЕ останавливает игру для других (мультиплеер). Это просто локальное меню.

### Шаг 7: Полный игровой цикл (GameManager State Machine)

```
class GameManager : NetworkBehaviour, Singleton

    NetworkVariable<GameState> currentState
    
    enum GameState:
        WaitingForPlayers  // лобби
        RoleAssignment     // раздача ролей
        DevilSetup         // дьявол расставляет предметы
        Playing            // основная игра
        GameOver           // конец

    method TransitionTo(newState):
        currentState.Value = newState
        
        switch newState:
            case RoleAssignment:
                RoleAssignmentService.AssignRoles()
                wait 5 seconds (показать игрокам их роли)
                TransitionTo(DevilSetup)
            
            case DevilSetup:
                SpawnDevil()
                StartSetupTimer(180 seconds)  // 3 минуты
            
            case Playing:
                SpawnHumans()
                StartGameTimer() // опционально, лимит игры
            
            case GameOver:
                ShowResults()
                wait 10 seconds
                ReturnToLobby()
    
    method EndGame(result: GameResult):
        gameResult = result
        TransitionTo(GameOver)
```

### Шаг 8: Тестирование
- [ ] Полный цикл: меню → лобби → роли → подготовка → игра → конец → лобби
- [ ] UI корректно обновляется
- [ ] HUD отображает правильную информацию
- [ ] Лобби работает с 2-5 игроками
- [ ] Ready/Start работают
- [ ] Экран смерти и наблюдение
- [ ] Экран результатов
- [ ] Пауза-меню
- [ ] Отключение из игры

---

# 11. ЭТАП 8 — ПОЛИРОВКА, БАЛАНС, ТЕСТИРОВАНИЕ

## 11.1 Описание этапа
Финальная полировка, балансировка, оптимизация и полноценное тестирование.

## 11.2 Шаги

### Шаг 1: Балансировка

**Параметры для настройки (через ScriptableObjects):**

| Параметр | Текущее значение | Диапазон для тестов |
|---|---|---|
| HP Перс-1 (Trapper) | 110 | 90-130 |
| HP Перс-2 (Healer) | 80 | 70-100 |
| HP Перс-3 (Exorcist) | 90 | 80-110 |
| HP Перс-4 (Scout) | 100 | 85-120 |
| HP Дьявола | 150 | 120-200 |
| Базовая атака | 15 | 10-20 |
| Усиленная атака | 30 | 25-40 |
| Кд базовой атаки | 1.25с | 0.8-2.0с |
| Кд усиленной атаки | 37.5с | 20-60с |
| Количество усиленных атак | 3 | 2-5 |
| Длительность ослепления | 5с | 3-8с |
| Скорость людей | 5.0 | 4.0-6.5 |
| Скорость дьявола | 6.0 | 5.0-8.0 |
| Скорость агрессии | 9.0 | 7.0-12.0 |
| Длительность агрессии | 15с | 10-20с |
| Кд агрессии | 120с | 90-180с |
| HP Сердца | 100 | 50-150 |
| Урон ножа | 25 | 15-35 |
| Время чтения заклинания | 15с | 10-30с |
| Реген хилера | 25хп/мин | 15-40хп/мин |
| Время горения свечи | 60с | 30-120с |
| Время подготовки дьявола | 180с | 120-300с |

**Методология балансировки:**
1. Провести 10+ тестовых игр
2. Записать статистику побед (люди vs дьявол)
3. Целевой винрейт: ~50/50
4. Если дьявол побеждает слишком часто → увеличить HP людей, уменьшить урон, увеличить время подготовки
5. Если люди побеждают слишком часто → увеличить скорость/урон дьявола, уменьшить HP людей

### Шаг 2: Оптимизация

**Rendering:**
- Occlusion Culling настроен и работает
- LOD Groups на сложных моделях (если есть)
- Ограничить количество одновременных Point Lights (< 8-10)
- Использовать Light Probes для непрямого освещения
- Shadow Distance: минимальный (в темноте тени не так важны)
- Baked тени для статичных объектов

**Networking:**
- NetworkTransform: увеличить Threshold для уменьшения трафика
- Не отправлять RPC каждый кадр (batch updates)
- Компрессия данных где возможно
- Проверить Bandwidth через Network Profiler

**Physics:**
- Уменьшить количество Rigidbody (только необходимые)
- Использовать простые коллайдеры (Box, Sphere, Capsule) вместо Mesh Collider
- Layer-based collision matrix (игроки не сталкиваются друг с другом, но сталкиваются с дьяволом)

**Memory:**
- Объектный пул для часто создаваемых/удаляемых объектов (если есть)
- Profiler: проверить утечки памяти

### Шаг 3: Анти-чит (базовый уровень)

Так как используется host-authoritative модель:
- Вся логика урона проверяется на хосте
- Расстояние для взаимодействия проверяется на хосте
- Подбор предмета — проверка расстояния и доступности на хосте
- Здоровье — NetworkVariable, клиент не может изменить напрямую

**Слабое место:** хост может читерить (он же сервер). Для LAN-игры между друзьями это приемлемо.

### Шаг 4: Полировка визуала

- Туман в коридорах (Fog или Particle System)
- Пыль в воздухе (Particle System, видимая в свете)
- Анимации персонажей (idle, walk, run, attack, death)
- Анимации взаимодействия (подбор, бросание)
- Экран загрузки между сценами
- Плавные переходы (fade in/out)
- Курсор кастомный (в меню)

### Шаг 5: Полировка аудио

- Ambient звуки (зацикленные, разные для разных комнат)
- Рандомные пугающие звуки (скрипы, шорохи) с интервалами
- Heartbeat эффект при низком HP
- Тяжёлое дыхание при беге
- Звук шагов дьявола (слышен игрокам, более тяжёлый)
- Музыка: минималистичная, напряжённая (только в ключевые моменты)

### Шаг 6: QA тестирование

**Тест-план:**

| Тест-кейс | Описание | Ожидаемый результат |
|---|---|---|
| TC-001 | 5 игроков подключаются | Все видят друг друга в лобби |
| TC-002 | Старт игры | Роли назначены, дьявол видит фазу подготовки |
| TC-003 | Фаза подготовки | Дьявол расставляет все предметы за 3 мин |
| TC-004 | Спавн людей | 4 человека в разных комнатах |
| TC-005 | Подбор книги | Книга переносится, видна всем |
| TC-006 | Доставка 3 книг | Счётчик на алтаре = 3 |
| TC-007 | Чтение заклинания | Экзорцист читает, люди побеждают |
| TC-008 | Прерывание заклинания | Дьявол бьёт экзорциста, чтение прерывается |
| TC-009 | Открытие логова | Скаут открывает, все могут войти |
| TC-010 | Удар по сердцу без ножа | Урон не наносится |
| TC-011 | Удар по сердцу с ножом | Урон наносится, сердце уничтожается |
| TC-012 | Побег через выход | Игрок убегает, условие победы проверяется |
| TC-013 | Убийство всех людей | Дьявол побеждает |
| TC-014 | Скример-ловушка | Ослепление 5 сек, звук, тряска |
| TC-015 | Капкан на дьявола | Замедление дьявола |
| TC-016 | Телепортация экзорциста | Работает, прибор разрушаем |
| TC-017 | Лечение хилером | HP восстанавливается |
| TC-018 | Перетяжки скаута | Дьявол отображается на карте 3 сек |
| TC-019 | Режим агрессии | Скорость дьявола увеличивается на 15 сек |
| TC-020 | Отключение игрока | Игра продолжается, персонаж исчезает |
| TC-021 | Хост отключается | Игра завершается для всех (или host migration) |
| TC-022 | Длительная сессия (20 мин) | Нет утечек памяти, стабильный FPS |

### Шаг 7: Финальная сборка

- Build для Windows (x64)
- Тестирование билда (не только в редакторе)
- Проверка через Radmin VPN на разных машинах
- Проверка при разных разрешениях экрана
- Readme файл с инструкциями по установке и подключению через Radmin VPN

---

# 12. ПРИЛОЖЕНИЯ

## Приложение А: Схема карты (пример)

```
    ┌──────────┐         ┌──────────┐
    │          │         │          │
    │  Room 1  ├────┬────┤  Room 2  │
    │          │    │    │          │
    └────┬─────┘    │    └─────┬────┘
         │          │          │
    ┌────┴─────┐    │    ┌─────┴────┐
    │          │    │    │          │
    │  Room 3  ├────┤    │  Room 4  │
    │          │    │    │          │
    └────┬─────┘    │    └─────┬────┘
         │          │          │
         │    ┌─────┴────┐     │
         │    │          │     │
         ├────┤  Room 5  ├─────┤
         │    │ (Central)│     │
         │    └─────┬────┘     │
         │          │          │
    ┌────┴─────┐    │    ┌─────┴────┐
    │          │    │    │          │
    │  Room 6  ├────┤    │  Room 7  │
    │          │    │    │          │
    └────┬─────┘    │    └─────┬────┘
         │          │          │
         │    ┌─────┴────┐     │
         └────┤          ├─────┘
              │  Room 8  │
              │          │
              └──────────┘
```

## Приложение Б: Таблица персонажей

| # | Название | HP | Уникальная механика | Способности |
|---|---|---|---|---|
| 1 | Trapper | 110 | Контроль территории | Капканы (3 шт.), Стены (2 шт., 3-4 удара для разрушения) |
| 2 | Healer | 80 | Поддержка | Лечение (10-15 хп), Реген (25 хп/мин), Линейный камень |
| 3 | Exorcist | 90 | Мобильность + Изгнание | Телепорт-прибор (1 шт., разрушаем), Чтение заклинаний |
| 4 | Scout | 100 | Разведка + Доступ | Фонарь (1.5x), Свечи (3 шт., 60с), Перетяжки (2 шт., обнаруж. 3с), Открытие логова |
| D | Devil | 150 | Охота | Базовая атака (15, кд 1-1.5с), Усиленная (30, кд 30-45с, 3 раза), Скримеры (4 шт.), Агрессия (15с, кд 2мин) |

## Приложение В: Таблица предметов на карте

| Предмет | Количество | Размещает | Подбирается | Назначение |
|---|---|---|---|---|
| Книга | 3 | Дьявол (setup) | Любой человек | Донести до алтаря |
| Зачарованный нож | 1 | Рандом / Дьявол | Любой человек | Бить по сердцу дьявола |
| Алтарь | 1 | Дьявол выбирает комнату | Нет | Место для книг и заклинания |
| Сердце дьявола | 1 | Автоматически в логове | Нет (бьют ножом) | Цель для убийства дьявола |
| Логово дьявола | 1 | Дьявол (setup) | Нет (Скаут открывает) | Скрывает сердце |
| Выход | 1 | Дьявол (setup) | Нет (проходят через) | Побег |

## Приложение Г: Диаграмма сетевых взаимодействий (ключевые)

```
Подбор предмета:
Client(Owner) → [ServerRpc] RequestPickUp(itemId) → Host validates → 
    → Host: item.PickUp() → [ClientRpc] ConfirmPickUp → All clients update visual

Нанесение урона:
Devil(Owner) → [ServerRpc] RequestAttack() → Host validates (distance, cooldown) →
    → Host: player.TakeDamage() → NetworkVariable<Health> updates → All clients see HP change

Доставка книги:
Client drops book near altar → Host detects (OnTriggerEnter) → 
    → Host: altar.DeliverBook() → NetworkVariable<BooksDelivered>++ → 
    → [ClientRpc] UpdateAltarVisual

Конец игры:
Host detects win condition → Host: GameManager.EndGame(result) → 
    → NetworkVariable<GameState> = GameOver → [ClientRpc] ShowResultScreen
```

**Конец документа**