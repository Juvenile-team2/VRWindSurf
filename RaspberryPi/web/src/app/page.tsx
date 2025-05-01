'use client'

export default function Home() {
  const handleClick = async (id: string) => {
    await fetch(`/api/send-command?id=${id}`, { method: 'POST' })
  }

  return (
    <main className="w-screen h-screen flex items-center justify-center bg-gradient-to-b from-blue-100 to-blue-200">
        <div className="grid grid-cols-7 grid-rows-7 w-full max-w-screen-md h-full max-h-screen-md gap-2 p-4">

          {/* 扇風機１：上中央 */}
          <div className="col-start-3 col-end-6 row-start-1 row-end-3 flex justify-center items-center">
            <button
              onClick={() => handleClick('1')}
              className="rounded-3xl backdrop-blur-md bg-white/50 hover:bg-white/60 p-4 shadow-md transition"
            >
              <img
                src="/electric_fan.png"
                alt="扇風機1"
                className="w-20 h-20 object-contain filter opacity-50"
              />
            </button>
          </div>

          {/* 扇風機２：左中央 */}
          <div className="col-start-1 col-end-4 row-start-5 row-end-7 flex justify-center items-center">
            <button
              onClick={() => handleClick('2')}
              className="rounded-3xl backdrop-blur-md bg-white/50 hover:bg-white/60 p-4 shadow-md transition"
            >
              <img
                src="/electric_fan.png"
                alt="扇風機2"
                className="w-20 h-20 object-contain filter opacity-50"
              />
            </button>
          </div>

          {/* 扇風機３：右中央 */}
          <div className="col-start-5 col-end-8 row-start-5 row-end-7 flex justify-center items-center">
            <button
              onClick={() => handleClick('3')}
              className="rounded-3xl backdrop-blur-md bg-white/50 hover:bg-white/60 p-4 shadow-md transition"
            >
              <img
                src="/electric_fan.png"
                alt="扇風機3"
                className="w-20 h-20 object-contain filter opacity-50"
              />
            </button>
          </div>

          {/* 中央のボード */}
          <div className="col-start-3 col-end-6 row-start-2 row-end-6 flex justify-center items-center">
            <div
              className="rounded-full bg-gradient-to-br from-gray-400 to-gray-600 text-white font-semibold shadow-lg flex justify-center items-center w-24 h-48">
              Board
            </div>
          </div>
        </div>
    </main>
  )
}
