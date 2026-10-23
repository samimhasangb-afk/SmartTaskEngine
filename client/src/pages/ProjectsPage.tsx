import { useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { projectApi, taskApi, workspaceApi } from '../api/resources'
import { useAuth } from '../auth/AuthContext'
import { Banner } from '../components/Modal'
import { messageOf, when } from '../lib/format'
import type { Person, Project, TaskPriority } from '../types'

export function ProjectsPage() {
  const [projects, setProjects] = useState<Project[]>([])
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const [error, setError] = useState('')

  async function load() {
    try {
      setProjects(await projectApi.list())
      setError('')
    } catch (err) {
      setError(messageOf(err))
    }
  }

  useEffect(() => { void load() }, [])

  async function submit(event: FormEvent) {
    event.preventDefault()
    try {
      await projectApi.create({ name, description: description || undefined })
      setName('')
      setDescription('')
      await load()
    } catch (err) {
      setError(messageOf(err))
    }
  }

  return (
    <section>
      <header className="page-head">
        <div>
          <p className="eyebrow">Spaces</p>
          <h1>Projects</h1>
        </div>
      </header>
      <Banner text={error} />
      <div className="split">
        <form className="card stack" onSubmit={submit}>
          <h2>Start a project</h2>
          <label>Name<input value={name} onChange={(e) => setName(e.target.value)} required maxLength={150} /></label>
          <label>Description<textarea value={description} onChange={(e) => setDescription(e.target.value)} maxLength={2000} /></label>
          <button className="btn primary" type="submit">Create project</button>
        </form>
        <div className="stack">
          {projects.map((project) => (
            <Link key={project.id} to={`/projects/${project.id}`} className="card project-card">
              <strong>{project.name}</strong>
              <p>{project.description || 'No description'}</p>
              <small>{project.taskCount} tasks · {project.members.length} members · {project.createdByUsername}</small>
            </Link>
          ))}
          {projects.length === 0 && <p className="muted">No projects yet. Create the first one.</p>}
        </div>
      </div>
    </section>
  )
}

export function ProjectPage() {
  const { id } = useParams()
  const projectId = Number(id)
  const navigate = useNavigate()
  const { session } = useAuth()
  const [project, setProject] = useState<Project | null>(null)
  const [people, setPeople] = useState<Person[]>([])
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const [memberId, setMemberId] = useState('')
  const [taskTitle, setTaskTitle] = useState('')
  const [priority, setPriority] = useState<TaskPriority>('Medium')
  const [error, setError] = useState('')

  async function load() {
    try {
      const [nextProject, nextPeople] = await Promise.all([projectApi.get(projectId), workspaceApi.users()])
      setProject(nextProject)
      setPeople(nextPeople)
      setName(nextProject.name)
      setDescription(nextProject.description ?? '')
      setError('')
    } catch (err) {
      setError(messageOf(err))
    }
  }

  useEffect(() => { if (Number.isFinite(projectId)) void load() }, [projectId])

  if (!Number.isFinite(projectId)) return <p className="banner">That project link is not valid.</p>
  if (!project && !error) return <p className="muted">Loading project…</p>
  if (!project) return <Banner text={error} />

  const owner = project.createdByUserId === session?.id
  const candidates = people.filter((person) =>
    person.id !== project.createdByUserId && !project.members.some((member) => member.userId === person.id))

  return (
    <section>
      <header className="page-head">
        <div>
          <p className="eyebrow">Project</p>
          <h1>{project.name}</h1>
          <p className="muted">Opened by {project.createdByUsername} · {when(project.createdAt)}</p>
        </div>
        <Link className="btn" to={`/tasks?projectId=${project.id}`}>View tasks</Link>
      </header>
      <Banner text={error} />
      <div className="split">
        <form className="card stack" onSubmit={(event) => {
          event.preventDefault()
          void projectApi.update(project.id, { name, description }).then(load).catch((err) => setError(messageOf(err)))
        }}>
          <h2>Details</h2>
          <label>Name<input value={name} onChange={(e) => setName(e.target.value)} required disabled={!owner} /></label>
          <label>Description<textarea value={description} onChange={(e) => setDescription(e.target.value)} disabled={!owner} /></label>
          {owner && <button className="btn primary" type="submit">Save project</button>}
          {owner && <button className="btn danger" type="button" onClick={() => void projectApi.remove(project.id).then(() => navigate('/projects')).catch((err) => setError(messageOf(err)))}>Delete project</button>}
        </form>
        <div className="stack">
          <div className="card stack">
            <h2>Members</h2>
            {project.members.map((member) => (
              <div key={member.userId} className="person">
                <div>
                  <strong>{member.username}</strong>
                  <small>{member.email}</small>
                </div>
                {(owner || member.userId === session?.id) && (
                  <button className="btn ghost" type="button" onClick={() => void projectApi.removeMember(project.id, member.userId).then(load).catch((err) => setError(messageOf(err)))}>
                    {member.userId === session?.id ? 'Leave' : 'Remove'}
                  </button>
                )}
              </div>
            ))}
            {project.members.length === 0 && <p className="muted">No extra members yet. You still own this project.</p>}
            {owner && (
              <form className="row" onSubmit={(event) => {
                event.preventDefault()
                void projectApi.addMember(project.id, Number(memberId)).then(() => { setMemberId(''); return load() }).catch((err) => setError(messageOf(err)))
              }}>
                <select value={memberId} onChange={(e) => setMemberId(e.target.value)} required>
                  <option value="">Add a person</option>
                  {candidates.map((person) => <option key={person.id} value={person.id}>{person.username}</option>)}
                </select>
                <button className="btn" type="submit">Add</button>
              </form>
            )}
          </div>
          <form className="card stack" onSubmit={(event) => {
            event.preventDefault()
            void taskApi.create({ title: taskTitle, priority, projectId: project.id }).then(() => { setTaskTitle(''); return load() }).catch((err) => setError(messageOf(err)))
          }}>
            <h2>Add a task here</h2>
            <label>Title<input value={taskTitle} onChange={(e) => setTaskTitle(e.target.value)} required /></label>
            <label>Priority
              <select value={priority} onChange={(e) => setPriority(e.target.value as TaskPriority)}>
                <option>Low</option><option>Medium</option><option>High</option>
              </select>
            </label>
            <button className="btn" type="submit">Create in this project</button>
          </form>
        </div>
      </div>
    </section>
  )
}
