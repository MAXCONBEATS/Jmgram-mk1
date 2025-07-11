import axios from "axios";

// Настройка axios - остаются без изменений
axios.defaults.withCredentials = true;
axios.defaults.timeout = 30000;

// Добавляем интерцепторы для логирования
// В продакшене я бы рекомендовал сделать это логирование условным или использовать сторонние библиотеки для логирования
axios.interceptors.request.use(
  (config) => {
    // console.log("FileController: отправляем запрос", config.method?.toUpperCase(), config.url); // Меньше логирования в повседневной работе
    return config;
  },
  (error) => {
    console.error("FileController: ошибка запроса", error);
    return Promise.reject(error);
  }
);

axios.interceptors.response.use(
  (response) => {
    // console.log("FileController: получен ответ", response.status, response.config.url); // Меньше логирования
    return response;
  },
  (error) => {
    // Улучшенное логирование CORS ошибок
    if (error.message === "Network Error") {
      console.error("FileController: CORS ошибка или сетевая проблема", {
        url: error.config?.url,
        message: "Возможно проблема с CORS настройками на сервере",
      });
    } else {
      console.error("FileController: ошибка ответа", {
        status: error.response?.status,
        statusText: error.response?.statusText,
        url: error.config?.url,
        message: error.message,
      });
    }
    return Promise.reject(error);
  }
);

// Базовый URL API
// ВНИМАНИЕ: Для продакшена этот URL должен быть конфигурируемым (например, через переменные окружения)
const API_BASE_URL = "https://localhost:5087/api";

export const FileController = {
  uploadFile: async (file, chatId) => {
    try {
      console.log("FileController: загружаем файл", file.name, "для чата", chatId);

      const formData = new FormData();
      formData.append("file", file);
      formData.append("chatId", chatId);

      const response = await axios.post(`${API_BASE_URL}/file/upload`, formData, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
        withCredentials: true,
      });

      console.log("FileController: ответ сервера", response.data);
      return response.data;
    } catch (error) {
      console.error("FileController: ошибка при загрузке файла:", {
        message: error.message,
        status: error.response?.status,
        statusText: error.response?.statusText,
        data: error.response?.data,
      });
      throw error;
    }
  },

  getFileBlob: async (fileName) => {
    try {
      console.log("FileController: получаем blob для файла", fileName);

      const url = `${API_BASE_URL}/file/download/${encodeURIComponent(fileName)}`;
      // console.log("FileController: запрос к URL:", url); // Убрано из постоянных логов

      const response = await axios.get(url, {
        responseType: "blob",
        withCredentials: true,
        headers: {
          Accept: "*/*", // Это стандартный Accept для axios, можно оставить или убрать
         // Примечание: axios автоматически обрабатывает условные запросы (If-None-Match, If-Modified-Since)
         // если предыдущий ответ содержал ETag/Last-Modified и браузер кешировал ресурс.
        },
        validateStatus: (status) => {
          // Позволяем 4xx статусам пройти, чтобы обрабатывать их явно
          return status < 500;
        },
      });

      // console.log("FileController: ответ получен", { // Убрано из постоянных логов
      //   status: response.status,
      //   statusText: response.statusText,
      //   headers: response.headers,
      //   dataType: typeof response.data,
      //   dataSize: response.data?.size,
      // });

      // Обработка конкретных статусов ответа
      if (response.status === 404) {
        throw new Error("Файл не найден на сервере");
      }
      if (response.status === 401 || response.status === 403) { // 403 Forbidden также означает отсутствие доступа
        throw new Error("Нет доступа к файлу");
      }
      // 200 OK для полного контента, 206 Partial Content
      if (response.status !== 200 && response.status !== 206) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      if (!response.data || response.data.size === 0) {
        throw new Error("Получен пустой файл");
      }

      console.log("FileController: blob получен успешно для", fileName, "размер:", response.data.size);
      return response.data;
    } catch (error) {
      console.error("FileController: ошибка получения blob:", {
        fileName,
        error: error.message,
        status: error.response?.status,
        statusText: error.response?.statusText,
      });
      throw error;
    }
  },

  createMediaObjectUrl: async (fileName) => {
    try {
      console.log("FileController: создаем Object URL для", fileName);

      const blob = await FileController.getFileBlob(fileName);

      if (!blob) {
        throw new Error("Blob не получен");
      }

      // console.log("FileController: blob получен, создаем Object URL", { // Убрано из постоянных логов
      //   blobSize: blob.size,
      //   blobType: blob.type,
      // });

      const objectUrl = URL.createObjectURL(blob);

      console.log("FileController: Object URL создан:", objectUrl);
      return objectUrl;
    } catch (error) {
      console.error("FileController: ошибка создания Object URL:", {
        fileName,
        errorMessage: error.message,
        errorStack: error.stack,
      });
      return null;
    }
  },

  // Этот метод теперь будет более эффективно использовать кеширование браузера благодаря заголовкам сервера
  getFileUrl: (fileName) => {
    const url = `${API_BASE_URL}/file/download/${encodeURIComponent(fileName)}`;
    // console.log("FileController: URL файла:", url); // Убрано из постоянных логов
    return url;
  },

  // Этот метод теперь будет более эффективно использовать кеширование сервера и браузера
  getThumbnailUrl: (fileName, width = 200, height = 200) => {
    const url = `${API_BASE_URL}/file/thumbnail/${encodeURIComponent(fileName)}?width=${width}&height=${height}`;
    // console.log("FileController: URL миниатюры:", url); // Убрано из постоянных логов
    return url;
  },

  checkFileAvailability: async (fileName) => {
    try {
      const url = `${API_BASE_URL}/file/download/${encodeURIComponent(fileName)}`;

      const response = await axios.head(url, {
        withCredentials: true,
        timeout: 5000,
        validateStatus: (status) => status < 500, // Позволяем 4xx статусам пройти, чтобы обрабатывать их явно
      });

      console.log("FileController: проверка файла (HEAD) для", fileName, "статус:", response.status);

      return {
        available: response.status === 200, // HEAD должен вернуть 200 OK, если файл найден
        status: response.status,
        statusText: response.statusText,
      };
    } catch (error) {
      console.error("FileController: ошибка проверки файла (HEAD) для", fileName, error.message);
      return {
        available: false,
        status: error.response?.status || 0, // 0 может означать сетевую ошибку
        statusText: error.message,
      };
    }
  },

  getFileType: (fileName) => {
    const extension = fileName.split(".").pop()?.toLowerCase();
    if (["jpg", "jpeg", "png", "gif", "webp", "bmp"].includes(extension)) {
      return "image";
    }
    if (["mp4", "avi", "mov", "webm", "mkv"].includes(extension)) {
      return "video";
    }
    if (["mp3", "wav", "ogg", "aac", "flac"].includes(extension)) {
      return "audio";
    }
    return "file";
  },

  formatFileSize: (bytes) => {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  },
};