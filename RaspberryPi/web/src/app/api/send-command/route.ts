// app/api/send-command/route.ts
import { NextResponse } from 'next/server'
import net from 'net'

// グローバルで接続を保持
let client: net.Socket | null = null

// 最初の接続を一度だけ確立
function ensureConnection() {
    return new Promise<void>((resolve, reject) => {
        if (client) return resolve();  // すでに接続済み

        client = new net.Socket()

        client.connect(12345, '0.0.0.0', () => {
            console.log('Connected to Python server')
            client?.write('REGISTER:ClientA\n')
            resolve()
        })

        client.on('error', (err) => {
            console.error('TCP Error:', err)
            reject(err)
        })

        client.on('close', () => {
            console.log('Connection closed')
            client = null  // 接続が切れた場合はリセット
        })
    })
}

export async function POST(request: Request) {
    const { searchParams } = new URL(request.url)
    const id = searchParams.get('id')

    if (!id) {
        return NextResponse.json({ status: 'error', message: 'ID is required' }, { status: 400 })
    }

    try {
        // 接続を確立
        await ensureConnection()

        return new Promise((resolve) => {
            if (!client) {
                return resolve(NextResponse.json({ status: 'error', message: 'No connection available' }))
            }

            // メッセージ送信
            client.write(`{${id}}\n`)

            client.once('data', (data) => {
                console.log('Received:', data.toString())
                resolve(NextResponse.json({ status: 'ok', response: data.toString() }))
            })

            client.once('error', (err) => {
                console.error('TCP Error:', err)
                resolve(NextResponse.json({ status: 'error', message: err.message }))
            })
        })
    } catch (err) {
        return NextResponse.json({ status: 'error', message: 'Failed to connect or send message' }, { status: 500 })
    }
}
