import type { ReactNode } from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'
import { useAuth } from './auth/AuthContext'
import { Shell } from './components/Shell'
import { DashboardPage } from './pages/DashboardPage'
import { LoginPage, RegisterPage } from './pages/AuthPages'
import { NotificationsPage, PeoplePage } from './pages/InboxPages'
import { BookmarksPage, LabelsPage } from './pages/LibraryPages'
import { ProjectPage, ProjectsPage } from './pages/ProjectsPage'
import { TaskPage, TasksPage } from './pages/TasksPage'

function Private() {
  const { session } = useAuth()
  if (!session) return <Navigate to="/login" replace />
  return <Shell />
}

function Guest({ children }: { children: ReactNode }) {
  const { session } = useAuth()
  if (session) return <Navigate to="/" replace />
  return children
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<Guest><LoginPage /></Guest>} />
      <Route path="/register" element={<Guest><RegisterPage /></Guest>} />
      <Route element={<Private />}>
        <Route index element={<DashboardPage />} />
        <Route path="tasks" element={<TasksPage />} />
        <Route path="tasks/:id" element={<TaskPage />} />
        <Route path="projects" element={<ProjectsPage />} />
        <Route path="projects/:id" element={<ProjectPage />} />
        <Route path="bookmarks" element={<BookmarksPage />} />
        <Route path="labels" element={<LabelsPage />} />
        <Route path="notifications" element={<NotificationsPage />} />
        <Route path="people" element={<PeoplePage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
