import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr"

class ChatHubService {
  constructor() {
    this.connection = null
    this.isConnected = false
    this.messageHandlers = new Map()
    this.connectionHandlers = {
      onConnected: [],
      onDisconnected: [],
      onReconnecting: [],
      onReconnected: []
    }
    this.currentChatId = null
    this.reconnectAttempts = 0
    this.maxReconnectAttempts = 5
    this.joinInProgress = new Set() // Отслеживаем процесс присоединения
  }

  // Создание соединения
  async createConnection() {
    if (this.connection && this.connection.state !== "Disconnected") {
      console.log("Соединение уже существует, состояние:", this.connection.state)
      return this.connection
    }

    console.log("Создаем новое соединение ChatHub")
    this.connection = new HubConnectionBuilder()
      .withUrl("https://localhost:5087/chatHub", {
        withCredentials: true
      })
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect([0, 2000, 10000, 30000])
      .build()

    this.setupConnectionEvents()
    return this.connection
  }

  // Настройка событий соединения
  setupConnectionEvents() {
    if (!this.connection) return

    this.connection.onclose(async (error) => {
      this.isConnected = false
      console.log("ChatHub соединение закрыто:", error)
      this.connectionHandlers.onDisconnected.forEach(handler => handler(error))
    })

    this.connection.onreconnecting((error) => {
      this.isConnected = false
      console.warn("ChatHub переподключение из-за ошибки:", error)
      this.connectionHandlers.onReconnecting.forEach(handler => handler(error))
    })

    this.connection.onreconnected(async (connectionId) => {
      this.isConnected = true
      this.reconnectAttempts = 0
      console.log("ChatHub переподключен. ConnectionId:", connectionId)
      
      // Автоматически присоединяемся к текущему чату
      if (this.currentChatId) {
        try {
          await this.joinChat(this.currentChatId)
        } catch (error) {
          console.error("Ошибка при автоматическом присоединении к чату:", error)
        }
      }
      
      this.connectionHandlers.onReconnected.forEach(handler => handler(connectionId))
    })
  }

  // Подключение к серверу
  async connect() {
    try {
      if (!this.connection) {
        await this.createConnection()
      }

      if (this.connection.state === "Disconnected") {
        console.log("Подключаемся к ChatHub...")
        await this.connection.start()
        this.isConnected = true
        this.reconnectAttempts = 0
        console.log("ChatHub подключен успешно")
        this.connectionHandlers.onConnected.forEach(handler => handler())
        return true
      } else if (this.connection.state === "Connected") {
        this.isConnected = true
        console.log("ChatHub уже подключен")
        return true
      } else {
        console.log("ChatHub в состоянии:", this.connection.state)
        // Ждем подключения
        await new Promise(resolve => setTimeout(resolve, 1000))
        return this.connection.state === "Connected"
      }
    } catch (error) {
      console.error("Ошибка подключения к ChatHub:", error)
      this.isConnected = false
      return false
    }
  }

  // Присоединение к чату
  async joinChat(chatId) {
    if (!chatId) {
      throw new Error("chatId не может быть пустым")
    }

    // Предотвращаем множественные вызовы для одного чата
    if (this.joinInProgress.has(chatId)) {
      console.log("Присоединение к чату уже в процессе:", chatId)
      return
    }

    // Если уже в этом чате, не присоединяемся повторно
    if (this.currentChatId === chatId) {
      console.log("Уже присоединены к чату:", chatId)
      return
    }

    this.joinInProgress.add(chatId)

    try {
      // Сначала убеждаемся, что соединение установлено
      if (!this.isConnected) {
        const connected = await this.connect()
        if (!connected) {
          throw new Error("Не удалось установить соединение с ChatHub")
        }
      }

      // Если мы в другом чате, покидаем его
      if (this.currentChatId && this.currentChatId !== chatId) {
        console.log("Покидаем предыдущий чат:", this.currentChatId)
        await this.connection.invoke("LeaveChat", this.currentChatId)
      }

      console.log("Присоединяемся к чату:", chatId)
      await this.connection.invoke("JoinChat", chatId)
      this.currentChatId = chatId
      console.log("Успешно присоединились к чату:", chatId)
    } catch (error) {
      console.error("Ошибка при присоединении к чату:", error)
      throw error
    } finally {
      this.joinInProgress.delete(chatId)
    }
  }

