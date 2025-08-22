/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_GATEWAY_URL: string
  readonly VITE_AUTH_URL: string
  readonly VITE_PROBLEMS_URL: string
  readonly VITE_SUBMISSIONS_URL: string
  readonly VITE_AI_URL: string
  readonly VITE_SEARCH_URL: string
  readonly VITE_ADMIN_URL: string
  readonly VITE_PAYMENTS_URL: string
  readonly VITE_APP_NAME: string
  readonly VITE_APP_VERSION: string
  readonly VITE_APP_DESCRIPTION: string
  readonly VITE_STRIPE_PUBLISHABLE_KEY: string
  readonly VITE_GOOGLE_CLIENT_ID: string
  readonly VITE_GITHUB_CLIENT_ID: string
  readonly VITE_ENABLE_DEV_TOOLS: string
  readonly VITE_LOG_LEVEL: string
  readonly VITE_WS_URL: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}