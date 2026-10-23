import { useEffect, useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { collabApi } from '../api/resources'
import { Banner } from '../components/Modal'
import { messageOf, when } from '../lib/format'
import type { Label, Task } from '../types'

export function LabelsPage() {
  const [labels, setLabels] = useState<Label[]>([])
  const [name, setName] = useState('')
  const [color, setColor] = useState('#c4622d')
  const [error, setError] = useState('')

  async function load() {
    try {
      setLabels(await collabApi.labels())
      setError('')
    } catch (err) {
      setError(messageOf(err))
    }
  }

  useEffect(() => { void load() }, [])

  async function submit(event: FormEvent) {
    event.preventDefault()
    try {
      await collabApi.createLabel({ name, color })
      setName('')
      await load()
    } catch (err) {
      setError(messageOf(err))
    }
  }

  return (
    <section>
      <header className="page-head">
        <div>
          <p className="eyebrow">Organize</p>
          <h1>Labels</h1>
        </div>
      </header>
      <Banner text={error} />
      <div className="split">
        <form className="card stack" onSubmit={submit}>
          <h2>New label</h2>
          <label>Name<input value={name} onChange={(e) => setName(e.target.value)} required maxLength={50} /></label>
          <label>Color<input type="color" value={color} onChange={(e) => setColor(e.target.value)} /></label>
          <button className="btn primary" type="submit">Create label</button>
        </form>
        <div className="stack">
          {labels.map((label) => (
            <div key={label.id} className="card person">
              <div>
                <strong style={{ color: label.color }}>{label.name}</strong>
                <small>{label.color}</small>
              </div>
              <button className="btn ghost" type="button" onClick={() => void collabApi.deleteLabel(label.id).then(load).catch((err) => setError(messageOf(err)))}>Delete</button>
            </div>
          ))}
          {labels.length === 0 && <p className="muted">Labels you create can be attached from any task.</p>}
        </div>
      </div>
    </section>
  )
}

export function BookmarksPage() {
  const [tasks, setTasks] = useState<Task[]>([])
  const [error, setError] = useState('')

  async function load() {
    try {
      setTasks(await collabApi.bookmarks())
      setError('')
    } catch (err) {
      setError(messageOf(err))
    }
  }

  useEffect(() => { void load() }, [])

  return (
    <section>
      <header className="page-head">
        <div>
          <p className="eyebrow">Saved</p>
          <h1>Bookmarks</h1>
        </div>
      </header>
      <Banner text={error} />
      <div className="stack">
        {tasks.map((task) => (
          <article key={task.id} className="card person">
            <div>
              <Link to={`/tasks/${task.id}`}><strong>{task.title}</strong></Link>
              <small>{task.status} · {task.priority} · {when(task.dueDate)}</small>
            </div>
            <button className="btn ghost" type="button" onClick={() => void collabApi.unbookmark(task.id).then(load).catch((err) => setError(messageOf(err)))}>Remove</button>
          </article>
        ))}
        {tasks.length === 0 && <p className="muted">Bookmark a task from its page and it will show up here.</p>}
      </div>
    </section>
  )
}
