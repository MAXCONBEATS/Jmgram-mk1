"use client"

import { fileService } from "../services/FileService"
import { useState, useRef, useEffect } from "react"
import "../css/messageUtils.css"

export const formatMessageTime = (timestamp) => {
  if (!timestamp) return ""
  const correctedDate = new Date(timestamp)
  const now = new Date()
  const today = new Date(now.getFullYear(), now.getMonth(), now.getDate())
  const messageDate = new Date(correctedDate.getFullYear(), correctedDate.getMonth(), correctedDate.getDate())

  const timeString = correctedDate.toLocaleTimeString("ru-RU", {
    hour: "2-digit",
    minute: "2-digit",
  })

  if (messageDate.getTime() === today.getTime()) {
    return timeString
  }

  const yesterday = new Date(today.getTime() - 24 * 60 * 60 * 1000)
  if (messageDate.getTime() === yesterday.getTime()) {
    return `вчера ${timeString}`
  }

  if (correctedDate.getFullYear() === now.getFullYear()) {
    return (
      correctedDate.toLocaleDateString("ru-RU", {
        day: "2-digit",
        month: "2-digit",
      }) + ` ${timeString}`
    )
  }

  return (
    correctedDate.toLocaleDateString("ru-RU", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    }) + ` ${timeString}`
  )
}

