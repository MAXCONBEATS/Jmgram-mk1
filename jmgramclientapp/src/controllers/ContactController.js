import axios from 'axios';
axios.defaults.baseURL = 'https://localhost:5087';

// ContactController.js
export async function getContactList() {
  try {
    const response = await axios.get(`https://localhost:5087/Contact/List`, { withCredentials: true });
    return response.data;
  } catch (error) {
    console.error('Ошибка при получении списка контактов:', error);
    throw error;
  }
}

// New function to get contact requests
export async function getContactRequests() {
  try {
    const response = await axios.get(`https://localhost:5087/Contact/Requests`, { withCredentials: true });
    const incomingRequests = response.data.incomingRequests || [];
    // Filter only requests with status 0 (not accepted)
    return incomingRequests.filter(request => request.status === 0);
  } catch (error) {
    console.error('Ошибка при получении запросов в контакты:', error);
    throw error;
  }
}

// New function to accept a contact request
export async function acceptContactRequest(contactRequestId) {
  try {
    const response = await axios.post(`https://localhost:5087/Contact/Accept`, { ContactRequestId: contactRequestId }, { withCredentials: true });
    return response.data;
  } catch (error) {
    console.error('Ошибка при принятии запроса в контакты:', error);
    throw error;
  }
}
 