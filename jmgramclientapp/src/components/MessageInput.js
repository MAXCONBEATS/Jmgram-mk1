import React, { useState, useRef } from "react"
import EmojiPicker from "emoji-picker-react"
import { fileService } from "../services/FileService"
import "../css/MessageInput.css"

const MessageInput = ({
  chat,
  isConnected,
  sendMessage,
  editingMessageId,
  replyingToMessage,
  participantNameMap,
  onCancelReply,
  onEditingComplete,
  onMessageSent,
  disabled,
}) => {
  const [message, setMessage] = useState("")
  const [uploadingFile, setUploadingFile] = useState(false)
  const [showEmojiPicker, setShowEmojiPicker] = useState(false)
  const inputRef = useRef(null)
  const fileInputRef = useRef(null)
  const emojiPickerRef = useRef(null)

  console.log("MessageInput render - uploadingFile:", uploadingFile, "disabled:", disabled, "isConnected:", isConnected)

  // Обработчик клика вне эмодзи пикера
  React.useEffect(() => {
    const handleClickOutside = (event) => {
      if (emojiPickerRef.current && !emojiPickerRef.current.contains(event.target)) {
        setShowEmojiPicker(false)
      }
    }

    document.addEventListener("mousedown", handleClickOutside)
    return () => {
      document.removeEventListener("mousedown", handleClickOutside)
    }
  }, [])

  const handleEmojiClick = (emojiData) => {
    const input = inputRef.current
    if (input) {
      const start = input.selectionStart
      const end = input.selectionEnd
      const newMessage = message.slice(0, start) + emojiData.emoji + message.slice(end)
      setMessage(newMessage)

      // Устанавливаем курсор после эмодзи
      setTimeout(() => {
        input.focus()
        input.setSelectionRange(start + emojiData.emoji.length, start + emojiData.emoji.length)
      }, 0)
    } else {
      setMessage((prev) => prev + emojiData.emoji)
    }
    setShowEmojiPicker(false)
  }

  const handleKeyPress = async (event) => {
    if (event.key === "Enter" && !disabled && !uploadingFile && isConnected) {
      event.preventDefault()
      if (message.trim()) {
        try {
          await sendMessage(message)
          setMessage("")
          if (onMessageSent) {
            onMessageSent()
          }
        } catch (error) {
          console.error("Ошибка отправки сообщения:", error)
          alert("Ошибка отправки сообщения")
        }
      }
    }
  }

  const handleFileUpload = async (event) => {
    const file = event.target.files[0]
    if (!file) return

    console.log("=== НАЧАЛО ЗАГРУЗКИ ФАЙЛА ===")
    console.log("Файл:", file.name, "Размер:", file.size)

    setUploadingFile(true)

    try {
      console.log("Вызываем fileService.uploadFile")
      const response = await fileService.uploadFile(file, chat.chatId || chat.id)
      console.log("Ответ от fileService:", response)

      if (response && response.success) {
        const fileMessage = `📎 Файл: ${response.fileName} (${fileService.formatFileSize(response.fileSize)})`
        console.log("Сформированное сообщение:", JSON.stringify(fileMessage))

        console.log("Отправляем через sendMessage")
        await sendMessage(fileMessage)
        console.log("Сообщение отправлено успешно")

        // ДОБАВЛЯЕМ: Очищаем кэш файлового сервиса
        fileService.clearCache()

        // ДОБАВЛЯЕМ: Принудительно обновляем компонент
        if (onMessageSent) {
          console.log("Вызываем onMessageSent")
          onMessageSent()
        }

        // ДОБАВЛЯЕМ: Небольшая задержка для обновления UI
        setTimeout(() => {
          if (onMessageSent) {
            onMessageSent()
          }
        }, 100)
      } else {
        console.error("Ошибка в ответе:", response)
        alert(`Ошибка при загрузке файла: ${response?.message || "Неизвестная ошибка"}`)
      }
    } catch (error) {
      console.error("=== ОШИБКА ЗАГРУЗКИ ФАЙЛА ===", error)
      alert("Ошибка при загрузке файла: " + error.message)
    } finally {
      console.log("=== ЗАВЕРШЕНИЕ ЗАГРУЗКИ (finally) ===")
      setUploadingFile(false)
      if (fileInputRef.current) {
        fileInputRef.current.value = ""
      }
      console.log("uploadingFile установлен в false")
    }
  }

  const resetUploadState = () => {
    console.log("Принудительный сброс состояния загрузки")
    setUploadingFile(false)
  }

  const handleSendMessage = async () => {
    if (message.trim() && !disabled && !uploadingFile && isConnected) {
      try {
        await sendMessage(message)
        setMessage("")
        if (onMessageSent) {
          onMessageSent()
        }
      } catch (error) {
        console.error("Ошибка отправки сообщения:", error)
        alert("Ошибка отправки сообщения")
      }
    }
  }

  const isInputDisabled = disabled || uploadingFile || !isConnected

  return (
    <div className="message-input-container">
      {!isConnected && <div className="connection-warning">Нет соединения с сервером. Подождите...</div>}

      <div className="input-wrapper">
        <input
          ref={inputRef}
          type="text"
          value={message}
          onChange={(e) => setMessage(e.target.value)}
          onKeyPress={handleKeyPress}
          placeholder={
            !isConnected ? "Ожидание соединения..." : uploadingFile ? "Загрузка файла..." : "Введите сообщение"
          }
          className="message-input"
          disabled={isInputDisabled}
        />

        <div className="message-input-buttons">
          {/* Кнопка эмодзи */}
          <button
            type="button"
            onClick={() => setShowEmojiPicker(!showEmojiPicker)}
            className="emoji-button"
            disabled={isInputDisabled}
          >
            😀
          </button>

          {/* Кнопка загрузки файла */}
          <input
            type="file"
            style={{ display: "none" }}
            id="fileInput"
            onChange={handleFileUpload}
            ref={fileInputRef}
            disabled={uploadingFile || !isConnected}
          />
          <label
            htmlFor="fileInput"
            className={`file-upload-button ${uploadingFile ? "uploading" : ""} ${!isConnected ? "disabled" : ""}`}
          >
            📎
          </label>

          <button onClick={handleSendMessage} disabled={isInputDisabled || !message.trim()} className="send-button">
            Отправить
          </button>
        </div>
      </div>

      {/* Эмодзи пикер */}
      {showEmojiPicker && (
        <div className="emoji-picker-container" ref={emojiPickerRef}>
          <EmojiPicker
            onEmojiClick={handleEmojiClick}
            theme="dark"
            width={300}
            height={400}
            previewConfig={{
              showPreview: false,
            }}
            skinTonesDisabled={true}
          />
        </div>
      )}

      {uploadingFile && (
        <div className="upload-status">
          <span>Загрузка файла...</span>
          <button onClick={resetUploadState} className="upload-status-cancel-button">
            Отменить
          </button>
        </div>
      )}
    </div>
  )
}

export default MessageInput
