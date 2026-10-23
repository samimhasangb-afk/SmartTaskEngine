export type TaskState = 'ToDo' | 'InProgress' | 'Done'
export type TaskPriority = 'Low' | 'Medium' | 'High'
export type NotificationType = 'Assigned' | 'CommentAdded' | 'StatusChanged' | 'ProjectInvite' | 'ChecklistDone'

export type Session = {
  id: number
  username: string
  email: string
  role: string
  token: string
  expiresAt: string
}

export type Label = {
  id: number
  name: string
  color: string
}

export type Task = {
  id: number
  title: string
  description: string | null
  status: TaskState
  priority: TaskPriority
  dueDate: string | null
  createdAt: string
  createdByUserId: number
  createdByUsername: string | null
  assignedToUserId: number | null
  assignedToUsername: string | null
  projectId: number | null
  projectName: string | null
  isBookmarked: boolean
  commentCount: number
  checklistTotal: number
  checklistDone: number
  labels: Label[]
}

export type ActivityLog = {
  id: number
  taskId: number
  userId: number
  username: string | null
  action: string
  timestamp: string
}

export type ProjectMember = {
  userId: number
  username: string
  email: string
}

export type Project = {
  id: number
  name: string
  description: string | null
  createdAt: string
  createdByUserId: number
  createdByUsername: string | null
  taskCount: number
  members: ProjectMember[]
}

export type Comment = {
  id: number
  taskId: number
  userId: number
  username: string | null
  body: string
  createdAt: string
}

export type ChecklistItem = {
  id: number
  taskId: number
  title: string
  isDone: boolean
  sortOrder: number
  createdAt: string
}

export type Notification = {
  id: number
  type: NotificationType
  message: string
  isRead: boolean
  createdAt: string
  actorUserId: number | null
  actorUsername: string | null
  taskId: number | null
  projectId: number | null
}

export type Dashboard = {
  totalTasks: number
  toDo: number
  inProgress: number
  done: number
  overdue: number
  assignedToMe: number
  projects: number
  openChecklistItems: number
  unreadNotifications: number
  bookmarks: number
}

export type Person = {
  id: number
  username: string
  email: string
  role: string
}

export type TaskQuery = {
  status?: TaskState
  priority?: TaskPriority
  search?: string
  projectId?: number
}
