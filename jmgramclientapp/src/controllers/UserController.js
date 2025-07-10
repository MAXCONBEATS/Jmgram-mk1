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
  },

  // Изменение пароля
  changePassword: async (oldPassword, newPassword) => {
    try {
      const response = await axios.post(
        'https://localhost:5087/User/ChangePassword',
        {
          oldPassword: oldPassword,
          newPassword: newPassword
        }
      );
      return response.data;
    } catch (error) {
      console.error('Error changing password:', error);
      throw error;
    }
  },
  updateAvatar: async(fileName) => {
    try {
      console.log("UserController.updateAvatar: обновляем аватарку", fileName)

      const response = await axios.put(
        "https://localhost:5087/User/UpdateAvatar/avatar",
        { avatarFileName: fileName },
        {
          withCredentials: true,
          headers: {
            "Content-Type": "application/json",
          },
        },
      )

      console.log("UserController.updateAvatar: ответ получен", response.data)
      return response.data
    } catch (error) {
      console.error("UserController.updateAvatar: ошибка обновления аватарки:", error)
      throw error
    }
  },

  removeAvatar: async () =>{
    try {
      console.log("UserController.removeAvatar: удаляем аватарку")

      const response = await axios.delete("https://localhost:5087/User/RemoveAvatar/avatar", {
        withCredentials: true,
      })

      console.log("UserController.removeAvatar: ответ получен", response.data)
      return response.data
    } catch (error) {
      console.error("UserController.removeAvatar: ошибка удаления аватарки:", error)
      throw error
    }
  },
  // --- МЕТОД getUserAvatar УДАЛЕН ---
  // async getUserAvatar(userId) {
  //   try {
  //     const response = await axios.get(`https://localhost:5087/User/GetUserAvatar/avatar/${userId}`, {
  //       withCredentials: true,
  //     })
  //     return response.data
  //   } catch (error) {
  //     console.error("Error fetching user avatar:", error)
  //     return null
  //   }
  // },
};