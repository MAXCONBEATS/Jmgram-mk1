"use client"

import { useState, useRef, useEffect } from "react"
import EmojiPicker from "emoji-picker-react"
import { fileService } from "../services/FileService"
import { canSendMessage } from "../controllers/ChatController"
import "../css/MessageInput.css"

const MessageInput = ({ chat, isConnected, sendMessage, onMessageSent, disabled }) => {
  const [message, setMessage] = useState("")
  const [uploadingFile, setUploadingFile] = useState(false)
  const [showEmojiPicker, setShowEmojiPicker] = useState(false)

  // УПРОЩАЕМ: только проверка прав отправки
  const [canSend, setCanSend] = useState(true)
  const [permissionMessage, setPermissionMessage] = useState("")

  const inputRef = useRef(null)
  const fileInputRef = useRef(null)
  const emojiPickerRef = useRef(null)

  // Проверяем права только при смене чата
  useEffect(() => {
    if (!chat?.chatId && !chat?.id) return

    const checkPermissions = async () => {
      try {
        const result = await canSendMessage(chat.chatId || chat.id)
        setCanSend(result.canSend)
        setPermissionMessage(result.message)
      } catch (error) {
        console.error("Ошибка проверки прав:", error)
        // По умолчанию разрешаем (fail-safe)
        setCanSend(true)
        setPermissionMessage("")
      }
    }

    checkPermissions()
  }, [chat?.chatId, chat?.id])

  // Обработчик клика вне эмодзи пикера
  useEffect(() => {
    const handleClickOutside = (event) => {
      if (emojiPickerRef.current && !emojiPickerRef.current.contains(event.target)) {
        setShowEmojiPicker(false)
      }
    }

    document.addEventListener("mousedown", handleClickOutside)
    return () => document.removeEventListener("mousedown", handleClickOutside)
  }, [])

  const handleEmojiClick = (emojiData) => {
    const input = inputRef.current
    if (input) {
      const start = input.selectionStart
      const end = input.selectionEnd
      const newMessage = message.slice(0, start) + emojiData.emoji + message.slice(end)
      setMessage(newMessage)

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
    if (event.key === "Enter" && !disabled && !uploadingFile && isConnected && canSend && message.trim()) {
      event.preventDefault()
      await handleSendMessage()
    }
  }

  const handleFileUpload = async (event) => {
    const file = event.target.files[0]
    if (!file) return

    setUploadingFile(true)

    try {
      const response = await fileService.uploadFile(file, chat.chatId || chat.id)

      if (response?.success) {
        const fileMessage = `📎 Файл: ${response.fileName} (${fileService.formatFileSize(response.fileSize)})`
        await sendMessage(fileMessage)
        if (onMessageSent) onMessageSent()
      } else {
        alert(`Ошибка загрузки: ${response?.message || "Неизвестная ошибка"}`)
      }
    } catch (error) {
      console.error("Ошибка загрузки файла:", error)
      alert("Ошибка загрузки файла: " + error.message)
    } finally {
      setUploadingFile(false)
      if (fileInputRef.current) fileInputRef.current.value = ""
    }
  }

  const handleSendMessage = async () => {
    if (!message.trim() || disabled || uploadingFile || !isConnected || !canSend) return

    try {
      await sendMessage(message)
      setMessage("")
      if (onMessageSent) onMessageSent()
    } catch (error) {
      console.error("Ошибка отправки:", error)
      alert("Ошибка отправки: " + error.message)
    }
  }

  // Определяем состояние интерфейса
  const isInputDisabled = disabled || uploadingFile || !isConnected || !canSend

  let placeholder = "Введите сообщение..."
  if (!isConnected) {
    placeholder = "Ожидание соединения..."
  } else if (uploadingFile) {
    placeholder = "Загрузка файла..."
  } else if (!canSend) {
    placeholder = permissionMessage || "Нет прав для отправки"
  }

  return (
    <div className="message-input-container">
      {/* Показываем предупреждения только если есть проблемы */}
      {!isConnected && <div className="connection-warning">Нет соединения с сервером</div>}

      {!canSend && permissionMessage && (
        <div className="permission-warning">
          {chat?.chatType === 1 ? "📢" : "👥"} {permissionMessage}
        </div>
      )}

      <div className="input-wrapper">
        <input
          ref={inputRef}
          type="text"
          value={message}
          onChange={(e) => setMessage(e.target.value)}
          onKeyPress={handleKeyPress}
          placeholder={placeholder}
          className="message-input"
          disabled={isInputDisabled}
        />

        <div className="message-input-buttons">
          <button
            type="button"
            onClick={() => setShowEmojiPicker(!showEmojiPicker)}
            className="emoji-button"
            disabled={isInputDisabled}
          >
            😀
          </button>

          <input
            type="file"
            style={{ display: "none" }}
            id="fileInput"
            onChange={handleFileUpload}
            ref={fileInputRef}
            disabled={isInputDisabled}
          />
          <label
            htmlFor="fileInput"
            className={`file-upload-button ${uploadingFile ? "uploading" : ""} ${isInputDisabled ? "disabled" : ""}`}
          >
            📎
          </label>

          <button onClick={handleSendMessage} disabled={isInputDisabled || !message.trim()} className="send-button">
            Отправить
          </button>
        </div>
      </div>

      {showEmojiPicker && canSend && (
        <div className="emoji-picker-container" ref={emojiPickerRef}>
          <EmojiPicker
            onEmojiClick={handleEmojiClick}
            theme="dark"
            width={300}
            height={400}
            previewConfig={{ showPreview: false }}
            skinTonesDisabled={true}
          />
        </div>
      )}

      {uploadingFile && (
        <div className="upload-status">
          <span>Загрузка файла...</span>
          <button onClick={() => setUploadingFile(false)} className="upload-status-cancel-button">
            Отменить
          </button>
        </div>
      )}
    </div>
  )
}

export default MessageInput
