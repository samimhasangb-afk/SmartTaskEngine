import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { workspaceApi } from '../api/resources'
import { Banner } from '../components/Modal'
import { messageOf } from '../lib/format'
import type { Dashboard } from '../types'

const cards: { key: keyof Dashboard; label: string; to: string }[] = [
  { key: 'totalTasks', label: 'All tasks', to: '/tasks' },
  { key: 'toDo', label: 'To do', to: '/tasks?status=ToDo' },
  { key: 'inProgress', label: 'In progress', to: '/tasks?status=InProgress' },
  { key: 'done', label: 'Done', to: '/tasks?status=Done' },
  { key: 'overdue', label: 'Overdue', to: '/tasks' },
  { key: 'assignedToMe', label: 'Assigned to me', to: '/tasks' },
  { key: 'projects', label: 'Projects', to: '/projects' },
  { key: 'openChecklistItems', label: 'Open checklist', to: '/tasks' },
  { key: 'unreadNotifications', label: 'Unread', to: '/notifications' },
  { key: 'bookmarks', label: 'Bookmarks', to: '/bookmarks' },
]

export function DashboardPage() {
  const [data, setData] = useState<Dashboard | null>(null)
  const [error, setError] = useState('')

  useEffect(() => {
    workspaceApi.dashboard().then(setData).catch((err) => setError(messageOf(err)))
  }, [])

  return (
    <section>
      <header className="page-head">
        <div>
          <p className="eyebrow">Overview</p>
          <h1>Today’s board</h1>
        </div>
        <Link className="btn primary" to="/tasks">Open tasks</Link>
      </header>
      <Banner text={error} />
      {!data && !error && <p className="muted">Loading the workspace…</p>}
      {data && (
        <div className="stats">
          {cards.map((card) => (
            <Link key={card.key} className="stat" to={card.to}>
              <strong>{data[card.key]}</strong>
              <span>{card.label}</span>
            </Link>
          ))}
        </div>
      )}
    </section>
  )
}