  // Покидание чата
  async leaveChat(chatId) {
    if (!this.isConnected || !this.connection || !chatId) {
      return
    }

    try {
      console.log("Покидаем чат:", chatId)
      await this.connection.invoke("LeaveChat", chatId)
      
      if (this.currentChatId === chatId) {
        this.currentChatId = null
      }
      
      this.clearChatHandlers()
      console.log("Покинули чат:", chatId)
    } catch (error) {
      console.error("Ошибка при покидании чата:", error)
    }
  }

  // Отправка сообщения
  async sendMessage(chatId, message) {
    if (!this.isConnected) {
      const connected = await this.connect()
      if (!connected) {
        throw new Error("Нет соединения с ChatHub")
      }
    }

    try {
      console.log("Отправляем сообщение:", { chatId, message })
      await this.connection.invoke("SendMessage", chatId, message)
      console.log("Сообщение отправлено успешно")
    } catch (error) {
      console.error("Ошибка при отправке сообщения:", error)
      throw error
    }
  }

  // Остальные методы остаются без изменений...
  async updateMessage(messageId, newText) {
    if (!this.isConnected) {
      throw new Error("Нет соединения с ChatHub")
    }

    try {
      await this.connection.invoke("UpdateMessage", messageId, newText)
      console.log("Сообщение обновлено:", messageId)
    } catch (error) {
      console.error("Ошибка при обновлении сообщения:", error)
      throw error
    }
  }

  onReceiveMessage(handler) {
    if (!this.connection) return

    const wrappedHandler = (user, message, messageId, senderId, status) => {
      console.log("Получено сообщение от ChatHub:", { user, message, messageId, senderId, status })
      handler(user, message, messageId, senderId, status)
    }

    this.connection.on("ReceiveMessage", wrappedHandler)
    
    if (!this.messageHandlers.has("ReceiveMessage")) {
      this.messageHandlers.set("ReceiveMessage", [])
    }
    this.messageHandlers.get("ReceiveMessage").push(wrappedHandler)
  }

  onMessageUpdated(handler) {
    if (!this.connection) return

    const wrappedHandler = (messageId, newText, userId) => {
      console.log("Сообщение обновлено через ChatHub:", { messageId, newText, userId })
      handler(messageId, newText, userId)
    }

    this.connection.on("MessageUpdated", wrappedHandler)
    
    if (!this.messageHandlers.has("MessageUpdated")) {
      this.messageHandlers.set("MessageUpdated", [])
    }
    this.messageHandlers.get("MessageUpdated").push(wrappedHandler)
  }

  onMessageUpdateFailed(handler) {
    if (!this.connection) return

    const wrappedHandler = (messageId, error) => {
      console.error("Ошибка обновления сообщения:", { messageId, error })
      handler(messageId, error)
    }

    this.connection.on("MessageUpdateFailed", wrappedHandler)
    
    if (!this.messageHandlers.has("MessageUpdateFailed")) {
      this.messageHandlers.set("MessageUpdateFailed", [])
    }
    this.messageHandlers.get("MessageUpdateFailed").push(wrappedHandler)
  }

  onConnected(handler) {
    this.connectionHandlers.onConnected.push(handler)
  }

  onDisconnected(handler) {
    this.connectionHandlers.onDisconnected.push(handler)
  }

  onReconnecting(handler) {
    this.connectionHandlers.onReconnecting.push(handler)
  }

  onReconnected(handler) {
    this.connectionHandlers.onReconnected.push(handler)
  }

  clearAllHandlers() {
    if (this.connection) {
      this.messageHandlers.forEach((handlers, eventName) => {
        this.connection.off(eventName)
      })
    }
    
    this.messageHandlers.clear()
    this.connectionHandlers = {
      onConnected: [],
      onDisconnected: [],
      onReconnecting: [],
      onReconnected: []
    }
  }

  clearChatHandlers() {
    if (this.connection) {
      this.connection.off("ReceiveMessage")
      this.connection.off("MessageUpdated")
      this.connection.off("MessageUpdateFailed")
    }
    
    this.messageHandlers.delete("ReceiveMessage")
    this.messageHandlers.delete("MessageUpdated")
    this.messageHandlers.delete("MessageUpdateFailed")
  }

  getConnectionState() {
    return {
      isConnected: this.isConnected,
      state: this.connection?.state || "Disconnected",
      currentChatId: this.currentChatId
    }
  }
}

const chatHubService = new ChatHubService()
export default chatHubService