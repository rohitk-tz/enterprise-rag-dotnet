# RAG Project - Frontend Client

A modern Next.js web application for the RAG (Retrieval-Augmented Generation) project with authentication and responsive UI.

## Overview

This is a Next.js 16 frontend built with React 19, TypeScript, and Tailwind CSS. It integrates Clerk for authentication and provides a clean, user-friendly interface for managing RAG projects.

## Tech Stack

- **Framework**: Next.js (16.1.4)
- **Runtime**: React (19.2.3) with React DOM (19.2.3)
- **Language**: TypeScript (5)
- **Styling**: Tailwind CSS (4) with PostCSS (4)
- **Authentication**: Clerk (@clerk/nextjs 6.36.8)
- **Linting**: ESLint (9)
- **Node.js**: 18+ recommended

## Project Structure

```
client/
├── src/
│   ├── app/                         # Next.js 13+ app router
│   │   ├── layout.tsx               # Root layout with ClerkProvider
│   │   ├── page.tsx                 # Home page (redirects based on auth)
│   │   ├── globals.css              # Global styles
│   │   ├── about/
│   │   │   └── page.tsx             # About page
│   │   ├── projects/
│   │   │   └── page.tsx             # Projects listing page
│   │   ├── sign-in/
│   │   │   └── [[...sign-in]]/
│   │   │       └── page.tsx         # Clerk sign-in page
│   │   └── sign-up/
│   │       └── [[...sign-up]]/
│   │           └── page.tsx         # Clerk sign-up page
│   └── proxy.ts                     # Clerk proxy for auth
├── public/                          # Static assets
├── next.config.ts                   # Next.js configuration
├── tsconfig.json                    # TypeScript configuration
├── package.json                     # Project dependencies
└── README.md                        # This file
```

## Installation

### Prerequisites

- Node.js 18+ (npm or yarn)
- Clerk account for authentication

### Setup Steps

1. **Navigate to client directory**:
   ```bash
   cd client
   ```

2. **Install dependencies**:
   ```bash
   npm install
   ```

3. **Configure environment variables**:
   Create a `.env.local` file with:
   ```
   NEXT_PUBLIC_CLERK_PUBLISHABLE_KEY=<your-clerk-publishable-key>
   CLERK_SECRET_KEY=<your-clerk-secret-key>
   ```

4. **Start development server**:
   ```bash
   npm run dev
   ```
   Access at `http://localhost:3000`

## Available Scripts

- `npm run dev` - Start development server with hot reload
- `npm run build` - Build production-ready application
- `npm start` - Start production server
- `npm run lint` - Run ESLint code quality checks

## Pages & Routes

| Route | Component | Auth Required | Status |
|-------|-----------|---------------|--------|
| `/` | page.tsx | No | Redirects based on auth |
| `/sign-in` | sign-in/page.tsx | No | Clerk auth UI |
| `/sign-up` | sign-up/page.tsx | No | Clerk registration |
| `/projects` | projects/page.tsx | Yes | Placeholder |
| `/about` | about/page.tsx | No | Placeholder |

## Authentication with Clerk

**Setup Overview**:
- `ClerkProvider` wraps the app in `layout.tsx`
- `src/proxy.ts` manages auth routes (Clerk's newer convention)
- Public routes: `/`, `/sign-in/*`, `/sign-up/*`

**Protecting a Page**:
```typescript
import { auth } from "@clerk/nextjs/server";
import { redirect } from "next/navigation";

export const dynamic = "force-dynamic";

export default async function ProtectedPage() {
  const { userId } = await auth();
  if (!userId) redirect("/sign-in");
  return <div>Protected Content</div>;
}
```

## Styling

- **Tailwind CSS v4**: Utility-first CSS framework
- **Global Styles**: `src/app/globals.css`
- **Fonts**: Geist Sans & Mono from Google Fonts

## Deployment

### Vercel (Recommended)
```bash
npm run build
npm start
```
Then push to GitHub and connect to Vercel dashboard.

### Self-hosted
```bash
npm run build
npm start
```

### Pre-deployment Checklist
- [ ] Environment variables configured
- [ ] Clerk keys correct for production
- [ ] `npm run lint` passes
- [ ] `npm run build` succeeds locally
- [ ] All tests pass

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Auth returns null | Add `export const dynamic = "force-dynamic";` to server components |
| Redirect loop on sign-in | Ensure `/sign-in(.*)` and `/sign-up(.*)` are in public routes in `proxy.ts` |
| Port 3000 in use | Kill process or use `npm run dev -- -p 3001` |
| Missing Clerk keys | Create `.env.local` with both `NEXT_PUBLIC_*` and `CLERK_SECRET_KEY` |

## Project Status

**Version**: 0.1.0 (In Development)

- ✅ Authentication with Clerk
- ✅ Basic page structure
- 🚧 Projects page implementation
- 🚧 Backend API integration
- ⏳ Additional features

## Support

- [Next.js Docs](https://nextjs.org/docs)
- [Clerk Docs](https://clerk.com/docs)
- [Tailwind Docs](https://tailwindcss.com/docs)

## License

MIT
