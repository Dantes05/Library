import axios from "axios";

const API_URL = "http://localhost:7143/api";

const api = axios.create({
  baseURL: API_URL,
  headers: { "Content-Type": "application/json" },
});

export const getBooks = async (search = "", genre = "", author = "", page = 1, pageSize = 10) => {
  try {
    const response = await axios.get(`${API_URL}/books`, {
      params: { search, genre, author, page, pageSize },
    });
    return response.data;
  } catch (error) {
    console.error("Ошибка при загрузке книг:", error);
    return [];
  }
};


export const login = async (email: string, password: string) => {
  const response = await api.post("/auth/authenticate", { email, password });
  return response.data;
};

export const register = async (firstName: string, lastName: string, email: string, password: string, confirmPassword: string) => {
  const response = await api.post("/auth/register", { firstName, lastName, email, password, confirmPassword });
  return response.data;
};

