import { useState, useEffect, useCallback, useRef } from 'react'
import chatHubService from '../services/ChatHub'

export const useChatHub = (chat, onMessageReceived, onMessageUpdated, onMessageUpdateFailed) => {
  const [isConnected, setIsConnected] = useState(false)
  const setupInProgress = useRef(false)
  const currentChatId = useRef(null)
  
  // Стабилизируем колбэки
  const stableOnMessageReceived = useRef(onMessageReceived)
  const stableOnMessageUpdated = useRef(onMessageUpdated)
  const stableOnMessageUpdateFailed = useRef(onMessageUpdateFailed)
  
  // Обновляем ссылки на колбэки
  stableOnMessageReceived.current = onMessageReceived
  stableOnMessageUpdated.current = onMessageUpdated
  stableOnMessageUpdateFailed.current = onMessageUpdateFailed

  const sendMessage = useCallback(async (message) => {
    if (!isConnected || !chat) {
      throw new Error('Нет соединения с сервером или чат не выбран');
    }

    try {
      console.log('Отправляем сообщение через ChatHub:', message);
      await chatHubService.sendMessage(chat.chatId || chat.id, message);
      console.log('Сообщение отправлено успешно');
    } catch (error) {
      console.error('Ошибка отправки сообщения в useChatHub:', error);
      throw error;
    }
  }, [isConnected, chat]);

  useEffect(() => {
    if (!chat) {
      setIsConnected(false)
      return
    }

    const chatId = chat.chatId || chat.id
    
    // Если это тот же чат, не переподключаемся
    if (currentChatId.current === chatId && isConnected) {
      console.log("Уже подключены к этому чату:", chatId)
      return
    }

    // Предотвращаем множественные вызовы
    if (setupInProgress.current) {
      console.log("Настройка уже в процессе, пропускаем...")
      return
    }

    setupInProgress.current = true
    currentChatId.current = chatId

    const setupChatHub = async () => {
      try {
        console.log("=== НАЧАЛО НАСТРОЙКИ CHATHUB ===", chatId)
        
        const connected = await chatHubService.connect()
        if (!connected) {
          console.error("Не удалось подключиться к ChatHub")
          setIsConnected(false)
          return
        }

        console.log("Соединение установлено, присоединяемся к чату...")
        await chatHubService.joinChat(chatId)
        
        // Настраиваем обработчики с использованием стабильных ссылок
        chatHubService.onReceiveMessage((...args) => {
          if (stableOnMessageReceived.current) {
            stableOnMessageReceived.current(...args)
          }
        })
        
        chatHubService.onMessageUpdated((...args) => {
          if (stableOnMessageUpdated.current) {
            stableOnMessageUpdated.current(...args)
          }
        })
        
        chatHubService.onMessageUpdateFailed((...args) => {
          if (stableOnMessageUpdateFailed.current) {
            stableOnMessageUpdateFailed.current(...args)
          }
        })

        // Обработчики соединения
        chatHubService.onDisconnected(() => {
          console.log("Соединение потеряно")
          setIsConnected(false)
        })
        
        chatHubService.onReconnecting(() => {
          console.log("Переподключение...")
          setIsConnected(false)
        })
        
        chatHubService.onReconnected(() => {
          console.log("Переподключено успешно")
          setIsConnected(true)
        })

        setIsConnected(true)
        console.log("=== НАСТРОЙКА CHATHUB ЗАВЕРШЕНА ===", chatId)

      } catch (error) {
        console.error("Ошибка при настройке ChatHub:", error)
        setIsConnected(false)
      } finally {
        setupInProgress.current = false
      }
    }

    setupChatHub()

    // Cleanup функция
    return () => {
      // Только если это действительно наш чат
      if (currentChatId.current === chatId) {
        console.log("=== ОЧИСТКА CHATHUB ===", chatId)
        
        // Не покидаем чат сразу, даем время на переключение
        setTimeout(async () => {
          // Проверяем, не переключились ли мы на другой чат
          if (currentChatId.current === chatId) {
            try {
              await chatHubService.leaveChat(chatId)
              chatHubService.clearChatHandlers()
              currentChatId.current = null
              console.log("Очистка завершена для чата:", chatId)
            } catch (error) {
              console.error("Ошибка при очистке ChatHub:", error)
            }
          }
        }, 100) // Небольшая задержка
      }
    }
  }, [chat?.chatId || chat?.id]) // Только ID чата как зависимость

  return { isConnected, sendMessage }
}