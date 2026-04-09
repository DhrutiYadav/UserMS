import axios from "axios";

const Axios = axios.create({
  baseURL: "https://localhost:7251",
  headers: {
    "Content-Type": "application/json",
    "X-API-Key": "6CBxzdYcEgNDrRhMbDpkBF7e4d4Kib46dwL9ZE5egiL0iL5Y3dzREUBSUYVWUkN",
  },
  withCredentials: true,
});

Axios.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

export default Axios;