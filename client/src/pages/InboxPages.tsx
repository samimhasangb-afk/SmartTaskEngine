import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { workspaceApi } from '../api/resources'
import { Banner } from '../components/Modal'
import { messageOf, when } from '../lib/format'
import type { Notification, Person } from '../types'

export function NotificationsPage() {
  const [items, setItems] = useState<Notification[]>([])
  const [unreadOnly, setUnreadOnly] = useState(false)
  const [error, setError] = useState('')

  async function load(only = unreadOnly) {
    try {
      setItems(await workspaceApi.notifications(only))
      setError('')
    } catch (err) {
      setError(messageOf(err))
    }
  }

  useEffect(() => { void load(unreadOnly) }, [unreadOnly])

  return (
    <section>
      <header className="page-head">
        <div>
          <p className="eyebrow">Inbox</p>
          <h1>Notifications</h1>
        </div>
        <div className="row">
          <button className="btn ghost" type="button" onClick={() => setUnreadOnly((value) => !value)}>{unreadOnly ? 'Show all' : 'Unread only'}</button>
          <button className="btn" type="button" onClick={() => void workspaceApi.markAllRead().then(() => load()).catch((err) => setError(messageOf(err)))}>Mark all read</button>
        </div>
      </header>
      <Banner text={error} />
      <div className="stack">
        {items.map((item) => (
          <article key={item.id} className={item.isRead ? 'card note' : 'card note unread'}>
            <header>
              <strong>{item.type}</strong>
              <small>{when(item.createdAt)}</small>
            </header>
            <p>{item.message}</p>
            <div className="row">
              {item.taskId && <Link to={`/tasks/${item.taskId}`}>Open task</Link>}
              {item.projectId && <Link to={`/projects/${item.projectId}`}>Open project</Link>}
              {!item.isRead && <button className="btn ghost" type="button" onClick={() => void workspaceApi.markRead(item.id).then(() => load()).catch((err) => setError(messageOf(err)))}>Mark read</button>}
            </div>
          </article>
        ))}
        {items.length === 0 && <p className="muted">Nothing in the inbox.</p>}
      </div>
    </section>
  )
}

export function PeoplePage() {
  const [people, setPeople] = useState<Person[]>([])
  const [error, setError] = useState('')

  useEffect(() => {
    workspaceApi.users().then(setPeople).catch((err) => setError(messageOf(err)))
  }, [])

  return (
    <section>
      <header className="page-head">
        <div>
          <p className="eyebrow">Directory</p>
          <h1>People</h1>
        </div>
      </header>
      <Banner text={error} />
      <div className="table">
        {people.map((person) => (
          <div key={person.id} className="person">
            <div>
              <strong>{person.username}</strong>
              <small>{person.email}</small>
            </div>
            <span className="pill">{person.role}</span>
          </div>
        ))}
      </div>
    </section>
  )
}
