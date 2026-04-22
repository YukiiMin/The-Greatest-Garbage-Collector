import {
  Award,
  FileText,
  Leaf,
  LogOut,
  MessageSquare,
  User,
} from 'lucide-react'
import { useEffect, useState } from 'react'
import { useLocation, useNavigate, useParams } from 'react-router-dom'
import Report from '../components/Report/Report'
import ReportID from '../components/Report/ReportID'

const CitizenPage = () => {
  const navigate = useNavigate()

  const { reportId } = useParams()
  const isDetailPage = !!reportId

  useEffect(() => {
    if (isDetailPage) {
      navigate(`/citizen/report/${reportId}`)
    }
  }, [isDetailPage, navigate, reportId])

  const [openProfile, setOpenProfile] = useState(false)
  const location = useLocation()

  const user = {
    name: 'Nguyen Van A',
    avatar: 'A',
  }

  const menu = [
    { id: 'report', label: 'Báo Cáo', icon: FileText },
    { id: 'complaint', label: 'Phản Ánh', icon: MessageSquare },
    { id: 'reward', label: 'Điểm', icon: Award },
    { id: 'profile', label: 'Hồ Sơ', icon: User },
  ]

  return (
    <div className="flex min-h-screen bg-gray-100">
      <aside className="hidden lg:flex selection:w-64  border-r flex-col bg-green-950">
        <div className="flex items-center gap-2 p-4 border-b">
          <Leaf className="text-green-600" />
          <span className="font-bold text-lg text-white">
            Garbage Collection
          </span>
        </div>

        <div className="flex-1 p-3 space-y-2">
          {menu.map((item) => {
            const Icon = item.icon
            const isActive = location.pathname.includes(`/citizen/${item.id}`)

            return (
              <button
                key={item.id}
                onClick={() => {
                  setOpenProfile(false)
                  navigate(`/citizen/${item.id}`)
                }}
                className={`w-full flex items-center gap-3 p-2 rounded-lg transition ${
                  isActive
                    ? 'bg-green-200 text-green-800'
                    : 'hover:bg-gray-100 text-white hover:text-green-800'
                }`}
              >
                <Icon size={18} />
                {item.label}
              </button>
            )
          })}
        </div>

        <div className="relative border-t p-3">
          <button
            className="flex items-center gap-3 w-full"
            onClick={() => setOpenProfile(!openProfile)}
          >
            <div className="w-9 h-9 rounded-full bg-green-600 text-white flex items-center justify-center font-bold">
              {user.avatar}
            </div>
            <span className="text-l font-medium text-green-200">
              {user.name}
            </span>
          </button>

          {openProfile && (
            <div className="absolute bottom-14 left-3 w-[90%] bg-white border rounded-lg shadow-lg overflow-hidden">
              <button
                className="w-full flex items-center gap-2 p-2 hover:bg-gray-300"
                onClick={() => {
                  setOpenProfile(!openProfile)
                  navigate('/citizen/profile')
                }}
              >
                <User size={16} />
                Hồ sơ
              </button>
              <button className="w-full flex items-center gap-2 p-2 hover:bg-gray-300 text-red-500">
                <LogOut size={16} />
                Đăng xuất
              </button>
            </div>
          )}
        </div>
      </aside>

      <nav className="fixed bottom-0 left-0 right-0 bg-green-950 border-t flex justify-around py-2 lg:hidden">
        {menu.map((item) => {
          const Icon = item.icon
          const isActive = location.pathname.includes(`/citizen/${item.id}`)

          return (
            <button
              key={item.id}
              onClick={() => navigate(`/citizen/${item.id}`)}
              className={`flex flex-col items-center text-xs ${
                isActive ? 'text-green-400' : 'text-white'
              }`}
            >
              <Icon size={20} />
              {item.label}
            </button>
          )
        })}
      </nav>

      <main className="flex-1 p-6">
        {location.pathname.includes('/citizen/report') && !isDetailPage && (
          <Report />
        )}
        {location.pathname.includes('/citizen/report') && isDetailPage && (
          <ReportID />
        )}
      </main>
    </div>
  )
}

export default CitizenPage
