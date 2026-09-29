# Due Jobs frontend

Standalone Angular application using local mock data. Run `npm ci` and
`npm start` here, then open http://localhost:4200. The backend is not required.

## Commands

- `npm start`: development server with an API proxy and source maps.
- `npm run build`: optimized production build with bundle size budgets.
- `npm run build:development`: unoptimized build with source maps.
- `npm run typecheck`: strict TypeScript and Angular template checks.

## Structure

```text
src/
  app/
    app.component.*              Application shell and navigation
    app.config.ts                Application-wide providers
    app.routes.ts                Lazy page routes
    core/http/                   API base URL configuration
    features/jobs/
      models/                    Typed API response contracts
      data-access/               Jobs service and local mock fixtures
      pages/jobs-page/           Loading, errors, and page state
      components/jobs-table/     Job table presentation
      components/job-status/     Job status presentation
    shared/
      ui/empty-state/            Reusable empty-state presentation
      pages/not-found/           Unknown-route fallback
  styles.css                     Global styles and design variables
  main.ts                        Application bootstrap
```

Keep business-specific UI and data access inside its feature. Shared UI should
not depend on a feature. Page components coordinate requests and state; display
components receive typed inputs. Data subscriptions end when the page is destroyed.

The jobs service returns three mock jobs through an Observable with a short delay
so the loading state remains visible. Each request creates fresh fixtures.

The jobs page lives at `/dashboard/jobs`, keeping browser routes separate from
the backend's `/jobs` endpoint. During development, `proxy.conf.json` forwards
API requests when HTTP is used; the current mock service makes no API requests.
For deployment with an API, serve `dist/due-jobs/browser`, forward `/jobs`
and its child paths to the API, and use `index.html` as the fallback for UI routes.
An `API_BASE_URL` injection token is available for HTTP data services; it is not
used by the current mock service.

The dashboard supports listing and refreshing jobs. The backend's create,
cancel, and process-due actions are not yet exposed in the UI.
