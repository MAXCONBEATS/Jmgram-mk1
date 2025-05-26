import React, { useState, useEffect } from 'react';
import { UserController } from '../controllers/UserController';
import '../css/UserProfile.css';

const UserProfile = ({ userId, onClose }) => {
  const [profile, setProfile] = useState(null);
  const [editMode, setEditMode] = useState(false);
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    avatarPath: '',
    bio: ''
  });

  // Загрузка профиля
  useEffect(() => {
    const loadProfile = async () => {
      try {
        const data = await UserController.getProfile(userId);
        setProfile(data);
        setFormData({
          firstName: data.firstName || '',
          lastName: data.lastName || '',
          avatarPath: data.avatarPath || '',
          bio: data.bio || ''
        });
      } catch (error) {
        console.error('Failed to load profile:', error);
      }
    };
    loadProfile();
  }, [userId]);

  // Обработка изменений формы
  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  // Сохранение изменений
  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      await UserController.updateProfile(formData);
      setProfile(prev => ({ ...prev, ...formData }));
      setEditMode(false);
    } catch (error) {
      console.error('Update failed:', error);
    }
  };

  if (!profile) return <div className="user-profile-loading">Loading...</div>;

  const isOwnProfile = userId === localStorage.getItem('UserId');

  return (
    <div className="user-profile-overlay">
      <div className="user-profile-container">
        <button className="close-btn" onClick={onClose}>×</button>
        
        {profile.avatarPath && (
          <div className="avatar-container">
            <img src={profile.avatarPath} alt="Profile" className="profile-avatar" />
          </div>
        )}

        {editMode ? (
          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label>Имя:</label>
              <input
                type="text"
                name="firstName"
                value={formData.firstName}
                onChange={handleChange}
              />
            </div>
            {/* Аналогично для других полей */}
            <button type="submit">Сохранить</button>
          </form>
        ) : (
          <div className="profile-info">
            <h2>{profile.firstName} {profile.lastName}</h2>
            <p>{profile.bio}</p>
            <p>Был в сети: {new Date(profile.lastSeen).toLocaleString()}</p>
            {isOwnProfile && <button onClick={() => setEditMode(true)}>Редактировать</button>}
          </div>
        )}
      </div>
    </div>
  );
};

export default UserProfile;