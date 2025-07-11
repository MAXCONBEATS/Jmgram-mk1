import { fileService } from "../services/FileService"; // Ваш объединенный FileService (axios)
import { useState, useRef, useEffect, useCallback } from "react";
import "../css/messageUtils.css";

// --- Helper Functions ---

export const formatMessageTime = (timestamp) => {
  if (!timestamp) return "";
  const correctedDate = new Date(timestamp);
  const now = new Date();
  const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
  const messageDate = new Date(correctedDate.getFullYear(), correctedDate.getMonth(), correctedDate.getDate());

  const timeString = correctedDate.toLocaleTimeString("ru-RU", {
    hour: "2-digit",
    minute: "2-digit",
  });

  if (messageDate.getTime() === today.getTime()) {
    return timeString;
  }

  const yesterday = new Date(today.getTime() - 24 * 60 * 60 * 1000);
  if (messageDate.getTime() === yesterday.getTime()) {
    return `вчера ${timeString}`;
  }

  if (correctedDate.getFullYear() === now.getFullYear()) {
    return (
      correctedDate.toLocaleDateString("ru-RU", {
        day: "2-digit",
        month: "2-digit",
      }) + ` ${timeString}`
    );
  }

  return (
    correctedDate.toLocaleDateString("ru-RU", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    }) + ` ${timeString}`
  );
};

// --- MediaComponent ---