const MediaComponent = ({ fileName, fileType, fileSize }) => {
  const [status, setStatus] = useState("loading") // Сразу начинаем с загрузки
  const [errorMessage, setErrorMessage] = useState("")
  const [objectUrl, setObjectUrl] = useState(null)
  const [retryCount, setRetryCount] = useState(0)
  const [autoRetryCount, setAutoRetryCount] = useState(0)
  const abortControllerRef = useRef(null)
  const retryTimeoutRef = useRef(null)

  // Очистка при размонтировании
  useEffect(() => {
    return () => {
      if (abortControllerRef.current) {
        abortControllerRef.current.abort()
      }
      if (retryTimeoutRef.current) {
        clearTimeout(retryTimeoutRef.current)
      }
      if (objectUrl) {
        URL.revokeObjectURL(objectUrl)
      }
    }
  }, [objectUrl])

  // Автоматическая загрузка при монтировании
  useEffect(() => {
    console.log("MediaComponent: монтирование компонента для файла", fileName)
    loadImageDirectly()
  }, [fileName])

  // Прямая загрузка изображения (самый простой способ)
  const loadImageDirectly = () => {
    if (fileType !== "image") {
      setStatus("loaded")
      return
    }

    console.log("MediaComponent: прямая загрузка изображения", fileName)
    setStatus("loading")
    setErrorMessage("")

    // Создаем новый элемент изображения для проверки
    const testImg = new Image()
    const fileUrl = fileService.getFileUrl(fileName)

    testImg.onload = () => {
      console.log("MediaComponent: изображение загружено успешно", fileName)
      setStatus("loaded")
      setRetryCount(0)
      setAutoRetryCount(0)
    }

    testImg.onerror = () => {
      console.error("MediaComponent: ошибка загрузки изображения", fileName)

      // Автоматические повторы
      if (autoRetryCount < 3) {
        const delay = (autoRetryCount + 1) * 1000 // 1с, 2с, 3с
        console.log(`MediaComponent: автоповтор через ${delay}мс (попытка ${autoRetryCount + 1}/3)`)

        setAutoRetryCount((prev) => prev + 1)
        retryTimeoutRef.current = setTimeout(() => {
          loadImageDirectly()
        }, delay)
      } else {
        console.log("MediaComponent: все автоповторы исчерпаны, показываем ошибку")
        setStatus("error")
        setErrorMessage("Не удалось загрузить изображение после нескольких попыток")
      }
    }

    // Добавляем случайный параметр для обхода кеша
    testImg.src = `${fileUrl}?_=${Date.now()}`
  }

  // Загрузка через Object URL (для проблемных файлов)
  const loadViaObjectUrl = async () => {
    try {
      setStatus("loading")
      setErrorMessage("")

      console.log("MediaComponent: загружаем через Object URL", fileName)

      const url = await fileService.createObjectUrl(fileName)
      if (url) {
        setObjectUrl(url)
        setStatus("loaded")
        console.log("MediaComponent: Object URL создан", url)
      } else {
        throw new Error("Не удалось создать Object URL")
      }
    } catch (error) {
      console.error("MediaComponent: ошибка загрузки Object URL", error)
      setStatus("error")
      setErrorMessage(error.message)
    }
  }

  // Принудительная перезагрузка
  const forceReload = () => {
    console.log("MediaComponent: принудительная перезагрузка", fileName)
    fileService.clearCache()
    setStatus("loading")
    setErrorMessage("")
    setObjectUrl(null)
    setRetryCount(0)
    setAutoRetryCount(0)

    // Небольшая задержка перед перезагрузкой
    setTimeout(() => {
      loadImageDirectly()
    }, 100)
  }

  const fileUrl = fileService.getFileUrl(fileName)

  // Рендеринг в зависимости от статуса
  if (status === "loading") {
    return (
      <div className="media-message">
        <div className="media-status">
          <div className="loading-spinner"></div>
          <p>Загрузка файла...</p>
          {autoRetryCount > 0 && <p className="retry-info">Попытка {autoRetryCount}/3</p>}
        </div>
        <div className="file-info">
          <span className="file-name">{fileName}</span>
          <span className="file-size">({fileSize})</span>
        </div>
      </div>
    )
  }

  if (status === "error") {
    return (
      <div className="media-message">
        <div className="media-error">
          <div className="error-header">
            <i className="bi bi-exclamation-triangle"></i>
            <span>Ошибка загрузки</span>
          </div>
          <div className="error-details">
            <p>
              <strong>Файл:</strong> {fileName}
            </p>
            <p>
              <strong>Ошибка:</strong> {errorMessage}
            </p>
          </div>
          <div className="error-actions">
            <button onClick={forceReload} className="btn btn-sm btn-primary">
              Попробовать снова
            </button>
            <button onClick={loadViaObjectUrl} className="btn btn-sm btn-secondary">
              Загрузить через blob
            </button>
            <a href={fileUrl} target="_blank" rel="noopener noreferrer" className="btn btn-sm btn-outline">
              Открыть в новой вкладке
            </a>
          </div>
        </div>
        <div className="file-info">
          <span className="file-name">{fileName}</span>
          <span className="file-size">({fileSize})</span>
        </div>
      </div>
    )
  }

  // Статус "loaded" - показываем медиафайл
  const displayUrl = objectUrl || fileUrl

  if (fileType === "image") {
    return (
      <div className="media-message">
        <div className="image-container">
          <img
            src={displayUrl || "/placeholder.svg"}
            alt={fileName}
            className="image-thumbnail"
            onClick={() => window.open(fileUrl, "_blank")}
            onLoad={() => {
              console.log("MediaComponent: изображение отображено успешно", fileName)
            }}
            onError={(e) => {
              console.error("MediaComponent: ошибка отображения изображения", fileName)

              // Если это не Object URL, пробуем его
              if (!objectUrl && retryCount < 1) {
                console.log("MediaComponent: пробуем Object URL")
                setRetryCount((prev) => prev + 1)
                loadViaObjectUrl()
              } else {
                setStatus("error")
                setErrorMessage("Не удалось отобразить изображение")
              }
            }}
            loading="lazy"
            style={{ maxWidth: "300px", maxHeight: "300px" }}
          />
        </div>
        <div className="file-info">
          <span className="file-name">{fileName}</span>
          <span className="file-size">({fileSize})</span>
        </div>
      </div>
    )
  }

  if (fileType === "video") {
    return (
      <div className="media-message">
        <video
          controls
          className="video-player"
          preload="metadata"
          style={{ maxWidth: "400px", maxHeight: "300px" }}
          onError={() => {
            console.error("MediaComponent: ошибка видео", fileName)
            setStatus("error")
            setErrorMessage("Ошибка воспроизведения видео")
          }}
        >
          <source src={displayUrl} type="video/mp4" />
          Ваш браузер не поддерживает видео
        </video>
        <div className="file-info">
          <span className="file-name">{fileName}</span>
          <span className="file-size">({fileSize})</span>
        </div>
      </div>
    )
  }

  if (fileType === "audio") {
    return (
      <div className="media-message">
        <audio
          controls
          className="audio-player"
          preload="metadata"
          onError={() => {
            console.error("MediaComponent: ошибка аудио", fileName)
            setStatus("error")
            setErrorMessage("Ошибка воспроизведения аудио")
          }}
        >
          <source src={displayUrl} type="audio/mpeg" />
          Ваш браузер не поддерживает аудио
        </audio>
        <div className="file-info">
          <span className="file-name">{fileName}</span>
          <span className="file-size">({fileSize})</span>
        </div>
      </div>
    )
  }

  // Для остальных файлов
  return (
    <div className="media-message">
      <div className="file-download">
        <a href={fileUrl} download={fileName} className="file-link" target="_blank" rel="noopener noreferrer">
          <i className="bi bi-file-earmark"></i>
          <span>{fileName}</span>
          <span className="file-size">({fileSize})</span>
        </a>
      </div>
    </div>
  )
}

export const renderMediaContent = (message) => {
  if (
    message.text.includes("📎") &&
    message.text.includes("Файл:") &&
    message.text.includes("(") &&
    message.text.includes(")")
  ) {
    const fileStartIndex = message.text.indexOf("Файл:") + 5
    const sizeStartIndex = message.text.lastIndexOf("(")
    const sizeEndIndex = message.text.lastIndexOf(")")

    if (fileStartIndex > 4 && sizeStartIndex > fileStartIndex && sizeEndIndex > sizeStartIndex) {
      const fileName = message.text.substring(fileStartIndex, sizeStartIndex).trim()
      const fileSize = message.text.substring(sizeStartIndex + 1, sizeEndIndex).trim()

      if (fileName && fileSize) {
        const fileType = fileService.getFileType(fileName)
        console.log("renderMediaContent: рендерим медиа", { fileName, fileType, fileSize })
        return <MediaComponent key={fileName} fileName={fileName} fileType={fileType} fileSize={fileSize} />
      }
    }
  }

  return <span>{message.text}</span>
}
