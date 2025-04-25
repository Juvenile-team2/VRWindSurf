// app/api/send-command/route.ts
import { NextResponse } from 'next/server'
import net from 'net'

export async function POST() {
    return new Promise((resolve) => {
        const client = new net.Socket()

        client.connect(12345, '192.168.16.11', () => {
            console.log('Connected to Python server')
            client.write('REGISTER:ClientA\n')
            setTimeout(() => {
                client.write('{1}\n') // コマンド送信
            }, 500)
        })

        client.on('data', (data) => {
            console.log('Received:', data.toString())
            client.destroy() // 接続切断
            resolve(NextResponse.json({ status: 'ok', response: data.toString() }))
        })

        client.on('error', (err) => {
            console.error('TCP Error:', err)
            resolve(NextResponse.json({ status: 'error', message: err.message }))
        })

        client.on('close', () => {
            console.log('Connection closed')
        })
    })
}
