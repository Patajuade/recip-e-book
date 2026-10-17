# recip-e-book

A little project for me to remember my prodigious cooking experiments, and recipes i liked.

> **Ce README est la source de vérité du projet** : pitch, choix techniques, architecture, plan de route et avancement.
> Tout agent (humain ou Claude) qui reprend le projet commence par lire ce fichier, puis la section [Avancement](#7-avancement--journal).

---

## Sommaire

1. [Pitch](#1-pitch)
2. [Objectifs pédagogiques](#2-objectifs-pédagogiques)
3. [Stack technique & choix](#3-stack-technique--choix)
4. [Architecture](#4-architecture)
5. [Modèle de données & API](#5-modèle-de-données--api)
6. [Plan de route (phases)](#6-plan-de-route-phases)
7. [Avancement & journal](#7-avancement--journal)
8. [Instructions pour l'agent](#8-instructions-pour-lagent)
9. [Aide-mémoire React → Angular](#9-aide-mémoire-react--angular)
10. [Commandes utiles](#10-commandes-utiles)

---

## 1. Pitch

Une application web pour **écrire et consulter mes recettes**, comme un livre de cuisine personnel.

> *« J'ai créé une super soupe ce soir un peu au pif, j'aimerais l'écrire quelque part et pouvoir consulter mon livre de cuisine. »*

**MVP (ce qu'on doit avoir à la fin) :**
- Créer une recette (titre, description, ingrédients, étapes, temps, portions, notes, tags).
- Lister mes recettes, en chercher une par titre ou par tag.
- Consulter le détail d'une recette.
- Modifier / supprimer une recette.

**Hors MVP (bonus, voir phase 10)** : authentification multi-utilisateur, photos, favoris, mise à l'échelle des portions, import depuis une URL, PWA hors-ligne, déploiement.

---

## 2. Objectifs pédagogiques

La développeuse vient de **React** (apps modernes, Expo, monorepo Nx) et veut **apprendre Angular moderne** grâce à ce projet. Elle a des bases légères en **ASP.NET** et un peu d'expérience en **Entity Framework**.

Donc :
- **Frontend** : apprendre l'Angular d'aujourd'hui (standalone, signals, nouveau control flow, zoneless), pas l'Angular de 2018 (NgModules, RxJS partout, `*ngIf`).
- **Backend** : une API .NET propre et moderne (Minimal APIs, EF Core, OpenAPI), sans sur-architecture.
- Comprendre **le pourquoi** de chaque choix, avec des analogies React quand c'est utile.

---

## 3. Stack technique & choix

### Frontend
| Sujet | Choix | Pourquoi |
|---|---|---|
| Framework | **Angular** (dernière version stable, ≥ 21) via Angular CLI | Outil officiel, le plus pédagogique pour apprendre Angular |
| Composants | **Standalone** uniquement (pas de NgModule) | Standard actuel |
| Réactivité | **Signals** (`signal`, `computed`, `effect`, `input()`, `output()`, `model()`) | L'avenir d'Angular, proche du mental model des hooks |
| Détection de changements | **Zoneless** | Défaut des nouvelles apps Angular récentes |
| Templates | **Control flow** `@if` / `@for` / `@switch`, `@defer` | Remplace `*ngIf` / `*ngFor` |
| Injection | Fonction **`inject()`** | Plus de constructeurs à rallonge |
| Données serveur | **`httpResource()`** / `resource()` pour les lectures, `HttpClient` pour les mutations | Équivalent « léger » de React Query, basé sur les signals |
| Formulaires | **Reactive Forms typés** (`FormBuilder`, `FormArray`) — *Signal Forms* à évaluer si stables dans la version utilisée | Les FormArray sont parfaits pour les listes d'ingrédients/étapes |
| État global | Services + signals. **NgRx SignalStore** en option (phase 8) | Pas besoin de Redux pour un CRUD |
| UI / Style | **Angular Material** (+ CDK) avec SCSS | Apprendre l'écosystème Angular (theming, directives, CDK). Alternative : Tailwind v4 |
| Tests | **Vitest** (runner par défaut des versions récentes du CLI) + Angular Testing Library en option | Familier venant de React |
| Qualité | ESLint (`angular-eslint`) + Prettier | |
| Style guide | Conventions de nommage récentes (`recipe-list.ts` au lieu de `recipe-list.component.ts`) | Style guide officiel actuel |

### Backend
| Sujet | Choix | Pourquoi |
|---|---|---|
| Runtime | **.NET 10 (LTS)** | Version LTS courante |
| API | **ASP.NET Core Minimal APIs** organisées par feature (`MapGroup`, `TypedResults`) | Moderne, peu de cérémonie, proche d'Express/Fastify |
| Organisation | **Vertical slices** (dossier par feature) dans un seul projet API | Pas de Clean Architecture à 4 projets : inutile pour ce périmètre |
| ORM | **Entity Framework Core 10** | Déjà un peu connu |
| Base de données | **SQLite** en dev (fichier local, zéro installation) | Docker n'est pas installé ; migration vers PostgreSQL possible plus tard |
| Validation | Validation intégrée des Minimal APIs (`AddValidation()` + DataAnnotations). FluentValidation si besoin de règles complexes | Natif depuis .NET 10 |
| Erreurs | **ProblemDetails** (RFC 9457) via `AddProblemDetails()` | Format d'erreur standard, facile à consommer côté front |
| Documentation | **OpenAPI** natif (`Microsoft.AspNetCore.OpenApi`) + **Scalar** comme UI | Swashbuckle n'est plus le défaut |
| Tests | **xUnit** + `WebApplicationFactory` (tests d'intégration) | Teste l'API de bout en bout |

### Glue front ↔ back
- En dev, le **proxy du serveur Angular** redirige `/api/*` vers l'API .NET → pas de souci CORS.
- Phase 9 : **génération du client TypeScript** depuis le document OpenAPI (ex. `openapi-typescript` ou `ng-openapi-gen`) pour avoir des types partagés sans les écrire deux fois.

### Pourquoi pas Nx ?
Nx sait gérer Angular + .NET (`@nx-dotnet`), mais ajoute une couche d'abstraction au-dessus du CLI Angular. Pour **apprendre** Angular, mieux vaut utiliser le CLI directement. Le dépôt est quand même un monorepo simple (`frontend/` + `backend/`). Migrer vers Nx plus tard reste possible.

---

## 4. Architecture

### Arborescence cible
```
recip-e-book/
├── README.md                  ← ce fichier (plan + avancement)
├── CLAUDE.md                  ← pointeur pour les agents Claude
├── .editorconfig
├── .gitignore
├── .github/workflows/ci.yml   ← phase 9
├── docs/
│   └── decisions/             ← notes de décision (ADR courtes) si besoin
├── backend/
│   ├── RecipEBook.slnx
│   ├── src/
│   │   └── RecipEBook.Api/
│   │       ├── Program.cs
│   │       ├── appsettings*.json
│   │       ├── Domain/                    ← entités : Recipe, Ingredient, RecipeStep, Tag
│   │       ├── Infrastructure/
│   │       │   └── Persistence/           ← AppDbContext, configurations EF, Migrations/
│   │       ├── Features/
│   │       │   └── Recipes/               ← endpoints, DTOs (requests/responses), mapping
│   │       │       ├── RecipeEndpoints.cs
│   │       │       ├── CreateRecipe.cs
│   │       │       ├── GetRecipe.cs
│   │       │       ├── ListRecipes.cs
│   │       │       ├── UpdateRecipe.cs
│   │       │       └── DeleteRecipe.cs
│   │       └── Common/                    ← extensions, gestion d'erreurs, config
│   └── tests/
│       └── RecipEBook.Api.Tests/
└── frontend/
    ├── angular.json
    ├── proxy.conf.json
    └── src/
        ├── main.ts
        ├── styles.scss
        └── app/
            ├── app.ts / app.config.ts / app.routes.ts
            ├── core/                      ← layout (shell, header), interceptors, gestion d'erreurs
            ├── shared/                    ← composants UI génériques, pipes (ex. durée)
            └── features/
                └── recipes/
                    ├── recipes.routes.ts  ← routes lazy-loadées de la feature
                    ├── data-access/       ← modèles TS, recipe-api.ts (HttpClient), état
                    ├── pages/             ← composants « smart » routés : list, detail, edit
                    └── ui/                ← composants « dumb » : recipe-card, ingredient-list…
```

### Principes
- **Frontend** : séparation *pages* (smart, branchées sur les données, routées) / *ui* (dumb, uniquement `input()` / `output()`). Routes de feature en **lazy loading** (`loadChildren` / `loadComponent`).
- **Backend** : chaque fichier de `Features/Recipes` contient un cas d'usage complet (request DTO, handler, response DTO). Les entités ne sortent **jamais** de l'API : on renvoie des DTOs.
- **Pas d'abstraction prématurée** : pas de repository générique au-dessus d'EF Core (le `DbContext` est déjà un unit of work), pas de MediatR.

---

## 5. Modèle de données & API

### Entités (première version)
```
Recipe
├── Id              Guid
├── Title           string (requis, ≤ 200)
├── Description     string?          ← « l'histoire » de la recette
├── Servings        int?             ← nombre de portions
├── PrepTimeMinutes int?
├── CookTimeMinutes int?
├── Notes           string?          ← « j'ai mis un peu au pif… », variantes, idées
├── CreatedAt       DateTimeOffset
├── UpdatedAt       DateTimeOffset
├── Ingredients     List<Ingredient> (ordonnés)
│     ├── Id, Position
│     ├── Name      string (requis)
│     ├── Quantity  decimal?         ← optionnel : « une pincée », « au pif »
│     └── Unit      string?          ← g, ml, c. à s., pièce…
├── Steps           List<RecipeStep> (ordonnés)
│     ├── Id, Position
│     └── Instruction string (requis)
└── Tags            List<Tag> (many-to-many)  ← soupe, végé, hiver…
      └── Id, Name (unique)
```

### Endpoints REST (préfixe `/api`)
| Méthode | Route | Description |
|---|---|---|
| `GET` | `/api/recipes?search=&tag=` | Liste (résumé : id, titre, tags, temps total) |
| `GET` | `/api/recipes/{id}` | Détail complet |
| `POST` | `/api/recipes` | Création → `201 Created` + `Location` |
| `PUT` | `/api/recipes/{id}` | Remplacement complet (ingrédients/étapes inclus) |
| `DELETE` | `/api/recipes/{id}` | Suppression → `204 No Content` |
| `GET` | `/api/tags` | Liste des tags existants (autocomplétion) |
| `GET` | `/health` | Healthcheck |

Erreurs : `400` (validation, ProblemDetails avec `errors`), `404` (ProblemDetails).

---

## 6. Plan de route (phases)

Chaque phase = une branche `feat/phase-N-xxx`, des commits en [Conventional Commits](https://www.conventionalcommits.org/fr/), puis merge dans `main`.
Une phase est **terminée** quand ses critères de validation sont vérifiés.

### Phase 0 — Environnement
- [ ] Mettre Node à jour : **Node 24 LTS** (actuellement 18.12, incompatible avec Angular récent). Conseil : installer [fnm](https://github.com/Schniz/fnm) ou nvm-windows pour gérer les versions.
- [ ] Installer le **SDK .NET 10** (actuellement seul le SDK 6 est présent).
- [ ] Installer l'Angular CLI globalement (`npm i -g @angular/cli`) ou utiliser `npx`.
- [ ] Installer l'outil EF Core : `dotnet tool install --global dotnet-ef`.
- [ ] VS Code : extensions *Angular Language Service*, *C# Dev Kit*, *ESLint*, *Prettier*, *SQLite Viewer*.
- **Validation** : `node -v` ≥ 24, `dotnet --list-sdks` contient 10.x, `ng version` fonctionne.

### Phase 1 — Squelette du monorepo
- [ ] Corriger le `.gitignore` : ne **pas** ignorer `package-lock.json` (il doit être commité), ajouter les entrées .NET (`bin/`, `obj/`, `*.user`, `.vs/`), SQLite (`*.db`, `*.db-shm`, `*.db-wal`), et adapter les chemins Angular au dossier `frontend/`.
- [ ] Ajouter un `.editorconfig` (indentation, fins de ligne, règles C#).
- [ ] Créer les dossiers `backend/` et `frontend/`.
- **Validation** : arborescence en place, `git status` propre.

### Phase 2 — Fondations backend
- [ ] `dotnet new sln` (format `.slnx`) + `dotnet new web` → `RecipEBook.Api` + projet de tests xUnit.
- [ ] `Program.cs` minimal : OpenAPI + Scalar (en dev), ProblemDetails, healthcheck `/health`.
- [ ] Un `MapGroup("/api")` vide prêt à recevoir les features.
- [ ] Premier test d'intégration avec `WebApplicationFactory` sur `/health`.
- **Concepts** : hosting model, DI container (`builder.Services`), middleware pipeline, configuration (`appsettings`, `IOptions`).
- **Validation** : `dotnet run` → `/health` répond 200, `/scalar` affiche la doc ; `dotnet test` passe.

### Phase 3 — Domaine & persistance
- [ ] Entités du domaine (section 5).
- [ ] `AppDbContext` + configurations `IEntityTypeConfiguration<T>` (longueurs, index, relations, ordre).
- [ ] SQLite via chaîne de connexion dans `appsettings.Development.json`.
- [ ] Première migration (`dotnet ef migrations add Initial`) + application au démarrage en dev.
- [ ] Données de démo (2-3 recettes) via le seeding EF Core (`UseSeeding` / `UseAsyncSeeding`).
- **Concepts** : migrations, relations 1-n et n-n, change tracking, `AsNoTracking`, projections.
- **Validation** : la base est créée, les recettes de démo sont présentes.

### Phase 4 — API CRUD recettes
- [ ] Endpoints de la section 5 dans `Features/Recipes`, avec `TypedResults`.
- [ ] DTOs request/response + mapping manuel (pas d'AutoMapper).
- [ ] Validation des requêtes (titre requis, longueurs, quantités ≥ 0…).
- [ ] Recherche par titre et filtre par tag sur la liste.
- [ ] `GET /api/tags`.
- [ ] Tests d'intégration pour chaque endpoint (cas nominal + 400 + 404), sur une base SQLite in-memory.
- **Concepts** : model binding, `Results<Ok<T>, NotFound>`, projections LINQ → SQL, gestion des collections enfants en update.
- **Validation** : tout le CRUD est utilisable depuis Scalar ; `dotnet test` passe.

### Phase 5 — Fondations frontend
- [ ] `ng new frontend` (standalone, SCSS, zoneless, routing) dans le dossier `frontend/`.
- [ ] ESLint (`ng add angular-eslint`) + Prettier.
- [ ] Angular Material (`ng add @angular/material`) + thème.
- [ ] `proxy.conf.json` : `/api` → URL de l'API .NET.
- [ ] Layout global (`core/layout`) : header, zone de contenu, navigation.
- [ ] Routing : `/recipes`, `/recipes/new`, `/recipes/:id`, `/recipes/:id/edit`, redirection `/` → `/recipes`, page 404. Feature en lazy loading.
- [ ] `withComponentInputBinding()` pour recevoir `:id` directement en `input()`.
- **Concepts** : `app.config.ts` (providers), bootstrap, router, standalone components, DI.
- **Validation** : `ng serve` affiche le layout, la navigation entre pages vides fonctionne.

### Phase 6 — Consulter les recettes
- [ ] Modèles TypeScript (`data-access/recipe.model.ts`).
- [ ] Service `RecipeApi` (`providedIn: 'root'`, `inject(HttpClient)`).
- [ ] Page liste : `httpResource()`, états chargement / erreur / vide, cartes `recipe-card`.
- [ ] Recherche par titre (signal + debounce) et filtre par tag.
- [ ] Page détail : ingrédients, étapes, temps, notes, tags.
- [ ] Pipe `duration` (minutes → « 1 h 15 »).
- **Concepts** : signals, `computed`, `httpResource`, `input()` / `output()`, `@if` / `@for` (avec `track`), pipes, smart vs dumb components.
- **Validation** : la liste et le détail affichent les données de l'API.

### Phase 7 — Créer & modifier une recette
- [ ] Formulaire réactif typé : champs simples + `FormArray` pour ingrédients et étapes (ajouter, supprimer, réordonner via CDK drag & drop).
- [ ] Le même composant sert à la création et à l'édition.
- [ ] Saisie des tags avec autocomplétion (`GET /api/tags`).
- [ ] Affichage des erreurs de validation (client + ProblemDetails serveur).
- [ ] Suppression avec dialogue de confirmation.
- [ ] Garde `canDeactivate` fonctionnelle : avertir si le formulaire a des modifications non sauvegardées.
- [ ] Snackbar de confirmation après sauvegarde.
- **Concepts** : Reactive Forms, validators, `FormArray`, guards fonctionnels, CDK, dialogs Material.
- **Validation** : je peux écrire ma soupe « au pif » de A à Z, la modifier, la supprimer.

### Phase 8 — Finitions UX & état
- [ ] Interceptor HTTP fonctionnel pour la gestion d'erreurs globale.
- [ ] Responsive (mobile : consulter une recette en cuisinant !).
- [ ] Mode « cuisine » : affichage étape par étape, gros texte.
- [ ] Optionnel : NgRx SignalStore pour l'état de la feature recettes.
- [ ] Optionnel : `@defer` pour des blocs non critiques.
- **Validation** : l'app est agréable sur mobile et desktop.

### Phase 9 — Qualité & industrialisation
- [ ] Tests unitaires frontend (Vitest) sur les composants et services clés.
- [ ] Génération du client TS depuis l'OpenAPI.
- [ ] GitHub Actions : build + lint + tests front et back sur chaque PR.
- [ ] Scripts racine pour lancer front + back ensemble.
- **Validation** : CI verte.

### Phase 10 — Bonus (au choix, dans n'importe quel ordre)
- [ ] Authentification (ASP.NET Core Identity + cookies, ou fournisseur externe) → recettes par utilisateur.
- [ ] Photos de recettes (upload, stockage local puis blob).
- [ ] Favoris, note personnelle, « déjà cuisiné X fois ».
- [ ] Mise à l'échelle des quantités selon le nombre de portions.
- [ ] PWA (consultation hors-ligne).
- [ ] .NET Aspire pour orchestrer front + back + base.
- [ ] Passage à PostgreSQL (Docker) et déploiement (Azure, Fly.io…).
- [ ] Migration vers Nx si l'envie se présente.

---

## 7. Avancement & journal

**Phase en cours : 0 — Environnement**

| Phase | Statut |
|---|---|
| 0. Environnement | ⏳ À faire |
| 1. Squelette monorepo | ⏳ À faire |
| 2. Fondations backend | ⏳ À faire |
| 3. Domaine & persistance | ⏳ À faire |
| 4. API CRUD | ⏳ À faire |
| 5. Fondations frontend | ⏳ À faire |
| 6. Consulter | ⏳ À faire |
| 7. Créer & modifier | ⏳ À faire |
| 8. Finitions UX | ⏳ À faire |
| 9. Qualité & CI | ⏳ À faire |
| 10. Bonus | ⏳ À faire |

Légende : ⏳ À faire · 🚧 En cours · ✅ Terminé

### Décisions prises
<!-- Ajouter ici toute décision qui s'écarte du plan, avec la date et la raison. -->
- 2026-10-17 — Stack et plan initial définis (ce document).

### Journal des sessions
<!-- Une entrée par session, la plus récente en haut. Format :
#### AAAA-MM-JJ — Phase N
- Fait : …
- Appris : … (concepts Angular/.NET vus)
- Prochaine étape : …
- Points ouverts : …
-->
#### 2026-10-17 — Planification
- Fait : définition du pitch, de la stack, de l'architecture et du plan de route.
- Constat : Node 18.12 et SDK .NET 6 installés → mise à jour nécessaire (phase 0). Docker absent → SQLite en dev.
- Prochaine étape : phase 0, puis phase 1.

---

## 8. Instructions pour l'agent

Si tu es un agent Claude qui reprend ce projet :

1. **Lis ce README en entier**, en particulier [Avancement & journal](#7-avancement--journal), pour savoir où on en est.
2. **Mode pédagogique** : l'objectif est autant d'apprendre que de livrer.
   - Avance par **petites étapes**, explique chaque concept nouveau (Angular ou .NET) avant ou pendant sa mise en place.
   - Pour Angular, fais le **parallèle avec React** quand c'est pertinent (voir section 9).
   - Signale ce qui est « l'ancienne façon » d'Angular si la doc ou des exemples en ligne risquent de semer la confusion (NgModules, `*ngIf`, `@Input()` décorateur, constructeur pour la DI…).
   - Propose régulièrement à la développeuse d'écrire elle-même une partie (un composant, un endpoint) puis relis son code.
3. **Respecte la stack et l'architecture** décrites ici. Pour toute déviation, demande d'abord, puis consigne-la dans « Décisions prises ».
4. **Versions** : vérifie les versions réellement installées et la doc officielle avant d'utiliser une API récente (ex. Signal Forms, `httpResource`) ; certaines API évoluent vite.
5. **Git** : une branche par phase, Conventional Commits, ne commite/pousse que quand on te le demande.
6. **En fin de session** : mets à jour les cases à cocher de la phase, le tableau de statut, la « Phase en cours » et ajoute une entrée au journal.

---

## 9. Aide-mémoire React → Angular

| React | Angular moderne |
|---|---|
| Composant fonction | Classe `@Component({ ... })` standalone, template HTML séparé ou inline |
| `useState` | `signal()` → lecture `count()`, écriture `count.set()` / `count.update()` |
| `useMemo` | `computed()` |
| `useEffect` | `effect()` (à utiliser avec parcimonie), `afterNextRender()` pour le DOM |
| Props | `input()` / `input.required()` |
| Callback en prop (`onChange`) | `output()` + `(event)="..."` dans le template |
| Prop + callback contrôlé (`value`/`onChange`) | `model()` + `[(value)]` (two-way binding) |
| `children` | `<ng-content />` (content projection) |
| Context + Provider | Services + injection de dépendances (`inject()`) |
| Custom hooks | Fonctions utilisant `inject()` / services |
| React Router | Angular Router (`provideRouter`, `routerLink`, `loadComponent`) |
| React Query / SWR | `httpResource()` / `resource()` |
| react-hook-form | Reactive Forms (`FormGroup`, `FormArray`) / Signal Forms |
| `{cond && <X/>}` | `@if (cond) { <x /> }` |
| `list.map(i => <X key={i.id}/>)` | `@for (i of list(); track i.id) { <x /> }` |
| `React.lazy` + `Suspense` | `loadComponent` dans les routes, `@defer` dans les templates |
| `className={...}` | `[class.active]="isActive()"` ou `[class]="..."` |
| Redux / Zustand | Services + signals, ou NgRx SignalStore |

---

## 10. Commandes utiles

*(À compléter au fil des phases.)*

```bash
# Backend (depuis backend/)
dotnet run --project src/RecipEBook.Api
dotnet test
dotnet ef migrations add <Nom> --project src/RecipEBook.Api
dotnet ef database update --project src/RecipEBook.Api

# Frontend (depuis frontend/)
npm start                      # ng serve avec le proxy
npm test
ng generate component features/recipes/ui/recipe-card
```
