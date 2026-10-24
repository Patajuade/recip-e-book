# recip-e-book

A little project for me to remember my prodigious cooking experiments, and recipes I liked.

> **This README is the project's source of truth**: pitch, technical choices, architecture, roadmap and progress.
> Any agent (human or Claude) picking up the project starts by reading this file, then the [Progress](#7-progress--log) section.

---

## Table of contents

1. [Pitch](#1-pitch)
2. [Learning goals](#2-learning-goals)
3. [Tech stack & choices](#3-tech-stack--choices)
4. [Architecture](#4-architecture)
5. [Data model & API](#5-data-model--api)
6. [Roadmap (phases)](#6-roadmap-phases)
7. [Progress & log](#7-progress--log)
8. [Agent instructions](#8-agent-instructions)
9. [React → Angular cheat sheet](#9-react--angular-cheat-sheet)
10. [Useful commands](#10-useful-commands)

---

## 1. Pitch

A web app to **write down and browse my recipes**, like a personal cookbook.

> _"I made a great soup tonight, a bit improvised. I'd like to write it down somewhere and be able to browse my cookbook."_

**MVP (what we must have at the end):**

- Create a recipe (title, description, ingredients, steps, times, servings, notes, tags).
- List my recipes, search by title or tag.
- View a recipe's details.
- Edit / delete a recipe.
- **Multilingual English / French** UI, with on-the-fly language switching.

**Out of MVP (bonus, see phase 10)**: multi-user authentication, photos, favorites, serving scaling, import from URL, offline PWA, deployment.

**Long-term vision** (not planned yet, we stick to recipes for now):

- **Multiple users**: each user has their own recipes and tags. Draft data model in [#53](https://github.com/Patajuade/recip-e-book/issues/53).
- **A more suitable database** than SQLite once the needs are known (PostgreSQL, MongoDB…), compared then migrated with the existing data: [#91](https://github.com/Patajuade/recip-e-book/issues/91).
- **A personal companion app**: recip-e-book becomes one module of a bigger app grouping several areas of life (budget, home improvement ideas…), with shared accounts: [#92](https://github.com/Patajuade/recip-e-book/issues/92).

---

## 2. Learning goals

The developer comes from **React** (modern apps, Expo, Nx monorepos, Tailwind, TanStack Query) and wants to **learn modern Angular** through this project. She has light **ASP.NET** basics and some **Entity Framework** experience.

Therefore:

- **Frontend**: learn today's Angular (standalone, signals, new control flow, zoneless), not 2018 Angular (NgModules, RxJS everywhere, `*ngIf`).
- **Backend**: a clean, modern .NET API (Minimal APIs, EF Core, OpenAPI), without over-engineering.
- **Reuse familiar tools** (Nx, Tailwind) to focus the learning effort on Angular and .NET; **prefer Angular's official tools** when they cover the need (data fetching; i18n went back to Transloco, see decisions).
- Understand **the why** behind each choice, with React analogies where helpful.

---

## 3. Tech stack & choices

### Monorepo

| Topic      | Choice                                                | Why                                                                                                                                                   |
| ---------- | ----------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| Tool       | **Nx** (≥ 22)                                         | Already familiar. Handles Angular **and** .NET in the same project graph                                                                              |
| Angular    | Official **`@nx/angular`** plugin                     | Nx generators for Angular apps and libs                                                                                                               |
| .NET       | Official **`@nx/dotnet`** plugin (Nx 22+)             | Detects `.csproj` files, infers MSBuild tasks (build, test…), with caching and `affected`. Replaces the deprecated community plugin `@nx-dotnet/core` |
| Boundaries | Nx tags + ESLint rule `@nx/enforce-module-boundaries` | Prevents e.g. a `ui` lib from importing a `data-access` lib                                                                                           |

### Frontend

| Topic                  | Choice                                                                                                                                                                                                                                                                                      | Why                                                                                                                                                                                                                                                   |
| ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Framework              | **Angular** (latest stable, ≥ 21)                                                                                                                                                                                                                                                           |                                                                                                                                                                                                                                                       |
| Components             | **Standalone** only (no NgModule)                                                                                                                                                                                                                                                           | Current standard                                                                                                                                                                                                                                      |
| Reactivity             | **Signals** (`signal`, `computed`, `effect`, `input()`, `output()`, `model()`)                                                                                                                                                                                                              | Angular's future, close to the hooks mental model                                                                                                                                                                                                     |
| Change detection       | **Zoneless**                                                                                                                                                                                                                                                                                | Default for new recent Angular apps                                                                                                                                                                                                                   |
| Templates              | **Control flow** `@if` / `@for` / `@switch`, `@defer`                                                                                                                                                                                                                                       | Replaces `*ngIf` / `*ngFor`                                                                                                                                                                                                                           |
| Injection              | **`inject()`** function                                                                                                                                                                                                                                                                     | No more bloated constructors                                                                                                                                                                                                                          |
| Server data            | **Angular resources**: `httpResource()` for reads (signals: `value()`, `isLoading()`, `error()`, `reload()`), `HttpClient` for writes                                                                                                                                                       | Official and stable since Angular 22 ([httpResource](https://angular.dev/guide/http/http-resource)). No shared cache: after a write, the affected resources are reloaded. Developer's choice over TanStack Query (Angular adapter still experimental) |
| Forms                  | **Signal Forms** (`@angular/forms/signals`, stable since Angular 22): model signal + `form()`, schema rules (`required`, `maxLength`, `applyEach`…), `[formField]` bindings, `[formRoot]` + `submit()`                                                                                      | Fits an app built on signals; the docs recommend it for new apps. Replaced the planned Reactive Forms (#37)                                                                                                                                           |
| Global state           | Services + signals. Server state lives in resources                                                                                                                                                                                                                                         | No store needed yet; revisit when the app grows (#92)                                                                                                                                                                                                 |
| Styling                | **Tailwind CSS v4**                                                                                                                                                                                                                                                                         | Already familiar                                                                                                                                                                                                                                      |
| Headless components    | **Angular CDK** (dialog, overlay, drag & drop, a11y). Angular Aria to evaluate if available                                                                                                                                                                                                 | Behavior and accessibility provided, 100% Tailwind styling                                                                                                                                                                                            |
| i18n                   | **Transloco v8** (`@jsverse/transloco`, [v8 docs](https://jsverse.gitbook.io/transloco/docs/docs-v8/)): `en.json` / `fr.json` in `apps/web/public/i18n/`, `*transloco="let t"` in templates, runtime language switching, language remembered (`@jsverse/transloco-persist-lang`)            | One app for both languages, switching without reload. Replaced Angular's build-time i18n (#105)                                                                                                                                                       |
| Date/number formatting | **Transloco Locale** (`@jsverse/transloco-locale`, [docs](https://jsverse.gitbook.io/transloco/docs/docs-v8/plugins-and-extensions/locale-l10n)): `translocoDate` / `translocoDecimal` pipes following the active language (`en` → `en-US`, `fr` → `fr-FR`), based on the native `Intl` API | "18 octobre 2026" vs "October 18, 2026", "1,5" vs "1.5"                                                                                                                                                                                               |
| Tests                  | **Vitest** + optionally Angular Testing Library                                                                                                                                                                                                                                             | Familiar coming from React                                                                                                                                                                                                                            |
| Quality                | ESLint (Nx config + `angular-eslint`) + Prettier                                                                                                                                                                                                                                            |                                                                                                                                                                                                                                                       |
| Style guide            | Recent naming conventions (`recipe-list.ts` instead of `recipe-list.component.ts`)                                                                                                                                                                                                          | Current official style guide                                                                                                                                                                                                                          |

### Backend

| Topic        | Choice                                                                                                                                                                                       | Why                                                            |
| ------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------- |
| Runtime      | **.NET 10 (LTS)**                                                                                                                                                                            | Current LTS                                                    |
| API          | **ASP.NET Core Minimal APIs** organized by feature (`MapGroup`, `TypedResults`)                                                                                                              | Modern, low ceremony, close to Express/Fastify                 |
| Organization | **Vertical slices** (one folder per feature) in a single API project                                                                                                                         | No 4-project Clean Architecture: overkill for this scope       |
| ORM          | **Entity Framework Core 10**                                                                                                                                                                 | Already somewhat familiar                                      |
| Database     | **SQLite** in dev (local file, zero install)                                                                                                                                                 | Docker isn't installed. Migration to PostgreSQL possible later |
| Validation   | Built-in Minimal API validation (`AddValidation()` + DataAnnotations). FluentValidation if complex rules are needed                                                                          | Native since .NET 10                                           |
| Errors       | **ProblemDetails** (RFC 9457) via `AddProblemDetails()`                                                                                                                                      | Standard error format, easy to consume on the frontend         |
| i18n         | Recipe **content** isn't translated (it's what the user writes). The frontend sends `Accept-Language`. The backend localizes its error messages (`RequestLocalization` + `.resx`) in phase 8 | UI strings are translated on the frontend                      |
| Docs         | Native **OpenAPI** (`Microsoft.AspNetCore.OpenApi`) + **Scalar** as UI                                                                                                                       | Swashbuckle is no longer the default                           |
| Tests        | **xUnit** + `WebApplicationFactory` (integration tests)                                                                                                                                      | Tests the API end to end                                       |

### Frontend ↔ backend glue

- In dev, the **Angular dev server proxy** forwards `/api/**` to the .NET API (`apps/web/src/proxy.conf.json`, [docs](https://angular.dev/tools/cli/serve#proxying-to-a-backend-server)) → the browser only talks to the dev server, no CORS issues. The API must run with the `https` launch profile.
- An HTTP interceptor adds the `Accept-Language` header based on the active Transloco language.
- Phase 9: **TypeScript type generation** from the OpenAPI document (e.g. `openapi-typescript`), exposed as an Nx task, so types aren't written twice.

---

## 4. Architecture

### Target tree

```
recip-e-book/
├── README.md                       ← this file (plan + progress)
├── CLAUDE.md                       ← pointer for Claude agents
├── nx.json                         ← plugins (@nx/angular, @nx/dotnet), cache, targetDefaults
├── package.json
├── tsconfig.base.json              ← path aliases to libs (@recip-e-book/...)
├── RecipEBook.slnx                 ← .NET solution (for the IDE and MSBuild)
├── .editorconfig
├── .github/workflows/ci.yml        ← phase 9 (nx affected)
├── docs/decisions/                 ← short decision records (ADRs) if needed
├── apps/
│   ├── web/                        ← Angular app (shell): app.config.ts, app.routes.ts, layout
│   │   └── src/
│   │       ├── proxy.conf.json     ← dev server proxy: /api/** → https://localhost:7288
│   │       └── app/transloco-loader.ts   ← loads public/i18n/<lang>.json
│   │   └── public/i18n/            ← en.json, fr.json
│   ├── api/                        ← RecipEBook.Api.csproj
│   │   ├── Program.cs
│   │   ├── Domain/                 ← entities: Recipe, Ingredient, RecipeStep, Tag
│   │   ├── Infrastructure/Persistence/   ← AppDbContext, EF configurations, Migrations/
│   │   ├── Features/Recipes/       ← one file per use case (Create, Get, List, Update, Delete)
│   │   ├── Features/Tags/
│   │   ├── Common/                 ← extensions, errors, localization
│   │   └── Resources/              ← .resx error messages (phase 8)
│   └── api-tests/                  ← RecipEBook.Api.Tests.csproj (xUnit)
└── libs/
    └── web/
        ├── recipes/
        │   ├── feature/            ← routed pages (list, detail, edit) + recipes.routes.ts
        │   ├── data-access/        ← read resources (httpResource), recipe-api.ts (HttpClient writes)
        │   ├── models/             ← TS models of the API DTOs (type:util, usable by every layer)
        │   └── ui/                 ← presentational components: recipe-card, ingredient-list…
        └── shared/
            ├── ui/                 ← generic Tailwind components: button, input, dialog, tag-chip…
            ├── i18n/               ← languages, language switcher, Accept-Language interceptor, title strategy, <html lang>
            └── util/               ← pipes (duration), helpers
```

### Nx tags & dependency rules

| Lib tag            | Can import                             |
| ------------------ | -------------------------------------- |
| `type:app`         | anything                               |
| `type:feature`     | `feature`, `data-access`, `ui`, `util` |
| `type:data-access` | `data-access`, `util`                  |
| `type:ui`          | `ui`, `util`                           |
| `type:util`        | `util`                                 |

Plus `scope:recipes` / `scope:shared`: a `scope:recipes` lib can import `scope:shared`, not the other way around.

Enforced by `@nx/enforce-module-boundaries` in the root `eslint.config.mjs` ([docs](https://nx.dev/docs/features/enforce-module-boundaries)). A project must satisfy every constraint matching one of its tags (its `type:` rule **and** its `scope:` rule).

### Principles

- **Frontend**: split between _feature_ (pages, data-connected, routed) and _ui_ (presentational, only `input()` / `output()`). Feature routes are **lazy loaded** (`loadChildren`).
- **Data**: each `data-access` lib exposes typed models, **read resources** built with `httpResource()` (reactive to signal parameters such as filters) and **write methods** built with `HttpClient` (create, update, delete). After a write, the caller reloads the affected resources (`reload()`). Components never make direct HTTP calls: keeping all fetching inside `data-access` lets us change the strategy later (a store such as NgRx SignalStore, or a cache library) without touching features and UI, as the app grows (#92).
- **i18n**: no hardcoded UI text: templates use one `*transloco="let t"` per template and `t('recipes.list.heading')`; code uses `translateSignal()`; route `title`s are keys translated by `TranslocoTitleStrategy`. Keys are nested by feature (`recipes.list.heading`).
- **Backend**: each file in `Features/Recipes` holds a complete use case (request DTO, handler, response DTO). Entities **never** leave the API: we return DTOs.
- **CQRS (basic level: separate models, single data store)**, as described in the [CQRS pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs) ("Separate models in a single data store"):
  - each use case file is **either a query or a command**, named after it (`ListRecipes`, `GetRecipe` = queries; `CreateRecipe`, `UpdateRecipe`, `DeleteRecipe` = commands);
  - **queries** "never alter data": they return read DTOs built by projection (`Select`), no domain logic, no tracked entity;
  - **commands** express a business intent, take their own request model (never a read DTO), validate it, change entities and save; they return only what the client needs (e.g. `201 Created` + `Location`, `204 No Content`);
  - read and write models live in separate types, even when they look alike.
- **No premature abstraction**: no generic repository on top of EF Core (the `DbContext` already is a unit of work), no MediatR (handlers are plain static methods mapped by Minimal APIs).
- **Dates are UTC everywhere** (stored and returned with a `Z`); conversion to local time only happens in the UI.

---

## 5. Data model & API

### Entities

Code in `apps/api/Domain/` (namespace `RecipEBook.Api.Domain`).

```
Recipe (entity)
├── Id              Guid             ← generated by EF Core on insert (convention)
├── Title           string           (required)
├── Description     string?          ← the recipe's "story"
├── Servings        int?
├── PrepTimeMinutes int?
├── CookTimeMinutes int?
├── Notes           string?          ← "I improvised a bit…", variants, ideas
├── CreatedAt       DateTime (UTC)
├── UpdatedAt       DateTime (UTC)
├── Ingredients     List<Ingredient> ← complex collection, stored as JSON, order kept
│     ├── Name      string           (required)
│     ├── Quantity  decimal?         ← optional: "a pinch", "to taste"
│     └── Unit      string?          ← free text (g, ml, tbsp, piece…)
├── Steps           List<RecipeStep> ← complex collection, stored as JSON, order kept
│     └── Instruction string         (required)
└── Tags            List<Tag>        ← many-to-many (join table RecipeTag by convention)

Tag (entity, fixed catalog — TagCatalog, inserted by migrations)
├── Key             string           (primary key, stable English key: "gluten-free")
├── Category        TagCategory      (Course, Diet, MainIngredient, CookingMethod, Season, Occasion, Cuisine, Practical; stored as text)
└── Recipes         List<Recipe>
```

Modeling choices (2026-10-18, from the EF Core docs):

- **`Ingredient` and `RecipeStep` are complex types** (immutable `record`s): they have no identity of their own and only exist inside a recipe, which is what [complex types](https://learn.microsoft.com/en-us/ef/core/modeling/complex-types) are for. In EF Core 10, complex collections are mapped to a JSON column (`ComplexCollection(...).ToJson()`, configured in #12). No join, no `Position` column, and replacing all ingredients on `PUT` is a plain assignment.
- **`DateTime` in UTC** rather than `DateTimeOffset`: the [SQLite provider](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations) can't compare or order `DateTimeOffset` in the database and recommends UTC `DateTime` values.
- **`decimal` kept for quantities**: equality is supported on SQLite, and EF Core 10 added `MIN`/`MAX`/`ORDER BY` on `decimal` for SQLite ([what's new](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/whatsnew)); quantities are never sorted anyway.
- **`Guid` keys**, generated by EF Core when the value is `Guid.Empty` ([generated values](https://learn.microsoft.com/en-us/ef/core/modeling/generated-properties)), so `Id` is not `required`.

> Units are **free text** entered by the user, so they're not translated (a list of predefined, translated units may come as a bonus). Tags come from a **fixed catalog** (#116) and are translated on the frontend (`tags.<key>`, `tagCategories.<category>`).

### REST endpoints (`/api` prefix)

| Method   | Route                       | Description                                    |
| -------- | --------------------------- | ---------------------------------------------- |
| `GET`    | `/api/recipes?search=&tag=` | List (summary: id, title, tags, total time)    |
| `GET`    | `/api/recipes/{id}`         | Full details                                   |
| `POST`   | `/api/recipes`              | Create → `201 Created` + `Location`            |
| `PUT`    | `/api/recipes/{id}`         | Full replace (ingredients/steps included)      |
| `DELETE` | `/api/recipes/{id}`         | Delete → `204 No Content`                      |
| `GET`    | `/api/tags`                 | Tag catalog (key, category) for the tag picker |
| `GET`    | `/health`                   | Healthcheck                                    |

Errors: `400` (validation, ProblemDetails with `errors`), `404` (ProblemDetails).

---

## 6. Roadmap (phases)

Tasks are tracked on the **[GitHub project board](https://github.com/users/Patajuade/projects/2)**: one milestone per phase, one issue per task (labels `area:frontend`, `area:backend`, `area:infra`, `area:i18n`, `type:test`, `bonus`). This section only keeps the big picture: what each phase teaches and how we know it's done.

Each task (issue) = one `<type>/<issue>-<short-slug>` branch (e.g. `feat/15-list-recipes`), [Conventional Commits](https://www.conventionalcommits.org/) **in English** referencing their issue (`Closes #15`), then a pull request into `main`. **PRs are reviewed and merged manually by the developer.**
A phase is **done** when all its milestone issues are closed and its validation criteria are met.

### Phase 0 — Environment

**Tasks:** [Phase 0 milestone](https://github.com/Patajuade/recip-e-book/milestone/1)

- **Validation**: `node -v` ≥ 24, `dotnet --list-sdks` lists 10.x.

### Phase 1 — Nx workspace

**Tasks:** [Phase 1 milestone](https://github.com/Patajuade/recip-e-book/milestone/2)

- **Validation**: `nx graph` opens, `git status` clean after commit.

### Phase 2 — Backend foundations

**Tasks:** [Phase 2 milestone](https://github.com/Patajuade/recip-e-book/milestone/3)

- **Concepts**: hosting model, DI container (`builder.Services`), middleware pipeline, configuration (`appsettings`, `IOptions`), MSBuild ↔ Nx integration.
- **Validation**: the API starts, `/health` returns 200, `/scalar` shows the docs, tests pass via Nx.

### Phase 3 — Domain & persistence

**Tasks:** [Phase 3 milestone](https://github.com/Patajuade/recip-e-book/milestone/4)

- **Concepts**: migrations, 1-n and n-n relationships, change tracking, `AsNoTracking`, projections.
- **Validation**: the database is created, demo recipes are present.

### Phase 4 — Recipe CRUD API

**Tasks:** [Phase 4 milestone](https://github.com/Patajuade/recip-e-book/milestone/5)

- **Concepts**: model binding, `Results<Ok<T>, NotFound>`, LINQ → SQL projections, handling child collections on update.
- **Validation**: the full CRUD is usable from Scalar, tests pass.

### Phase 5 — Frontend foundations

**Tasks:** [Phase 5 milestone](https://github.com/Patajuade/recip-e-book/milestone/6)

- **Concepts**: `app.config.ts` (providers), bootstrap, router, standalone components, DI, functional interceptors, Nx Angular libs.
- **Validation**: `nx serve web` shows the Tailwind-styled layout, navigation works, switching EN ↔ FR updates the UI without a page reload.

### Phase 6 — Browse recipes

**Tasks:** [Phase 6 milestone](https://github.com/Patajuade/recip-e-book/milestone/7)

- **Concepts**: signals, `computed`, `input()` / `output()`, `@if` / `@for` (with `track`), pipes, smart vs dumb components, `httpResource()` (signal-based reads), `HttpClient` and Observables (the bare minimum).
- **Validation**: list and detail show API data, in EN and FR.

### Phase 7 — Create & edit a recipe

**Tasks:** [Phase 7 milestone](https://github.com/Patajuade/recip-e-book/milestone/8)

- **Concepts**: Signal Forms (model, schema, `applyEach`, `linkedSignal` pre-filling), validators, functional guards, CDK (dialog, overlay, drag-drop), writes with `HttpClient` and reloading the affected resources.
- **Validation**: I can write my improvised soup from A to Z, edit it, delete it. The list updates without reloading.

### Phase 8 — UX polish, backend i18n & state

**Tasks:** [Phase 8 milestone](https://github.com/Patajuade/recip-e-book/milestone/9)

- **Validation**: the app is pleasant on mobile and desktop, fully translated, server errors included.

### Phase 9 — Quality & tooling

**Tasks:** [Phase 9 milestone](https://github.com/Patajuade/recip-e-book/milestone/10)

- **Validation**: green CI.

### Phase 10 — Bonus (pick any, in any order)

**Tasks:** [Phase 10 milestone](https://github.com/Patajuade/recip-e-book/milestone/11)

### Phase 11 — Shopping lists & PWA

**Tasks:** [Phase 11 milestone](https://github.com/Patajuade/recip-e-book/milestone/12)

Added on 2026-10-24 at the developer's request.

- **PWA** (#58): installable, the app shell and the recipes already viewed stay readable offline; creating and editing stay online.
- **Shopping lists** (#139–#142): several named lists stored by the API (the same lists on every device), filled from a recipe's ingredients (« Ajouter à la liste ») or by hand, items checked off in the store.
- **Validation**: on a phone, install the app, add a recipe to a list, then check the items off in a store.

### Phase 12 — Meal planner

**Tasks:** [Phase 12 milestone](https://github.com/Patajuade/recip-e-book/milestone/13)

Added on 2026-10-25 at the developer's request, with phases 13 and 14.

- Meal moments defined by the user (a default set to rename, reorder or delete), a week of plan entries (a recipe with servings, or a free item), entries checked as eaten with the eaten servings (#146–#148).
- The week's shopping list from the planned recipes (#149).
- **Validation**: plan a week on the phone, check the meals eaten, shop for the week.

### Phase 13 — Foods & structured ingredients

**Tasks:** [Phase 13 milestone](https://github.com/Patajuade/recip-e-book/milestone/14)

- Nutrition needs to know what an ingredient contains: today ingredients are free text. Foods come from the ANSES **Ciqual** table (generic foods, #150) and **Open Food Facts** (packaged products, barcode, ODbL, #151).
- Ingredient lines linked to a food, with quantities convertible to grams (#152, with #57), then nutrition per recipe (#153).
- **Validation**: a recipe shows its nutrition per serving, and what isn't counted.

### Phase 14 — Nutrition & dashboard

**Tasks:** [Phase 14 milestone](https://github.com/Patajuade/recip-e-book/milestone/15)

- Intake from the entries checked as eaten (#154), compared with reference values (#155). The user chooses a generic profile or a personal one; the personal profile is health data and comes **only after authentication** (#53, #156).
- A dashboard with accessible charts (#157). Informative only, not medical advice.
- **Validation**: after a week of checked meals, the dashboard shows the intake and the gaps.

---

## 7. Progress & log

**Current phase: 8 — UX polish, backend i18n & state** (phases 0–7 done)

- Task-level progress: [project board](https://github.com/users/Patajuade/projects/2) (Todo / In Progress / Done).
- Phase-level progress: [milestones](https://github.com/Patajuade/recip-e-book/milestones) (open/closed issue ratio).

### Decisions

<!-- Add any decision that departs from the plan here, with the date and the reason. -->

- 2026-10-17 — Initial stack and plan defined (this document).
- 2026-10-17 — **Nx** chosen (official `@nx/angular` + `@nx/dotnet` plugins) over the plain Angular CLI: already familiar tool.
- 2026-10-17 — **Tailwind v4 + Angular CDK** over Angular Material: Tailwind already familiar, the CDK provides behavior without imposing styles.
- 2026-10-17 — **EN/FR i18n with Transloco** (runtime) over `@angular/localize` (one build per language). _Replaced on 2026-10-18, see below._
- 2026-10-17 — **TanStack Query (Angular adapter)** over `httpResource()` alone: already familiar, handles caching and invalidation. Adapter still _experimental_ → pinned version. _Replaced on 2026-10-18, see below._
- 2026-10-17 — **Documentation and commits in English**; conversation with the agent in French.
- 2026-10-17 — **Kanban on GitHub Projects** (over Notion): issues link to commits/PRs and everything stays in the repo.
- 2026-10-17 — **One branch + PR per issue**, merged manually by the developer (the agent never merges into `main`).
- 2026-10-17 — **Node managed with fnm** (`.nvmrc` = 24). The system Node 18 stays fnm's default so other projects on the machine are unaffected.
- 2026-10-17 — **dotnet-ef as a local tool** instead of a global install: version pinned in the repo, restored with `dotnet tool restore`. Manifest in `.config/dotnet-tools.json`, the [documented location](https://learn.microsoft.com/en-us/dotnet/core/tools/local-tools-how-to-use) (moved there in #72).
- 2026-10-17 — **Nx AI agent config kept** (`.claude/settings.json` enabling the official Nx Claude Code plugin, Nx block in `CLAUDE.md`), as generated by `nx init`.
- 2026-10-18 — **`@nx/dotnet` added in #8, together with the first .NET projects**, following the order of the plugin README (`nx add` → configure `nx.json` → `dotnet new`). v23.3.0 crashes the project graph ("No project files provided") when it finds `global.json` without any `.csproj`.
- 2026-10-18 — **Official docs only**: after an audit of phases 0–1, every install and config step must follow the official docs of the tool, cited in the PR; when in doubt, ask the developer (see section 8). Audit follow-ups:
  - `nx init` redone interactively by the developer, on a `package.json` created with `npm init -y` and cleaned with `npm pkg` (#71).
  - nx install script **denied** (`npm install-scripts deny nx`): it only checks the platform and updates the Nx Cloud client, which isn't used (#71).
  - `.editorconfig` and `.gitignore` rebuilt from the official templates (Nx Angular template + `dotnet new editorconfig` / `dotnet new gitignore`); VS Code settings limited to what the Prettier extension documents (#73).
  - The plan's API names (sections 3–6) and the planning issues were written from memory: verify them against the official docs when starting each task. Known gap: the Nx 23 Angular application generator has no Tailwind option, so Tailwind is set up from Tailwind's own Angular guide (#23).
- 2026-10-18 — **Teaching mode in practice**: the developer runs the commands and writes the code herself when she wants to; the agent prepares the steps from the official docs, then reviews and verifies before committing. She can also ask the agent to do a task (e.g. #10).
- 2026-10-18 — **Backend project layout**: `RecipEBook.slnx` at the root, `apps/api` (`RecipEBook.Api`, `dotnet new webapi`) and `apps/api-tests` (`RecipEBook.Api.IntegrationTests`, xUnit 2.9.3), following the Nx example (`apps/`) and the Microsoft advice to keep integration tests in their own project.
- 2026-10-18 — **HTTPS kept in development** (template default); the dev certificate is trusted locally with `dotnet dev-certs https --trust`.
- 2026-10-18 — **Scalar** as the OpenAPI UI (documented by Microsoft next to Swagger UI and ReDoc); the OpenAPI document itself comes from the built-in `Microsoft.AspNetCore.OpenApi` (Swashbuckle left the templates in .NET 9, see dotnet/aspnetcore#54599).
- 2026-10-18 — **Health check on `/health`** (the docs' example uses `/healthz`; `/health` matches section 5). The `/api` route group is created with the first real endpoint (#15) instead of an empty group in #9.
- 2026-10-18 — **Postman local files ignored** (`/.postman/`, `/postman/`): they're created by the Postman VS Code extension and not shared (#82).
- 2026-10-18 — **CQRS at the basic level, without MediatR** (developer's request): queries and commands are separate use cases with separate read/write models, in a single SQLite database (see section 4). MediatR isn't used: Minimal API handlers already give one file per use case, and MediatR moved to a dual open-source/commercial license in July 2025.
- 2026-10-18 — **Search on a normalized `SearchTitle`** (#15): SQLite only folds the case of ASCII letters, so titles are also stored lowercase and accent-free for searching.
- 2026-10-18 — **UTC `DateTime` converter** (#16): SQLite doesn't keep `DateTime.Kind`; a value converter marks dates read from the database as UTC, so the API returns them with a `Z` ([value conversions](https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions#specify-the-datetimekind-when-reading-dates)).
- 2026-10-18 — **Frontend libs** (#24): the six section-4 libs are generated with `@nx/angular:library`. Project names include the folder path (`web-recipes-ui`, `web-shared-ui`…) because two libs are called `ui`; import paths follow (`@recip-e-book/web-recipes-ui`). Default selector prefix `lib`. `shared/i18n` is tagged `type:util` so every layer can use it. No unit test runner in libs for now (`--unitTestRunner=none`): the frontend test setup is chosen in phase 9. Lint targets converted to the inferred plugin (`convert-to-inferred`), as for the app. The feature / ui / data-access / util split is an **Nx convention** ([Tag in Multiple Dimensions](https://nx.dev/docs/guides/enforce-module-boundaries/tag-multiple-dimensions)), not an Angular one: the [Angular style guide](https://angular.dev/style-guide) only asks to organize code by feature area. Kept on purpose (developer's choice) so that ESLint enforces the layers instead of relying on folder conventions.
- 2026-10-18 — _Replaced on 2026-10-19 (#105), see below._ **Angular i18n (`@angular/localize`) instead of Transloco** (#25, developer's choice: prefer the official tool). Source locale `en-US`, translated locale `fr`, XLIFF 1.2 (default format). Translated at build time: switching language reloads the page on the other locale's subdirectory; the dev server serves one locale at a time (`nx serve web` = English, `--configuration=fr` = French). Set up by running Angular's own `ng-add` schematic through Nx (`nx g @angular/localize:ng-add --project=web`), then the `i18n` / `localize` options from [Merge translations](https://angular.dev/guide/i18n/merge) (`localize: true` only in the `production` configuration) and an `extract-i18n` target using `@angular/build:extract-i18n`.
- 2026-10-18 — **Angular resources instead of TanStack Query** (#26, developer's choice: prefer the official tool). `httpResource()`, `resource()` and `rxResource()` are stable since Angular 22, while `@tanstack/angular-query-experimental` is still experimental ("breaking changes will happen in minor AND patch releases"). Reads use `httpResource()`; writes use `HttpClient`, as the docs advise ("Avoid using httpResource for mutations like POST or PUT"). `HttpClient` needs no setup: it "is available for injection by default in Angular v21 and later" and uses `fetch` by default ([setup](https://angular.dev/guide/http/setup)); `provideHttpClient(withInterceptors(...))` is only added with the first interceptor (#27). Trade-off: no shared cache, writes reload the affected resources. **Revisit when the app grows** (multi-user, companion app #92): the fetching strategy is contained in `data-access` libs.
- 2026-10-18 — **Dev proxy targets the HTTPS endpoint** (#28): `/api/**` → `https://localhost:7288` with `"secure": false` (as in the [Angular example](https://angular.dev/tools/cli/serve#proxying-to-a-backend-server); the dev certificate is trusted by Windows, not by Node). Targeting `http://localhost:5047` would break with the `https` profile: `UseHttpsRedirection` answers `307` to the HTTPS port and the browser would leave the dev server. Frontend and backend run in two terminals: the inferred `RecipEBook.Api:watch` target uses the default (`http`) profile, and the planned `nx run-many -t serve` doesn't apply (the API has no `serve` target).
- 2026-10-19 — **Back to Transloco (v8) instead of Angular i18n** (#105, developer's choice). Build-time i18n proved too constraining: one build per locale, no SPA fallback with a simple static server (`serve-static` needed `spa: false`), no switching in `nx serve`, hand-edited XLIFF. **v8.4** is the current stable version; the docs home already describes v9 (alpha), so we follow the [v8 docs](https://jsverse.gitbook.io/transloco/docs/docs-v8/). Setup with the Nx path of the docs (`nx g @jsverse/transloco:ng-add --project=web --langs=en,fr --path=../public/i18n/`, the path is relative to `apps/web/src`), then three fixes to the generated code: a duplicate `provideHttpClient()` that would have dropped our interceptor, the loader URL (`/i18n/<lang>.json`) and `transloco.config.ts`'s path. Keys kept from the XLIFF IDs (the official `ng-migrate` schematic would generate kebab-case keys from the texts). Initial language: remembered one, else browser language, else `en` (`@jsverse/transloco-persist-lang` with `getLangFn`). Route titles are keys translated by a custom `TitleStrategy`; `<html lang>` follows the active language.
- 2026-10-19 — **Recipe models in their own lib** (#33): `libs/web/recipes/models` (`type:util`, `scope:recipes`). The recipe card (`type:ui`) needs `RecipeSummary`, and module boundaries forbid `ui` → `data-access` even for type-only imports (checked with a probe import). A `util` lib can be imported by every layer.
- 2026-10-20 — **Transloco Locale for dates and numbers** (#35, developer's choice) over Angular's `date` / `number` pipes: they follow the active Transloco language without passing a locale to every call, and need no `registerLocaleData` (native `Intl`). `LOCALE_ID` stays `en-US` and isn't used.
- 2026-10-20 — **Durations with the native `Intl.DurationFormat`** (#36, developer's choice): `DurationPipe` in `libs/web/shared/util` (`{{ minutes | duration: lang() }}`, style `short`: "1 hr, 15 min" / "1 h et 15 min", zero units omitted), so no duration strings to translate. It is [Baseline 2025](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Intl/DurationFormat) (newly available, recent browsers only) and needs `es2025.intl` in the TypeScript `lib` (`tsconfig.base.json`). The pipe is pure: the language is passed as an argument (Transloco `activeLang` signal), so it recomputes when it changes.
- 2026-10-20 — **Fixed, translated tag catalog** (#116, developer's choice; replaces free-text tags). 101 tags in 8 categories, built from [schema.org Recipe](https://schema.org/Recipe) (`recipeCategory`, `recipeCuisine`, `cookingMethod`, `suitableForDiet` / [RestrictedDiet](https://schema.org/RestrictedDiet)) and recipe-organization guides. Inserted with EF Core [model managed data](https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding#model-managed-data) (`HasData`), meant for "static data that's not expected to change outside of migrations". The stable English key is the primary key, so no generated ids to pin; it is also what the API returns and what URLs use (`?tag=vegetarian`). The migration first deletes the old free-text tags (they can't be mapped to keys); local databases were recreated (`dotnet ef database drop` + `database update`). Adding a tag = edit `TagCatalog` + a migration + both translation files.
- 2026-10-20 — **Built-in Minimal API validation for commands** (#17): `AddValidation()` + DataAnnotations on the request records ([docs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/validation?view=aspnetcore-10.0)), `IValidatableObject` for the items of the steps list; tags are checked against the catalog in the handler (400 "Unknown tag" with the same ProblemDetails shape, via `TypedResults.ValidationProblem`). Invalid requests get a 400 ProblemDetails with `errors` keyed by property path (`Ingredients[0].Name`). Creation returns `201` + `Location` (`CreatedAtRoute` on `GetRecipe`) and only `{ id }`.
- 2026-10-20 — **Shared write model for POST and PUT** (#18): both commands receive a whole recipe, so they share `RecipeInput` (validation, tag loading, `ApplyTo`). CQRS still holds: the split is between read DTOs and write models, not between commands. `RecipeInput` never contains the id or the dates (no mass assignment): `PUT /api/recipes/{id}` takes the id from the route only, keeps `CreatedAt`, sets `UpdatedAt`, and **replaces everything** (fields left out become empty), then returns `204 No Content`; unknown id → 404. Concurrent edits: last write wins for now (single user).
- 2026-10-20 — **DELETE with `ExecuteDeleteAsync`** (#19): one SQL `DELETE` without loading the recipe ([docs](https://learn.microsoft.com/en-us/ef/core/saving/execute-insert-update-delete)); it returns the number of affected rows, so 0 → 404. Its `RecipeTag` links go through the database cascade (`onDelete: Cascade`; e_sqlite3 enables foreign keys, checked on a copy of the database). Hard delete for now (no soft delete / trash). Deleting twice gives 204 then 404: the effect is idempotent, which is what HTTP requires of DELETE, not the status code.
- 2026-10-20 — **`GET /api/tags`** (#20): the catalog as `[{ key, category }]`, in `TagCatalog` order (the database has no order), with the category in camelCase (`mainIngredient`) like the frontend `tagCategories.*` keys. The frontend groups and translates them.
- 2026-10-20 — **Integration tests on in-memory SQLite** (#21, developer's choice). `RecipeBookApiFactory` follows the [ASP.NET Core integration tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0) sample: it replaces the app's `DbContext` registration with an open `DataSource=:memory:` connection and applies the real migrations when the host starts (schema + tag catalog, so migrations are tested too). Replacing the registration also removes the development demo seeding, so tests never see demo data and never touch `recipebook.db`. SQLite is our production database for now, which is what [EF Core recommends testing against](https://learn.microsoft.com/en-us/ef/core/testing/). One database per test class (`IClassFixture`); each test creates its own data, so test order doesn't matter. 25 tests, one class per use case (`Recipes/`, `Tags/`); a mutation check confirmed they catch a regression.
- 2026-10-21 — **Signal Forms instead of Reactive Forms** (#37, developer's choice). All the APIs used are `@publicApi 22.0` (stable): `form()`, `FormField`, `FormRoot`, `required` / `maxLength` / `min` / `max`, `applyEach`, `submit()` ([guide](https://angular.dev/guide/forms/signals/overview): "Signal Forms work best in new applications built with signals"). The form model (`recipe-form.ts`) is not the API format: empty texts are `''`, empty numbers `null` (checked in the `FormField` source), steps are `{ text }` objects so items keep their identity. Rules mirror the API ones (the API stays the source of truth); blank ingredient/step lines are not errors and will be left out of the request (#38). Edit mode pre-fills the model with `linkedSignal` from the detail resource. Errors are shown by `FieldErrors` (`shared/ui`) once a field is touched; `submit()` touches every field.
- 2026-10-21 — **Home-made `LabeledField` instead of Angular Material** (#123, developer's choice). Material's own [form field docs](https://github.com/angular/components/blob/main/src/material/form-field/form-field.md) leave error messages to the template ("it is up to the consumer to toggle which messages should be displayed"), so it wouldn't remove the per-field markup; Tailwind recommends a component for multi-element duplication and a `@layer components` class for a single element ([managing duplication](https://tailwindcss.com/docs/styling-with-utility-classes#managing-duplication)). `lib-labeled-field` (`shared/ui`) wraps label + projected control + `FieldErrors`; it adds what `[formField]` doesn't set (checked in its source): the control's `id` for the label's `for`, `aria-invalid` and `aria-describedby` (via `contentChild(FormField)` + `afterRenderEffect`); errors sit in an `aria-live="polite"` region. Controls share the global `.form-control` class (`styles.css`), red border when `aria-invalid`. Tailwind + CDK is kept.
- 2026-10-21 — **Saving recipes** (#38). `RecipeApi` (`data-access`) holds the commands (`create` → `POST`, `update` → `PUT`), with `HttpClient` turned into promises by RxJS `firstValueFrom` for the Signal Forms async `action`. It uses Angular 22's `@Service()`, generated by the official schematic: "a modern, ergonomic shorthand for the traditional `@Injectable({ providedIn: 'root' })`" ([guide](https://angular.dev/guide/di/creating-and-using-services)). Its body type `RecipeInput` (models lib) is a write model, separate from the read DTOs. `toRecipeInput` leaves blank ingredient/step lines out and sends empty texts as `null`. After saving, the app navigates to the recipe: resources are created per component, so the detail and list pages fetch fresh data, and there is no cache to reload. A failed save returns `{ kind: 'saveFailed' }` from the action, shown as a banner (root field error); mapping the API's 400 errors onto fields is #40.
- 2026-10-21 — **Tag picker as a Signal Forms custom control** (#39). `TagPicker` (`recipes/ui`) implements `FormValueControl<string[]>`: its only required member is `value = model()`, so `[formField]` binds it like a native input ([custom controls](https://angular.dev/guide/forms/signals/custom-controls), stable since v22). Collapsible category sections (native `<details>`, developer's choice over an overlay search), chips with `aria-pressed`, selected tags listed first with a remove button. The catalog comes from `tagCatalogResource()` (`GET /api/tags`) and keeps the API order.
- 2026-10-22 — **API validation errors shown on their fields** (#40). On a 400 with ProblemDetails `errors`, the submission `action` returns one `{ kind: 'server', message, fieldTree }` per message: `submit()` "applies any submission errors resulting from the action to the field … on the sub-field indicated by the `fieldTree` property" (`submit` API docs, [form submission guide](https://angular.dev/guide/forms/signals/form-submission)). `toServerErrors` (`recipe-form.ts`) maps the C# paths (`Title`, `Ingredients[0].Name`, `Steps[1]`, `Tags[0]`) to fields; indexes are shifted back because the request has no blank lines. Unknown paths stay on the root field and are shown in the banner with their message. A server error stays until its field changes, and the form can't be submitted while it is there. Messages are the API's English ones until backend localization (#45). The client rules stay as the first line; the API remains the source of truth (e.g. a whitespace-only title passes `required` but not `[Required]`).
- 2026-10-23 — **Delete with a CDK Dialog confirmation** (#41). `@angular/cdk` installed at `~22.1.0`, aligned with Angular (its `ng add` schematic only adds the dependency, with a tilde). The CDK [dialog docs](https://github.com/angular/components/blob/main/src/cdk/dialog/dialog.md) require its structural styles without Material: `@import '@angular/cdk/overlay-prebuilt.css'` in `styles.css`. `Confirmation.ask()` (`shared/ui`, reusable for #42) opens `ConfirmDialog` (Tailwind-styled through its host classes) and resolves to `true` only on confirm; Escape and backdrop click cancel. Accessibility from the docs: `role: 'alertdialog'`, `ariaLabelledBy`/`ariaDescribedBy` on the title and message, focus goes to the first tabbable element (Cancel is first, so Enter doesn't delete) and returns to the Delete button on close; `aria-modal` stays off, as the CDK marks outside content `aria-hidden` instead. `RecipeApi.delete` → `DELETE /api/recipes/{id}`; on success, or on a 404 (already deleted, the outcome the user wanted), the app navigates to the list; other errors show a banner. Angular Material was considered again and declined: the developer doesn't want the Material Design look, and the CDK already gives the hard parts (a11y, focus, overlay).
- 2026-10-23 — **Unsaved changes guard** (#42). Functional `CanDeactivateFn` generated with `@schematics/angular:guard` (Nx has no guard generator), as in the [route guards guide](https://angular.dev/guide/routing/route-guards#candeactivate), on the new and edit routes. It returns the `Confirmation.ask()` promise ("Router uses the first emitted value"). `RecipeEdit.hasUnsavedChanges` compares the model with its starting value as API bodies (`toRecipeInput`), so blank lines don't count; a `saved` flag lets the post-save navigation through. The guard only covers navigation inside the app: closing or reloading the tab uses `beforeunload`, added only while there are unsaved changes and removed after, as [MDN](https://developer.mozilla.org/en-US/docs/Web/API/Window/beforeunload_event) recommends (bfcache); the browser shows its own generic message there.
- 2026-10-23 — **Home-made toasts** (#43, developer's choice over a library). Checked on npm: `ngx-toastr` only declares Angular `^21`, `ngx-sonner` hasn't been published since March 2025, `@ngxpert/hot-toast` supports Angular 22 but adds `@ngneat/overview` and its own look. The need is small (a confirmation after save and delete), so `Toaster` (`shared/ui`, a signal of messages removed after 5 s) + `ToastList`, placed once in the app layout. `ToastList` is the live region, always rendered: [`role="status"`](https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/Reference/Roles/status_role) (polite, never takes focus), with `aria-atomic="false"` instead of the role's implicit `true` so only the new toast is read. Callers pass translated text, like `Confirmation`. `@ngxpert/hot-toast` is the library to reconsider if richer toasts are needed (actions, promises).
- 2026-10-23 — **Global HTTP error interceptor tells the cause, screens tell what failed** (#44, developer's choice). Every screen already handles its errors (banner + Retry, 400 → fields, 404 → not found), so a toast for every error would duplicate them. `httpErrorInterceptor` (functional, generated with `@schematics/angular:interceptor`, in the app because it wires app-wide UI) only toasts the cause on `/api/` calls: **API unreachable** for status 0 (network error or timeout, per the [request failure docs](https://angular.dev/guide/http/making-requests#handling-request-failure)) and 502–504 (a proxy couldn't reach the API; the Vite dev server proxy answers 502 when the API is down, checked in its source), **server error** for other 5xx. 4xx are left to the screens. The error is passed on unchanged (`tap({ error })`), so resources and `try/catch` still see it. `Toaster.show()` now ignores a message that is already shown, so several failing requests give one toast.
- 2026-10-24 — **API localization in two steps** (#45, developer's choice). The `Microsoft.Extensions.Validation` pipeline that localizes DataAnnotations messages and display names only exists from .NET 11 ([docs](https://learn.microsoft.com/aspnet/core/fundamentals/validation#localize-validation-messages), `>= aspnetcore-11.0`); on .NET 10 every attribute would need `ErrorMessageResourceType`/`ErrorMessageResourceName`, code to throw away a month later (.NET 11 ships on November 10, 2026, STS supported 24 months, about as long as .NET 10 LTS). **Step 1** (now): `AddLocalization` (`ResourcesPath = "Resources"`) and `UseRequestLocalization` with `en` (default) and `fr`, configured as in the [official sample](https://learn.microsoft.com/aspnet/core/fundamentals/localization/select-language-culture) and placed first in the pipeline; the culture comes from `Accept-Language`, which the frontend already sends (#27), `fr-FR` falls back to `fr`. Our own messages (empty step, step too long, unknown tag) go through `IStringLocalizer<RecipeInput>`, with the English text as key ("If the localized value … isn't found, then the indexer key is returned", [docs](https://learn.microsoft.com/aspnet/core/fundamentals/localization/make-content-localizable)), so only `Resources/Features/Recipes/RecipeInput.fr.resx` exists (path naming, [resource file naming](https://learn.microsoft.com/aspnet/core/fundamentals/localization/provide-resources#resource-file-naming)). `IValidatableObject.Validate` gets the localizer from `ValidationContext` (it carries the request services). Edit `.resx` files with the ResX Viewer and Editor extension the docs mention. **Step 2**: #132, upgrade to .NET 11, then the attribute messages.
- 2026-10-24 — **Dark themes chosen by the user instead of dark mode** (#112, developer's choice from her theme artifact). Bordeaux brand color in every theme; backgrounds Cave à vin (default) / Sous-bois / Nuit bleue (`data-theme` on `<html>`), accents Citron (default) / Safran / Menthe / Glacier (`data-accent`), picked separately (picker: #47, favicon: #134). Tokens are Tailwind [theme variables](https://tailwindcss.com/docs/theme) in `@theme` (`bg-canvas`, `text-fg-muted`, `bg-primary`, `border-line-strong`…); the generated utilities reference the CSS variables, so overriding them under `[data-theme]` / `[data-accent]` (in `@layer base`) reskins the app without per-theme classes. The default palette is removed (`--color-*: initial`) so no raw color can slip back in. `color-scheme: dark` for native controls. Contrast checked for every combination: text ≥ 6:1 (AA asks 4.5:1); `line` (≈1.5:1) is for decorative separators only, inputs use `line-strong`, computed per background to reach 3:1 (WCAG 1.4.11); focus ring in the accent color (`:focus-visible`). Repeated buttons and error banners became `@layer components` classes (`.btn-primary`, `.btn-accent`, `.btn-ghost`, `.btn-danger`, `.alert-error`), like `.form-control`; in Tailwind v4 `@apply` only takes utilities, so the shared button base is one combined selector. Fonts self-hosted with [Fontsource](https://fontsource.org/docs/getting-started/install) (no request to Google): Young Serif (headings, single weight, so headings are `font-normal`) and Figtree Variable (text). They are imported in `styles.css`: the Fontsource Angular guide imports them from `main.ts`, which Angular's esbuild builder rejects (no loader for font files imported from TypeScript); from the stylesheet, Angular copies the font files to `media/` and rewrites their URLs.
- 2026-10-24 — **Theme picker with the CDK Menu** (#47). A « Thème » button in the header opens a [CDK menu](https://github.com/angular/components/blob/main/src/cdk/menu/menu.md) with two `cdkMenuGroup`s of `cdkMenuItemRadio` (background, accent): roles (`menu`, `group`, `menuitemradio` + `aria-checked`), keyboard navigation and focus return come from the CDK; groups are labelled with `aria-label`. `ThemeSettings` (`shared/ui`) holds two signals and an `effect` writes them to `<html data-theme / data-accent>` and `localStorage` (syncing with storage and custom DOM behavior are the use cases the [effects guide](https://angular.dev/guide/signals/effect) gives); stored values are validated against the lists, storage errors are ignored. An inline script in `index.html` applies the stored values before the first paint (Angular boots after the CSS is painted, so the default theme would flash); it doesn't validate, unknown values just match no CSS rule. Each option shows a swatch of its own colors: the themed tokens moved to `@theme inline` reading `--theme-*` variables, so utilities resolve the variable on the element itself ([referencing other variables](https://tailwindcss.com/docs/theme#referencing-other-variables)) and an element with `data-theme` shows that theme.
- 2026-10-24 — **Favicon « Marmite »** (#134, developer's choice). `public/favicon.svg` is the source (bordeaux tile, Citron steam: favicons are cached by browsers, so it doesn't follow the chosen accent). Following [MDN `rel="icon"`](https://developer.mozilla.org/en-US/docs/Web/HTML/Reference/Attributes/rel#icon) ("the browser uses their `media`, `type`, and `sizes` attributes … If several icons are equally appropriate, the last one is used … if … an unsupported format, the browser proceeds to the next"), `index.html` lists `favicon.ico` (16 + 32 px fallback, replacing Angular's logo) then `favicon.svg`, plus `apple-touch-icon.png` (180 px, square: iOS ignores `rel="icon"` and rounds the corners itself). The PNG/ICO files were rendered from the SVG with headless Chrome (ICO with PNG-compressed images); regenerate them if the SVG changes. The header logo is the same drawing inline, colored with theme utilities (`fill-primary`, `stroke-accent`), so its steam follows the accent.

### Session log

<!-- One entry per session, most recent first. Format:
#### YYYY-MM-DD — Phase N
- Done: …
- Learned: … (Angular/.NET concepts covered)
- Next step: …
- Open questions: …
-->

#### 2026-10-19 → 23 — Phases 6 and 7 (+ phase 4 writes)

- Done:
  - Phase 6 (browse): #31 read models (PR #108), #32 read resources with `httpResource()` (PR #109), #33 list page with recipe cards and a `models` lib (PR #110), #34 title search and tag filter in the URL (PR #111), #35 detail page with Transloco Locale (PR #113), #36 `Intl.DurationFormat` pipe (PR #114).
  - Phase 4 writes: #17 `POST` with built-in validation (PR #115), #116 fixed translated tag catalog (PR #117), #18 `PUT` (PR #118), #19 `DELETE` (PR #119), #20 `GET /api/tags` (PR #120), #21 integration tests on in-memory SQLite (PR #121).
  - Phase 7 (create & edit): #37 Signal Forms recipe form (PR #122), #123 `LabeledField` (PR #124), #38 save (PR #125), #39 tag picker (PR #126), #40 API validation errors on fields (PR #127), #41 delete with a CDK Dialog confirmation (PR #128), #42 unsaved changes guard (PR #129), #43 toasts.
- Learned:
  - `httpResource()` (status signals, reload), route params and query params as `input()`s, RxJS debounce with `takeUntilDestroyed`;
  - Minimal API commands: `TypedResults` / `Results<…>`, `AddValidation()` + DataAnnotations, `CreatedAtRoute`, `ExecuteDeleteAsync`, `HasData` for reference data;
  - API safety: server-side validation as the source of truth, no mass assignment (id from the route only), idempotent DELETE, unknown tags refused even though the UI can't send them;
  - `WebApplicationFactory` integration tests;
  - Signal Forms: `form()`, schema rules, `applyEach`, `when`, `submit()` with server errors through `fieldTree`, custom controls (`FormValueControl`);
  - `linkedSignal`, `contentChild`, `afterRenderEffect`, `effect` with `onCleanup`, `@Service()`;
  - CDK Dialog, functional `canDeactivate` guard, `beforeunload`, live regions (`aria-live`, `role="status"`).
- Next step: phase 8, e.g. #44 global HTTP error interceptor, #45 backend localization (will translate the API's validation messages), #112 app theme, #47 dark mode, #46 cooking mode.
- Open questions:
  - whitespace-only titles/names are only caught by the API (client `required` accepts them);
  - no shared cache between pages (the edit page fetches the recipe again), accepted for now;
  - frontend unit tests still to set up (#49).

#### 2026-10-18 / 19 — Phase 5

- Done:
  - #22 Angular app `apps/web`, generated with explicit flags (Vitest via `@angular/build:unit-test`, no SSR, no e2e), lint target converted to the inferred plugin (PR #96).
  - #23 Tailwind v4 (PR #97).
  - #24 six libs with `type:`/`scope:` tags and `@nx/enforce-module-boundaries` rules (PR #98).
  - #26 data fetching with Angular resources (`httpResource()`) + `HttpClient` instead of TanStack Query (PR #100, docs only).
  - #27 `Accept-Language` interceptor (PR #101).
  - #28 dev proxy `/api/**` → `https://localhost:7288` (PR #102).
  - #30 routing with lazy-loaded recipes feature, 404 page, `withComponentInputBinding()` (PR #103).
  - #29 layout with header, navigation and language switcher; `nx-welcome` removed (PR #104).
  - i18n: #25 Angular i18n with one build per locale (PR #99), then replaced by **Transloco v8** in #105 (PR #106), because build-time i18n was too constraining.
- Learned:
  - Nx lib types and module boundaries (Nx convention, not Angular's);
  - standalone components: `imports`, `input()` / `input.required()`, signals, `@if` / `@for` with `track`;
  - template bindings: `[prop]`, `[attr.x]`, `[class.x]`, `(event)`;
  - router: `loadChildren`, route order, `redirectTo` + `pathMatch: 'full'`, `**`, `title`, `routerLink` / `routerLinkActive`, `<router-outlet>`;
  - functional interceptors and `inject()`;
  - Angular build-time i18n vs runtime i18n (Transloco: `*transloco="let t"`, `setActiveLang`, signals API, persist-lang);
  - custom `TitleStrategy`, `provideEnvironmentInitializer`;
  - the Vite-based dev server proxy;
  - `UseHttpsRedirection` and launch profiles.
- Next step: phase 6, starting with #31 (TS models + `RecipeApi`) and #32 (read resources with `httpResource()`).
- Open questions:
  - date formatting for the active language (#35: Transloco Locale l10n plugin or Angular locale data);
  - libs have no unit test runner yet (decide in phase 9, #49; the interceptor test is noted there);
  - in the agent's shell only, remove `NX_WORKSPACE_ROOT_PATH` before Nx commands (lowercase drive letter breaks Vitest).

#### 2026-10-18 — Phase 4 (reads)

- Done: #15 `GET /api/recipes` with accent-insensitive title search (normalized `SearchTitle` column) and tag filter (PR #94). #16 `GET /api/recipes/{id}` with `Results<Ok<T>, NotFound>` (PR #95). Long-term vision added to the README (PR #93: multi-user, database migration, companion app; issues #53, #91, #92).
- Learned: Minimal API route groups and extension methods, `TypedResults`, route constraints (`{id:guid}`); CQRS at the basic level (queries project to read DTOs, no MediatR); C# `record`, `static`, primary constructors; SQLite limits (case folding for ASCII only, no APPLY for projections of JSON collections → load the columns, then map in memory).
- Next step: the write endpoints (#17–#21) are postponed until after the frontend foundations, at the developer's request (the read endpoints are enough for phase 6).
- Open questions: tag name normalization when creating recipes (#17/#20); demo seeding vs integration tests (#21).

#### 2026-10-18 — Phase 3

- Done: #11 domain entities `Recipe`, `Tag`, `Ingredient`, `RecipeStep` (PR #87). #12 `AppDbContext` and entity configurations (PR #88). #13 SQLite connection and initial migration (PR #89). #14 idempotent demo recipes in development with `UseSeeding` / `UseAsyncSeeding` (PR #90).
- Learned: code-first EF Core; complex types stored as JSON (`ComplexCollection(...).ToJson()`); skip-navigation many-to-many; `IEntityTypeConfiguration` + `ApplyConfigurationsFromAssembly`; migrations with the local `dotnet ef` tool; value converters (UTC dates); connection strings and secrets (no secret in config files: user-secrets in dev, environment variables in prod, step added to #60).
- Next step: phase 4 (#15, #16).
- Open questions: killing `dotnet ef` mid-run leaves an abandoned `__EFMigrationsLock` row (fix in section 10).

#### 2026-10-18 — Phase 2

- Done: #79 whole repo formatted with Prettier (PR #80). #8 `nx add @nx/dotnet`, `RecipEBook.slnx`, API and integration test projects (PR #81). #9 Scalar, ProblemDetails (`AddProblemDetails` + `UseExceptionHandler` + `UseStatusCodePages`), `/health`, weather sample removed (PR #84, `Program.cs` written by the developer). #82 Postman files ignored (PR #83). #10 first integration test with `WebApplicationFactory<Program>` (PR #85).
- Learned: .NET vs ASP.NET Core (platform vs web framework, `Microsoft.NET.Sdk` vs `Microsoft.NET.Sdk.Web`); OpenAPI vs Swagger (spec vs tools) and Scalar; the ASP.NET Core pipeline (services before `Build()`, middlewares after); launch profiles (`dotnet run --launch-profile https`, first profile = `http` by default); `dotnet watch` hot reload; .NET 10 noun-first CLI (`dotnet package add`, `dotnet reference add`); xUnit `IClassFixture` and primary constructors; .NET 10 generates `public partial class Program` for tests.
- Next step: phase 3 (#11 domain entities, then EF Core + SQLite).
- Open questions: `nx format:check` without `--all` only checks changed files → use `--all` in CI (#51). C# formatter in VS Code still to decide (Prettier can't format C#; `.cs` files are saved with CRLF, normalized to LF by `.gitattributes`). `dotnet tool install/update` rewrites `.config/dotnet-tools.json` in its own style: run `nx format:write` afterwards.

#### 2026-10-18 — Official-docs audit

- Done: audited phases 0–1 against the official docs (Nx, @nx/angular, @nx/dotnet README, Microsoft .NET docs, fnm, npm, Prettier, git, VS Code, SQLite). Follow-up issues #71–#74: redo `nx init` interactively (PR #76), move the tool manifest to `.config/` (PR #75), official config templates (PR #77, stacked on #76), this docs update.
- Learned: `nx init` behaves differently with and without a `package.json` (`.nx/installation` mode for non-JS repos); npm 11 `install-scripts approve/deny`; `dotnet new editorconfig` / `gitignore` templates.
- Next step: once merged, phase 2 (#8: `nx add @nx/dotnet` + .NET projects).
- Open questions: C# formatter in VS Code (Prettier is the global default formatter and can't format C#), to decide in phase 2 from the C# Dev Kit docs. The VS Code terminal needs a full VS Code restart after installing tools that change the PATH (fnm).

#### 2026-10-17 — Phase 1

- Done: #5 Nx 23.3.0 + `@nx/angular` (Angular 22.1) + `@nx/dotnet`, `package-lock.json` committed, nx postinstall approved via npm `allowScripts` (PR #67). #6 `.gitignore` rewrite (PR #68, stacked on #67). #7 `.editorconfig`, `.gitattributes` (LF), Prettier 3.9.10, VS Code workspace settings (PR #69, stacked on #67).
- Learned: Nx inferred plugins (`plugins` in `nx.json`), `nx format`, npm 11 `allowScripts`, `.gitattributes` vs `core.autocrlf`.
- Next step: once merged, phase 2 (#8: create the .NET projects and register `@nx/dotnet`).
- Open questions: `npm audit` reports 7 vulnerabilities (1 high) in `undici`, a transitive dev dependency of the Nx/Angular tooling; revisit when upgrading Nx/Angular.

#### 2026-10-17 — Phase 0

- Done: #1 Node 24.22 via fnm + `.nvmrc` (PR #63), #2 .NET SDK 10.0.402 + `global.json` (PR #64), #3 dotnet-ef 10.0.13 as a local tool (PR #65), #4 VS Code extensions + `.vscode/extensions.json` (PR #66). PowerShell CurrentUser execution policy set to `RemoteSigned` (needed for the fnm profile hook and npm `.ps1` shims).
- Learned: SDK pinning with `global.json` (`rollForward`), .NET local tools (≈ npm devDependencies), Node version switching with fnm.
- Next step: once the PRs are merged, phase 1 (#5 Nx init, #6 .gitignore, #7 .editorconfig/.gitattributes/Prettier).
- Open questions: git uses `core.autocrlf=true` locally → `.gitattributes` with LF planned in #7. Agent shells may not load the PowerShell profile: if `node -v` isn't 24, run `fnm use` first. `dotnet dev-certs https --trust` will be needed in phase 2.

#### 2026-10-17 — Planning

- Done: defined pitch, stack, architecture and roadmap. Added Nx, Tailwind, EN/FR i18n and TanStack Query at the developer's request. Docs switched to English. Created the GitHub project board: 6 labels, 11 milestones, 60 issues (#1–#60).
- Finding: Node 18.12 and .NET SDK 6 installed → upgrade needed (phase 0). Docker missing → SQLite in dev. `gh` CLI installed and authenticated (with `project` scope).
- Next step: phase 0 (#1–#4), then phase 1.

---

## 8. Agent instructions

If you're a Claude agent picking up this project:

1. **Read this whole README**, especially [Progress & log](#7-progress--log), then check the [project board](https://github.com/users/Patajuade/projects/2) to know where we are.
2. **Languages**: talk with the developer **in French**. Write all documentation, code comments and **commit messages in English**.
3. **Teaching mode**: the goal is as much learning as shipping.
   - Move in **small steps**, explain every new concept (Angular or .NET) before or while introducing it.
   - For Angular, draw **parallels with React** where relevant (see section 9).
   - Flag "the old Angular way" when docs or online examples may cause confusion (NgModules, `*ngIf`, `@Input()` decorator, constructor-based DI…).
   - Regularly offer the developer to write a part herself (a component, an endpoint), then review her code.
4. **Respect the stack and architecture** described here. For any deviation, ask first, then record it under "Decisions".
5. **Official docs only — never improvise**:
   - Before installing or configuring anything (package, CLI flag, config file, generator, API), read the official docs of that tool and follow the documented procedure. Prefer official generators/templates (`nx g`, `dotnet new`, `npm init`, `npm pkg`…) over hand-written files.
   - Cite the doc pages you followed in the PR description.
   - The plan in this README and the planning issues were written from memory: treat API names and commands there as hints to verify, not as instructions.
   - When the docs are missing, ambiguous or contradict the actual tool behavior, or a workaround seems needed: **stop and ask the developer**. Never guess a flag or an option.
   - Check the actually installed versions: these tools move fast.
6. **i18n**: every new UI string needs its key in both `apps/web/public/i18n/en.json` **and** `fr.json` (the agent writes the translations; the developer reviews them). A component using `t(...)` imports `TranslocoDirective`.
7. **Nx**: generate apps and libs with Nx generators (not by hand), respect tags and module boundaries.
8. **Git**:
   - Always start from an up-to-date `main`. One branch per issue: `<type>/<issue>-<short-slug>` (`feat`, `fix`, `chore`, `docs`, `test`, `refactor`).
   - Conventional Commits in English, with `Closes #N` in the commit or PR body.
   - Push the branch and open a PR (`gh pr create`), then stop. **Never merge into `main`** (no `git merge` into main, no `gh pr merge`, no push to main): the developer reviews and merges PRs herself.
9. **Board (GitHub Projects, via `gh`)**:
   - At the start of a session, check the board and the current milestone's open issues.
   - When starting an issue, move it to _In Progress_ (`gh project item-edit`). Reference it in commits/PRs (`Closes #N`) so it closes and moves to _Done_ once merged into `main`. Issues closed outside a merge must be closed manually.
   - New task discovered along the way → create an issue in the right milestone with the right labels, and add it to the board. Don't add tasks to this README.
10. **End of session**: update the "Current phase" line if needed and add a session log entry (issues done, concepts learned, next step).

---

## 9. React → Angular cheat sheet

| React                                           | Modern Angular                                                                                  |
| ----------------------------------------------- | ----------------------------------------------------------------------------------------------- |
| Function component                              | Standalone `@Component({ ... })` class, separate or inline HTML template                        |
| `useState`                                      | `signal()` → read `count()`, write `count.set()` / `count.update()`                             |
| `useMemo`                                       | `computed()`                                                                                    |
| `useEffect`                                     | `effect()` (use sparingly), `afterNextRender()` for DOM work                                    |
| Props                                           | `input()` / `input.required()`                                                                  |
| Callback prop (`onChange`)                      | `output()` + `(event)="..."` in the template                                                    |
| Controlled prop + callback (`value`/`onChange`) | `model()` + `[(value)]` (two-way binding)                                                       |
| `children`                                      | `<ng-content />` (content projection)                                                           |
| Context + Provider                              | Services + dependency injection (`inject()`)                                                    |
| Custom hooks                                    | Functions using `inject()` or `httpResource()`, called in an injection context                  |
| React Router                                    | Angular Router (`provideRouter`, `routerLink`, `loadComponent`)                                 |
| `useQuery` (TanStack)                           | `httpResource(() => url)`: signals `value()`, `isLoading()`, `error()`, refetch with `reload()` |
| `useMutation` (TanStack)                        | A `HttpClient` call (`post`, `put`, `delete`), then `reload()` on the affected resources        |
| `fetch` / axios                                 | `HttpClient` (returns Observables; uses `fetch` under the hood)                                 |
| react-hook-form (+ Zod)                         | Signal Forms: `form(signal(model), schema)`, `[formField]`, `field().errors()`                  |
| react-i18next `t('key')`                        | Transloco: `*transloco="let t"` then `t('key', { param })`; `translateSignal('key')` in code    |
| `{cond && <X/>}`                                | `@if (cond) { <x /> }`                                                                          |
| `list.map(i => <X key={i.id}/>)`                | `@for (i of list(); track i.id) { <x /> }`                                                      |
| `React.lazy` + `Suspense`                       | `loadComponent` in routes, `@defer` in templates                                                |
| `className={clsx(...)}`                         | `class="..."` + `[class.active]="isActive()"` (Tailwind works the same)                         |
| Radix / Headless UI                             | Angular CDK (dialog, overlay, drag-drop, a11y)                                                  |

---

## 10. Useful commands

_(To be completed along the phases: check project names with `nx show projects`.)_

```bash
# Toolchain (pinned in the repo: .nvmrc → Node 24, global.json → .NET SDK 10, .config/dotnet-tools.json → dotnet-ef)
fnm use                                 # switch to the Node version from .nvmrc (automatic on cd in a profile-enabled shell)
dotnet tool restore                     # restore local .NET tools (dotnet-ef)

# Nx
nx graph                                # project graph
nx show projects                        # detected projects
nx show project <name>                  # inferred tasks of a project
nx run-many -t serve                    # run frontend + backend
nx affected -t lint test build          # what CI runs

# Frontend + backend in dev: two terminals
dotnet watch run --project apps/api/RecipEBook.Api.csproj --launch-profile https   # 1. API on https://localhost:7288
nx serve web                                                                       # 2. app on http://localhost:4200, /api/** proxied to the API

# Frontend
nx serve web
nx build web
nx serve-static web                     # serve the production build
nx test web
nx g @nx/angular:component libs/web/recipes/ui/src/lib/recipe-card/recipe-card
nx g @nx/angular:library libs/web/recipes/ui --tags=type:ui,scope:recipes

# Backend (from the root)
dotnet run --project apps/api --launch-profile https                          # API on https://localhost:7288 (/health, /scalar)
dotnet watch run --project apps/api/RecipEBook.Api.csproj --launch-profile https  # same, with hot reload
npm exec nx -- run RecipEBook.Api:run --launch-profile=https                   # same, through Nx
npm exec nx -- test RecipEBook.Api.IntegrationTests                           # integration tests (or `dotnet test`)

# Backend (EF Core, from the root; database file: apps/api/recipebook.db, git-ignored)
dotnet ef migrations add <Name> --project apps/api --output-dir Infrastructure/Persistence/Migrations  # after a model change
dotnet ef database update --project apps/api           # create/upgrade the local database (also after pulling new migrations); in Development it also inserts the demo recipes if the database has none
# If `database update` hangs, a previous run was killed and left an abandoned lock: run `DELETE FROM "__EFMigrationsLock";` on apps/api/recipebook.db (https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations#concurrent-migrations-protection)
dotnet ef migrations has-pending-model-changes --project apps/api  # model changed without a migration?
dotnet ef migrations remove --project apps/api          # undo the last, not yet applied, migration
```
