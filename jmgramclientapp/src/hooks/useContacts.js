import { useState, useEffect } from 'react';
import { getContactList, getContactRequests, acceptContactRequest } from '../controllers/ContactController';

export function useContacts(userId, refreshTrigger) {
  const [contacts, setContacts] = useState([]);
  const [contactRequests, setContactRequests] = useState([]);

  useEffect(() => {
    const fetchContacts = async () => {
      try {
        const contactList = await getContactList();
        setContacts(contactList);
      } catch (error) {
        console.error('Ошибка при получении списка контактов:', error);
      }
    };
    fetchContacts();
  }, [refreshTrigger]);

  useEffect(() => {
    const fetchContactRequests = async () => {
      try {
        const requestsData = await getContactRequests();
        const mappedRequests = requestsData.map((request) => {
          let name = 'Неизвестный пользователь';
          if (request.senderUserId === userId) {
            name = 'Неизвестный номер';
          } else {
            name = 'Неизвестный номер';
          }
          return {
            ...request,
            senderName: name,
          };
        });
        setContactRequests(mappedRequests);
      } catch (error) {
        console.error('Ошибка при получении запросов в контакты:', error);
      }
    };
    fetchContactRequests();
  }, [refreshTrigger, userId]);

  const handleAcceptContactRequest = async (contactRequestId) => {
    try {
      const result = await acceptContactRequest(contactRequestId);
      if (typeof result === 'string' || (result && result.isSuccess)) {
        setContactRequests((prev) => prev.filter((req) => req.id !== contactRequestId));
      } else {
        alert(`Ошибка при принятии запроса: ${result.errorMessage || 'Неизвестная ошибка'}`);
      }
    } catch (error) {
      console.error('Ошибка при принятии запроса в контакты:', error);
    }
  };

  return {
    contacts,
    contactRequests,
    handleAcceptContactRequest,
  };
}
