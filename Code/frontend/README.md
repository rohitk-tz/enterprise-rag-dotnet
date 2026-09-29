# RAG Project — Frontend

Next.js web UI for the RAG platform: sign in with Clerk, create projects, upload files or add URLs to a project's knowledge base, watch each document move through the processing pipeline, and chat with an assistant that answers with citations.

It talks to the .NET Api (see the [main README](../README.md)) over REST, sending the Clerk session token as a bearer token.

## Tech stack

- **Next.js 16.1** (App Router, `output: "standalone"`) + **React 19.2** + **TypeScript 5**
- **Tailwind CSS 4** (via `@tailwindcss/postcss`)
- **Clerk** (`@clerk/nextjs` 6) for authentication
- `react-dropzone` (file uploads), `react-hot-toast` (notifications), `lucide-react` (icons)
- Geist Sans / Geist Mono fonts via `next/font`

## Project structure

```
frontend/
├── src/
│   ├── app/
│   │   ├── layout.tsx                         # Root layout: ClerkProvider + Toaster
│   │   ├── page.tsx                           # "/" — redirects to /projects or /sign-in
│   │   ├── globals.css
│   │   ├── (auth)/
│   │   │   ├── sign-in/[[...sign-in]]/page.tsx
│   │   │   └── sign-up/[[...sign-up]]/page.tsx
│   │   └── (dashboard)/projects/
│   │       ├── layout.tsx                     # Auth check + Sidebar
│   │       ├── page.tsx                       # Projects grid, create/delete projects
│   │       └── [projectId]/
│   │           ├── page.tsx                   # Knowledge base (files/URLs), conversations, settings
│   │           └── chats/[chatId]/page.tsx    # Chat with citations + message feedback
│   ├── components/
│   │   ├── chat/                              # ChatInterface, MessageList, MessageItem, ChatInput, feedback modal
│   │   ├── projects/                          # ProjectsGrid, CreateProjectModal, KnowledgeBaseSidebar,
│   │   │   │                                  # ConversationsList, FileDetailsModal
│   │   │   └── document-details/              # Pipeline viewer: partitioning / chunking / summarising steps, chunk inspector
│   │   ├── layout/Sidebar.tsx
│   │   └── ui/                                # LoadingSpinner, NotFound
│   ├── lib/
│   │   ├── api/index.ts                       # apiClient: get/post/put/delete + direct S3 upload
│   │   └── types/index.ts                     # Shared TypeScript types
│   └── proxy.ts                               # Clerk route protection (Next.js 16's replacement for middleware.ts)
├── public/
├── Dockerfile                                 # Multi-stage node:20-alpine build, runs the standalone server
├── next.config.ts
└── package.json
```

## Routes

| Route | Auth | Purpose |
|---|---|---|
| `/` | — | Redirects to `/projects` when signed in, otherwise `/sign-in` |
| `/sign-in`, `/sign-up` | Public | Clerk hosted components |
| `/projects` | Required | List, create and delete projects |
| `/projects/[projectId]` | Required | Manage documents (upload / add URL / inspect / delete), chats and RAG settings |
| `/projects/[projectId]/chats/[chatId]` | Required | Chat with the assistant |

Public routes are declared in `src/proxy.ts`; everything else is protected by `clerkMiddleware`, and the dashboard layout also redirects unauthenticated users.

## Getting started

### Prerequisites

- Node.js 20.9+ (required by Next.js 16)
- A Clerk application (publishable + secret key)
- The Api running (default `http://localhost:8000`)

### Setup

```bash
cd Code/frontend
cp .env.example .env.local
# fill in NEXT_PUBLIC_CLERK_PUBLISHABLE_KEY and CLERK_SECRET_KEY
npm install
npm run dev
```

Open http://localhost:3000.

### Environment variables

| Variable | Purpose |
|---|---|
| `NEXT_PUBLIC_API_URL` | Base URL of the .NET Api (falls back to `http://localhost:8000`) |
| `NEXT_PUBLIC_CLERK_PUBLISHABLE_KEY` | Clerk publishable key (baked in at build time) |
| `CLERK_SECRET_KEY` | Clerk secret key (read at runtime by the server) |

## Scripts

- `npm run dev` — development server with hot reload
- `npm run build` — production build (standalone output)
- `npm start` — run the production build
- `npm run lint` — ESLint

## Docker

The frontend is built and run by the root `docker-compose.yml` (from `Code/`). `NEXT_PUBLIC_API_URL` and `NEXT_PUBLIC_CLERK_PUBLISHABLE_KEY` are passed as build args, and `CLERK_SECRET_KEY` is passed at runtime; all values come from `Code/.env`.

```bash
cd Code
docker compose up --build frontend
```
