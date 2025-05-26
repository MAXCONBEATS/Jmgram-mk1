import axios from 'axios';
axios.defaults.withCredentials = true;

export const UserController = {
  // Получение профиля
  getProfile: async (userId = null) => {
    try {
      const params = userId ? { userId } : {};
      const response = await axios.get('https://localhost:5087/User/GetProfile', { params });
      return response.data;
    } catch (error) {
      console.error('Error fetching profile:', error);
      throw error;
    }
  },

  // Обновление профиля
  updateProfile: async (profileData) => {
    try {
      const response = await axios.patch(
        'https://localhost:5087/User/UpdateProfile',
        { profile: profileData }
      );
      return response.data;
    } catch (error) {
      console.error('Error updating profile:', error);
      throw error;
    }
  }
};