// Фронтенд - проверка типа чата
export const ChatService = {
  // Проверяем, может ли пользователь писать в чат
  canUserSendMessage(chat, currentUserId) {
    if (!chat || !currentUserId) return false

    switch (chat.chatType) {
      case "Channel":
        // В канале писать может только создатель
        return chat.creatorUserId === currentUserId

      case "Group":
        // В группе могут писать все участники
        return chat.participants?.some((p) => p.userId === currentUserId) || false

      case "Private":
        // В приватном чате могут писать оба участника
        return chat.participants?.some((p) => p.userId === currentUserId) || false

      default:
        return false
    }
  },

  // Получаем иконку для типа чата
  getChatIcon(chatType) {
    switch (chatType) {
      case "Channel":
        return "📢"
      case "Group":
        return "👥"
      case "Private":
        return "💬"
      default:
        return "💬"
    }
  },
}
