import { NavLink, Outlet, useLocation } from 'react-router-dom'
import { useEffect, useState } from 'react'
import { workspaceApi } from '../api/resources'
import { useAuth } from '../auth/AuthContext'

const links = [
  { to: '/', label: 'Dashboard', end: true },
  { to: '/tasks', label: 'Tasks' },
  { to: '/projects', label: 'Projects' },
  { to: '/bookmarks', label: 'Bookmarks' },
  { to: '/labels', label: 'Labels' },
  { to: '/notifications', label: 'Notifications' },
  { to: '/people', label: 'People' },
]

export function Shell() {
  const { session, logout } = useAuth()
  const location = useLocation()
  const [unread, setUnread] = useState(0)

  useEffect(() => {
    let active = true
    workspaceApi.notifications(true)
      .then((items) => { if (active) setUnread(items.length) })
      .catch(() => { if (active) setUnread(0) })
    return () => { active = false }
  }, [location.pathname])

  return (
    <div className="shell">
      <aside className="rail">
        <div className="brand">
          <span className="mark">S</span>
          <div>
            <strong>Smart Task</strong>
            <small>Collaboration</small>
          </div>
        </div>
        <nav>
          {links.map((link) => (
            <NavLink key={link.to} to={link.to} end={link.end} className={({ isActive }) => isActive ? 'nav active' : 'nav'}>
              {link.label}
              {link.to === '/notifications' && unread > 0 && <em>{unread}</em>}
            </NavLink>
          ))}
        </nav>
        <div className="who">
          <div>
            <strong>{session?.username}</strong>
            <small>{session?.role}</small>
          </div>
          <button className="btn ghost" type="button" onClick={logout}>Log out</button>
        </div>
      </aside>
      <main className="stage">
        <Outlet />
      </main>
    </div>
  )
}
