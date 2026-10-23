const base = import.meta.env.VITE_API_URL ?? ''

export class ApiError extends Error {
  status: number

  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

function token() {
  const raw = localStorage.getItem('ste.session')
  if (!raw) return null
  try {
    return (JSON.parse(raw) as { token?: string }).token ?? null
  } catch {
    return null
  }
}

function errorMessage(data: unknown, fallback: string) {
  if (!data || typeof data !== 'object') return fallback
  const body = data as { message?: string; title?: string; errors?: Record<string, string[]> }
  if (body.message) return body.message
  if (body.errors) return Object.values(body.errors).flat().join(' ')
  return body.title || fallback
}

export async function api<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers = new Headers(options.headers)
  if (options.body) headers.set('Content-Type', 'application/json')
  const auth = token()
  if (auth) headers.set('Authorization', `Bearer ${auth}`)

  let response: Response
  try {
    response = await fetch(`${base}${path}`, { ...options, headers })
  } catch {
    throw new ApiError(0, 'Cannot reach the API. Start it with dotnet run, then try again.')
  }

  const isLogin = path.startsWith('/api/auth/')
  if (response.status === 401 && !isLogin) {
    localStorage.removeItem('ste.session')
    window.dispatchEvent(new Event('ste:unauthorized'))
  }

  if (response.status === 204) return undefined as T

  const text = await response.text()
  let data: unknown = null
  if (text) {
    try {
      data = JSON.parse(text) as unknown
    } catch {
      if (!response.ok) {
        throw new ApiError(response.status, 'The API returned an unexpected response. Make sure it is running on http://localhost:52205.')
      }
    }
  }
  if (!response.ok) throw new ApiError(response.status, errorMessage(data, response.statusText))
  return data as T
}

export const http = {
  get: <T>(path: string) => api<T>(path),
  post: <T>(path: string, body?: unknown) =>
    api<T>(path, { method: 'POST', body: body === undefined ? undefined : JSON.stringify(body) }),
  put: <T>(path: string, body?: unknown) =>
    api<T>(path, { method: 'PUT', body: body === undefined ? undefined : JSON.stringify(body) }),
  patch: <T>(path: string, body?: unknown) =>
    api<T>(path, { method: 'PATCH', body: body === undefined ? undefined : JSON.stringify(body) }),
  del: (path: string) => api<void>(path, { method: 'DELETE' }),
}
