// Полностью изолированный сервис для работы с файлами
class FileService {
  constructor() {
    this.baseUrl = "https://localhost:5087/api/file"
    this.cache = new Map()
  }

  // Загрузка файла на сервер
  async uploadFile(file, chatId) {
    try {
      console.log("FileService: загружаем файл", file.name)

      const formData = new FormData()
      formData.append("file", file)
      formData.append("chatId", chatId)

      const response = await fetch(`${this.baseUrl}/upload`, {
        method: "POST",
        body: formData,
        credentials: "include",
        mode: "cors",
      })

      if (!response.ok) {
        const errorText = await response.text()
        throw new Error(`HTTP ${response.status}: ${errorText}`)
      }

      const result = await response.json()
      console.log("FileService: файл загружен", result)
      return result
    } catch (error) {
      console.error("FileService: ошибка загрузки", error)
      throw error
    }
  }

  // Получение URL файла
  getFileUrl(fileName) {
    return `${this.baseUrl}/download/${encodeURIComponent(fileName)}`
  }

  // Получение URL миниатюры
  getThumbnailUrl(fileName, width = 200, height = 200) {
    return `${this.baseUrl}/thumbnail/${encodeURIComponent(fileName)}?width=${width}&height=${height}`
  }

  // Проверка доступности файла
  async checkFileAvailability(fileName) {
    const cacheKey = `check_${fileName}`

    // Проверяем кэш (кэшируем на 30 секунд)
    if (this.cache.has(cacheKey)) {
      const cached = this.cache.get(cacheKey)
      if (Date.now() - cached.timestamp < 30000) {
        return cached.result
      }
    }

    try {
      const url = this.getFileUrl(fileName)

      const response = await fetch(url, {
        method: "HEAD",
        credentials: "include",
        mode: "cors",
        cache: "no-cache",
      })

      const result = {
        available: response.ok,
        status: response.status,
        statusText: response.statusText,
        contentType: response.headers.get("content-type"),
        contentLength: response.headers.get("content-length"),
      }

      // Кэшируем результат
      this.cache.set(cacheKey, {
        result,
        timestamp: Date.now(),
      })

      return result
    } catch (error) {
      const result = {
        available: false,
        status: 0,
        statusText: error.message,
        error: error.name,
      }

      // Кэшируем ошибку на меньшее время (5 секунд)
      this.cache.set(cacheKey, {
        result,
        timestamp: Date.now() - 25000, // Будет перепроверен через 5 секунд
      })

      return result
    }
  }

  // Получение файла как Blob
  async getFileBlob(fileName) {
    try {
      const url = this.getFileUrl(fileName)

      const response = await fetch(url, {
        method: "GET",
        credentials: "include",
        mode: "cors",
        cache: "no-cache",
      })

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`)
      }

      const blob = await response.blob()
      return blob
    } catch (error) {
      console.error("FileService: ошибка получения blob", error)
      throw error
    }
  }

  // Создание Object URL
  async createObjectUrl(fileName) {
    try {
      const blob = await this.getFileBlob(fileName)
      return URL.createObjectURL(blob)
    } catch (error) {
      console.error("FileService: ошибка создания Object URL", error)
      return null
    }
  }

  // Определение типа файла
  getFileType(fileName) {
    const extension = fileName.split(".").pop()?.toLowerCase()

    if (["jpg", "jpeg", "png", "gif", "webp", "bmp"].includes(extension)) {
      return "image"
    }
    if (["mp4", "avi", "mov", "webm", "mkv"].includes(extension)) {
      return "video"
    }
    if (["mp3", "wav", "ogg", "aac", "flac"].includes(extension)) {
      return "audio"
    }
    return "file"
  }

  // Форматирование размера файла
  formatFileSize(bytes) {
    if (bytes < 1024) return `${bytes} B`
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
  }

  // Очистка кэша для конкретного файла
  clearFileCache(fileName) {
    const keys = Array.from(this.cache.keys()).filter((key) => key.includes(fileName))
    keys.forEach((key) => this.cache.delete(key))
    console.log("FileService: очищен кэш для файла", fileName, "ключей:", keys.length)
  }

  // Принудительная перезагрузка файла
  async forceReload(fileName) {
    console.log("FileService: принудительная перезагрузка", fileName)
    this.clearFileCache(fileName)

    // Ждем немного и проверяем снова
    await new Promise((resolve) => setTimeout(resolve, 500))
    return await this.checkFileAvailability(fileName)
  }

  // Предзагрузка файла (для новых сообщений)
  async preloadFile(fileName) {
    console.log("FileService: предзагрузка файла", fileName)
    try {
      const result = await this.checkFileAvailability(fileName)
      if (result.available) {
        // Предзагружаем blob для быстрого отображения
        setTimeout(() => {
          this.getFileBlob(fileName).catch((err) => {
            console.log("FileService: ошибка предзагрузки blob", err.message)
          })
        }, 100)
      }
      return result
    } catch (error) {
      console.error("FileService: ошибка предзагрузки", error)
      return { available: false, error: error.message }
    }
  }

  // Очистка кэша
  clearCache() {
    this.cache.clear()
  }
}

// Экспортируем синглтон
export const fileService = new FileService()
