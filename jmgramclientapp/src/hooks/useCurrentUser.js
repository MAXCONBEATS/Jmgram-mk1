import { useState, useEffect } from "react"
import { getCurrentUser } from "../controllers/AccountController"

export const useCurrentUser = () => {
  const [currentUser, setCurrentUser] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    const fetchCurrentUser = async () => {
      try {
        console.log("useCurrentUser: загружаем данные пользователя...")
        setLoading(true)
        setError(null)

        const userData = await getCurrentUser()
        console.log("useCurrentUser: данные пользователя получены:", userData)

        setCurrentUser(userData)
      } catch (err) {
        console.error("useCurrentUser: ошибка загрузки пользователя:", err)
        setError(err.message || "Ошибка загрузки пользователя")
        setCurrentUser(null)
      } finally {
        setLoading(false)
      }
    }

    fetchCurrentUser()
  }, [])

  return { currentUser, loading, error, setCurrentUser }
}
