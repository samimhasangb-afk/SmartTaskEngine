import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { authApi } from '../api/resources'
import type { Session } from '../types'

const KEY = 'ste.session'

type AuthValue = {
  session: Session | null
  login: (email: string, password: string) => Promise<void>
  register: (username: string, email: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthValue | null>(null)

function readSession(): Session | null {
  const raw = localStorage.getItem(KEY)
  if (!raw) return null
  try {
    return JSON.parse(raw) as Session
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session | null>(readSession)

  function save(next: Session | null) {
    setSession(next)
    if (next) localStorage.setItem(KEY, JSON.stringify(next))
    else localStorage.removeItem(KEY)
  }

  useEffect(() => {
    const onUnauthorized = () => setSession(null)
    window.addEventListener('ste:unauthorized', onUnauthorized)
    return () => window.removeEventListener('ste:unauthorized', onUnauthorized)
  }, [])

  const value = useMemo<AuthValue>(() => ({
    session,
    async login(email, password) {
      save(await authApi.login({ email: email.trim(), password }))
    },
    async register(username, email, password) {
      save(await authApi.register({ username, email, password }))
    },
    logout() {
      save(null)
    },
  }), [session])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const value = useContext(AuthContext)
  if (!value) throw new Error('useAuth must be used inside AuthProvider')
  return value
}
