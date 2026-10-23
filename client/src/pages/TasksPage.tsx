import { useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { collabApi, projectApi, taskApi, workspaceApi } from '../api/resources'
import { useAuth } from '../auth/AuthContext'
import { Banner, Modal } from '../components/Modal'
import { fromLocal, localInput, messageOf, when } from '../lib/format'
import type { ActivityLog, ChecklistItem, Comment, Label, Person, Project, Task, TaskPriority, TaskState } from '../types'

const states: TaskState[] = ['ToDo', 'InProgress', 'Done']
const priorities: TaskPriority[] = ['Low', 'Medium', 'High']

export function TasksPage() {
  const [params, setParams] = useSearchParams()
  const [tasks, setTasks] = useState<Task[]>([])
  const [projects, setProjects] = useState<Project[]>([])
  const [people, setPeople] = useState<Person[]>([])
  const [error, setError] = useState('')
  const [open, setOpen] = useState(false)
  const status = (params.get('status') as TaskState | null) || ''
  const priority = (params.get('priority') as TaskPriority | null) || ''
  const search = params.get('search') ?? ''
  const projectId = params.get('projectId') ?? ''

  async function load() {
    setError('')
    try {
      const [nextTasks, nextProjects, nextPeople] = await Promise.all([
        taskApi.list({
          status: status || undefined,
          priority: priority || undefined,
          search: search || undefined,
          projectId: projectId ? Number(projectId) : undefined,
        }),
        projectApi.list(),
        workspaceApi.users(),
      ])
      setTasks(nextTasks)
      setProjects(nextProjects)
      setPeople(nextPeople)
    } catch (err) {
      setError(messageOf(err))
    }
  }

  useEffect(() => { void load() }, [status, priority, search, projectId])

  function setFilter(key: string, value: string) {
    const next = new URLSearchParams(params)
    if (value) next.set(key, value)
    else next.delete(key)
    setParams(next)
  }

  return (
    <section>
      <header className="page-head">
        <div>
          <p className="eyebrow">Work</p>
          <h1>Tasks</h1>
        </div>
        <button className="btn primary" type="button" onClick={() => setOpen(true)}>New task</button>
      </header>
      <Banner text={error} />
      <div className="filters">
        <input placeholder="Search title or description" value={search} onChange={(e) => setFilter('search', e.target.value)} />
        <select value={status} onChange={(e) => setFilter('status', e.target.value)}>
          <option value="">Any status</option>
          {states.map((item) => <option key={item}>{item}</option>)}
        </select>
        <select value={priority} onChange={(e) => setFilter('priority', e.target.value)}>
          <option value="">Any priority</option>
          {priorities.map((item) => <option key={item}>{item}</option>)}
        </select>
        <select value={projectId} onChange={(e) => setFilter('projectId', e.target.value)}>
          <option value="">Any project</option>
          {projects.map((project) => <option key={project.id} value={project.id}>{project.name}</option>)}
        </select>
      </div>
      <div className="board">
        {states.filter((item) => !status || item === status).map((column) => (
          <div key={column} className="column">
            <header><span className={`dot ${column}`} />{column}<em>{tasks.filter((task) => task.status === column).length}</em></header>
            {tasks.filter((task) => task.status === column).map((task) => (
              <Link key={task.id} to={`/tasks/${task.id}`} className="task-card">
                <strong>{task.title}</strong>
                <small>{task.projectName || 'No project'} · {task.priority}</small>
                <div className="meta">
                  <span>{task.assignedToUsername || 'Unassigned'}</span>
                  <span>{task.checklistDone}/{task.checklistTotal}</span>
                  {task.isBookmarked && <span>Saved</span>}
                </div>
              </Link>
            ))}
          </div>
        ))}
      </div>
      {open && (
        <CreateTask
          projects={projects}
          people={people}
          defaultProjectId={projectId}
          onClose={() => setOpen(false)}
          onCreated={() => { setOpen(false); void load() }}
        />
      )}
    </section>
  )
}

function CreateTask({ projects, people, defaultProjectId, onClose, onCreated }: {
  projects: Project[]
  people: Person[]
  defaultProjectId: string
  onClose: () => void
  onCreated: () => void
}) {
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [priority, setPriority] = useState<TaskPriority>('Medium')
  const [dueDate, setDueDate] = useState('')
  const [projectId, setProjectId] = useState(defaultProjectId)
  const [assignee, setAssignee] = useState('')
  const [error, setError] = useState('')

  async function submit(event: FormEvent) {
    event.preventDefault()
    setError('')
    try {
      await taskApi.create({
        title,
        description: description || undefined,
        priority,
        dueDate: fromLocal(dueDate),
        projectId: projectId ? Number(projectId) : undefined,
        assignedToUserId: assignee ? Number(assignee) : undefined,
      })
      onCreated()
    } catch (err) {
      setError(messageOf(err))
    }
  }

  return (
    <Modal title="New task" onClose={onClose}>
      <form className="stack" onSubmit={submit}>
        <Banner text={error} />
        <label>Title<input value={title} onChange={(e) => setTitle(e.target.value)} required maxLength={200} /></label>
        <label>Description<textarea value={description} onChange={(e) => setDescription(e.target.value)} maxLength={2000} /></label>
        <label>Priority
          <select value={priority} onChange={(e) => setPriority(e.target.value as TaskPriority)}>
            {priorities.map((item) => <option key={item}>{item}</option>)}
          </select>
        </label>
        <label>Due<input type="datetime-local" value={dueDate} onChange={(e) => setDueDate(e.target.value)} /></label>
        <label>Project
          <select value={projectId} onChange={(e) => setProjectId(e.target.value)}>
            <option value="">None</option>
            {projects.map((project) => <option key={project.id} value={project.id}>{project.name}</option>)}
          </select>
        </label>
        <label>Assign
          <select value={assignee} onChange={(e) => setAssignee(e.target.value)}>
            <option value="">Unassigned</option>
            {people.map((person) => <option key={person.id} value={person.id}>{person.username}</option>)}
          </select>
        </label>
        <button className="btn primary" type="submit">Create task</button>
      </form>
    </Modal>
  )
}

export function TaskPage() {
  const { id } = useParams()
  const taskId = Number(id)
  const navigate = useNavigate()
  const { session } = useAuth()
  const [task, setTask] = useState<Task | null>(null)
  const [comments, setComments] = useState<Comment[]>([])
  const [checklist, setChecklist] = useState<ChecklistItem[]>([])
  const [logs, setLogs] = useState<ActivityLog[]>([])
  const [labels, setLabels] = useState<Label[]>([])
  const [projects, setProjects] = useState<Project[]>([])
  const [people, setPeople] = useState<Person[]>([])
  const [error, setError] = useState('')
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [status, setStatus] = useState<TaskState>('ToDo')
  const [priority, setPriority] = useState<TaskPriority>('Medium')
  const [dueDate, setDueDate] = useState('')
  const [comment, setComment] = useState('')
  const [checkTitle, setCheckTitle] = useState('')
  const [labelId, setLabelId] = useState('')

  async function load() {
    setError('')
    try {
      const [nextTask, nextComments, nextChecklist, nextLogs, nextLabels, nextProjects, nextPeople] = await Promise.all([
        taskApi.get(taskId),
        collabApi.comments(taskId),
        collabApi.checklist(taskId),
        taskApi.logs(taskId),
        collabApi.labels(),
        projectApi.list(),
        workspaceApi.users(),
      ])
      setTask(nextTask)
      setComments(nextComments)
      setChecklist(nextChecklist)
      setLogs(nextLogs)
      setLabels(nextLabels)
      setProjects(nextProjects)
      setPeople(nextPeople)
      setTitle(nextTask.title)
      setDescription(nextTask.description ?? '')
      setStatus(nextTask.status)
      setPriority(nextTask.priority)
      setDueDate(localInput(nextTask.dueDate))
    } catch (err) {
      setError(messageOf(err))
    }
  }

  useEffect(() => { if (Number.isFinite(taskId)) void load() }, [taskId])

  async function run(action: () => Promise<unknown>) {
    setError('')
    try {
      await action()
      await load()
    } catch (err) {
      setError(messageOf(err))
    }
  }

  if (!Number.isFinite(taskId)) return <p className="banner">That task link is not valid.</p>
  if (!task && !error) return <p className="muted">Loading task…</p>
  if (!task) return <Banner text={error} />

  const unusedLabels = labels.filter((label) => !task.labels.some((item) => item.id === label.id))

  return (
    <section className="task-page">
      <header className="page-head">
        <div>
          <p className="eyebrow">Task #{task.id}</p>
          <h1>{task.title}</h1>
          <p className="muted">Created by {task.createdByUsername} · {when(task.createdAt)}</p>
        </div>
        <div className="row">
          <button className="btn ghost" type="button" onClick={() => void run(() => task.isBookmarked ? collabApi.unbookmark(task.id) : collabApi.bookmark(task.id))}>
            {task.isBookmarked ? 'Remove bookmark' : 'Bookmark'}
          </button>
          <button className="btn danger" type="button" onClick={() => void taskApi.remove(task.id).then(() => navigate('/tasks')).catch((err) => setError(messageOf(err)))}>Delete</button>
        </div>
      </header>
      <Banner text={error} />
      <div className="split">
        <form className="card stack" onSubmit={(event) => { event.preventDefault(); void run(() => taskApi.update(task.id, { title, description, status, priority, dueDate: fromLocal(dueDate) })) }}>
          <label>Title<input value={title} onChange={(e) => setTitle(e.target.value)} required /></label>
          <label>Description<textarea value={description} onChange={(e) => setDescription(e.target.value)} /></label>
          <div className="row">
            <label>Status
              <select value={status} onChange={(e) => setStatus(e.target.value as TaskState)}>
                {states.map((item) => <option key={item}>{item}</option>)}
              </select>
            </label>
            <label>Priority
              <select value={priority} onChange={(e) => setPriority(e.target.value as TaskPriority)}>
                {priorities.map((item) => <option key={item}>{item}</option>)}
              </select>
            </label>
          </div>
          <label>Due<input type="datetime-local" value={dueDate} onChange={(e) => setDueDate(e.target.value)} /></label>
          <button className="btn primary" type="submit">Save changes</button>
        </form>
        <div className="stack">
          <div className="card stack">
            <h2>Assignment</h2>
            <p className="muted">Currently {task.assignedToUsername || 'unassigned'}.</p>
            <select defaultValue="" onChange={(e) => { if (e.target.value) void run(() => taskApi.assign(task.id, Number(e.target.value))) }}>
              <option value="">Choose a person</option>
              {people.map((person) => <option key={person.id} value={person.id}>{person.username}</option>)}
            </select>
          </div>
          <div className="card stack">
            <h2>Project</h2>
            <p className="muted">{task.projectName || 'Not in a project yet.'}</p>
            <select value={task.projectId ?? ''} onChange={(e) => void run(() => taskApi.move(task.id, e.target.value ? Number(e.target.value) : null))}>
              <option value="">No project</option>
              {projects.map((project) => <option key={project.id} value={project.id}>{project.name}</option>)}
            </select>
          </div>
          <div className="card stack">
            <h2>Labels</h2>
            <div className="chips">
              {task.labels.map((label) => (
                <button key={label.id} type="button" className="chip" style={{ borderColor: label.color }} onClick={() => void run(() => collabApi.detachLabel(task.id, label.id))}>
                  {label.name} ×
                </button>
              ))}
              {task.labels.length === 0 && <p className="muted">No labels yet.</p>}
            </div>
            <div className="row">
              <select value={labelId} onChange={(e) => setLabelId(e.target.value)}>
                <option value="">Attach a label</option>
                {unusedLabels.map((label) => <option key={label.id} value={label.id}>{label.name}</option>)}
              </select>
              <button className="btn" type="button" disabled={!labelId} onClick={() => void run(async () => { await collabApi.attachLabel(task.id, Number(labelId)); setLabelId('') })}>Attach</button>
            </div>
          </div>
        </div>
      </div>
      <div className="split">
        <div className="card stack">
          <h2>Checklist {task.checklistDone}/{task.checklistTotal}</h2>
          {checklist.map((item) => (
            <label key={item.id} className="check">
              <input type="checkbox" checked={item.isDone} onChange={() => void run(() => collabApi.updateChecklist(task.id, item.id, { isDone: !item.isDone }))} />
              <span>{item.title}</span>
              <button type="button" className="btn ghost" onClick={() => void run(() => collabApi.deleteChecklist(task.id, item.id))}>Remove</button>
            </label>
          ))}
          <form className="row" onSubmit={(event) => { event.preventDefault(); void run(async () => { await collabApi.addChecklist(task.id, checkTitle); setCheckTitle('') }) }}>
            <input value={checkTitle} onChange={(e) => setCheckTitle(e.target.value)} placeholder="Add a step" required maxLength={200} />
            <button className="btn" type="submit">Add</button>
          </form>
        </div>
        <div className="card stack">
          <h2>Comments</h2>
          {comments.map((item) => (
            <article key={item.id} className="note">
              <header>
                <strong>{item.username}</strong>
                <small>{when(item.createdAt)}</small>
                {item.userId === session?.id && <button type="button" className="btn ghost" onClick={() => void run(() => collabApi.deleteComment(task.id, item.id))}>Delete</button>}
              </header>
              <p>{item.body}</p>
            </article>
          ))}
          <form className="stack" onSubmit={(event) => { event.preventDefault(); void run(async () => { await collabApi.addComment(task.id, comment); setComment('') }) }}>
            <textarea value={comment} onChange={(e) => setComment(e.target.value)} placeholder="Write a comment" required maxLength={2000} />
            <button className="btn" type="submit">Comment</button>
          </form>
        </div>
      </div>
      <div className="card">
        <h2>Activity</h2>
        <ul className="log">
          {logs.map((item) => (
            <li key={item.id}><strong>{item.username}</strong> {item.action} <small>{when(item.timestamp)}</small></li>
          ))}
          {logs.length === 0 && <li className="muted">No history yet.</li>}
        </ul>
      </div>
    </section>
  )
}
