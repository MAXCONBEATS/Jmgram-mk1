import { useState, useEffect, useRef } from "react"
import { UserController } from "../controllers/UserController"
import { deleteContact } from "../controllers/ContactController"
import { fileService } from "../services/FileService"
import "../css/UserProfile.css"

const UserProfile = ({ userId, onClose, onContactDeleted, updateContactName }) => {
  const [profile, setProfile] = useState(null)
  const [editMode, setEditMode] = useState(false)
  const [contactNameEditMode, setContactNameEditMode] = useState(false)
  const [newContactName, setNewContactName] = useState("")
  const [uploadingAvatar, setUploadingAvatar] = useState(false)
  const [formData, setFormData] = useState({
    firstName: "",
    lastName: "",
    bio: "",
  })

  const avatarInputRef = useRef(null)

  useEffect(() => {
    const loadProfile = async () => {
      try {
        const data = await UserController.getProfile(userId)
        console.log("Loaded profile:", data)
        // Маппинг: используем avatarPath из ответа бэкенда для поля avatarFileName в состоянии
        setProfile({
          ...data,
          avatarFileName: data.avatarPath || null, // data.avatarPath приходит из GetProfile
        })
        setFormData({
          firstName: data.firstName || "",
          lastName: data.lastName || "",
          bio: data.bio || "",
        })
        setNewContactName(`${data.firstName || ""} ${data.lastName || ""}`.trim())
      } catch (error) {
        console.error("Failed to load profile:", error)
      }
    }

    loadProfile()
  }, [userId])

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    })
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    try {
      await UserController.updateProfile(formData)
      setProfile((prev) => ({ ...prev, ...formData }))
      setEditMode(false)
    } catch (error) {
      console.error("Update failed:", error)
    }
  }

  // Обработка загрузки аватарки
  const handleAvatarUpload = async (event) => {
    const file = event.target.files[0]
    if (!file) return

    // Проверяем тип файла
    if (!file.type.startsWith("image/")) {
      alert("Пожалуйста, выберите изображение")
      return
    }

    // Проверяем размер файла (максимум 5MB)
    if (file.size > 5 * 1024 * 1024) {
      alert("Размер файла не должен превышать 5MB")
      return
    }

    setUploadingAvatar(true)

    try {
      console.log("Загружаем аватарку:", file.name)

      // Загружаем файл через наш файловый сервис
      const uploadResponse = await fileService.uploadFile(file, "avatar-" + userId)

      if (uploadResponse?.success) {
        console.log("Аватарка загружена:", uploadResponse.fileName)

        // Обновляем профиль с новой аватаркой
        const updateResponse = await UserController.updateAvatar(uploadResponse.fileName)

        if (updateResponse.success) {
          // Обновляем состояние профиля. uploadResponse.fileName - это то, что мы передали
          // в updateAvatar и, предположительно, это же имя файла теперь хранится в бэкенде.
          setProfile((prev) => ({
            ...prev,
            avatarFileName: uploadResponse.fileName,
          }))

          alert("Аватарка успешно обновлена!")
        } else {
          throw new Error(updateResponse.message || "Ошибка обновления аватарки")
        }
      } else {
        throw new Error(uploadResponse?.message || "Ошибка загрузки файла")
      }
    } catch (error) {
      console.error("Ошибка загрузки аватарки:", error)
      alert("Не удалось загрузить аватарку: " + error.message)
    } finally {
      setUploadingAvatar(false)
      if (avatarInputRef.current) {
        avatarInputRef.current.value = ""
      }
    }
  }

  // Удаление аватарки
  const handleRemoveAvatar = async () => {
    if (!window.confirm("Вы уверены, что хотите удалить аватарку?")) return

    try {
      const response = await UserController.removeAvatar()
      if (response.success) {
        setProfile((prev) => ({
          ...prev,
          avatarFileName: null, // Устанавливаем в null, так как аватарка удалена
        }))
        alert("Аватарка удалена")
      } else {
        throw new Error(response.message || "Ошибка удаления аватарки")
      }
    } catch (error) {
      console.error("Ошибка удаления аватарки:", error)
      alert("Не удалось удалить аватарку: " + error.message)
    }
  }

  const handleEditContactName = () => {
    setContactNameEditMode(true)
  }

  const handleContactNameSubmit = async (e) => {
    e.preventDefault()
    try {
      await updateContactName(userId, newContactName)
      alert("Имя контакта успешно обновлено!")
      setContactNameEditMode(false)
      setProfile((prev) => ({
        ...prev,
        displayName: newContactName,
      }))
    } catch (error) {
      console.error("Ошибка при обновлении имени контакта:", error)
      alert("Не удалось обновить имя контакта.")
    }
  }

  const handleCancelContactNameEdit = () => {
    setContactNameEditMode(false)
    setNewContactName(`${profile.firstName || ""} ${profile.lastName || ""}`.trim())
  }

  const handleDeleteContact = async () => {
    if (window.confirm("Вы уверены, что хотите удалить этот контакт?")) {
      try {
        await deleteContact(userId)
        alert("Контакт успешно удален.")
        if (onContactDeleted) {
          await onContactDeleted()
        }
        onClose()
      } catch (error) {
        console.error("Ошибка при удалении контакта:", error)
        alert("Не удалось удалить контакт.")
      }
    }
  }

  if (!profile) return <div className="user-profile-loading">Loading...</div>

  const isOwnProfile = userId === localStorage.getItem("UserId")

  // Получаем URL аватарки
  const getAvatarUrl = () => {
    // Используем profile.avatarFileName, который теперь гарантированно содержит имя файла
    if (profile.avatarFileName) {
      return fileService.getFileUrl(profile.avatarFileName)
    }
    return null
  }

  // Получаем инициалы для аватарки по умолчанию
  const getInitials = () => {
    const firstName = profile.firstName || ""
    const lastName = profile.lastName || ""
    return (firstName.charAt(0) + lastName.charAt(0)).toUpperCase() || "?"
  }

  return (
    <div className="user-profile-overlay">
      <div className="user-profile-container">
        <button className="close-btn" onClick={onClose}>
          ×
        </button>

        {/* Секция аватарки */}
        <div className="avatar-section">
          <div className="avatar-container">
            {/* Отображаем изображение, если avatarFileName существует. Теперь аватарка загружается в первый раз из getProfile. */}
            {profile.avatarFileName ? (
              <img
                src={getAvatarUrl() || "/placeholder.svg"}
                alt="Аватарка"
                className="avatar-image"
                onError={(e) => {
                  console.error("Ошибка загрузки аватарки:", e.currentTarget.src)
                  e.currentTarget.style.display = "none" // Скрываем сломанное изображение
                  const placeholder = e.currentTarget.nextElementSibling;
                  // Проверяем, что следующий элемент - это наш placeholder
                  if (placeholder && placeholder.classList.contains("avatar-placeholder")) {
                      placeholder.style.display = "flex"; // Показываем placeholder
                  }
                }}
              />
            ) : null}
            <div className="avatar-placeholder" style={{ display: profile.avatarFileName ? "none" : "flex" }}>
              {getInitials()}
            </div>
            {uploadingAvatar && <div className="avatar-loading">Загрузка...</div>}
          </div>

          {isOwnProfile && (
            <div className="avatar-actions">
              <input
                type="file"
                ref={avatarInputRef}
                onChange={handleAvatarUpload}
                accept="image/*"
                style={{ display: "none" }}
                disabled={uploadingAvatar}
              />
              <button
                onClick={() => avatarInputRef.current?.click()}
                className="upload-avatar-btn"
                disabled={uploadingAvatar}
              >
                {uploadingAvatar ? "Загрузка..." : profile.avatarFileName ? "Изменить фото" : "Добавить фото"}
              </button>
              {profile.avatarFileName && (
                <button onClick={handleRemoveAvatar} className="remove-avatar-btn" disabled={uploadingAvatar}>
                  Удалить фото
                </button>
              )}
            </div>
          )}
        </div>

        {contactNameEditMode && !isOwnProfile && (
          <div className="contact-name-edit-form">
            <h3>Изменить имя контакта</h3>
            <form onSubmit={handleContactNameSubmit}>
              <div className="form-group">
                <label>Новое имя контакта:</label>
                <input
                  type="text"
                  value={newContactName}
                  onChange={(e) => setNewContactName(e.target.value)}
                  placeholder="Введите новое имя контакта"
                  required
                />
              </div>
              <div className="form-actions">
                <button type="submit" className="save-btn">
                  Сохранить
                </button>
                <button type="button" className="cancel-btn" onClick={handleCancelContactNameEdit}>
                  Отмена
                </button>
              </div>
            </form>
          </div>
        )}

        {editMode ? (
          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label>Имя:</label>
              <input type="text" name="firstName" value={formData.firstName} onChange={handleChange} />
            </div>
            <div className="form-group">
              <label>Фамилия:</label>
              <input type="text" name="lastName" value={formData.lastName} onChange={handleChange} />
            </div>
            <div className="form-group">
              <label>О себе:</label>
              <textarea name="bio" value={formData.bio} onChange={handleChange} rows="4" />
            </div>
            <div className="form-actions">
              <button type="submit" className="save-btn">
                Сохранить
              </button>
              <button type="button" className="cancel-btn" onClick={() => setEditMode(false)}>
                Отмена
              </button>
            </div>
          </form>
        ) : (
          <div className="profile-info">
            <h2>
              {profile.displayName || `${profile.firstName} ${profile.lastName}`}
              {!isOwnProfile && profile.displayName && (
                <small>
                  (Настоящее имя: {profile.firstName} {profile.lastName})
                </small>
              )}
            </h2>
            {profile.bio && <p className="bio-text">{profile.bio}</p>}
            <p className="last-seen">Был в сети: {new Date(profile.lastSeen).toLocaleString()}</p>

            {isOwnProfile ? (
              <button onClick={() => setEditMode(true)} className="edit-btn">
                <i className="bi bi-pencil-square"></i> Редактировать профиль
              </button>
            ) : (
              <div className="contact-actions">
                {!contactNameEditMode && (
                  <button onClick={handleEditContactName} className="edit-contact-name-btn">
                    Изменить имя контакта
                  </button>
                )}
                <button onClick={handleDeleteContact} className="delete-btn">
                  Удалить контакт
                </button>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  )
}

export default UserProfile