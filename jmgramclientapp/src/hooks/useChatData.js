import { useState, useEffect } from 'react'
import { getMessages, getChatUsersList, getChatNameForUser } from '../controllers/ChatController'

export const useChatData = (chat) => {
  const [messages, setMessages] = useState([])
  const [participants, setParticipants] = useState([])
  const [chatName, setChatName] = useState("")
  const [loading, setLoading] = useState(false)
  const [loadingChatName, setLoadingChatName] = useState(false)

  // Загрузка названия чата
  useEffect(() => {
    async function fetchChatName() {
      if (!chat) {
        setChatName("")
        return
      }
      
      setLoadingChatName(true)
      try {
        const name = await getChatNameForUser(chat.chatId || chat.id)
        setChatName(name || "Чат")
      } catch (error) {
        console.error("Ошибка при загрузке названия чата:", error)
        setChatName(chat?.name || chat?.chatName || "Чат")
      }
      setLoadingChatName(false)
    }
    
    fetchChatName()
  }, [chat])

  // Загрузка сообщений
  useEffect(() => {
    async function fetchMessages() {
      if (!chat) {
        setMessages([])
        return
      }
      setLoading(true)
      try {
        const response = await getMessages(chat.chatId || chat.id, 1, 20)
        const mappedMessages = (response.chat || []).map((msg) => ({
          ...msg,
          senderName: msg.senderName || "Unknown",
        }))
        
        const sortedMessages = mappedMessages.sort((a, b) => {
          if (!isNaN(a.id) && !isNaN(b.id)) {
            return parseInt(a.id) - parseInt(b.id)
          }
          return a.id.localeCompare(b.id)
        })
        
        setMessages(sortedMessages)
      } catch (error) {
        console.error("Ошибка при загрузке сообщений:", error)
        setMessages([])
      }
      setLoading(false)
    }
    fetchMessages()
  }, [chat])

  // Загрузка участников
  useEffect(() => {
    async function fetchParticipants() {
      if (!chat) {
        setParticipants([])
        return
      }
      try {
        const users = await getChatUsersList(chat.chatId || chat.id)
        const uniqueUsers = Array.from(new Map(users.map((u) => [u.id, u])).values())
        setParticipants(uniqueUsers)
      } catch (error) {
        console.error("Ошибка при загрузке участников чата:", error)
        setParticipants([])
      }
    }
    fetchParticipants()
  }, [chat])

  const refreshParticipants = async () => {
    if (!chat) return
    try {
      const users = await getChatUsersList(chat.chatId || chat.id)
      const uniqueUsers = Array.from(new Map(users.map((u) => [u.id, u])).values())
      setParticipants(uniqueUsers)
    } catch (error) {
      console.error("Ошибка при обновлении участников чата:", error)
    }
  }

  return {
    messages,
    setMessages,
    participants,
    chatName,
    loading,
    loadingChatName,
    refreshParticipants
  }
}