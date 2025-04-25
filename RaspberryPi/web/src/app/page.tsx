// app/page.tsx
'use client'

import { useState } from 'react'

export default function Home() {
  const [status, setStatus] = useState('')

  const handleClick = async () => {
    setStatus('送信中...')
    const res = await fetch('/api/send-command', { method: 'POST' })

    if (res.ok) {
      setStatus('送信成功')
    } else {
      setStatus('送信失敗')
    }
  }

  return (
      <main className="p-4">
        <button
            onClick={handleClick}
            className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
        >
          実行コマンド送信
        </button>
        <p className="mt-2">{status}</p>
      </main>
  )
}