const MediaComponent = ({ fileName, fileType, fileSize }) => {
  // Для всех медиа начинаем с "resolved", так как URL теперь напрямую в src/
  // Если браузер не справится, он вызовет onError
  const [status, setStatus] = useState("resolved");
  const [errorMessage, setErrorMessage] = useState("");
  const [objectUrl, setObjectUrl] = useState(null); // Для ручного вызова через кнопку
  const retryTimeoutRef = useRef(null); // Сохраняем для надежности, хотя setTimeout на автоповторы убран

  // Очистка Object URL при размонтировании или смене файла
  useEffect(() => {
    return () => {
      if (retryTimeoutRef.current) {
        clearTimeout(retryTimeoutRef.current);
      }
      if (objectUrl) {
        URL.revokeObjectURL(objectUrl);
        setObjectUrl(null);
      }
    };
  }, [objectUrl]);

  // Сброс состояния при смене файла
  useEffect(() => {
    setStatus("resolved");
    setErrorMessage("");
    if (objectUrl) {
      URL.revokeObjectURL(objectUrl);
      setObjectUrl(null);
    }
  }, [fileName, fileType]);


  // Логика загрузки через Object URL
  const loadViaObjectUrl = useCallback(async () => {
    try {
      if (objectUrl) return; // Уже есть Object URL
      setStatus("loading");
      setErrorMessage("");
      console.log("MediaComponent: загружаем через Object URL", fileName);

      const url = await fileService.createMediaObjectUrl(fileName); // Используем fileService
      if (url) {
        setObjectUrl(url);
        setStatus("resolved");
        console.log("MediaComponent: Object URL создан", url);
      } else {
        throw new Error("Не удалось создать Object URL");
      }
    } catch (error) {
      console.error("MediaComponent: ошибка загрузки Object URL", error);
      setStatus("error");
      setErrorMessage(error.message || "Ошибка загрузки через Object URL");
    }
  }, [fileName, objectUrl]);


  // Обработчик ошибки для медиа элементов (img, video, audio)
  const handleMediaError = useCallback((e) => {
    console.error("MediaComponent: ошибка отображения медиа", fileName, e.type, "-> Устанавливаем статус error.");
    e.target.onerror = null; // Предотвращаем бесконечный цикл ошибок
    setStatus("error");
    // Более детальное сообщение об ошибке, если можно получить
    setErrorMessage(`Браузер не смог отобразить файл. Проверьте консоль для деталей. Status: ${e.target.naturalWidth === 0 ? "Empty Image Data" : "Unknown"}`);

  }, [fileName]);

  // Обработчик успешной загрузки для медиа элементов
  const handleMediaLoad = useCallback((e) => {
    console.log("MediaComponent: медиа отображено успешно", fileName);
    setStatus("resolved"); // Убедимся, что статус "resolved"
  }, [fileName]);

  // Принудительная перезагрузка
  const forceReload = useCallback(() => {
    console.log("MediaComponent: принудительная перезагрузка", fileName);
    if (objectUrl) {
      URL.revokeObjectURL(objectUrl); // Отзываем старый Object URL
    }
    setObjectUrl(null); // Сбрасываем ObjectUrl
    setStatus("resolved"); // Сбрасываем статус, чтобы браузер попробовал загрузить напрямую

    // Можно добавить небольшой хак для принудительной перезагрузки ресурса в браузере,
    // но обычно это не требуется, если URL файла изменился или кеш сброшен.
    // Если URL не меняется, браузер может использовать кеш.
    // Для отладки можно добавить временную метку, но не для продакшена:
    // const currentUrl = fileService.getFileUrl(fileName); // Это не здесь, а там, где используется src
    // Если src элемент не обновится, то браузер может не перезагрузить.
  }, [fileName, objectUrl]);


  const fileUrl = fileService.getFileUrl(fileName);
  const displayUrl = objectUrl || fileUrl; // Используем Object URL, если он был сгенерирован

  // --- Рендеринг в зависимости от статуса ---

  // Когда статус `loading` - это только если мы вручную запустили `loadViaObjectUrl`
  if (status === "loading") {
    return (
      <div className="media-message">
        <div className="media-status">
          <div className="loading-spinner"></div>
          <p>Загрузка файла...</p>
        </div>
        <div className="file-info">
          <span className="file-name">{fileName}</span>
          <span className="file-size">({fileService.formatFileSize(fileSize)})</span>
        </div>
      </div>
    );
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
            {/* Дополнительный текст для диагностики */}
            <p className="debug-hint">
              С сервера получено 200 OK. Возможно, проблема с CSP или форматом файла в браузере.
              Проверьте консоль браузера (F12) на ошибки.
            </p>
          </div>
          <div className="error-actions">
            <button onClick={forceReload} className="btn btn-sm btn-primary">
              Попробовать снова
            </button>
            {/* Кнопка "Загрузить через blob" для ручной попытки */}
            {!objectUrl && ( // Показываем только если еще не пробовали или Object URL не активен
                <button onClick={loadViaObjectUrl} className="btn btn-sm btn-secondary">
                  Загрузить через blob
                </button>
            )}
            <a href={fileUrl} target="_blank" rel="noopener noreferrer" className="btn btn-sm btn-outline">
              Открыть в новой вкладке
            </a>
          </div>
        </div>
        <div className="file-info">
          <span className="file-name">{fileName}</span>
          <span className="file-size">({fileService.formatFileSize(fileSize)})</span>
        </div>
      </div>
    );
  }

  // Статус "resolved" - показываем медиафайл
  // Используем fileUrl напрямую, если objectUrl не активен
  // Если objectUrl был успешно сгенерирован, он будет использоваться
  if (fileType === "image") {
    return (
      <div className="media-message">
        <div className="image-container">
          <img
            src={displayUrl || "/placeholder.svg"} // Если displayUrl пуст, показать заглушку
            alt={fileName}
            className="image-thumbnail"
            onClick={() => window.open(fileUrl, "_blank")}
            onLoad={handleMediaLoad}
            onError={handleMediaError} // Отлавливаем ошибки загрузки браузером
            loading="lazy"
            style={{ maxWidth: "300px", maxHeight: "300px" }}
          />
        </div>
        <div className="file-info">
          <span className="file-name">{fileName}</span>
          <span className="file-size">({fileService.formatFileSize(fileSize)})</span>
        </div>
      </div>
    );
  }

  if (fileType === "video") {
    return (
      <div className="media-message">
        <video
          controls
          className="video-player"
          preload="metadata"
          style={{ maxWidth: "400px", maxHeight: "300px" }}
          onLoadedData={handleMediaLoad}
          onError={handleMediaError}
        >
          <source src={displayUrl} type="video/mp4" />
          Ваш браузер не поддерживает видео
        </video>
        <div className="file-info">
          <span className="file-name">{fileName}</span>
          <span className="file-size">({fileService.formatFileSize(fileSize)})</span>
        </div>
      </div>
    );
  }

  if (fileType === "audio") {
    return (
      <div className="media-message">
        <audio
          controls
          className="audio-player"
          preload="metadata"
          onLoadedData={handleMediaLoad}
          onError={handleMediaError}
        >
          <source src={displayUrl} type="audio/mpeg" />
          Ваш браузер не поддерживает аудио
        </audio>
        <div className="file-info">
          <span className="file-name">{fileName}</span>
          <span className="file-size">({fileService.formatFileSize(fileSize)})</span>
        </div>
      </div>
    );
  }

  // Для остальных файлов
  return (
    <div className="media-message">
      <div className="file-download">
        <a href={fileUrl} download={fileName} className="file-link" target="_blank" rel="noopener noreferrer">
          <i className="bi bi-file-earmark"></i>
          <span>{fileName}</span>
          <span className="file-size">({fileService.formatFileSize(fileSize)})</span>
        </a>
      </div>
    </div>
  );
};


export const renderMediaContent = (message) => {
  if (
    message.text.includes("📎") &&
    message.text.includes("Файл:") &&
    message.text.includes("(") &&
    message.text.includes(")")
  ) {
    const fileStartIndex = message.text.indexOf("Файл:") + 5;
    const sizeStartIndex = message.text.lastIndexOf("(");
    const sizeEndIndex = message.text.lastIndexOf(")");

    if (fileStartIndex > 4 && sizeStartIndex > fileStartIndex && sizeEndIndex > sizeStartIndex) {
      const fileName = message.text.substring(fileStartIndex, sizeStartIndex).trim();
      const fileSizeStr = message.text.substring(sizeStartIndex + 1, sizeEndIndex).trim();

      const fileSize = message.rawFileSizeInBytes || fileSizeStr;

      const fileType = fileService.getFileType(fileName);

      console.log("renderMediaContent: рендерим медиа", { fileName, fileType, fileSize });

      return <MediaComponent key={fileName} fileName={fileName} fileType={fileType} fileSize={fileSize} />;
    }
  }

  return <span>{message.text}</span>;
};