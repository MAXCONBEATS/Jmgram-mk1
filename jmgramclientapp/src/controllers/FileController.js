import axios from "axios"

// Настройка axios
axios.defaults.withCredentials = true
axios.defaults.timeout = 30000

// Добавляем интерцепторы для логирования
axios.interceptors.request.use(
  (config) => {
    console.log("FileController: отправляем запрос", config.method?.toUpperCase(), config.url)
    return config
  },
  (error) => {
    console.error("FileController: ошибка запроса", error)
    return Promise.reject(error)
  },
)

axios.interceptors.response.use(
  (response) => {
    console.log("FileController: получен ответ", response.status, response.config.url)
    return response
  },
  (error) => {
    // Улучшенное логирование CORS ошибок
    if (error.message === "Network Error") {
      console.error("FileController: CORS ошибка или сетевая проблема", {
        url: error.config?.url,
        message: "Возможно проблема с CORS настройками на сервере",
      })
    } else {
      console.error("FileController: ошибка ответа", {
        status: error.response?.status,
        statusText: error.response?.statusText,
        url: error.config?.url,
        message: error.message,
      })
    }
    return Promise.reject(error)
  },
)

// Базовый URL API
const API_BASE_URL = "https://localhost:5087/api"

export const FileController = {
  uploadFile: async (file, chatId) => {
    try {
      console.log("FileController: загружаем файл", file.name, "для чата", chatId)

      const formData = new FormData()
      formData.append("file", file)
      formData.append("chatId", chatId)

      const response = await axios.post(`${API_BASE_URL}/file/upload`, formData, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
        withCredentials: true,
      })

      console.log("FileController: ответ сервера", response.data)
      return response.data
    } catch (error) {
      console.error("FileController: ошибка при загрузке файла:", {
        message: error.message,
        status: error.response?.status,
        statusText: error.response?.statusText,
        data: error.response?.data,
      })
      throw error
    }
  },

  getFileBlob: async (fileName) => {
    try {
      console.log("FileController: получаем blob для файла", fileName)

      const url = `${API_BASE_URL}/file/download/${encodeURIComponent(fileName)}`
      console.log("FileController: запрос �� URL:", url)

      const response = await axios.get(url, {
        responseType: "blob",
        withCredentials: true,
        headers: {
          Accept: "*/*",
        },
        validateStatus: (status) => {
          return status < 500
        },
      })

      console.log("FileController: ответ получен", {
        status: response.status,
        statusText: response.statusText,
        headers: response.headers,
        dataType: typeof response.data,
        dataSize: response.data?.size,
      })

      if (response.status === 404) {
        throw new Error("Файл не найден на сервере")
      }

      if (response.status === 401) {
        throw new Error("Нет доступа к файлу")
      }

      if (response.status !== 200) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`)
      }

      if (!response.data || response.data.size === 0) {
        throw new Error("Получен пустой файл")
      }

      console.log("FileController: blob получен успешно для", fileName, "размер:", response.data.size)
      return response.data
    } catch (error) {
      console.error("FileController: ошибка получения blob:", {
        fileName,
        error: error.message,
        response: error.response?.status,
        responseText: error.response?.statusText,
      })
      throw error
    }
  },

  createMediaObjectUrl: async (fileName) => {
    try {
      console.log("FileController: создаем Object URL для", fileName)

      const blob = await FileController.getFileBlob(fileName)

      if (!blob) {
        throw new Error("Blob не получен")
      }

      console.log("FileController: blob получен, создаем Object URL", {
        blobSize: blob.size,
        blobType: blob.type,
      })

      const objectUrl = URL.createObjectURL(blob)

      console.log("FileController: Object URL создан:", objectUrl)
      return objectUrl
    } catch (error) {
      console.error("FileController: ошибка создания Object URL:", {
        fileName,
        errorMessage: error.message,
        errorStack: error.stack,
      })
      return null
    }
  },

  getFileUrl: (fileName) => {
    const url = `${API_BASE_URL}/file/download/${encodeURIComponent(fileName)}`
    console.log("FileController: URL файла:", url)
    return url
  },

  getThumbnailUrl: (fileName, width = 200, height = 200) => {
    const url = `${API_BASE_URL}/file/thumbnail/${encodeURIComponent(fileName)}?width=${width}&height=${height}`
    console.log("FileController: URL миниатюры:", url)
    return url
  },

  // Используем простой метод проверки для изображений
  checkImageAvailability: (fileName) => {
    return new Promise((resolve) => {
      const img = new Image()
      const url = `${API_BASE_URL}/file/download/${encodeURIComponent(fileName)}`

      img.onload = () => {
        console.log("FileController: изображение загружено успешно", fileName)
        resolve({
          available: true,
          status: 200,
          statusText: "OK",
        })
      }

      img.onerror = () => {
        console.error("FileController: ошибка загрузки изображения", fileName)
        resolve({
          available: false,
          status: 404,
          statusText: "Image load failed",
        })
      }

      // Добавляем случайный параметр для обхода кеша
      img.src = `${url}?_=${Date.now()}`
    })
  },

  // Используем GET с Range для проверки других файлов
  checkFileAvailability: async (fileName) => {
    try {
      const url = `${API_BASE_URL}/file/download/${encodeURIComponent(fileName)}`

      // Используем GET запрос с Range заголовком для получения только первого байта
      const response = await axios.get(url, {
        withCredentials: true,
        timeout: 5000,
        headers: {
          Range: "bytes=0-0", // Запрашиваем только первый байт
        },
        validateStatus: (status) => status < 500,
      })

      console.log("FileController: проверка файла", fileName, "статус:", response.status)

      return {
        available: response.status === 206 || response.status === 200, // 206 для partial content, 200 для полного
        status: response.status,
        statusText: response.statusText,
      }
    } catch (error) {
      console.error("FileController: ошибка проверки файла", fileName, error.message)
      return {
        available: false,
        status: error.response?.status || 0,
        statusText: error.message,
      }
    }
  },

  getFileType: (fileName) => {
    const extension = fileName.split(".").pop()?.toLowerCase()
    console.log("FileController: определяем тип файла для", fileName, "расширение:", extension)

    if (["jpg", "jpeg", "png", "gif", "webp", "bmp"].includes(extension)) {
      console.log("FileController: тип файла - image")
      return "image"
    }
    if (["mp4", "avi", "mov", "webm", "mkv"].includes(extension)) {
      console.log("FileController: тип файла - video")
      return "video"
    }
    if (["mp3", "wav", "ogg", "aac", "flac"].includes(extension)) {
      console.log("FileController: тип файла - audio")
      return "audio"
    }
    console.log("FileController: тип файла - file")
    return "file"
  },

  formatFileSize: (bytes) => {
    if (bytes < 1024) return `${bytes} B`
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
  },
}
