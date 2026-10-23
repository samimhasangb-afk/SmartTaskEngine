import type {
  ActivityLog,
  ChecklistItem,
  Comment,
  Dashboard,
  Label,
  Notification,
  Person,
  Project,
  Session,
  Task,
  TaskQuery,
} from '../types'
import { http } from './http'

function query(params: TaskQuery) {
  const search = new URLSearchParams()
  if (params.status) search.set('status', params.status)
  if (params.priority) search.set('priority', params.priority)
  if (params.search) search.set('search', params.search)
  if (params.projectId) search.set('projectId', String(params.projectId))
  const text = search.toString()
  return text ? `?${text}` : ''
}

export const authApi = {
  register: (body: { username: string; email: string; password: string }) =>
    http.post<Session>('/api/auth/register', body),
  login: (body: { email: string; password: string }) =>
    http.post<Session>('/api/auth/login', body),
}

export const taskApi = {
  list: (params: TaskQuery = {}) => http.get<Task[]>(`/api/tasks${query(params)}`),
  get: (id: number) => http.get<Task>(`/api/tasks/${id}`),
  create: (body: {
    title: string
    description?: string
    priority: string
    dueDate?: string
    assignedToUserId?: number
    projectId?: number
  }) => http.post<Task>('/api/tasks', body),
  update: (id: number, body: {
    title?: string
    description?: string
    status?: string
    priority?: string
    dueDate?: string
  }) => http.put<Task>(`/api/tasks/${id}`, body),
  move: (id: number, projectId: number | null) =>
    http.patch<Task>(`/api/tasks/${id}/project`, { projectId }),
  remove: (id: number) => http.del(`/api/tasks/${id}`),
  assign: (id: number, assignedToUserId: number) =>
    http.patch<Task>(`/api/tasks/${id}/assign`, { assignedToUserId }),
  logs: (id: number) => http.get<ActivityLog[]>(`/api/tasks/${id}/logs`),
}

export const projectApi = {
  list: () => http.get<Project[]>('/api/projects'),
  get: (id: number) => http.get<Project>(`/api/projects/${id}`),
  create: (body: { name: string; description?: string }) =>
    http.post<Project>('/api/projects', body),
  update: (id: number, body: { name: string; description?: string }) =>
    http.put<Project>(`/api/projects/${id}`, body),
  remove: (id: number) => http.del(`/api/projects/${id}`),
  addMember: (id: number, userId: number) =>
    http.post<Project>(`/api/projects/${id}/members`, { userId }),
  removeMember: (id: number, userId: number) =>
    http.del(`/api/projects/${id}/members/${userId}`),
}

export const collabApi = {
  comments: (taskId: number) => http.get<Comment[]>(`/api/tasks/${taskId}/comments`),
  addComment: (taskId: number, body: string) =>
    http.post<Comment>(`/api/tasks/${taskId}/comments`, { body }),
  deleteComment: (taskId: number, commentId: number) =>
    http.del(`/api/tasks/${taskId}/comments/${commentId}`),
  labels: () => http.get<Label[]>('/api/labels'),
  createLabel: (body: { name: string; color?: string }) =>
    http.post<Label>('/api/labels', body),
  deleteLabel: (id: number) => http.del(`/api/labels/${id}`),
  attachLabel: (taskId: number, labelId: number) =>
    http.post<Task>(`/api/tasks/${taskId}/labels/${labelId}`),
  detachLabel: (taskId: number, labelId: number) =>
    http.del(`/api/tasks/${taskId}/labels/${labelId}`),
  checklist: (taskId: number) => http.get<ChecklistItem[]>(`/api/tasks/${taskId}/checklist`),
  addChecklist: (taskId: number, title: string) =>
    http.post<ChecklistItem>(`/api/tasks/${taskId}/checklist`, { title }),
  updateChecklist: (taskId: number, itemId: number, body: { title?: string; isDone?: boolean }) =>
    http.patch<ChecklistItem>(`/api/tasks/${taskId}/checklist/${itemId}`, body),
  deleteChecklist: (taskId: number, itemId: number) =>
    http.del(`/api/tasks/${taskId}/checklist/${itemId}`),
  bookmark: (taskId: number) => http.post<void>(`/api/tasks/${taskId}/bookmark`),
  unbookmark: (taskId: number) => http.del(`/api/tasks/${taskId}/bookmark`),
  bookmarks: () => http.get<Task[]>('/api/bookmarks'),
}

export const workspaceApi = {
  notifications: (unreadOnly?: boolean) =>
    http.get<Notification[]>(`/api/notifications${unreadOnly ? '?unreadOnly=true' : ''}`),
  markRead: (id: number) => http.patch<void>(`/api/notifications/${id}/read`),
  markAllRead: () => http.post<{ marked: number }>('/api/notifications/read-all'),
  dashboard: () => http.get<Dashboard>('/api/dashboard'),
  users: () => http.get<Person[]>('/api/users'),
}
