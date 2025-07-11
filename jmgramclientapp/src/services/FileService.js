import axios from "axios";

// Настройка axios (остаются без изменений)
axios.defaults.withCredentials = true;
axios.defaults.timeout = 30000; // Таймаут 30 секунд

// Добавляем интерцепторы для логирования (можно сделать условными для продакшена)
axios.interceptors.request.use(
  (config) => {
    // console.log("API: отправляем запрос", config.method?.toUpperCase(), config.url);
    return config;
  },
  (error) => {
    console.error("API: ошибка запроса", error);
    return Promise.reject(error);
  }
);

axios.interceptors.response.use(
  (response) => {
    // console.log("API: получен ответ", response.status, response.config.url);
    return response;
  },
  (error) => {
    if (error.message === "Network Error") {
      console.error("API: CORS ошибка или сетевая проблема", {
        url: error.config?.url,
        message: "Возможно проблема с CORS настройками на сервере",
      });
    } else {
      console.error("API: ошибка ответа", {
        status: error.response?.status,
        statusText: error.response?.statusText,
        url: error.config?.url,
        message: error.message,
      });
    }
    return Promise.reject(error);
  }
);

const API_BASE_URL = "https://localhost:5087/api"; // Убедитесь, что это ваш реальный URL

// Объединенный и исправленный сервис
class FileService {
  constructor() {
    this.baseUrl = `${API_BASE_URL}/file`;
    // Убираем Map cache, так как axios будет полагаться на кэш браузера и HTTP заголовки
  }

  // Загрузка файла на сервер
  async uploadFile(file, chatId) {
    try {
      console.log("FileService: загружаем файл", file.name, "для чата", chatId);

      const formData = new FormData();
      formData.append("file", file);
      formData.append("chatId", chatId);

      const response = await axios.post(`${this.baseUrl}/upload`, formData, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
        withCredentials: true,
      });

      console.log("FileService: файл загружен", response.data);
      return response.data;
    } catch (error) {
      console.error("FileService: ошибка при загрузке файла:", {
        message: error.message,
        status: error.response?.status,
        statusText: error.response?.statusText,
        data: error.response?.data,
      });
      throw error;
    }
  }

  // Получение URL файла для прямого использования в src (img, video, audio)
  getFileUrl(fileName) {
    return `${this.baseUrl}/download/${encodeURIComponent(fileName)}`;
  }

  // Получение URL миниатюры
  getThumbnailUrl(fileName, width = 200, height = 200) {
    return `${this.baseUrl}/thumbnail/${encodeURIComponent(fileName)}?width=${width}&height=${height}`;
  }

  // Проверка доступности файла (использует HEAD запрос через axios)
  async checkFileAvailability(fileName) {
    try {
      const url = this.getFileUrl(fileName);
      console.log("FileService: проверка файла (HEAD) через axios", fileName); // Добавлено логирование

      const response = await axios.head(url, {
        withCredentials: true,
        timeout: 5000,
        validateStatus: (status) => status < 500, // Позволяем 4xx статусам пройти
      });

      return {
        available: response.status === 200,
        status: response.status,
        statusText: response.statusText,
        contentType: response.headers["content-type"], // axios предоставляет заголовки в нижнем регистре
        contentLength: response.headers["content-length"],
      };
    } catch (error) {
      console.error("FileService: ошибка проверки файла (HEAD) через axios", fileName, error.message); // Добавлено логирование
      return {
        available: false,
        status: error.response?.status || 0,
        statusText: error.message,
        error: error.name,
      };
    }
  }

  // Получение файла как Blob (использует GET запрос через axios)
  async getFileBlob(fileName) {
    try {
      const url = this.getFileUrl(fileName);
      console.log("FileService: получение blob через axios GET", fileName); // Добавлено логирование

      const response = await axios.get(url, {
        responseType: "blob",
        withCredentials: true,
        validateStatus: (status) => status < 500, // Позволяем 4xx статусам пройти
      });

      if (response.status === 404) {
        throw new Error("Файл не найден на сервере");
      }
      if (response.status === 401 || response.status === 403) {
        throw new Error("Нет доступа к файлу");
      }
      if (response.status !== 200 && response.status !== 206) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      if (!response.data || response.data.size === 0) {
        throw new Error("Получен пустой файл");
      }
      return response.data;
    } catch (error) {
      console.error("FileService: ошибка получения blob через axios:", fileName, error); // Добавлено логирование
      throw error;
    }
  }

  // Создание Object URL
  async createMediaObjectUrl(fileName) { // Переименовано для ясности (было createObjectUrl)
    try {
      const blob = await this.getFileBlob(fileName);
      if (!blob) {
        throw new Error("Blob не получен");
      }
      const objectUrl = URL.createObjectURL(blob);
      console.log("FileService: Object URL создан", objectUrl); // Добавлено логирование
      return objectUrl;
    } catch (error) {
      console.error("FileService: ошибка создания Object URL", fileName, error); // Добавлено логирование
      return null;
    }
  }

  // Определение типа файла
  getFileType(fileName) {
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
  }

  // Форматирование размера файла
  formatFileSize(bytes) {
    if (isNaN(bytes) || bytes < 0) return "0 B"; // Добавлена проверка на невалидные байты
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }

  // Методы очистки кеша (для сброса клиентского кеша браузера - полезно для отладки)
  // Реально эти функции в продакшен коде используются редко для принудительной инвалидации кеша
  clearFileCache(fileName) {
    // В axios/браузере нет прямого API для очистки кеша для конкретного URL
    // Самый простой способ - сделать запрос с Cache-Control: no-cache
    // но в клиентском коде это проблематично.
    // Для отладки можно попросить пользователя очистить кеш браузера,
    // или добавить timestamp к URL как в старой версии, но это обходит кеш.
    console.warn("FileService: `clearFileCache` неэффективен для HTTP-кеша браузера. Для полного сброса нужно очистить кеш в браузере.");
  }

  // Предзагрузка файла (сохранил логику, но с поправками)
  async preloadFile(fileName) {
    console.log("FileService: предзагрузка файла", fileName);
    try {
      // Здесь не кэшируем, а просто запрашиваем заголовки
      const result = await this.checkFileAvailability(fileName);
      if (result.available) {
        // Если файл доступен, возможно, имеет смысл запросить его,
        // чтобы он попал в кеш браузера.
        // Запускаем это "в фоновом режиме" и не ждем.
        axios.get(this.getFileUrl(fileName), {
          responseType: "arraybuffer", // Чтобы не пытался создать blob, если не нужно
          withCredentials: true,
          // `cache` опция отсутствует, полагается на дефолтное поведение браузера
        }).catch((err) => {
          console.log("FileService: ошибка предзагрузки GET", err.message);
        });
      }
      return result;
    } catch (error) {
      console.error("FileService: ошибка предзагрузки", error);
      return { available: false, error: error.message };
    }
  }

  // Глобальная очистка кеша (по сути, бесполезна для HTTP-кеша)
  clearCache() {
    console.warn("FileService: `clearCache` бесполезна для HTTP-кеша браузера. Для полного сброса нужно очистить кеш в браузере.");
  }
}

// Экспортируем синглтон
export const fileService = new FileService();
