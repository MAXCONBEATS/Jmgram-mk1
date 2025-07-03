import { useEffect, useRef } from "react"

const ChatAutoRefresh = ({ onRefresh, isEnabled = true, interval = 3000 }) => {
  const intervalRef = useRef(null)
  const lastRefreshRef = useRef(Date.now())

  useEffect(() => {
    if (!isEnabled || !onRefresh) return

    const startAutoRefresh = () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current)
      }

      intervalRef.current = setInterval(() => {
        const now = Date.now()
        if (now - lastRefreshRef.current >= interval) {
          console.log("ChatAutoRefresh: автоматическое обновление")
          onRefresh()
          lastRefreshRef.current = now
        }
      }, interval)
    }

    startAutoRefresh()

    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current)
      }
    }
  }, [isEnabled, onRefresh, interval])

  // Обновляем при фокусе на окне
  useEffect(() => {
    const handleFocus = () => {
      console.log("ChatAutoRefresh: окно получило фокус, обновляем")
      if (onRefresh) {
        onRefresh()
      }
    }

    window.addEventListener("focus", handleFocus)
    return () => window.removeEventListener("focus", handleFocus)
  }, [onRefresh])

  return null // Этот компонент не рендерит ничего видимого
}

export default ChatAutoRefresh
