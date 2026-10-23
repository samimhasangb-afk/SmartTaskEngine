import { useState, type FormEvent, type ReactNode } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { messageOf } from '../lib/format'

function AuthFrame({ children, title, text }: { children: ReactNode; title: string; text: string }) {
  return (
    <div className="auth">
      <section className="auth-story">
        <p className="eyebrow">Smart Task Engine</p>
        <h1>Work that stays in one place.</h1>
        <p>Projects, assignments, comments, checklists, and the trail of every change.</p>
      </section>
      <section className="auth-card">
        <h2>{title}</h2>
        <p className="muted">{text}</p>
        {children}
      </section>
    </div>
  )
}

export function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')

  async function submit(event: FormEvent) {
    event.preventDefault()
    setError('')
    try {
      await login(email, password)
      navigate('/')
    } catch (err) {
      setError(messageOf(err))
    }
  }

  return (
    <AuthFrame title="Welcome back" text="Use the email or username from your account.">
      <form className="stack" onSubmit={submit}>
        {error && <p className="banner">{error}</p>}
        <label>Email or username<input value={email} onChange={(e) => setEmail(e.target.value)} required autoComplete="username" /></label>
        <label>Password<input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required /></label>
        <button className="btn primary" type="submit">Log in</button>
        <p className="muted">New here? <Link to="/register">Create an account</Link></p>
      </form>
    </AuthFrame>
  )
}

export function RegisterPage() {
  const { register } = useAuth()
  const navigate = useNavigate()
  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')

  async function submit(event: FormEvent) {
    event.preventDefault()
    setError('')
    try {
      await register(username, email, password)
      navigate('/')
    } catch (err) {
      setError(messageOf(err))
    }
  }

  return (
    <AuthFrame title="Create your workspace" text="A member account is ready as soon as you register.">
      <form className="stack" onSubmit={submit}>
        {error && <p className="banner">{error}</p>}
        <label>Username<input value={username} onChange={(e) => setUsername(e.target.value)} required maxLength={100} /></label>
        <label>Email<input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required /></label>
        <label>Password<input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required minLength={6} /></label>
        <button className="btn primary" type="submit">Register</button>
        <p className="muted">Already registered? <Link to="/login">Log in</Link></p>
      </form>
    </AuthFrame>
  )
}
