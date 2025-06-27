import React, { useState } from 'react';
import { UserController } from '../controllers/UserController';
import '../css/SettingsModal.css';

const SettingsModal = ({ onClose, onLogout }) => {
  const [showChangePassword, setShowChangePassword] = useState(false);
  const [passwordData, setPasswordData] = useState({
    oldPassword: '',
    newPassword: '',
    confirmPassword: ''
  });
  const [isLoading, setIsLoading] = useState(false);

  const handlePasswordChange = (e) => {
    setPasswordData({
      ...passwordData,
      [e.target.name]: e.target.value
    });
  };

  const handleChangePasswordSubmit = async (e) => {
    e.preventDefault();
    
    if (passwordData.newPassword !== passwordData.confirmPassword) {
      alert('Новый пароль и подтверждение не совпадают');
      return;
    }

    if (passwordData.newPassword.length < 6) {
      alert('Новый пароль должен содержать минимум 6 символов');
      return;
    }

    setIsLoading(true);
    try {
      const result = await UserController.changePassword(
        passwordData.oldPassword, 
        passwordData.newPassword
      );
      
      if (result.isSuccess) {
        alert('Пароль успешно изменен');
        setShowChangePassword(false);
        setPasswordData({
          oldPassword: '',
          newPassword: '',
          confirmPassword: ''
        });
      } else {
        alert(`Ошибка при изменении пароля: ${result.errorMessage || 'Неизвестная ошибка'}`);
      }
    } catch (error) {
      console.error('Ошибка при изменении пароля:', error);
      alert('Произошла ошибка при изменении пароля');
    } finally {
      setIsLoading(false);
    }
  };

  const handleCancelPasswordChange = () => {
    setShowChangePassword(false);
    setPasswordData({
      oldPassword: '',
      newPassword: '',
      confirmPassword: ''
    });
  };

  const handleLogout = () => {
    if (window.confirm('Вы уверены, что хотите выйти?')) {
      onLogout();
    }
  };

  return (
    <div className="settings-overlay">
      <div className="settings-container">
        <button className="close-btn" onClick={onClose}>×</button>
        
        <h2>Настройки</h2>

        {showChangePassword ? (
          <div className="change-password-form">
            <h3>Изменить пароль</h3>
            <form onSubmit={handleChangePasswordSubmit}>
              <div className="form-group">
                <label>Текущий пароль:</label>
                <input
                  type="password"
                  name="oldPassword"
                  value={passwordData.oldPassword}
                  onChange={handlePasswordChange}
                  required
                  disabled={isLoading}
                />
              </div>

              <div className="form-group">
                <label>Новый пароль:</label>
                <input
                  type="password"
                  name="newPassword"
                  value={passwordData.newPassword}
                  onChange={handlePasswordChange}
                  required
                  minLength="6"
                  disabled={isLoading}
                />
              </div>

              <div className="form-group">
                <label>Подтвердите новый пароль:</label>
                <input
                  type="password"
                  name="confirmPassword"
                  value={passwordData.confirmPassword}
                  onChange={handlePasswordChange}
                  required
                  minLength="6"
                  disabled={isLoading}
                />
              </div>

              <div className="form-actions">
                <button 
                  type="submit" 
                  className="save-btn"
                  disabled={isLoading}
                >
                  {isLoading ? 'Сохранение...' : 'Сохранить'}
                </button>
                <button 
                  type="button" 
                  className="cancel-btn"
                  onClick={handleCancelPasswordChange}
                  disabled={isLoading}
                >
                  Отмена
                </button>
              </div>
            </form>
          </div>
        ) : (
          <div className="settings-menu">
            <button 
              className="settings-menu-item"
              onClick={() => setShowChangePassword(true)}
            >
              <i className="bi bi-key"></i>
              Изменить пароль
            </button>
            
            <button 
              className="settings-menu-item logout-item"
              onClick={handleLogout}
            >
              <i className="bi bi-box-arrow-right"></i>
              Выйти
            </button>
          </div>
        )}
      </div>
    </div>
  );
};

export default SettingsModal;