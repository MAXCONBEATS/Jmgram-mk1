import axios from 'axios';
const getContactList = async () => {
    try {
        const response = await axios.get('/Contact/List');
        return response.data;
    } catch (error) {
        throw error;
    }
};

export default getContactList;