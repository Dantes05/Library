import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";

interface Book {
  id: number;
  title: string;
  genre: string;
  authorName: string;
  isAvailable: boolean;
}

const BooksList: React.FC = () => {
  const [allBooks, setAllBooks] = useState<Book[]>([]); 
  const [books, setBooks] = useState<Book[]>([]); 
  const [notifications, setNotifications] = useState<string[]>([]);
  const [search, setSearch] = useState("");
  const [genre, setGenre] = useState("");
  const [author, setAuthor] = useState("");
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [isAdmin, setIsAdmin] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    fetchBooks();
    fetchNotifications();
    checkAdmin();
  }, [search, genre, author, page]);


  const fetchBooks = async () => {
    try {
      const response = await axios.get("http://localhost:7143/api/books", {
        params: { search, genre, author }
      });
  
      if (!response.data) {
        throw new Error("Данные о книгах отсутствуют");
      }
  
      const data = response.data.books || response.data;
      const total = data.length;
  
      const booksWithAvailability = await Promise.all(
        data.map(async (book: any) => {
          try {
            const rentalsResponse = await axios.get(`http://localhost:7143/api/books/${book.id}/rentals`);
            const isAvailable = rentalsResponse.data.length === 0;
            return { ...book, isAvailable };
          } catch (error) {
            console.error(`Ошибка при проверке доступности книги (ID ${book.id}):`, error);
            return { ...book, isAvailable: true };
          }
        })
      );
  
      setAllBooks(booksWithAvailability);
      setTotalCount(total);
  
      const startIndex = (page - 1) * 9;
      const endIndex = startIndex + 9;
      const paginatedBooks = booksWithAvailability.slice(startIndex, endIndex);
      setBooks(paginatedBooks);
    } catch (error) {
      console.error("Ошибка при загрузке книг:", error);
      setAllBooks([]);
      setBooks([]);
      setTotalCount(0);
    }
  };
  


  const fetchNotifications = async () => {
    const token = localStorage.getItem("token");
    if (!token) return;

    try {
      const response = await axios.get("http://localhost:7143/api/books/user/rentals/notifications", {
        headers: { Authorization: `Bearer ${token}` }
      });

      setNotifications(response.data.map((n: any) => n.message));
    } catch (error) {
      console.error("Ошибка при загрузке уведомлений:", error);
    }
  };


  const checkAdmin = () => {
    const token = localStorage.getItem("token");

    if (!token) {
      setIsAdmin(false);
      return;
    }

    try {
      const parts = token.split(".");
      const base64Url = parts[1];
      const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
      const decodedPayload = JSON.parse(atob(base64));

      const userRole = decodedPayload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
      setIsAdmin(userRole?.toLowerCase() === "admin");
    } catch (error) {
      console.error("Ошибка декодирования токена:", error);
      setIsAdmin(false);
    }
  };


  const totalPages = Math.ceil(totalCount / 9);

  return (
    <div className="p-6">
      <h1 className="text-2xl font-bold mb-4">📚 Список книг</h1>


      {notifications.length > 0 && (
        <div className="bg-yellow-100 border-l-4 border-yellow-500 text-yellow-700 p-4 mb-4">
          {notifications.map((message, index) => (
            <p key={index}>⚠️ {message}</p>
          ))}
        </div>
      )}


      <div className="flex gap-2 mb-4">
        <button className="bg-blue-500 text-white px-4 py-2" onClick={() => navigate("/profile")}>
          👤 Личный кабинет
        </button>

        {isAdmin && (
          <button className="bg-green-500 text-white px-4 py-2" onClick={() => navigate("/add-book")}>
            ➕ Добавить книгу
          </button>
        )}
      </div>


      <div className="flex gap-2 mb-4">
        <input
          type="text"
          placeholder="Поиск по названию..."
          className="border p-2"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <input
          type="text"
          placeholder="Фильтр по жанру..."
          className="border p-2"
          value={genre}
          onChange={(e) => setGenre(e.target.value)}
        />
        <input
          type="text"
          placeholder="Фильтр по автору..."
          className="border p-2"
          value={author}
          onChange={(e) => setAuthor(e.target.value)}
        />
      </div>


      <div className="grid grid-cols-3 gap-4">
        {books.length === 0 ? (
          <p>Книги не найдены</p>
        ) : (
          books.map((book) => (
            <div key={book.id} className="border p-4 rounded shadow cursor-pointer" onClick={() => navigate(`/books/${book.id}`)}>
              <h2 className="text-xl font-bold">{book.title}</h2>
              <p>{book.genre}</p>
              <p>Автор: {book.authorName}</p>
              <p className={book.isAvailable ? "text-green-500" : "text-red-500"}>
                {book.isAvailable ? "В наличии" : "Нет в наличии"}
              </p>
            </div>
          ))
        )}
      </div>


      <div className="flex justify-center mt-4">
        <button
          className="p-2 border"
          onClick={() => setPage(page - 1)}
          disabled={page === 1}
        >
          ⬅️ Назад
        </button>
        <span className="p-2">
          Страница {page} из {totalPages}
        </span>
        <button
          className="p-2 border"
          onClick={() => setPage(page + 1)}
          disabled={page === totalPages}
        >
          Вперед ➡️
        </button>
      </div>
    </div>
  );
};

export default BooksList;