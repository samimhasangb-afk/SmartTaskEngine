import type { ReactNode } from 'react'

export function Modal({ title, children, onClose }: { title: string; children: ReactNode; onClose: () => void }) {
  return (
    <div className="modal-back" onClick={onClose} role="presentation">
      <div className="modal" role="dialog" aria-modal="true" aria-label={title} onClick={(event) => event.stopPropagation()}>
        <header>
          <h2>{title}</h2>
          <button className="btn ghost" type="button" onClick={onClose}>Close</button>
        </header>
        {children}
      </div>
    </div>
  )
}

export function Banner({ text }: { text: string }) {
  if (!text) return null
  return <p className="banner">{text}</p>
}
