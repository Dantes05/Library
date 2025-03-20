import React, { useState, useEffect } from "react";
import axios from "axios";
import { useParams, useNavigate } from "react-router-dom";

const EditBook: React.FC = () => {
  const { id } = useParams<{ id: string }>(); 
  const navigate = useNavigate();

  const [isbn, setIsbn] = useState("");
  const [title, setTitle] = useState("");
  const [genre, setGenre] = useState("");
  const [description, setDescription] = useState("");
  const [authorId, setAuthorId] = useState<string | null>(null);
  const [imagePath, setImagePath] = useState<string>(""); 

  const [authors, setAuthors] = useState<{ id: number; firstName: string; lastName: string }[]>([]);
  const [showAddAuthor, setShowAddAuthor] = useState(false);
  const [newAuthor, setNewAuthor] = useState({
    firstName: "",
    lastName: "",
    birthDate: "",
    country: "",
  });


  useEffect(() => {
    fetchBook();
    fetchAuthors();
  }, [id]);


  const fetchBook = async () => {
    try {
      const response = await axios.get(`http://localhost:7143/api/books/${id}`);
      const book = response.data;

      setIsbn(book.isbn);
      setTitle(book.title);
      setGenre(book.genre);
      setDescription(book.description);
      setAuthorId(book.authorId.toString());
      setImagePath(book.imagePath); 
    } catch (error) {
      console.error("Ошибка при загрузке книги:", error);
    }
  };


  const fetchAuthors = async () => {
    try {
      const response = await axios.get("http://localhost:7143/api/authors");
      setAuthors(response.data);
    } catch (error) {
      console.error("Ошибка при загрузке авторов:", error);
    }
  };

  const handleAddAuthor = async () => {
    try {
      const response = await axios.post("http://localhost:7143/api/authors", newAuthor);
      const addedAuthor = response.data;

      setAuthors([...authors, addedAuthor]);
      setAuthorId(addedAuthor.id.toString());
      setShowAddAuthor(false);

      console.log("Автор добавлен:", addedAuthor);
    } catch (error) {
      console.error("Ошибка при добавлении автора:", error);
    }
  };

  const handleSaveChanges = async () => {
    try {
      const bookId = id ?? "0"; 
      const numericId = parseInt(bookId); 
  
      if (numericId === 0) {
        alert("ID книги не найден.");
        return;
      }
  
      if (!authorId) {
        alert("Выберите или добавьте автора.");
        return;
      }
  
      if (!isbn || !title || !genre || !description) {
        alert("Все поля должны быть заполнены.");
        return;
      }
  
      const formData = new FormData();
      formData.append("Id", numericId.toString());
      formData.append("ISBN", isbn);
      formData.append("Title", title);
      formData.append("Genre", genre);
      formData.append("Description", description);
      formData.append("AuthorId", authorId);
  
      if (imagePath) {
        formData.append("Image", imagePath);
      }
  
      console.log("📤 Данные для отправки:", formData);
  
      const response = await axios.put(`http://localhost:7143/api/books/${numericId}`, formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });
  
      console.log("✅ Ответ сервера:", response.data);
      navigate("/books");
    } catch (error: any) {
      console.error("🛑 Ошибка при сохранении изменений:", error);
  
      if (error.response) {
        console.error("❗ Данные ошибки:", error.response.data);
        alert("Ошибка при сохранении: " + JSON.stringify(error.response.data.errors, null, 2));
      } else if (error.request) {
        console.error("Запрос отправлен, но нет ответа:", error.request);
      } else {
        console.error("Ошибка при настройке запроса:", error.message);
      }
    }
  };

  return (
    <div className="p-6">
      <h1 className="text-2xl font-bold mb-4">Редактирование книги</h1>

      <label className="block mb-2 font-medium">ISBN:</label>
      <input className="border p-2 mb-4 w-full" type="text" value={isbn} onChange={(e) => setIsbn(e.target.value)} />

      <label className="block mb-2 font-medium">Название книги:</label>
      <input className="border p-2 mb-4 w-full" type="text" value={title} onChange={(e) => setTitle(e.target.value)} />

      <label className="block mb-2 font-medium">Жанр:</label>
      <input className="border p-2 mb-4 w-full" type="text" value={genre} onChange={(e) => setGenre(e.target.value)} />

      <label className="block mb-2 font-medium">Описание:</label>
      <textarea className="border p-2 mb-4 w-full" value={description} onChange={(e) => setDescription(e.target.value)} />

      {/* Выбор автора */}
      <label className="block mb-2 font-medium">Автор:</label>
      <select className="border p-2 mb-4 w-full" value={authorId || ""} onChange={(e) => setAuthorId(e.target.value)}>
        <option value="">Выберите автора</option>
        {authors.map((author) => (
          <option key={author.id} value={author.id}>
            {author.firstName} {author.lastName}
          </option>
        ))}
      </select>

      {/* Добавление нового автора */}
      <button className="bg-gray-500 text-white px-4 py-2 mb-4" onClick={() => setShowAddAuthor(true)}>
        ➕ Добавить нового автора
      </button>

      {showAddAuthor && (
        <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50">
          <div className="bg-white p-6 rounded-lg shadow-lg">
            <h2 className="text-xl font-bold mb-4">Добавление автора</h2>
            
            <label className="block mb-2 font-medium">Имя:</label>
            <input className="border p-2 mb-4 w-full" type="text" value={newAuthor.firstName} onChange={(e) => setNewAuthor({ ...newAuthor, firstName: e.target.value })} />

            <label className="block mb-2 font-medium">Фамилия:</label>
            <input className="border p-2 mb-4 w-full" type="text" value={newAuthor.lastName} onChange={(e) => setNewAuthor({ ...newAuthor, lastName: e.target.value })} />

            <div className="flex justify-between mt-4">
              <button className="bg-green-500 text-white px-4 py-2" onClick={handleAddAuthor}>✅ Добавить</button>
              <button className="bg-red-500 text-white px-4 py-2" onClick={() => setShowAddAuthor(false)}>❌ Закрыть</button>
            </div>
          </div>
        </div>
      )}

      {/* Кнопка сохранения */}
      <button className="bg-green-500 text-white px-4 py-2 mt-4" onClick={handleSaveChanges}>
        💾 Сохранить изменения
      </button>
    </div>
  );
};

export default EditBook;
