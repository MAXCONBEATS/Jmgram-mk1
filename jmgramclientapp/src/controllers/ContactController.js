import axios from 'axios';
axios.defaults.baseURL = 'https://localhost:5087';

export async function getContactList() {
  try {
    const response = await axios.get(`https://localhost:5087/Contact/List`, { withCredentials: true });
    return response.data;
  } catch (error) {
    console.error('Ошибка при получении списка контактов:', error);
    throw error;
  }
}

export async function getContactRequests() {
  try {
    const response = await axios.get(`https://localhost:5087/Contact/Requests`, { withCredentials: true });
    const incomingRequests = response.data.incomingRequests || [];
    return incomingRequests.filter(request => request.status === 0);
  } catch (error) {
    console.error('Ошибка при получении запросов в контакты:', error);
    throw error;
  }
}

export async function acceptContactRequest(contactRequestId) {
  try {
    const response = await axios.post(`https://localhost:5087/Contact/Accept`, { ContactRequestId: contactRequestId }, { withCredentials: true });
    return response.data;
  } catch (error) {
    console.error('Ошибка при принятии запроса в контакты:', error);
    throw error;
  }
}

export async function deleteContact(contactUserId) {
  try {
    const response = await axios.post('/Contact/Delete', 
      { contactUserId: contactUserId }, 
      { withCredentials: true }
    );
    return response.data;
  } catch (error) {
    console.error('Ошибка при удалении контакта:', error);
    throw error;
  }
}
